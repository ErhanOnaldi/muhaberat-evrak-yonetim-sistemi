using muhaberat_evrak_yonetim.Models;
using muhaberat_evrak_yonetim.Entities;
using muhaberat_evrak_yonetim.Enums;
using Microsoft.EntityFrameworkCore;

namespace muhaberat_evrak_yonetim.Data;

public static class SeedData
{
    public static async Task Initialize(DataContext context)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // GERÇEK VERİLER - Her zaman kontrol et ve yükle
        await SeedRealData(context);

        // ÖRNEK VERİLER - Sadece yoksa yükle
        if (await context.Units.AnyAsync())
        {
            return; // Sample data already seeded
        }

        await SeedSampleData(context);
    }

    private static async Task SeedRealData(DataContext context)
    {
        // Document Type Categories - GERÇEK veriler
        if (!await context.DocumentTypeCategories.AnyAsync())
        {
            var categories = new[]
            {
                new DocumentTypeCategory { CategoryName = "Bireysel Müşteri Devir Evrakları", CategoryCode = "BMDE", Description = "Bireysel müşteri devir işlemleri için evraklar", IsActive = true, CreatedAt = DateTime.Now },
                new DocumentTypeCategory { CategoryName = "Bireysel Müşteri Kayıt Evrakları", CategoryCode = "BMKE", Description = "Bireysel müşteri kayıt işlemleri için evraklar", IsActive = true, CreatedAt = DateTime.Now },
                new DocumentTypeCategory { CategoryName = "Bireysel Müşteri Virman Evrakları", CategoryCode = "BMVE", Description = "Bireysel müşteri virman işlemleri için evraklar", IsActive = true, CreatedAt = DateTime.Now },
                new DocumentTypeCategory { CategoryName = "Kurumsal Müşteri Devir Evrakları", CategoryCode = "KMDE", Description = "Kurumsal müşteri devir işlemleri için evraklar", IsActive = true, CreatedAt = DateTime.Now },
                new DocumentTypeCategory { CategoryName = "Kurumsal Müşteri Kayıt Evrakları", CategoryCode = "KMKE", Description = "Kurumsal müşteri kayıt işlemleri için evraklar", IsActive = true, CreatedAt = DateTime.Now },
                new DocumentTypeCategory { CategoryName = "Kurumsal Müşteri Virman Evrakları", CategoryCode = "KMVE", Description = "Kurumsal müşteri virman işlemleri için evraklar", IsActive = true, CreatedAt = DateTime.Now },
                new DocumentTypeCategory { CategoryName = "Tahsisat Evrakları", CategoryCode = "TE", Description = "Tahsisat işlemleri için evraklar", IsActive = true, CreatedAt = DateTime.Now },
                new DocumentTypeCategory { CategoryName = "Ayrılma Evrakları", CategoryCode = "AE", Description = "Ayrılma işlemleri için evraklar", IsActive = true, CreatedAt = DateTime.Now },
                new DocumentTypeCategory { CategoryName = "Teminat/Ekspertiz Evrakları Araç", CategoryCode = "TEEA", Description = "Araç teminat ve ekspertiz evrakları", IsActive = true, CreatedAt = DateTime.Now },
                new DocumentTypeCategory { CategoryName = "Teminat/Ekspertiz Evrakları Konut", CategoryCode = "TEEK", Description = "Konut teminat ve ekspertiz evrakları", IsActive = true, CreatedAt = DateTime.Now },
                new DocumentTypeCategory { CategoryName = "Satış Sonrası Evrakları Araç", CategoryCode = "SSEA", Description = "Araç satış sonrası evrakları", IsActive = true, CreatedAt = DateTime.Now },
                new DocumentTypeCategory { CategoryName = "Satış Sonrası Evrakları Konut", CategoryCode = "SSEK", Description = "Konut satış sonrası evrakları", IsActive = true, CreatedAt = DateTime.Now }
            };

            await context.DocumentTypeCategories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedSampleData(DataContext context)
    {

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

        // Document Types - GERÇEK veriler (statik)
        if (!await context.DocumentTypes.AnyAsync())
        {
            await SeedDocumentTypes(context);
        }

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

        // Create sample documents with proper category relationships
        await SeedSampleDocuments(context, users, departments);

        // Sample document histories, cargo logs, and permissions will be managed through the UI
    }

    private static async Task SeedDocumentTypes(DataContext context)
    {
        // Get category IDs
        var categories = await context.DocumentTypeCategories.ToDictionaryAsync(c => c.CategoryName, c => c.Id);

        var documentTypes = new[]
        {
            // Bireysel Müşteri Devir Evrakları
            new DocumentType { TypeName = "Ön Bilgilendirme Formu", TypeCode = "BMDE_ONBILG", CategoryId = categories["Bireysel Müşteri Devir Evrakları"], Description = "Bireysel müşteri devir işlemleri için ön bilgilendirme formu", IsActive = true, IsUrgent = false, RequiresSignature = true, EstimatedDeliveryDays = 2, PackageRequirements = "Standart zarf", CreatedAt = DateTime.Now },
            new DocumentType { TypeName = "Kimlik Fotokopisi", TypeCode = "BMDE_KIMLIK", CategoryId = categories["Bireysel Müşteri Devir Evrakları"], Description = "Bireysel müşteri devir işlemleri için kimlik fotokopisi", IsActive = true, IsUrgent = false, RequiresSignature = false, EstimatedDeliveryDays = 1, PackageRequirements = "Standart zarf", CreatedAt = DateTime.Now },
            new DocumentType { TypeName = "İmza Beyannamesi", TypeCode = "BMDE_IMZA", CategoryId = categories["Bireysel Müşteri Devir Evrakları"], Description = "Bireysel müşteri devir işlemleri için imza beyannamesi", IsActive = true, IsUrgent = false, RequiresSignature = true, EstimatedDeliveryDays = 2, PackageRequirements = "Standart zarf", CreatedAt = DateTime.Now },
            new DocumentType { TypeName = "Devir Sözleşmesi", TypeCode = "BMDE_SOZLES", CategoryId = categories["Bireysel Müşteri Devir Evrakları"], Description = "Bireysel müşteri devir işlemleri için devir sözleşmesi", IsActive = true, IsUrgent = true, RequiresSignature = true, EstimatedDeliveryDays = 3, PackageRequirements = "Özel zarf", CreatedAt = DateTime.Now },

            // Kurumsal Müşteri Kayıt Evrakları
            new DocumentType { TypeName = "Sözleşme", TypeCode = "KMKE_SOZLES", CategoryId = categories["Kurumsal Müşteri Kayıt Evrakları"], Description = "Kurumsal müşteri kayıt işlemleri için sözleşme", IsActive = true, IsUrgent = true, RequiresSignature = true, EstimatedDeliveryDays = 3, PackageRequirements = "Özel zarf", CreatedAt = DateTime.Now },
            new DocumentType { TypeName = "Kimlik Fotokopisi", TypeCode = "KMKE_KIMLIK", CategoryId = categories["Kurumsal Müşteri Kayıt Evrakları"], Description = "Kurumsal müşteri kayıt işlemleri için kimlik fotokopisi", IsActive = true, IsUrgent = false, RequiresSignature = false, EstimatedDeliveryDays = 1, PackageRequirements = "Standart zarf", CreatedAt = DateTime.Now },
            new DocumentType { TypeName = "Dekont", TypeCode = "KMKE_DEKONT", CategoryId = categories["Kurumsal Müşteri Kayıt Evrakları"], Description = "Kurumsal müşteri kayıt işlemleri için dekont", IsActive = true, IsUrgent = false, RequiresSignature = false, EstimatedDeliveryDays = 1, PackageRequirements = "Standart zarf", CreatedAt = DateTime.Now },
            new DocumentType { TypeName = "Katılım Payı Dilekçesi", TypeCode = "KMKE_KATPAY", CategoryId = categories["Kurumsal Müşteri Kayıt Evrakları"], Description = "Kurumsal müşteri kayıt işlemleri için katılım payı dilekçesi", IsActive = true, IsUrgent = false, RequiresSignature = true, EstimatedDeliveryDays = 2, PackageRequirements = "Standart zarf", CreatedAt = DateTime.Now },
            new DocumentType { TypeName = "Ön Bilgilendirme Formu", TypeCode = "KMKE_ONBILG", CategoryId = categories["Kurumsal Müşteri Kayıt Evrakları"], Description = "Kurumsal müşteri kayıt işlemleri için ön bilgilendirme formu", IsActive = true, IsUrgent = false, RequiresSignature = true, EstimatedDeliveryDays = 2, PackageRequirements = "Standart zarf", CreatedAt = DateTime.Now },
            new DocumentType { TypeName = "Vergi Levhası", TypeCode = "KMKE_VERGIL", CategoryId = categories["Kurumsal Müşteri Kayıt Evrakları"], Description = "Kurumsal müşteri kayıt işlemleri için vergi levhası", IsActive = true, IsUrgent = false, RequiresSignature = false, EstimatedDeliveryDays = 2, PackageRequirements = "Standart zarf", CreatedAt = DateTime.Now },
            new DocumentType { TypeName = "Ödeme Planı", TypeCode = "KMKE_ODEME", CategoryId = categories["Kurumsal Müşteri Kayıt Evrakları"], Description = "Kurumsal müşteri kayıt işlemleri için ödeme planı", IsActive = true, IsUrgent = false, RequiresSignature = false, EstimatedDeliveryDays = 2, PackageRequirements = "Standart zarf", CreatedAt = DateTime.Now },
            new DocumentType { TypeName = "İmza Beyannamesi", TypeCode = "KMKE_IMZA", CategoryId = categories["Kurumsal Müşteri Kayıt Evrakları"], Description = "Kurumsal müşteri kayıt işlemleri için imza beyannamesi", IsActive = true, IsUrgent = false, RequiresSignature = true, EstimatedDeliveryDays = 2, PackageRequirements = "Standart zarf", CreatedAt = DateTime.Now },
            new DocumentType { TypeName = "Şirketin Faaliyet Belgesi", TypeCode = "KMKE_FAAL", CategoryId = categories["Kurumsal Müşteri Kayıt Evrakları"], Description = "Kurumsal müşteri kayıt işlemleri için şirket faaliyet belgesi", IsActive = true, IsUrgent = false, RequiresSignature = false, EstimatedDeliveryDays = 3, PackageRequirements = "Büyük zarf", CreatedAt = DateTime.Now },
            new DocumentType { TypeName = "İmza Sirküleri", TypeCode = "KMKE_SIRK", CategoryId = categories["Kurumsal Müşteri Kayıt Evrakları"], Description = "Kurumsal müşteri kayıt işlemleri için imza sirküleri", IsActive = true, IsUrgent = true, RequiresSignature = true, EstimatedDeliveryDays = 2, PackageRequirements = "Özel zarf", CreatedAt = DateTime.Now },
            new DocumentType { TypeName = "Ticaret Sicil Gazete Yazısı", TypeCode = "KMKE_TICSIC", CategoryId = categories["Kurumsal Müşteri Kayıt Evrakları"], Description = "Kurumsal müşteri kayıt işlemleri için ticaret sicil gazete yazısı", IsActive = true, IsUrgent = false, RequiresSignature = false, EstimatedDeliveryDays = 3, PackageRequirements = "Büyük zarf", CreatedAt = DateTime.Now },
            new DocumentType { TypeName = "Virman Dilekçesi", TypeCode = "KMKE_VIRMAN", CategoryId = categories["Kurumsal Müşteri Kayıt Evrakları"], Description = "Kurumsal müşteri kayıt işlemleri için virman dilekçesi", IsActive = true, IsUrgent = false, RequiresSignature = true, EstimatedDeliveryDays = 2, PackageRequirements = "Standart zarf", CreatedAt = DateTime.Now },

            // Bireysel Müşteri Kayıt Evrakları
            new DocumentType { TypeName = "Sözleşme", TypeCode = "BMKE_SOZLES", CategoryId = categories["Bireysel Müşteri Kayıt Evrakları"], Description = "Bireysel müşteri kayıt işlemleri için sözleşme", IsActive = true, IsUrgent = true, RequiresSignature = true, EstimatedDeliveryDays = 3, PackageRequirements = "Özel zarf", CreatedAt = DateTime.Now },
            new DocumentType { TypeName = "Kimlik Fotokopisi", TypeCode = "BMKE_KIMLIK", CategoryId = categories["Bireysel Müşteri Kayıt Evrakları"], Description = "Bireysel müşteri kayıt işlemleri için kimlik fotokopisi", IsActive = true, IsUrgent = false, RequiresSignature = false, EstimatedDeliveryDays = 1, PackageRequirements = "Standart zarf", CreatedAt = DateTime.Now },
            new DocumentType { TypeName = "Dekont", TypeCode = "BMKE_DEKONT", CategoryId = categories["Bireysel Müşteri Kayıt Evrakları"], Description = "Bireysel müşteri kayıt işlemleri için dekont", IsActive = true, IsUrgent = false, RequiresSignature = false, EstimatedDeliveryDays = 1, PackageRequirements = "Standart zarf", CreatedAt = DateTime.Now },
            new DocumentType { TypeName = "Katılım Payı Dilekçesi", TypeCode = "BMKE_KATPAY", CategoryId = categories["Bireysel Müşteri Kayıt Evrakları"], Description = "Bireysel müşteri kayıt işlemleri için katılım payı dilekçesi", IsActive = true, IsUrgent = false, RequiresSignature = true, EstimatedDeliveryDays = 2, PackageRequirements = "Standart zarf", CreatedAt = DateTime.Now },
            new DocumentType { TypeName = "Ön Bilgilendirme Formu", TypeCode = "BMKE_ONBILG", CategoryId = categories["Bireysel Müşteri Kayıt Evrakları"], Description = "Bireysel müşteri kayıt işlemleri için ön bilgilendirme formu", IsActive = true, IsUrgent = false, RequiresSignature = true, EstimatedDeliveryDays = 2, PackageRequirements = "Standart zarf", CreatedAt = DateTime.Now },
            new DocumentType { TypeName = "Ödeme Planı", TypeCode = "BMKE_ODEME", CategoryId = categories["Bireysel Müşteri Kayıt Evrakları"], Description = "Bireysel müşteri kayıt işlemleri için ödeme planı", IsActive = true, IsUrgent = false, RequiresSignature = false, EstimatedDeliveryDays = 2, PackageRequirements = "Standart zarf", CreatedAt = DateTime.Now },
            new DocumentType { TypeName = "İmza Beyannamesi", TypeCode = "BMKE_IMZA", CategoryId = categories["Bireysel Müşteri Kayıt Evrakları"], Description = "Bireysel müşteri kayıt işlemleri için imza beyannamesi", IsActive = true, IsUrgent = false, RequiresSignature = true, EstimatedDeliveryDays = 2, PackageRequirements = "Standart zarf", CreatedAt = DateTime.Now },
            new DocumentType { TypeName = "Virman Dilekçesi", TypeCode = "BMKE_VIRMAN", CategoryId = categories["Bireysel Müşteri Kayıt Evrakları"], Description = "Bireysel müşteri kayıt işlemleri için virman dilekçesi", IsActive = true, IsUrgent = false, RequiresSignature = true, EstimatedDeliveryDays = 2, PackageRequirements = "Standart zarf", CreatedAt = DateTime.Now },

            // Kurumsal Müşteri Devir Evrakları
            new DocumentType { TypeName = "Ön Bilgilendirme Formu", TypeCode = "KMDE_ONBILG", CategoryId = categories["Kurumsal Müşteri Devir Evrakları"], Description = "Kurumsal müşteri devir işlemleri için ön bilgilendirme formu", IsActive = true, IsUrgent = false, RequiresSignature = true, EstimatedDeliveryDays = 2, PackageRequirements = "Standart zarf", CreatedAt = DateTime.Now },
            new DocumentType { TypeName = "İmza Beyannamesi", TypeCode = "KMDE_IMZA", CategoryId = categories["Kurumsal Müşteri Devir Evrakları"], Description = "Kurumsal müşteri devir işlemleri için imza beyannamesi", IsActive = true, IsUrgent = false, RequiresSignature = true, EstimatedDeliveryDays = 2, PackageRequirements = "Standart zarf", CreatedAt = DateTime.Now },
            new DocumentType { TypeName = "Devir Sözleşmesi", TypeCode = "KMDE_SOZLES", CategoryId = categories["Kurumsal Müşteri Devir Evrakları"], Description = "Kurumsal müşteri devir işlemleri için devir sözleşmesi", IsActive = true, IsUrgent = true, RequiresSignature = true, EstimatedDeliveryDays = 3, PackageRequirements = "Özel zarf", CreatedAt = DateTime.Now },
            new DocumentType { TypeName = "Vergi Levhası", TypeCode = "KMDE_VERGIL", CategoryId = categories["Kurumsal Müşteri Devir Evrakları"], Description = "Kurumsal müşteri devir işlemleri için vergi levhası", IsActive = true, IsUrgent = false, RequiresSignature = false, EstimatedDeliveryDays = 2, PackageRequirements = "Standart zarf", CreatedAt = DateTime.Now },
            new DocumentType { TypeName = "Şirketin Faaliyet Belgesi", TypeCode = "KMDE_FAAL", CategoryId = categories["Kurumsal Müşteri Devir Evrakları"], Description = "Kurumsal müşteri devir işlemleri için şirket faaliyet belgesi", IsActive = true, IsUrgent = false, RequiresSignature = false, EstimatedDeliveryDays = 3, PackageRequirements = "Büyük zarf", CreatedAt = DateTime.Now },
            new DocumentType { TypeName = "İmza Sirküleri", TypeCode = "KMDE_SIRK", CategoryId = categories["Kurumsal Müşteri Devir Evrakları"], Description = "Kurumsal müşteri devir işlemleri için imza sirküleri", IsActive = true, IsUrgent = true, RequiresSignature = true, EstimatedDeliveryDays = 2, PackageRequirements = "Özel zarf", CreatedAt = DateTime.Now },
            new DocumentType { TypeName = "Ticaret Sicil Gazete Yazısı", TypeCode = "KMDE_TICSIC", CategoryId = categories["Kurumsal Müşteri Devir Evrakları"], Description = "Kurumsal müşteri devir işlemleri için ticaret sicil gazete yazısı", IsActive = true, IsUrgent = false, RequiresSignature = false, EstimatedDeliveryDays = 3, PackageRequirements = "Büyük zarf", CreatedAt = DateTime.Now },

            // Tahsisat Evrakları (örnekler)
            new DocumentType { TypeName = "Kimlik", TypeCode = "TE_KIMLIK", CategoryId = categories["Tahsisat Evrakları"], Description = "Tahsisat işlemleri için kimlik belgesi", IsActive = true, IsUrgent = false, RequiresSignature = false, EstimatedDeliveryDays = 1, PackageRequirements = "Standart zarf", CreatedAt = DateTime.Now },
            new DocumentType { TypeName = "Maaş Bordrosu", TypeCode = "TE_MAAS", CategoryId = categories["Tahsisat Evrakları"], Description = "Tahsisat işlemleri için maaş bordrosu", IsActive = true, IsUrgent = false, RequiresSignature = false, EstimatedDeliveryDays = 2, PackageRequirements = "Standart zarf", CreatedAt = DateTime.Now },
            new DocumentType { TypeName = "Kefalet Sözleşmesi", TypeCode = "TE_KEFALET", CategoryId = categories["Tahsisat Evrakları"], Description = "Tahsisat işlemleri için kefalet sözleşmesi", IsActive = true, IsUrgent = true, RequiresSignature = true, EstimatedDeliveryDays = 3, PackageRequirements = "Özel zarf", CreatedAt = DateTime.Now },

            // Ayrılma Evrakları
            new DocumentType { TypeName = "Ayrılma Dilekçesi", TypeCode = "AE_DILEKCE", CategoryId = categories["Ayrılma Evrakları"], Description = "Ayrılma işlemleri için dilekçe", IsActive = true, IsUrgent = false, RequiresSignature = true, EstimatedDeliveryDays = 2, PackageRequirements = "Standart zarf", CreatedAt = DateTime.Now },
            new DocumentType { TypeName = "Ölüm Belgesi", TypeCode = "AE_OLUM", CategoryId = categories["Ayrılma Evrakları"], Description = "Ayrılma işlemleri için ölüm belgesi", IsActive = true, IsUrgent = true, RequiresSignature = false, EstimatedDeliveryDays = 1, PackageRequirements = "Özel güvenlik zarfı", CreatedAt = DateTime.Now },

            // Araç ve Konut evrakları (örnekler)
            new DocumentType { TypeName = "Ruhsat Fotokopisi", TypeCode = "TEEA_RUHSAT", CategoryId = categories["Teminat/Ekspertiz Evrakları Araç"], Description = "Araç teminat ve ekspertiz için ruhsat fotokopisi", IsActive = true, IsUrgent = false, RequiresSignature = false, EstimatedDeliveryDays = 1, PackageRequirements = "Standart zarf", CreatedAt = DateTime.Now },
            new DocumentType { TypeName = "Tapu", TypeCode = "TEEK_TAPU", CategoryId = categories["Teminat/Ekspertiz Evrakları Konut"], Description = "Konut teminat ve ekspertiz için tapu", IsActive = true, IsUrgent = true, RequiresSignature = false, EstimatedDeliveryDays = 3, PackageRequirements = "Özel güvenlik zarfı", CreatedAt = DateTime.Now }
        };

        await context.DocumentTypes.AddRangeAsync(documentTypes);
        await context.SaveChangesAsync();
    }

    private static async Task SeedSampleDocuments(DataContext context, User[] users, Department[] departments)
    {
        // Get some document types for sample documents
        var documentTypes = await context.DocumentTypes.Take(10).ToListAsync();
        if (!documentTypes.Any()) return;

        var sampleDocuments = new[]
        {
            new Document
            {
                DocumentNumber = "DOC001",
                Title = "Ahmet Yılmaz - Kimlik Fotokopisi",
                DocumentTypeId = documentTypes.First(dt => dt.TypeCode == "BMKE_KIMLIK").Id,
                Description = "Bireysel müşteri kayıt işlemi için kimlik belgesi",
                SenderUserId = users[0].Id, // Muhaberat admin
                ReceiverUserId = users[1].Id, // Muhasebe uzman
                SenderDepartmentId = departments[0].Id, // Muhaberat
                ReceiverDepartmentId = departments[1].Id, // Muhasebe
                CreatedBy = users[0].Id,
                CustomerName = "Ahmet Yılmaz",
                CustomerId = "12345678901",
                PhysicalDocumentType = "COPY",
                Status = "DRAFT",
                IsActive = true,
                CreatedDate = DateTime.Now.AddDays(-5),
                CreatedAt = DateTime.Now.AddDays(-5),
                UpdatedAt = DateTime.Now.AddDays(-5)
            },
            new Document
            {
                DocumentNumber = "DOC002",
                Title = "ABC Şirketi - İmza Sirküleri",
                DocumentTypeId = documentTypes.First(dt => dt.TypeCode == "KMKE_SIRK").Id,
                Description = "Kurumsal müşteri kayıt işlemi için imza sirküleri",
                SenderUserId = users[0].Id,
                ReceiverUserId = users[2].Id, // Portföy yöneticisi
                SenderDepartmentId = departments[0].Id,
                ReceiverDepartmentId = departments[2].Id, // Şube Portföy Yönetimi
                CreatedBy = users[0].Id,
                CustomerName = "ABC Şirketi",
                CustomerId = "1234567890",
                PhysicalDocumentType = "ORIGINAL",
                Status = "SENT",
                IsActive = true,
                CreatedDate = DateTime.Now.AddDays(-3),
                CreatedAt = DateTime.Now.AddDays(-3),
                UpdatedAt = DateTime.Now.AddDays(-2)
            },
            new Document
            {
                DocumentNumber = "DOC003",
                Title = "Mehmet Demir - Devir Sözleşmesi",
                DocumentTypeId = documentTypes.First(dt => dt.TypeCode == "BMDE_SOZLES").Id,
                Description = "Bireysel müşteri devir işlemi için sözleşme belgesi",
                SenderUserId = users[3].Id, // Müşteri temsilcisi
                ReceiverUserId = users[0].Id,
                SenderDepartmentId = departments[3].Id, // Müşteri İlişkileri
                ReceiverDepartmentId = departments[0].Id,
                CreatedBy = users[3].Id,
                CustomerName = "Mehmet Demir",
                CustomerId = "98765432109",
                PhysicalDocumentType = "NOTARIZED",
                Status = "DELIVERED",
                IsActive = true,
                CreatedDate = DateTime.Now.AddDays(-7),
                CreatedAt = DateTime.Now.AddDays(-7),
                UpdatedAt = DateTime.Now.AddDays(-2)
            }
        };

        await context.Documents.AddRangeAsync(sampleDocuments);
        await context.SaveChangesAsync();

        // Sample cargo data and tracking logs will be added when needed
    }
}