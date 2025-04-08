// Menu animations
document.addEventListener('DOMContentLoaded', function() {
    // Handle image loading errors
    const menuImages = document.querySelectorAll('.menu-item-image img');
    menuImages.forEach(img => {
        img.onerror = function() {
            // Try to get the category and item name from the parent elements
            const menuItem = this.closest('.menu-item-card');
            const itemName = menuItem ? menuItem.querySelector('.item-name')?.textContent : '';
            const categorySection = menuItem ? menuItem.closest('.category-section') : null;
            const categoryId = categorySection ? categorySection.getAttribute('data-category') : '';

            // Log the error
            console.error(`Failed to load image for ${itemName} in category ${categoryId}`);

            // Set a fallback image
            this.src = '/images/menu/placeholder.jpg?v=' + new Date().getTime();
        };
    });
    // Add staggered animation to menu items
    function animateMenuItems() {
        const menuItems = document.querySelectorAll('.menu-item-card');

        menuItems.forEach((item, index) => {
            // Remove any existing animation
            item.style.animation = 'none';

            // Force reflow
            void item.offsetWidth;

            // Add animation with delay based on index
            const delay = index * 0.1;
            item.style.animation = `fadeIn 0.5s ease-in-out ${delay}s both`;
        });
    }

    // Run animation when page loads
    animateMenuItems();

    // Re-run animation when category changes
    const categoryLinks = document.querySelectorAll('.category-item a');
    categoryLinks.forEach(link => {
        link.addEventListener('click', function() {
            // Wait for the DOM to update with new items
            setTimeout(animateMenuItems, 100);
        });
    });

    // Add hover effect for category images
    const categoryImages = document.querySelectorAll('.category-image');
    categoryImages.forEach(image => {
        image.addEventListener('mouseenter', function() {
            const img = this.querySelector('img');
            img.style.transform = 'scale(1.1)';
        });

        image.addEventListener('mouseleave', function() {
            const img = this.querySelector('img');
            img.style.transform = 'scale(1)';
        });
    });
});
