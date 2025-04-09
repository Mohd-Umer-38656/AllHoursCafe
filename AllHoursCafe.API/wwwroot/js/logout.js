// Logout functionality with cache clearing
document.addEventListener('DOMContentLoaded', function() {
    // Get the logout forms
    const logoutForm = document.getElementById('logoutForm');
    const adminLogoutForm = document.getElementById('adminLogoutForm');

    // Function to handle logout form submission
    function handleLogoutSubmit(e) {
            e.preventDefault();

            // Clear all localStorage items
            localStorage.clear();

            // Clear sessionStorage
            sessionStorage.clear();

            // Clear all cookies (except those needed for the form submission)
            clearCookies();

            // Clear cart data
            window.cart = [];

            // Update cart UI if functions exist
            if (typeof updateCartUI === 'function') {
                updateCartUI();
            }

            if (typeof updateCartCount === 'function') {
                updateCartCount();
            }

            // Call the clearCache function
            clearCache();

            // Get the form action URL
            const formAction = this.getAttribute('action');

            // Create a new FormData object from the form
            const formData = new FormData(this);

            // Use fetch to submit the form data
            fetch(formAction, {
                method: 'POST',
                body: formData,
                credentials: 'same-origin',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            }).then(function(response) {
                if (response.ok) {
                    // Force a complete page refresh to clear everything
                    window.location.href = '/';
                    // For an even more thorough refresh, you can use:
                    // window.location.reload(true);
                } else {
                    // If there was an error, still try to redirect to home
                    window.location.href = '/';
                }
            }).catch(function(error) {
                console.error('Logout error:', error);
                // Even if there's an error, try to redirect to home
                window.location.href = '/';
            });
    }

    // Add event listeners to the logout forms
    if (logoutForm) {
        logoutForm.addEventListener('submit', handleLogoutSubmit);
    }

    if (adminLogoutForm) {
        adminLogoutForm.addEventListener('submit', handleLogoutSubmit);
    }
});

// Function to clear cookies
function clearCookies() {
    const cookies = document.cookie.split(";");

    for (let i = 0; i < cookies.length; i++) {
        const cookie = cookies[i];
        const eqPos = cookie.indexOf("=");
        const name = eqPos > -1 ? cookie.substr(0, eqPos).trim() : cookie.trim();

        // Skip the anti-forgery cookie needed for form submission
        if (!name.includes('RequestVerificationToken')) {
            document.cookie = name + "=;expires=Thu, 01 Jan 1970 00:00:00 GMT;path=/";
        }
    }
}

// Function to clear browser cache for the current page
function clearCache() {
    // Clear Cache API if available
    if ('caches' in window) {
        caches.keys().then(function(names) {
            for (let name of names) {
                caches.delete(name);
            }
        }).catch(function(error) {
            console.error('Error clearing cache:', error);
        });
    }

    // Clear application cache if available (deprecated but still used in some browsers)
    if (window.applicationCache) {
        try {
            window.applicationCache.swapCache();
        } catch (e) {
            console.log('Application cache clear failed, but that\'s okay');
        }
    }

    // Clear service workers if available
    if ('serviceWorker' in navigator) {
        navigator.serviceWorker.getRegistrations().then(function(registrations) {
            for (let registration of registrations) {
                registration.unregister();
            }
        }).catch(function(error) {
            console.error('Error clearing service workers:', error);
        });
    }
}
