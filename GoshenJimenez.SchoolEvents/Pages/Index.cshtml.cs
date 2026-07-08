using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GoshenJimenez.SchoolEvents.Pages;

public class IndexModel : PageModel
{
    public void OnGet()
    {
        var sadTitleDefense = new SchoolEvent("SAD Title Defense", new DateTime(2024, 6, 30), 3);
    }
}


