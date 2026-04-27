using JuniorCodeCRM.Data;
using JuniorCodeCRM.Models.Entities;
using JuniorCodeCRM.Models.Enums;
using JuniorCodeCRM.Services.Interfaces;

namespace JuniorCodeCRM.Services;

public class AuditService : IAuditService
{
    private readonly AppDbContext _context;

    public AuditService(AppDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(ActionType actionType, EntityType entityType, int? entityId,
        string? oldValues, string? newValues, string? ipAddress)
    {
        var log = new AuditLog
        {
            ActionType = actionType.ToString(),
            EntityType = entityType.ToString(),
            EntityID = entityId,
            OldValues = oldValues,
            NewValues = newValues,
            ActionDate = DateTime.Now,
            IPAddress = ipAddress
        };

        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }
}