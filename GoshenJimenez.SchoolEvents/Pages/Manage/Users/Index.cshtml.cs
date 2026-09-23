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
using Microsoft.EntityFrameworkCore;

namespace GoshenJimenez.SchoolEvents.Pages.Manage.Users;

public class Index : PageModel
{
    private readonly SchoolEventsDbContext _dbContext; 
    public List<User> Users { get; set; }
    public string? Keyword { get; set; }
    public int? PageIndex { get; set; }
    public int? PageSize { get; set; }
    public int? TotalPages { get; set; }
    public int? TotalRecords { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
    public Boolean? ShowDeletedUsers { get; set; } = false;
    public Index(SchoolEventsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task OnGet(int? pageIndex = 1, int? pageSize = 10, string? sortBy="username", string? sortOrder="asc", string? keyword="", Boolean? isDeleted = false)
    {
        await ShowRecords(pageIndex, pageSize, sortBy, sortOrder, keyword, isDeleted);
    }

    public async Task<IActionResult> ShowRecords(int? pageIndex = 1, int? pageSize = 10, string? sortBy="username", string? sortOrder="asc", string? keyword="", Boolean? isDeleted = false)
    {
        keyword = keyword?.ToLower() ?? "";

        IQueryable<User> query = _dbContext.Users;

        if(!string.IsNullOrEmpty(keyword)){
            query = query.Where(a => a.UserName!.ToLower().Contains(keyword ?? ""));
            Keyword = keyword;
        }

        if(isDeleted.HasValue){
            query = query.Where(a => a.IsDeleted == isDeleted);
        }

        sortBy = sortBy?.ToLower() ?? "username";
        sortOrder = sortOrder?.ToLower() ?? "asc";
        
        if(sortBy == "username" && sortOrder == "asc")
        {
            query = query.OrderBy(s => s.UserName);
        }
        else if(sortBy == "username" && sortOrder == "desc")
        {
            query = query.OrderByDescending(s => s.UserName);            
        }

        var skip = ((pageIndex ?? 1) - 1) * (pageSize ?? 10);
        var take = pageSize ?? 10;

        query = query.Skip(skip).Take(take);
        Users = await query.ToListAsync();
        Keyword = keyword;
        PageIndex = pageIndex;
        PageSize = pageSize;
        TotalRecords = await _dbContext.Users.CountAsync();
        TotalPages = (int)Math.Ceiling(TotalRecords.Value / (double)(pageSize ?? 10));
        SortBy = sortBy;
        SortOrder = sortOrder;  
        ShowDeletedUsers = isDeleted;
        return Page();
    }
    public async Task<IActionResult> OnPostDeleteUser(Guid? userId)
    {
        Console.WriteLine($"Deleting user with ID: {userId}");

        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound();
        }
        user.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
        Console.WriteLine($"Deleted user with ID: {userId}");
        return await ShowRecords(1, 10, "username", "asc", "", false);
    }
}