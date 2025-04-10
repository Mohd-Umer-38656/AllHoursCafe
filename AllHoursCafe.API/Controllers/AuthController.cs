using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AllHoursCafe.API.Data;
using AllHoursCafe.API.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using AllHoursCafe.API.Services;

namespace AllHoursCafe.API.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly IEmailService _emailService;

        public AuthController(ApplicationDbContext context, IConfiguration configuration, ILogger<AuthController> logger, IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _emailService = emailService;
        }

        // GET: /Auth/Login
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // GET: /Auth/Signup
        public IActionResult Signup()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            _logger.LogInformation("Login attempt - User found: {UserFound}, Email: {Email}, Role: {Role}",
                user != null, model.Email, user?.Role);

            if (user == null || !VerifyPassword(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View(model);
            }

            // If the password is verified but not in BCrypt format, update it to BCrypt
            if (!IsBCryptHash(user.PasswordHash))
            {
                _logger.LogInformation("Upgrading password hash to BCrypt for user {Email}", user.Email);
                user.PasswordHash = HashPassword(model.Password);
                await _context.SaveChangesAsync();
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError("", "Account is inactive");
                return View(model);
            }

            // Create claims for the user
            _logger.LogInformation("Creating claims for user - ID: {UserId}, Email: {Email}, Role: {Role}",
                user.Id, user.Email, user.Role);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role)
            };

            _logger.LogInformation("Claims created - Role claim value: {RoleClaim}",
                claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value);

            // Create identity and principal
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // Sign in the user
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddDays(7)
            });

            // Also generate JWT token for API access
            var token = GenerateJwtToken(user);
            HttpContext.Response.Cookies.Append("AuthToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            _logger.LogInformation("User {Email} logged in successfully", user.Email);

            // Check if there's a return URL in the parameter
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Check if there's a return URL in TempData
            if (TempData["ReturnUrl"] != null)
            {
                string tempDataReturnUrl = TempData["ReturnUrl"].ToString();
                return Redirect(tempDataReturnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        // POST: /Auth/Signup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Signup(SignupViewModel model)
        {
            _logger.LogInformation("Signup attempt for email: {Email}", model.Email);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is invalid for signup attempt");
                return View(model);
            }

            try
            {
                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                {
                    _logger.LogWarning("Email {Email} is already registered", model.Email);
                    ModelState.AddModelError("Email", "Email is already registered");
                    return View(model);
                }

                var user = new User
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    PasswordHash = HashPassword(model.Password),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Role = model.Role // Default is "User" as set in the model
                };

                _logger.LogInformation("Adding new user with email: {Email}", model.Email);
                _context.Users.Add(user);

                var result = await _context.SaveChangesAsync();
                _logger.LogInformation("SaveChangesAsync result: {Result}", result);

                TempData["SuccessMessage"] = "Registration successful! Please login.";
                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration for email: {Email}", model.Email);
                ModelState.AddModelError("", "An error occurred during registration. Please try again.");
                return View(model);
            }
        }

        // POST: /Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Sign out the user from cookie authentication
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Remove the JWT token cookie
            HttpContext.Response.Cookies.Delete("AuthToken");

            // Clear all session data
            HttpContext.Session.Clear();

            // Clear cart data from session
            HttpContext.Session.Remove("Cart");

            // Add cache control headers to prevent caching
            HttpContext.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            HttpContext.Response.Headers["Pragma"] = "no-cache";
            HttpContext.Response.Headers["Expires"] = "0";

            _logger.LogInformation("User logged out and session cleared");

            // Check if it's an AJAX request
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = "Logged out successfully" });
            }

            return RedirectToAction("Index", "Home");
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string hash)
        {
            // Check if the hash is in BCrypt format
            if (IsBCryptHash(hash))
            {
                try
                {
                    return BCrypt.Net.BCrypt.Verify(password, hash);
                }
                catch (BCrypt.Net.SaltParseException)
                {
                    // If BCrypt verification fails, fall back to SHA256
                    return HashPasswordSHA256(password) == hash;
                }
            }
            else
            {
                // For backward compatibility with SHA256 hashes
                return HashPasswordSHA256(password) == hash;
            }
        }

        private bool IsBCryptHash(string hash)
        {
            return hash != null && hash.StartsWith("$2");
        }

        private string HashPasswordSHA256(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"] ?? "your-secret-key");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        // GET: /Auth/ForgotPassword
        public IActionResult ForgotPassword()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: /Auth/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                _logger.LogWarning("Password reset requested for non-existent email: {Email}", model.Email);
                TempData["SuccessMessage"] = "If your email is registered, you will receive password reset instructions shortly.";
                return RedirectToAction(nameof(Login));
            }

            // Generate password reset token
            string token = GeneratePasswordResetToken();

            // Set token expiry time
            int tokenExpiryMinutes = _configuration.GetValue<int>("PasswordReset:TokenExpiryMinutes", 60);
            user.PasswordResetToken = token;
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddMinutes(tokenExpiryMinutes);

            await _context.SaveChangesAsync();

            // Send password reset email
            string resetUrl = $"{_configuration["PasswordReset:ResetUrl"]}?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(token)}";
            string emailBody = $@"<html>
<body>
<h2>Reset Your Password</h2>
<p>Dear {user.FullName},</p>
<p>We received a request to reset your password. Click the link below to reset your password:</p>
<p><a href='{resetUrl}'>Reset Password</a></p>
<p>If you did not request a password reset, please ignore this email.</p>
<p>This link will expire in {tokenExpiryMinutes} minutes.</p>
<p>Regards,<br>All Hours Cafe Team</p>
</body>
</html>";

            await _emailService.SendEmailAsync(user.Email, "Reset Your Password", emailBody);

            TempData["SuccessMessage"] = "If your email is registered, you will receive password reset instructions shortly.";
            return RedirectToAction(nameof(Login));
        }

        // GET: /Auth/ResetPassword
        public IActionResult ResetPassword(string email, string token)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new ResetPasswordViewModel
            {
                Email = email,
                Token = token
            };

            return View(model);
        }

        // POST: /Auth/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null || user.PasswordResetToken != model.Token || user.PasswordResetTokenExpiry < DateTime.UtcNow)
            {
                ModelState.AddModelError("", "Invalid or expired password reset token.");
                return View(model);
            }

            // Update password
            user.PasswordHash = HashPassword(model.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your password has been reset successfully. Please login with your new password.";
            return RedirectToAction(nameof(Login));
        }

        private string GeneratePasswordResetToken()
        {
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }
    }
}