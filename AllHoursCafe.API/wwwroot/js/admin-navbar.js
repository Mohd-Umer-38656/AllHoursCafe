// Admin Navbar JavaScript
document.addEventListener('DOMContentLoaded', function() {
    // Fix dropdown functionality
    initializeDropdowns();

    // Add active class to current nav item
    highlightCurrentNavItem();

    // Add hover effects to nav items
    addNavHoverEffects();

    // Add scroll effects to navbar
    addNavbarScrollEffects();
});

// Initialize Bootstrap dropdowns
function initializeDropdowns() {
    // Ensure jQuery is loaded
    if (typeof $ !== 'undefined') {
        // Bootstrap 5 initializes dropdowns automatically via data attributes

        // Handle window resize events
        $(window).on('resize', function() {
            handleResponsiveNavbar();
        });

        // Initial setup based on screen size
        handleResponsiveNavbar();

        // Add click handler for mobile navbar toggler
        $('.navbar-toggler').on('click', function() {
            console.log('Navbar toggler clicked');
        });
    } else {
        console.warn('jQuery or Bootstrap dropdown plugin not loaded');
    }
}

// Handle responsive navbar behavior based on screen size
function handleResponsiveNavbar() {
    if (window.innerWidth >= 992) {
        // Desktop behavior: hover dropdowns
        setupDesktopNavbar();
    } else {
        // Mobile behavior: click dropdowns
        setupMobileNavbar();
    }
}

// Setup desktop navbar behavior
function setupDesktopNavbar() {
    // Remove any mobile-specific event handlers
    $('.dropdown-toggle').off('click.mobile');

    // Setup hover behavior for desktop
    $('.navbar .dropdown').hover(
        function() {
            $(this).find('.dropdown-menu').first().stop(true, true).delay(200).slideDown(200);
            $(this).addClass('show');
            $(this).find('.dropdown-toggle').attr('aria-expanded', 'true');
        },
        function() {
            $(this).find('.dropdown-menu').first().stop(true, true).delay(100).slideUp(150);
            $(this).removeClass('show');
            $(this).find('.dropdown-toggle').attr('aria-expanded', 'false');
        }
    );
}

// Setup mobile navbar behavior
function setupMobileNavbar() {
    // Remove desktop hover handlers
    $('.navbar .dropdown').off('mouseenter mouseleave');

    // Add click handler for mobile dropdowns
    $('.dropdown-toggle').off('click.mobile').on('click.mobile', function(e) {
        e.preventDefault();
        e.stopPropagation();

        const $this = $(this);
        const $parent = $this.parent();
        const $menu = $this.next('.dropdown-menu');

        // Toggle this dropdown
        $parent.toggleClass('show');
        $menu.slideToggle(200);
        $this.attr('aria-expanded', $parent.hasClass('show'));

        // Close other open dropdowns
        $('.dropdown').not($parent).removeClass('show');
        $('.dropdown-menu').not($menu).slideUp(200);
        $('.dropdown-toggle').not($this).attr('aria-expanded', 'false');

        return false;
    });
}

// Highlight current nav item based on URL
function highlightCurrentNavItem() {
    const currentUrl = window.location.pathname;

    // Find the nav link that matches the current URL
    const navLinks = document.querySelectorAll('.navbar-nav .nav-link');

    navLinks.forEach(link => {
        const href = link.getAttribute('href');

        // Skip dropdown toggles
        if (link.classList.contains('dropdown-toggle')) {
            // Check if any dropdown item matches the current URL
            const dropdownItems = link.nextElementSibling.querySelectorAll('.dropdown-item');
            let isActive = false;

            dropdownItems.forEach(item => {
                const itemHref = item.getAttribute('href');
                if (currentUrl.includes(itemHref) || (itemHref && currentUrl.endsWith(itemHref.split('/').pop()))) {
                    item.classList.add('active');
                    isActive = true;
                }
            });

            if (isActive) {
                link.classList.add('active');
                link.parentElement.classList.add('active');
            }
        }
        // Regular nav links
        else if (href && (currentUrl === href || currentUrl.endsWith(href.split('/').pop()))) {
            link.classList.add('active');
            link.parentElement.classList.add('active');
        }
    });
}

// Add hover effects to nav items
function addNavHoverEffects() {
    const navItems = document.querySelectorAll('.navbar-nav .nav-item');

    navItems.forEach(item => {
        item.addEventListener('mouseenter', function() {
            const icon = this.querySelector('i');
            if (icon) {
                icon.classList.add('fa-bounce');
                setTimeout(() => {
                    icon.classList.remove('fa-bounce');
                }, 1000);
            }
        });
    });
}

// Add scroll effects to navbar
function addNavbarScrollEffects() {
    const navbar = document.querySelector('.navbar');

    if (navbar) {
        window.addEventListener('scroll', function() {
            if (window.scrollY > 50) {
                navbar.classList.add('navbar-scrolled');
            } else {
                navbar.classList.remove('navbar-scrolled');
            }
        });
    }
}
