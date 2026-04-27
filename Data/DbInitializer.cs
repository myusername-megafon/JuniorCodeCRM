using JuniorCodeCRM.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace JuniorCodeCRM.Data;

public static class DbInitializer
{
    public static void Initialize(AppDbContext context)
    {
        // Применяем миграции (если есть)
        context.Database.EnsureCreated();

        // Если данные уже есть — выходим
        if (context.Departments.Any())
            return;

        // ========== СПРАВОЧНИКИ ==========

        // Отделы
        var departments = new[]
        {
            new Department { Name = "Педагогический", Description = "Отдел обучения детей и подростков 6-15 лет" },
            new Department { Name = "Программистский", Description = "Отдел коммерческой разработки ПО" },
            new Department { Name = "Административный", Description = "Управление и координация деятельности" }
        };
        context.Departments.AddRange(departments);
        context.SaveChanges();

        // Должности
        var positions = new[]
        {
            new Position { Name = "Директор", CanBeCombined = false },
            new Position { Name = "Педагог", CanBeCombined = true },
            new Position { Name = "Программист", CanBeCombined = true },
            new Position { Name = "Администратор педагогического отдела", CanBeCombined = true },
            new Position { Name = "Администратор отдела программистов", CanBeCombined = true }
        };
        context.Positions.AddRange(positions);
        context.SaveChanges();

        // Статусы поручений
        var taskStatuses = new[]
        {
            new Models.Entities.TaskStatus { Name = "В работе", ColorHex = "#2196F3" },
            new Models.Entities.TaskStatus { Name = "Выполнено", ColorHex = "#4CAF50" },
            new Models.Entities.TaskStatus { Name = "Просрочено", ColorHex = "#F44336" },
            new Models.Entities.TaskStatus { Name = "Возвращено на доработку", ColorHex = "#FF9800" }
        };
        context.TaskStatuses.AddRange(taskStatuses);
        context.SaveChanges();

        // Приоритеты поручений
        var taskPriorities = new[]
        {
            new TaskPriority { Name = "Низкий", ColorHex = "#8BC34A" },
            new TaskPriority { Name = "Средний", ColorHex = "#FFC107" },
            new TaskPriority { Name = "Высокий", ColorHex = "#F44336" }
        };
        context.TaskPriorities.AddRange(taskPriorities);
        context.SaveChanges();

        // ========== СОТРУДНИКИ ==========

        var employees = new[]
        {
            new Employee
            {
                LastName = "Иванов", FirstName = "Максим", MiddleName = "Сергеевич",
                PositionID = 1, DepartmentID = 3,
                Phone = "+7 (910) 123-45-67", Email = "director@yuniorkod.ru",
                IsCombined = false, HireDate = new DateTime(2022, 1, 15),
                Notes = "Директор организации"
            },
            new Employee
            {
                LastName = "Петрова", FirstName = "Анна", MiddleName = "Владимировна",
                PositionID = 2, DepartmentID = 1,
                Phone = "+7 (910) 234-56-78", Email = "petrova@yuniorkod.ru",
                IsCombined = true, CombinedPositionID = 3,
                HireDate = new DateTime(2022, 3, 1),
                Notes = "Ведущий педагог, также пишет код"
            },
            new Employee
            {
                LastName = "Сидоров", FirstName = "Дмитрий", MiddleName = "Александрович",
                PositionID = 3, DepartmentID = 2,
                Phone = "+7 (910) 345-67-89", Email = "sidorov@yuniorkod.ru",
                IsCombined = false, HireDate = new DateTime(2022, 6, 15),
                Notes = "Fullstack-разработчик"
            },
            new Employee
            {
                LastName = "Кузнецова", FirstName = "Елена", MiddleName = "Игоревна",
                PositionID = 2, DepartmentID = 1,
                Phone = "+7 (910) 456-78-90", Email = "kuznetsova@yuniorkod.ru",
                IsCombined = false, HireDate = new DateTime(2023, 1, 10),
                Notes = "Педагог начальных групп"
            },
            new Employee
            {
                LastName = "Смирнов", FirstName = "Алексей", MiddleName = "Петрович",
                PositionID = 3, DepartmentID = 2,
                Phone = "+7 (910) 567-89-01", Email = "smirnov@yuniorkod.ru",
                IsCombined = true, CombinedPositionID = 2,
                HireDate = new DateTime(2023, 2, 1),
                Notes = "Программист, иногда ведёт занятия"
            }
        };
        context.Employees.AddRange(employees);
        context.SaveChanges();

        // ========== ПОРУЧЕНИЯ ==========

        var tasks = new[]
        {
            new TaskItem
            {
                Title = "Разработать лендинг для заказчика \"ТехноПром\"",
                Description = "Создать одностраничный сайт с формой заявки. Требуется адаптивная вёрстка.",
                AssigneeID = 3, StatusID = 1, PriorityID = 3,
                CreatedDate = DateTime.Now.AddDays(-5),
                Deadline = new DateTime(2026, 5, 15),
                BoardColumn = "В работе", SortOrder = 1
            },
            new TaskItem
            {
                Title = "Подготовить программу летнего лагеря",
                Description = "Разработать учебный план на 2 недели для детей 8-10 лет: Scratch + Python basics.",
                AssigneeID = 2, StatusID = 1, PriorityID = 2,
                CreatedDate = DateTime.Now.AddDays(-3),
                Deadline = new DateTime(2026, 5, 1),
                BoardColumn = "В работе", SortOrder = 2
            },
            new TaskItem
            {
                Title = "Провести тестирование нового модуля CRM",
                Description = "Протестировать модуль отчётности, составить баг-репорт.",
                AssigneeID = 5, StatusID = 2, PriorityID = 1,
                CreatedDate = DateTime.Now.AddDays(-10),
                Deadline = new DateTime(2026, 4, 20),
                CompletedDate = DateTime.Now.AddDays(-2),
                BoardColumn = "Готово", SortOrder = 1
            },
            new TaskItem
            {
                Title = "Обновить контент на сайте школы",
                Description = "Добавить информацию о новых курсах, обновить фото преподавателей.",
                AssigneeID = 4, StatusID = 4, PriorityID = 2,
                CreatedDate = DateTime.Now.AddDays(-7),
                Deadline = new DateTime(2026, 4, 25),
                BoardColumn = "На доработке", SortOrder = 1
            },
            new TaskItem
            {
                Title = "Составить отчёт по финансам за квартал",
                Description = "Подготовить сводку доходов/расходов для учредителей.",
                AssigneeID = 1, StatusID = 3, PriorityID = 3,
                CreatedDate = DateTime.Now.AddDays(-20),
                Deadline = new DateTime(2026, 4, 10),
                BoardColumn = "Просрочено", SortOrder = 1
            }
        };
        context.Tasks.AddRange(tasks);
        context.SaveChanges();

        // ========== РАСПИСАНИЕ ==========

        var schedules = new[]
        {
            new ScheduleItem
            {
                Title = "Python для начинающих (группа А)",
                Direction = "Python",
                TeacherID = 2,
                StartDate = new DateTime(2026, 2, 1),
                EndDate = new DateTime(2026, 6, 30),
                StartTime = new TimeSpan(10, 0, 0),
                Duration = 90,
                IsRecurring = true,
                RecurrenceRule = "Weekly",
                DayOfWeek = 1,
                Room = "Аудитория 101",
                MaxStudents = 12
            },
            new ScheduleItem
            {
                Title = "Веб-разработка (группа B)",
                Direction = "Web",
                TeacherID = 4,
                StartDate = new DateTime(2026, 2, 5),
                EndDate = new DateTime(2026, 6, 30),
                StartTime = new TimeSpan(15, 0, 0),
                Duration = 120,
                IsRecurring = true,
                RecurrenceRule = "Weekly",
                DayOfWeek = 5,
                Room = "Аудитория 102",
                MaxStudents = 10
            },
            new ScheduleItem
            {
                Title = "Scratch для детей 6-8 лет",
                Direction = "Scratch",
                TeacherID = 4,
                StartDate = new DateTime(2026, 3, 1),
                EndDate = new DateTime(2026, 5, 31),
                StartTime = new TimeSpan(12, 0, 0),
                Duration = 60,
                IsRecurring = true,
                RecurrenceRule = "Weekly",
                DayOfWeek = 3,
                Room = "Аудитория 103",
                MaxStudents = 8
            },
            new ScheduleItem
            {
                Title = "Мастер-класс по нейросетям",
                Direction = "AI",
                TeacherID = 5,
                StartDate = new DateTime(2026, 5, 15),
                StartTime = new TimeSpan(14, 0, 0),
                Duration = 180,
                IsRecurring = false,
                Room = "Конференц-зал",
                MaxStudents = 30
            }
        };
        context.Schedule.AddRange(schedules);
        context.SaveChanges();
    }
}