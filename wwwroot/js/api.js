// Универсальный API-клиент
const api = {
    /**
     * Получить токен из localStorage
     */
    getToken() {
        return localStorage.getItem(CONFIG.TOKEN_KEY);
    },

    /**
     * Проверить, авторизован ли пользователь
     */
    isAuthenticated() {
        const token = this.getToken();
        const expiry = localStorage.getItem(CONFIG.TOKEN_EXPIRY_KEY);
        if (!token || !expiry) return false;
        return new Date(expiry) > new Date();
    },

    /**
     * Базовый fetch-запрос
     */
    async request(endpoint, options = {}) {
        const url = `${CONFIG.API_BASE_URL}${endpoint}`;
        const token = this.getToken();

        const defaultHeaders = {
            'Content-Type': 'application/json'
        };

        if (token && this.isAuthenticated()) {
            defaultHeaders['Authorization'] = `Bearer ${token}`;
        }

        const config = {
            ...options,
            headers: {
                ...defaultHeaders,
                ...options.headers
            }
        };

        try {
            const response = await fetch(url, config);

            // Если 401 — разлогиниваем
            if (response.status === 401) {
                localStorage.removeItem(CONFIG.TOKEN_KEY);
                localStorage.removeItem(CONFIG.TOKEN_EXPIRY_KEY);
                window.location.hash = '#login';
                throw new Error('Сессия истекла');
            }

            // Если 503 — нет соединения
            if (response.status === 503) {
                ui.showNotification('Нет соединения с сервером', 'error');
                throw new Error('Нет соединения с сервером');
            }

            // Для экспорта возвращаем blob
            const contentType = response.headers.get('content-type');
            if (contentType && contentType.includes('application/vnd.openxmlformats') ||
                contentType && contentType.includes('application/pdf')) {
                return {
                    blob: await response.blob(),
                    fileName: this.getFileNameFromResponse(response)
                };
            }

            const data = await response.json();

            if (!response.ok) {
                throw new Error(data.error || 'Ошибка запроса');
            }

            return data;
        } catch (error) {
            if (error.message !== 'Сессия истекла' && error.message !== 'Нет соединения с сервером') {
                console.error('API Error:', error);
            }
            throw error;
        }
    },

    /**
     * GET-запрос
     */
    async get(endpoint, params = {}) {
        const query = new URLSearchParams();
        Object.keys(params).forEach(key => {
            if (params[key] !== null && params[key] !== undefined && params[key] !== '') {
                query.append(key, params[key]);
            }
        });

        const queryString = query.toString();
        const url = queryString ? `${endpoint}?${queryString}` : endpoint;

        return this.request(url, { method: 'GET' });
    },

    /**
     * POST-запрос
     */
    async post(endpoint, data = {}) {
        return this.request(endpoint, {
            method: 'POST',
            body: JSON.stringify(data)
        });
    },

    /**
     * PUT-запрос
     */
    async put(endpoint, data = {}) {
        return this.request(endpoint, {
            method: 'PUT',
            body: JSON.stringify(data)
        });
    },

    /**
     * PATCH-запрос
     */
    async patch(endpoint, data = {}) {
        return this.request(endpoint, {
            method: 'PATCH',
            body: JSON.stringify(data)
        });
    },

    /**
     * DELETE-запрос
     */
    async delete(endpoint) {
        return this.request(endpoint, { method: 'DELETE' });
    },

    /**
     * Скачать файл (XLSX/PDF)
     */
    async download(endpoint, format = 'XLSX') {
        const result = await this.get(endpoint, { format });
        if (result.blob) {
            const url = window.URL.createObjectURL(result.blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = result.fileName || 'export';
            document.body.appendChild(a);
            a.click();
            a.remove();
            window.URL.revokeObjectURL(url);
        }
    },

    /**
     * Извлечь имя файла из заголовков ответа
     */
    getFileNameFromResponse(response) {
        const disposition = response.headers.get('content-disposition');
        if (disposition) {
            const match = disposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/);
            if (match) return match[1].replace(/['"]/g, '');
        }
        return `export_${Date.now()}`;
    }
};