// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Sidebar Functionality
document.addEventListener('DOMContentLoaded', function () {
    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Initialize popovers
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });

    // Sidebar toggle on mobile
    const sidebarToggler = document.getElementById('sidebarToggler');
    const mobileSidebarToggler = document.getElementById('mobileSidebarToggler');
    const sidebar = document.getElementById('sidebar');
    
    if (sidebarToggler) {
        sidebarToggler.addEventListener('click', function () {
            sidebar.classList.toggle('show');
        });
    }
    
    if (mobileSidebarToggler) {
        mobileSidebarToggler.addEventListener('click', function () {
            sidebar.classList.toggle('show');
        });
    }
    
    // Close sidebar when clicking outside on mobile
    document.addEventListener('click', function (event) {
        const isClickInsideSidebar = sidebar && sidebar.contains(event.target);
        const isClickOnToggler = (sidebarToggler && sidebarToggler.contains(event.target)) || 
                               (mobileSidebarToggler && mobileSidebarToggler.contains(event.target));
        
        if (!isClickInsideSidebar && !isClickOnToggler && sidebar && sidebar.classList.contains('show') && window.innerWidth < 768) {
            sidebar.classList.remove('show');
        }
    });

    // Highlight active sidebar link
    highlightActiveNavLink();

    // Initialize any charts on the page
    initializeCharts();

    // Form validation styles
    enableCustomFormValidation();
    
    // Apply saved theme settings on page load
    applySavedThemeSettings();
});

// Animasyon yardımcı fonksiyonları
document.addEventListener('DOMContentLoaded', function () {
    // Sayfa içi bağlantılar için yumuşak kaydırma
    document.querySelectorAll('a[href^="#"]:not([data-bs-toggle])').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                window.scrollTo({
                    top: target.offsetTop - 80,
                    behavior: 'smooth'
                });
            }
        });
    });

    // Liste animasyonlarını başlat
    initListAnimations();

    // Hover animasyonları için sınıflar ekleyelim
    applyHoverEffects();
    
    // Kart animasyonları için sınıflar ekleyelim
    applyCardAnimations();
    
    // Sayfa geçiş animasyonu
    animatePageTransition();
    
    // Veri yükleme animasyonlarını otomatik ekleyelim
    convertLoadingElements();
    
    // Tablo satır animasyonları
    animateTableRows();
    
    // Bildirim rozetlerini canlandıralım
    animateNotifications();
});

// Sayfa geçiş animasyonu
function animatePageTransition() {
    // Sayfa yüklendiğinde içeriği animasyonlu göster
    const mainContent = document.querySelector('.container-fluid.px-4');
    if (mainContent) {
        mainContent.style.opacity = '0';
        setTimeout(() => {
            mainContent.classList.add('animate-fade-in');
            mainContent.style.opacity = '1';
        }, 50);
    }
}

// Liste animasyonlarını başlat
function initListAnimations() {
    // Tablo, liste ve kart gruplarında animasyon efektleri
    const listContainers = document.querySelectorAll('.table tbody, .list-group, .card-deck, .row.g-3');
    
    listContainers.forEach(container => {
        if (!container.classList.contains('no-animation')) {
            container.classList.add('animate-list');
        }
    });
}

// Hover efektlerini uygula
function applyHoverEffects() {
    // Kartlara hover efektleri
    document.querySelectorAll('.card:not(.no-animation)').forEach(card => {
        card.classList.add('hover-shadow');
    });
    
    // Butonlara ripple efekti
    document.querySelectorAll('.btn:not(.no-animation):not(.btn-link)').forEach(btn => {
        btn.classList.add('btn-ripple');
    });
}

// Kart animasyonlarını uygula
function applyCardAnimations() {
    // Dashboard istatistik kartları için sayaç animasyonu
    const statCards = document.querySelectorAll('.stats-card-value');
    
    statCards.forEach(card => {
        const finalValue = parseInt(card.textContent.replace(/[^\d]/g, ''));
        if (!isNaN(finalValue) && finalValue > 0) {
            animateCounter(card, 0, finalValue);
        }
    });
}

// Sayaç animasyonu
function animateCounter(element, start, end) {
    const duration = 1500;
    const frameDuration = 1000 / 60; // 60fps
    const totalFrames = Math.round(duration / frameDuration);
    const increment = (end - start) / totalFrames;
    
    let currentFrame = 0;
    let currentValue = start;
    const originalText = element.textContent;
    const hasNonNumeric = /[^\d]/.test(originalText);
    
    const animate = () => {
        currentFrame++;
        currentValue += increment;
        
        if (currentFrame === totalFrames) {
            if (hasNonNumeric) {
                element.textContent = originalText;
            } else {
                element.textContent = end;
            }
            return;
        }
        
        const value = Math.round(currentValue);
        if (hasNonNumeric) {
            element.textContent = originalText.replace(/\d+/, value);
        } else {
            element.textContent = value;
        }
        
        requestAnimationFrame(animate);
    };
    
    requestAnimationFrame(animate);
}

// Loading durumu göstergeleri oluştur
function convertLoadingElements() {
    // Loading metni olan öğelere animasyonlu spinner ekle
    document.querySelectorAll('.loading, .loading-text').forEach(element => {
        if (element.textContent.toLowerCase().includes('yükleniyor') || 
            element.textContent.toLowerCase().includes('loading')) {
            const originalText = element.textContent;
            element.innerHTML = `
                <span class="loading-spinner me-2"></span>
                ${originalText}
            `;
        }
    });
    
    // Yükleme yapan butonlara spinner ekle
    document.querySelectorAll('.btn-loading').forEach(button => {
        const originalHTML = button.innerHTML;
        button.setAttribute('data-original-html', originalHTML);
        button.innerHTML = `
            <span class="loading-spinner me-2" style="width: 16px; height: 16px; border-width: 1px;"></span>
            Yükleniyor...
        `;
        
        // Yükleme bittiğinde eski içeriği geri yükle (örnek olarak 3 saniye sonra)
        setTimeout(() => {
            button.innerHTML = button.getAttribute('data-original-html');
            button.classList.remove('btn-loading');
            button.removeAttribute('disabled');
        }, 3000);
    });
}

// Tablo satır animasyonları
function animateTableRows() {
    const tables = document.querySelectorAll('table.table:not(.no-animation)');
    
    tables.forEach(table => {
        const tbody = table.querySelector('tbody');
        if (tbody) {
            const rows = tbody.querySelectorAll('tr');
            rows.forEach((row, index) => {
                row.style.opacity = '0';
                setTimeout(() => {
                    row.classList.add('animate-slide-up');
                    row.style.opacity = '1';
                    row.style.animationDelay = `${index * 0.05}s`;
                }, 100);
            });
        }
    });
}

// Bildirim rozetlerini canlandır
function animateNotifications() {
    const badges = document.querySelectorAll('.badge.bg-danger');
    
    badges.forEach(badge => {
        if (!badge.classList.contains('no-animation')) {
            badge.classList.add('notification-badge');
        }
    });
}

// Sidebar toggle animation
document.addEventListener('DOMContentLoaded', function() {
    const sidebarItems = document.querySelectorAll('.sidebar-item');
    
    sidebarItems.forEach((item, index) => {
        item.style.opacity = '0';
        item.style.transform = 'translateX(-10px)';
        
        setTimeout(() => {
            item.style.transition = 'opacity 0.3s ease, transform 0.3s ease';
            item.style.opacity = '1';
            item.style.transform = 'translateX(0)';
        }, 100 + (index * 50)); // Her öğe için kademeli animasyon
    });
});

// Apply theme settings from localStorage
function applySavedThemeSettings() {
    const rootElement = document.documentElement;
    
    // Apply theme mode (dark/light)
    const themeMode = localStorage.getItem('themeMode');
    if (themeMode === 'dark') {
        rootElement.classList.add('dark-mode');
        
        // Update theme switcher if on settings page
        const darkModeRadio = document.getElementById('darkMode');
        if (darkModeRadio) {
            darkModeRadio.checked = true;
        }
        
        // Koyu tema aktifse yüksek kontrast switch'ini devre dışı bırak
        const highContrastSwitch = document.getElementById('highContrastSwitch');
        if (highContrastSwitch) {
            highContrastSwitch.disabled = true;
            highContrastSwitch.checked = false; // Emin olmak için false yap
            
            // LocalStorage'dan da kaldır
            localStorage.removeItem('highContrast');
        }
    } else if (themeMode === 'light') {
        rootElement.classList.remove('dark-mode');
        
        // Update theme switcher if on settings page
        const lightModeRadio = document.getElementById('lightMode');
        if (lightModeRadio) {
            lightModeRadio.checked = true;
        }
        
        // Açık temada yüksek kontrast aktif edilebilir
        const highContrastSwitch = document.getElementById('highContrastSwitch');
        if (highContrastSwitch) {
            highContrastSwitch.disabled = false;
        }
    } else if (themeMode === 'auto') {
        // Check system preference
        const prefersDarkMode = window.matchMedia('(prefers-color-scheme: dark)').matches;
        if (prefersDarkMode) {
            rootElement.classList.add('dark-mode');
            
            // Sistem tercihi koyu temaysa yüksek kontrast devre dışı olmalı
            const highContrastSwitch = document.getElementById('highContrastSwitch');
            if (highContrastSwitch) {
                highContrastSwitch.disabled = true;
                highContrastSwitch.checked = false;
                localStorage.removeItem('highContrast');
            }
        } else {
            rootElement.classList.remove('dark-mode');
            
            // Sistem tercihi açık temaysa yüksek kontrast aktif edilebilir
            const highContrastSwitch = document.getElementById('highContrastSwitch');
            if (highContrastSwitch) {
                highContrastSwitch.disabled = false;
            }
        }
        
        // Update theme switcher if on settings page
        const autoModeRadio = document.getElementById('autoMode');
        if (autoModeRadio) {
            autoModeRadio.checked = true;
        }
    }
    
    // Apply color palette
    const colorPalette = localStorage.getItem('colorPalette');
    if (colorPalette) {
        // Remove any existing color palette
        rootElement.removeAttribute('data-color-palette');
        
        // Only apply if not default
        if (colorPalette !== 'default') {
            rootElement.setAttribute('data-color-palette', colorPalette);
        }
        
        // Update color palette selector if on settings page
        const colorPaletteRadio = document.getElementById(colorPalette + 'Color');
        if (colorPaletteRadio) {
            colorPaletteRadio.checked = true;
        }
    }
    
    // Apply font size
    const fontSize = localStorage.getItem('fontSize');
    if (fontSize) {
        rootElement.style.setProperty('--base-font-size', (fontSize / 100) + 'rem');
        
        // Update font size slider if on settings page
        const fontSizeRange = document.getElementById('fontSizeRange');
        if (fontSizeRange) {
            fontSizeRange.value = fontSize;
            
            // Also update the label
            const fontSizeLabel = document.getElementById('fontSizeLabel');
            if (fontSizeLabel) {
                fontSizeLabel.textContent = fontSize + '%';
            }
        }
    }
    
    // Apply font family
    const fontFamily = localStorage.getItem('fontFamily');
    if (fontFamily) {
        rootElement.style.setProperty('--font-family', fontFamily);
        
        // Update font family selector if on settings page
        const fontFamilySelect = document.getElementById('fontFamily');
        if (fontFamilySelect) {
            fontFamilySelect.value = fontFamily;
        }
    }
    
    // Apply compact layout
    const compactLayout = localStorage.getItem('compactLayout') === 'true';
    if (compactLayout) {
        document.body.classList.add('compact-layout');
        
        // Update checkbox if on settings page
        const compactLayoutSwitch = document.getElementById('compactLayoutSwitch');
        if (compactLayoutSwitch) {
            compactLayoutSwitch.checked = true;
        }
    } else {
        document.body.classList.remove('compact-layout');
    }
    
    // Apply animations setting
    const animations = localStorage.getItem('animations') !== 'false'; // Default is true
    if (!animations) {
        document.body.classList.add('no-animations');
        
        // Update checkbox if on settings page
        const animationsSwitch = document.getElementById('animationsSwitch');
        if (animationsSwitch) {
            animationsSwitch.checked = false;
        }
    } else {
        document.body.classList.remove('no-animations');
    }
    
    // Apply high contrast mode - koyu tema ile çakışmamasını sağla
    const isDarkMode = rootElement.classList.contains('dark-mode');
    const highContrast = localStorage.getItem('highContrast') === 'true';
    
    if (highContrast && !isDarkMode) {
        // Sadece açık temada yüksek kontrast uygula
        document.body.classList.add('high-contrast');
        
        // Update checkbox if on settings page
        const highContrastSwitch = document.getElementById('highContrastSwitch');
        if (highContrastSwitch) {
            highContrastSwitch.checked = true;
        }
    } else {
        document.body.classList.remove('high-contrast');
        
        // Koyu temadaysa highContrast ayarını localStorage'dan kaldır
        if (isDarkMode) {
            localStorage.removeItem('highContrast');
        }
    }
    
    // Apply border radius
    const borderRadius = localStorage.getItem('borderRadius');
    if (borderRadius) {
        // Remove any existing border radius class
        document.body.classList.remove('border-radius-0', 'border-radius-1', 'border-radius-2', 'border-radius-3', 'border-radius-4');
        document.body.classList.add('border-radius-' + borderRadius);
        
        // Update range input if on settings page
        const borderRadiusRange = document.getElementById('borderRadiusRange');
        if (borderRadiusRange) {
            borderRadiusRange.value = borderRadius;
        }
    } else {
        // Default is 2 (medium)
        document.body.classList.add('border-radius-2');
    }
    
    // Apply sidebar width
    const sidebarWidth = localStorage.getItem('sidebarWidth');
    if (sidebarWidth) {
        // Remove any existing sidebar width class
        document.body.classList.remove('sidebar-narrow', 'sidebar-default', 'sidebar-wide');
        document.body.classList.add('sidebar-' + sidebarWidth);
        
        // Update select if on settings page
        const sidebarWidthSelect = document.getElementById('sidebarWidth');
        if (sidebarWidthSelect) {
            sidebarWidthSelect.value = sidebarWidth;
        }
    } else {
        // Default is default
        document.body.classList.add('sidebar-default');
    }
    
    // Apply icon size
    const iconSize = localStorage.getItem('iconSize');
    if (iconSize) {
        // Remove any existing icon size class
        document.body.classList.remove('icon-small', 'icon-normal', 'icon-large');
        document.body.classList.add('icon-' + iconSize);
        
        // Update radio button if on settings page
        const iconSizeRadio = document.getElementById('iconSize' + (iconSize.charAt(0).toUpperCase() + iconSize.slice(1)));
        if (iconSizeRadio) {
            iconSizeRadio.checked = true;
        }
    } else {
        // Default is normal
        document.body.classList.add('icon-normal');
    }
}

// Listen for system dark mode changes if in auto mode
if (localStorage.getItem('themeMode') === 'auto') {
    window.matchMedia('(prefers-color-scheme: dark)')
        .addEventListener('change', event => {
            if (event.matches) {
                document.documentElement.classList.add('dark-mode');
            } else {
                document.documentElement.classList.remove('dark-mode');
            }
        });
}

// Highlight active nav link based on current URL
function highlightActiveNavLink() {
    const currentUrl = window.location.pathname;
    document.querySelectorAll('.sidebar-link').forEach(link => {
        const href = link.getAttribute('href');
        if (href === currentUrl || 
            (currentUrl.includes(href) && href !== '/')) {
            link.classList.add('active');
        }
    });
}

// Initialize Charts
function initializeCharts() {
    // Project progress chart
    const progressChartEl = document.getElementById('projectProgressChart');
    if (progressChartEl) {
        new Chart(progressChartEl, {
            type: 'line',
            data: {
                labels: ['Ocak', 'Şubat', 'Mart', 'Nisan', 'Mayıs', 'Haziran'],
                datasets: [{
                    label: 'Tamamlanan Projeler',
                    data: [5, 10, 15, 12, 18, 22],
                    borderColor: getComputedStyle(document.documentElement).getPropertyValue('--primary-color').trim(),
                    backgroundColor: 'rgba(var(--primary-color-rgb), 0.1)',
                    borderWidth: 2,
                    tension: 0.4,
                    fill: true
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: false
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        grid: {
                            drawBorder: false
                        }
                    },
                    x: {
                        grid: {
                            display: false
                        }
                    }
                }
            }
        });
    }

    // Distribution chart
    const distributionChartEl = document.getElementById('projectDistributionChart');
    if (distributionChartEl) {
        new Chart(distributionChartEl, {
            type: 'doughnut',
            data: {
                labels: ['Bekleyen', 'Aktif', 'Tamamlanan', 'İptal'],
                datasets: [{
                    data: [12, 19, 8, 5],
                    backgroundColor: [
                        '#ffc107', // warning
                        getComputedStyle(document.documentElement).getPropertyValue('--primary-color').trim(),
                        '#198754', // success
                        '#dc3545'  // danger
                    ],
                    borderWidth: 0
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom'
                    }
                },
                cutout: '75%'
            }
        });
    }
}

// Custom form validation
function enableCustomFormValidation() {
    // Example of custom validation
    document.querySelectorAll('form').forEach(form => {
        form.addEventListener('submit', function(event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        }, false);
    });
}

// File input preview
function previewFile(input, previewElement) {
    if (input.files && input.files[0]) {
        var reader = new FileReader();
        reader.onload = function(e) {
            document.querySelector(previewElement).setAttribute('src', e.target.result);
        }
        reader.readAsDataURL(input.files[0]);
    }
}

// Confirmation dialog
function confirmAction(message, callback) {
    if (confirm(message)) {
        callback();
    }
}

// Format date for display
function formatDate(dateString) {
    const options = { year: 'numeric', month: 'long', day: 'numeric', hour: '2-digit', minute: '2-digit' };
    return new Date(dateString).toLocaleDateString('tr-TR', options);
}

// Save theme settings to localStorage
function saveThemeSettings(settings) {
    // Koyu tema ve yüksek kontrast kontrolü
    if (settings.themeMode === 'dark' && settings.highContrast === 'true') {
        // Koyu temada yüksek kontrast kullanılamaz, ayarı false yap
        settings.highContrast = 'false';
        
        // Ayarlar sayfasındaysa, checkbox'ı güncelle ve uyarı göster
        const highContrastSwitch = document.getElementById('highContrastSwitch');
        if (highContrastSwitch) {
            highContrastSwitch.checked = false;
            showThemeAlert('Koyu tema ile yüksek kontrast modu birlikte kullanılamaz.', 'warning');
        }
    }
    
    for (const key in settings) {
        localStorage.setItem(key, settings[key]);
    }
    
    applySavedThemeSettings();
    
    return true;
}

// Ayarlar sayfasında tema modu ve yüksek kontrast arasında kontrol ekleyen fonksiyon
document.addEventListener('DOMContentLoaded', function() {
    const darkModeRadio = document.getElementById('darkMode');
    const highContrastSwitch = document.getElementById('highContrastSwitch');
    
    // Hem radio button hem de switch varsa (ayarlar sayfasındayız)
    if (darkModeRadio && highContrastSwitch) {
        // Dark mode seçildiğinde high contrast'ı devre dışı bırak
        darkModeRadio.addEventListener('change', function() {
            if (this.checked) {
                if (highContrastSwitch.checked) {
                    highContrastSwitch.checked = false;
                    showThemeAlert('Koyu tema ile yüksek kontrast modu birlikte kullanılamaz.', 'warning');
                }
                highContrastSwitch.disabled = true;
            } else {
                highContrastSwitch.disabled = false;
            }
        });
        
        // Sayfa yüklendiğinde koyu tema seçiliyse high contrast'ı devre dışı bırak
        if (darkModeRadio.checked) {
            highContrastSwitch.disabled = true;
        }
        
        // High contrast açılırsa koyu temanın seçilememesini sağla
        highContrastSwitch.addEventListener('change', function() {
            if (this.checked && darkModeRadio.checked) {
                this.checked = false;
                showThemeAlert('Koyu tema ile yüksek kontrast modu birlikte kullanılamaz.', 'warning');
            }
        });
    }
});

// Uyarı mesajı gösterme fonksiyonu
function showThemeAlert(message, type = 'success') {
    // Toast yerine sayfanın üstünde bir alert göster
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type} alert-dismissible fade show`;
    alertDiv.innerHTML = `
        <i class="fas fa-info-circle me-2"></i> ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    `;
    
    // Sayfanın başına alert ekle
    const container = document.querySelector('.container-fluid');
    if (container) {
        container.insertBefore(alertDiv, container.firstChild);
        
        // 5 saniye sonra otomatik kapat
        setTimeout(() => {
            const bsAlert = new bootstrap.Alert(alertDiv);
            bsAlert.close();
        }, 5000);
    }
}

// Reset all theme settings
function resetThemeSettings() {
    localStorage.removeItem('themeMode');
    localStorage.removeItem('colorPalette');
    localStorage.removeItem('fontSize');
    localStorage.removeItem('fontFamily');
    localStorage.removeItem('compactLayout');
    localStorage.removeItem('animations');
    localStorage.removeItem('highContrast');
    localStorage.removeItem('borderRadius');
    localStorage.removeItem('sidebarWidth');
    localStorage.removeItem('iconSize');
    
    applySavedThemeSettings();
    
    return true;
}

// İkon Değiştirici
const projectIcons = [
    'fa-project-diagram',
    'fa-tasks',
    'fa-book',
    'fa-graduation-cap',
    'fa-clipboard-check',
    'fa-calendar-check',
    'fa-chart-bar',
    'fa-code',
    'fa-file-alt',
    'fa-users'
];

// İkonları değiştir
function cycleIcons() {
    const iconElement = document.getElementById('transitionIcon');
    if (!iconElement) return;
    
    // Mevcut class'ı bul ve kaldır
    const currentIconClass = Array.from(iconElement.classList).find(cls => cls.startsWith('fa-'));
    if (currentIconClass) {
        iconElement.classList.remove(currentIconClass);
    }
    
    // Rastgele yeni bir ikon seç
    const nextIconIndex = Math.floor(Math.random() * projectIcons.length);
    const nextIconClass = projectIcons[nextIconIndex];
    
    // Yeni ikonu ekle
    iconElement.classList.add(nextIconClass);
}

// Global değişkenler
let iconInterval;

// Sayfa Geçiş Yöneticisi
document.addEventListener('DOMContentLoaded', function() {
    // Sayfa yüklendiğinde geçiş ekranını kontrol et
    const pageTransition = document.getElementById('pageTransition');
    if (!pageTransition) return;
    
    // Sayfa geçiş durumunu kontrol et
    const isNavigating = sessionStorage.getItem('pageIsNavigating') === 'true';
    
    if (isNavigating) {
        // Sayfa geçişi tamamlandı, işareti temizle
        sessionStorage.removeItem('pageIsNavigating');
        
        // Geçiş animasyonunu hemen kapat (kopukluk olmaması için)
        pageTransition.classList.remove('show');
        document.body.classList.remove('page-transitioning');
        
        // Herhangi bir çalışan interval'ı temizle
        if (iconInterval) {
            clearInterval(iconInterval);
            iconInterval = null;
        }
    } else {
        // Normal sayfa yükleme (ilk giriş veya yenileme), animasyon gösterme
        pageTransition.style.display = 'none';
        setTimeout(() => {
            pageTransition.style.display = '';
            pageTransition.classList.remove('show'); // Animasyonu kesin olarak kapat
        }, 100);
    }
    
    // Tüm navigasyon linklerini dinle
    setupPageTransitions();
    
    // Sidebar animasyonunu başlat
    animateSidebar();
    
    // Geri tuşu sorunu için ekstra önlem (sayfanın tam yüklenmesini bekleyip animasyonu gizle)
    window.addEventListener('load', function() {
        // Gecikmeli olarak animasyonu gizle
        setTimeout(() => {
            hidePageTransition();
            sessionStorage.removeItem('pageIsNavigating');
        }, 100);
    });
});

// Sidebar animasyonu - yukarıdan aşağı sıralı canlandırma
function animateSidebar() {
    // Animasyonlar kapalıysa çalıştırma
    if (document.body.classList.contains('no-animations')) return;
    
    // Logo ve marka elementlerini net bir şekilde seç
    const logoContainer = document.querySelector('.sidebar .logo-container');
    const brandText = document.querySelector('.sidebar .brand-text');
    const sidebarHeader = document.querySelector('.sidebar-header');
    
    // Tüm sidebar öğelerini topla
    const sidebarItems = document.querySelectorAll('.sidebar-item');
    const sidebarHeaders = document.querySelectorAll('.sidebar-header');
    
    // Logo ve marka elementlerini kontrol et ve stillerini uygula
    if (logoContainer) {
        logoContainer.style.opacity = '0';
        logoContainer.style.transform = 'translateX(-20px)';
    }
    
    if (brandText) {
        brandText.style.opacity = '0';
        brandText.style.transform = 'translateX(-20px)';
    }
    
    // Animasyon sınıflarını temizle ve tekrar ekleyebilmek için
    sidebarItems.forEach(item => {
        item.style.opacity = '0';
        item.style.transform = 'translateX(-20px)';
    });
    
    sidebarHeaders.forEach(header => {
        if (header !== sidebarHeader) { // Logo içeren header'ı hariç tut
            header.style.opacity = '0';
            header.style.transform = 'translateX(-20px)';
        }
    });
    
    // Animasyon sırasını oluştur
    const animationSequence = [];
    
    // Önce logo
    if (logoContainer) animationSequence.push(logoContainer);
    
    // Sonra marka yazısı
    if (brandText) animationSequence.push(brandText);
    
    // Sonra tüm sidebar başlık ve öğeleri (DOM sırasına göre)
    document.querySelectorAll('.sidebar-nav .sidebar-header, .sidebar-nav .sidebar-item').forEach(el => {
        animationSequence.push(el);
    });
    
    // Animasyon zamanlaması
    let delay = 50;
    const increment = 60; // ms
    
    // Her element için animasyonu uygula
    animationSequence.forEach((item) => {
        setTimeout(() => {
            // Tüm elementler için aynı animasyon (soldan sağa)
            item.style.transition = 'opacity 0.5s ease, transform 0.5s ease';
            item.style.opacity = '1';
            item.style.transform = 'translateX(0)';
        }, delay);
        
        delay += increment;
    });
    
    // Animasyon tamamlandıktan sonra tüm elementlerin görünür olmasını garantile
    setTimeout(() => {
        // Logo ve marka yazısı için kesin görünürlük
        if (logoContainer) {
            logoContainer.style.opacity = '1';
            logoContainer.style.transform = 'none';
        }
        
        if (brandText) {
            brandText.style.opacity = '1';
            brandText.style.transform = 'none';
        }
        
        // Tüm sidebar öğeleri için kesin görünürlük
        document.querySelectorAll('.sidebar-item, .sidebar-header, .sidebar-brand, .logo-container, .brand-text').forEach(el => {
            el.style.opacity = '1';
            el.style.transform = 'none';
        });
    }, delay + 500); // Tüm animasyonlar tamamlandıktan 500ms sonra
}

// Sayfa geçişlerini ayarla
function setupPageTransitions() {
    // Navigasyon linklerini yakala (Sadece uygulama içi linkler)
    const links = document.querySelectorAll('a:not([href^="http"]):not([href^="#"]):not([href^="javascript"]):not([data-bs-toggle]):not([target])');
    
    // Her link için tıklama olayı ekle
    links.forEach(link => {
        link.addEventListener('click', function(e) {
            // Eğer metod belirtilmişse (POST formları gibi) veya animasyonlar kapalıysa, normal geçiş yap
            if (link.getAttribute('data-method') || 
                link.closest('form') || 
                document.body.classList.contains('no-animations')) {
                return;
            }
            
            // Ctrl veya Command ile tıklama - Yeni sekmede açma
            if (e.ctrlKey || e.metaKey) {
                return;
            }
            
            e.preventDefault();
            
            // Geçiş sayfasını göster
            showPageTransition();
            
            // Sayfa geçiş durumunu ayarla
            sessionStorage.setItem('pageIsNavigating', 'true');
            
            // Sayfayı yönlendir (gecikme ile)
            setTimeout(() => {
                window.location.href = link.href;
            }, 400); // 400ms sonra yönlendir
        });
    });
    
    // Form gönderim animasyonu
    const forms = document.querySelectorAll('form:not([data-no-transition])');
    forms.forEach(form => {
        form.addEventListener('submit', function(e) {
            // Animasyonlar kapalıysa veya AJAX formu ise normal gönder
            if (document.body.classList.contains('no-animations') || 
                form.getAttribute('data-ajax') === 'true') {
                return;
            }
            
            // Formun button elementini bul ve "loading" durumuna geçir
            const submitButtons = form.querySelectorAll('button[type="submit"], input[type="submit"]');
            submitButtons.forEach(button => {
                if (button.tagName === 'BUTTON') {
                    const originalText = button.innerHTML;
                    button.setAttribute('data-original-text', originalText);
                    button.innerHTML = '<span class="loading-spinner me-2" style="width: 16px; height: 16px;"></span> Yükleniyor...';
                }
                button.disabled = true;
            });
            
            // Sayfa geçiş durumunu ayarla
            sessionStorage.setItem('pageIsNavigating', 'true');
            
            // Geçiş animasyonunu göster
            showPageTransition();
        });
    });
    
    // Geri/ileri tuşu geçişleri
    window.addEventListener('popstate', function() {
        // Geri tuşu için özel bir geçici animasyon göster (çok kısa süreli)
        const pageTransition = document.getElementById('pageTransition');
        if (pageTransition) {
            // Interval'ı önceden temizle
            if (iconInterval) {
                clearInterval(iconInterval);
            }
            
            // Geçiş efektini göster
            document.body.classList.add('page-transitioning');
            pageTransition.classList.add('show');
            
            // Icon'u değiştir (tek seferlik)
            cycleIcons();
            
            // Çok kısa süre sonra (300ms) animasyonu gizle
            setTimeout(() => {
                hidePageTransition();
                // sessionStorage'ı temizle (geri tuşu için gerekli değil)
                sessionStorage.removeItem('pageIsNavigating');
            }, 300);
        }
    });
}

// Geçiş animasyonunu göster
function showPageTransition() {
    const pageTransition = document.getElementById('pageTransition');
    if (!pageTransition) return;
    
    // Var olan interval'ı temizle
    if (iconInterval) {
        clearInterval(iconInterval);
    }
    
    // Sayfaya geçiş sınıfı ekle (blur efekti için)
    document.body.classList.add('page-transitioning');
    pageTransition.classList.add('show');
    
    // İkon değiştirme interval'ı başlat
    iconInterval = setInterval(cycleIcons, 700); // Her 700ms'de bir ikon değiştir
    
    // Animasyonlar kapatılmışsa, geçiş ekranını 400ms sonra gizle
    if (document.body.classList.contains('no-animations')) {
        setTimeout(() => {
            pageTransition.classList.remove('show');
            if (iconInterval) {
                clearInterval(iconInterval);
                iconInterval = null;
            }
        }, 400);
    }
}

// Geçiş animasyonunu gizle
function hidePageTransition() {
    const pageTransition = document.getElementById('pageTransition');
    if (!pageTransition) return;
    
    pageTransition.classList.remove('show');
    setTimeout(() => {
        document.body.classList.remove('page-transitioning');
    }, 300);
    
    // İkon değiştirme interval'ını temizle
    if (iconInterval) {
        clearInterval(iconInterval);
        iconInterval = null;
    }
}
