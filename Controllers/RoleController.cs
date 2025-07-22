using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using muhaberat_evrak_yonetim.Models;
using muhaberat_evrak_yonetim.Entities;

namespace muhaberat_evrak_yonetim.Controllers;

public class RoleController : Controller
{
    private readonly DataContext _context;
    private readonly ILogger<RoleController> _logger;

    public RoleController(DataContext context, ILogger<RoleController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var roles = await _context.Roles
            .Where(r => r.IsActive)
            .OrderBy(r => r.RoleName)
            .ToListAsync();

        return View(roles);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var role = await _context.Roles
            .Include(r => r.Users.Where(u => u.IsActive))
                .ThenInclude(u => u.Department)
            .Include(r => r.DocumentPermissions)
                .ThenInclude(dp => dp.DocumentType)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null)
        {
            return NotFound();
        }

        // Get role statistics
        var stats = new
        {
            TotalUsers = role.Users.Count,
            TotalPermissions = role.DocumentPermissions.Count,
            DocumentTypesWithAccess = role.DocumentPermissions.Select(dp => dp.DocumentTypeId).Distinct().Count(),
            UsersWithUploadPermission = role.DocumentPermissions.Count(dp => dp.CanUpload),
            UsersWithApprovePermission = role.DocumentPermissions.Count(dp => dp.CanApprove)
        };

        ViewBag.Statistics = stats;
        return View(role);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Role role)
    {
        // Check for duplicate role code
        if (await _context.Roles.AnyAsync(r => r.RoleCode == role.RoleCode))
        {
            ModelState.AddModelError("RoleCode", "Bu rol kodu zaten kullanımda.");
        }

        // Check for duplicate role name
        if (await _context.Roles.AnyAsync(r => r.RoleName == role.RoleName))
        {
            ModelState.AddModelError("RoleName", "Bu rol adı zaten kullanımda.");
        }

        if (ModelState.IsValid)
        {
            role.CreatedAt = DateTime.Now;
            role.IsActive = true;

            _context.Add(role);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Rol başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Index));
        }

        return View(role);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var role = await _context.Roles.FindAsync(id);
        if (role == null)
        {
            return NotFound();
        }

        return View(role);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Role role)
    {
        if (id != role.Id)
        {
            return NotFound();
        }

        // Check for duplicate role code (excluding current role)
        if (await _context.Roles.AnyAsync(r => r.RoleCode == role.RoleCode && r.Id != id))
        {
            ModelState.AddModelError("RoleCode", "Bu rol kodu zaten kullanımda.");
        }

        // Check for duplicate role name (excluding current role)
        if (await _context.Roles.AnyAsync(r => r.RoleName == role.RoleName && r.Id != id))
        {
            ModelState.AddModelError("RoleName", "Bu rol adı zaten kullanımda.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                var existingRole = await _context.Roles.FindAsync(id);
                if (existingRole == null)
                {
                    return NotFound();
                }

                existingRole.RoleName = role.RoleName;
                existingRole.RoleCode = role.RoleCode;
                existingRole.Description = role.Description;
                existingRole.IsActive = role.IsActive;

                _context.Update(existingRole);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Rol bilgileri başarıyla güncellendi.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoleExists(role.Id))
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

        return View(role);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null)
        {
            return NotFound();
        }

        // Check if role has active users
        var hasActiveUsers = await _context.Users.AnyAsync(u => u.RoleId == id && u.IsActive);
        if (hasActiveUsers)
        {
            TempData["Error"] = "Bu rolde aktif kullanıcılar bulunduğu için deaktif edilemez. Önce kullanıcıları başka rollere taşıyın.";
            return RedirectToAction(nameof(Index));
        }

        role.IsActive = false;
        _context.Update(role);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Rol deaktif edildi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(int id)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null)
        {
            return NotFound();
        }

        role.IsActive = true;
        _context.Update(role);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Rol aktif edildi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Users(int id)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == id);
        if (role == null)
        {
            return NotFound();
        }

        var users = await _context.Users
            .Include(u => u.Department)
                .ThenInclude(d => d.Unit)
            .Where(u => u.RoleId == id && u.IsActive)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToListAsync();

        ViewBag.Role = role;
        return View(users);
    }

    public async Task<IActionResult> Permissions(int id)
    {
        var role = await _context.Roles
            .Include(r => r.DocumentPermissions)
                .ThenInclude(dp => dp.DocumentType)
            .Include(r => r.DocumentPermissions)
                .ThenInclude(dp => dp.Department)
                    .ThenInclude(d => d.Unit)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null)
        {
            return NotFound();
        }

        ViewData["DocumentTypes"] = await _context.DocumentTypes
            .Where(dt => dt.IsActive)
            .OrderBy(dt => dt.TypeName)
            .ToListAsync();

        ViewData["Departments"] = await _context.Departments
            .Include(d => d.Unit)
            .Where(d => d.IsActive)
            .OrderBy(d => d.DepartmentName)
            .ToListAsync();

        return View(role);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPermission(int roleId, int? documentTypeId, int? departmentId,
        bool canView, bool canUpload, bool canDownload, bool canReview, bool canApprove)
    {
        if (!documentTypeId.HasValue)
        {
            TempData["Error"] = "Evrak türü seçilmelidir.";
            return RedirectToAction(nameof(Permissions), new { id = roleId });
        }

        // Check if permission already exists
        var existingPermission = await _context.DocumentPermissions
            .FirstOrDefaultAsync(dp => dp.RoleId == roleId && 
                                     dp.DocumentTypeId == documentTypeId && 
                                     dp.DepartmentId == departmentId);

        if (existingPermission != null)
        {
            TempData["Error"] = "Bu rol için bu evrak türünde yetki zaten tanımlanmış.";
            return RedirectToAction(nameof(Permissions), new { id = roleId });
        }

        var permission = new DocumentPermission
        {
            RoleId = roleId,
            DocumentTypeId = documentTypeId,
            DepartmentId = departmentId,
            CanView = canView,
            CanUpload = canUpload,
            CanDownload = canDownload,
            CanReview = canReview,
            CanApprove = canApprove,
            CreatedAt = DateTime.Now
        };

        _context.DocumentPermissions.Add(permission);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Yetki başarıyla eklendi.";
        return RedirectToAction(nameof(Permissions), new { id = roleId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePermission(int permissionId,
        bool canView, bool canUpload, bool canDownload, bool canReview, bool canApprove)
    {
        var permission = await _context.DocumentPermissions.FindAsync(permissionId);
        if (permission == null)
        {
            return NotFound();
        }

        permission.CanView = canView;
        permission.CanUpload = canUpload;
        permission.CanDownload = canDownload;
        permission.CanReview = canReview;
        permission.CanApprove = canApprove;

        _context.Update(permission);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Yetki başarıyla güncellendi.";
        return RedirectToAction(nameof(Permissions), new { id = permission.RoleId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemovePermission(int permissionId)
    {
        var permission = await _context.DocumentPermissions.FindAsync(permissionId);
        if (permission == null)
        {
            return NotFound();
        }

        var roleId = permission.RoleId;

        _context.DocumentPermissions.Remove(permission);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Yetki başarıyla kaldırıldı.";
        return RedirectToAction(nameof(Permissions), new { id = roleId });
    }

    public async Task<IActionResult> AllInactive()
    {
        var inactiveRoles = await _context.Roles
            .Where(r => !r.IsActive)
            .OrderBy(r => r.RoleName)
            .ToListAsync();

        return View(inactiveRoles);
    }

    public async Task<IActionResult> Statistics()
    {
        var roleStats = await _context.Roles
            .Where(r => r.IsActive)
            .Select(r => new
            {
                Role = r,
                UserCount = r.Users.Count(u => u.IsActive),
                PermissionCount = r.DocumentPermissions.Count()
            })
            .OrderByDescending(rs => rs.UserCount)
            .ToListAsync();

        return View(roleStats);
    }

    private bool RoleExists(int id)
    {
        return _context.Roles.Any(e => e.Id == id);
    }
}