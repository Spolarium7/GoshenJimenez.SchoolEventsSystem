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
using Microsoft.EntityFrameworkCore;
using Resend;
public class Register : PageModel
{
    private readonly SchoolEventsDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly IResend _resend;
    private readonly ILogger<Register> _logger;

     public Register(SchoolEventsDbContext dbContext, IConfiguration configuration, IResend resend, ILogger<Register> logger)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _resend = resend;
        _logger = logger;
    }

    [BindProperty]
    public UserRegisterDto UserRegisterDto { get; set; } = new UserRegisterDto();

    public async Task<IActionResult> OnPost()
    {
        if(!ModelState.IsValid)
        {
            return Page();
        }

        var dateOfBirth = UserRegisterDto!.DateOfBirth!.Value;
        var today = DateTime.Today;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age))
        {
            age--;
        }

        if(age < 18)
        {
            ModelState.AddModelError("Date Of Birth", "You must be 18 years old to Register.");
            return Page();
        }

        var passwordErrors = PasswordValidator.Validate(UserRegisterDto!.Password!);
        if (passwordErrors.Any())
        {
            foreach (var error in passwordErrors)
            {
                ModelState.AddModelError("Weak Password", error);
            }

            return Page();
        }

        User? existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName!.ToLower() == UserRegisterDto!.Username!.ToLower());
        if (existingUser != null)
        {
            ModelState.AddModelError("Username", "Username is already taken.");
            return Page();
        }

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(UserRegisterDto!.Password!);

        var newUser = new User(UserRegisterDto!.Username!, UserRegisterDto!.DateOfBirth!.Value)
        {
            DateOfBirth = UserRegisterDto!.DateOfBirth!.Value,
            UserName = UserRegisterDto!.Username,
            FirstName = UserRegisterDto!.FirstName,
            LastName = UserRegisterDto!.LastName
        };

        await _dbContext.Users.AddAsync(newUser);

        var userPassword = new UserLoginInfo(newUser.Id, "password", hashedPassword);
        var userLoginStatus = new UserLoginInfo(newUser.Id, "loginstatus", "active");
        var loginRetries = new UserLoginInfo(newUser.Id, "loginretries", "0");
        var role = new UserLoginInfo(newUser.Id, "role", "user");

        await _dbContext.UserLoginInfos.AddRangeAsync(userPassword, userLoginStatus, loginRetries, role);

        await _dbContext.SaveChangesAsync();

        // A failed welcome email must not fail the registration itself.
        try
        {
            var fromAddress = _configuration["Resend:From"] ?? "onboarding@resend.dev";

            await _resend.EmailSendAsync(new EmailMessage()
            {
                From = fromAddress,
                To = newUser.UserName!,
                Subject = "Hello from School Events!",
                HtmlBody = $"<p>Welcome to School Events system, <strong>{newUser.FirstName} {newUser.LastName}</strong>!</p>",
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to {Recipient}.", newUser.UserName);
        }

        return RedirectToPage("/Account/Login");
    }
}

public class UserRegisterDto
{
    
    [Required(ErrorMessage = "Username is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string? Username { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    public string? Password { get; set; }
    
    [Required(ErrorMessage = "Confirm Password is required.")]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string? ConfirmPassword { get; set; }

    [Required(ErrorMessage = "First Name is required.")]
    public string? FirstName { get; set; }

    [Required(ErrorMessage = "Last Name is required.")]
    public string? LastName { get; set; }

    [Required(ErrorMessage = "Date of Birth is required.")]
    [DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
    public DateTime? DateOfBirth { get; set; }

}