// Global image fallback handler
document.addEventListener('DOMContentLoaded', function() {
    // Log all image sources for debugging
    const allImages = document.querySelectorAll('img');
    console.log('Total images on page:', allImages.length);

    // Log all images for debugging
    allImages.forEach(img => {
        console.log('Image source:', img.src, 'Alt:', img.alt);
    });

    // Handle all image loading errors globally
    document.addEventListener('error', function(e) {
        const target = e.target;

        // Check if the error is from an image
        if (target.tagName.toLowerCase() === 'img') {
            console.error('Image failed to load:', target.src, 'Alt:', target.alt);

            // Try to determine the category from parent elements
            const menuItem = target.closest('.menu-item-card');
            const itemName = menuItem ? menuItem.querySelector('.item-name')?.textContent?.trim() : '';
            console.log('Menu item name:', itemName);

            // Set fallback image with cache-busting
            target.src = '/images/placeholder.jpg?v=' + new Date().getTime();
        }
    }, true); // Use capture phase to catch all errors
});
