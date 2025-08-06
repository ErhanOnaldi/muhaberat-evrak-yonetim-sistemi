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
    public DbSet<DocumentTypeCategory> DocumentTypeCategories { get; set; }
    public DbSet<CargoTrackingLog> CargoTrackingLogs { get; set; }
    public DbSet<DocumentHistory> DocumentHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Document>()
            .HasOne(d => d.SenderUser)
            .WithMany(u => u.SentDocuments)
            .HasForeignKey(d => d.SenderUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Document>()
            .HasOne(d => d.ReceiverUser)
            .WithMany(u => u.ReceivedDocuments)
            .HasForeignKey(d => d.ReceiverUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Document>()
            .HasOne(d => d.CreatedByUser)
            .WithMany(u => u.CreatedDocuments)
            .HasForeignKey(d => d.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Document>()
            .HasOne(d => d.ReviewedByUser)
            .WithMany(u => u.ReviewedDocuments)
            .HasForeignKey(d => d.ReviewedBy)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Document>()
            .HasOne(d => d.SenderDepartment)
            .WithMany(dept => dept.SentDocuments)
            .HasForeignKey(d => d.SenderDepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Document>()
            .HasOne(d => d.ReceiverDepartment)
            .WithMany(dept => dept.ReceivedDocuments)
            .HasForeignKey(d => d.ReceiverDepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Document>()
            .HasOne(d => d.DocumentType)
            .WithMany(dt => dt.Documents)
            .HasForeignKey(d => d.DocumentTypeId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<DocumentHistory>()
            .HasOne(dh => dh.User)
            .WithMany(u => u.DocumentHistories)
            .HasForeignKey(dh => dh.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<DocumentHistory>()
            .HasOne(dh => dh.Document)
            .WithMany(d => d.DocumentHistories)
            .HasForeignKey(dh => dh.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CargoTrackingLog>()
            .HasOne(ct => ct.Document)
            .WithMany(d => d.CargoTrackingLogs)
            .HasForeignKey(ct => ct.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CargoTrackingLog>()
            .HasOne(ct => ct.UpdatedByUser)
            .WithMany()
            .HasForeignKey(ct => ct.UpdatedBy)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Department)
            .WithMany(d => d.Users)
            .HasForeignKey(u => u.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Unit)
            .WithMany(un => un.Users)
            .HasForeignKey(u => u.UnitId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Department>()
            .HasOne(d => d.Unit)
            .WithMany(u => u.Departments)
            .HasForeignKey(d => d.UnitId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DocumentPermission>()
            .HasOne(dp => dp.DocumentType)
            .WithMany(dt => dt.DocumentPermissions)
            .HasForeignKey(dp => dp.DocumentTypeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DocumentPermission>()
            .HasOne(dp => dp.Role)
            .WithMany(r => r.DocumentPermissions)
            .HasForeignKey(dp => dp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DocumentPermission>()
            .HasOne(dp => dp.Department)
            .WithMany(d => d.DocumentPermissions)
            .HasForeignKey(dp => dp.DepartmentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DocumentType>()
            .HasOne(dt => dt.Category)
            .WithMany(dtc => dtc.DocumentTypes)
            .HasForeignKey(dt => dt.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Constraints
        modelBuilder.Entity<Document>(entity =>
        {
            entity.ToTable(t => t.HasCheckConstraint("CK_Document_ReceiverCheck", "[ReceiverUserId] IS NOT NULL OR [ReceiverDepartmentId] IS NOT NULL"));
            entity.ToTable(t => t.HasCheckConstraint("CK_Document_DeliveryStatus", "[DeliveryStatus] IN ('PREPARING', 'SHIPPED', 'IN_TRANSIT', 'DELIVERED', 'RETURNED')"));
            entity.ToTable(t => t.HasCheckConstraint("CK_Document_Status", "[Status] IN ('DRAFT', 'SENT', 'DELIVERED', 'RECEIVED', 'CANCELLED')"));
            entity.ToTable(t => t.HasCheckConstraint("CK_Document_PhysicalDocumentType", "[PhysicalDocumentType] IS NULL OR [PhysicalDocumentType] IN ('ORIGINAL', 'COPY', 'NOTARIZED')"));
            entity.ToTable(t => t.HasCheckConstraint("CK_Document_PackageType", "[PackageType] IS NULL OR [PackageType] IN ('ENVELOPE', 'SMALL_PACKAGE', 'LARGE_PACKAGE', 'SPECIAL')"));
        });

        // Indexes for performance
        modelBuilder.Entity<Document>()
            .HasIndex(d => d.DocumentNumber)
            .IsUnique();

        modelBuilder.Entity<Document>()
            .HasIndex(d => d.CustomerId);

        modelBuilder.Entity<Document>()
            .HasIndex(d => d.CargoTrackingNumber);

        modelBuilder.Entity<Document>()
            .HasIndex(d => d.CreatedDate);

        modelBuilder.Entity<Document>()
            .HasIndex(d => d.Status);

        modelBuilder.Entity<Document>()
            .HasIndex(d => d.DeliveryStatus);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Department>()
            .HasIndex(d => d.DepartmentCode)
            .IsUnique();

        modelBuilder.Entity<Role>()
            .HasIndex(r => r.RoleCode)
            .IsUnique();

        modelBuilder.Entity<DocumentType>()
            .HasIndex(dt => dt.TypeCode)
            .IsUnique();
    }
}