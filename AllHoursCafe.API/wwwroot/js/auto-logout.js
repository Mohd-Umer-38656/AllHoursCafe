/**
 * Auto Logout Functionality
 * Automatically logs out users after a period of inactivity
 */

// Configuration
const AUTO_LOGOUT_CONFIG = {
    // Default times (can be overridden based on user role)
    default: {
        // Time in milliseconds before showing the warning dialog (20 minutes)
        warningTime: 20 * 60 * 1000,

        // Time in milliseconds to show the warning dialog before logout (1 minute)
        countdownTime: 60 * 1000
    },

    // Admin-specific times (longer session for admins)
    admin: {
        // Time in milliseconds before showing the warning dialog (30 minutes)
        warningTime: 30 * 60 * 1000,

        // Time in milliseconds to show the warning dialog before logout (1 minute)
        countdownTime: 60 * 1000
    },

    // URL to redirect to after logout
    logoutUrl: '/Auth/Logout',

    // Whether to show debug messages in console
    debug: false
};

// Determine if the user is an admin based on the URL or other indicators
const isAdmin = window.location.pathname.toLowerCase().includes('/admin');

// Set the appropriate configuration based on user role
const activeConfig = isAdmin ? AUTO_LOGOUT_CONFIG.admin : AUTO_LOGOUT_CONFIG.default;

// State variables
let warningTimer = null;
let countdownTimer = null;
let countdownInterval = null;
let lastActivity = Date.now();
let warningDialogShown = false;

// Initialize the auto logout functionality
function initAutoLogout() {
    log('Auto logout initialized');

    // Reset the timer on user activity
    const activityEvents = [
        'mousedown', 'mousemove', 'keypress',
        'scroll', 'touchstart', 'click', 'keydown'
    ];

    activityEvents.forEach(event => {
        document.addEventListener(event, resetTimer);
    });

    // Start the initial timer
    resetTimer();

    // Create the warning dialog if it doesn't exist
    createWarningDialog();
}

// Reset the inactivity timer
function resetTimer() {
    lastActivity = Date.now();

    // Clear existing timers
    if (warningTimer) {
        clearTimeout(warningTimer);
    }

    if (countdownTimer) {
        clearTimeout(countdownTimer);
    }

    if (countdownInterval) {
        clearInterval(countdownInterval);
    }

    // Hide the warning dialog if it's shown
    if (warningDialogShown) {
        hideWarningDialog();
    }

    // Set a new timer
    warningTimer = setTimeout(showWarningDialog, activeConfig.warningTime);

    log('Timer reset');
}

// Show the warning dialog
function showWarningDialog() {
    log('Showing warning dialog');
    warningDialogShown = true;

    const warningDialog = document.getElementById('auto-logout-warning');
    const countdownElement = document.getElementById('auto-logout-countdown');

    if (warningDialog && countdownElement) {
        // Calculate the countdown in seconds
        let countdown = Math.floor(activeConfig.countdownTime / 1000);
        countdownElement.textContent = countdown;

        // Show the dialog
        warningDialog.classList.add('show');

        // Start the countdown
        countdownInterval = setInterval(() => {
            countdown--;
            countdownElement.textContent = countdown;

            if (countdown <= 0) {
                clearInterval(countdownInterval);
            }
        }, 1000);

        // Set the final logout timer
        countdownTimer = setTimeout(performLogout, activeConfig.countdownTime);
    }
}

// Hide the warning dialog
function hideWarningDialog() {
    log('Hiding warning dialog');
    warningDialogShown = false;

    const warningDialog = document.getElementById('auto-logout-warning');

    if (warningDialog) {
        warningDialog.classList.remove('show');
    }
}

// Perform the logout action
function performLogout() {
    log('Performing logout');

    // Submit the logout form if it exists
    const logoutForm = document.getElementById('logoutForm') || document.getElementById('adminLogoutForm');

    if (logoutForm) {
        logoutForm.submit();
    } else {
        // Fallback to redirect
        window.location.href = AUTO_LOGOUT_CONFIG.logoutUrl;
    }

    // Show a message to the user
    alert('You have been logged out due to inactivity.');
}

// Create the warning dialog
function createWarningDialog() {
    // Check if the dialog already exists
    if (document.getElementById('auto-logout-warning')) {
        return;
    }

    // Create the dialog element
    const dialogHtml = `
        <div id="auto-logout-warning" class="auto-logout-dialog">
            <div class="auto-logout-content">
                <div class="auto-logout-header">
                    <h5>Session Timeout Warning</h5>
                </div>
                <div class="auto-logout-body">
                    <p>Your session is about to expire due to inactivity.</p>
                    <p>You will be logged out in <span id="auto-logout-countdown">60</span> seconds.</p>
                </div>
                <div class="auto-logout-footer">
                    <button id="auto-logout-stay" class="btn btn-primary">Stay Logged In</button>
                    <button id="auto-logout-logout" class="btn btn-secondary">Logout Now</button>
                </div>
            </div>
        </div>
    `;

    // Append the dialog to the body
    const dialogContainer = document.createElement('div');
    dialogContainer.innerHTML = dialogHtml;
    document.body.appendChild(dialogContainer.firstElementChild);

    // Add event listeners
    document.getElementById('auto-logout-stay').addEventListener('click', resetTimer);
    document.getElementById('auto-logout-logout').addEventListener('click', performLogout);
}

// Helper function for logging
function log(message) {
    if (AUTO_LOGOUT_CONFIG.debug) {
        console.log(`[Auto Logout] ${message}`);
    }
}

// Initialize when the DOM is ready
document.addEventListener('DOMContentLoaded', initAutoLogout);
