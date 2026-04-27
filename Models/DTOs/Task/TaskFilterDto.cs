namespace JuniorCodeCRM.Models.DTOs.Task;

public class TaskFilterDto
{
    public string? Search { get; set; }              // Поиск по названию
    public int? StatusID { get; set; }               // Фильтр по статусу
    public int? PriorityID { get; set; }             // Фильтр по приоритету
    public int? AssigneeID { get; set; }             // Фильтр по исполнителю
    public string? BoardColumn { get; set; }         // Фильтр по колонке доски
    public DateTime? DeadlineFrom { get; set; }      // Дедлайн с
    public DateTime? DeadlineTo { get; set; }        // Дедлайн по

    // Пагинация
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}