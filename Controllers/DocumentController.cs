using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using muhaberat_evrak_yonetim.Models;
using muhaberat_evrak_yonetim.Entities;

namespace muhaberat_evrak_yonetim.Controllers;

public class DocumentController : BaseController
{
    private readonly DataContext _context;
    private readonly ILogger<DocumentController> _logger;

    public DocumentController(DataContext context, ILogger<DocumentController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var documents = await _context.Documents
            .Include(d => d.DocumentType)
            .Include(d => d.SenderUser)
            .Include(d => d.ReceiverUser)
            .Include(d => d.SenderDepartment)
            .Include(d => d.ReceiverDepartment)
            .Include(d => d.CreatedByUser)
            .Where(d => d.IsActive)
            .OrderByDescending(d => d.CreatedDate)
            .ToListAsync();

        return View(documents);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var document = await _context.Documents
            .Include(d => d.DocumentType)
            .Include(d => d.SenderUser)
            .Include(d => d.ReceiverUser)
            .Include(d => d.SenderDepartment)
            .Include(d => d.ReceiverDepartment)
            .Include(d => d.CreatedByUser)
            .Include(d => d.ReviewedByUser)
            .Include(d => d.DocumentHistories)
                .ThenInclude(dh => dh.User)
            .Include(d => d.CargoTrackingLogs)
                .ThenInclude(ct => ct.UpdatedByUser)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (document == null)
        {
            return NotFound();
        }

        return View(document);
    }

    public async Task<IActionResult> Create()
    {
        ViewData["DocumentTypes"] = await _context.DocumentTypes
            .Where(dt => dt.IsActive)
            .ToListAsync();
        
        ViewData["Users"] = await _context.Users
            .Where(u => u.IsActive)
            .Include(u => u.Department)
            .ToListAsync();
        
        ViewData["Departments"] = await _context.Departments
            .Where(d => d.IsActive)
            .ToListAsync();

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Document document)
    {
        if (ModelState.IsValid)
        {
            // Generate document number
            document.DocumentNumber = await GenerateDocumentNumberAsync();
            document.CreatedDate = DateTime.Now;
            document.CreatedAt = DateTime.Now;
            document.UpdatedAt = DateTime.Now;
            document.IsActive = true;
            document.Status = "DRAFT";
            document.DeliveryStatus = "PREPARING";

            // Set created by from session (you'll need to implement session management)
            // For now, using a default value
            document.CreatedBy = document.SenderUserId;

            _context.Add(document);
            await _context.SaveChangesAsync();

            // Log document creation
            await LogDocumentHistoryAsync(document.Id, "UPLOADED", document.CreatedBy, "Evrak oluşturuldu");

            TempData["Success"] = "Evrak başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Details), new { id = document.Id });
        }

        // Reload ViewData if model is invalid
        ViewData["DocumentTypes"] = await _context.DocumentTypes
            .Where(dt => dt.IsActive)
            .ToListAsync();
        
        ViewData["Users"] = await _context.Users
            .Where(u => u.IsActive)
            .Include(u => u.Department)
            .ToListAsync();
        
        ViewData["Departments"] = await _context.Departments
            .Where(d => d.IsActive)
            .ToListAsync();

        return View(document);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var document = await _context.Documents.FindAsync(id);
        if (document == null)
        {
            return NotFound();
        }

        ViewData["DocumentTypes"] = await _context.DocumentTypes
            .Where(dt => dt.IsActive)
            .ToListAsync();
        
        ViewData["Users"] = await _context.Users
            .Where(u => u.IsActive)
            .Include(u => u.Department)
            .ToListAsync();
        
        ViewData["Departments"] = await _context.Departments
            .Where(d => d.IsActive)
            .ToListAsync();

        return View(document);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Document document)
    {
        if (id != document.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                var existingDocument = await _context.Documents.FindAsync(id);
                if (existingDocument == null)
                {
                    return NotFound();
                }

                // Store old values for history logging
                var oldValues = new
                {
                    Title = existingDocument.Title,
                    Description = existingDocument.Description,
                    Status = existingDocument.Status,
                    DeliveryStatus = existingDocument.DeliveryStatus
                };

                // Update fields
                existingDocument.Title = document.Title;
                existingDocument.Description = document.Description;
                existingDocument.DocumentTypeId = document.DocumentTypeId;
                existingDocument.ReceiverUserId = document.ReceiverUserId;
                existingDocument.ReceiverDepartmentId = document.ReceiverDepartmentId;
                existingDocument.CustomerName = document.CustomerName;
                existingDocument.CustomerId = document.CustomerId;
                existingDocument.ShippingAddress = document.ShippingAddress;
                existingDocument.DeliveryAddress = document.DeliveryAddress;
                existingDocument.PhysicalDocumentType = document.PhysicalDocumentType;
                existingDocument.PackageType = document.PackageType;
                existingDocument.UpdatedAt = DateTime.Now;

                _context.Update(existingDocument);
                await _context.SaveChangesAsync();

                // Log the update
                await LogDocumentHistoryAsync(id, "UPDATED", existingDocument.SenderUserId, "Evrak bilgileri güncellendi", oldValues, document);

                TempData["Success"] = "Evrak başarıyla güncellendi.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DocumentExists(document.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        ViewData["DocumentTypes"] = await _context.DocumentTypes
            .Where(dt => dt.IsActive)
            .ToListAsync();
        
        ViewData["Users"] = await _context.Users
            .Where(u => u.IsActive)
            .Include(u => u.Department)
            .ToListAsync();
        
        ViewData["Departments"] = await _context.Departments
            .Where(d => d.IsActive)
            .ToListAsync();

        return View(document);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(int id)
    {
        var document = await _context.Documents.FindAsync(id);
        if (document == null)
        {
            return NotFound();
        }

        if (document.Status == "DRAFT")
        {
            document.Status = "SENT";
            document.UpdatedAt = DateTime.Now;

            _context.Update(document);
            await _context.SaveChangesAsync();

            await LogDocumentHistoryAsync(id, "SENT", document.SenderUserId, "Evrak gönderildi");

            TempData["Success"] = "Evrak başarıyla gönderildi.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateCargoStatus(int id, string cargoCompany, string trackingNumber, string deliveryStatus, string location = null, string notes = null)
    {
        var document = await _context.Documents.FindAsync(id);
        if (document == null)
        {
            return NotFound();
        }

        var oldStatus = document.DeliveryStatus;
        
        // Update document cargo information
        document.CargoCompany = cargoCompany;
        document.CargoTrackingNumber = trackingNumber;
        document.DeliveryStatus = deliveryStatus;
        document.UpdatedAt = DateTime.Now;

        if (deliveryStatus == "SHIPPED" && document.ShippingDate == null)
        {
            document.ShippingDate = DateTime.Now;
        }
        else if (deliveryStatus == "DELIVERED")
        {
            document.DeliveryDate = DateTime.Now;
            document.Status = "DELIVERED";
        }

        _context.Update(document);
        
        // Create cargo tracking log
        var cargoLog = new CargoTrackingLog
        {
            DocumentId = id,
            OldStatus = oldStatus,
            NewStatus = deliveryStatus,
            StatusChangeDate = DateTime.Now,
            Location = location,
            UpdatedBy = document.SenderUserId, // This should be current user from session
            Notes = notes ?? $"Kargo şirketi: {cargoCompany}, Takip No: {trackingNumber}",
            CreatedAt = DateTime.Now
        };

        _context.CargoTrackingLogs.Add(cargoLog);
        await _context.SaveChangesAsync();

        await LogDocumentHistoryAsync(id, "CARGO_UPDATED", document.SenderUserId, $"Kargo durumu güncellendi: {deliveryStatus}");

        TempData["Success"] = "Kargo durumu başarıyla güncellendi.";
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> History(int id)
    {
        var document = await _context.Documents
            .Include(d => d.DocumentType)
            .Include(d => d.SenderUser)
            .Include(d => d.ReceiverUser)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (document == null)
        {
            return NotFound();
        }

        var documentHistory = await _context.DocumentHistories
            .Where(dh => dh.DocumentId == id)
            .Include(dh => dh.User)
            .OrderByDescending(dh => dh.ActionDate)
            .ToListAsync();

        ViewBag.Document = document;
        
        return View(documentHistory);
    }

    public async Task<IActionResult> CargoTracking(int id)
    {
        var document = await _context.Documents
            .Include(d => d.DocumentType)
            .Include(d => d.SenderUser)
            .Include(d => d.ReceiverUser)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (document == null)
        {
            return NotFound();
        }

        var cargoLogs = await _context.CargoTrackingLogs
            .Where(ct => ct.DocumentId == id)
            .Include(ct => ct.UpdatedByUser)
            .OrderByDescending(ct => ct.StatusChangeDate)
            .ToListAsync();

        ViewBag.Document = document;
        
        return View(cargoLogs);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Review(int id, bool approved, string reviewNotes)
    {
        var document = await _context.Documents.FindAsync(id);
        if (document == null)
        {
            return NotFound();
        }

        document.ReviewedBy = document.SenderUserId; // This should be current user from session
        document.ReviewDate = DateTime.Now;
        document.ReviewNotes = reviewNotes;
        document.Status = approved ? "APPROVED" : "REJECTED";
        document.UpdatedAt = DateTime.Now;

        _context.Update(document);
        await _context.SaveChangesAsync();

        await LogDocumentHistoryAsync(id, approved ? "APPROVED" : "REJECTED", document.ReviewedBy.Value, 
            $"Evrak {(approved ? "onaylandı" : "reddedildi")}: {reviewNotes}");

        TempData["Success"] = $"Evrak başarıyla {(approved ? "onaylandı" : "reddedildi")}.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var document = await _context.Documents.FindAsync(id);
        if (document == null)
        {
            return NotFound();
        }

        document.IsActive = false;
        document.UpdatedAt = DateTime.Now;

        _context.Update(document);
        await _context.SaveChangesAsync();

        await LogDocumentHistoryAsync(id, "DELETED", document.SenderUserId, "Evrak silindi");

        TempData["Success"] = "Evrak başarıyla silindi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> MyDocuments()
    {
        // Get current user from session
        int currentUserId = GetCurrentUserIdRequired();

        var sentDocuments = await _context.Documents
            .Include(d => d.DocumentType)
            .Include(d => d.ReceiverUser)
            .Include(d => d.ReceiverDepartment)
            .Where(d => d.SenderUserId == currentUserId && d.IsActive)
            .OrderByDescending(d => d.CreatedDate)
            .ToListAsync();

        var receivedDocuments = await _context.Documents
            .Include(d => d.DocumentType)
            .Include(d => d.SenderUser)
            .Include(d => d.SenderDepartment)
            .Where(d => d.ReceiverUserId == currentUserId && d.IsActive)
            .OrderByDescending(d => d.CreatedDate)
            .ToListAsync();

        ViewBag.SentDocuments = sentDocuments;
        ViewBag.ReceivedDocuments = receivedDocuments;

        return View();
    }

    private async Task<string> GenerateDocumentNumberAsync()
    {
        var today = DateTime.Now;
        var prefix = "EVR";
        var dateStr = today.ToString("yyyyMMdd");
        
        var todaysCount = await _context.Documents
            .Where(d => d.CreatedDate.Date == today.Date)
            .CountAsync();
        
        var sequence = (todaysCount + 1).ToString("D3");
        
        return $"{prefix}-{dateStr}-{sequence}";
    }

    private async Task LogDocumentHistoryAsync(int documentId, string actionType, int userId, string notes, object oldValues = null, object newValues = null)
    {
        var history = new DocumentHistory
        {
            DocumentId = documentId,
            ActionType = actionType,
            UserId = userId,
            ActionDate = DateTime.Now,
            Notes = notes,
            OldValues = oldValues != null ? System.Text.Json.JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues != null ? System.Text.Json.JsonSerializer.Serialize(newValues) : null,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        };

        _context.DocumentHistories.Add(history);
        await _context.SaveChangesAsync();
    }

    private bool DocumentExists(int id)
    {
        return _context.Documents.Any(e => e.Id == id);
    }
}