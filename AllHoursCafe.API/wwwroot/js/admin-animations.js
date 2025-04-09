// Admin Dashboard Animations
document.addEventListener('DOMContentLoaded', function() {
    // Add animation classes to elements
    animateAdminElements();

    // Add hover effects to cards
    setupCardHoverEffects();

    // Add counter animation to card values
    animateCounters();
});

// Function to add animation classes to elements
function animateAdminElements() {
    // Animate the admin header
    const adminHeader = document.querySelector('.admin-header');
    if (adminHeader) {
        adminHeader.classList.add('animate__animated', 'animate__fadeIn');
        adminHeader.style.animationDuration = '1s';
    }

    // Animate admin cards with staggered delay
    const adminCards = document.querySelectorAll('.admin-card');
    adminCards.forEach((card, index) => {
        card.classList.add('animate__animated', 'animate__fadeInUp');
        card.style.animationDuration = '0.8s';
        card.style.animationDelay = `${0.1 * (index + 1)}s`;
    });

    // Animate admin sections
    const adminSections = document.querySelectorAll('.admin-section');
    adminSections.forEach((section, index) => {
        section.classList.add('animate__animated', 'animate__fadeIn');
        section.style.animationDuration = '1s';
        section.style.animationDelay = `${0.5 + (0.2 * index)}s`;
    });

    // Animate cards in admin sections
    const sectionCards = document.querySelectorAll('.admin-section .card');
    sectionCards.forEach((card, index) => {
        card.classList.add('animate__animated', 'animate__fadeInUp');
        card.style.animationDuration = '0.8s';
        card.style.animationDelay = `${0.7 + (0.1 * index)}s`;
    });
}

// Function to add hover effects to cards
function setupCardHoverEffects() {
    const adminCards = document.querySelectorAll('.admin-card');

    adminCards.forEach(card => {
        // Add icon based on card title
        const cardTitle = card.querySelector('.admin-card-title');
        if (cardTitle) {
            let iconClass = 'fa-chart-line'; // Default icon

            if (cardTitle.textContent.includes('Menu')) {
                iconClass = 'fa-utensils';
            } else if (cardTitle.textContent.includes('Categories')) {
                iconClass = 'fa-list';
            } else if (cardTitle.textContent.includes('Reservations')) {
                iconClass = 'fa-calendar-check';
            } else if (cardTitle.textContent.includes('Contact')) {
                iconClass = 'fa-envelope';
            } else if (cardTitle.textContent.includes('User')) {
                iconClass = 'fa-users';
            }

            // Create icon element if it doesn't exist
            if (!card.querySelector('.admin-card-icon')) {
                const iconElement = document.createElement('i');
                iconElement.className = `fas ${iconClass} admin-card-icon`;
                card.appendChild(iconElement);
            }
        }

        // Add hover effect
        card.addEventListener('mouseenter', function() {
            const icon = this.querySelector('.admin-card-icon');
            if (icon) {
                icon.style.opacity = '0.3';
                icon.style.transform = 'scale(1.2) rotate(10deg)';
            }
        });

        card.addEventListener('mouseleave', function() {
            const icon = this.querySelector('.admin-card-icon');
            if (icon) {
                icon.style.opacity = '0.2';
                icon.style.transform = 'scale(1) rotate(0deg)';
            }
        });
    });
}

// Function to animate counters
function animateCounters() {
    const counters = document.querySelectorAll('.admin-card-value');

    counters.forEach((counter, index) => {
        // Store the original value
        const target = parseInt(counter.textContent) || 0;
        const duration = 1500; // Animation duration in milliseconds

        // Reset counter to 0
        counter.textContent = '0';

        // Create animation function using requestAnimationFrame
        function animateCounter() {
            const startTime = Date.now();

            function updateCounter() {
                const currentTime = Date.now();
                const elapsedTime = currentTime - startTime;

                if (elapsedTime < duration) {
                    // Calculate the current value based on elapsed time
                    const progress = elapsedTime / duration;
                    const currentValue = Math.round(progress * target);
                    counter.textContent = currentValue;

                    // Continue animation
                    requestAnimationFrame(updateCounter);
                } else {
                    // Animation complete, set to final value
                    counter.textContent = target;
                }
            }

            // Start the animation
            requestAnimationFrame(updateCounter);
        }

        // Start animation after a staggered delay
        setTimeout(animateCounter, 300 * index);
    });
}

// Add pulse effect to badges
const badges = document.querySelectorAll('.badge-primary');
badges.forEach(badge => {
    badge.classList.add('animate__animated', 'animate__pulse', 'animate__infinite');
});
