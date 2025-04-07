// Scroll animations for the website
document.addEventListener('DOMContentLoaded', function() {
    // Get all elements with the animate-on-scroll class
    const animatedElements = document.querySelectorAll('.animate-on-scroll');

    // Function to check if an element is in viewport
    function isInViewport(element) {
        const rect = element.getBoundingClientRect();
        return (
            rect.top <= (window.innerHeight || document.documentElement.clientHeight) * 0.85 &&
            rect.bottom >= 0
        );
    }

    // Function to handle scroll animation
    function handleScrollAnimation() {
        animatedElements.forEach(element => {
            if (isInViewport(element)) {
                // Get the delay attribute if it exists
                const delay = element.getAttribute('data-delay') || 0;

                // Add the animated class after the specified delay
                setTimeout(() => {
                    element.classList.add('animated');
                }, delay);
            }
        });
    }

    // Add scroll event listener
    window.addEventListener('scroll', handleScrollAnimation);

    // Trigger once on page load
    handleScrollAnimation();

    // Add hover animations for feature cards
    const featureCards = document.querySelectorAll('.feature-card');

    featureCards.forEach(card => {
        card.addEventListener('mouseenter', function() {
            const icon = this.querySelector('.feature-icon i');
            icon.classList.add('animated', 'heartBeat');

            setTimeout(() => {
                icon.classList.remove('animated', 'heartBeat');
            }, 1000);
        });
    });

    // Add typing animation for the about title
    const aboutTitle = document.querySelector('.about-title');
    if (aboutTitle) {
        // Create a typing animation when the about section comes into view
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    // Add a class to trigger the underline animation
                    setTimeout(() => {
                        aboutTitle.classList.add('title-animated');
                    }, 500);

                    // Unobserve after animation is triggered
                    observer.unobserve(aboutTitle);
                }
            });
        }, { threshold: 0.5 });

        observer.observe(aboutTitle);
    }

    // Add hover effect for about paragraphs
    const aboutParagraphs = document.querySelectorAll('.about-paragraph');
    aboutParagraphs.forEach(paragraph => {
        paragraph.addEventListener('mouseenter', function() {
            this.classList.add('paragraph-hovered');
        });

        paragraph.addEventListener('mouseleave', function() {
            this.classList.remove('paragraph-hovered');
        });
    });
});
