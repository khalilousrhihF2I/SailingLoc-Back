using System.ComponentModel.DataAnnotations;
namespace Api.DTOs;
public record RegisterDto(
  [Required, EmailAddress] string Email,
  [Required, MinLength(8)] string Password,
  [Required] string FirstName,
  [Required] string LastName, 
  [Required] string Role, 
  [Required] string PhoneNumber,
  DateTime BirthDate,
  AddressDto Address,
  string? AvatarBase64
);
public record LoginDto([Required, EmailAddress] string Email, [Required] string Password);
public record RefreshDto([Required] string RefreshToken);
public record ResetRequestDto([Required, EmailAddress] string Email);
public record ResetPasswordDto([Required] string Token, [Required, MinLength(8)] string NewPassword);
public record ExternalLoginDto([Required] string Provider, string? ProviderToken, string? Code);
public record AddressDto(string Street, string City, string State, string PostalCode, string Country);
public record UpdateProfileDto(string FirstName, string LastName, AddressDto Address);
public record AssignRolesDto(List<string> Roles);
public record VerifyResetCodeDto(string Email, string Code);
public class RequestResetCodeDto
{
    public string Email { get; set; } = default!;
    // Optionnel si futur SMS
    public string? PhoneNumber { get; set; }
    // "email" (défaut) ou "sms"
    public string Channel { get; set; } = "email";
}