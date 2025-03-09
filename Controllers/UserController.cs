using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;
using System.Text.RegularExpressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace SafeVault;

public class UserController : Controller
{
    private readonly DataContext _context;

    public UserController(DataContext context)
    {
        _context = context;
    }

    [Authorize(Roles = "Admin")]
    public IActionResult AdminOnlyAction()
    {
        // Logic for admin-only action
        return Unauthorized();
    }

    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public IActionResult Index()
    {
        return View(_context.Users.ToList());
    }

    [HttpGet]
    public IActionResult Search(string query)
    {
        if (!string.IsNullOrEmpty(query))
        {
            // Sanitize input
            query = SanitizeInput(query);

            var results = _context.Users
                .Where(u => u.Username.Contains(query) || u.Email.Contains(query))
                .ToList();

            return View(results);
        }
        return View(new List<User>());
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(UserViewModel user)
    {
        if (ModelState.IsValid)
        {
            // Sanitize inputs
            user.Username = SanitizeInput(user.Username);
            user.Email = SanitizeInput(user.Email);

            var newUser = new User
            {
                Username = user.Username,
                Email = user.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Password),
                Role = user.isAdmin ? "Admin" : "User"
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        return View(user);
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginModel model)
    {
        if (ModelState.IsValid)
        {
            // Sanitize inputs
            model.UsernameOrEmail = SanitizeInput(model.UsernameOrEmail);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == model.UsernameOrEmail || u.Email == model.UsernameOrEmail);

            if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role ?? "User")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid username or password");
        }
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Edit(int id)
    {
        var user = _context.Users.Find(id);
        if (user == null)
        {
            return NotFound();
        }

        var userViewModel = new UserViewModel
        {
            Username = user.Username,
            Email = user.Email,
            isAdmin = user.Role == "Admin",
            Password = "",
            ConfrimPassword = ""
        };

        return View(userViewModel);
    }

    [HttpPost]
    public IActionResult Edit(User user)
    {
        if (ModelState.IsValid)
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            _context.Update(user);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        return View(user);
    }

    [Authorize(Policy = "AdminOnly")]
    public IActionResult Delete(int id)
    {
        var user = _context.Users.Find(id);
        if (user == null)
        {
            return NotFound();
        }

        _context.Users.Remove(user);
        _context.SaveChanges();
        return RedirectToAction("Index");
    }

    private string SanitizeInput(string input)
    {
        // Remove potentially harmful characters
        return Regex.Replace(input, @"[^\w\.@-]", "", RegexOptions.None, TimeSpan.FromSeconds(1.5));
    }
}