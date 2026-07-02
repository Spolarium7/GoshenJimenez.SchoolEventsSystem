using System.Diagnostics;
using BCrypt.Net;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml.Schema;
using System.Runtime.ExceptionServices;
public class Login : PageModel
{    
    private readonly SchoolEventsDbContext _dbContext; 

     public Login(SchoolEventsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [BindProperty]
    public UserLoginDto UserLoginDto { get; set; } = new UserLoginDto();

    public void OnPost()
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

        if(BCrypt.Net.BCrypt.EnhancedVerify(UserLoginDto.Password, password!.Value))
        {
            //Login Success
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