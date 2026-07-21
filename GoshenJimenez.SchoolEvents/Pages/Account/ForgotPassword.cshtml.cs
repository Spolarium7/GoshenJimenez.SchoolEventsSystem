using System.Diagnostics;
using BCrypt.Net;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml.Schema;
using System.Runtime.ExceptionServices;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using GoshenJimenez.SchoolEvents.Infrastructure.Helpers;
using Resend;
public class ForgotPassword : PageModel
{    
    private readonly SchoolEventsDbContext _dbContext;
    private readonly UserTokenService _userTokenService;
    private readonly IConfiguration _configuration;
    private readonly IResend _resend;
    private readonly ILogger<Invite> _logger;
     public ForgotPassword(SchoolEventsDbContext dbContext, UserTokenService userTokenService, IConfiguration configuration, IResend resend, ILogger<Invite> logger)
    {
        _dbContext = dbContext;
        _userTokenService = userTokenService;
        _configuration = configuration;
        _logger = logger;
        _resend = resend;
    }

    [BindProperty]
    public ForgotPasswordDto ForgotPasswordDto { get; set; } = new ForgotPasswordDto();

    public string? ResetToken { get; private set; }

    public async Task OnPost()
    {
        if(!ModelState.IsValid)
        {
            return;
        }

        var user = _dbContext.Users.FirstOrDefault( u => 
                            u.UserName!= null 
                        &&  u.UserName!.ToLower() == ForgotPasswordDto!.Username!.ToLower());

        if(user == null)
        {
            ModelState.AddModelError("","We have sent you an email with link to reset your password.");
            return;
        }
        else
        {
            var resetToken = _userTokenService.CreatePasswordResetToken(user.Id!.Value);
            ResetToken = resetToken;

            var resetUrl = $"http://localhost:5257/account/verify-forgot-password?token={resetToken}";

            try
            {
                var fromAddress = _configuration["Resend:From"] ?? "onboarding@resend.dev";

                await _resend.EmailSendAsync(new EmailMessage()
                {
                    From = fromAddress,
                    To = user.UserName!,
                    Subject = "Hello from School Events!",
                    HtmlBody = $@"
                                <p>You requested to reset your password!</p>
                                <p>You can reset your password by clicking the link below:</p>
                                <p><a href='{resetUrl}'>Reset Password</a></p>"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send forgot password email to {Recipient}.", user.UserName);
            }

            // TODO: send this token to the user over email as a password reset link.
            ModelState.AddModelError("","We have sent you an email with link to reset your password.");
            return;
        }

    }

}

public class ForgotPasswordDto
{
    [Required(ErrorMessage = "Please input your email address.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string? Username { get; set; }
}