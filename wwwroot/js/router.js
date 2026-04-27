// Простой SPA-роутер на основе хешей
const router = {
    routes: {},
    currentPage: null,

    /**
     * Зарегистрировать маршрут
     */
    register(hash, config) {
        this.routes[hash] = config;
    },

    /**
     * Загрузить страницу по текущему хешу
     */
    async navigate() {
        const hash = window.location.hash || '#login';
        const route = this.routes[hash];

        // Если маршрут не найден — на дашборд (если авторизован) или на логин
        if (!route) {
            if (api.isAuthenticated()) {
                window.location.hash = '#dashboard';
            } else {
                window.location.hash = '#login';
            }
            return;
        }

        // Проверка авторизации
        if (route.auth && !api.isAuthenticated()) {
            window.location.hash = '#login';
            return;
        }

        // Если уже на странице логина и авторизован — редирект на дашборд
        if (hash === '#login' && api.isAuthenticated()) {
            window.location.hash = '#dashboard';
            return;
        }

        // Обновляем активный пункт меню
        this.updateMenu(hash);

        // Показываем загрузку
        const content = document.getElementById('app-content');
        if (content) {
            content.innerHTML = '<div class="loading-spinner"><div class="spinner"></div></div>';
        }

        try {
            // Загружаем HTML страницы
            const response = await fetch(`pages/${route.page}`);
            const html = await response.text();

            if (content) {
                content.innerHTML = html;
            }

            // Загружаем CSS если есть
            if (route.css) {
                this.loadCSS(route.css);
            }

            // Загружаем JS и инициализируем
            if (route.js) {
                const script = document.createElement('script');
                script.src = `js/${route.js}`;
                script.onload = () => {
                    if (route.init && typeof window[route.init] === 'function') {
                        window[route.init]();
                    }
                };
                document.body.appendChild(script);
            }

            this.currentPage = hash;
        } catch (error) {
            console.error('Ошибка загрузки страницы:', error);
            if (content) {
                content.innerHTML = '<div class="error-state">Ошибка загрузки страницы</div>';
            }
        }
    },

    /**
     * Обновить активный пункт меню
     */
    updateMenu(hash) {
        document.querySelectorAll('.nav-item').forEach(item => {
            item.classList.remove('active');
            if (item.getAttribute('href') === hash) {
                item.classList.add('active');
            }
        });
    },

    /**
     * Загрузить CSS динамически
     */
    loadCSS(filename) {
        const id = `css-${filename.replace('.css', '')}`;
        if (document.getElementById(id)) return;

        const link = document.createElement('link');
        link.id = id;
        link.rel = 'stylesheet';
        link.href = `css/${filename}`;
        document.head.appendChild(link);
    },

    /**
     * Инициализация роутера
     */
    init() {
        // Регистрируем маршруты
        this.register('#login', {
            page: 'login.html',
            css: 'login.css',
            auth: false
        });

        this.register('#dashboard', {
            page: 'dashboard.html',
            css: 'dashboard.css',
            js: 'dashboard.js',
            init: 'initDashboard',
            auth: true
        });

        this.register('#employees', {
            page: 'employees.html',
            js: 'employees.js',
            init: 'initEmployees',
            auth: true
        });

        this.register('#tasks', {
            page: 'tasks.html',
            css: 'tasks.css',
            js: 'tasks.js',
            init: 'initTasks',
            auth: true
        });

        this.register('#schedule', {
            page: 'schedule.html',
            js: 'schedule.js',
            init: 'initSchedule',
            auth: true
        });

        this.register('#reports', {
            page: 'reports.html',
            js: 'reports.js',
            init: 'initReports',
            auth: true
        });

        // Слушаем изменения хеша
        window.addEventListener('hashchange', () => this.navigate());

        // Первая загрузка
        this.navigate();
    }
};