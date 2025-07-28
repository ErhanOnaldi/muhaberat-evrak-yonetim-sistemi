using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using muhaberat_evrak_yonetim.Models;
using muhaberat_evrak_yonetim.Entities;

namespace muhaberat_evrak_yonetim.Controllers;

public class UserController : BaseController
{
    private readonly DataContext _context;
    private readonly ILogger<UserController> _logger;

    public UserController(DataContext context, ILogger<UserController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _context.Users
            .Include(u => u.Department)
            .Include(u => u.Role)
            .Include(u => u.Unit)
            .Where(u => u.IsActive)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToListAsync();

        return View(users);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _context.Users
            .Include(u => u.Department)
            .Include(u => u.Role)
            .Include(u => u.Unit)
            .Include(u => u.SentDocuments)
                .ThenInclude(d => d.DocumentType)
            .Include(u => u.ReceivedDocuments)
                .ThenInclude(d => d.DocumentType)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound();
        }

        // Get user statistics
        var stats = new
        {
            TotalSent = user.SentDocuments.Count(d => d.IsActive),
            TotalReceived = user.ReceivedDocuments.Count(d => d.IsActive),
            PendingDocuments = user.SentDocuments.Count(d => d.IsActive && d.Status != "DELIVERED" && d.Status != "RECEIVED"),
            ThisMonthDocuments = user.SentDocuments.Count(d => d.IsActive && 
                d.CreatedDate.Month == DateTime.Now.Month && d.CreatedDate.Year == DateTime.Now.Year)
        };

        ViewBag.Statistics = stats;
        return View(user);
    }

    public async Task<IActionResult> Create()
    {
        // Sadece Muhaberat departmanından olanlar yeni kullanıcı ekleyebilir
        var currentUserDepartment = GetCurrentUserDepartment();
        if (currentUserDepartment != "Muhaberat")
        {
            TempData["Error"] = "Yeni kullanıcı ekleme yetkiniz bulunmamaktadır.";
            return RedirectToAction("Index");
        }

        ViewData["Departments"] = await _context.Departments
            .Where(d => d.IsActive)
            .OrderBy(d => d.DepartmentName)
            .ToListAsync();
        
        ViewData["Roles"] = await _context.Roles
            .Where(r => r.IsActive)
            .OrderBy(r => r.RoleName)
            .ToListAsync();
        
        ViewData["Units"] = await _context.Units
            .Where(u => u.IsActive)
            .OrderBy(u => u.UnitName)
            .ToListAsync();

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(User user, string confirmPassword)
    {
        // Sadece Muhaberat departmanından olanlar yeni kullanıcı ekleyebilir
        var currentUserDepartment = GetCurrentUserDepartment();
        if (currentUserDepartment != "Muhaberat")
        {
            TempData["Error"] = "Yeni kullanıcı ekleme yetkiniz bulunmamaktadır.";
            return RedirectToAction("Index");
        }

        if (string.IsNullOrEmpty(user.PasswordHash) || user.PasswordHash != confirmPassword)
        {
            ModelState.AddModelError("", "Şifreler eşleşmiyor.");
        }

        if (await _context.Users.AnyAsync(u => u.Username == user.Username))
        {
            ModelState.AddModelError("Username", "Bu kullanıcı adı zaten kullanımda.");
        }

        if (await _context.Users.AnyAsync(u => u.Email == user.Email))
        {
            ModelState.AddModelError("Email", "Bu e-posta adresi zaten kayıtlı.");
        }

        if (ModelState.IsValid)
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            user.CreatedAt = DateTime.Now;
            user.UpdatedAt = DateTime.Now;
            user.IsActive = true;

            _context.Add(user);
            await _context.SaveChangesAsync();
            
            TempData["Success"] = "Kullanıcı başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Index));
        }

        ViewData["Departments"] = await _context.Departments
            .Where(d => d.IsActive)
            .OrderBy(d => d.DepartmentName)
            .ToListAsync();
        
        ViewData["Roles"] = await _context.Roles
            .Where(r => r.IsActive)
            .OrderBy(r => r.RoleName)
            .ToListAsync();
        
        ViewData["Units"] = await _context.Units
            .Where(u => u.IsActive)
            .OrderBy(u => u.UnitName)
            .ToListAsync();

        return View(user);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        ViewData["Departments"] = await _context.Departments
            .Where(d => d.IsActive)
            .OrderBy(d => d.DepartmentName)
            .ToListAsync();
        
        ViewData["Roles"] = await _context.Roles
            .Where(r => r.IsActive)
            .OrderBy(r => r.RoleName)
            .ToListAsync();
        
        ViewData["Units"] = await _context.Units
            .Where(u => u.IsActive)
            .OrderBy(u => u.UnitName)
            .ToListAsync();

        var userToEdit = new User
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            DepartmentId = user.DepartmentId,
            RoleId = user.RoleId,
            UnitId = user.UnitId,
            IsActive = user.IsActive,
            PasswordHash = ""
        };

        return View(userToEdit);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, User user)
    {
        if (id != user.Id)
        {
            return NotFound();
        }

        if (await _context.Users.AnyAsync(u => u.Username == user.Username && u.Id != id))
        {
            ModelState.AddModelError("Username", "Bu kullanıcı adı zaten kullanımda.");
        }

        if (await _context.Users.AnyAsync(u => u.Email == user.Email && u.Id != id))
        {
            ModelState.AddModelError("Email", "Bu e-posta adresi zaten kayıtlı.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                var existingUser = await _context.Users.FindAsync(id);
                if (existingUser == null)
                {
                    return NotFound();
                }

                existingUser.Username = user.Username;
                existingUser.Email = user.Email;
                existingUser.FirstName = user.FirstName;
                existingUser.LastName = user.LastName;
                existingUser.DepartmentId = user.DepartmentId;
                existingUser.RoleId = user.RoleId;
                existingUser.UnitId = user.UnitId;
                existingUser.IsActive = user.IsActive;
                existingUser.UpdatedAt = DateTime.Now;

                if (!string.IsNullOrEmpty(user.PasswordHash))
                {
                    existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
                }

                _context.Update(existingUser);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Kullanıcı bilgileri başarıyla güncellendi.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(user.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }

        ViewData["Departments"] = await _context.Departments
            .Where(d => d.IsActive)
            .OrderBy(d => d.DepartmentName)
            .ToListAsync();
        
        ViewData["Roles"] = await _context.Roles
            .Where(r => r.IsActive)
            .OrderBy(r => r.RoleName)
            .ToListAsync();
        
        ViewData["Units"] = await _context.Units
            .Where(u => u.IsActive)
            .OrderBy(u => u.UnitName)
            .ToListAsync();

        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        user.IsActive = false;
        user.UpdatedAt = DateTime.Now;

        _context.Update(user);
        await _context.SaveChangesAsync();
        
        TempData["Success"] = "Kullanıcı deaktif edildi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        user.IsActive = true;
        user.UpdatedAt = DateTime.Now;

        _context.Update(user);
        await _context.SaveChangesAsync();
        
        TempData["Success"] = "Kullanıcı aktif edildi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> MyDocuments(int? userId)
    {
        if (userId == null)
        {
            userId = GetCurrentUserIdRequired();
        }

        var user = await _context.Users
            .Include(u => u.Department)
            .Include(u => u.Unit)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound();
        }

        var sentDocuments = await _context.Documents
            .Include(d => d.DocumentType)
            .Include(d => d.ReceiverUser)
            .Include(d => d.ReceiverDepartment)
            .Where(d => d.SenderUserId == userId && d.IsActive)
            .OrderByDescending(d => d.CreatedDate)
            .ToListAsync();

        var receivedDocuments = await _context.Documents
            .Include(d => d.DocumentType)
            .Include(d => d.SenderUser)
            .Include(d => d.SenderDepartment)
            .Where(d => (d.ReceiverUserId == userId || 
                        (user.DepartmentId.HasValue && d.ReceiverDepartmentId == user.DepartmentId)) && d.IsActive)
            .OrderByDescending(d => d.CreatedDate)
            .ToListAsync();

        ViewBag.User = user;
        ViewBag.SentDocuments = sentDocuments;
        ViewBag.ReceivedDocuments = receivedDocuments;

        return View();
    }

    public async Task<IActionResult> Statistics(int? userId)
    {
        if (userId == null)
        {
            userId = GetCurrentUserIdRequired();
        }

        var user = await _context.Users
            .Include(u => u.Department)
            .Include(u => u.Unit)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound();
        }

        var stats = new
        {
            TotalSent = await _context.Documents.CountAsync(d => d.SenderUserId == userId && d.IsActive),
            TotalReceived = await _context.Documents.CountAsync(d => 
                (d.ReceiverUserId == userId || 
                 (user.DepartmentId.HasValue && d.ReceiverDepartmentId == user.DepartmentId)) && d.IsActive),
            PendingDocuments = await _context.Documents.CountAsync(d => 
                (d.SenderUserId == userId || d.ReceiverUserId == userId) && 
                d.Status != "DELIVERED" && d.Status != "RECEIVED" && d.IsActive),
            DeliveredDocuments = await _context.Documents.CountAsync(d => 
                (d.SenderUserId == userId || d.ReceiverUserId == userId) && 
                (d.Status == "DELIVERED" || d.Status == "RECEIVED") && d.IsActive),
            ThisMonthDocuments = await _context.Documents.CountAsync(d => 
                (d.SenderUserId == userId || d.ReceiverUserId == userId) && 
                d.CreatedDate.Month == DateTime.Now.Month && 
                d.CreatedDate.Year == DateTime.Now.Year && d.IsActive),
            DraftDocuments = await _context.Documents.CountAsync(d => 
                d.SenderUserId == userId && d.Status == "DRAFT" && d.IsActive)
        };

        ViewBag.User = user;
        ViewBag.Statistics = stats;

        return View();
    }

    public async Task<IActionResult> ByDepartment(int departmentId)
    {
        var department = await _context.Departments
            .Include(d => d.Unit)
            .FirstOrDefaultAsync(d => d.Id == departmentId);

        if (department == null)
        {
            return NotFound();
        }

        var users = await _context.Users
            .Include(u => u.Role)
            .Where(u => u.DepartmentId == departmentId && u.IsActive)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToListAsync();

        ViewBag.Department = department;
        return View(users);
    }

    public async Task<IActionResult> ByRole(int roleId)
    {
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == roleId);

        if (role == null)
        {
            return NotFound();
        }

        var users = await _context.Users
            .Include(u => u.Department)
            .Include(u => u.Unit)
            .Where(u => u.RoleId == roleId && u.IsActive)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToListAsync();

        ViewBag.Role = role;
        return View(users);
    }

    private bool UserExists(int id)
    {
        return _context.Users.Any(e => e.Id == id && e.IsActive);
    }
}