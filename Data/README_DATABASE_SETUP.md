# Database Setup Talimatları

## Evrak Türü ve Kategori Sisteminin Kurulumu

### 1. Veritabanı Durumu Kontrolü

Önce mevcut veritabanı durumunu kontrol edin:

SELECT COUNT(*) as CategoryCount FROM DocumentTypeCategories;


SELECT COUNT(*) as DocumentTypeCount FROM DocumentTypes;
```

### 2. Kategorilerin Otomatik Yüklenmesi

**SeedData.cs** dosyası artık kategorileri otomatik yükler:
- Uygulama her başladığında kategoriler kontrol edilir
- Eksik kategoriler otomatik eklenir
- Bu işlem gerçek veriler için yapılır (örnek değil)

### 3. Evrak Türlerinin Manuel Yüklenmesi

#### Seçenek A: Tam SQL Script (Önerilen)

-- CompleteDocumentTypesImport.sql dosyasını çalıştır
-- Bu script 112 evrak türünü kategorilere göre ekler


#### Seçenek B: Kısmi Import
```sql
-- ImportDocumentTypesFromCsv.sql dosyasını çalıştır  
-- Bu script sadece temel evrak türlerini ekler
```

### 4. Veritabanı Sıfırlama (Development)

Eğer tamamen yeni başlamak istiyorsanız:

```bash
# Migration'ları sil ve yeniden oluştur
dotnet ef database drop --force
dotnet ef database update
```

Sonra uyglamayı başlatın, kategoriler otomatik yüklenecek.

### 5. Production Kurulumu

Production ortamında:

1. **Kategoriler**: Otomatik yüklenir (SeedData.cs)
2. **Evrak Türleri**: `CompleteDocumentTypesImport.sql` script'ini **bir kez** çalıştır
3. **Kullanıcılar**: Gerçek kullanıcıları UI'dan ekle

### 6. Veri Durumu Kontrolü

Import sonrası kontrol:
```sql
-- Kategori başına evrak türü sayısı
SELECT 
    c.CategoryName,
    c.CategoryCode,
    COUNT(dt.Id) as DocumentTypeCount
FROM DocumentTypeCategories c
LEFT JOIN DocumentTypes dt ON c.Id = dt.CategoryId
GROUP BY c.CategoryName, c.CategoryCode
ORDER BY c.CategoryName;

-- Toplam sayılar
SELECT 
    (SELECT COUNT(*) FROM DocumentTypeCategories WHERE IsActive = 1) as ActiveCategories,
    (SELECT COUNT(*) FROM DocumentTypes WHERE IsActive = 1) as ActiveDocumentTypes;
```

### 7. Sistem Özellikleri

✅ **Gerçek Veriler**: Kategoriler ve evrak türleri gerçek iş verileri
✅ **Otomatik Kategoriler**: Her uygulama başlatmada kontrol edilir
✅ **Manuel Evrak Türleri**: SQL script ile bir kez yüklenir
✅ **UI Yönetimi**: Sonrasında tüm yönetim UI üzerinden
✅ **Duplicate Koruması**: Aynı evrak türü iki kez eklenmez

### 8. Sorun Giderme

**Kategoriler yüklenmiyor**: SeedData.cs kontrol et
**Evrak türleri eksik**: SQL script'i çalıştır
**Duplicate error**: Script'te zaten duplicate kontrolü var

### 9. CSV Dosyası

`DocumentType.csv` dosyası sadece referans için kalıyor.
Artık uygulama bu dosyadan otomatik okumaz.
Tüm veriler database'de statik olarak saklanır.