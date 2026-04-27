// Модуль отчётов
async function initReports() {
    if (!auth.checkAuth()) return;
    await loadAllReports();
}

async function loadAllReports() {
    await Promise.all([
        loadReport('staff'),
        loadReport('tasks'),
        loadReport('teachers')
    ]);
}

async function loadReport(type) {
    const contentId = `report-${type}-content`;
    const container = document.getElementById(contentId);
    if (!container) return;

    container.innerHTML = '<div class="loading-spinner"><div class="spinner"></div></div>';

    try {
        let data;
        switch (type) {
            case 'staff':
                data = await api.get('/reports/staff-by-department');
                renderStaffReport(container, data);
                break;
            case 'tasks':
                data = await api.get('/reports/task-execution');
                renderTasksReport(container, data);
                break;
            case 'teachers':
                data = await api.get('/reports/teacher-load');
                renderTeachersReport(container, data);
                break;
        }
    } catch (error) {
        container.innerHTML = '<div class="error-state">Ошибка загрузки отчёта</div>';
    }
}

async function exportReport(endpoint, format) {
    try {
        ui.showNotification(`Формирование ${format}-файла...`, 'info');
        await api.download(`/export/${endpoint}`, format);
        ui.showNotification('Файл загружен', 'success');
    } catch (error) {
        ui.showNotification('Ошибка экспорта', 'error');
    }
}

// Отчёт «Кадровый состав по отделам»
function renderStaffReport(container, data) {
    if (!data || data.length === 0) {
        container.innerHTML = '<div class="empty-state"><span class="material-icons-outlined">people</span><p>Нет данных для отчёта</p></div>';
        return;
    }

    // Группировка по отделам
    const departments = {};
    data.forEach(item => {
        if (!departments[item.department]) {
            departments[item.department] = { items: [], total: 0, combined: 0 };
        }
        departments[item.department].items.push(item);
        departments[item.department].total += item.employeeCount;
        departments[item.department].combined += item.combinedCount;
    });

    let html = '';
    for (const [deptName, dept] of Object.entries(departments)) {
        html += `
            <div class="report-section">
                <div class="report-section-header">
                    <h4>${deptName}</h4>
                    <div class="report-section-totals">
                        <span class="report-total-badge">Всего: ${dept.total}</span>
                        <span class="report-total-badge combined">Совместителей: ${dept.combined}</span>
                    </div>
                </div>
                <table class="report-table">
                    <thead>
                        <tr>
                            <th>Должность</th>
                            <th class="text-right">Кол-во</th>
                            <th class="text-right">Совмещают</th>
                            <th class="text-right">Доля совмещения</th>
                        </tr>
                    </thead>
                    <tbody>
                        ${dept.items.map(item => `
                            <tr>
                                <td>${item.position}</td>
                                <td class="text-right"><strong>${item.employeeCount}</strong></td>
                                <td class="text-right">${item.combinedCount}</td>
                                <td class="text-right">
                                    <div class="progress-mini">
                                        <div class="progress-mini-fill" style="width:${item.employeeCount > 0 ? Math.round(item.combinedCount / item.employeeCount * 100) : 0}%"></div>
                                    </div>
                                    <span style="font-size:0.8rem;margin-left:8px;">${item.employeeCount > 0 ? Math.round(item.combinedCount / item.employeeCount * 100) : 0}%</span>
                                </td>
                            </tr>
                        `).join('')}
                    </tbody>
                    <tfoot>
                        <tr>
                            <td><strong>Итого по отделу</strong></td>
                            <td class="text-right"><strong>${dept.total}</strong></td>
                            <td class="text-right"><strong>${dept.combined}</strong></td>
                            <td></td>
                        </tr>
                    </tfoot>
                </table>
            </div>
        `;
    }

    container.innerHTML = html;
}

// Отчёт «Исполнение поручений»
function renderTasksReport(container, data) {
    if (!data || data.length === 0) {
        container.innerHTML = '<div class="empty-state"><span class="material-icons-outlined">assignment</span><p>Нет данных для отчёта</p></div>';
        return;
    }

    // Группировка по статусам
    const statuses = {};
    let totalTasks = 0;
    let totalOverdue = 0;

    data.forEach(item => {
        if (!statuses[item.status]) {
            statuses[item.status] = { items: [], total: 0, overdue: 0 };
        }
        statuses[item.status].items.push(item);
        statuses[item.status].total += item.taskCount;
        statuses[item.status].overdue += item.overdueCount;
        totalTasks += item.taskCount;
        totalOverdue += item.overdueCount;
    });

    let html = `
        <div class="report-summary">
            <div class="summary-item">
                <span class="summary-value">${totalTasks}</span>
                <span class="summary-label">Всего поручений</span>
            </div>
            <div class="summary-item danger">
                <span class="summary-value">${totalOverdue}</span>
                <span class="summary-label">Просрочено</span>
            </div>
            <div class="summary-item success">
                <span class="summary-value">${totalTasks > 0 ? Math.round((totalTasks - totalOverdue) / totalTasks * 100) : 0}%</span>
                <span class="summary-label">Выполнено в срок</span>
            </div>
        </div>
    `;

    for (const [statusName, status] of Object.entries(statuses)) {
        html += `
            <div class="report-section">
                <div class="report-section-header">
                    <h4>
                        <span class="status-indicator" style="background:${getStatusColor(statusName)}"></span>
                        ${statusName}
                    </h4>
                    <span class="report-total-badge">${status.total}</span>
                </div>
                <table class="report-table">
                    <thead>
                        <tr>
                            <th>Исполнитель</th>
                            <th class="text-right">Поручений</th>
                            <th class="text-right">Просрочено</th>
                            <th class="text-right">Выполнение</th>
                        </tr>
                    </thead>
                    <tbody>
                        ${status.items.map(item => `
                            <tr>
                                <td>${item.assignee}</td>
                                <td class="text-right">${item.taskCount}</td>
                                <td class="text-right ${item.overdueCount > 0 ? 'text-danger' : ''}">${item.overdueCount}</td>
                                <td class="text-right">
                                    <div class="progress-mini">
                                        <div class="progress-mini-fill ${item.completionPercent >= 80 ? 'success' : item.completionPercent >= 50 ? 'warning' : 'danger'}" 
                                             style="width:${item.completionPercent}%"></div>
                                    </div>
                                    <span style="font-size:0.8rem;margin-left:8px;">${item.completionPercent}%</span>
                                </td>
                            </tr>
                        `).join('')}
                    </tbody>
                </table>
            </div>
        `;
    }

    container.innerHTML = html;
}

// Отчёт «Загрузка преподавателей»
function renderTeachersReport(container, data) {
    if (!data || data.length === 0) {
        container.innerHTML = '<div class="empty-state"><span class="material-icons-outlined">school</span><p>Нет данных для отчёта</p></div>';
        return;
    }

    const totalHours = data.reduce((sum, t) => sum + parseFloat(t.totalHours), 0);
    const totalClasses = data.reduce((sum, t) => sum + t.totalClasses, 0);

    let html = `
        <div class="report-summary">
            <div class="summary-item">
                <span class="summary-value">${totalClasses}</span>
                <span class="summary-label">Всего занятий</span>
            </div>
            <div class="summary-item accent">
                <span class="summary-value">${totalHours.toFixed(1)}</span>
                <span class="summary-label">Всего часов</span>
            </div>
            <div class="summary-item">
                <span class="summary-value">${data.length}</span>
                <span class="summary-label">Преподавателей</span>
            </div>
        </div>
        <table class="report-table">
            <thead>
                <tr>
                    <th>Преподаватель</th>
                    <th>Отдел</th>
                    <th class="text-right">Занятий</th>
                    <th class="text-right">Часов</th>
                    <th class="text-right">Цикличных</th>
                    <th class="text-right">Нагрузка</th>
                </tr>
            </thead>
            <tbody>
                ${data.map(t => {
        const loadPercent = Math.min(Math.round(parseFloat(t.totalHours) / 40 * 100), 100);
        return `
                        <tr>
                            <td><strong>${t.teacherName}</strong></td>
                            <td>${t.department}</td>
                            <td class="text-right">${t.totalClasses}</td>
                            <td class="text-right"><strong>${t.totalHours.toFixed(1)}</strong></td>
                            <td class="text-right">${t.recurringClasses}</td>
                            <td class="text-right">
                                <div class="progress-mini">
                                    <div class="progress-mini-fill ${loadPercent > 80 ? 'danger' : loadPercent > 50 ? 'warning' : 'success'}" 
                                         style="width:${loadPercent}%"></div>
                                </div>
                                <span style="font-size:0.8rem;margin-left:8px;">${loadPercent}%</span>
                            </td>
                        </tr>
                    `;
    }).join('')}
            </tbody>
        </table>
    `;

    container.innerHTML = html;
}

function getStatusColor(statusName) {
    const colors = {
        'В работе': '#2196F3',
        'Выполнено': '#4CAF50',
        'Просрочено': '#F44336',
        'Возвращено на доработку': '#FF9800'
    };
    return colors[statusName] || '#999';
}