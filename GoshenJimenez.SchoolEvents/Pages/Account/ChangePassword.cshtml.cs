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
public class ChangePassword : PageModel
{  

    private readonly SchoolEventsDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;

    public ChangePassword(SchoolEventsDbContext dbContext, IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _environment = environment;
    }

    [BindProperty]
    public ChangePasswordDto ChangePasswordDto { get; set; } = new ChangePasswordDto();

    public IActionResult OnGet()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return RedirectToPage("/Account/Login");
        }

        var user = _dbContext.Users.FirstOrDefault(u => u.Id.ToString() == userId);
        if (user == null)
        {
            return NotFound();
        }

        ChangePasswordDto.Username = user.UserName;

        var avatarDiskPath = Path.Combine(_environment.WebRootPath, "users", $"{user.Id}.png");
        if (System.IO.File.Exists(avatarDiskPath))
        {
            ChangePasswordDto.ProfileImage = $"/users/{user.Id}.png";
        }
        else
        {
            ChangePasswordDto.ProfileImage = "/users/default.png";// Path to the default profile image
        }
        return Page();
    }

    public IActionResult OnPost()
    {
        if(!ModelState.IsValid)
        {
            return Page();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return RedirectToPage("/Account/Login");
        }

        var user = _dbContext.Users.FirstOrDefault(u => u.Id.ToString() == userId);
        if (user == null)
        {
            return NotFound();
        }

        var passwordErrors = PasswordValidator.Validate(ChangePasswordDto!.NewPassword!);
        if (passwordErrors.Any())
        {
            foreach (var error in passwordErrors)
            {
                ModelState.AddModelError("Weak Password", error);
            }

            return Page();
        }

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(ChangePasswordDto!.NewPassword!);

        var userPassword = _dbContext.UserLoginInfos.FirstOrDefault(u => u.UserId == user.Id && u.Key == "password");
        if (userPassword != null)
        {
            userPassword.Value = hashedPassword;
        }
        else
        {
            userPassword = new UserLoginInfo(user.Id, "password", hashedPassword);
            _dbContext.UserLoginInfos.Add(userPassword);
        }

        _dbContext.SaveChanges();
        return RedirectToPage("/Account/Profile");
    }

}

public class ChangePasswordDto
{    
    public string? Username { get; set; }

    [Required(ErrorMessage = "Current Password is required.")]
    public string? CurrentPassword { get; set; }
    
    [Required(ErrorMessage = "New Password is required.")]
    public string? NewPassword { get; set; }

    [Required(ErrorMessage = "Confirm Password is required.")]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
    public string? ConfirmNewPassword { get; set; }

    public string? ProfileImage { get; set; }

}