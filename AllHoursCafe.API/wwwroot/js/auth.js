// API endpoints
const API_BASE_URL = '/api';
const LOGIN_ENDPOINT = `${API_BASE_URL}/auth/login`;
const SIGNUP_ENDPOINT = `${API_BASE_URL}/auth/signup`;

// Form elements
const loginForm = document.getElementById('loginForm');
const signupForm = document.getElementById('signupForm');

// Handle login form submission
if (loginForm) {
    loginForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        const email = document.getElementById('email').value;
        const password = document.getElementById('password').value;

        try {
            const response = await fetch(LOGIN_ENDPOINT, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ email, password }),
            });

            const data = await response.json();

            if (response.ok) {
                // Store the token in localStorage
                localStorage.setItem('token', data.token);
                localStorage.setItem('user', JSON.stringify(data.user));

                // Redirect to home page
                window.location.href = '/';
            } else {
                alert(data.message || 'Login failed. Please try again.');
            }
        } catch (error) {
            console.error('Login error:', error);
            alert('An error occurred during login. Please try again.');
        }
    });
}

// Handle signup form submission
if (signupForm) {
    // We're using the MVC form submission, so we don't need to prevent default
    // Just add client-side validation for password matching
    signupForm.addEventListener('submit', (e) => {
        const password = document.getElementById('Password').value;
        const confirmPassword = document.getElementById('ConfirmPassword').value;

        // Validate passwords match
        if (password !== confirmPassword) {
            e.preventDefault();
            alert('Passwords do not match!');
            return false;
        }

        // Let the form submit normally to the MVC controller
        return true;
    });
}

// Check if user is logged in
function checkAuthStatus() {
    const token = localStorage.getItem('token');
    const user = JSON.parse(localStorage.getItem('user') || '{}');

    if (token && user) {
        // Update UI for logged-in user
        const navLinks = document.querySelector('.nav-links');
        if (navLinks) {
            navLinks.innerHTML = `
                <li><a href="/">Home</a></li>
                <li><a href="#menu">Menu</a></li>
                <li><a href="#about">About</a></li>
                <li><a href="#contact">Contact</a></li>
                <li><span>Welcome, ${user.fullName}</span></li>
                <li><a href="#" id="logoutBtn">Logout</a></li>
            `;

            // Add logout handler
            const logoutBtn = document.getElementById('logoutBtn');
            if (logoutBtn) {
                logoutBtn.addEventListener('click', handleLogout);
            }
        }
    }
}

// Handle logout
function handleLogout() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    window.location.href = '/';
}

// Check auth status when page loads
document.addEventListener('DOMContentLoaded', checkAuthStatus);