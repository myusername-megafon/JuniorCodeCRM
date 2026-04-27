// Расписание занятий
let scheduleState = {
    viewMode: 'week', // 'week' | 'list'
    currentWeekStart: getMonday(new Date()),
    allSchedules: [],
    teachers: [],
    directions: [],
    page: 1,
    pageSize: 50,
    totalPages: 1
};

const DAY_NAMES = ['Понедельник', 'Вторник', 'Среда', 'Четверг', 'Пятница', 'Суббота', 'Воскресенье'];
const DAY_SHORT = ['ПН', 'ВТ', 'СР', 'ЧТ', 'ПТ', 'СБ', 'ВС'];

function getMonday(date) {
    const d = new Date(date);
    const day = d.getDay();
    const diff = d.getDate() - day + (day === 0 ? -6 : 1);
    d.setDate(diff);
    d.setHours(0, 0, 0, 0);
    return d;
}

async function initSchedule() {
    if (!auth.checkAuth()) return;

    await loadTeachers();
    await loadSchedules();

    document.getElementById('btn-add-schedule').addEventListener('click', () => openScheduleModal());
    document.getElementById('btn-refresh-schedule').addEventListener('click', () => loadSchedules());
    document.getElementById('search-schedule').addEventListener('input', debounce(() => loadSchedules(), 400));
    document.getElementById('filter-schedule-teacher').addEventListener('change', () => loadSchedules());
    document.getElementById('filter-schedule-direction').addEventListener('change', () => loadSchedules());
    document.getElementById('filter-schedule-recurring').addEventListener('change', () => loadSchedules());

    document.getElementById('btn-prev-week').addEventListener('click', () => {
        scheduleState.currentWeekStart.setDate(scheduleState.currentWeekStart.getDate() - 7);
        renderWeekView();
    });
    document.getElementById('btn-next-week').addEventListener('click', () => {
        scheduleState.currentWeekStart.setDate(scheduleState.currentWeekStart.getDate() + 7);
        renderWeekView();
    });
    document.getElementById('btn-today-week').addEventListener('click', () => {
        scheduleState.currentWeekStart = getMonday(new Date());
        renderWeekView();
    });
    document.getElementById('btn-list-view').addEventListener('click', toggleView);
}

async function loadTeachers() {
    try {
        const employees = await api.get('/employees', { isActive: 'true', pageSize: 100 });
        scheduleState.teachers = employees;
        const select = document.getElementById('filter-schedule-teacher');
        employees.forEach(e => {
            const option = document.createElement('option');
            option.value = e.employeeID;
            option.textContent = e.fullName;
            select.appendChild(option);
        });
    } catch (e) { }
}

async function loadSchedules() {
  
    try {
        const params = { pageSize: 200 };
        const search = document.getElementById('search-schedule').value;
        const teacherID = document.getElementById('filter-schedule-teacher').value;
        const direction = document.getElementById('filter-schedule-direction').value;
        const recurring = document.getElementById('filter-schedule-recurring').value;

        if (search) params.search = search;
        if (teacherID) params.teacherID = teacherID;
        if (direction) params.direction = direction;
        if (recurring !== '') params.isRecurring = recurring;

        scheduleState.allSchedules = await api.get('/schedule', params);

        // Собираем направления
        const dirSet = new Set();
        scheduleState.allSchedules.forEach(s => {
            if (s.direction) dirSet.add(s.direction);
        });
        const dirSelect = document.getElementById('filter-schedule-direction');
        dirSelect.innerHTML = '<option value="">Все направления</option>';
        dirSet.forEach(d => {
            const option = document.createElement('option');
            option.value = d;
            option.textContent = d;
            dirSelect.appendChild(option);
        });

        if (scheduleState.viewMode === 'week') {
            renderWeekView();
        } else {
            renderListView();
        }
    } catch (error) {
        document.getElementById('schedule-grid').innerHTML = '<div class="error-state">Ошибка загрузки</div>';
    }
}

function toggleView() {
    const btn = document.getElementById('btn-list-view');
    if (scheduleState.viewMode === 'week') {
        scheduleState.viewMode = 'list';
        btn.innerHTML = '<span class="material-icons-outlined">calendar_view_week</span> Неделя';
        document.getElementById('schedule-grid').style.display = 'none';
        document.getElementById('schedule-list-container').style.display = 'block';
        renderListView();
    } else {
        scheduleState.viewMode = 'week';
        btn.innerHTML = '<span class="material-icons-outlined">list</span> Список';
        document.getElementById('schedule-grid').style.display = 'grid';
        document.getElementById('schedule-list-container').style.display = 'none';
        renderWeekView();
    }
}

function renderWeekView() {
    const container = document.getElementById('schedule-grid');
    const weekStart = new Date(scheduleState.currentWeekStart);
    const weekEnd = new Date(weekStart);
    weekEnd.setDate(weekEnd.getDate() + 6);
    weekEnd.setHours(23, 59, 59, 999);

    // Обновляем заголовок
    const weekLabel = document.getElementById('week-label');
    if (weekLabel) {
        weekLabel.textContent =
            `${weekStart.toLocaleDateString('ru-RU', { day: 'numeric', month: 'long' })} — ${weekEnd.toLocaleDateString('ru-RU', { day: 'numeric', month: 'long', year: 'numeric' })}`;
    }

    // Получаем день недели для каждого дня (1 = ПН, 7 = ВС)
    const weekDays = [];
    for (let i = 0; i < 7; i++) {
        const date = new Date(weekStart);
        date.setDate(date.getDate() + i);
        const dayOfWeek = date.getDay(); // 0 = ВС, 1 = ПН, ..., 6 = СБ
        weekDays.push({
            date: date,
            dayNumber: dayOfWeek === 0 ? 7 : dayOfWeek, // Преобразуем: ПН=1, ВС=7
            dateStr: date.toISOString().split('T')[0]
        });
    }

    // Фильтруем занятия для этой недели
    const weekSchedules = scheduleState.allSchedules.filter(s => {
        const startDate = new Date(s.startDate);
        const endDate = s.endDate ? new Date(s.endDate) : new Date(2099, 11, 31);

        // Занятие должно быть активно в эту неделю
        if (endDate < weekStart || startDate > weekEnd) return false;

        if (s.isRecurring && s.dayOfWeek) {
            // Цикличное: проверяем, есть ли указанный день недели в этой неделе
            return weekDays.some(wd => wd.dayNumber === s.dayOfWeek);
        } else if (!s.isRecurring) {
            // Разовое: проверяем, попадает ли дата в неделю
            const sDate = new Date(s.startDate);
            return sDate >= weekStart && sDate <= weekEnd;
        }

        return false;
    });

    // Строим сетку
    let html = '<div class="week-header">';
    html += '<div class="week-time-col"></div>';

    for (let i = 0; i < 7; i++) {
        const date = weekDays[i].date;
        const isToday = date.toDateString() === new Date().toDateString();
        html += `
            <div class="week-day-header ${isToday ? 'today' : ''}">
                <span class="week-day-name">${DAY_SHORT[i]}</span>
                <span class="week-day-date">${date.getDate()}</span>
            </div>
        `;
    }
    html += '</div>';

    // Временные слоты (8:00 - 20:00)
    for (let hour = 8; hour <= 20; hour++) {
        html += '<div class="week-row">';
        html += `<div class="week-time-col">${String(hour).padStart(2, '0')}:00</div>`;

        for (let day = 0; day < 7; day++) {
            const currentDate = weekDays[day].date;
            const currentDayNumber = weekDays[day].dayNumber;

            // Находим занятия в этом слоте
            const cellSchedules = weekSchedules.filter(s => {
                // Проверяем час
                const startHour = parseInt(s.startTime?.split(':')[0]) || 0;
                if (startHour !== hour) return false;

                if (s.isRecurring && s.dayOfWeek) {
                    // Цикличное в этот день недели
                    return s.dayOfWeek === currentDayNumber;
                } else if (!s.isRecurring) {
                    // Разовое в эту дату
                    const sDate = new Date(s.startDate);
                    return sDate.toDateString() === currentDate.toDateString();
                }
                return false;
            });

            html += '<div class="week-cell">';
            cellSchedules.forEach(s => {
                const teacher = scheduleState.teachers.find(t => t.employeeID === s.teacherID);
                const bgColor = stringToColor(s.title);
                html += `
                    <div class="week-event" 
                         style="background:${bgColor}15;border-left:3px solid ${bgColor}"
                         onclick="openScheduleModal(${s.scheduleID})">
                        <div class="week-event-title">${escapeHtml(s.title)}</div>
                        <div class="week-event-time">${s.startTime?.substring(0, 5)} · ${s.duration} мин</div>
                        <div class="week-event-teacher">${teacher?.fullName || '—'}</div>
                        ${s.room ? `<div class="week-event-room">📍 ${s.room}</div>` : ''}
                    </div>
                `;
            });
            html += '</div>';
        }
        html += '</div>';
    }

    container.innerHTML = html;
}


function renderListView() {
    const container = document.getElementById('schedule-table');
    const columns = [
        {
            key: 'title', title: 'Название', render: (row) => `<strong>${row.title}</strong>
            ${row.direction ? `<br><span class="chip chip-info" style="font-size:0.75rem">${row.direction}</span>` : ''}`
        },
        { key: 'teacherName', title: 'Преподаватель' },
        { key: 'startDate', title: 'Начало', render: (row) => ui.formatDate(row.startDate) },
        { key: 'endDate', title: 'Конец', render: (row) => row.endDate ? ui.formatDate(row.endDate) : 'Бессрочно' },
        { key: 'startTime', title: 'Время', render: (row) => `${row.startTime?.substring(0, 5)} (${row.duration} мин)` },
        {
            key: 'isRecurring', title: 'Тип', render: (row) => row.isRecurring
                ? `<span class="chip chip-accent" style="background:rgba(131,212,254,0.2);color:#0a7abf;">🔄 ${row.dayOfWeekName || 'Цикл.'}</span>`
                : '<span class="chip">Разовое</span>'
        },
        { key: 'room', title: 'Аудитория', render: (row) => row.room || '—' },
        { key: 'maxStudents', title: 'Мест', render: (row) => row.maxStudents || '—' }
    ];

    const rowActions = [
        { key: 'edit', icon: 'edit', title: 'Редактировать', idKey: 'scheduleID' },
        { key: 'delete', icon: 'delete', title: 'Удалить', idKey: 'scheduleID' }
    ];

    ui.createDataTable(container, columns, scheduleState.allSchedules, {
        rowActions,
        emptyMessage: 'Занятия не найдены'
    });

    // Обработчики
    container.querySelectorAll('[data-action="edit"]').forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.stopPropagation();
            openScheduleModal(parseInt(btn.getAttribute('data-id')));
        });
    });

    container.querySelectorAll('[data-action="delete"]').forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.stopPropagation();
            deleteSchedule(parseInt(btn.getAttribute('data-id')));
        });
    });
}

function getDayNumber(date, weekStart) {
    const diff = date.getDay();
    return diff === 0 ? 7 : diff;
}

// Модальное окно занятия
function openScheduleModal(scheduleId = null) {
    const schedule = scheduleId ? scheduleState.allSchedules.find(s => s.scheduleID === scheduleId) : null;
    const isEdit = !!schedule;
    const title = isEdit ? 'Редактировать занятие' : 'Новое занятие';

    const bodyHTML = `
        <form id="schedule-form">
            <div class="form-group">
                <label class="form-label">Название <span class="required">*</span></label>
                <input type="text" class="form-input" id="sched-title" value="${isEdit ? schedule.title : ''}" required maxlength="200">
            </div>
            <div class="form-row">
                <div class="form-group">
                    <label class="form-label">Направление</label>
                    <input type="text" class="form-input" id="sched-direction" value="${isEdit ? (schedule.direction || '') : ''}" placeholder="Python, Web, Scratch...">
                </div>
                <div class="form-group">
                    <label class="form-label">Преподаватель <span class="required">*</span></label>
                    <select class="form-select" id="sched-teacher" required>
                        <option value="">Выберите...</option>
                        ${scheduleState.teachers.map(t => `
                            <option value="${t.employeeID}" ${isEdit && schedule.teacherID === t.employeeID ? 'selected' : ''}>
                                ${t.fullName}
                            </option>
                        `).join('')}
                    </select>
                </div>
            </div>
            <div class="form-row">
                <div class="form-group">
                    <label class="form-label">Дата начала <span class="required">*</span></label>
                    <input type="date" class="form-input" id="sched-startdate" value="${isEdit ? schedule.startDate?.split('T')[0] : ''}" required>
                </div>
                <div class="form-group">
                    <label class="form-label">Дата окончания</label>
                    <input type="date" class="form-input" id="sched-enddate" value="${isEdit && schedule.endDate ? schedule.endDate.split('T')[0] : ''}">
                </div>
            </div>
            <div class="form-row">
                <div class="form-group">
                    <label class="form-label">Время начала <span class="required">*</span></label>
                    <input type="time" class="form-input" id="sched-starttime" value="${isEdit ? (schedule.startTime?.substring(0, 5) || '10:00') : '10:00'}" required>
                </div>
                <div class="form-group">
                    <label class="form-label">Длительность (мин) <span class="required">*</span></label>
                    <input type="number" class="form-input" id="sched-duration" value="${isEdit ? schedule.duration : 60}" min="15" max="480" required>
                </div>
            </div>
            <div class="form-group">
                <label class="form-checkbox">
                    <input type="checkbox" id="sched-recurring" ${isEdit && schedule.isRecurring ? 'checked' : ''}>
                    <span>Цикличное занятие</span>
                </label>
            </div>
            <div class="form-row" id="recurring-options" style="display:${(isEdit && schedule.isRecurring) ? 'grid' : 'none'}">
                <div class="form-group">
                    <label class="form-label">День недели</label>
                    <select class="form-select" id="sched-dayofweek">
                        <option value="">Выберите...</option>
                        ${DAY_NAMES.map((name, i) => `
                            <option value="${i + 1}" ${isEdit && schedule.dayOfWeek === i + 1 ? 'selected' : ''}>${name}</option>
                        `).join('')}
                    </select>
                </div>
                <div class="form-group">
                    <label class="form-label">Правило</label>
                    <select class="form-select" id="sched-recurrencerule">
                        <option value="Weekly" ${isEdit && schedule.recurrenceRule === 'Weekly' ? 'selected' : ''}>Еженедельно</option>
                        <option value="BiWeekly" ${isEdit && schedule.recurrenceRule === 'BiWeekly' ? 'selected' : ''}>Раз в 2 недели</option>
                        <option value="Monthly" ${isEdit && schedule.recurrenceRule === 'Monthly' ? 'selected' : ''}>Раз в месяц</option>
                    </select>
                </div>
            </div>
            <div class="form-row">
                <div class="form-group">
                    <label class="form-label">Аудитория</label>
                    <input type="text" class="form-input" id="sched-room" value="${isEdit ? (schedule.room || '') : ''}">
                </div>
                <div class="form-group">
                    <label class="form-label">Макс. учеников</label>
                    <input type="number" class="form-input" id="sched-maxstudents" value="${isEdit ? (schedule.maxStudents || '') : ''}" min="1" max="100">
                </div>
            </div>
        </form>
    `;

    ui.openModal(title, bodyHTML, [
        { text: 'Отмена', class: 'btn-outline', callback: () => ui.closeModal() },
        {
            text: isEdit ? 'Сохранить' : 'Добавить',
            class: 'btn-accent',
            callback: async () => await saveSchedule(isEdit, scheduleId)
        }
    ]);

    // Переключение цикличных опций
    setTimeout(() => {
        document.getElementById('sched-recurring').addEventListener('change', (e) => {
            document.getElementById('recurring-options').style.display = e.target.checked ? 'grid' : 'none';
        });
    }, 100);
}

async function saveSchedule(isEdit, scheduleId) {
    const data = {
        title: document.getElementById('sched-title').value.trim(),
        direction: document.getElementById('sched-direction').value.trim() || null,
        teacherID: parseInt(document.getElementById('sched-teacher').value),
        startDate: document.getElementById('sched-startdate').value,
        endDate: document.getElementById('sched-enddate').value || null,
        startTime: document.getElementById('sched-starttime').value + ':00',
        duration: parseInt(document.getElementById('sched-duration').value),
        isRecurring: document.getElementById('sched-recurring').checked,
        recurrenceRule: document.getElementById('sched-recurring').checked ? document.getElementById('sched-recurrencerule').value : null,
        dayOfWeek: document.getElementById('sched-recurring').checked ? parseInt(document.getElementById('sched-dayofweek').value) || null : null,
        room: document.getElementById('sched-room').value.trim() || null,
        maxStudents: document.getElementById('sched-maxstudents').value ? parseInt(document.getElementById('sched-maxstudents').value) : null
    };

    if (!data.title || !data.teacherID || !data.startDate || !data.startTime || !data.duration) {
        ui.showNotification('Заполните обязательные поля', 'error');
        return;
    }

    try {
        if (isEdit) {
            await api.put(`/schedule/${scheduleId}`, data);
            ui.showNotification('Занятие обновлено', 'success');
        } else {
            await api.post('/schedule', data);
            ui.showNotification('Занятие добавлено', 'success');
        }
        ui.closeModal();
        loadSchedules();
    } catch (error) {
        ui.showNotification(error.message || 'Ошибка сохранения', 'error');
    }
}

async function deleteSchedule(scheduleId) {
    const schedule = scheduleState.allSchedules.find(s => s.scheduleID === scheduleId);
    if (!schedule) return;

    ui.confirmDialog(
        'Удалить занятие',
        `Вы уверены, что хотите удалить занятие «${schedule.title}»?`,
        async () => {
            try {
                await api.delete(`/schedule/${scheduleId}`);
                ui.showNotification('Занятие удалено', 'success');
                loadSchedules();
            } catch (error) {
                ui.showNotification('Ошибка удаления', 'error');
            }
        }
    );
}

// Утилиты
function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function debounce(func, wait) {
    let timeout;
    return function (...args) {
        clearTimeout(timeout);
        timeout = setTimeout(() => func.apply(this, args), wait);
    };
}

function stringToColor(str) {
    let hash = 0;
    for (let i = 0; i < str.length; i++) hash = str.charCodeAt(i) + ((hash << 5) - hash);
    const colors = ['#0a153a', '#1a73e8', '#4CAF50', '#FF9800', '#9C27B0', '#00BCD4', '#E91E63', '#3F51B5'];
    return colors[Math.abs(hash) % colors.length];
}

function getInitials(fullName) {
    const parts = fullName.split(' ');
    return parts.length >= 2 ? (parts[0][0] + parts[1][0]).toUpperCase() : (fullName[0] || '?').toUpperCase();
}