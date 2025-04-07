// API endpoints
const MENU_API_BASE = '/api/menu';
let currentCategory = 'all';
let allMenuItems = [];
let categories = [];

// Load everything when the page loads
document.addEventListener('DOMContentLoaded', initializeMenu);

// Use server-side categories if available
if (typeof serverCategories !== 'undefined' && serverCategories) {
    console.log('Using server-side categories');
    categories = serverCategories;
}

async function initializeMenu() {
    try {
        // Skip loading categories if we already have them from the server
        if (categories.length === 0) {
            // Load categories
            const categoriesResponse = await fetch(`${MENU_API_BASE}/categories`);
            console.log('Categories response status:', categoriesResponse.status);

            // Store the response text for debugging
            const responseText = await categoriesResponse.text();
            console.log('Categories response text:', responseText);

            // Try to parse the response as JSON
            let categoriesData;
            try {
                categoriesData = JSON.parse(responseText);
            } catch (parseError) {
                console.error('Error parsing categories JSON:', parseError);
                throw new Error(`Failed to parse categories response: ${parseError.message}`);
            }

            console.log('Categories loaded:', categoriesData);

            // Check if the response is OK
            if (!categoriesResponse.ok) {
                throw new Error(categoriesData.message || `Failed to load categories: ${categoriesResponse.status}`);
            }

            // Ensure categories is an array
            categories = Array.isArray(categoriesData) ? categoriesData :
                        (categoriesData.value ? categoriesData.value : []);

            console.log('Processed categories:', categories);
        } else {
            console.log('Using server-side categories, skipping API call');
        }

        if (categories.length === 0) {
            throw new Error('No categories found');
        }

        // Load all menu items
        const itemsResponse = await fetch(`${MENU_API_BASE}/items`);
        console.log('Menu items response status:', itemsResponse.status);

        // Store the response text for debugging
        const itemsResponseText = await itemsResponse.text();
        console.log('Menu items response text:', itemsResponseText);

        // Try to parse the response as JSON
        let itemsData;
        try {
            itemsData = JSON.parse(itemsResponseText);
        } catch (parseError) {
            console.error('Error parsing menu items JSON:', parseError);
            throw new Error(`Failed to parse menu items response: ${parseError.message}`);
        }

        console.log('Menu items loaded:', itemsData);

        // Check if the response is OK
        if (!itemsResponse.ok) {
            throw new Error(itemsData.message || `Failed to load menu items: ${itemsResponse.status}`);
        }

        // Ensure allMenuItems is an array
        allMenuItems = Array.isArray(itemsData) ? itemsData :
                      (itemsData.value ? itemsData.value : []);

        console.log('Processed menu items:', allMenuItems);

        if (allMenuItems.length === 0) {
            throw new Error('No menu items found');
        }

        // Display all menu items grouped by category
        displayMenuItems(allMenuItems);

        // Set up category click handlers
        setupCategoryHandlers();

        // Set up scroll spy for categories
        setupScrollSpy();
    } catch (error) {
        console.error('Error initializing menu:', error);
        document.getElementById('menuItemsContainer').innerHTML = `
            <div class="error-message">
                <p>Failed to load menu. Please try again later.</p>
                <p>Error details: ${error.message}</p>
                <button class="retry-button" onclick="location.reload()">Retry</button>
            </div>
        `;

        // Also update the sidebar to show the error
        const sidebar = document.getElementById('categoriesSidebar');
        if (sidebar) {
            sidebar.innerHTML = `
                <div class="error-message">
                    <p>Failed to load categories.</p>
                </div>
            `;
        }
    }
}

function setupCategoryHandlers() {
    const categoryItems = document.querySelectorAll('.category-item');
    if (!categoryItems || categoryItems.length === 0) {
        console.warn('No category items found in the DOM');
        return;
    }

    categoryItems.forEach(item => {
        item.addEventListener('click', function(e) {
            e.preventDefault();

            // Remove active class from all categories
            categoryItems.forEach(cat => cat.classList.remove('active'));

            // Add active class to clicked category
            this.classList.add('active');

            // Get category ID
            const categoryId = this.getAttribute('data-category');
            if (!categoryId) {
                console.warn('Category item has no data-category attribute');
                return;
            }

            currentCategory = categoryId;

            // Filter and display menu items
            if (categoryId === 'all') {
                displayMenuItems(allMenuItems);
            } else {
                const filteredItems = allMenuItems.filter(item => {
                    // Check both categoryId and CategoryId to be safe
                    const itemCategoryId = item.categoryId || item.CategoryId;
                    return itemCategoryId == categoryId;
                });
                displayMenuItems(filteredItems, categoryId);
            }

            // Scroll to the category section
            const targetElement = document.getElementById(`category-${categoryId}`);
            if (targetElement) {
                targetElement.scrollIntoView({ behavior: 'smooth' });
            } else {
                console.warn(`Category section with ID category-${categoryId} not found`);
            }
        });
    });
}

function setupScrollSpy() {
    try {
        // Set up intersection observer for category sections
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const categoryId = entry.target.getAttribute('data-category');
                    if (categoryId) {
                        updateActiveCategoryInSidebar(categoryId);
                    }
                }
            });
        }, { threshold: 0.2 });

        // Observe all category sections
        const categorySections = document.querySelectorAll('.category-section');
        if (categorySections.length === 0) {
            console.warn('No category sections found for scroll spy');
        } else {
            categorySections.forEach(section => {
                observer.observe(section);
            });
        }

        // Handle scroll events for sticky sidebar
        window.addEventListener('scroll', () => {
            const sidebar = document.getElementById('categoriesSidebar');
            if (!sidebar) {
                console.warn('Categories sidebar not found');
                return;
            }

            const header = document.querySelector('.menu-header');
            if (!header) {
                console.warn('Menu header not found');
                return;
            }

            const headerBottom = header.getBoundingClientRect().bottom;

            if (headerBottom <= 0) {
                sidebar.classList.add('sticky');
            } else {
                sidebar.classList.remove('sticky');
            }
        });
    } catch (error) {
        console.error('Error setting up scroll spy:', error);
    }
}

function updateActiveCategoryInSidebar(categoryId) {
    if (!categoryId) {
        console.warn('No category ID provided to updateActiveCategoryInSidebar');
        return;
    }

    const categoryItems = document.querySelectorAll('.category-item');
    if (!categoryItems || categoryItems.length === 0) {
        console.warn('No category items found in the DOM');
        return;
    }

    categoryItems.forEach(item => {
        item.classList.remove('active');
        const itemCategoryId = item.getAttribute('data-category');
        if (itemCategoryId === categoryId) {
            item.classList.add('active');
        }
    });
}

function displayMenuItems(items, highlightCategoryId = null) {
    const container = document.getElementById('menuItemsContainer');
    console.log('Displaying menu items:', items);
    console.log('Categories:', categories);

    // Safety check - ensure we have arrays
    if (!Array.isArray(items)) {
        console.error('Items is not an array:', items);
        items = [];
    }

    if (!Array.isArray(categories)) {
        console.error('Categories is not an array:', categories);
        categories = [];
    }

    // Group items by category
    const itemsByCategory = {};

    if (highlightCategoryId) {
        // If a specific category is selected, only show those items
        const category = categories.find(c => {
            // Check both id and Id to be safe
            return (c.id == highlightCategoryId || c.Id == highlightCategoryId);
        });

        if (category) {
            const categoryId = category.id || category.Id;
            itemsByCategory[categoryId] = {
                category: category,
                items: items
            };
        }
    } else if (categories.length > 0) {
        // Group all items by their categories
        categories.forEach(category => {
            // Get category ID (handle both camelCase and PascalCase)
            const categoryId = category.id || category.Id;

            // Use lowercase property names to match the API response
            const categoryItems = items.filter(item => {
                // Check both categoryId and CategoryId to be safe
                const itemCategoryId = item.categoryId || item.CategoryId;
                return itemCategoryId == categoryId;
            });

            if (categoryItems.length > 0) {
                itemsByCategory[categoryId] = {
                    category: category,
                    items: categoryItems
                };
            }
        });
    }

    // Generate HTML for each category and its items
    let html = '';

    Object.values(itemsByCategory).forEach(({ category, items }) => {
        // Handle both camelCase and PascalCase property names for category
        const categoryId = category.id || category.Id || 0;
        const categoryName = category.name || category.Name || 'Unknown Category';
        const categoryDescription = category.description || category.Description || '';

        html += `
            <div id="category-${categoryId}" class="category-section" data-category="${categoryId}">
                <h2 class="category-title">${categoryName}</h2>
                <p class="category-description">${categoryDescription}</p>
                <div class="items-grid">
                    ${items.map(item => {
                        // Handle both camelCase and PascalCase property names
                        const name = item.name || item.Name;
                        const description = item.description || item.Description;
                        const price = item.price || item.Price;
                        const imageUrl = item.imageUrl || item.ImageUrl;
                        const isVegetarian = item.isVegetarian || item.IsVegetarian;
                        const isVegan = item.isVegan || item.IsVegan;
                        const isGlutenFree = item.isGlutenFree || item.IsGlutenFree;
                        const spicyLevel = item.spicyLevel || item.SpicyLevel;

                        return `
                        <div class="menu-item-card">
                            <div class="menu-item-image">
                                <img src="${imageUrl || '/images/menu/placeholder.jpg'}" alt="${name}">
                            </div>
                            <div class="item-details">
                                <div class="item-header">
                                    <h3 class="item-name">${name}</h3>
                                    <span class="item-price">$${typeof price === 'number' ? price.toFixed(2) : price}</span>
                                </div>
                                <p class="item-description">${description}</p>
                                <div class="item-meta">
                                    ${isVegetarian ? '<span class="badge vegetarian">Vegetarian</span>' : ''}
                                    ${isVegan ? '<span class="badge vegan">Vegan</span>' : ''}
                                    ${isGlutenFree ? '<span class="badge gluten-free">Gluten-Free</span>' : ''}
                                    ${spicyLevel && spicyLevel !== 'None' ? `<span class="badge spicy">Spicy: ${spicyLevel}</span>` : ''}
                                </div>
                                <button class="order-button">Add to Order</button>
                            </div>
                        </div>
                        `;
                    }).join('')}
                </div>
            </div>
        `;
    });

    // If no items found
    if (html === '') {
        html = `
            <div class="no-items-message">
                <p>No menu items found. Please try a different search or category.</p>
            </div>
        `;
    }

    container.innerHTML = html;
}

async function searchItems() {
    const query = document.getElementById('searchInput').value.trim();
    if (!query) {
        // Reset to show all items or current category
        if (currentCategory === 'all') {
            displayMenuItems(allMenuItems);
        } else {
            const filteredItems = allMenuItems.filter(item => {
                return (item.categoryId == currentCategory || item.CategoryId == currentCategory);
            });
            displayMenuItems(filteredItems, currentCategory);
        }
        return;
    }

    try {
        const response = await fetch(`${MENU_API_BASE}/items/search?query=${encodeURIComponent(query)}`);
        const items = await response.json();
        console.log('Search results:', items);

        // Reset category selection to 'all' when searching
        updateActiveCategoryInSidebar('all');
        currentCategory = 'all';

        // Display search results
        displayMenuItems(items);

        // Add search results heading
        const container = document.getElementById('menuItemsContainer');
        container.innerHTML = `
            <div class="search-results-header">
                <h2>Search Results for "${query}"</h2>
                <button class="clear-search" onclick="clearSearch()">Clear Search</button>
            </div>
        ` + container.innerHTML;
    } catch (error) {
        console.error('Error searching menu items:', error);
        alert('Failed to search menu items. Please try again later.');
    }
}

function clearSearch() {
    document.getElementById('searchInput').value = '';
    if (currentCategory === 'all') {
        displayMenuItems(allMenuItems);
    } else {
        const filteredItems = allMenuItems.filter(item => {
            return (item.categoryId == currentCategory || item.CategoryId == currentCategory);
        });
        displayMenuItems(filteredItems, currentCategory);
    }
}