namespace JuniorCodeCRM.Models.DTOs.Dashboard;

public class DashboardDto
{
    public int TotalEmployees { get; set; }
    public int ActiveSchedules { get; set; }
    public int TodayClasses { get; set; }
    public int TasksInProgress { get; set; }
    public int TasksOverdue { get; set; }
    public int TasksNearDeadline { get; set; }

    public List<DepartmentLoadDto> DepartmentLoads { get; set; } = new();
    public List<TaskStatusDistributionDto> TaskDistribution { get; set; } = new();
    public List<UpcomingClassDto> UpcomingClasses { get; set; } = new();
}

public class DepartmentLoadDto
{
    public int DepartmentID { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int TotalEmployees { get; set; }
    public int CombinedEmployees { get; set; }
}

public class TaskStatusDistributionDto
{
    public string StatusName { get; set; } = string.Empty;
    public string? ColorHex { get; set; }
    public int Count { get; set; }
}

public class UpcomingClassDto
{
    public string Title { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public string? Room { get; set; }
}