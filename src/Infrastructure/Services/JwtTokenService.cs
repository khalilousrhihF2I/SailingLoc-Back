using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Services;
public class JwtTokenService : ITokenService
{
    private readonly IConfiguration _cfg;
    private readonly UserManager<AppUser> _um;
    private readonly RoleManager<AppRole> _rm;
    public JwtTokenService(UserManager<AppUser> um, IConfiguration cfg, RoleManager<AppRole> rm) { _cfg = cfg; _um = um; _rm = rm; }


    public async Task<(string accessToken, DateTime expiresAt)> CreateAccessTokenAsync(AppUser user, IEnumerable<Claim>? extraClaims = null)
    {
        var issuer = _cfg["Jwt:Issuer"];
        var audience = _cfg["Jwt:Audience"];
        var signingKey = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY")
                           ??_cfg["Jwt:SigningKey"]
                           ?? throw new Exception("JWT_SIGNING_KEY is missing");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(int.Parse(_cfg["Jwt:AccessTokenMinutes"] ?? "15"));

        var claims = new List<Claim>
           {
               new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
               new(ClaimTypes.NameIdentifier, user.Id.ToString()),
               new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
               new("name", $"{user.FirstName} {user.LastName}".Trim())
           };

        // User claims
        var userClaims = await _um.GetClaimsAsync(user);
        claims.AddRange(userClaims);

        // Roles + role claims
        var roles = await _um.GetRolesAsync(user);
        foreach (var roleName in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, roleName));
            var role = await _rm.FindByNameAsync(roleName);
            IList<Claim> roleClaims = await _rm.GetClaimsAsync(role);
            claims.AddRange(roleClaims);
        }

        // Extra claims
        if (extraClaims != null) claims.AddRange(extraClaims);

        // Generate JWT
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return (jwt, expires);
    }

 
    public RefreshToken IssueRefreshToken(AppUser user, TimeSpan lifetime)
    {
        return new RefreshToken
        {
            User = user,
            UserId = user.Id,
            Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
            ExpiresAt = DateTime.UtcNow.Add(lifetime),
            Revoked = false
        };
    }
}
