// Global image fallback handler
document.addEventListener('DOMContentLoaded', function() {
    // Handle all image loading errors globally
    document.addEventListener('error', function(e) {
        const target = e.target;
        
        // Check if the error is from an image
        if (target.tagName.toLowerCase() === 'img') {
            console.log('Image failed to load:', target.src);
            
            // Set fallback image with cache-busting
            target.src = '/images/menu/placeholder.jpg?v=' + new Date().getTime();
        }
    }, true); // Use capture phase to catch all errors
});
