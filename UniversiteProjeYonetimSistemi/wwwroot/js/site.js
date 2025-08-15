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

    // Ana animasyon sistemini başlat
    initializePageAnimations();
    
    // Hover animasyonları için sınıflar ekleyelim
    applyHoverEffects();
    
    // Veri yükleme animasyonlarını otomatik ekleyelim
    convertLoadingElements();
    
    // Bildirim rozetlerini canlandıralım
    animateNotifications();
});

// =====================================================
// YENİ: KAPSAMLI SAYFA ANIMASYON SİSTEMİ
// =====================================================

// Ana animasyon sistemi - tüm sayfalarda çalışır (Enhanced Smooth tarz)
function initializePageAnimations() {
    // Animasyonlar kapalıysa hiçbir şey yapma
    if (document.body.classList.contains('no-animations')) {
        // Animasyonlar kapalıysa tüm elementleri görünür yap
        makeAllElementsVisible();
        return;
    }

    // Enhanced smooth staggered animasyonlar - daha güzel ve akıcı
    setTimeout(function() { $('.animate-header').addClass('animate-in'); }, 150);
    setTimeout(function() { $('.stat-card').addClass('animate-in'); }, 300);
    setTimeout(function() { $('.main-table').addClass('animate-in'); }, 300);
    setTimeout(function() { $('.animate-card').addClass('animate-in'); }, 450);
    setTimeout(function() { $('.animate-button').addClass('animate-in'); }, 600);
    setTimeout(function() { $('.animate-form').addClass('animate-in'); }, 750);
    setTimeout(function() { $('.animate-table').addClass('animate-in'); }, 900);
    setTimeout(function() { $('.table-hover-item').addClass('animate-in'); }, 1050);
    setTimeout(function() { $('.comment-item').addClass('animate-in'); }, 1200);
    setTimeout(function() { $('.meeting-item').addClass('animate-in'); }, 1200);
    setTimeout(function() { $('.animate-nav-tabs').addClass('animate-in'); }, 1350);
}

// Animasyonlar kapalıysa elementleri görünür yap
function makeAllElementsVisible() {
    const allElements = document.querySelectorAll('.page-header, .stat-card, .dashboard-card, .main-table, .card, form, .btn');
    allElements.forEach(element => {
        element.style.opacity = '1';
        element.style.transform = 'none';
    });
}

// Sayfa elementlerini kategorilere ayır ve animasyon sınıfları ekle
function categorizeAndAnimateElements() {
    // Animasyon sırasını belirlemek için tüm elementleri topla
    const animationSequence = [];
    
    // 1. Sayfa başlıkları (en üstte) - Sadece .page-header div'lerini seç - AkademisyenDashboard hariç tutuyoruz
    const pageHeaders = document.querySelectorAll('.page-header:not(.no-animation)');
    pageHeaders.forEach(header => {
        if (!header.classList.contains('animate-header')) {
            header.classList.add('animate-header');
            animationSequence.push({ element: header, type: 'header', delay: 0 });
        }
    });

    // 2. İstatistik kartları (dashboard)
    const statCards = document.querySelectorAll('.stat-card, .dashboard-card, .card.shadow-sm.h-100');
    statCards.forEach(card => {
        if (!card.classList.contains('animate-stat')) {
            card.classList.add('animate-stat');
            animationSequence.push({ element: card, type: 'stat', delay: 100 });
        }
    });

    // 3. Ana tablolar ve içerik kartları
    const mainTables = document.querySelectorAll('.main-table, .card.shadow-sm.mb-4, table.table, .table-responsive');
    mainTables.forEach(table => {
        if (!table.classList.contains('animate-table')) {
            table.classList.add('animate-table');
            animationSequence.push({ element: table, type: 'table', delay: 200 });
        }
    });

    // 4. Genel kartlar
    const cards = document.querySelectorAll('.card:not(.stat-card):not(.dashboard-card):not(.main-table)');
    cards.forEach(card => {
        if (!card.classList.contains('animate-card')) {
            card.classList.add('animate-card');
            animationSequence.push({ element: card, type: 'card', delay: 300 });
        }
    });

    // 5. Alt bölüm kartları (bottom-section)
    const bottomCards = document.querySelectorAll('.bottom-section .card');
    bottomCards.forEach(card => {
        if (!card.classList.contains('animate-card')) {
            card.classList.add('animate-card');
            animationSequence.push({ element: card, type: 'bottom-card', delay: 400 });
        }
    });

    // 6. Formlar
    const forms = document.querySelectorAll('form, .form-container');
    forms.forEach(form => {
        if (!form.classList.contains('animate-form')) {
            form.classList.add('animate-form');
            animationSequence.push({ element: form, type: 'form', delay: 350 });
        }
    });

    // 7. Butonlar (en son)
    const buttons = document.querySelectorAll('.btn:not(.animate-button), .details-btn');
    buttons.forEach(button => {
        if (!button.classList.contains('animate-button')) {
            button.classList.add('animate-button');
            animationSequence.push({ element: button, type: 'button', delay: 500 });
        }
    });

    // Tablo satırlarını ayrı olarak canlandır
    animateTableRows();
    
    // Liste öğelerini ayrı olarak canlandır
    animateListItems();

    return animationSequence;
}

// Sıralı animasyonları başlat
function startStaggeredAnimations() {
    // Ana elementler için delay'ler
    const delays = {
        header: 0,
        stat: 150,
        table: 300,
        card: 450,
        'bottom-card': 600,
        form: 400,
        button: 750
    };

    // Elementleri DOM sırasına göre topla ve animasyonlarını başlat
    setTimeout(() => {
        // Başlıklar - no-animation sınıfına sahip olanları hariç tut
        document.querySelectorAll('.animate-header:not(.no-animation)').forEach((element, index) => {
            // Ebeveyn elementi no-animation sınıfına sahipse bu elementi atla
            if (element.closest('.no-animation')) return;
            
            setTimeout(() => {
                element.classList.add('animate-in');
            }, delays.header + (index * 100));
        });
    }, 50);

    setTimeout(() => {
        // İstatistik kartları
        document.querySelectorAll('.animate-stat').forEach((element, index) => {
            setTimeout(() => {
                element.classList.add('animate-in');
            }, delays.stat + (index * 80));
        });
    }, 100);

    setTimeout(() => {
        // Tablolar
        document.querySelectorAll('.animate-table').forEach((element, index) => {
            setTimeout(() => {
                element.classList.add('animate-in');
            }, delays.table + (index * 120));
        });
    }, 150);

    setTimeout(() => {
        // Normal kartlar
        document.querySelectorAll('.animate-card').forEach((element, index) => {
            setTimeout(() => {
                element.classList.add('animate-in');
            }, delays.card + (index * 100));
        });
    }, 200);

    setTimeout(() => {
        // Formlar
        document.querySelectorAll('.animate-form').forEach((element, index) => {
            setTimeout(() => {
                element.classList.add('animate-in');
            }, delays.form + (index * 90));
        });
    }, 250);

    setTimeout(() => {
        // Butonlar
        document.querySelectorAll('.animate-button').forEach((element, index) => {
            setTimeout(() => {
                element.classList.add('animate-in');
            }, delays.button + (index * 50));
        });
    }, 300);
}

// Tablo satırlarını canlandır
function animateTableRows() {
    const tables = document.querySelectorAll('table.table:not(.no-animation) tbody');
    
    tables.forEach(tbody => {
        const rows = tbody.querySelectorAll('tr');
        rows.forEach((row, index) => {
            row.classList.add('animate-table-row');
            setTimeout(() => {
                row.classList.add('animate-in');
            }, 800 + (index * 50)); // Tablolar canlandıktan sonra satırlar
        });
    });
}

// Liste öğelerini canlandır
function animateListItems() {
    const listContainers = document.querySelectorAll('.list-group, .nav-pills, .nav-tabs');
    
    listContainers.forEach(container => {
        const items = container.querySelectorAll('.list-group-item, .nav-item, .nav-link');
        items.forEach((item, index) => {
            item.classList.add('animate-list-item');
            setTimeout(() => {
                item.classList.add('animate-in');
            }, 600 + (index * 40));
        });
    });
}

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
    // Bu fonksiyon artık initializePageAnimations içinde entegre edildi
    // Eski implementasyon yerine yeni sistem kullanılıyor
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

// Bildirim rozetlerini canlandır
function animateNotifications() {
    const badges = document.querySelectorAll('.badge.bg-danger');
    
    badges.forEach(badge => {
        if (!badge.classList.contains('no-animation')) {
            badge.classList.add('notification-badge');
        }
    });
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

// Dashboard specific animations
document.addEventListener('DOMContentLoaded', function() {
    // Count-up animasyonu için fonksiyon
    function animateCounter(element) {
        const target = parseInt(element.getAttribute('data-count'));
        if (isNaN(target) || target === 0) return;
        
        const duration = 2000; // 2 saniye
        const increment = target / (duration / 16); // 60fps
        let current = 0;
        
        const timer = setInterval(() => {
            current += increment;
            if (current >= target) {
                current = target;
                clearInterval(timer);
            }
            element.textContent = Math.floor(current);
        }, 16);
    }
    
    // Intersection Observer ile görünür olduğunda animasyonu başlat
    const observerOptions = {
        threshold: 0.5,
        rootMargin: '0px 0px -100px 0px'
    };
    
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const counters = entry.target.querySelectorAll('.stat-number[data-count]');
                counters.forEach(counter => {
                    if (!counter.classList.contains('animated')) {
                        animateCounter(counter);
                        counter.classList.add('animated');
                    }
                });
            }
        });
    }, observerOptions);
    
    // İstatistik kartlarını gözlemle
    document.querySelectorAll('.stat-card').forEach(card => {
        observer.observe(card);
    });
    
    // Detay butonları için ripple efekti
    document.querySelectorAll('.details-btn').forEach(btn => {
        btn.addEventListener('click', function(e) {
            // Animasyon kapalıysa ripple efekti yapma
            if (document.body.classList.contains('no-animations')) return;
            
            const ripple = document.createElement('span');
            const size = Math.max(this.offsetWidth, this.offsetHeight);
            const rect = this.getBoundingClientRect();
            
            ripple.style.width = ripple.style.height = size + 'px';
            ripple.style.left = (e.clientX - rect.left - size / 2) + 'px';
            ripple.style.top = (e.clientY - rect.top - size / 2) + 'px';
            ripple.style.position = 'absolute';
            ripple.style.borderRadius = '50%';
            ripple.style.backgroundColor = 'rgba(255,255,255,0.6)';
            ripple.style.transform = 'scale(0)';
            ripple.style.animation = 'ripple-effect 0.6s linear';
            ripple.style.pointerEvents = 'none';
            
            this.appendChild(ripple);
            
            setTimeout(() => {
                ripple.remove();
            }, 600);
        });
    });
    
    // Dashboard'da enhanced hover efektleri
    initializeDashboardHoverEffects();
});

// Dashboard hover efektlerini başlat
function initializeDashboardHoverEffects() {
    // Project row hover efektleri
    document.querySelectorAll('.project-row').forEach(row => {
        row.addEventListener('mouseenter', function() {
            if (!document.body.classList.contains('no-animations')) {
                this.style.backgroundColor = 'rgba(var(--primary-color-rgb), 0.02)';
            }
        });
        
        row.addEventListener('mouseleave', function() {
            this.style.backgroundColor = '';
        });
    });
    
    // Comment ve meeting item hover efektleri - tam olarak tablo satırları gibi
    document.querySelectorAll('.comment-item, .meeting-item').forEach(item => {
        item.addEventListener('mouseenter', function() {
            if (!document.body.classList.contains('no-animations')) {
                this.style.backgroundColor = 'var(--table-hover-bg)';
                this.style.transform = 'translateX(3px)';
            }
        });
        
        item.addEventListener('mouseleave', function() {
            this.style.backgroundColor = '';
            this.style.transform = '';
        });
    });
    
    // Stat icon pulse efekti (hover üzerine)
    document.querySelectorAll('.stat-card').forEach(card => {
        const icon = card.querySelector('.stat-icon');
        if (icon) {
                    card.addEventListener('mouseenter', function() {
            if (!document.body.classList.contains('no-animations')) {
                // icon.style.animation = 'pulse 2s infinite'; - KALDIRILDI
            }
        });
            
            card.addEventListener('mouseleave', function() {
                // icon.style.animation = ''; - KALDIRILDI
            });
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
        rootElement.classList.add('dark-theme');
        
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
        rootElement.classList.remove('dark-theme');
        
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
            rootElement.classList.add('dark-theme');
            
            // Sistem tercihi koyu temaysa yüksek kontrast devre dışı olmalı
            const highContrastSwitch = document.getElementById('highContrastSwitch');
            if (highContrastSwitch) {
                highContrastSwitch.disabled = true;
                highContrastSwitch.checked = false;
                localStorage.removeItem('highContrast');
            }
        } else {
            rootElement.classList.remove('dark-mode');
            rootElement.classList.remove('dark-theme');
            
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
                document.documentElement.classList.add('dark-theme');
            } else {
                document.documentElement.classList.remove('dark-mode');
                document.documentElement.classList.remove('dark-theme');
            }
        });
}

// Highlight active nav link based on current URL
function highlightActiveNavLink() {
    const currentUrl = window.location.pathname.toLowerCase();
    let bestMatch = null;

    document.querySelectorAll('.sidebar-link').forEach(link => {
        const href = link.getAttribute('href');
        if (!href) return;
        
        const normalizedHref = href.toLowerCase();
        
        // Ana sayfa (/) icin ozel kontrol - sadece tam eslesme
        if (normalizedHref === '/' || normalizedHref === '/home') {
            if (currentUrl === '/' || currentUrl === '/home' || currentUrl === '/home/index') {
                if (!bestMatch || normalizedHref.length > bestMatch.getAttribute('href').length) {
                    bestMatch = link;
                }
            }
        }
        // Diger linkler icin startsWith kullan ama "/" hariç
        else if (normalizedHref !== '/' && currentUrl.startsWith(normalizedHref)) {
            if (!bestMatch || normalizedHref.length > bestMatch.getAttribute('href').length) {
                bestMatch = link;
            }
        }
    });

    if (bestMatch) {
        bestMatch.classList.add('active');
    }
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
        // Geri tuşu için özel bir geçici animasyon göster
        const pageTransition = document.getElementById('pageTransition');
        if (pageTransition) {
            // Önceki timeout'ları temizle
            if (window.backButtonTimeout) {
                clearTimeout(window.backButtonTimeout);
            }
            
            // Interval'ı önceden temizle
            if (iconInterval) {
                clearInterval(iconInterval);
            }
            
            // Geçiş efektini göster
            document.body.classList.add('page-transitioning');
            pageTransition.classList.add('show');
            
            // Icon'u değiştir (tek seferlik)
            cycleIcons();
            
            // İkon değiştirme interval'ı başlat (geri tuşu için kısa süreli)
            iconInterval = setInterval(cycleIcons, 500);
            
            // 1 saniye sonra animasyonu gizle (kullanıcının isteği)
            window.backButtonTimeout = setTimeout(() => {
                hidePageTransition();
                // sessionStorage'ı temizle (geri tuşu için gerekli değil)
                sessionStorage.removeItem('pageIsNavigating');
                
                // Interval'ı temizle
                if (iconInterval) {
                    clearInterval(iconInterval);
                    iconInterval = null;
                }
            }, 1000); // 1 saniye = 1000ms
        }
    });
}

// Geçiş animasyonunu göster
function showPageTransition() {
    const pageTransition = document.getElementById('pageTransition');
    if (!pageTransition) return;
    
    // forceHidePageTransition'in verdigi inline gizleme stillerini kaldir
    pageTransition.style.opacity = '';
    pageTransition.style.visibility = '';
    pageTransition.style.display = '';
    
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

// Geri donuslerde veya bfcache restorasyonunda animasyonu aninda kapatmak icin guclu kapatma
function forceHidePageTransition() {
    try {
        const pageTransition = document.getElementById('pageTransition');
        if (pageTransition) {
            pageTransition.classList.remove('show');
            // Ekran artefakti kalmasin diye style ile de gizleyelim (sonra tekrar acilabilir)
            pageTransition.style.opacity = '0';
            pageTransition.style.visibility = 'hidden';
        }
        document.body.classList.remove('page-transitioning');
        if (iconInterval) {
            clearInterval(iconInterval);
            iconInterval = null;
        }
        if (window.backButtonTimeout) {
            clearTimeout(window.backButtonTimeout);
            window.backButtonTimeout = null;
        }
        // Navigasyon bayragini temizle (geri donuste overlay kalmasin)
        sessionStorage.removeItem('pageIsNavigating');
    } catch (_) { /* no-op */ }
}

// bfcache'den geri gelindiginde sayfa blur kalmasin
window.addEventListener('pageshow', function() {
    forceHidePageTransition();
});

// Sekme tekrar gorunur hale geldiginde de emniyet icin kapat
document.addEventListener('visibilitychange', function() {
    if (document.visibilityState === 'visible') {
        forceHidePageTransition();
    }
});
