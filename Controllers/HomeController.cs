using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using muhaberat_evrak_yonetim.Entities;
using muhaberat_evrak_yonetim.Models;

namespace muhaberat_evrak_yonetim.Controllers;

public class HomeController : BaseController
{
    private readonly DataContext _context;
    private readonly ILogger<HomeController> _logger;

    public HomeController(DataContext context, ILogger<HomeController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var currentUserId = GetCurrentUserId();
        var currentUserDepartmentId = GetCurrentUserDepartmentId();
        var hasFullAccess = HasFullAccess();

        IQueryable<Document> documentsQuery = _context.Documents.Where(d => d.IsActive);

        var currentUserDepartment = GetCurrentUserDepartment();

        if (!hasFullAccess && currentUserDepartment != "Muhaberat")
        {
            documentsQuery = documentsQuery.Where(d =>
                d.SenderUserId == currentUserId ||
                d.ReceiverUserId == currentUserId ||
                d.SenderDepartmentId == currentUserDepartmentId ||
                d.ReceiverDepartmentId == currentUserDepartmentId);
        }

        var dashboardStats = new
        {
            TotalDocuments = await documentsQuery.CountAsync(),
            TotalDocumentsThisMonth = await documentsQuery.CountAsync(d =>
                d.CreatedDate.Month == DateTime.Now.Month && d.CreatedDate.Year == DateTime.Now.Year),
            PendingDocuments = await documentsQuery.CountAsync(d =>
                d.Status != "DELIVERED" && d.Status != "RECEIVED"),
            DeliveredDocuments = await documentsQuery.CountAsync(d =>
                (d.Status == "DELIVERED" || d.Status == "RECEIVED")),

            InTransitCargo = await documentsQuery.CountAsync(d =>
                (d.DeliveryStatus == "SHIPPED" || d.DeliveryStatus == "IN_TRANSIT")),
            DeliveredCargo = await documentsQuery.CountAsync(d =>
                d.DeliveryStatus == "DELIVERED"),

            TotalUsers = await _context.Users.CountAsync(u => u.IsActive),
            TotalDepartments = await _context.Departments.CountAsync(d => d.IsActive),
            TotalRoles = await _context.Roles.CountAsync(r => r.IsActive),
            TotalDocumentTypes = await _context.DocumentTypes.CountAsync(dt => dt.IsActive),

            RecentDocuments = await documentsQuery
                .Include(d => d.DocumentType)
                .Include(d => d.SenderUser)
                .Include(d => d.SenderDepartment)
                .Include(d => d.ReceiverUser)
                .Include(d => d.ReceiverDepartment)
                .OrderByDescending(d => d.CreatedDate)
                .Take(4)
                .ToListAsync(),

            

            DocumentTypeUsage = await GetDocumentTypeUsageAsync(documentsQuery),

            MonthlyStats = await GetMonthlyStatistics(documentsQuery),
            
            // Yeni grafik verileri
            DocumentStatusChart = await GetDocumentStatusChartData(documentsQuery),
            TopDepartmentsChart = await GetTopDepartmentsChartData(documentsQuery),
            DeliverySuccessChart = await GetDeliverySuccessChartData(documentsQuery)
        };

        // Debug bilgisi ekle
        _logger.LogInformation($"Total Document Types: {await _context.DocumentTypes.CountAsync(dt => dt.IsActive)}");
        _logger.LogInformation($"Total Documents (filtered): {await documentsQuery.CountAsync()}");
        _logger.LogInformation($"DocumentTypeUsage count: {dashboardStats.DocumentTypeUsage.Count}");
        _logger.LogInformation($"Current User ID: {currentUserId}, Department: {currentUserDepartment}, HasFullAccess: {hasFullAccess}");
        _logger.LogInformation($"DocumentStatusChart: {System.Text.Json.JsonSerializer.Serialize(dashboardStats.DocumentStatusChart)}");
        _logger.LogInformation($"TopDepartmentsChart: {System.Text.Json.JsonSerializer.Serialize(dashboardStats.TopDepartmentsChart)}");
        _logger.LogInformation($"DeliverySuccessChart: {System.Text.Json.JsonSerializer.Serialize(dashboardStats.DeliverySuccessChart)}");
        
        ViewBag.DashboardStats = dashboardStats;
        return View();
    }

    private async Task<object> GetMonthlyStatistics(IQueryable<Document> documentsQuery)
    {
        var monthlyData = new List<object>();

        for (int i = 5; i >= 0; i--)
        {
            var targetDate = DateTime.Now.AddMonths(-i);
            var monthStart = new DateTime(targetDate.Year, targetDate.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var monthStats = new
            {
                Month = targetDate.ToString("MMM yyyy"),
                DocumentCount = await documentsQuery.CountAsync(d =>
                    d.CreatedDate >= monthStart && d.CreatedDate <= monthEnd),
                DeliveredCount = await documentsQuery.CountAsync(d =>
                    d.CreatedDate >= monthStart && d.CreatedDate <= monthEnd &&
                    (d.Status == "DELIVERED" || d.Status == "RECEIVED"))
            };

            monthlyData.Add(monthStats);
        }

        return monthlyData;
    }

    private async Task<List<object>> GetDocumentTypeUsageAsync(IQueryable<Document> documentsQuery)
    {
        // Önce tüm aktif document type'ları al
        var documentTypes = await _context.DocumentTypes
            .Where(dt => dt.IsActive)
            .ToListAsync();

        var documentTypeUsage = new List<object>();

        foreach (var docType in documentTypes)
        {
            // Her document type için ayrı ayrı count yap
            var count = await documentsQuery.CountAsync(d => d.DocumentTypeId == docType.Id);
            
            if (count > 0)
            {
                documentTypeUsage.Add(new
                {
                    DocumentType = docType,
                    DocumentCount = count
                });
            }
        }

        return documentTypeUsage
            .OrderByDescending(x => ((dynamic)x).DocumentCount)
            .Take(6)
            .ToList();
    }

    private async Task<object> GetDocumentStatusChartData(IQueryable<Document> documentsQuery)
    {
        var statusCounts = new
        {
            Delivered = await documentsQuery.CountAsync(d => d.Status == "DELIVERED" || d.Status == "RECEIVED"),
            InProgress = await documentsQuery.CountAsync(d => d.Status == "SENT" || d.Status == "IN_TRANSIT"),
            Pending = await documentsQuery.CountAsync(d => d.Status == "PENDING" || d.Status == "DRAFT"),
            Total = await documentsQuery.CountAsync()
        };

        return new
        {
            Labels = new[] { "Teslim Edildi", "İşlemde", "Beklemede" },
            Data = new[] { statusCounts.Delivered, statusCounts.InProgress, statusCounts.Pending },
            Colors = new[] { "#28a745", "#ffc107", "#dc3545" }
        };
    }

    private async Task<object> GetTopDepartmentsChartData(IQueryable<Document> documentsQuery)
    {
        var senderStats = await documentsQuery
            .Where(d => d.SenderDepartment != null)
            .GroupBy(d => d.SenderDepartment.DepartmentName)
            .Select(g => new { Department = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(8)
            .ToListAsync();

        var receiverStats = await documentsQuery
            .Where(d => d.ReceiverDepartment != null)
            .GroupBy(d => d.ReceiverDepartment.DepartmentName)
            .Select(g => new { Department = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(8)
            .ToListAsync();

        return new
        {
            SenderData = new
            {
                Labels = senderStats.Select(s => s.Department).ToArray(),
                Data = senderStats.Select(s => s.Count).ToArray()
            },
            ReceiverData = new
            {
                Labels = receiverStats.Select(r => r.Department).ToArray(),
                Data = receiverStats.Select(r => r.Count).ToArray()
            }
        };
    }

    private async Task<object> GetDeliverySuccessChartData(IQueryable<Document> documentsQuery)
    {
        var departmentSuccess = await documentsQuery
            .Where(d => d.SenderDepartment != null)
            .GroupBy(d => d.SenderDepartment.DepartmentName)
            .Select(g => new
            {
                Department = g.Key,
                Total = g.Count(),
                Delivered = g.Count(d => d.Status == "DELIVERED" || d.Status == "RECEIVED")
            })
            .Where(x => x.Total >= 5) // En az 5 evrakı olan departmanlar
            .OrderByDescending(x => (double)x.Delivered / x.Total)
            .Take(10)
            .ToListAsync();

        var successRates = departmentSuccess.Select(d => new
        {
            Department = d.Department,
            SuccessRate = Math.Round((double)d.Delivered / d.Total * 100, 1),
            Total = d.Total,
            Delivered = d.Delivered
        }).ToList();

        return new
        {
            Labels = successRates.Select(s => s.Department).ToArray(),
            Data = successRates.Select(s => s.SuccessRate).ToArray(),
            Details = successRates.Select(s => new { s.Total, s.Delivered }).ToArray()
        };
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public async Task<IActionResult> Admin()
    {
        // Sadece Muhaberat departmanından olanlar admin paneline erişebilir
        var currentUserDepartment = GetCurrentUserDepartment();
        if (currentUserDepartment != "Muhaberat")
        {
            TempData["Error"] = "Yönetim paneline erişim yetkiniz bulunmamaktadır.";
            return RedirectToAction("Index");
        }

        var adminStats = new
        {
            // Active entities
            TotalActiveUsers = await _context.Users.CountAsync(u => u.IsActive),
            TotalActiveDepartments = await _context.Departments.CountAsync(d => d.IsActive),
            TotalActiveRoles = await _context.Roles.CountAsync(r => r.IsActive),
            TotalActiveDocumentTypes = await _context.DocumentTypes.CountAsync(dt => dt.IsActive),

            // Inactive entities
            TotalInactiveUsers = await _context.Users.CountAsync(u => !u.IsActive),
            TotalInactiveDepartments = await _context.Departments.CountAsync(d => !d.IsActive),
            TotalInactiveRoles = await _context.Roles.CountAsync(r => !r.IsActive),
            TotalInactiveDocumentTypes = await _context.DocumentTypes.CountAsync(dt => !dt.IsActive),

            // Recent deactivations (last 30 days)
            RecentlyDeactivatedUsers = await _context.Users
                .Where(u => !u.IsActive && u.UpdatedAt >= DateTime.Now.AddDays(-30))
                .CountAsync(),
            
            RecentlyDeactivatedDepartments = await _context.Departments
                .Where(d => !d.IsActive && d.CreatedAt >= DateTime.Now.AddDays(-30))
                .CountAsync(),

            // Quick access data
            RecentInactiveUsers = await _context.Users
                .Include(u => u.Department)
                .Include(u => u.Role)
                .Where(u => !u.IsActive)
                .OrderByDescending(u => u.UpdatedAt)
                .Take(5)
                .ToListAsync(),

            RecentInactiveDepartments = await _context.Departments
                .Include(d => d.Unit)
                .Where(d => !d.IsActive)
                .OrderByDescending(d => d.CreatedAt)
                .Take(5)
                .ToListAsync()
        };

        ViewBag.AdminStats = adminStats;
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
