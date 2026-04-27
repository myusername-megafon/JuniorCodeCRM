// Модуль сотрудников
let employeesState = {
    page: 1,
    pageSize: CONFIG.DEFAULT_PAGE_SIZE,
    totalPages: 1,
    search: '',
    departmentID: '',
    positionID: '',
    isActive: ''
};

let departmentsCache = [];
let positionsCache = [];

async function initEmployees() {
    if (!auth.checkAuth()) return;

    await loadFilters();
    await loadEmployees();

    // Обработчики
    document.getElementById('btn-add-employee').addEventListener('click', openEmployeeModal);
    document.getElementById('btn-refresh-employees').addEventListener('click', () => loadEmployees());
    document.getElementById('search-employees').addEventListener('input', debounce(() => {
        employeesState.search = document.getElementById('search-employees').value;
        employeesState.page = 1;
        loadEmployees();
    }, 400));
    document.getElementById('filter-department').addEventListener('change', (e) => {
        employeesState.departmentID = e.target.value;
        employeesState.page = 1;
        loadEmployees();
    });
    document.getElementById('filter-position').addEventListener('change', (e) => {
        employeesState.positionID = e.target.value;
        employeesState.page = 1;
        loadEmployees();
    });
    document.getElementById('filter-status').addEventListener('change', (e) => {
        employeesState.isActive = e.target.value;
        employeesState.page = 1;
        loadEmployees();
    });
}

async function loadFilters() {
    try {
        // Загружаем отделы и должности для фильтров
        // (предполагаем что есть эндпоинты /departments и /positions)
        const deptFilter = document.getElementById('filter-department');
        departmentsCache.forEach(d => {
            const option = document.createElement('option');
            option.value = d.departmentID;
            option.textContent = d.name;
            deptFilter.appendChild(option);
        });
    } catch (e) {
        // Фильтры загружаем из данных сотрудников (костыль, но без отдельных эндпоинтов)
    }
}

async function loadEmployees() {
    const container = document.getElementById('employees-table-container');
    container.innerHTML = '<div class="loading-spinner"><div class="spinner"></div></div>';

    try {
        const params = {
            page: employeesState.page,
            pageSize: employeesState.pageSize
        };
        if (employeesState.search) params.search = employeesState.search;
        if (employeesState.departmentID) params.departmentID = employeesState.departmentID;
        if (employeesState.positionID) params.positionID = employeesState.positionID;
        if (employeesState.isActive !== '') params.isActive = employeesState.isActive;

        const employees = await api.get('/employees', params);

        // Заполняем фильтры, если пустые
        if (departmentsCache.length === 0 && employees.length > 0) {
            updateFilterOptions(employees);
        }

        renderEmployeesTable(employees);
        renderEmployeesPagination();
    } catch (error) {
        container.innerHTML = '<div class="error-state">Ошибка загрузки данных</div>';
    }
}

function updateFilterOptions(employees) {
    const deptSet = new Map();
    const posSet = new Map();

    employees.forEach(e => {
        if (e.departmentName) deptSet.set(e.departmentID, e.departmentName);
        if (e.positionName) posSet.set(e.positionID, e.positionName);
    });

    const deptFilter = document.getElementById('filter-department');
    deptSet.forEach((name, id) => {
        if (!deptFilter.querySelector(`option[value="${id}"]`)) {
            const option = document.createElement('option');
            option.value = id;
            option.textContent = name;
            deptFilter.appendChild(option);
        }
    });

    const posFilter = document.getElementById('filter-position');
    posSet.forEach((name, id) => {
        if (!posFilter.querySelector(`option[value="${id}"]`)) {
            const option = document.createElement('option');
            option.value = id;
            option.textContent = name;
            posFilter.appendChild(option);
        }
    });
}

function renderEmployeesTable(employees) {
    const container = document.getElementById('employees-table-container');

    const columns = [
        {
            key: 'fullName', title: 'ФИО', render: (row) => `
            <div class="employee-name-cell">
                <div class="employee-avatar-sm" style="background: ${stringToColor(row.fullName)}">
                    ${getInitials(row.fullName)}
                </div>
                <div>
                    <div class="employee-fullname">${row.fullName}</div>
                    <div class="employee-position-sm">${row.positionName}${row.isCombined ? ' + ' + row.combinedPositionName : ''}</div>
                </div>
            </div>
        `},
        { key: 'departmentName', title: 'Отдел', render: (row) => row.departmentName },
        { key: 'phone', title: 'Телефон', render: (row) => ui.formatPhone(row.phone) },
        { key: 'email', title: 'Email', render: (row) => row.email || '—' },
        {
            key: 'isCombined', title: 'Совмещение', render: (row) => row.isCombined
                ? '<span class="chip chip-accent" style="background:rgba(131,212,254,0.2);color:#0a7abf;">Совмещает</span>'
                : '<span class="chip" style="background:#f0f0f0;color:#999;">Нет</span>'
        },
        {
            key: 'isActive', title: 'Статус', render: (row) => row.isActive
                ? '<span class="chip chip-success">Активен</span>'
                : '<span class="chip chip-danger">Архивирован</span>'
        }
    ];

    const rowActions = [
        { key: 'edit', icon: 'edit', title: 'Редактировать', idKey: 'employeeID' },
        { key: 'delete', icon: 'archive', title: 'Архивировать', idKey: 'employeeID' }
    ];

    const tempDiv = document.createElement('div');
    ui.createDataTable(tempDiv, columns, employees, {
        rowActions,
        emptyMessage: 'Сотрудники не найдены'
    });

    container.innerHTML = tempDiv.innerHTML;

    // Обработчики действий
    container.querySelectorAll('[data-action="edit"]').forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.stopPropagation();
            const id = btn.getAttribute('data-id');
            const employee = employees.find(emp => emp.employeeID == id);
            if (employee) openEmployeeModal(employee);
        });
    });

    container.querySelectorAll('[data-action="delete"]').forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.stopPropagation();
            const id = btn.getAttribute('data-id');
            const employee = employees.find(emp => emp.employeeID == id);
            if (employee) archiveEmployee(employee);
        });
    });
}

function renderEmployeesPagination() {
    const container = document.getElementById('employees-pagination');
    ui.renderPagination(container, employeesState.page, employeesState.totalPages, (page) => {
        employeesState.page = page;
        loadEmployees();
        document.getElementById('employees-table-container').scrollIntoView({ behavior: 'smooth' });
    });
}

// Модальное окно сотрудника
function openEmployeeModal(employee = null) {
    const isEdit = !!employee;
    const title = isEdit ? 'Редактировать сотрудника' : 'Новый сотрудник';

    const bodyHTML = `
        <form id="employee-form">
            <div class="form-row">
                <div class="form-group">
                    <label class="form-label">Фамилия <span class="required">*</span></label>
                    <input type="text" class="form-input" id="emp-lastname" value="${isEdit ? employee.lastName : ''}" required maxlength="50">
                </div>
                <div class="form-group">
                    <label class="form-label">Имя <span class="required">*</span></label>
                    <input type="text" class="form-input" id="emp-firstname" value="${isEdit ? employee.firstName : ''}" required maxlength="50">
                </div>
            </div>
            <div class="form-row">
                <div class="form-group">
                    <label class="form-label">Отчество</label>
                    <input type="text" class="form-input" id="emp-middlename" value="${isEdit ? (employee.middleName || '') : ''}" maxlength="50">
                </div>
                <div class="form-group">
                    <label class="form-label">Телефон</label>
                    <input type="tel" class="form-input" id="emp-phone" value="${isEdit ? (employee.phone || '') : ''}" placeholder="+7 (___) ___-__-__">
                </div>
            </div>
            <div class="form-row">
                <div class="form-group">
                    <label class="form-label">Должность <span class="required">*</span></label>
                    <select class="form-select" id="emp-position" required>
                        <option value="">Выберите...</option>
                    </select>
                </div>
                <div class="form-group">
                    <label class="form-label">Отдел <span class="required">*</span></label>
                    <select class="form-select" id="emp-department" required>
                        <option value="">Выберите...</option>
                    </select>
                </div>
            </div>
            <div class="form-group">
                <label class="form-label">Email</label>
                <input type="email" class="form-input" id="emp-email" value="${isEdit ? (employee.email || '') : ''}">
            </div>
            <div class="form-group">
                <label class="form-checkbox">
                    <input type="checkbox" id="emp-combined" ${isEdit && employee.isCombined ? 'checked' : ''}>
                    <span>Совмещает должности</span>
                </label>
            </div>
            <div class="form-group" id="combined-position-group" style="display: ${(isEdit && employee.isCombined) ? 'block' : 'none'}">
                <label class="form-label">Совмещаемая должность</label>
                <select class="form-select" id="emp-combined-position">
                    <option value="">Выберите...</option>
                </select>
            </div>
            <div class="form-group">
                <label class="form-label">Примечания</label>
                <textarea class="form-textarea" id="emp-notes" rows="2" maxlength="500">${isEdit ? (employee.notes || '') : ''}</textarea>
            </div>
        </form>
    `;

    ui.openModal(title, bodyHTML, [
        {
            text: 'Отмена',
            class: 'btn-outline',
            callback: () => ui.closeModal()
        },
        {
            text: isEdit ? 'Сохранить' : 'Добавить',
            class: 'btn-accent',
            callback: async () => {
                await saveEmployee(isEdit, employee?.employeeID);
            }
        }
    ]);

    // Заполняем выпадающие списки
    setTimeout(async () => {
        await populateFormDropdowns();

        if (isEdit) {
            document.getElementById('emp-position').value = employee.positionID;
            document.getElementById('emp-department').value = employee.departmentID;
            if (employee.isCombined && employee.combinedPositionID) {
                document.getElementById('emp-combined-position').value = employee.combinedPositionID;
            }
        }

        // Переключение видимости совмещаемой должности
        document.getElementById('emp-combined').addEventListener('change', (e) => {
            document.getElementById('combined-position-group').style.display = e.target.checked ? 'block' : 'none';
        });
    }, 100);
}

async function populateFormDropdowns() {
    try {
        // Грузим справочники через API
        const employees = await api.get('/employees', { pageSize: 1 });
        // Если есть отдельные эндпоинты — используй их
        // Здесь упрощённо: данные уже в кэше из loadEmployees
    } catch (e) { }

    // Заполняем из глобального состояния (костыль)
    const deptSelect = document.getElementById('emp-department');
    const posSelect = document.getElementById('emp-position');
    const combinedPosSelect = document.getElementById('emp-combined-position');

    // Базовые значения (если API не дал)
    const defaultDepartments = [
        { id: 1, name: 'Педагогический' },
        { id: 2, name: 'Программистский' },
        { id: 3, name: 'Административный' }
    ];
    const defaultPositions = [
        { id: 1, name: 'Директор' },
        { id: 2, name: 'Педагог' },
        { id: 3, name: 'Программист' },
        { id: 4, name: 'Администратор педагогического отдела' },
        { id: 5, name: 'Администратор отдела программистов' }
    ];

    if (deptSelect && deptSelect.options.length <= 1) {
        defaultDepartments.forEach(d => {
            deptSelect.innerHTML += `<option value="${d.id}">${d.name}</option>`;
        });
    }
    if (posSelect && posSelect.options.length <= 1) {
        defaultPositions.forEach(p => {
            posSelect.innerHTML += `<option value="${p.id}">${p.name}</option>`;
        });
    }
    if (combinedPosSelect && combinedPosSelect.options.length <= 1) {
        defaultPositions.filter(p => p.id !== 1).forEach(p => {
            combinedPosSelect.innerHTML += `<option value="${p.id}">${p.name}</option>`;
        });
    }
}

async function saveEmployee(isEdit, employeeId) {
    const data = {
        lastName: document.getElementById('emp-lastname').value.trim(),
        firstName: document.getElementById('emp-firstname').value.trim(),
        middleName: document.getElementById('emp-middlename').value.trim() || null,
        phone: document.getElementById('emp-phone').value.trim() || null,
        email: document.getElementById('emp-email').value.trim() || null,
        positionID: parseInt(document.getElementById('emp-position').value),
        departmentID: parseInt(document.getElementById('emp-department').value),
        isCombined: document.getElementById('emp-combined').checked,
        combinedPositionID: document.getElementById('emp-combined').checked
            ? (parseInt(document.getElementById('emp-combined-position').value) || null)
            : null,
        notes: document.getElementById('emp-notes').value.trim() || null
    };

    // Базовая валидация
    if (!data.lastName || !data.firstName) {
        ui.showNotification('Заполните фамилию и имя', 'error');
        return;
    }
    if (!data.positionID || !data.departmentID) {
        ui.showNotification('Выберите должность и отдел', 'error');
        return;
    }

    try {
        if (isEdit) {
            data.isActive = true; // сохраняем активность при редактировании
            await api.put(`/employees/${employeeId}`, data);
            ui.showNotification('Сотрудник обновлён', 'success');
        } else {
            await api.post('/employees', data);
            ui.showNotification('Сотрудник добавлен', 'success');
        }
        ui.closeModal();
        loadEmployees();
    } catch (error) {
        ui.showNotification(error.message || 'Ошибка сохранения', 'error');
    }
}

async function archiveEmployee(employee) {
    ui.confirmDialog(
        'Архивировать сотрудника',
        `Вы уверены, что хотите архивировать ${employee.fullName}?`,
        async () => {
            try {
                await api.delete(`/employees/${employee.employeeID}`);
                ui.showNotification('Сотрудник архивирован', 'success');
                loadEmployees();
            } catch (error) {
                ui.showNotification('Ошибка архивации', 'error');
            }
        }
    );
}

// Вспомогательные функции
function stringToColor(str) {
    let hash = 0;
    for (let i = 0; i < str.length; i++) {
        hash = str.charCodeAt(i) + ((hash << 5) - hash);
    }
    const colors = ['#0a153a', '#1a73e8', '#4CAF50', '#FF9800', '#9C27B0', '#00BCD4', '#E91E63', '#3F51B5'];
    return colors[Math.abs(hash) % colors.length];
}

function getInitials(fullName) {
    const parts = fullName.split(' ');
    if (parts.length >= 2) {
        return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return (fullName[0] || '?').toUpperCase();
}

function debounce(func, wait) {
    let timeout;
    return function (...args) {
        clearTimeout(timeout);
        timeout = setTimeout(() => func.apply(this, args), wait);
    };
}