// Special handler for the burger image
document.addEventListener('DOMContentLoaded', function() {
    console.log('Burger image fix script loaded');
    
    // Function to fix the burger image
    function fixBurgerImage() {
        // Find all burger menu items
        const menuItems = document.querySelectorAll('.menu-item-card');
        menuItems.forEach(item => {
            const itemName = item.querySelector('.item-name')?.textContent?.trim().toLowerCase();
            if (itemName === 'burger') {
                console.log('Found burger menu item, applying fix');
                const img = item.querySelector('img');
                if (img) {
                    // Force the image to use the root burger image which is known to work
                    img.src = '/images/burger.jpg?v=' + new Date().getTime();
                    console.log('Set burger image to root burger.jpg');
                    
                    // Add an onload handler to verify the image loaded
                    img.onload = function() {
                        console.log('Burger image loaded successfully from root');
                    };
                    
                    // Add an onerror handler as a fallback
                    img.onerror = function() {
                        console.error('Root burger image failed to load, trying lunch folder');
                        this.src = '/images/Items/lunch/burger.jpg?v=' + new Date().getTime();
                        
                        // If that fails too, use the placeholder
                        this.onerror = function() {
                            console.error('Lunch folder burger image failed too, using placeholder');
                            this.src = '/images/placeholder.jpg?v=' + new Date().getTime();
                        };
                    };
                }
            }
        });
    }
    
    // Run the fix immediately
    fixBurgerImage();
    
    // Also run it after a short delay to catch any dynamically loaded content
    setTimeout(fixBurgerImage, 500);
});
