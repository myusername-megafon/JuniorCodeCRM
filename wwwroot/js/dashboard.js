// Дашборд
async function initDashboard() {
    updateDashboardDate();
    await loadDashboardData();
}

function updateDashboardDate() {
    const el = document.getElementById('dashboard-date');
    if (!el) return;

    const now = new Date();
    const options = { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' };
    el.textContent = now.toLocaleDateString('ru-RU', options);
}

async function loadDashboardData() {
    if (!auth.checkAuth()) return;

    try {
        const data = await api.get('/dashboard');
        renderMetrics(data);
        renderDepartmentChart(data.departmentLoads);
        renderTaskChart(data.taskDistribution);
        renderUpcomingClasses(data.upcomingClasses);
        renderCriticalTasks(data);
    } catch (error) {
        console.error('Ошибка загрузки дашборда:', error);
        ui.showNotification('Не удалось загрузить данные дашборда', 'error');
    }
}

function renderMetrics(data) {
    const container = document.getElementById('metrics-grid');
    if (!container) return;

    const metrics = [
        {
            value: data.totalEmployees,
            label: 'Сотрудников',
            icon: 'people',
            color: 'primary'
        },
        {
            value: data.activeSchedules,
            label: 'Активных занятий',
            icon: 'school',
            color: 'accent'
        },
        {
            value: data.todayClasses,
            label: 'Занятий сегодня',
            icon: 'today',
            color: 'info'
        },
        {
            value: data.tasksInProgress,
            label: 'Поручений в работе',
            icon: 'assignment',
            color: 'warning'
        },
        {
            value: data.tasksOverdue,
            label: 'Просрочено',
            icon: 'error',
            color: 'danger'
        },
        {
            value: data.tasksNearDeadline,
            label: 'Срочных (до 3 дней)',
            icon: 'schedule',
            color: 'warning'
        }
    ];

    container.innerHTML = metrics.map((m, i) => `
        <div class="metric-card ${m.color}" style="animation-delay: ${i * 0.05}s;">
            <div class="metric-icon">
                <span class="material-icons-outlined">${m.icon}</span>
            </div>
            <div class="metric-value" id="metric-${i}">0</div>
            <div class="metric-label">${m.label}</div>
        </div>
    `).join('');

    // Анимированный счётчик
    metrics.forEach((m, i) => {
        animateCounter(`metric-${i}`, m.value);
    });
}

function animateCounter(elementId, target) {
    const el = document.getElementById(elementId);
    if (!el) return;

    const duration = 800;
    const start = 0;
    const startTime = performance.now();

    function update(currentTime) {
        const elapsed = currentTime - startTime;
        const progress = Math.min(elapsed / duration, 1);
        // Ease out cubic
        const eased = 1 - Math.pow(1 - progress, 3);
        const current = Math.round(start + (target - start) * eased);
        el.textContent = current;

        if (progress < 1) {
            requestAnimationFrame(update);
        }
    }

    requestAnimationFrame(update);
}

function renderDepartmentChart(departments) {
    const container = document.getElementById('chart-departments');
    if (!container || !departments || departments.length === 0) {
        if (container) container.innerHTML = '<div class="empty-state"><span class="material-icons-outlined">business</span><p>Нет данных</p></div>';
        return;
    }

    const maxEmployees = Math.max(...departments.map(d => d.totalEmployees), 1);

    container.innerHTML = `
        <div class="h-bar-chart">
            ${departments.map(d => {
        const widthPercent = (d.totalEmployees / maxEmployees * 100);
        return `
                    <div class="h-bar-item">
                        <div class="h-bar-label">${d.departmentName}</div>
                        <div class="h-bar-track">
                            <div class="h-bar-fill ${d.departmentName.includes('Педагог') ? 'accent' : d.departmentName.includes('Программ') ? 'primary' : 'success'}" 
                                 style="width: 0%">
                                ${d.totalEmployees}
                            </div>
                        </div>
                        <div class="h-bar-value">${d.totalEmployees}</div>
                    </div>
                `;
    }).join('')}
        </div>
    `;

    // Анимация заполнения
    setTimeout(() => {
        container.querySelectorAll('.h-bar-fill').forEach(bar => {
            const targetWidth = departments.find(d =>
                d.totalEmployees === parseInt(bar.textContent.trim())
            );
            if (targetWidth) {
                const widthPercent = (targetWidth.totalEmployees / maxEmployees * 100);
                bar.style.width = widthPercent + '%';
            }
        });
    }, 100);
}

function renderTaskChart(taskDistribution) {
    const container = document.getElementById('chart-tasks');
    if (!container || !taskDistribution || taskDistribution.length === 0) {
        if (container) container.innerHTML = '<div class="empty-state"><span class="material-icons-outlined">pie_chart</span><p>Нет данных</p></div>';
        return;
    }

    const total = taskDistribution.reduce((sum, t) => sum + t.count, 0);
    const colors = {
        'В работе': '#2196F3',
        'Выполнено': '#4CAF50',
        'Просрочено': '#F44336',
        'Возвращено на доработку': '#FF9800'
    };

    // Строим SVG donut
    let cumulativePercent = 0;
    const slices = taskDistribution.map(t => {
        const percent = total > 0 ? (t.count / total) * 100 : 0;
        const startAngle = cumulativePercent * 3.6;
        cumulativePercent += percent;
        const endAngle = cumulativePercent * 3.6;

        const startX = 80 + 70 * Math.cos((startAngle - 90) * Math.PI / 180);
        const startY = 80 + 70 * Math.sin((startAngle - 90) * Math.PI / 180);
        const endX = 80 + 70 * Math.cos((endAngle - 90) * Math.PI / 180);
        const endY = 80 + 70 * Math.sin((endAngle - 90) * Math.PI / 180);

        const largeArc = percent > 50 ? 1 : 0;

        return { ...t, percent, color: colors[t.statusName] || '#999', startX, startY, endX, endY, largeArc };
    });

    container.innerHTML = `
        <div class="donut-chart">
            <div class="donut-visual">
                <svg class="donut-svg" viewBox="0 0 160 160">
                    ${slices.map(s => `
                        <path d="M80,80 L${s.startX},${s.startY} A70,70 0 ${s.largeArc},1 ${s.endX},${s.endY} Z" 
                              fill="${s.color}" 
                              stroke="#fff" stroke-width="2"
                              style="opacity: 0; transform: scale(0.8); transform-origin: 80px 80px; transition: all 0.6s ease;">
                        </path>
                    `).join('')}
                    <circle cx="80" cy="80" r="42" fill="#fff"/>
                </svg>
                <div class="donut-center">
                    <span class="donut-total">${total}</span>
                    <span class="donut-label">всего</span>
                </div>
            </div>
            <div class="donut-legend">
                ${taskDistribution.map(t => `
                    <div class="donut-legend-item">
                        <span class="donut-legend-dot" style="background: ${colors[t.statusName] || '#999'}"></span>
                        <span>${t.statusName}</span>
                        <span class="donut-legend-value">${t.count}</span>
                    </div>
                `).join('')}
            </div>
        </div>
    `;

    // Анимация появления сегментов
    setTimeout(() => {
        container.querySelectorAll('path').forEach(path => {
            path.style.opacity = '1';
            path.style.transform = 'scale(1)';
        });
    }, 100);
}

function renderUpcomingClasses(classes) {
    const container = document.getElementById('upcoming-classes');
    if (!container) return;

    if (!classes || classes.length === 0) {
        container.innerHTML = '<div class="empty-state"><span class="material-icons-outlined">event_busy</span><p>Нет ближайших занятий</p></div>';
        return;
    }

    container.innerHTML = `
        <div class="class-list">
            ${classes.map(c => `
                <div class="class-item fade-in">
                    <div class="class-time">
                        <div class="time">${c.startTime?.substring(0, 5) || '—'}</div>
                        <div class="date">${ui.formatDate(c.startDate)}</div>
                    </div>
                    <div class="class-info">
                        <div class="class-title">${c.title}</div>
                        <div class="class-teacher">${c.teacherName}</div>
                    </div>
                    <div class="class-room">
                        <span class="material-icons-outlined" style="font-size:1rem">room</span>
                        ${c.room || 'Не указана'}
                    </div>
                </div>
            `).join('')}
        </div>
    `;
}

function renderCriticalTasks(data) {
    const container = document.getElementById('critical-tasks');
    if (!container) return;

    // Здесь нужно получить просроченные и срочные поручения
    // Пока используем заглушку с данными из дашборда
    const criticalCount = data.tasksOverdue + data.tasksNearDeadline;

    if (criticalCount === 0) {
        container.innerHTML = '<div class="empty-state"><span class="material-icons-outlined">check_circle</span><p>Нет критических поручений</p></div>';
        return;
    }

    // Загружаем реальные просроченные задачи
    api.get('/tasks', { statusID: 3, pageSize: 5 })
        .then(tasks => {
            if (!tasks || tasks.length === 0) {
                container.innerHTML = '<div class="empty-state"><span class="material-icons-outlined">check_circle</span><p>Нет просроченных поручений</p></div>';
                return;
            }

            container.innerHTML = `
                <div class="critical-list">
                    ${tasks.map(t => {
                const daysOverdue = t.deadline ? Math.ceil((new Date() - new Date(t.deadline)) / (1000 * 60 * 60 * 24)) : 0;
                return `
                            <div class="critical-item fade-in" onclick="window.location.hash='#tasks'">
                                <div class="critical-days">${daysOverdue}д</div>
                                <div class="critical-info">
                                    <div class="critical-title">${t.title}</div>
                                    <div class="critical-assignee">${t.assigneeName}</div>
                                </div>
                                <span class="material-icons-outlined text-danger">chevron_right</span>
                            </div>
                        `;
            }).join('')}
                </div>
            `;
        })
        .catch(() => {
            container.innerHTML = '<div class="empty-state"><span class="material-icons-outlined">error</span><p>Не удалось загрузить данные</p></div>';
        });
}