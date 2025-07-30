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

            MonthlyStats = await GetMonthlyStatistics(documentsQuery)
        };

        // Debug bilgisi ekle
        _logger.LogInformation($"Total Document Types: {await _context.DocumentTypes.CountAsync(dt => dt.IsActive)}");
        _logger.LogInformation($"Total Documents (filtered): {await documentsQuery.CountAsync()}");
        _logger.LogInformation($"DocumentTypeUsage count: {dashboardStats.DocumentTypeUsage.Count}");
        _logger.LogInformation($"Current User ID: {currentUserId}, Department: {currentUserDepartment}, HasFullAccess: {hasFullAccess}");
        
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

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
