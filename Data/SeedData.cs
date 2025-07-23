using muhaberat_evrak_yonetim.Models;
using muhaberat_evrak_yonetim.Entities;
using Microsoft.EntityFrameworkCore;

namespace muhaberat_evrak_yonetim.Data;

public static class SeedData
{
    public static async Task Initialize(DataContext context)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Check if data already exists
        if (await context.Units.AnyAsync())
        {
            return; // Data already seeded
        }

        // Create Units
        var units = new[]
        {
            new Unit
            {
                UnitName = "Mali İşler",
                Description = "Mali işlemler ve muhasebe birimi",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new Unit
            {
                UnitName = "İnsan Kaynakları",
                Description = "Personel ve insan kaynakları yönetimi",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new Unit
            {
                UnitName = "Operasyonel İşlemler",
                Description = "Günlük operasyon yönetimi",
                IsActive = true,
                CreatedAt = DateTime.Now
            }
        };

        await context.Units.AddRangeAsync(units);
        await context.SaveChangesAsync();

        // Create Departments
        var departments = new[]
        {
            new Department
            {
                DepartmentName = "Muhaberat",
                DepartmentCode = "MUH_BIRIM",
                Description = "Evrak gönderme ve alma işlemleri - Özel yetkiler",
                HasFullAccess = true,
                IsActive = true,
                CreatedAt = DateTime.Now,
                UnitId = units[0].Id
            },
            new Department
            {
                DepartmentName = "Muhasebe",
                DepartmentCode = "MUH",
                Description = "Mali muhasebe işlemleri",
                HasFullAccess = false,
                IsActive = true,
                CreatedAt = DateTime.Now,
                UnitId = units[0].Id
            },
            new Department
            {
                DepartmentName = "Şube Portföy Yönetimi",
                DepartmentCode = "SPY",
                Description = "Portföy yönetimi ve analizi",
                HasFullAccess = false,
                IsActive = true,
                CreatedAt = DateTime.Now,
                UnitId = units[2].Id
            },
            new Department
            {
                DepartmentName = "Müşteri İlişkileri Yönetimi",
                DepartmentCode = "MIY",
                Description = "Müşteri ilişkileri ve destek",
                HasFullAccess = false,
                IsActive = true,
                CreatedAt = DateTime.Now,
                UnitId = units[1].Id
            },
            new Department
            {
                DepartmentName = "Operasyon",
                DepartmentCode = "OPR",
                Description = "Operasyonel süreçler",
                HasFullAccess = false,
                IsActive = true,
                CreatedAt = DateTime.Now,
                UnitId = units[2].Id
            }
        };

        await context.Departments.AddRangeAsync(departments);
        await context.SaveChangesAsync();

        // Create Roles
        var roles = new[]
        {
            new Role
            {
                RoleName = "Muhasebe Uzmanı",
                RoleCode = "MUH_UZ",
                Description = "Mali işlemler uzmanı",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new Role
            {
                RoleName = "Portföy Yöneticisi",
                RoleCode = "PY",
                Description = "Portföy yönetimi uzmanı",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new Role
            {
                RoleName = "Müşteri İlişkileri Yöneticisi",
                RoleCode = "MIY",
                Description = "Müşteri ilişkileri uzmanı",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new Role
            {
                RoleName = "Operasyon Uzmanı",
                RoleCode = "OPR_UZ",
                Description = "Operasyonel süreç uzmanı",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new Role
            {
                RoleName = "Sistem Yöneticisi",
                RoleCode = "SYS_ADM",
                Description = "Sistem yönetimi ve teknik destek",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new Role
            {
                RoleName = "Muhaberat Uzmanı",
                RoleCode = "MUHABERAT",
                Description = "Evrak gönderme ve alma uzmanı",
                IsActive = true,
                CreatedAt = DateTime.Now
            }
        };

        await context.Roles.AddRangeAsync(roles);
        await context.SaveChangesAsync();

        // Create Document Types
        var documentTypes = new[]
        {
            new DocumentType
            {
                TypeName = "Kimlik Belgesi",
                TypeCode = "KIMLIK",
                Description = "Kimlik belgesi ve nüfus kayıt örnekleri",
                IsUrgent = false,
                RequiresSignature = true,
                EstimatedDeliveryDays = 2,
                PackageRequirements = "Kapalı zarf içinde",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new DocumentType
            {
                TypeName = "İmza Sirküleri",
                TypeCode = "IMZA_SIRK",
                Description = "Yetkili imza sirküleri ve örnekleri",
                IsUrgent = true,
                RequiresSignature = true,
                EstimatedDeliveryDays = 1,
                PackageRequirements = "Özel güvenlik zarfı",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new DocumentType
            {
                TypeName = "Mali Durum Belgesi",
                TypeCode = "MALI_DURUM",
                Description = "Mali durum raporları ve belgeleri",
                IsUrgent = false,
                RequiresSignature = false,
                EstimatedDeliveryDays = 3,
                PackageRequirements = "Standart zarf",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new DocumentType
            {
                TypeName = "Risk Değerlendirme Raporu",
                TypeCode = "RISK_DEG",
                Description = "Risk analizi ve değerlendirme raporları",
                IsUrgent = false,
                RequiresSignature = false,
                EstimatedDeliveryDays = 5,
                PackageRequirements = "Büyük zarf",
                IsActive = true,
                CreatedAt = DateTime.Now
            },
            new DocumentType
            {
                TypeName = "Sözleşme Belgeleri",
                TypeCode = "SOZLESME",
                Description = "Çeşitli sözleşme belgeleri",
                IsUrgent = true,
                RequiresSignature = true,
                EstimatedDeliveryDays = 1,
                PackageRequirements = "Özel güvenlik zarfı",
                IsActive = true,
                CreatedAt = DateTime.Now
            }
        };

        await context.DocumentTypes.AddRangeAsync(documentTypes);
        await context.SaveChangesAsync();

        // Create Users
        var users = new[]
        {
            new User
            {
                Username = "muhaberat_admin",
                Email = "muhaberat@firma.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                FirstName = "Ahmet",
                LastName = "Yılmaz",
                DepartmentId = departments[0].Id, // Muhaberat
                RoleId = roles[5].Id, // Muhaberat Uzmanı
                UnitId = units[0].Id,
                IsActive = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            },
            new User
            {
                Username = "muhasebe_uzman",
                Email = "muhasebe@firma.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                FirstName = "Ayşe",
                LastName = "Kara",
                DepartmentId = departments[1].Id, // Muhasebe
                RoleId = roles[0].Id, // Muhasebe Uzmanı
                UnitId = units[0].Id,
                IsActive = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            },
            new User
            {
                Username = "portfoy_yoneticisi",
                Email = "portfoy@firma.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                FirstName = "Mehmet",
                LastName = "Demir",
                DepartmentId = departments[2].Id, // Şube Portföy Yönetimi
                RoleId = roles[1].Id, // Portföy Yöneticisi
                UnitId = units[2].Id,
                IsActive = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            },
            new User
            {
                Username = "musteri_temsilcisi",
                Email = "musteri@firma.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                FirstName = "Fatma",
                LastName = "Özkan",
                DepartmentId = departments[3].Id, // Müşteri İlişkileri
                RoleId = roles[2].Id, // Müşteri İlişkileri Yöneticisi
                UnitId = units[1].Id,
                IsActive = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            },
            new User
            {
                Username = "operasyon_uzman",
                Email = "operasyon@firma.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                FirstName = "Ali",
                LastName = "Çelik",
                DepartmentId = departments[4].Id, // Operasyon
                RoleId = roles[3].Id, // Operasyon Uzmanı
                UnitId = units[2].Id,
                IsActive = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            }
        };

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();

        // Create Sample Documents
        var documents = new[]
        {
            new Document
            {
                DocumentNumber = "EVR-20250122-001",
                DocumentTypeId = documentTypes[0].Id, // Kimlik Belgesi
                Title = "Yeni Müşteri Kimlik Belgesi",
                Description = "Yeni müşteri kaydı için kimlik belgesi fotokopisi",
                SenderUserId = users[1].Id, // Muhasebe uzmanı
                SenderDepartmentId = departments[1].Id, // Muhasebe
                ReceiverUserId = users[0].Id, // Muhaberat admin
                ReceiverDepartmentId = departments[0].Id, // Muhaberat
                CustomerId = "MSTR001",
                CustomerName = "Ahmet Yılmaz",
                CreatedBy = users[1].Id,
                CreatedDate = DateTime.Now.AddDays(-5),
                PhysicalDocumentType = "COPY",
                PackageType = "ENVELOPE",
                ShippingAddress = "İstanbul Şube, Levent Mahallesi",
                DeliveryAddress = "Ankara Merkez, Kızılay",
                Status = "SENT",
                DeliveryStatus = "IN_TRANSIT",
                IsActive = true,
                CreatedAt = DateTime.Now.AddDays(-5),
                UpdatedAt = DateTime.Now.AddDays(-2)
            },
            new Document
            {
                DocumentNumber = "EVR-20250122-002",
                DocumentTypeId = documentTypes[1].Id, // İmza Sirküleri
                Title = "Yetki İmza Sirküleri Güncelleme",
                Description = "Şube yetki imza sirküleri güncelleme belgesi",
                SenderUserId = users[2].Id, // Portföy yöneticisi
                SenderDepartmentId = departments[2].Id, // Şube Portföy Yönetimi
                ReceiverDepartmentId = departments[0].Id, // Muhaberat
                CustomerId = "SUBE001",
                CustomerName = "İstanbul Şubesi",
                CreatedBy = users[2].Id,
                CreatedDate = DateTime.Now.AddDays(-3),
                PhysicalDocumentType = "ORIGINAL",
                PackageType = "SPECIAL",
                ShippingAddress = "İstanbul Şube Portföy Departmanı",
                DeliveryAddress = "Ankara Merkez Muhaberat",
                Status = "DRAFT",
                DeliveryStatus = "PREPARING",
                IsActive = true,
                CreatedAt = DateTime.Now.AddDays(-3),
                UpdatedAt = DateTime.Now.AddDays(-3)
            },
            new Document
            {
                DocumentNumber = "EVR-20250122-003",
                DocumentTypeId = documentTypes[2].Id, // Mali Durum Belgesi
                Title = "Aylık Mali Durum Raporu",
                Description = "Ocak 2025 aylık mali durum raporu",
                SenderUserId = users[1].Id, // Muhasebe uzmanı
                SenderDepartmentId = departments[1].Id, // Muhasebe
                ReceiverUserId = users[2].Id, // Portföy yöneticisi
                ReceiverDepartmentId = departments[2].Id, // Şube Portföy Yönetimi
                CreatedBy = users[1].Id,
                CreatedDate = DateTime.Now.AddDays(-1),
                PhysicalDocumentType = "COPY",
                PackageType = "LARGE_PACKAGE",
                ShippingAddress = "Mali İşler Muhasebe Departmanı",
                DeliveryAddress = "Operasyon Şube Portföy Yönetimi",
                CargoCompany = "Aras Kargo",
                CargoTrackingNumber = "1234567890",
                ShippingDate = DateTime.Now,
                Status = "SENT",
                DeliveryStatus = "SHIPPED",
                IsActive = true,
                CreatedAt = DateTime.Now.AddDays(-1),
                UpdatedAt = DateTime.Now
            }
        };

        await context.Documents.AddRangeAsync(documents);
        await context.SaveChangesAsync();

        // Create Document History entries
        var documentHistories = new[]
        {
            new DocumentHistory
            {
                DocumentId = documents[0].Id,
                ActionType = "UPLOADED",
                UserId = users[1].Id,
                ActionDate = DateTime.Now.AddDays(-5),
                Notes = "Evrak sisteme yüklendi",
                IpAddress = "192.168.1.100"
            },
            new DocumentHistory
            {
                DocumentId = documents[0].Id,
                ActionType = "SENT",
                UserId = users[1].Id,
                ActionDate = DateTime.Now.AddDays(-3),
                Notes = "Evrak muhaberata gönderildi",
                IpAddress = "192.168.1.100"
            },
            new DocumentHistory
            {
                DocumentId = documents[2].Id,
                ActionType = "UPLOADED",
                UserId = users[1].Id,
                ActionDate = DateTime.Now.AddDays(-1),
                Notes = "Mali durum raporu hazırlandı",
                IpAddress = "192.168.1.101"
            }
        };

        await context.DocumentHistories.AddRangeAsync(documentHistories);
        await context.SaveChangesAsync();

        // Create Cargo Tracking Logs
        var cargoLogs = new[]
        {
            new CargoTrackingLog
            {
                DocumentId = documents[2].Id,
                OldStatus = "PREPARING",
                NewStatus = "SHIPPED",
                StatusChangeDate = DateTime.Now,
                Location = "İstanbul Merkez",
                UpdatedBy = users[0].Id, // Muhaberat admin
                Notes = "Aras Kargo ile gönderildi - Takip No: 1234567890",
                CreatedAt = DateTime.Now
            }
        };

        await context.CargoTrackingLogs.AddRangeAsync(cargoLogs);
        await context.SaveChangesAsync();

        // Create Document Permissions
        var documentPermissions = new[]
        {
            // Muhaberat uzmanı tüm evrak türlerinde full yetki
            new DocumentPermission
            {
                DocumentTypeId = documentTypes[0].Id, // Kimlik
                RoleId = roles[5].Id, // Muhaberat Uzmanı
                CanView = true,
                CanUpload = true,
                CanDownload = true,
                CanReview = true,
                CanApprove = true,
                CreatedAt = DateTime.Now
            },
            new DocumentPermission
            {
                DocumentTypeId = documentTypes[1].Id, // İmza Sirküleri
                RoleId = roles[5].Id, // Muhaberat Uzmanı
                CanView = true,
                CanUpload = true,
                CanDownload = true,
                CanReview = true,
                CanApprove = true,
                CreatedAt = DateTime.Now
            },
            // Muhasebe uzmanı mali belgeler için yetki
            new DocumentPermission
            {
                DocumentTypeId = documentTypes[2].Id, // Mali Durum
                RoleId = roles[0].Id, // Muhasebe Uzmanı
                CanView = true,
                CanUpload = true,
                CanDownload = true,
                CanReview = false,
                CanApprove = false,
                CreatedAt = DateTime.Now
            },
            // Portföy yöneticisi risk raporları için yetki
            new DocumentPermission
            {
                DocumentTypeId = documentTypes[3].Id, // Risk Değerlendirme
                RoleId = roles[1].Id, // Portföy Yöneticisi
                CanView = true,
                CanUpload = true,
                CanDownload = true,
                CanReview = true,
                CanApprove = false,
                CreatedAt = DateTime.Now
            }
        };

        await context.DocumentPermissions.AddRangeAsync(documentPermissions);
        await context.SaveChangesAsync();
    }
}