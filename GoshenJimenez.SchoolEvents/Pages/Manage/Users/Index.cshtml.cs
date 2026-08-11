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
using Microsoft.AspNetCore.Authorization;

namespace GoshenJimenez.SchoolEvents.Pages.Manage.Users;

[Authorize(Roles = "admin")]
public class Index : PageModel
{
    private readonly SchoolEventsDbContext _dbContext; 

    public Index(SchoolEventsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

}