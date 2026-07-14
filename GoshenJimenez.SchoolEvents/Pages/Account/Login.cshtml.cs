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
public class Login : PageModel
{    
    private readonly SchoolEventsDbContext _dbContext; 

     public Login(SchoolEventsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [BindProperty]
    public UserLoginDto UserLoginDto { get; set; } = new UserLoginDto();

    public async Task OnPost()
    {
        if(!ModelState.IsValid)
        {
            return;
        }

        var user = _dbContext.Users.FirstOrDefault( u => 
                            u.UserName!= null 
                        &&  u.UserName!.ToLower() == UserLoginDto!.Username!.ToLower());

        if(user == null)
        {
            ModelState.AddModelError("","Invalid Login");
            return;
        }

        //user.Id
        var loginStatus = _dbContext.UserLoginInfos.FirstOrDefault( ls => 
                            ls.UserId == user.Id
                         && ls!.Key!.ToLower() == "loginstatus"
        );

        if(loginStatus == null || loginStatus.Value!.ToLower() != "active")
        {
            ModelState.AddModelError("","Your account is LockedOut, have an Admin unlock your account first");
            return; 
        }


        var password = _dbContext.UserLoginInfos.FirstOrDefault( ls => 
                            ls.UserId == user.Id
                         && ls!.Key!.ToLower() == "password"
        );

        if(password == null)
        {
            ModelState.AddModelError("","Invalid Login");
            return; 
        }

        if(BCrypt.Net.BCrypt.Verify(UserLoginDto.Password, password!.Value))
        {
            //Login Success
            if(loginStatus != null)
            {
                loginStatus.Value = "Active";
            }
            else
            {
                loginStatus = new UserLoginInfo(user.Id, "loginstatus", "Active");
                _dbContext.UserLoginInfos.Add(loginStatus);
            }

            _dbContext.SaveChanges();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName ?? user.FirstName!),
                new Claim(ClaimTypes.NameIdentifier, user.Id!.ToString()!)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // 2. Sign the user in
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme, 
                new ClaimsPrincipal(claimsIdentity)
            );

            // 3. Optional: Store temporary server-side information in Session
            HttpContext.Session.SetString("UserLoginTime", DateTime.UtcNow.ToString());
            RedirectToPage("/Index");
            return;
        }

        //Login Failed
        var loginRetries = _dbContext.UserLoginInfos.FirstOrDefault( lr => 
                            lr.UserId == user.Id
                         && lr!.Key!.ToLower() == "loginretries"
        );

        if(loginRetries == null)
        {
            loginRetries = new UserLoginInfo(user.Id, "loginretries", "1");
            _dbContext.UserLoginInfos.Add(loginRetries);
            _dbContext.SaveChanges();
        }
        else
        {
            loginRetries.Value = (int.Parse(loginRetries.Value ?? "0") + 1).ToString();
            _dbContext.SaveChanges();
        }

        if(int.Parse(loginRetries.Value ?? "0") >= 3)
        {
            if(loginStatus != null)
            {
                loginStatus.Value = "LockedOut";
            }
            else
            {
                loginStatus = new UserLoginInfo(user.Id, "loginstatus", "LockedOut");
                _dbContext.UserLoginInfos.Add(loginStatus);
            }

            _dbContext.SaveChanges();
        }

        ModelState.AddModelError("","Invalid Login");
    }

}

public class UserLoginDto
{
    [Required(ErrorMessage = "Invalid login.")]
    public string? Username { get; set; }

    [Required(ErrorMessage = "Invalid login.")]
    public string? Password { get; set; }
}