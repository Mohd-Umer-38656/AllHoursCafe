// Admin Dashboard JavaScript

document.addEventListener('DOMContentLoaded', function() {
    // Image preview functionality
    const imageInput = document.getElementById('image-upload');
    const imagePreview = document.getElementById('image-preview');
    
    if (imageInput && imagePreview) {
        imageInput.addEventListener('change', function() {
            const file = this.files[0];
            if (file) {
                const reader = new FileReader();
                
                reader.addEventListener('load', function() {
                    imagePreview.innerHTML = `<img src="${this.result}" alt="Preview">`;
                    imagePreview.style.display = 'block';
                });
                
                reader.readAsDataURL(file);
            } else {
                imagePreview.innerHTML = '';
                imagePreview.style.display = 'none';
            }
        });
    }
    
    // Confirm delete actions
    const deleteButtons = document.querySelectorAll('.delete-confirm');
    
    deleteButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            if (!confirm('Are you sure you want to delete this item? This action cannot be undone.')) {
                e.preventDefault();
            }
        });
    });
    
    // Toggle form sections
    const toggleButtons = document.querySelectorAll('.toggle-section');
    
    toggleButtons.forEach(button => {
        button.addEventListener('click', function() {
            const targetId = this.getAttribute('data-target');
            const targetSection = document.getElementById(targetId);
            
            if (targetSection) {
                const isVisible = targetSection.style.display !== 'none';
                targetSection.style.display = isVisible ? 'none' : 'block';
                this.textContent = isVisible ? 'Show' : 'Hide';
            }
        });
    });
    
    // Price formatting
    const priceInputs = document.querySelectorAll('input[type="number"][step="0.01"]');
    
    priceInputs.forEach(input => {
        input.addEventListener('blur', function() {
            const value = parseFloat(this.value);
            if (!isNaN(value)) {
                this.value = value.toFixed(2);
            }
        });
    });
    
    // Category selection affects form fields
    const categorySelect = document.getElementById('CategoryId');
    
    if (categorySelect) {
        categorySelect.addEventListener('change', function() {
            // You can add custom logic here based on the selected category
            // For example, showing/hiding certain fields
        });
    }
    
    // Initialize any tooltips
    const tooltips = document.querySelectorAll('[data-toggle="tooltip"]');
    tooltips.forEach(tooltip => {
        tooltip.addEventListener('mouseenter', function() {
            const tip = document.createElement('div');
            tip.className = 'tooltip';
            tip.textContent = this.getAttribute('title');
            
            const rect = this.getBoundingClientRect();
            tip.style.position = 'absolute';
            tip.style.top = `${rect.bottom + window.scrollY + 5}px`;
            tip.style.left = `${rect.left + window.scrollX}px`;
            
            document.body.appendChild(tip);
            
            this.addEventListener('mouseleave', function() {
                document.body.removeChild(tip);
            }, { once: true });
        });
    });
    
    // Handle form validation
    const forms = document.querySelectorAll('.needs-validation');
    
    forms.forEach(form => {
        form.addEventListener('submit', function(event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            
            form.classList.add('was-validated');
        });
    });
});
