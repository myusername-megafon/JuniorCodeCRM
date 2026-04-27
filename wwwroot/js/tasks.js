// Доска поручений (Trello-like)
let tasksState = {
    columns: [
        { id: 'В работе', title: 'В работе', color: '#2196F3', statusID: 1 },
        { id: 'На доработке', title: 'На доработке', color: '#FF9800', statusID: 4 },
        { id: 'Готово', title: 'Готово', color: '#4CAF50', statusID: 2 },
        { id: 'Просрочено', title: 'Просрочено', color: '#F44336', statusID: 3 }
    ],
    allTasks: [],
    assignees: []
};

let draggedCard = null;
let contextMenu = null;

async function initTasks() {
    if (!auth.checkAuth()) return;

    await loadAssignees();
    await loadTasks();

    document.getElementById('btn-add-task').addEventListener('click', () => openTaskModal());
    document.getElementById('btn-refresh-tasks').addEventListener('click', () => loadTasks());
    document.getElementById('search-tasks').addEventListener('input', debounce(() => {
        filterTasks();
    }, 300));
    document.getElementById('filter-assignee').addEventListener('change', filterTasks);
    document.getElementById('filter-priority').addEventListener('change', filterTasks);

    // Закрытие контекстного меню
    document.addEventListener('click', (e) => {
        if (contextMenu && !e.target.closest('.task-context-menu')) {
            closeContextMenu();
        }
    });
}

async function loadAssignees() {
    try {
        const employees = await api.get('/employees', { isActive: 'true', pageSize: 100 });
        tasksState.assignees = employees;
        const select = document.getElementById('filter-assignee');
        employees.forEach(e => {
            const option = document.createElement('option');
            option.value = e.employeeID;
            option.textContent = e.fullName;
            select.appendChild(option);
        });
    } catch (e) { }
}

async function loadTasks() {
    const container = document.getElementById('board-container');
    container.innerHTML = '<div class="loading-spinner"><div class="spinner"></div></div>';

    try {
        const tasks = await api.get('/tasks', { pageSize: 200 });
        tasksState.allTasks = tasks;
        renderBoard(tasks);
    } catch (error) {
        container.innerHTML = '<div class="error-state">Ошибка загрузки поручений</div>';
    }
}

function filterTasks() {
    const search = document.getElementById('search-tasks').value.toLowerCase();
    const assigneeID = document.getElementById('filter-assignee').value;
    const priorityID = document.getElementById('filter-priority').value;

    let filtered = [...tasksState.allTasks];

    if (search) {
        filtered = filtered.filter(t =>
            t.title.toLowerCase().includes(search) ||
            (t.description && t.description.toLowerCase().includes(search))
        );
    }
    if (assigneeID) {
        filtered = filtered.filter(t => t.assigneeID == assigneeID);
    }
    if (priorityID) {
        filtered = filtered.filter(t => t.priorityID == priorityID);
    }

    renderBoard(filtered);
}

function renderBoard(tasks) {
    const container = document.getElementById('board-container');

    container.innerHTML = tasksState.columns.map(col => {
        const columnTasks = tasks.filter(t => t.boardColumn === col.id);

        return `
            <div class="board-column" data-column="${col.id}">
                <div class="board-column-header">
                    <div class="board-column-title">
                        <span class="column-dot" style="background: ${col.color}"></span>
                        ${col.title}
                    </div>
                    <span class="column-count">${columnTasks.length}</span>
                </div>
                <div class="board-column-body" 
                     data-column="${col.id}"
                     ondragover="handleDragOver(event)"
                     ondragleave="handleDragLeave(event)"
                     ondrop="handleDrop(event)">
                    ${columnTasks.length === 0 ? `
                        <div class="board-column-empty">
                            <span class="material-icons-outlined">inbox</span>
                            <span>Нет поручений</span>
                        </div>
                    ` : ''}
                    ${columnTasks.map(task => renderTaskCard(task)).join('')}
                </div>
                <button class="board-add-btn" onclick="openTaskModal('${col.id}')">
                    <span class="material-icons-outlined">add</span>
                    Добавить
                </button>
            </div>
        `;
    }).join('');

    // Делаем карточки перетаскиваемыми
    container.querySelectorAll('.task-card').forEach(card => {
        card.setAttribute('draggable', 'true');
        card.addEventListener('dragstart', handleDragStart);
        card.addEventListener('dragend', handleDragEnd);
    });
}

function renderTaskCard(task) {
    const assignee = tasksState.assignees.find(a => a.employeeID === task.assigneeID);
    const initials = assignee ? getInitials(assignee.fullName) : '?';
    const avatarColor = assignee ? stringToColor(assignee.fullName) : '#999';

    // Определяем статус дедлайна
    let deadlineClass = 'normal';
    if (task.isOverdue) {
        deadlineClass = 'overdue';
    } else if (task.deadline) {
        const daysLeft = Math.ceil((new Date(task.deadline) - new Date()) / (1000 * 60 * 60 * 24));
        if (daysLeft <= 3) deadlineClass = 'near';
    }

    return `
        <div class="task-card" 
             data-task-id="${task.taskID}" 
             data-column="${task.boardColumn}"
             draggable="true">
            <div class="task-card-header">
                <span class="task-card-title" onclick="openTaskModal(null, ${task.taskID})">${escapeHtml(task.title)}</span>
                <button class="task-card-menu" onclick="event.stopPropagation(); openTaskContextMenu(event, ${task.taskID})">
                    <span class="material-icons-outlined" style="font-size:1.1rem">more_horiz</span>
                </button>
            </div>
            ${task.description ? `<div style="font-size:0.8rem;color:var(--text-secondary);margin-bottom:0.5rem;display:-webkit-box;-webkit-line-clamp:2;-webkit-box-orient:vertical;overflow:hidden;">${escapeHtml(task.description)}</div>` : ''}
            <div class="task-card-meta">
                <div class="task-card-assignee">
                    <div class="avatar-xs" style="background:${avatarColor}">${initials}</div>
                    <span>${assignee ? assignee.fullName : '—'}</span>
                </div>
                ${task.deadline ? `
                    <div class="task-card-deadline ${deadlineClass}">
                        <span class="material-icons-outlined" style="font-size:0.8rem">schedule</span>
                        ${ui.formatDate(task.deadline)}
                    </div>
                ` : ''}
            </div>
            <div class="task-card-footer">
                <span class="chip ${ui.getPriorityChip(task.priorityName)}" style="font-size:0.7rem;padding:2px 8px;">
                    ${task.priorityName}
                </span>
                ${task.completedDate ? `
                    <span style="font-size:0.7rem;color:var(--text-muted)">✓ ${ui.formatDate(task.completedDate)}</span>
                ` : ''}
            </div>
        </div>
    `;
}

// Drag & Drop
function handleDragStart(e) {
    draggedCard = this;
    this.classList.add('dragging');
    e.dataTransfer.effectAllowed = 'move';
    e.dataTransfer.setData('text/plain', this.getAttribute('data-task-id'));
}

function handleDragEnd(e) {
    this.classList.remove('dragging');
    draggedCard = null;
    document.querySelectorAll('.board-column-body').forEach(body => {
        body.classList.remove('drag-over');
    });
}

function handleDragOver(e) {
    e.preventDefault();
    e.dataTransfer.dropEffect = 'move';
    this.classList.add('drag-over');
}

function handleDragLeave(e) {
    this.classList.remove('drag-over');
}

async function handleDrop(e) {
    e.preventDefault();
    this.classList.remove('drag-over');

    const taskId = e.dataTransfer.getData('text/plain');
    const newColumn = this.getAttribute('data-column');
    const columnConfig = tasksState.columns.find(c => c.id === newColumn);

    if (!taskId || !columnConfig) return;

    // Определяем новый порядок
    const afterElement = getDragAfterElement(this, e.clientY);
    const taskCards = [...this.querySelectorAll('.task-card')];
    const newSortOrder = afterElement
        ? taskCards.indexOf(afterElement)
        : taskCards.length;

    try {
        await api.patch('/tasks/move', {
            taskID: parseInt(taskId),
            newColumn: newColumn,
            newSortOrder: newSortOrder,
            newStatusID: columnConfig.statusID
        });

        // Оптимистичное обновление UI
        const task = tasksState.allTasks.find(t => t.taskID == taskId);
        if (task) {
            task.boardColumn = newColumn;
            task.statusID = columnConfig.statusID;
            task.sortOrder = newSortOrder;
        }
        renderBoard(tasksState.allTasks);
    } catch (error) {
        ui.showNotification('Ошибка перемещения', 'error');
        loadTasks();
    }
}

function getDragAfterElement(container, y) {
    const draggableElements = [...container.querySelectorAll('.task-card:not(.dragging)')];

    return draggableElements.reduce((closest, child) => {
        const box = child.getBoundingClientRect();
        const offset = y - box.top - box.height / 2;
        if (offset < 0 && offset > closest.offset) {
            return { offset: offset, element: child };
        } else {
            return closest;
        }
    }, { offset: Number.NEGATIVE_INFINITY }).element;
}

// Контекстное меню
function openTaskContextMenu(event, taskId) {
    event.stopPropagation();
    closeContextMenu();

    const task = tasksState.allTasks.find(t => t.taskID === taskId);
    if (!task) return;

    contextMenu = document.createElement('div');
    contextMenu.className = 'task-context-menu';
    contextMenu.innerHTML = `
        <button onclick="openTaskModal(null, ${taskId}); closeContextMenu();">
            <span class="material-icons-outlined">edit</span>
            Редактировать
        </button>
        <button onclick="moveTaskToColumn(${taskId}, 'В работе'); closeContextMenu();">
            <span class="material-icons-outlined" style="color:#2196F3">play_arrow</span>
            В работу
        </button>
        <button onclick="moveTaskToColumn(${taskId}, 'Готово'); closeContextMenu();">
            <span class="material-icons-outlined" style="color:#4CAF50">check</span>
            Выполнено
        </button>
        <div class="divider"></div>
        <button class="danger" onclick="deleteTask(${taskId}); closeContextMenu();">
            <span class="material-icons-outlined">delete</span>
            Удалить
        </button>
    `;

    document.body.appendChild(contextMenu);

    const rect = contextMenu.getBoundingClientRect();
    let x = event.clientX;
    let y = event.clientY;

    if (x + rect.width > window.innerWidth) x = window.innerWidth - rect.width - 8;
    if (y + rect.height > window.innerHeight) y = window.innerHeight - rect.height - 8;

    contextMenu.style.left = x + 'px';
    contextMenu.style.top = y + 'px';
}

function closeContextMenu() {
    if (contextMenu) {
        contextMenu.remove();
        contextMenu = null;
    }
}

async function moveTaskToColumn(taskId, columnName) {
    const column = tasksState.columns.find(c => c.id === columnName);
    if (!column) return;

    try {
        await api.patch('/tasks/move', {
            taskID: taskId,
            newColumn: columnName,
            newSortOrder: 0,
            newStatusID: column.statusID
        });
        ui.showNotification('Статус обновлён', 'success');
        loadTasks();
    } catch (error) {
        ui.showNotification('Ошибка обновления статуса', 'error');
    }
}

async function deleteTask(taskId) {
    const task = tasksState.allTasks.find(t => t.taskID === taskId);
    if (!task) return;

    ui.confirmDialog(
        'Удалить поручение',
        `Вы уверены, что хотите удалить поручение «${task.title}»?`,
        async () => {
            try {
                await api.delete(`/tasks/${taskId}`);
                ui.showNotification('Поручение удалено', 'success');
                loadTasks();
            } catch (error) {
                ui.showNotification('Ошибка удаления', 'error');
            }
        }
    );
}

// Модальное окно поручения
function openTaskModal(columnName = null, taskId = null) {
    const task = taskId ? tasksState.allTasks.find(t => t.taskID === taskId) : null;
    const isEdit = !!task;
    const title = isEdit ? 'Редактировать поручение' : 'Новое поручение';

    const bodyHTML = `
        <form id="task-form">
            <div class="form-group">
                <label class="form-label">Название <span class="required">*</span></label>
                <input type="text" class="form-input" id="task-title" value="${isEdit ? task.title : ''}" required minlength="3" maxlength="100">
            </div>
            <div class="form-group">
                <label class="form-label">Описание</label>
                <textarea class="form-textarea" id="task-description" rows="3" maxlength="500">${isEdit ? (task.description || '') : ''}</textarea>
            </div>
            <div class="form-row">
                <div class="form-group">
                    <label class="form-label">Исполнитель <span class="required">*</span></label>
                    <select class="form-select" id="task-assignee" required>
                        <option value="">Выберите...</option>
                        ${tasksState.assignees.map(a => `
                            <option value="${a.employeeID}" ${isEdit && task.assigneeID === a.employeeID ? 'selected' : ''}>
                                ${a.fullName}
                            </option>
                        `).join('')}
                    </select>
                </div>
                <div class="form-group">
                    <label class="form-label">Приоритет <span class="required">*</span></label>
                    <select class="form-select" id="task-priority" required>
                        <option value="1" ${isEdit && task.priorityID === 1 ? 'selected' : ''}>Низкий</option>
                        <option value="2" ${isEdit && task.priorityID === 2 ? 'selected' : ''}>Средний</option>
                        <option value="3" ${isEdit && task.priorityID === 3 ? 'selected' : ''}>Высокий</option>
                    </select>
                </div>
            </div>
            <div class="form-row">
                <div class="form-group">
                    <label class="form-label">Дедлайн</label>
                    <input type="date" class="form-input" id="task-deadline" value="${isEdit && task.deadline ? task.deadline.split('T')[0] : ''}">
                </div>
                <div class="form-group">
                    <label class="form-label">Колонка</label>
                    <select class="form-select" id="task-column">
                        ${tasksState.columns.map(c => `
                            <option value="${c.id}" ${(isEdit && task.boardColumn === c.id) || (!isEdit && columnName === c.id) ? 'selected' : ''}>
                                ${c.title}
                            </option>
                        `).join('')}
                    </select>
                </div>
            </div>
        </form>
    `;

    ui.openModal(title, bodyHTML, [
        { text: 'Отмена', class: 'btn-outline', callback: () => ui.closeModal() },
        {
            text: isEdit ? 'Сохранить' : 'Создать',
            class: 'btn-accent',
            callback: async () => await saveTask(isEdit, taskId)
        }
    ]);
}

async function saveTask(isEdit, taskId) {
    const title = document.getElementById('task-title').value.trim();
    const description = document.getElementById('task-description').value.trim() || null;
    const assigneeID = parseInt(document.getElementById('task-assignee').value);
    const priorityID = parseInt(document.getElementById('task-priority').value);
    const deadline = document.getElementById('task-deadline').value || null;
    const boardColumn = document.getElementById('task-column').value;

    if (!title || title.length < 3) {
        ui.showNotification('Название должно быть от 3 до 100 символов', 'error');
        return;
    }
    if (!assigneeID) {
        ui.showNotification('Выберите исполнителя', 'error');
        return;
    }

    const columnConfig = tasksState.columns.find(c => c.id === boardColumn);

    const data = {
        title,
        description,
        assigneeID,
        priorityID,
        deadline: deadline ? new Date(deadline).toISOString() : null,
        statusID: columnConfig ? columnConfig.statusID : 1,
        boardColumn,
        sortOrder: 0
    };

    try {
        if (isEdit) {
            await api.put(`/tasks/${taskId}`, data);
            ui.showNotification('Поручение обновлено', 'success');
        } else {
            await api.post('/tasks', data);
            ui.showNotification('Поручение создано', 'success');
        }
        ui.closeModal();
        loadTasks();
    } catch (error) {
        ui.showNotification(error.message || 'Ошибка сохранения', 'error');
    }
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}