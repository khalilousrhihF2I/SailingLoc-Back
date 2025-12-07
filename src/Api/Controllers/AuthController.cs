using System.Security.Claims;
using System.Linq;
using Api.DTOs;
using Core.Entities;
using Core.Interfaces;
using Core.Interfaces.Notifications;
using Core.Interfaces.PasswordReset;
using Core.Models.Templates;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;
[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<AppUser> _um;
    private readonly SignInManager<AppUser> _sm;
    private readonly ITokenService _tokenService;
    private readonly IFileStorageService _files;
    private readonly IConfiguration _cfg;
    private readonly IEmailService _emailService;
    private readonly IPasswordResetService _passwordResetService;

    public AuthController(
        ApplicationDbContext db,
        UserManager<AppUser> um,
        SignInManager<AppUser> sm,
        ITokenService tokenService,
        IFileStorageService files,
        IConfiguration cfg,
        IEmailService emailService,
        IPasswordResetService passwordResetService)
    {
        _db = db;
        _um = um;
        _sm = sm;
        _tokenService = tokenService;
        _files = files;
        _cfg = cfg;
        _emailService = emailService;
        _passwordResetService = passwordResetService;
    }

    [HttpPost("register"), AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken ct)
    {
        // Validate role
        var allowedRoles = new[] { "Renter", "Owner" };
        if (!allowedRoles.Contains(dto.Role, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException("Invalid role. Allowed: Renter, Owner.");

        var roleToAssign = allowedRoles
            .First(r => r.Equals(dto.Role, StringComparison.OrdinalIgnoreCase));

        if (!await _db.Roles.AnyAsync(r => r.Name == roleToAssign, ct))
            throw new InvalidOperationException($"Role '{roleToAssign}' not found in database.");

        // Create user
        var user = new AppUser
        {
            Email = dto.Email,
            UserName = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PhoneNumber = dto.PhoneNumber,
            BirthDate = dto.BirthDate,
            EmailConfirmed = true,
            UserType = roleToAssign.ToLower(),   // DB uses lowercase
            Address = new Address
            {
                Street = dto.Address.Street,
                City = dto.Address.City,
                State = dto.Address.State,
                PostalCode = dto.Address.PostalCode,
                Country = dto.Address.Country
            }
        };

        var create = await _um.CreateAsync(user, dto.Password);
        if (!create.Succeeded)
            return BadRequest(create.Errors);

        // Assign Identity role
        await _um.AddToRoleAsync(user, roleToAssign);

        // Handle avatar
        if (!string.IsNullOrWhiteSpace(dto.AvatarBase64))
        {
            var bytes = Convert.FromBase64String(dto.AvatarBase64);
            await using var ms = new MemoryStream(bytes);

            user.AvatarUrl = await _files.SaveAvatarAsync(
                ms,
                $"avatar_{user.Id}.png",
                "image/png",
                ct
            );

            await _um.UpdateAsync(user);
        }

        // Notify admins about new user registration (don't block registration on email failure)
        try
        {
            var admins = await _um.GetUsersInRoleAsync("Admin");
            var adminEmails = admins
                .Where(a => !string.IsNullOrWhiteSpace(a.Email))
                .Select(a => a.Email!)
                .ToList();

            if (adminEmails.Any())
            {
                var model = new NewUserTemplateModel
                {
                    BrandName = _cfg["Notifications:BrandName"] ?? "SailingLoc",
                    UserName = $"{user.FirstName} {user.LastName}".Trim(),
                    UserEmail = user.Email ?? string.Empty,
                    CreatedAt = DateTime.UtcNow
                };

                await _emailService.SendNewUserCreatedEmailAsync(adminEmails, model, ct);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de l'envoi de la notification de nouvel utilisateur: {ex.Message}");
        }

        return Ok(new { message = "Registered" });
    }


    [HttpPost("login"), AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
    {
        var user = await _um.Users.FirstOrDefaultAsync(u => u.Email == dto.Email, ct);
        if (user == null) return Unauthorized(new { message = "Invalid credentials" });
        var ok = await _um.CheckPasswordAsync(user, dto.Password);
        if (!ok) return Unauthorized(new { message = "Invalid credentials" });

        //var roles = await _um.GetRolesAsync(user);
        //var claims = roles.Select(r => new Claim(ClaimTypes.Role, r));
        var (access, exp) = await _tokenService.CreateAccessTokenAsync(user);
        var lifetimeDays = int.Parse(_cfg["Jwt:RefreshTokenDays"] ?? "30");
        var refresh = _tokenService.IssueRefreshToken(user, TimeSpan.FromDays(lifetimeDays));
        _db.RefreshTokens.Add(refresh);
        await _db.SaveChangesAsync(ct);

        user.LastLoginAt = DateTime.UtcNow; await _um.UpdateAsync(user);
        return Ok(new { accessToken = access, expiresAt = exp, refreshToken = refresh.Token });
    }

    [HttpPost("refresh"), AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshDto dto, CancellationToken ct)
    {
        var rt = await _db.RefreshTokens.Include(r => r.User).FirstOrDefaultAsync(r => r.Token == dto.RefreshToken, ct);
        if (rt == null || rt.Revoked || rt.ExpiresAt < DateTime.UtcNow) return Unauthorized();
        rt.Revoked = true;
        var roles = await _um.GetRolesAsync(rt.User);
        var (access, exp) = await _tokenService.CreateAccessTokenAsync(rt.User, roles.Select(r => new Claim(ClaimTypes.Role, r)));
        var newRt = _tokenService.IssueRefreshToken(rt.User, TimeSpan.FromDays(int.Parse(_cfg["Jwt:RefreshTokenDays"] ?? "30")));
        _db.RefreshTokens.Add(newRt);
        await _db.SaveChangesAsync(ct);
        return Ok(new { accessToken = access, expiresAt = exp, refreshToken = newRt.Token });
    }

    [HttpPost("logout"), Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshDto dto, CancellationToken ct)
    {
        var rt = await _db.RefreshTokens.FirstOrDefaultAsync(r => r.Token == dto.RefreshToken, ct);
        if (rt != null) { rt.Revoked = true; await _db.SaveChangesAsync(ct); }
        return Ok(new { message = "Logged out" });
    }

    [HttpPost("request-password-reset"), AllowAnonymous]
    public async Task<IActionResult> RequestPasswordReset([FromBody] ResetRequestDto dto, CancellationToken ct)
    {
        var user = await _um.Users.FirstOrDefaultAsync(u => u.Email == dto.Email, ct);
        if (user != null)
        {
            var resetToken = await _passwordResetService.CreatePasswordResetTokenAsync(user, ct);
            await _emailService.SendPasswordResetEmailAsync(user, resetToken, ct);
        }

        return Ok(new { message = "If the email exists, a reset link has been sent." });
    }

    [HttpPost("request-password-reset-code"), AllowAnonymous]
    public async Task<IActionResult> RequestPasswordResetCode([FromBody] RequestResetCodeDto dto, CancellationToken ct)
    {
        await _passwordResetService.SendResetCodeAsync(dto.Email, dto.PhoneNumber, dto.Channel, ct);
        return Ok(new { message = "If the email exists, a code has been sent." });
    }

    [HttpPost("verify-password-reset-code"), AllowAnonymous]
    public async Task<IActionResult> VerifyPasswordResetCode([FromBody] VerifyResetCodeDto dto, CancellationToken ct)
    {
        try
        {
            var resetToken = await _passwordResetService.VerifyResetCodeAndIssueTokenAsync(dto.Email, dto.Code, ct);
            return Ok(new { resetToken });
        }
        catch
        {
            return BadRequest(new { message = "Invalid or expired code" });
        }
    }

    [HttpPost("reset-password"), AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto, CancellationToken ct)
    {
        var entry = await _db.PasswordResetTokens.Include(x => x.User).FirstOrDefaultAsync(x => x.Token == dto.Token, ct);
        if (entry == null || entry.Used || entry.ExpiresAt < DateTime.UtcNow) return BadRequest(new { message = "Invalid token" });
        var token = await _um.GeneratePasswordResetTokenAsync(entry.User);
        var result = await _um.ResetPasswordAsync(entry.User, token, dto.NewPassword);
        if (!result.Succeeded) return BadRequest(result.Errors);
        entry.Used = true; await _db.SaveChangesAsync(ct);
        return Ok(new { message = "Password updated" });
    }

    [HttpGet("me"), Authorize]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (id == null) return Unauthorized();
        var guid = Guid.Parse(id);
        var user = await _um.Users.FirstOrDefaultAsync(u => u.Id == guid, ct);
        if (user == null) return NotFound();

        // Récupérer les rôles de l'utilisateur
        var roles = await _um.GetRolesAsync(user);

        // Récupérer les claims de l'utilisateur
        var userClaims = await _um.GetClaimsAsync(user);
        var claims = userClaims.Select(c => new { type = c.Type, value = c.Value }).ToArray();

        // Récupérer les claims de rôles (permissions)
        var permissions = new List<string>();
        foreach (var roleName in roles)
        {
            var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == roleName, ct);
            if (role != null)
            {
                var roleClaims = await _db.RoleClaims.Where(rc => rc.RoleId == role.Id).ToListAsync(ct);
                permissions.AddRange(roleClaims.Select(rc => rc.ClaimValue).Where(cv => !string.IsNullOrEmpty(cv)).Cast<string>());
            }
        }

        return Ok(new 
        { 
            user.Id, 
            user.Email, 
            user.FirstName, 
            user.LastName, 
            user.AvatarUrl, 
            user.Status, 
            user.LastLoginAt, 
            Address = user.Address,
            roles = roles.ToArray(),
            claims,
            permissions = permissions.Distinct().ToArray()
        });
    }

    [HttpPut("me"), Authorize]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileDto dto, CancellationToken ct)
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (id == null) return Unauthorized();
        var user = await _um.Users.FirstAsync(u => u.Id == Guid.Parse(id), ct);
        user.FirstName = dto.FirstName; user.LastName = dto.LastName;
        user.Address.Street = dto.Address.Street; user.Address.City = dto.Address.City; user.Address.State = dto.Address.State; user.Address.PostalCode = dto.Address.PostalCode; user.Address.Country = dto.Address.Country;
        user.UpdatedAt = DateTime.UtcNow;
        await _um.UpdateAsync(user);
        return Ok(new { message = "Updated" });
    }

    [HttpPost("upload-avatar")]
    [Authorize]
    [Consumes("multipart/form-data")] // <-- crucial pour OpenAPI
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UploadAvatar([FromForm] UploadAvatarDto form, CancellationToken ct)
    {
        if (form.File is null || !form.File.ContentType.StartsWith("image/"))
            return BadRequest("Invalid file");

        var id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (id is null) return Unauthorized();

        var user = await _um.FindByIdAsync(id);
        if (user is null) return Unauthorized();

        await using var stream = form.File.OpenReadStream();
        var url = await _files.SaveAvatarAsync(stream, form.File.FileName, form.File.ContentType, ct);

        user.AvatarUrl = url;
        await _um.UpdateAsync(user);

        return Ok(new { url });
    }

    [HttpGet("validate")]
    [Authorize]
    public async Task<IActionResult> ValidateToken(CancellationToken ct)
    {
        // If the request reached this controller, the token is valid per authentication middleware
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();

        return Ok(new
        {
            valid = true,
            userId = id,
            email,
            roles
        });
    }
}
