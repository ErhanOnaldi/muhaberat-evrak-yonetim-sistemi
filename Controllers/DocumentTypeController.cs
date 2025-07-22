using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using muhaberat_evrak_yonetim.Models;
using muhaberat_evrak_yonetim.Entities;

namespace muhaberat_evrak_yonetim.Controllers;

public class DocumentTypeController : Controller
{
    private readonly DataContext _context;
    private readonly ILogger<DocumentTypeController> _logger;

    public DocumentTypeController(DataContext context, ILogger<DocumentTypeController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var documentTypes = await _context.DocumentTypes
            .Where(dt => dt.IsActive)
            .OrderBy(dt => dt.TypeName)
            .ToListAsync();

        return View(documentTypes);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var documentType = await _context.DocumentTypes
            .Include(dt => dt.Documents.Where(d => d.IsActive))
                .ThenInclude(d => d.SenderUser)
            .Include(dt => dt.DocumentPermissions)
                .ThenInclude(dp => dp.Role)
            .Include(dt => dt.DocumentPermissions)
                .ThenInclude(dp => dp.Department)
            .FirstOrDefaultAsync(dt => dt.Id == id);

        if (documentType == null)
        {
            return NotFound();
        }

        // Get document type statistics
        var stats = new
        {
            TotalDocuments = documentType.Documents.Count,
            ThisMonthDocuments = documentType.Documents.Count(d => 
                d.CreatedDate.Month == DateTime.Now.Month && d.CreatedDate.Year == DateTime.Now.Year),
            PendingDocuments = documentType.Documents.Count(d => 
                d.Status != "DELIVERED" && d.Status != "RECEIVED"),
            PermissionsCount = documentType.DocumentPermissions.Count,
            AverageDeliveryTime = documentType.EstimatedDeliveryDays
        };

        ViewBag.Statistics = stats;
        return View(documentType);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DocumentType documentType)
    {
        // Check for duplicate type code
        if (await _context.DocumentTypes.AnyAsync(dt => dt.TypeCode == documentType.TypeCode))
        {
            ModelState.AddModelError("TypeCode", "Bu evrak türü kodu zaten kullanımda.");
        }

        // Check for duplicate type name
        if (await _context.DocumentTypes.AnyAsync(dt => dt.TypeName == documentType.TypeName))
        {
            ModelState.AddModelError("TypeName", "Bu evrak türü adı zaten kullanımda.");
        }

        if (ModelState.IsValid)
        {
            documentType.CreatedAt = DateTime.Now;
            documentType.IsActive = true;

            _context.Add(documentType);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Evrak türü başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Index));
        }

        return View(documentType);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var documentType = await _context.DocumentTypes.FindAsync(id);
        if (documentType == null)
        {
            return NotFound();
        }

        return View(documentType);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DocumentType documentType)
    {
        if (id != documentType.Id)
        {
            return NotFound();
        }

        // Check for duplicate type code (excluding current document type)
        if (await _context.DocumentTypes.AnyAsync(dt => dt.TypeCode == documentType.TypeCode && dt.Id != id))
        {
            ModelState.AddModelError("TypeCode", "Bu evrak türü kodu zaten kullanımda.");
        }

        // Check for duplicate type name (excluding current document type)
        if (await _context.DocumentTypes.AnyAsync(dt => dt.TypeName == documentType.TypeName && dt.Id != id))
        {
            ModelState.AddModelError("TypeName", "Bu evrak türü adı zaten kullanımda.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                var existingDocumentType = await _context.DocumentTypes.FindAsync(id);
                if (existingDocumentType == null)
                {
                    return NotFound();
                }

                existingDocumentType.TypeName = documentType.TypeName;
                existingDocumentType.TypeCode = documentType.TypeCode;
                existingDocumentType.Description = documentType.Description;
                existingDocumentType.IsUrgent = documentType.IsUrgent;
                existingDocumentType.RequiresSignature = documentType.RequiresSignature;
                existingDocumentType.PackageRequirements = documentType.PackageRequirements;
                existingDocumentType.EstimatedDeliveryDays = documentType.EstimatedDeliveryDays;
                existingDocumentType.IsActive = documentType.IsActive;

                _context.Update(existingDocumentType);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Evrak türü bilgileri başarıyla güncellendi.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DocumentTypeExists(documentType.Id))
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

        return View(documentType);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        var documentType = await _context.DocumentTypes.FindAsync(id);
        if (documentType == null)
        {
            return NotFound();
        }

        // Check if document type has active documents
        var hasActiveDocuments = await _context.Documents.AnyAsync(d => d.DocumentTypeId == id && d.IsActive);
        if (hasActiveDocuments)
        {
            TempData["Error"] = "Bu evrak türünde aktif evraklar bulunduğu için deaktif edilemez.";
            return RedirectToAction(nameof(Index));
        }

        documentType.IsActive = false;
        _context.Update(documentType);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Evrak türü deaktif edildi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(int id)
    {
        var documentType = await _context.DocumentTypes.FindAsync(id);
        if (documentType == null)
        {
            return NotFound();
        }

        documentType.IsActive = true;
        _context.Update(documentType);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Evrak türü aktif edildi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Documents(int id)
    {
        var documentType = await _context.DocumentTypes.FirstOrDefaultAsync(dt => dt.Id == id);
        if (documentType == null)
        {
            return NotFound();
        }

        var documents = await _context.Documents
            .Include(d => d.SenderUser)
            .Include(d => d.ReceiverUser)
            .Include(d => d.SenderDepartment)
            .Include(d => d.ReceiverDepartment)
            .Where(d => d.DocumentTypeId == id && d.IsActive)
            .OrderByDescending(d => d.CreatedDate)
            .ToListAsync();

        ViewBag.DocumentType = documentType;
        return View(documents);
    }

    public async Task<IActionResult> Permissions(int id)
    {
        var documentType = await _context.DocumentTypes
            .Include(dt => dt.DocumentPermissions)
                .ThenInclude(dp => dp.Role)
            .Include(dt => dt.DocumentPermissions)
                .ThenInclude(dp => dp.Department)
                    .ThenInclude(d => d.Unit)
            .FirstOrDefaultAsync(dt => dt.Id == id);

        if (documentType == null)
        {
            return NotFound();
        }

        ViewData["Roles"] = await _context.Roles
            .Where(r => r.IsActive)
            .OrderBy(r => r.RoleName)
            .ToListAsync();

        ViewData["Departments"] = await _context.Departments
            .Include(d => d.Unit)
            .Where(d => d.IsActive)
            .OrderBy(d => d.DepartmentName)
            .ToListAsync();

        return View(documentType);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPermission(int documentTypeId, int? roleId, int? departmentId, 
        bool canView, bool canUpload, bool canDownload, bool canReview, bool canApprove)
    {
        if (!roleId.HasValue && !departmentId.HasValue)
        {
            TempData["Error"] = "Rol veya departman seçilmelidir.";
            return RedirectToAction(nameof(Permissions), new { id = documentTypeId });
        }

        // Check if permission already exists
        var existingPermission = await _context.DocumentPermissions
            .FirstOrDefaultAsync(dp => dp.DocumentTypeId == documentTypeId && 
                                     dp.RoleId == roleId && dp.DepartmentId == departmentId);

        if (existingPermission != null)
        {
            TempData["Error"] = "Bu rol/departman için yetki zaten tanımlanmış.";
            return RedirectToAction(nameof(Permissions), new { id = documentTypeId });
        }

        var permission = new DocumentPermission
        {
            DocumentTypeId = documentTypeId,
            RoleId = roleId,
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
        return RedirectToAction(nameof(Permissions), new { id = documentTypeId });
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

        var documentTypeId = permission.DocumentTypeId;

        _context.DocumentPermissions.Remove(permission);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Yetki başarıyla kaldırıldı.";
        return RedirectToAction(nameof(Permissions), new { id = documentTypeId });
    }

    public async Task<IActionResult> UrgentTypes()
    {
        var urgentTypes = await _context.DocumentTypes
            .Where(dt => dt.IsUrgent && dt.IsActive)
            .OrderBy(dt => dt.EstimatedDeliveryDays)
            .ToListAsync();

        return View(urgentTypes);
    }

    public async Task<IActionResult> AllInactive()
    {
        var inactiveTypes = await _context.DocumentTypes
            .Where(dt => !dt.IsActive)
            .OrderBy(dt => dt.TypeName)
            .ToListAsync();

        return View(inactiveTypes);
    }

    private bool DocumentTypeExists(int id)
    {
        return _context.DocumentTypes.Any(e => e.Id == id);
    }
}