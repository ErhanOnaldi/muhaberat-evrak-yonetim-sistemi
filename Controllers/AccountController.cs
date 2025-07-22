using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using muhaberat_evrak_yonetim.Models;
using muhaberat_evrak_yonetim.Entities;
using System.Security.Claims;

namespace muhaberat_evrak_yonetim.Controllers;

public class AccountController : Controller
{
    private readonly DataContext _context;
    private readonly ILogger<AccountController> _logger;

    public AccountController(DataContext context, ILogger<AccountController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public IActionResult Login()
    {
        // Check if user is already logged in
        if (IsUserLoggedIn())
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string username, string password, bool rememberMe = false)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ModelState.AddModelError("", "Kullanıcı adı ve şifre gereklidir.");
            return View();
        }

        var user = await _context.Users
            .Include(u => u.Department)
            .Include(u => u.Role)
            .Include(u => u.Unit)
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            ModelState.AddModelError("", "Geçersiz kullanıcı adı veya şifre.");
            return View();
        }

        // Set session data
        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("Username", user.Username);
        HttpContext.Session.SetString("UserFullName", $"{user.FirstName} {user.LastName}");
        HttpContext.Session.SetString("UserEmail", user.Email);

        if (user.DepartmentId.HasValue)
        {
            HttpContext.Session.SetInt32("DepartmentId", user.DepartmentId.Value);
            HttpContext.Session.SetString("DepartmentName", user.Department?.DepartmentName ?? "");
            HttpContext.Session.SetString("DepartmentCode", user.Department?.DepartmentCode ?? "");
            HttpContext.Session.SetString("HasFullAccess", user.Department?.HasFullAccess.ToString() ?? "false");
        }

        if (user.RoleId.HasValue)
        {
            HttpContext.Session.SetInt32("RoleId", user.RoleId.Value);
            HttpContext.Session.SetString("RoleName", user.Role?.RoleName ?? "");
            HttpContext.Session.SetString("RoleCode", user.Role?.RoleCode ?? "");
        }

        if (user.UnitId.HasValue)
        {
            HttpContext.Session.SetInt32("UnitId", user.UnitId.Value);
            HttpContext.Session.SetString("UnitName", user.Unit?.UnitName ?? "");
        }

        // Set remember me cookie if requested
        if (rememberMe)
        {
            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            };
            Response.Cookies.Append("RememberUser", user.Username, cookieOptions);
        }

        _logger.LogInformation($"User {username} logged in successfully");

        TempData["Success"] = "Başarıyla giriş yaptınız.";
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Logout()
    {
        var username = HttpContext.Session.GetString("Username");
        
        HttpContext.Session.Clear();
        
        // Remove remember me cookie
        Response.Cookies.Delete("RememberUser");
        
        _logger.LogInformation($"User {username} logged out");
        
        TempData["Success"] = "Başarıyla çıkış yaptınız.";
        return RedirectToAction("Login");
    }

    public IActionResult Register()
    {
        ViewData["Departments"] = _context.Departments
            .Where(d => d.IsActive)
            .OrderBy(d => d.DepartmentName)
            .ToList();
        
        ViewData["Roles"] = _context.Roles
            .Where(r => r.IsActive)
            .OrderBy(r => r.RoleName)
            .ToList();
        
        ViewData["Units"] = _context.Units
            .Where(u => u.IsActive)
            .OrderBy(u => u.UnitName)
            .ToList();

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(User user, string confirmPassword)
    {
        // Validate password confirmation
        if (string.IsNullOrEmpty(user.PasswordHash) || user.PasswordHash != confirmPassword)
        {
            ModelState.AddModelError("", "Şifreler eşleşmiyor.");
        }

        // Check for duplicate username
        if (await _context.Users.AnyAsync(u => u.Username == user.Username))
        {
            ModelState.AddModelError("", "Bu kullanıcı adı zaten kullanımda.");
        }

        // Check for duplicate email
        if (await _context.Users.AnyAsync(u => u.Email == user.Email))
        {
            ModelState.AddModelError("", "Bu e-posta adresi zaten kayıtlı.");
        }

        if (ModelState.IsValid)
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            user.CreatedAt = DateTime.Now;
            user.UpdatedAt = DateTime.Now;
            user.IsActive = false; // Admin approval required

            _context.Add(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Kayıt işlemi tamamlandı. Hesabınızın admin tarafından onaylanmasını bekleyin.";
            return RedirectToAction("Login");
        }

        // Reload ViewData if model is invalid
        ViewData["Departments"] = _context.Departments
            .Where(d => d.IsActive)
            .OrderBy(d => d.DepartmentName)
            .ToList();
        
        ViewData["Roles"] = _context.Roles
            .Where(r => r.IsActive)
            .OrderBy(r => r.RoleName)
            .ToList();
        
        ViewData["Units"] = _context.Units
            .Where(u => u.IsActive)
            .OrderBy(u => u.UnitName)
            .ToList();

        return View(user);
    }

    public async Task<IActionResult> Profile()
    {
        if (!IsUserLoggedIn())
        {
            return RedirectToAction("Login");
        }

        var userId = HttpContext.Session.GetInt32("UserId");
        var user = await _context.Users
            .Include(u => u.Department)
            .Include(u => u.Role)
            .Include(u => u.Unit)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return RedirectToAction("Login");
        }

        // Get user document statistics
        var stats = new
        {
            TotalSent = await _context.Documents.CountAsync(d => d.SenderUserId == userId && d.IsActive),
            TotalReceived = await _context.Documents.CountAsync(d => d.ReceiverUserId == userId && d.IsActive),
            PendingDocuments = await _context.Documents.CountAsync(d => 
                (d.SenderUserId == userId || d.ReceiverUserId == userId) && 
                d.Status != "DELIVERED" && d.Status != "RECEIVED" && d.IsActive),
            ThisMonthDocuments = await _context.Documents.CountAsync(d => 
                (d.SenderUserId == userId || d.ReceiverUserId == userId) && 
                d.CreatedDate.Month == DateTime.Now.Month && 
                d.CreatedDate.Year == DateTime.Now.Year && d.IsActive)
        };

        ViewBag.Statistics = stats;
        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(User userModel)
    {
        if (!IsUserLoggedIn())
        {
            return RedirectToAction("Login");
        }

        var userId = HttpContext.Session.GetInt32("UserId");
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            return RedirectToAction("Login");
        }

        // Check for duplicate email (excluding current user)
        if (await _context.Users.AnyAsync(u => u.Email == userModel.Email && u.Id != userId))
        {
            ModelState.AddModelError("Email", "Bu e-posta adresi zaten kayıtlı.");
        }

        if (ModelState.IsValid)
        {
            user.FirstName = userModel.FirstName;
            user.LastName = userModel.LastName;
            user.Email = userModel.Email;
            user.UpdatedAt = DateTime.Now;

            _context.Update(user);
            await _context.SaveChangesAsync();

            // Update session data
            HttpContext.Session.SetString("UserFullName", $"{user.FirstName} {user.LastName}");
            HttpContext.Session.SetString("UserEmail", user.Email);

            TempData["Success"] = "Profil bilgileriniz güncellendi.";
            return RedirectToAction("Profile");
        }

        // Reload user data
        user = await _context.Users
            .Include(u => u.Department)
            .Include(u => u.Role)
            .Include(u => u.Unit)
            .FirstOrDefaultAsync(u => u.Id == userId);

        return View("Profile", user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
    {
        if (!IsUserLoggedIn())
        {
            return RedirectToAction("Login");
        }

        if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword) || newPassword != confirmPassword)
        {
            TempData["Error"] = "Şifre bilgileri geçersiz veya yeni şifreler eşleşmiyor.";
            return RedirectToAction("Profile");
        }

        var userId = HttpContext.Session.GetInt32("UserId");
        var user = await _context.Users.FindAsync(userId);

        if (user == null || !BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
        {
            TempData["Error"] = "Mevcut şifre yanlış.";
            return RedirectToAction("Profile");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatedAt = DateTime.Now;

        _context.Update(user);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Şifreniz başarıyla değiştirildi.";
        return RedirectToAction("Profile");
    }

    public IActionResult AccessDenied()
    {
        return View();
    }

    // Helper method to check if user is logged in
    private bool IsUserLoggedIn()
    {
        return HttpContext.Session.GetInt32("UserId").HasValue;
    }

    // Helper method to get current user ID
    public static int? GetCurrentUserId(HttpContext httpContext)
    {
        return httpContext.Session.GetInt32("UserId");
    }

    // Helper method to check if current user has full access (Muhaberat)
    public static bool HasFullAccess(HttpContext httpContext)
    {
        var hasFullAccess = httpContext.Session.GetString("HasFullAccess");
        return bool.TryParse(hasFullAccess, out bool result) && result;
    }
}