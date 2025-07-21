using Microsoft.EntityFrameworkCore;
using muhaberat_evrak_yonetim.Entities;

namespace muhaberat_evrak_yonetim.Models;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Unit> Units { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<DocumentPermission> DocumentPermissions { get; set; }
    public DbSet<DocumentType> DocumentTypes { get; set; }
    public DbSet<CargoTrackingLog> CargoTrackingLogs { get; set; }
    public DbSet<DocumentHistory> DocumentHistories { get; set; }
}