using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using muhaberat_evrak_yonetim.Models;
using muhaberat_evrak_yonetim.Entities;

namespace muhaberat_evrak_yonetim.Controllers;

public class DocumentTypeCategoryController : BaseController
{
    private readonly DataContext _context;
    private readonly ILogger<DocumentTypeCategoryController> _logger;

    public DocumentTypeCategoryController(DataContext context, ILogger<DocumentTypeCategoryController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _context.DocumentTypeCategories
            .Include(c => c.DocumentTypes)
            .Where(c => c.IsActive)
            .OrderBy(c => c.CategoryName)
            .ToListAsync();

        return View(categories);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var category = await _context.DocumentTypeCategories
            .Include(c => c.DocumentTypes.Where(dt => dt.IsActive))
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return NotFound();
        }

        var stats = new
        {
            TotalDocumentTypes = category.DocumentTypes.Count,
            ActiveDocumentTypes = category.DocumentTypes.Count(dt => dt.IsActive),
            UrgentDocumentTypes = category.DocumentTypes.Count(dt => dt.IsUrgent),
            SignatureRequiredTypes = category.DocumentTypes.Count(dt => dt.RequiresSignature)
        };

        ViewBag.Statistics = stats;
        return View(category);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DocumentTypeCategory category)
    {
        // Check for duplicate category name
        if (await _context.DocumentTypeCategories.AnyAsync(c => c.CategoryName == category.CategoryName))
        {
            ModelState.AddModelError("CategoryName", "Bu kategori adı zaten kullanımda.");
        }

        // Check for duplicate category code
        if (!string.IsNullOrEmpty(category.CategoryCode) && 
            await _context.DocumentTypeCategories.AnyAsync(c => c.CategoryCode == category.CategoryCode))
        {
            ModelState.AddModelError("CategoryCode", "Bu kategori kodu zaten kullanımda.");
        }

        if (ModelState.IsValid)
        {
            category.CreatedAt = DateTime.Now;
            category.IsActive = true;

            _context.Add(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Kategori başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Index));
        }

        return View(category);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var category = await _context.DocumentTypeCategories.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DocumentTypeCategory category)
    {
        if (id != category.Id)
        {
            return NotFound();
        }

        // Check for duplicate category name (excluding current category)
        if (await _context.DocumentTypeCategories.AnyAsync(c => c.CategoryName == category.CategoryName && c.Id != id))
        {
            ModelState.AddModelError("CategoryName", "Bu kategori adı zaten kullanımda.");
        }

        // Check for duplicate category code (excluding current category)
        if (!string.IsNullOrEmpty(category.CategoryCode) && 
            await _context.DocumentTypeCategories.AnyAsync(c => c.CategoryCode == category.CategoryCode && c.Id != id))
        {
            ModelState.AddModelError("CategoryCode", "Bu kategori kodu zaten kullanımda.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                var existingCategory = await _context.DocumentTypeCategories.FindAsync(id);
                if (existingCategory == null)
                {
                    return NotFound();
                }

                existingCategory.CategoryName = category.CategoryName;
                existingCategory.CategoryCode = category.CategoryCode;
                existingCategory.Description = category.Description;
                existingCategory.IsActive = category.IsActive;
                existingCategory.UpdatedAt = DateTime.Now;

                _context.Update(existingCategory);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Kategori bilgileri başarıyla güncellendi.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(category.Id))
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

        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        var category = await _context.DocumentTypeCategories.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        // Check if category has active document types
        var hasActiveDocumentTypes = await _context.DocumentTypes.AnyAsync(dt => dt.CategoryId == id && dt.IsActive);
        if (hasActiveDocumentTypes)
        {
            TempData["Error"] = "Bu kategoride aktif evrak türleri bulunduğu için deaktif edilemez.";
            return RedirectToAction(nameof(Index));
        }

        category.IsActive = false;
        category.UpdatedAt = DateTime.Now;
        _context.Update(category);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Kategori deaktif edildi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(int id)
    {
        var category = await _context.DocumentTypeCategories.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        category.IsActive = true;
        category.UpdatedAt = DateTime.Now;
        _context.Update(category);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Kategori aktif edildi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> DocumentTypes(int id)
    {
        var category = await _context.DocumentTypeCategories
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return NotFound();
        }

        var documentTypes = await _context.DocumentTypes
            .Where(dt => dt.CategoryId == id && dt.IsActive)
            .OrderBy(dt => dt.TypeName)
            .ToListAsync();

        ViewBag.Category = category;
        return View(documentTypes);
    }

    // CSV import functionality removed
    // Use ImportDocumentTypesFromCsv.sql script instead for one-time data setup

    private bool CategoryExists(int id)
    {
        return _context.DocumentTypeCategories.Any(c => c.Id == id);
    }
}