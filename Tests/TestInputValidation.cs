using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

using SafeVault;
using Tests;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
[TestFixture]
public class TestInputValidation 
{
    private UserController _controller;
    private DataContext _context;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        _context = new DataContext(options);

        // Seed the database with test data
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("password"); // Use the real password here
        _context.Users.Add(new User { Username = "testuser", Email = "test@example.com", PasswordHash = hashedPassword });

        var adminPassword = BCrypt.Net.BCrypt.HashPassword("adminpassword");
        _context.Users.Add(new User { Username = "adminuser", Email = "admin@example.com", PasswordHash = adminPassword });

        _context.SaveChanges();

        _controller = new UserController(_context);
    }

    [Test]
    public void TestInvalidLogin()
    {
        // Simulate invalid login attempt
        var loginModel = new LoginModel
        {
            UsernameOrEmail = "invaliduser",
            Password = "invalidpassword"
        };

        var result = _controller.Login(loginModel).Result as ViewResult;

        // Verify that the login attempt fails
        Assert.That(result, Is.Not.Null);
        Assert.That(_controller.ModelState.IsValid, Is.False);
        Assert.That(_controller.ModelState.ContainsKey(""), Is.True);

        var errorMessage = _controller.ModelState[""]?.Errors?[0].ErrorMessage;
        Assert.That(errorMessage, Is.EqualTo("Invalid username or password"));
    }

    [Test]
    public void TestUnauthorizedAccess()
    {
        // Simulate unauthorized access attempt
        var loginModel = new LoginModel
        {
            UsernameOrEmail = "testuser",
            Password = "password"
        };

        var result = _controller.Login(loginModel).Result as ViewResult;

        // Verify that the login attempt is successful
        Assert.That(result, Is.Not.Null);
        Assert.That(_controller.ModelState.IsValid, Is.True);

        // Simulate accessing an admin-only action
        var adminResult = _controller.AdminOnlyAction() as UnauthorizedResult;

        // Verify that the access is denied
        Assert.That(adminResult, Is.Not.Null);
    }

    [Test]
    public void TestAdminAccess()
    {
        // Simulate admin login attempt
        var loginModel = new LoginModel
        {
            UsernameOrEmail = "adminuser",
            Password = "adminpassword"
        };

        var result = _controller.Login(loginModel).Result as ViewResult;

        // Verify that the login attempt is successful
        Assert.That(result, Is.Not.Null);
        Assert.That(_controller.ModelState.IsValid, Is.True);

        // Simulate authentication
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "adminuser"),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Simulate accessing an admin-only action
        var adminResult = _controller.AdminOnlyAction() as ViewResult;

        // Verify that the access is granted
        Assert.That(adminResult, Is.Not.Null);
    }

    [Test]
    public void TestMethod()
    {
        // Arrange
        var query = "test query";
        var result = _controller.Search(query) as ViewResult;

        // Act & Assert
        Assert.That(result, Is.Not.Null);
        
        var users = result.Model as List<User>;
        Assert.That(users, Is.Not.Null);
        Assert.That(users?.Count ?? 0, Is.EqualTo(0));
    }

    [Test]
    public void TestForSQLInjectionInLogin()
    {
        // Simulate SQL injection attempt
        var loginModel = new LoginModel
        {
            UsernameOrEmail = "'; DROP TABLE Users; --",
            Password = "password"
        };

        var result = _controller.Login(loginModel).Result as ViewResult;

        // Verify that the login attempt fails and does not cause harm
        Assert.That(result, Is.Not.Null);
        Assert.That(_controller.ModelState.IsValid, Is.False);
        Assert.That(_controller.ModelState.ContainsKey(""), Is.True);

        var errorMessage = _controller.ModelState[""]?.Errors?[0].ErrorMessage;
        Assert.That(errorMessage, Is.EqualTo("Invalid username or password"));
    }

    [Test]
    public void TestForSQLInjectionInSearch()
    {
        // Simulate SQL injection attempt
        var query = "'; DROP TABLE Users; --";

        var result = _controller.Search(query) as ViewResult;

        // Verify that the search attempt fails and does not cause harm
        Assert.That(result, Is.Not.Null);

        var users = result.Model as List<User>;
        Assert.That(users, Is.Not.Null);
        Assert.That(users.Count, Is.EqualTo(0));
    }

    [Test]
    public void TestForXSS()
    {
        // Placeholder for XSS test
        Assert.Pass("XSS test not implemented yet.");
    }
}