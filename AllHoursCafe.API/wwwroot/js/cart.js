// Cart functionality
// Use window object to make cart globally available
window.cart = window.cart || [];
const MIN_ORDER_AMOUNT = 100; // Minimum order amount in rupees

// Load cart from localStorage if available
function loadCart() {
    const savedCart = localStorage.getItem('allHoursCafeCart');
    if (savedCart) {
        window.cart = JSON.parse(savedCart);
        updateCartUI();
        updateCartCount();
    }
}

// Save cart to localStorage
function saveCart() {
    localStorage.setItem('allHoursCafeCart', JSON.stringify(window.cart));

    // Also save to session for server-side access
    saveCartToSession();
}

// Save cart to session
async function saveCartToSession() {
    try {
        await fetch('/api/cart/save', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(window.cart)
        });
    } catch (error) {
        console.error('Error saving cart to session:', error);
    }
}

// Add item to cart
function addToCart(id, name, price, imageUrl) {
    // Check if item is already in cart
    const existingItem = window.cart.find(item => item.id === id);

    if (existingItem) {
        // Increment quantity if item already exists
        existingItem.quantity += 1;
    } else {
        // Add new item to cart
        window.cart.push({
            id: id,
            name: name,
            price: price,
            imageUrl: imageUrl,
            quantity: 1
        });
    }

    // Update UI and save cart
    updateCartUI();
    updateCartCount(true); // Pass true to animate the cart icon
    saveCart();

    // Show notification
    showNotification(`${name} added to cart`);
}

// Remove item from cart
function removeFromCart(id) {
    const index = window.cart.findIndex(item => item.id === id);
    if (index !== -1) {
        const removedItem = window.cart[index];
        window.cart.splice(index, 1);
        updateCartUI();
        updateCartCount();
        saveCart();
        showNotification(`${removedItem.name} removed from cart`);
    }
}

// Update item quantity
function updateQuantity(id, newQuantity) {
    const item = window.cart.find(item => item.id === id);
    if (item) {
        if (newQuantity <= 0) {
            removeFromCart(id);
        } else {
            const oldQuantity = item.quantity;
            item.quantity = newQuantity;
            updateCartUI();

            // Animate the cart icon only if quantity increased
            updateCartCount(newQuantity > oldQuantity);
            saveCart();
        }
    }
}

// Calculate cart total
function calculateTotal() {
    return window.cart.reduce((total, item) => total + (item.price * item.quantity), 0);
}

// Update cart UI
function updateCartUI() {
    const cartItemsElement = document.getElementById('cartItems');
    const cartTotalElement = document.getElementById('cartTotal');
    const checkoutButton = document.getElementById('checkoutButton');
    const minOrderMessage = document.getElementById('minOrderMessage');

    if (!cartItemsElement) return; // Exit if cart elements don't exist

    // Clear current cart items
    cartItemsElement.innerHTML = '';

    if (window.cart.length === 0) {
        // Cart is empty
        cartItemsElement.innerHTML = '<p class="empty-cart-message">Your cart is empty</p>';
        if (cartTotalElement) cartTotalElement.textContent = '₹0.00';
        if (checkoutButton) {
            checkoutButton.style.display = 'none'; // Hide the checkout link
            checkoutButton.classList.remove('active');
        }
        if (minOrderMessage) minOrderMessage.textContent = 'Add items to your cart to proceed';
    } else {
        // Add each item to the cart UI
        window.cart.forEach(item => {
            const itemElement = document.createElement('div');
            itemElement.className = 'cart-item';
            itemElement.innerHTML = `
                <div class="cart-item-details">
                    <h3>${item.name}</h3>
                    <div class="cart-item-price">₹${(item.price * item.quantity).toFixed(2)}</div>
                </div>
                <div class="cart-item-quantity">
                    <button onclick="updateQuantity(${item.id}, ${item.quantity - 1})" class="quantity-btn">-</button>
                    <span>${item.quantity}</span>
                    <button onclick="updateQuantity(${item.id}, ${item.quantity + 1})" class="quantity-btn">+</button>
                </div>
                <button onclick="removeFromCart(${item.id})" class="remove-item-btn">×</button>
            `;
            cartItemsElement.appendChild(itemElement);
        });

        // Update total and checkout button
        const total = calculateTotal();
        if (cartTotalElement) cartTotalElement.textContent = `₹${total.toFixed(2)}`;

        // Update checkout button and message
        if (checkoutButton && minOrderMessage) {
            if (total >= MIN_ORDER_AMOUNT) {
                // Enable checkout
                checkoutButton.style.display = 'block'; // Show the checkout link
                checkoutButton.classList.add('active');
                minOrderMessage.textContent = '';
                console.log('Checkout enabled, link shown');
            } else {
                // Disable checkout
                checkoutButton.style.display = 'none'; // Hide the checkout link
                checkoutButton.classList.remove('active');
                const remaining = MIN_ORDER_AMOUNT - total;
                minOrderMessage.textContent = `Add ₹${remaining.toFixed(2)} more to meet minimum order amount`;
                console.log('Checkout disabled, link hidden');
            }
        }
    }
}

// Update cart count in the header
function updateCartCount(animate = false) {
    const cartCountElement = document.getElementById('cartCount');
    const cartLinkElement = document.querySelector('.cart-link');

    if (cartCountElement) {
        const itemCount = window.cart.reduce((count, item) => count + item.quantity, 0);
        cartCountElement.textContent = itemCount;

        if (itemCount > 0) {
            cartCountElement.style.display = 'flex';

            // Add animation if requested
            if (animate && cartLinkElement) {
                // Remove any existing animation classes
                cartLinkElement.classList.remove('cart-bounce', 'cart-pulse');

                // Force a reflow to restart the animation
                void cartLinkElement.offsetWidth;

                // Add the bounce animation class
                cartLinkElement.classList.add('cart-bounce');

                // Add pulse animation to the count
                cartCountElement.classList.remove('cart-pulse');
                void cartCountElement.offsetWidth;
                cartCountElement.classList.add('cart-pulse');

                // Remove the animation classes after they complete
                setTimeout(() => {
                    cartLinkElement.classList.remove('cart-bounce');
                    cartCountElement.classList.remove('cart-pulse');
                }, 1000);
            }
        } else {
            cartCountElement.style.display = 'none';
        }
    }
}

// Show notification
function showNotification(message) {
    // Check if a notification already exists
    let notification = document.querySelector('.notification');

    // If not, create a new one
    if (!notification) {
        notification = document.createElement('div');
        notification.className = 'notification';
        document.body.appendChild(notification);
    }

    // Update the message
    notification.textContent = message;
    notification.classList.remove('fade-out');

    // Remove notification after 3 seconds
    clearTimeout(window.notificationTimeout);
    window.notificationTimeout = setTimeout(() => {
        notification.classList.add('fade-out');
        setTimeout(() => {
            if (notification.parentNode) {
                document.body.removeChild(notification);
            }
        }, 500);
    }, 2500);
}

// Initialize cart
document.addEventListener('DOMContentLoaded', function() {
    loadCart();

    // Set up cart toggle if it exists
    const cartToggle = document.getElementById('cartToggle');
    const shoppingCart = document.getElementById('shoppingCart');

    if (cartToggle && shoppingCart) {
        cartToggle.addEventListener('click', function() {
            shoppingCart.classList.toggle('collapsed');
            cartToggle.innerHTML = shoppingCart.classList.contains('collapsed') ?
                '<i class="fas fa-chevron-up"></i>' :
                '<i class="fas fa-chevron-down"></i>';
        });
    }

    // The checkout button is now an <a> tag that links directly to /Checkout
    // No need to add an event listener
    console.log('Cart.js: Checkout button is now a direct link to /Checkout');
});
