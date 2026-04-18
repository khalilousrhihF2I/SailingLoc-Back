using System;
using System.Threading.Tasks;

namespace Core.Interfaces;

public interface IAuditService
{
    Task LogAsync(Guid? userId, string action, string ip, string? details = null);
}
