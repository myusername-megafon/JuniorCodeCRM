// Модуль авторизации
const auth = {
    /**
     * Попытка входа
     */
    async login(login, password) {
        try {
            const response = await api.post('/auth/login', { login, password });

            if (response.success) {
                localStorage.setItem(CONFIG.TOKEN_KEY, response.token);
                localStorage.setItem(CONFIG.TOKEN_EXPIRY_KEY, response.expiresAt);
                return { success: true };
            }

            return { success: false, message: response.message };
        } catch (error) {
            return { success: false, message: 'Ошибка соединения с сервером' };
        }
    },

    /**
     * Выход
     */
    logout() {
        localStorage.removeItem(CONFIG.TOKEN_KEY);
        localStorage.removeItem(CONFIG.TOKEN_EXPIRY_KEY);
        window.location.hash = '#login';
    },

    /**
     * Проверка авторизации (для guard)
     */
    checkAuth() {
        if (!api.isAuthenticated()) {
            window.location.hash = '#login';
            return false;
        }
        return true;
    }
};