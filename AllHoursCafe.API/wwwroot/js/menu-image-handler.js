// General menu image handler
document.addEventListener('DOMContentLoaded', function() {
    console.log('Menu image handler loaded');
    
    // Find all menu item images
    const menuItems = document.querySelectorAll('.menu-item-card');
    console.log(`Found ${menuItems.length} menu items`);
    
    menuItems.forEach(item => {
        const itemName = item.querySelector('.item-name')?.textContent?.trim();
        const img = item.querySelector('img');
        
        if (img && itemName) {
            console.log(`Processing menu item: ${itemName}`);
            
            // Store the original src for fallback
            const originalSrc = img.src;
            
            // Create a robust fallback chain for images
            img.onerror = function() {
                console.error(`Image failed to load: ${this.src} for item: ${itemName}`);
                
                // Try different paths in sequence
                if (this.src === originalSrc) {
                    // 1. Try the category-specific path
                    const categorySection = item.closest('.category-section');
                    const categoryId = categorySection ? categorySection.getAttribute('data-category') : '';
                    const categoryName = getCategoryName(categoryId);
                    
                    console.log(`Trying category path for ${itemName} in ${categoryName}`);
                    const itemNameSlug = itemName.toLowerCase().replace(/[^a-z0-9]+/g, '-');
                    this.src = `/images/Items/${categoryName}/${itemNameSlug}.jpg?v=${new Date().getTime()}`;
                } 
                else if (this.src.includes('/images/Items/')) {
                    // 2. Try the root images folder
                    console.log(`Trying root images folder for ${itemName}`);
                    const itemNameSlug = itemName.toLowerCase().replace(/[^a-z0-9]+/g, '-');
                    this.src = `/images/${itemNameSlug}.jpg?v=${new Date().getTime()}`;
                }
                else {
                    // 3. Finally, use the placeholder
                    console.log(`Using placeholder for ${itemName}`);
                    this.src = '/images/placeholder.jpg?v=' + new Date().getTime();
                }
            };
            
            // Add a load event to confirm success
            img.onload = function() {
                console.log(`Successfully loaded image for ${itemName}: ${this.src}`);
            };
        }
    });
    
    // Helper function to get category name from ID
    function getCategoryName(categoryId) {
        // Map category IDs to names (this should be dynamically populated in a real app)
        const categoryMap = {
            '1': 'breakfast',
            '2': 'lunch',
            '3': 'dinner',
            '4': 'beverages',
            '5': 'dessert',
            '6': 'snacks'
        };
        
        return categoryMap[categoryId] || 'other';
    }
});
