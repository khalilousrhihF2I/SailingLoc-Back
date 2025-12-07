using Core.Entities;
using Core.Interfaces.Notifications;
using Core.Interfaces.PasswordReset;
using Core.Interfaces.Services.Notifications;
using Core.Utilities;
using Infrastructure.Data;
using Infrastructure.Models.Templates;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Resend;
using System.Reflection.Emit;

namespace Infrastructure.Services;

public class PasswordResetService : IPasswordResetService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _um;
    private readonly IPasswordHasher<AppUser> _hasher;
    private readonly IPasswordResetCodeRepository _codes;
    private readonly IPasswordResetTokenRepository _tokens;
    private readonly IEmailSender _emailSender;
    private readonly ISmsSender _smsSender;
    private readonly ITemplateRenderer _renderer;
    private readonly IConfiguration _cfg;
    private readonly IResend _resend;

    public PasswordResetService(
            ApplicationDbContext context,
            UserManager<AppUser> um,
            IPasswordHasher<AppUser> hasher,
            IPasswordResetCodeRepository codes,
            IPasswordResetTokenRepository tokens,
            IEmailSender emailSender,
            ISmsSender smsSender,
            ITemplateRenderer renderer,
            IConfiguration cfg)
    {
        _um = um; _hasher = hasher; _codes = codes; _tokens = tokens;
        _emailSender = emailSender; _smsSender = smsSender; _renderer = renderer; _cfg = cfg;
        _context = context;
        var resendKey = Environment.GetEnvironmentVariable("RESEND_KEY")
                 ?? _cfg["Resend:dataFlow"]
                 ?? throw new Exception("Missing RESEND_KEY environment variable.");

        _resend = ResendClient.Create(resendKey);

    }

    public async Task<string> CreatePasswordResetTokenAsync(AppUser user, CancellationToken cancellationToken = default)
    {
        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

        var passwordResetToken = new PasswordResetToken
        {
            Token = token,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        _context.PasswordResetTokens.Add(passwordResetToken);
        await _context.SaveChangesAsync(cancellationToken);

        return token;
    }

    public async Task SendResetCodeAsync(string email, string? phoneNumber, string channel, CancellationToken ct)
    {
        var user = await _um.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

        // Toujours silencieux pour la confidentialité
        if (user is null) return;

        // Invalider anciens codes
        await _codes.InvalidateActiveCodesAsync(user.Id, "password-reset", ct);

        var code = VerificationCodeHelper.GenerateSixDigitCode();
        var hash = _hasher.HashPassword(user, code);

        var expiresMinutes = int.Parse(_cfg["ResetCode:ExpiresMinutes"] ?? "10");
        var model = new ResetCodeTemplateModel
        {
            BrandName = _cfg["ResetCode:BrandName"] ?? "SailingLoc",
            Code = code,
            ExpiresMinutes = expiresMinutes,
            SupportEmail = _cfg["ResetCode:SupportEmail"],
            RecipientName = $"{user.FirstName} {user.LastName}".Trim()
        };

        // Save DB
        var entry = new PasswordResetCode
        {
            UserId = user.Id,
            CodeHash = hash,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiresMinutes),
            Purpose = "password-reset"
        };
            await _codes.AddAsync(entry, ct);
        

        // Dispatch
        if (string.Equals(channel, "sms", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(phoneNumber))
        {
            var msg = _renderer.RenderResetCodeSms(model);
            await _smsSender.SendAsync(phoneNumber, msg, ct);
        }
        else
        {
            //var (subject, html, text) = _renderer.RenderResetCodeEmail(model);
            //await _emailSender.SendAsync(user.Email!, subject, html, text, ct);
            var dynamicData = new Dictionary<string, object>
                {
                    { "user_name", $"{user.FirstName} {user.LastName}".Trim() },
                    { "reset_code", code },
                    { "brand_name", "SailingLoc" },
                    { "expires_in", $"{expiresMinutes} minutes" }
                };

            // envoi via template SendGrid
            //await _emailSender.SendTemplateAsync(user.Email!, dynamicData, ct);

            await SendPasswordResetCodeEmailAsync(user, code, expiresMinutes, ct);
        }
    }
    public async Task<bool> SendPasswordResetCodeEmailAsync(
    AppUser user,
    string code,
    int expiresMinutes,
    CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(user.Email))
                throw new ArgumentNullException(nameof(user.Email), "L'email de l'utilisateur ne peut pas être null ou vide.");

            // Préparer le modèle pour le template
            var model = new ResetCodeTemplateModel
            {
                BrandName = _cfg["Resend:BrandName"] ?? "Sailing Loc By Khalil",
                RecipientName = user.FirstName,
                Code = code,
                ExpiresMinutes = expiresMinutes,
                SupportEmail = _cfg["Resend:SupportEmail"] ?? "support@sailingloc.app"
            };

            // Rendu template (renvoie : Subject, HtmlBody, TextBody)
            var (subject, html, text) = _renderer.RenderResetCodeEmail(model);

            // Construire l'email
            var emailMessage = new EmailMessage
            {
                From = EmailAddress.Parse(_cfg["Resend:FromEmail"] ?? "support@sailingloc.app"),
                To = EmailAddressList.From(user.Email),
                Subject = subject,
                HtmlBody = html,
                TextBody = text
            };

            // Envoi via Resend
            var response = await _resend.EmailSendAsync(emailMessage, cancellationToken);

            return response.Success;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de l'envoi du code de réinitialisation à {user.Email}: {ex.Message}");
            return false;
        }
    }
    public async Task<string> VerifyResetCodeAndIssueTokenAsync(string email, string code, CancellationToken ct)
    {
        var user = await _um.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        if (user is null) throw new InvalidOperationException("Invalid code");

        var maxAttempts = int.Parse(_cfg["ResetCode:MaxAttempts"] ?? "5");
        var entry = await _codes.GetLatestActiveAsync(user.Id, "password-reset", ct);
        if (entry is null || entry.Attempts >= maxAttempts) throw new InvalidOperationException("Invalid code");

        var result = _hasher.VerifyHashedPassword(user, entry.CodeHash, code);
        if (result == PasswordVerificationResult.Failed)
        {
            await _codes.IncrementAttemptsAsync(entry.Id, ct);
            throw new InvalidOperationException("Invalid code");
        }

        // Mark used
        await _codes.MarkUsedAsync(entry.Id, ct);

        // Issue opaque token for /reset-password
        var lifetimeHours = int.Parse(_cfg["ResetCode:ResetTokenHours"] ?? "1");
        var rawToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());


        
            await _tokens.AddAsync(new PasswordResetToken
            {
                Token = rawToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddHours(lifetimeHours),
                Used = false
            }, ct);
        
        return rawToken;
    }
}