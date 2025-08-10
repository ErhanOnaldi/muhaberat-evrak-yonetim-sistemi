using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using muhaberat_evrak_yonetim.Models;
using muhaberat_evrak_yonetim.Entities;
using muhaberat_evrak_yonetim.Enums;
using muhaberat_evrak_yonetim.Extensions;

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

    public async Task<IActionResult> Index(string searchQuery, int? documentTypeId, string status, int? cargoId, DateTime? fromDate, DateTime? toDate)
    {
        var currentUserId = GetCurrentUserId();
        var currentUserDepartmentId = GetCurrentUserDepartmentId();
        var hasFullAccess = HasFullAccess();

        var documentsQuery = _context.Documents
            .Include(d => d.DocumentType)
            .Include(d => d.SenderUser)
            .Include(d => d.ReceiverUser)
            .Include(d => d.SenderDepartment)
            .Include(d => d.ReceiverDepartment)
            .Include(d => d.CreatedByUser)
            .Include(d => d.Cargo)
            .Where(d => d.IsActive);

        var currentUserDepartment = GetCurrentUserDepartment();

        if (!hasFullAccess && currentUserDepartment != "Muhaberat")
        {
            documentsQuery = documentsQuery.Where(d =>
                d.SenderUserId == currentUserId ||
                d.ReceiverUserId == currentUserId ||
                d.SenderDepartmentId == currentUserDepartmentId ||
                d.ReceiverDepartmentId == currentUserDepartmentId);
        }

        if (!string.IsNullOrEmpty(searchQuery))
        {
            documentsQuery = documentsQuery.Where(d =>
                d.Title.Contains(searchQuery) ||
                d.DocumentNumber.Contains(searchQuery) ||
                (d.Description != null && d.Description.Contains(searchQuery)) ||
                (d.CustomerName != null && d.CustomerName.Contains(searchQuery)) ||
                (d.CustomerId != null && d.CustomerId.Contains(searchQuery)));
        }

        if (documentTypeId.HasValue)
        {
            documentsQuery = documentsQuery.Where(d => d.DocumentTypeId == documentTypeId.Value);
        }

        if (!string.IsNullOrEmpty(status))
        {
            documentsQuery = documentsQuery.Where(d => d.Status == status);
        }

        if (cargoId.HasValue)
        {
            documentsQuery = documentsQuery.Where(d => d.CargoId == cargoId);
        }

        if (fromDate.HasValue)
        {
            documentsQuery = documentsQuery.Where(d => d.CreatedDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            documentsQuery = documentsQuery.Where(d => d.CreatedDate <= toDate.Value.AddDays(1));
        }

        var documents = await documentsQuery
            .OrderByDescending(d => d.CreatedDate)
            .ToListAsync();

        ViewData["DocumentTypes"] = await _context.DocumentTypes
            .Where(dt => dt.IsActive)
            .ToListAsync();

        ViewData["SearchQuery"] = searchQuery;
        ViewData["SelectedDocumentTypeId"] = documentTypeId;
        ViewData["SelectedStatus"] = status;
        ViewData["SelectedCargoId"] = cargoId;
        ViewData["Cargos"] = await _context.Cargos.OrderBy(c => c.CargoTrackingNumber).ToListAsync();
        ViewData["FromDate"] = fromDate?.ToString("yyyy-MM-dd");
        ViewData["ToDate"] = toDate?.ToString("yyyy-MM-dd");

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
            .Include(d => d.Cargo)
                .ThenInclude(c => c.CargoTrackingLogs)
                .ThenInclude(ct => ct.UpdatedByUser)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (document == null)
        {
            return NotFound();
        }

        // ViewBag değerlerini set et
        ViewBag.CurrentUserId = GetCurrentUserId();
        ViewBag.CurrentUserDepartmentId = GetCurrentUserDepartmentId();
        ViewBag.HasFullAccess = HasFullAccess();
        ViewBag.CurrentUserInfo = GetCurrentUserInfo();

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

        ViewData["Cargos"] = await _context.Cargos
            .Where(c => c.IsActive && c.DeliveryStatus == "PREPARING")
            .OrderBy(c => c.CargoTrackingNumber)
            .ToListAsync();

        var currentUserId = GetCurrentUserId();
        var currentUserDepartmentId = GetCurrentUserDepartmentId();
        
        if (currentUserId.HasValue)
        {
            ViewData["CurrentUserId"] = currentUserId;
            ViewData["CurrentUserDepartmentId"] = currentUserDepartmentId;
        }

        // Restore form data if coming back from cargo creation
        var document = new Document();
        
        if (TempData.ContainsKey("FormData"))
        {
            try
            {
                var formDataJson = TempData["FormData"]?.ToString();
                if (!string.IsNullOrEmpty(formDataJson))
                {
                    // Parse JSON manually to avoid navigation property issues
                    var formDataDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(formDataJson);
                    
                    if (formDataDict != null)
                    {
                        if (formDataDict.ContainsKey("Title") && formDataDict["Title"] != null)
                            document.Title = formDataDict["Title"].ToString();
                            
                        if (formDataDict.ContainsKey("Description") && formDataDict["Description"] != null)
                            document.Description = formDataDict["Description"].ToString();
                            
                        if (formDataDict.ContainsKey("DocumentTypeId") && formDataDict["DocumentTypeId"] != null && int.TryParse(formDataDict["DocumentTypeId"].ToString(), out int docTypeId))
                            document.DocumentTypeId = docTypeId > 0 ? docTypeId : null;
                            
                        if (formDataDict.ContainsKey("SenderUserId") && formDataDict["SenderUserId"] != null && int.TryParse(formDataDict["SenderUserId"].ToString(), out int senderUserId))
                            document.SenderUserId = senderUserId;
                            
                        if (formDataDict.ContainsKey("SenderDepartmentId") && formDataDict["SenderDepartmentId"] != null && int.TryParse(formDataDict["SenderDepartmentId"].ToString(), out int senderDeptId))
                            document.SenderDepartmentId = senderDeptId;
                            
                        if (formDataDict.ContainsKey("ReceiverUserId") && formDataDict["ReceiverUserId"] != null && int.TryParse(formDataDict["ReceiverUserId"].ToString(), out int receiverUserId))
                            document.ReceiverUserId = receiverUserId > 0 ? receiverUserId : null;
                            
                        if (formDataDict.ContainsKey("ReceiverDepartmentId") && formDataDict["ReceiverDepartmentId"] != null && int.TryParse(formDataDict["ReceiverDepartmentId"].ToString(), out int receiverDeptId))
                            document.ReceiverDepartmentId = receiverDeptId > 0 ? receiverDeptId : null;
                            
                        if (formDataDict.ContainsKey("CustomerName") && formDataDict["CustomerName"] != null)
                            document.CustomerName = formDataDict["CustomerName"].ToString();
                            
                        if (formDataDict.ContainsKey("CustomerId") && formDataDict["CustomerId"] != null)
                            document.CustomerId = formDataDict["CustomerId"].ToString();
                            
                        if (formDataDict.ContainsKey("PhysicalDocumentType") && formDataDict["PhysicalDocumentType"] != null)
                            document.PhysicalDocumentType = formDataDict["PhysicalDocumentType"].ToString();
                            
                        if (formDataDict.ContainsKey("CargoId") && formDataDict["CargoId"] != null && int.TryParse(formDataDict["CargoId"].ToString(), out int cargoId))
                            document.CargoId = cargoId > 0 ? cargoId : null;
                    }
                }
            }
            catch (Exception ex)
            {
                // If parsing fails, use empty document but log the error
                System.Diagnostics.Debug.WriteLine($"Form data parsing failed: {ex.Message}");
                document = new Document();
            }
            // Keep TempData for one more request
            TempData.Keep("FormData");
        }

        return View(document);
    }

    [HttpPost]
    public IActionResult SaveFormData(string formData)
    {
        if (!string.IsNullOrEmpty(formData))
        {
            TempData["FormData"] = formData;
            TempData.Keep("FormData"); // Ensure it persists
        }
        return Json(new { success = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Document document)
    {
        ModelState.Remove(nameof(Document.DocumentNumber));
        ModelState.Remove(nameof(Document.SenderUser));
        ModelState.Remove(nameof(Document.CreatedByUser));
        ModelState.Remove(nameof(Document.CreatedDate));
        
        document.DocumentNumber = await GenerateDocumentNumberAsync();
        document.CreatedDate = DateTime.Now;
        document.CreatedAt = DateTime.Now;
        document.UpdatedAt = DateTime.Now;
        document.CreatedBy = document.SenderUserId;
        document.IsActive = true;
        document.Status = DocumentStatus.DRAFT.ToString();

        if (document.SenderUserId <= 0)
        {
            ModelState.AddModelError("SenderUserId", "Lütfen gönderen kullanıcı seçin.");
        }

        if (document.SenderDepartmentId <= 0)
        {
            ModelState.AddModelError("SenderDepartmentId", "Lütfen gönderen departman seçin.");
        }

        if (document.ReceiverUserId == null && document.ReceiverDepartmentId == null)
        {
            ModelState.AddModelError("ReceiverUserId", "En az bir alıcı seçmelisiniz: Kullanıcı veya Departman.");
            ModelState.AddModelError("ReceiverDepartmentId", "En az bir alıcı seçmelisiniz: Kullanıcı veya Departman.");
        }

        if (string.IsNullOrWhiteSpace(document.Title))
        {
            ModelState.AddModelError("Title", "Başlık alanı zorunludur.");
        }

        if (string.IsNullOrWhiteSpace(document.PhysicalDocumentType))
        {
            ModelState.AddModelError("PhysicalDocumentType", "Fiziksel evrak türü seçimi zorunludur.");
        }

        if (document.CargoId == null || document.CargoId <= 0)
        {
            ModelState.AddModelError("CargoId", "Kargo seçimi zorunludur. Mevcut bir kargo seçin veya yeni kargo oluşturun.");
        }


        if (document.DocumentTypeId == null || document.DocumentTypeId <= 0)
        {
            ModelState.AddModelError("DocumentTypeId", "Evrak türü seçimi zorunludur.");
        }

        if (!ModelState.IsValid)
        {
            foreach (var key in ModelState.Keys)
            {
                var state = ModelState[key];
                if (state.Errors.Count > 0)
                {
                    foreach (var error in state.Errors)
                    {
                        _logger.LogWarning($"Validation Error - Field: {key}, Message: {error.ErrorMessage}");
                    }
                }
            }
        }

        if (ModelState.IsValid)
        {
            _context.Add(document);
            await _context.SaveChangesAsync();

            await LogDocumentHistoryAsync(document.Id, "UPLOADED", document.CreatedBy, "Evrak oluşturuldu");

            TempData["Success"] = "Evrak başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Details), new { id = document.Id });
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

        ViewData["Cargos"] = await _context.Cargos
            .Where(c => c.IsActive && c.DeliveryStatus == "PREPARING")
            .OrderBy(c => c.CargoTrackingNumber)
            .ToListAsync();

        var currentUserId = GetCurrentUserId();
        var currentUserDepartmentId = GetCurrentUserDepartmentId();
        
        if (currentUserId.HasValue)
        {
            ViewData["CurrentUserId"] = currentUserId;
            ViewData["CurrentUserDepartmentId"] = currentUserDepartmentId;
        }

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

        var currentUserId = GetCurrentUserIdRequired();

        if (ModelState.IsValid)
        {
            try
            {
                var existingDocument = await _context.Documents.FindAsync(id);
                if (existingDocument == null)
                {
                    return NotFound();
                }

                var oldValues = new
                {
                    Title = existingDocument.Title,
                    Description = existingDocument.Description,
                    Status = existingDocument.Status,
                };

                existingDocument.Title = document.Title;
                existingDocument.Description = document.Description;
                existingDocument.DocumentTypeId = document.DocumentTypeId;
                existingDocument.ReceiverUserId = document.ReceiverUserId;
                existingDocument.ReceiverDepartmentId = document.ReceiverDepartmentId;
                existingDocument.CustomerName = document.CustomerName;
                existingDocument.CustomerId = document.CustomerId;
                existingDocument.PhysicalDocumentType = document.PhysicalDocumentType;
                existingDocument.UpdatedAt = DateTime.Now;

                _context.Update(existingDocument);
                await _context.SaveChangesAsync();

                await LogDocumentHistoryAsync(id, "UPDATED", currentUserId, "Evrak bilgileri güncellendi", oldValues, document);

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
    public async Task<IActionResult> Send(int id, string sendNotes)
    {
        try
        {
            _logger.LogInformation($"Send action called - DocumentId: {id}, SendNotes: {sendNotes ?? "null"}");
            var currentUserId = GetCurrentUserIdRequired();
            _logger.LogInformation($"CurrentUserId obtained: {currentUserId}");
        
        var document = await _context.Documents
            .Include(d => d.Cargo)
            .ThenInclude(c => c.Documents)
            .FirstOrDefaultAsync(d => d.Id == id);
            
        if (document == null)
        {
            return NotFound();
        }

        if (document.Status == DocumentStatus.DRAFT.ToString())
        {
            // Update document status
            document.Status = DocumentStatus.SENT.ToString();
            document.UpdatedAt = DateTime.Now;
            
            // If document has cargo, update cargo and all related documents
            if (document.CargoId.HasValue && document.Cargo != null)
            {
                var cargo = document.Cargo;
                
                // Update cargo status to SHIPPED (Kargoya Verildi) and set shipping date
                cargo.DeliveryStatus = "SHIPPED";
                cargo.ShippingDate = DateTime.Now;
                cargo.UpdatedAt = DateTime.Now;
                
                // Update all documents in the same cargo to SENT status
                foreach (var cargoDocument in cargo.Documents)
                {
                    if (cargoDocument.Status == DocumentStatus.DRAFT.ToString())
                    {
                        cargoDocument.Status = DocumentStatus.SENT.ToString();
                        cargoDocument.UpdatedAt = DateTime.Now;
                        
                        // Log history for each document
                        await LogDocumentHistoryAsync(cargoDocument.Id, "SENT", currentUserId, 
                            $"Evrak kargo ile birlikte gönderildi. {(!string.IsNullOrEmpty(sendNotes) ? $"Not: {sendNotes}" : "")}");
                    }
                }
                
                _context.Update(cargo);
                TempData["Success"] = $"Evrak ve kargo başarıyla gönderildi. Kargo içindeki {cargo.Documents.Count(d => d.Status == DocumentStatus.SENT.ToString())} evrak 'Gönderildi' durumuna geçti.";
            }
            else
            {
                // Document without cargo - just update the document
                await LogDocumentHistoryAsync(id, "SENT", currentUserId, 
                    $"Evrak gönderildi. {(!string.IsNullOrEmpty(sendNotes) ? $"Not: {sendNotes}" : "")}");
                TempData["Success"] = "Evrak başarıyla gönderildi.";
            }

            _context.Update(document);
            await _context.SaveChangesAsync();
        }

            return RedirectToAction(nameof(Details), new { id });
        }
        catch (UnauthorizedAccessException)
        {
            TempData["Error"] = "Bu işlemi gerçekleştirmek için oturumunuzun açık olması gerekiyor.";
            return RedirectToAction("Login", "Account");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Evrak gönderimi sırasında hata oluştu: {ex.Message}";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    public async Task<IActionResult> History(int id)
    {
        var document = await _context.Documents
            .Include(d => d.DocumentType)
            .Include(d => d.SenderUser)
            .Include(d => d.ReceiverUser)
            .Include(d => d.SenderDepartment)
            .Include(d => d.ReceiverDepartment)
            .Include(d => d.CreatedByUser)
            .Include(d => d.ReviewedByUser)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (document == null)
        {
            return NotFound();
        }

        var documentHistory = await _context.DocumentHistories
            .Where(dh => dh.DocumentId == id)
            .Include(dh => dh.User)
                .ThenInclude(u => u.Department)
            .Include(dh => dh.User)
                .ThenInclude(u => u.Role)
            .OrderByDescending(dh => dh.ActionDate)
            .ToListAsync();

        ViewBag.Document = document;
        
        return View(documentHistory);
    }

    public async Task<IActionResult> CargoTracking(int id)
    {
        var document = await _context.Documents
            .Include(d => d.Cargo)
            .ThenInclude(c => c.Documents)
            .ThenInclude(d => d.DocumentType)
            .Include(d => d.Cargo)
            .ThenInclude(c => c.CargoTrackingLogs)
            .ThenInclude(ctl => ctl.UpdatedByUser)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (document == null || document.Cargo == null)
        {
            return NotFound();
        }

        return View(document.Cargo);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateCargoStatus(int cargoId, string status, string location, string notes)
    {
        var cargo = await _context.Cargos
            .Include(c => c.Documents)
            .FirstOrDefaultAsync(c => c.Id == cargoId);
            
        if (cargo == null)
        {
            return NotFound();
        }

        var oldStatus = cargo.DeliveryStatus;
        cargo.DeliveryStatus = status;
        cargo.UpdatedAt = DateTime.Now;

        // Update cargo timestamps
        if (status == "DELIVERED")
        {
            cargo.DeliveryDate = DateTime.Now;
        }
        else if (status == "SHIPPED")
        {
            cargo.ShippingDate = DateTime.Now;
        }

        // Sync document statuses with cargo status
        await SyncDocumentStatusesWithCargo(cargo, status);

        var trackingLog = new CargoTrackingLog
        {
            CargoId = cargoId,
            OldStatus = oldStatus,
            NewStatus = status,
            StatusChangeDate = DateTime.Now,
            Location = location,
            UpdatedBy = GetCurrentUserId(),
            Notes = notes,
            CreatedAt = DateTime.Now
        };

        _context.CargoTrackingLogs.Add(trackingLog);
        _context.Update(cargo);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(CargoTrackingById), new { cargoId });
    }

    private async Task SyncDocumentStatusesWithCargo(Cargo cargo, string cargoStatus)
    {
        string newDocumentStatus = cargoStatus switch
        {
            "PREPARING" => null, // Don't change - documents can stay as DRAFT
            "SHIPPED" => "SENT", // All documents should be SENT when cargo is shipped
            "IN_TRANSIT" => "SENT", // All documents should be SENT when cargo is in transit
            "DELIVERED" => "DELIVERED", // All documents should be DELIVERED when cargo is delivered
            "RETURNED" => null, // Don't change - keep current status
            _ => null
        };

        if (newDocumentStatus != null)
        {
            foreach (var document in cargo.Documents.Where(d => d.IsActive))
            {
                // Only update if the current status allows it
                if (ShouldUpdateDocumentStatus(document.Status, newDocumentStatus))
                {
                    var oldDocumentStatus = document.Status;
                    document.Status = newDocumentStatus;
                    document.UpdatedAt = DateTime.Now;

                    // Log document history for status change
                    var currentUserId = GetCurrentUserId() ?? 1; // Default to system user if null
                    await LogDocumentHistoryAsync(document.Id, newDocumentStatus, currentUserId, 
                        $"Durum kargo ile senkronize edildi: {cargoStatus}");
                }
            }
        }
    }

    private bool ShouldUpdateDocumentStatus(string currentStatus, string newStatus)
    {
        // Business rules for when document status can be updated
        return (currentStatus, newStatus) switch
        {
            // Can always move from DRAFT to SENT
            ("DRAFT", "SENT") => true,
            
            // Can move from SENT to DELIVERED
            ("SENT", "DELIVERED") => true,
            
            // Don't downgrade statuses (e.g., DELIVERED back to SENT)
            ("DELIVERED", "SENT") => false,
            ("RECEIVED", "SENT") => false,
            ("RECEIVED", "DELIVERED") => false,
            
            // Allow same status (no change needed but safe)
            var (current, target) when current == target => false,
            
            // Default: allow the change
            _ => true
        };
    }

    public async Task<IActionResult> CargoTrackingById(int cargoId)
    {
        var cargo = await _context.Cargos
            .Include(c => c.Documents)
            .ThenInclude(d => d.DocumentType)
            .Include(c => c.CargoTrackingLogs)
            .ThenInclude(ctl => ctl.UpdatedByUser)
            .FirstOrDefaultAsync(c => c.Id == cargoId);

        if (cargo == null)
        {
            return NotFound();
        }

        return View("CargoTracking", cargo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReceiveDocument(int id, string receivedByName, string receiptNotes)
    {
        try
        {
            var currentUserId = GetCurrentUserIdRequired();
            _logger.LogInformation($"ReceiveDocument başlatıldı. DocumentId: {id}, UserId: {currentUserId}");
            
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                _logger.LogWarning($"Document bulunamadı. DocumentId: {id}");
                return NotFound();
            }

            _logger.LogInformation($"Evrak durumu: {document.Status}");

            if (document.Status != DocumentStatus.SENT.ToString() && document.Status != DocumentStatus.DELIVERED.ToString())
            {
                TempData["Error"] = $"Bu evrak '{document.Status}' durumunda olduğu için teslim alınamaz. Sadece 'SENT' veya 'DELIVERED' durumundaki evraklar teslim alınabilir.";
                _logger.LogWarning($"Evrak durumu uygun değil. Mevcut durum: {document.Status}");
                return RedirectToAction(nameof(Details), new { id });
            }

            var hasFullAccess = HasFullAccess();
            var currentUserDepartmentId = GetCurrentUserDepartmentId();
            
            _logger.LogInformation($"Yetki kontrolleri - FullAccess: {hasFullAccess}, ReceiverUserId: {document.ReceiverUserId}, CurrentUserId: {currentUserId}, ReceiverDepartmentId: {document.ReceiverDepartmentId}, CurrentUserDepartmentId: {currentUserDepartmentId}");
            
            bool canReceive = hasFullAccess || 
                             document.ReceiverUserId == currentUserId ||
                             document.ReceiverDepartmentId == currentUserDepartmentId;

            if (!canReceive)
            {
                TempData["Error"] = "Bu evrağı teslim alma yetkiniz bulunmamaktadır.";
                _logger.LogWarning($"Yetkisiz teslim alma girişimi. DocumentId: {id}, UserId: {currentUserId}");
                return RedirectToAction(nameof(Details), new { id });
            }

            // Durumu güncelle
            document.Status = DocumentStatus.RECEIVED.ToString();
            document.UpdatedAt = DateTime.Now;

            _logger.LogInformation($"Evrak durumu güncellendi: {DocumentStatus.RECEIVED.ToString()}");

            _context.Update(document);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Veritabanı güncellemesi tamamlandı");

            var notes = $"Evrak teslim alındı. Teslim alan: {(!string.IsNullOrEmpty(receivedByName) ? receivedByName : GetCurrentUserInfo())}";
            if (!string.IsNullOrEmpty(receiptNotes))
            {
                notes += $" - Not: {receiptNotes}";
            }

            await LogDocumentHistoryAsync(id, "RECEIVED", currentUserId, notes);
            _logger.LogInformation("DocumentHistory kaydı eklendi");

            TempData["Success"] = "Evrak başarıyla teslim alındı.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"ReceiveDocument hatası. DocumentId: {id}");
            TempData["Error"] = $"Teslim alma işlemi sırasında hata oluştu: {ex.Message}";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Review(int id, bool approved, string reviewNotes)
    {
        var currentUserId = GetCurrentUserIdRequired();
        var document = await _context.Documents.FindAsync(id);
        if (document == null)
        {
            return NotFound();
        }

        document.ReviewedBy = currentUserId;
        document.ReviewDate = DateTime.Now;
        document.ReviewNotes = reviewNotes;
        document.Status = approved ? DocumentStatus.RECEIVED.ToString() : DocumentStatus.CANCELLED.ToString();
        document.UpdatedAt = DateTime.Now;

        _context.Update(document);
        await _context.SaveChangesAsync();

        var actionType = approved ? "RECEIVED" : "CANCELLED";
        await LogDocumentHistoryAsync(id, actionType, currentUserId, 
            $"Evrak {(approved ? "teslim alındı" : "iptal edildi")}: {(string.IsNullOrEmpty(reviewNotes) ? "" : reviewNotes)}");

        TempData["Success"] = $"Evrak başarıyla {(approved ? "teslim alındı" : "iptal edildi")}.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var currentUserId = GetCurrentUserIdRequired();
        var document = await _context.Documents.FindAsync(id);
        if (document == null)
        {
            return NotFound();
        }

        document.IsActive = false;
        document.UpdatedAt = DateTime.Now;

        _context.Update(document);
        await _context.SaveChangesAsync();

        await LogDocumentHistoryAsync(id, "DELETED", currentUserId, "Evrak silindi");

        TempData["Success"] = "Evrak başarıyla silindi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> MyDocuments()
    {
        int? currentUserId = GetCurrentUserId(); 

        IEnumerable<Document> sentDocuments = new List<Document>();
        IEnumerable<Document> receivedDocuments = new List<Document>(); 

        if (!currentUserId.HasValue)
        {
            _logger.LogWarning("MyDocuments: Current user ID is not available. Returning empty lists.");
            TempData["Error"] = "Oturum bilgileriniz eksik. Lütfen tekrar giriş yapın.";
            ViewBag.SentDocuments = sentDocuments;
            ViewBag.ReceivedDocuments = receivedDocuments;
            return View();
        }

        try
        {
            sentDocuments = await _context.Documents
                .Include(d => d.DocumentType)
                .Include(d => d.ReceiverUser)
                .Include(d => d.ReceiverDepartment)
                .Where(d => d.SenderUserId == currentUserId.Value && d.IsActive)
                .OrderByDescending(d => d.CreatedDate)
                .ToListAsync();

            receivedDocuments = await _context.Documents
                .Include(d => d.DocumentType)
                .Include(d => d.SenderUser)
                .Include(d => d.SenderDepartment)
                .Where(d => d.ReceiverUserId == currentUserId.Value && d.IsActive)
                .OrderByDescending(d => d.CreatedDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching documents for MyDocuments page.");
            TempData["Error"] = "Evraklar yüklenirken bir hata oluştu: " + ex.Message;
        }

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

    [HttpGet]
    public async Task<JsonResult> GetDocumentTitles(string query)
    {
        try
        {
            var documentsQuery = _context.Documents.Where(d => d.IsActive);

            if (!string.IsNullOrWhiteSpace(query))
            {
                documentsQuery = documentsQuery.Where(d => d.Title.Contains(query));
            }

            var titles = await documentsQuery
                .Select(d => d.Title)
                .ToListAsync();

            
            var distinctTitles = titles
                .Distinct()
                .Take(10)
                .ToList();

            return Json(distinctTitles);
        }
        catch (Exception ex)
        {
            
            _logger.LogError(ex, "Error in GetDocumentTitles with query: {Query}", query);
            return Json(new List<string>());
        }
    }

    [HttpGet]
    public async Task<JsonResult> GetUserNames(string query)
    {
        try
        {
            var usersQuery = _context.Users
                .Include(u => u.Department)
                .Where(u => u.IsActive);

            if (!string.IsNullOrWhiteSpace(query))
            {
                usersQuery = usersQuery.Where(u => 
                    u.FirstName.Contains(query) || 
                    u.LastName.Contains(query) || 
                    (u.FirstName + " " + u.LastName).Contains(query));
            }

            var users = await usersQuery
                .Select(u => new { 
                    id = u.Id,
                    name = u.FirstName + " " + u.LastName,
                    department = u.Department != null ? u.Department.DepartmentName : "",
                    departmentId = u.DepartmentId,
                    fullText = u.FirstName + " " + u.LastName + " (" + (u.Department != null ? u.Department.DepartmentName : "") + ")"
                })
                .Take(10)
                .ToListAsync();

            return Json(users);
        }
        catch (Exception ex)
        {
            
            _logger.LogError(ex, "Error in GetUserNames with query: {Query}", query);
            return Json(new List<object>());
        }
    }

    private bool DocumentExists(int id)
    {
        return _context.Documents.Any(e => e.Id == id);
    }
}