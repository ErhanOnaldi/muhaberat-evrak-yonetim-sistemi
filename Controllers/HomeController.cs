using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        // Get comprehensive dashboard statistics
        var dashboardStats = new
        {
            // Document Statistics
            TotalDocuments = await _context.Documents.CountAsync(d => d.IsActive),
            TotalDocumentsThisMonth = await _context.Documents.CountAsync(d => d.IsActive && 
                d.CreatedDate.Month == DateTime.Now.Month && d.CreatedDate.Year == DateTime.Now.Year),
            PendingDocuments = await _context.Documents.CountAsync(d => d.IsActive && 
                d.Status != "DELIVERED" && d.Status != "RECEIVED"),
            DeliveredDocuments = await _context.Documents.CountAsync(d => d.IsActive && 
                (d.Status == "DELIVERED" || d.Status == "RECEIVED")),
            
            // Cargo Statistics
            InTransitCargo = await _context.Documents.CountAsync(d => d.IsActive && 
                (d.DeliveryStatus == "SHIPPED" || d.DeliveryStatus == "IN_TRANSIT")),
            DeliveredCargo = await _context.Documents.CountAsync(d => d.IsActive && 
                d.DeliveryStatus == "DELIVERED"),
            
            // User Statistics
            TotalUsers = await _context.Users.CountAsync(u => u.IsActive),
            TotalDepartments = await _context.Departments.CountAsync(d => d.IsActive),
            TotalRoles = await _context.Roles.CountAsync(r => r.IsActive),
            TotalDocumentTypes = await _context.DocumentTypes.CountAsync(dt => dt.IsActive),
            
            // Recent Activity
            RecentDocuments = await _context.Documents
                .Where(d => d.IsActive)
                .Include(d => d.DocumentType)
                .Include(d => d.SenderUser)
                .Include(d => d.SenderDepartment)
                .Include(d => d.ReceiverUser)
                .Include(d => d.ReceiverDepartment)
                .OrderByDescending(d => d.CreatedDate)
                .Take(5)
                .ToListAsync(),
                
            // Department Activity
            DepartmentActivity = await _context.Departments
                .Where(d => d.IsActive)
                .Include(d => d.SentDocuments.Where(doc => doc.IsActive))
                .OrderByDescending(d => d.SentDocuments.Count)
                .Take(5)
                .ToListAsync(),
                
            // Document Type Usage
            DocumentTypeUsage = await _context.DocumentTypes
                .Where(dt => dt.IsActive)
                .Include(dt => dt.Documents.Where(d => d.IsActive))
                .OrderByDescending(dt => dt.Documents.Count)
                .Take(6)
                .ToListAsync(),
                
            // Monthly Statistics (Last 6 months)
            MonthlyStats = await GetMonthlyStatistics()
        };

        ViewBag.DashboardStats = dashboardStats;
        return View();
    }

    private async Task<object> GetMonthlyStatistics()
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
                DocumentCount = await _context.Documents.CountAsync(d => d.IsActive && 
                    d.CreatedDate >= monthStart && d.CreatedDate <= monthEnd),
                DeliveredCount = await _context.Documents.CountAsync(d => d.IsActive && 
                    d.CreatedDate >= monthStart && d.CreatedDate <= monthEnd &&
                    (d.Status == "DELIVERED" || d.Status == "RECEIVED"))
            };
            
            monthlyData.Add(monthStats);
        }
        
        return monthlyData;
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
