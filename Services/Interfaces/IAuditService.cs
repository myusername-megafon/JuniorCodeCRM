using JuniorCodeCRM.Models.Enums;

namespace JuniorCodeCRM.Services.Interfaces;

public interface IAuditService
{
    Task LogAsync(ActionType actionType, EntityType entityType, int? entityId,
        string? oldValues, string? newValues, string? ipAddress);
}