using Core.Entities;
using System.Security.Claims;
namespace Core.Interfaces;
public interface ITokenService
{

    Task<(string accessToken, DateTime expiresAt)> CreateAccessTokenAsync(AppUser user, IEnumerable<Claim>? extraClaims = null); 
    RefreshToken IssueRefreshToken(AppUser user, TimeSpan lifetime);
}
