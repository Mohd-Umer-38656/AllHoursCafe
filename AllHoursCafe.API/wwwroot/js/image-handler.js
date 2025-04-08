// Image handler for the admin dashboard
document.addEventListener('DOMContentLoaded', function() {
    // Fix image paths for dessert items
    const dessertItems = ['Chocolate Brownie', 'New York Cheesecake', 'Fruit Tart'];
    
    // Find all images in the admin dashboard
    const images = document.querySelectorAll('img');
    
    images.forEach(img => {
        // Add error handler to all images
        img.onerror = function() {
            // Get the item name from the alt attribute
            const itemName = this.alt;
            
            // Check if this is a dessert item
            if (dessertItems.includes(itemName)) {
                // Try to load the image from the dessert folder
                const fileName = itemName.toLowerCase().replace(/ /g, '-') + '.jpg';
                this.src = `/images/Items/dessert/${fileName}?v=${new Date().getTime()}`;
            } else {
                // Use a placeholder image
                this.src = '/images/Items/placeholder.jpg';
            }
        };
    });
});
