using System.Security.Claims;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/data-reset")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[ApiExplorerSettings(IgnoreApi = true)] // Hidden from Swagger
public class DataResetController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public DataResetController(ApplicationDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Reset all transactional data but keep: Destinations, Boats, BoatImages, Reviews,
    /// AspNet Roles, UserRoles, and the three test users.
    /// </summary>
    [HttpPost("reset")]
    public async Task<IActionResult> ResetData(CancellationToken ct)
    {
        var testEmails = new[]
        {
            "admin@local.test",
            "owner@local.test",
            "renter@local.test"
        };

        // Get IDs of test users to preserve
        var testUserIds = await _db.Users
            .Where(u => testEmails.Contains(u.NormalizedEmail!.ToLower()))
            .Select(u => u.Id)
            .ToListAsync(ct);

        // 1. Delete Disputes
        var disputes = await _db.Disputes.ToListAsync(ct);
        _db.Disputes.RemoveRange(disputes);

        // 2. Delete Messages
        var messages = await _db.Messages.ToListAsync(ct);
        _db.Messages.RemoveRange(messages);

        // 3. Delete Bookings (this also cascades BoatAvailabilities if tied)
        var bookings = await _db.Bookings.ToListAsync(ct);
        _db.Bookings.RemoveRange(bookings);

        // 4. Delete BoatAvailabilities
        var availabilities = await _db.BoatAvailabilities.ToListAsync(ct);
        _db.BoatAvailabilities.RemoveRange(availabilities);

        // 5. Delete BoatPricePeriods
        var pricePeriods = await _db.BoatPricePeriods.ToListAsync(ct);
        _db.BoatPricePeriods.RemoveRange(pricePeriods);

        // 6. Delete UserDocuments
        var userDocs = await _db.UserDocuments.ToListAsync(ct);
        _db.UserDocuments.RemoveRange(userDocs);

        // 7. Delete AuditLogs
        var auditLogs = await _db.AuditLogs.ToListAsync(ct);
        _db.AuditLogs.RemoveRange(auditLogs);

        // 8. Delete Profiles (not for test users)
        var profiles = await _db.Profiles
            .Where(p => !testUserIds.Contains(p.UserId))
            .ToListAsync(ct);
        _db.Profiles.RemoveRange(profiles);

        // 9. Delete RefreshTokens
        var refreshTokens = await _db.RefreshTokens.ToListAsync(ct);
        _db.RefreshTokens.RemoveRange(refreshTokens);

        // 10. Delete PasswordResetTokens
        var passwordResetTokens = await _db.PasswordResetTokens.ToListAsync(ct);
        _db.PasswordResetTokens.RemoveRange(passwordResetTokens);

        // 11. Delete PasswordResetCodes
        var passwordResetCodes = await _db.PasswordResetCodes.ToListAsync(ct);
        _db.PasswordResetCodes.RemoveRange(passwordResetCodes);

        // 12. Delete ExternalLogins (not for test users)
        var externalLogins = await _db.ExternalLogins
            .Where(e => !testUserIds.Contains(e.UserId))
            .ToListAsync(ct);
        _db.ExternalLogins.RemoveRange(externalLogins);

        // 13. Delete non-test users (keep test users, keep their roles)
        // First remove UserRoles for non-test users
        var nonTestUserRoles = await _db.UserRoles
            .Where(ur => !testUserIds.Contains(ur.UserId))
            .ToListAsync(ct);
        _db.UserRoles.RemoveRange(nonTestUserRoles);

        // Delete UserClaims for non-test users
        var nonTestUserClaims = await _db.UserClaims
            .Where(uc => !testUserIds.Contains(uc.UserId))
            .ToListAsync(ct);
        _db.UserClaims.RemoveRange(nonTestUserClaims);

        // Delete UserLogins for non-test users
        var nonTestUserLogins = await _db.UserLogins
            .Where(ul => !testUserIds.Contains(ul.UserId))
            .ToListAsync(ct);
        _db.UserLogins.RemoveRange(nonTestUserLogins);

        // Delete UserTokens for non-test users
        var nonTestUserTokens = await _db.UserTokens
            .Where(ut => !testUserIds.Contains(ut.UserId))
            .ToListAsync(ct);
        _db.UserTokens.RemoveRange(nonTestUserTokens);

        // Delete non-test users themselves
        var nonTestUsers = await _db.Users
            .Where(u => !testUserIds.Contains(u.Id))
            .ToListAsync(ct);
        _db.Users.RemoveRange(nonTestUsers);

        await _db.SaveChangesAsync(ct);

        // Log the reset action
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        _db.AuditLogs.Add(new Core.Entities.AuditLog
        {
            Action = "DATA_RESET",
            UserId = Guid.TryParse(userId, out var uid) ? uid : null,
            Ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            Details = $"Full data reset performed. Preserved {testUserIds.Count} test users, all boats, destinations, reviews, and roles."
        });
        await _db.SaveChangesAsync(ct);

        return Ok(new
        {
            message = "Data reset completed successfully.",
            preserved = new
            {
                testUsers = testUserIds.Count,
                note = "Destinations, Boats, BoatImages, Reviews, Roles, and test users were kept."
            }
        });
    }
}
