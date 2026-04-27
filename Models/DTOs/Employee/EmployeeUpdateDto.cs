using System.ComponentModel.DataAnnotations;

namespace JuniorCodeCRM.Models.DTOs.Employee;

public class EmployeeUpdateDto
{
    [Required(ErrorMessage = "Фамилия обязательна")]
    [MaxLength(50)]
    [RegularExpression(@"^[а-яА-ЯёЁa-zA-Z\- ]+$", ErrorMessage = "Фамилия содержит недопустимые символы")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Имя обязательно")]
    [MaxLength(50)]
    [RegularExpression(@"^[а-яА-ЯёЁa-zA-Z\- ]+$", ErrorMessage = "Имя содержит недопустимые символы")]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(50)]
    [RegularExpression(@"^[а-яА-ЯёЁa-zA-Z\- ]*$", ErrorMessage = "Отчество содержит недопустимые символы")]
    public string? MiddleName { get; set; }

    [Required(ErrorMessage = "Должность обязательна")]
    public int PositionID { get; set; }

    [Required(ErrorMessage = "Отдел обязателен")]
    public int DepartmentID { get; set; }

    [Phone(ErrorMessage = "Некорректный формат телефона")]
    [MaxLength(18)]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "Некорректный формат email")]
    [MaxLength(100)]
    public string? Email { get; set; }

    public bool IsCombined { get; set; }
    public int? CombinedPositionID { get; set; }
    public DateTime? HireDate { get; set; }
    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string? Notes { get; set; }
}