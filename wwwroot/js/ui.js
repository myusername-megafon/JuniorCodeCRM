// UI-утилиты: модальные окна, уведомления, пагинация, таблицы
const ui = {
    // ==================== УВЕДОМЛЕНИЯ ====================

    /**
     * Показать уведомление
     * @param {string} message - текст
     * @param {'success'|'error'|'warning'|'info'} type - тип
     * @param {number} duration - длительность в мс (0 = пока не закроют)
     */
    showNotification(message, type = 'info', duration = 4000) {
        const container = document.getElementById('notifications');
        if (!container) return;

        const icons = {
            success: 'check_circle',
            error: 'error',
            warning: 'warning',
            info: 'info'
        };

        const notification = document.createElement('div');
        notification.className = `notification ${type} fade-in`;
        notification.innerHTML = `
            <span class="material-icons-outlined">${icons[type]}</span>
            <span>${message}</span>
            <button class="notification-close" onclick="this.parentElement.remove()">
                <span class="material-icons-outlined" style="font-size:1rem">close</span>
            </button>
        `;

        container.appendChild(notification);

        if (duration > 0) {
            setTimeout(() => {
                notification.classList.add('removing');
                setTimeout(() => notification.remove(), 300);
            }, duration);
        }
    },

    // ==================== МОДАЛЬНЫЕ ОКНА ====================

    /**
     * Открыть модальное окно
     * @param {string} title - заголовок
     * @param {string} bodyHTML - содержимое
     * @param {Array} buttons - массив кнопок [{text, class, callback}]
     */
    openModal(title, bodyHTML, buttons = []) {
        const overlay = document.getElementById('modal-overlay');
        if (!overlay) return;

        const defaultButtons = buttons.length > 0 ? buttons : [
            { text: 'Закрыть', class: 'btn-outline', callback: () => this.closeModal() }
        ];

        const buttonsHTML = defaultButtons.map(btn => {
            const btnClass = btn.class || 'btn-primary';
            return `<button class="btn ${btnClass}" data-action="${btn.text}">${btn.text}</button>`;
        }).join('');

        overlay.innerHTML = `
            <div class="modal">
                <div class="modal-header">
                    <h3 class="modal-title">${title}</h3>
                    <button class="modal-close" onclick="ui.closeModal()">
                        <span class="material-icons-outlined">close</span>
                    </button>
                </div>
                <div class="modal-body">${bodyHTML}</div>
                <div class="modal-footer">${buttonsHTML}</div>
            </div>
        `;

        overlay.classList.add('active');
        document.body.style.overflow = 'hidden';

        // Привязываем обработчики кнопок
        defaultButtons.forEach(btn => {
            const el = overlay.querySelector(`[data-action="${btn.text}"]`);
            if (el && btn.callback) {
                el.addEventListener('click', (e) => {
                    e.preventDefault();
                    btn.callback();
                });
            }
        });

        // Закрытие по клику на оверлей
        overlay.addEventListener('click', (e) => {
            if (e.target === overlay) this.closeModal();
        });

        // Закрытие по Escape
        const escHandler = (e) => {
            if (e.key === 'Escape') {
                this.closeModal();
                document.removeEventListener('keydown', escHandler);
            }
        };
        document.addEventListener('keydown', escHandler);
    },

    /**
     * Закрыть модальное окно
     */
    closeModal() {
        const overlay = document.getElementById('modal-overlay');
        if (!overlay) return;
        overlay.classList.remove('active');
        overlay.innerHTML = '';
        document.body.style.overflow = '';
    },

    /**
     * Модальное окно подтверждения
     */
    confirmDialog(title, message, onConfirm, onCancel) {
        this.openModal(title, `<p>${message}</p>`, [
            {
                text: 'Отмена',
                class: 'btn-outline',
                callback: () => {
                    this.closeModal();
                    if (onCancel) onCancel();
                }
            },
            {
                text: 'Подтвердить',
                class: 'btn-danger',
                callback: () => {
                    this.closeModal();
                    if (onConfirm) onConfirm();
                }
            }
        ]);
    },

    // ==================== ПАГИНАЦИЯ ====================

    /**
     * Создать пагинацию
     */
    renderPagination(container, currentPage, totalPages, onPageChange) {
        if (totalPages <= 1) {
            container.innerHTML = '';
            return;
        }

        let html = '<div class="pagination">';
        html += `<span class="pagination-info">Страница ${currentPage} из ${totalPages}</span>`;
        html += '<div class="pagination-buttons">';

        // Кнопка "Назад"
        html += `<button class="pagination-btn" ${currentPage === 1 ? 'disabled' : ''} 
            onclick="arguments[0].stopPropagation();" data-page="${currentPage - 1}">
            <span class="material-icons-outlined" style="font-size:1rem">chevron_left</span>
        </button>`;

        // Номера страниц
        const maxButtons = 7;
        let startPage = Math.max(1, currentPage - Math.floor(maxButtons / 2));
        let endPage = Math.min(totalPages, startPage + maxButtons - 1);
        if (endPage - startPage < maxButtons - 1) {
            startPage = Math.max(1, endPage - maxButtons + 1);
        }

        if (startPage > 1) {
            html += `<button class="pagination-btn" data-page="1">1</button>`;
            if (startPage > 2) html += '<span class="pagination-btn" disabled>...</span>';
        }

        for (let i = startPage; i <= endPage; i++) {
            html += `<button class="pagination-btn ${i === currentPage ? 'active' : ''}" 
                data-page="${i}">${i}</button>`;
        }

        if (endPage < totalPages) {
            if (endPage < totalPages - 1) html += '<span class="pagination-btn" disabled>...</span>';
            html += `<button class="pagination-btn" data-page="${totalPages}">${totalPages}</button>`;
        }

        // Кнопка "Вперёд"
        html += `<button class="pagination-btn" ${currentPage === totalPages ? 'disabled' : ''} 
            data-page="${currentPage + 1}">
            <span class="material-icons-outlined" style="font-size:1rem">chevron_right</span>
        </button>`;

        html += '</div></div>';
        container.innerHTML = html;

        // Обработчики
        container.querySelectorAll('.pagination-btn[data-page]').forEach(btn => {
            btn.addEventListener('click', () => {
                const page = parseInt(btn.getAttribute('data-page'));
                if (page && page !== currentPage) {
                    onPageChange(page);
                }
            });
        });
    },

    // ==================== ТАБЛИЦЫ ====================

    /**
     * Создать data-таблицу с сортировкой
     */
    createDataTable(container, columns, data, options = {}) {
        const {
            onRowClick,
            rowActions,
            sortable = true,
            emptyMessage = 'Нет данных'
        } = options;

        if (!data || data.length === 0) {
            container.innerHTML = `
                <div class="empty-state">
                    <span class="material-icons-outlined">inbox</span>
                    <h3>${emptyMessage}</h3>
                </div>`;
            return;
        }

        let html = '<div class="table-container"><table><thead><tr>';

        columns.forEach(col => {
            html += `<th data-key="${col.key}" class="${col.sortable !== false && sortable ? '' : 'no-sort'}">
                ${col.title}
                ${col.sortable !== false && sortable ? '<span class="material-icons-outlined sort-icon" style="font-size:1rem">unfold_more</span>' : ''}
            </th>`;
        });

        if (rowActions) {
            html += '<th style="width:60px"></th>';
        }

        html += '</tr></thead><tbody>';

        data.forEach((row, index) => {
            html += '<tr>';
            columns.forEach(col => {
                const value = col.render ? col.render(row, index) : (row[col.key] ?? '');
                html += `<td>${value}</td>`;
            });

            if (rowActions) {
                html += '<td><div class="row-actions">';
                rowActions.forEach(action => {
                    html += `<button class="icon-btn row-action-btn" title="${action.title}" 
                        data-action="${action.key}" data-id="${row[action.idKey || 'id']}">
                        <span class="material-icons-outlined">${action.icon}</span>
                    </button>`;
                });
                html += '</div></td>';
            }

            html += '</tr>';
        });

        html += '</tbody></table></div>';
        container.innerHTML = html;

        // Обработчики кликов по строкам
        if (onRowClick) {
            container.querySelectorAll('tbody tr').forEach((tr, index) => {
                tr.style.cursor = 'pointer';
                tr.addEventListener('click', (e) => {
                    if (!e.target.closest('.row-action-btn')) {
                        onRowClick(data[index], index);
                    }
                });
            });
        }

        // Сортировка
        if (sortable) {
            container.querySelectorAll('th:not(.no-sort)').forEach(th => {
                th.addEventListener('click', () => {
                    const key = th.getAttribute('data-key');
                    const currentDir = th.getAttribute('data-sort');
                    const newDir = currentDir === 'asc' ? 'desc' : 'asc';

                    // Сброс всех заголовков
                    container.querySelectorAll('th').forEach(h => {
                        h.classList.remove('sorted');
                        h.removeAttribute('data-sort');
                    });

                    th.classList.add('sorted');
                    th.setAttribute('data-sort', newDir);

                    const icon = th.querySelector('.sort-icon');
                    if (icon) {
                        icon.textContent = newDir === 'asc' ? 'expand_less' : 'expand_more';
                    }

                    const sorted = [...data].sort((a, b) => {
                        const aVal = a[key] ?? '';
                        const bVal = b[key] ?? '';
                        if (typeof aVal === 'string') {
                            return newDir === 'asc'
                                ? aVal.localeCompare(bVal)
                                : bVal.localeCompare(aVal);
                        }
                        return newDir === 'asc' ? aVal - bVal : bVal - aVal;
                    });

                    this.createDataTable(container, columns, sorted, options);
                });
            });
        }
    },

    // ==================== ФОРМАТИРОВАНИЕ ====================

    formatDate(dateString) {
        if (!dateString) return '—';
        const date = new Date(dateString);
        return date.toLocaleDateString('ru-RU', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric'
        });
    },

    formatDateTime(dateString) {
        if (!dateString) return '—';
        const date = new Date(dateString);
        return date.toLocaleDateString('ru-RU', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    },

    formatPhone(phone) {
        if (!phone) return '—';
        return phone;
    },

    /**
     * Получить класс чипса для статуса
     */
    getStatusChip(statusName) {
        const map = {
            'В работе': 'chip-info',
            'Выполнено': 'chip-success',
            'Просрочено': 'chip-danger',
            'Возвращено на доработку': 'chip-warning'
        };
        return map[statusName] || 'chip-primary';
    },

    /**
     * Получить класс чипса для приоритета
     */
    getPriorityChip(priorityName) {
        const map = {
            'Низкий': 'chip-success',
            'Средний': 'chip-warning',
            'Высокий': 'chip-danger'
        };
        return map[priorityName] || 'chip-primary';
    }
};