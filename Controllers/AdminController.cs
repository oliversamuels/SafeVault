using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeVault;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{

    private readonly DataContext _context;

    public AdminController(DataContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View(_context.Users.ToList());
    }

    [Authorize(Policy = "AdminOnly")]
    public IActionResult AdminOnlyAction()
    {
        return View();
    }
}