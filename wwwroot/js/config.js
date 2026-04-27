// Конфигурация приложения
const CONFIG = {
    // Базовый URL API (менять при деплое)
    API_BASE_URL: window.location.origin + '/api',

    // Настройки приложения
    APP_NAME: 'АРМ Директора',
    APP_SUBTITLE: 'Юниоркод Брянск',

    // Токен (ключ в localStorage)
    TOKEN_KEY: 'juniorkod_token',
    TOKEN_EXPIRY_KEY: 'juniorkod_token_expiry',

    // Настройки пагинации
    DEFAULT_PAGE_SIZE: 20,

    // Цвета
    COLORS: {
        primary: '#0a153a',
        secondary: '#1a2555',
        accent: '#83d4fe',
        success: '#4CAF50',
        warning: '#FF9800',
        danger: '#F44336',
        info: '#2196F3',
        bg: '#f0f2f5',
        surface: '#ffffff',
        text: '#1a1a1a',
        textSecondary: '#666666',
        border: '#e0e0e0'
    }
};