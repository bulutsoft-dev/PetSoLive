// PetSoLive - Enhanced JavaScript
// Modern UI Utilities
class PetSoLiveUI {
    constructor() {
        this.init();
    }

    init() {
        this.setupAnimations();
        this.setupLoading();
        this.setupToasts();
        this.setupTooltips();
        this.setupProgressBars();
    }

    // Animation Setup
    setupAnimations() {
        // Intersection Observer for fade-in animations
        const observerOptions = {
            threshold: 0.1,
            rootMargin: '0px 0px -50px 0px'
        };

        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('fade-in');
                    observer.unobserve(entry.target);
                }
            });
        }, observerOptions);

        // Observe elements with animation classes
        document.querySelectorAll('.card-hover, .stat-card, .timeline-item').forEach(el => {
            observer.observe(el);
        });
    }

    // Loading System
    setupLoading() {
        this.showLoading = (message = 'Loading...') => {
            const overlay = document.createElement('div');
            overlay.className = 'loading-overlay';
            overlay.innerHTML = `
                <div class="loading-content">
                    <div class="loading-spinner"></div>
                    <p class="mt-3">${message}</p>
                </div>
            `;
            document.body.appendChild(overlay);
        };

        this.hideLoading = () => {
            const overlay = document.querySelector('.loading-overlay');
            if (overlay) {
                overlay.remove();
            }
        };
    }

    // Toast Notification System
    setupToasts() {
        this.showToast = (message, type = 'info', duration = 3000) => {
            const toast = document.createElement('div');
            toast.className = `toast-modern toast-${type}`;
            toast.innerHTML = `
                <div class="d-flex align-items-center">
                    <i class="fas fa-${this.getToastIcon(type)} me-2"></i>
                    <span>${message}</span>
                    <button class="btn-close ms-auto" onclick="this.parentElement.parentElement.remove()"></button>
                </div>
            `;
            
            document.body.appendChild(toast);
            
            // Show animation
            setTimeout(() => toast.classList.add('show'), 100);
            
            // Auto remove
            setTimeout(() => {
                toast.classList.remove('show');
                setTimeout(() => toast.remove(), 300);
            }, duration);
        };

        this.getToastIcon = (type) => {
            const icons = {
                success: 'check-circle',
                error: 'exclamation-circle',
                warning: 'exclamation-triangle',
                info: 'info-circle'
            };
            return icons[type] || 'info-circle';
        };
    }

    // Tooltip System
    setupTooltips() {
        document.querySelectorAll('[data-tooltip]').forEach(element => {
            element.addEventListener('mouseenter', (e) => {
                const tooltip = document.createElement('div');
                tooltip.className = 'tooltiptext';
                tooltip.textContent = e.target.dataset.tooltip;
                e.target.appendChild(tooltip);
            });

            element.addEventListener('mouseleave', (e) => {
                const tooltip = e.target.querySelector('.tooltiptext');
                if (tooltip) tooltip.remove();
            });
        });
    }

    // Progress Bar System
    setupProgressBars() {
        this.updateProgress = (selector, percentage) => {
            const progressBar = document.querySelector(selector);
            if (progressBar) {
                const fill = progressBar.querySelector('.progress-fill');
                if (fill) {
                    fill.style.width = `${percentage}%`;
                }
            }
        };
    }

    // Utility Functions
    debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }

    throttle(func, limit) {
        let inThrottle;
        return function() {
            const args = arguments;
            const context = this;
            if (!inThrottle) {
                func.apply(context, args);
                inThrottle = true;
                setTimeout(() => inThrottle = false, limit);
            }
        };
    }

    // Form Validation
    validateForm(formElement) {
        const inputs = formElement.querySelectorAll('input[required], select[required], textarea[required]');
        let isValid = true;

        inputs.forEach(input => {
            if (!input.value.trim()) {
                this.showFieldError(input, 'This field is required');
                isValid = false;
            } else {
                this.clearFieldError(input);
            }
        });

        return isValid;
    }

    showFieldError(field, message) {
        this.clearFieldError(field);
        field.classList.add('is-invalid');
        const errorDiv = document.createElement('div');
        errorDiv.className = 'invalid-feedback';
        errorDiv.textContent = message;
        field.parentNode.appendChild(errorDiv);
    }

    clearFieldError(field) {
        field.classList.remove('is-invalid');
        const errorDiv = field.parentNode.querySelector('.invalid-feedback');
        if (errorDiv) {
            errorDiv.remove();
        }
    }

    // Data Fetching with Loading
    async fetchWithLoading(url, options = {}) {
        this.showLoading();
        try {
            const response = await fetch(url, options);
            const data = await response.json();
            return data;
        } catch (error) {
            this.showToast('An error occurred while loading data', 'error');
            throw error;
        } finally {
            this.hideLoading();
        }
    }

    // Local Storage Utilities
    setLocalStorage(key, value) {
        try {
            localStorage.setItem(key, JSON.stringify(value));
        } catch (error) {
            console.error('Error saving to localStorage:', error);
        }
    }

    getLocalStorage(key, defaultValue = null) {
        try {
            const item = localStorage.getItem(key);
            return item ? JSON.parse(item) : defaultValue;
        } catch (error) {
            console.error('Error reading from localStorage:', error);
            return defaultValue;
        }
    }

    // Theme Management
    setTheme(theme) {
        document.documentElement.setAttribute('data-theme', theme);
        this.setLocalStorage('theme', theme);
    }

    getTheme() {
        return this.getLocalStorage('theme', 'light');
    }

    toggleTheme() {
        const currentTheme = this.getTheme();
        const newTheme = currentTheme === 'light' ? 'dark' : 'light';
        this.setTheme(newTheme);
    }

    // Responsive Utilities
    isMobile() {
        return window.innerWidth <= 768;
    }

    isTablet() {
        return window.innerWidth > 768 && window.innerWidth <= 1024;
    }

    isDesktop() {
        return window.innerWidth > 1024;
    }

    // Scroll Utilities
    smoothScrollTo(element) {
        element.scrollIntoView({
            behavior: 'smooth',
            block: 'start'
        });
    }

    scrollToTop() {
        window.scrollTo({
            top: 0,
            behavior: 'smooth'
        });
    }

    // Image Loading
    lazyLoadImages() {
        const images = document.querySelectorAll('img[data-src]');
        const imageObserver = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    img.src = img.dataset.src;
                    img.classList.remove('lazy');
                    imageObserver.unobserve(img);
                }
            });
        });

        images.forEach(img => imageObserver.observe(img));
    }
}

// Initialize UI when DOM is loaded
let petSoLiveUI;
document.addEventListener('DOMContentLoaded', function() {
    petSoLiveUI = new PetSoLiveUI();
    
    // Initialize lazy loading
    petSoLiveUI.lazyLoadImages();
    
    // Setup theme
    const savedTheme = petSoLiveUI.getTheme();
    petSoLiveUI.setTheme(savedTheme);
    updateThemeIcons();
    
    // Remove all code related to dynamically creating or showing scrollToTopBtn
});

// Global utility functions
window.showToast = (message, type, duration) => {
    if (petSoLiveUI) {
        petSoLiveUI.showToast(message, type, duration);
    }
};

window.showLoading = (message) => {
    if (petSoLiveUI) {
        petSoLiveUI.showLoading(message);
    }
};

window.hideLoading = () => {
    if (petSoLiveUI) {
        petSoLiveUI.hideLoading();
    }
};

// Theme toggle function for global access
window.toggleTheme = () => {
    if (petSoLiveUI) {
        petSoLiveUI.toggleTheme();
        updateThemeIcons();
    }
};

// Update theme icons based on current theme
function updateThemeIcons() {
    const currentTheme = petSoLiveUI ? petSoLiveUI.getTheme() : 'light';
    const darkIcons = document.querySelectorAll('.theme-icon-dark');
    const lightIcons = document.querySelectorAll('.theme-icon-light');
    
    if (currentTheme === 'dark') {
        darkIcons.forEach(icon => icon.style.display = 'none');
        lightIcons.forEach(icon => icon.style.display = 'inline-block');
    } else {
        darkIcons.forEach(icon => icon.style.display = 'inline-block');
        lightIcons.forEach(icon => icon.style.display = 'none');
    }
}
