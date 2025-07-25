using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using muhaberat_evrak_yonetim.Models;
using muhaberat_evrak_yonetim.Entities;

namespace muhaberat_evrak_yonetim.Controllers;

public class DepartmentController : BaseController
{
    private readonly DataContext _context;
    private readonly ILogger<DepartmentController> _logger;

    public DepartmentController(DataContext context, ILogger<DepartmentController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var departments = await _context.Departments
            .Include(d => d.Unit)
            .Where(d => d.IsActive)
            .OrderBy(d => d.DepartmentName)
            .ToListAsync();

        return View(departments);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var department = await _context.Departments
            .Include(d => d.Unit)
            .Include(d => d.Users)
                .ThenInclude(u => u.Role)
            .Include(d => d.SentDocuments)
                .ThenInclude(doc => doc.DocumentType)
            .Include(d => d.ReceivedDocuments)
                .ThenInclude(doc => doc.DocumentType)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (department == null)
        {
            return NotFound();
        }

        var stats = new
        {
            TotalUsers = department.Users.Count(u => u.IsActive),
            TotalSentDocuments = department.SentDocuments.Count(d => d.IsActive),
            TotalReceivedDocuments = department.ReceivedDocuments.Count(d => d.IsActive),
            PendingDocuments = department.SentDocuments.Count(d => d.IsActive && 
                d.Status != "DELIVERED" && d.Status != "RECEIVED"),
            ThisMonthDocuments = department.SentDocuments.Count(d => d.IsActive && 
                d.CreatedDate.Month == DateTime.Now.Month && d.CreatedDate.Year == DateTime.Now.Year)
        };

        ViewBag.Statistics = stats;
        return View(department);
    }

    public async Task<IActionResult> Create()
    {
        ViewData["Units"] = await _context.Units
            .Where(u => u.IsActive)
            .OrderBy(u => u.UnitName)
            .ToListAsync();

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Department department)
    {
        if (await _context.Departments.AnyAsync(d => d.DepartmentCode == department.DepartmentCode))
        {
            ModelState.AddModelError("DepartmentCode", "Bu departman kodu zaten kullanımda.");
        }

        if (await _context.Departments.AnyAsync(d => d.DepartmentName == department.DepartmentName && d.UnitId == department.UnitId))
        {
            ModelState.AddModelError("DepartmentName", "Bu birimde aynı isimde bir departman zaten mevcut.");
        }

        if (ModelState.IsValid)
        {
            department.CreatedAt = DateTime.Now;
            department.IsActive = true;

            _context.Add(department);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Departman başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Index));
        }

        ViewData["Units"] = await _context.Units
            .Where(u => u.IsActive)
            .OrderBy(u => u.UnitName)
            .ToListAsync();

        return View(department);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var department = await _context.Departments.FindAsync(id);
        if (department == null)
        {
            return NotFound();
        }

        ViewData["Units"] = await _context.Units
            .Where(u => u.IsActive)
            .OrderBy(u => u.UnitName)
            .ToListAsync();

        return View(department);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Department department)
    {
        if (id != department.Id)
        {
            return NotFound();
        }

        if (await _context.Departments.AnyAsync(d => d.DepartmentCode == department.DepartmentCode && d.Id != id))
        {
            ModelState.AddModelError("DepartmentCode", "Bu departman kodu zaten kullanımda.");
        }

        if (await _context.Departments.AnyAsync(d => d.DepartmentName == department.DepartmentName && 
            d.UnitId == department.UnitId && d.Id != id))
        {
            ModelState.AddModelError("DepartmentName", "Bu birimde aynı isimde bir departman zaten mevcut.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                var existingDepartment = await _context.Departments.FindAsync(id);
                if (existingDepartment == null)
                {
                    return NotFound();
                }

                existingDepartment.DepartmentName = department.DepartmentName;
                existingDepartment.DepartmentCode = department.DepartmentCode;
                existingDepartment.Description = department.Description;
                existingDepartment.HasFullAccess = department.HasFullAccess;
                existingDepartment.UnitId = department.UnitId;
                existingDepartment.IsActive = department.IsActive;

                _context.Update(existingDepartment);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Departman bilgileri başarıyla güncellendi.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DepartmentExists(department.Id))
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

        ViewData["Units"] = await _context.Units
            .Where(u => u.IsActive)
            .OrderBy(u => u.UnitName)
            .ToListAsync();

        return View(department);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        var department = await _context.Departments.FindAsync(id);
        if (department == null)
        {
            return NotFound();
        }

        var hasActiveUsers = await _context.Users.AnyAsync(u => u.DepartmentId == id && u.IsActive);
        if (hasActiveUsers)
        {
            TempData["Error"] = "Bu departmanda aktif kullanıcılar bulunduğu için deaktif edilemez. Önce kullanıcıları başka departmanlara taşıyın.";
            return RedirectToAction(nameof(Index));
        }

        department.IsActive = false;
        _context.Update(department);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Departman deaktif edildi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(int id)
    {
        var department = await _context.Departments.FindAsync(id);
        if (department == null)
        {
            return NotFound();
        }

        department.IsActive = true;
        _context.Update(department);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Departman aktif edildi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Users(int id)
    {
        var department = await _context.Departments
            .Include(d => d.Unit)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (department == null)
        {
            return NotFound();
        }

        var users = await _context.Users
            .Include(u => u.Role)
            .Where(u => u.DepartmentId == id && u.IsActive)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToListAsync();

        ViewBag.Department = department;
        return View(users);
    }

    public async Task<IActionResult> Documents(int id)
    {
        var department = await _context.Departments
            .Include(d => d.Unit)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (department == null)
        {
            return NotFound();
        }

        var sentDocuments = await _context.Documents
            .Include(doc => doc.DocumentType)
            .Include(doc => doc.SenderUser)
            .Include(doc => doc.ReceiverUser)
            .Include(doc => doc.ReceiverDepartment)
            .Where(doc => doc.SenderDepartmentId == id && doc.IsActive)
            .OrderByDescending(doc => doc.CreatedDate)
            .ToListAsync();

        var receivedDocuments = await _context.Documents
            .Include(doc => doc.DocumentType)
            .Include(doc => doc.SenderUser)
            .Include(doc => doc.SenderDepartment)
            .Include(doc => doc.ReceiverUser)
            .Where(doc => doc.ReceiverDepartmentId == id && doc.IsActive)
            .OrderByDescending(doc => doc.CreatedDate)
            .ToListAsync();

        ViewBag.Department = department;
        ViewBag.SentDocuments = sentDocuments;
        ViewBag.ReceivedDocuments = receivedDocuments;

        return View();
    }

    public async Task<IActionResult> ByUnit(int unitId)
    {
        var unit = await _context.Units.FirstOrDefaultAsync(u => u.Id == unitId);
        if (unit == null)
        {
            return NotFound();
        }

        var departments = await _context.Departments
            .Where(d => d.UnitId == unitId && d.IsActive)
            .OrderBy(d => d.DepartmentName)
            .ToListAsync();

        ViewBag.Unit = unit;
        return View(departments);
    }

    public async Task<IActionResult> AllInactive()
    {
        var departments = await _context.Departments
            .Include(d => d.Unit)
            .Where(d => !d.IsActive)
            .OrderBy(d => d.DepartmentName)
            .ToListAsync();

        return View(departments);
    }

    private bool DepartmentExists(int id)
    {
        return _context.Departments.Any(e => e.Id == id);
    }
}