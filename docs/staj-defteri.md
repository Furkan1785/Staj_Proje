# Staj Defteri — TicariSistem (30 İş Günü)

Her gün için Yapılan iş / Kullanılan teknoloji-yöntem / Karşılaşılan sorun
formatında tutuldu. Görevler, projenin başında belirlenen 30 günlük plana
birebir karşılık gelir. Proje sonu itibarıyla: 28 controller, 50 servis,
35 entity, 64 ViewModel, 92 Razor view, 21 EF Core migration, 118 xUnit testi.

---

## 1. HAFTA — Altyapı ve Finans

### Gün 1 - 2026-06-30
**Yapılan iş:** .NET 8 SDK, PostgreSQL ve Docker kurulumu doğrulandı. `dotnet new mvc` ile proje iskeleti oluşturuldu, git deposu başlatıldı ve ilk commit atıldı. `web` (ASP.NET Core) ve `postgres` servislerini içeren ilk `docker-compose.yml` yazıldı; ihtiyaç duyulacak NuGet paketleri (EF Core, Npgsql, Identity, QuestPDF, ClosedXML) projeye eklendi.
**Kullanılan teknoloji/yöntem:** ASP.NET Core 8 MVC şablonu, Docker Compose, NuGet paket yönetimi.
**Karşılaşılan sorun:** Yok — kurulum sorunsuz tamamlandı.

### Gün 2 - 2026-07-01
**Yapılan iş:** Projedeki tüm modüllerin (Altyapı, Finans, Muhasebe, Malzeme, Satınalma, Satış) varlık-ilişki diyagramı mermaid formatında `docs/er-diyagram.md`'ye çizildi. İlk taslak sonrası kapsam genişletilerek Şube, ortak `BaseEntity` alanları, AlışIrsaliyesi ve hiyerarşik MalzemeKategori de diyagrama eklendi; tüm 1-N/N-1 ilişkiler ve cascade davranışları belgelendi.
**Kullanılan teknoloji/yöntem:** Mermaid ER diyagramı sözdizimi.
**Karşılaşılan sorun:** İlk taslakta Sube ve MalzemeKategori gibi bazı destek entity'leri unutulmuştu; aynı gün ikinci bir geçişle diyagram tamamlandı.

### Gün 3 - 2026-07-02
**Yapılan iş:** Mimari katmanlar kuruldu: tüm entity'lerin türeyeceği soyut `BaseEntity` sınıfı (Id, CreatedAt, UpdatedAt, CreatedBy, IsDeleted — soft delete ve audit için), generic `IRepository<T>` arayüzü ve EF Core tabanlı `BaseRepository<T>` implementasyonu, `IUnitOfWork` deseni. `AppDbContext` oluşturulup tüm entity'ler `DbSet` olarak kaydedildi; `InitialCreate` migration'ı üretildi.
**Kullanılan teknoloji/yöntem:** EF Core Code First, Repository/UnitOfWork tasarım deseni, `dotnet ef migrations add`.
**Karşılaşılan sorun:** Yok.

### Gün 4 - 2026-07-03
**Yapılan iş:** Global `ExceptionHandlingMiddleware` (hatayı loglayıp kullanıcıya Türkçe hata sayfası gösteren) ve `RequestResponseLoggingMiddleware` eklendi. Cari entity'si (CariKodu, Unvan, CariTipi enum, VergiNo, Adres, Telefon, EMail, Bakiye, KrediLimiti) ile birlikte `CariService`/`CariRepository` katmanı yazıldı.
**Kullanılan teknoloji/yöntem:** ASP.NET Core custom middleware pipeline, Serilog yapılandırılmış loglama.
**Karşılaşılan sorun:** Yok.

### Gün 5 - 2026-07-06
**Yapılan iş:** Cari listesi ekranı için sunucu tarafı DataTables entegrasyonu yazıldı (büyük veri setinde performanslı sayfalama/sıralama/filtreleme için ayrı bir `ListeVerisi` endpoint'i). Cari ekle/düzenle CRUD ekranları ve soft-delete tabanlı aktif/pasif işlemi (fiziksel silme yok) tamamlandı. Haftanın sonunda özet commit atıldı.
**Kullanılan teknoloji/yöntem:** DataTables.js server-side processing, soft delete deseni.
**Karşılaşılan sorun:** Grid'de sütunlar DataTables'ın auto-detect mekanizmasına bırakılınca hizalama bozuluyordu — sütunlar `columns` dizisinde açıkça tanımlanarak düzeltildi. Ayrıca sayfa yüklenirken gereksiz bir `tablo.draw()` çağrısı çift render'a yol açıyordu; asıl kök neden bulunup kaldırıldı.

---

## 2. HAFTA — Finans (Banka, Kasa, Cari Fişleri) ve Çek/Senet

### Gün 6 - 2026-07-07
**Yapılan iş:** BankaHesabi (HesapAdi, BankaAdi, IBAN, Bakiye, ParaBirimi) ve KasaHesabi (KasaAdi, Bakiye, ParaBirimi) entity'leri ve CRUD ekranları eklendi. Çok şubeli firma desteği için Şube entity'si (SubeAdi, Adres) oluşturuldu; tüm belge türlerinde şube seçimi altyapısı bu günden itibaren mevcut.
**Kullanılan teknoloji/yöntem:** EF Core migration, client-side DataTables (küçük veri setleri için).
**Karşılaşılan sorun:** Yok.

### Gün 7 - 2026-07-08
**Yapılan iş:** CariFisi entity'si (FisNo, FisTipi enum [Borç/Alacak/Mahsup], Tutar, ÖdemeYöntemi enum [Nakit/Havale/Kredi Kartı], BankaHesabiId/KasaHesabiId nullable) eklendi. Fiş kaydedildiğinde Cari bakiyesi ve seçilen banka/kasa hesabının bakiyesi **tek transaction içinde** güncelleniyor — ya ikisi de başarılı olur ya da hiçbiri. `AddCariFisiFisNoUnique` migration'ı ile FisNo alanına unique index eklendi.
**Kullanılan teknoloji/yöntem:** EF Core transaction (`IDbContextTransaction`), unique constraint.
**Karşılaşılan sorun:** Grid sütun filtreleri (üst satırdaki arama kutuları) yanlış sütuna uygulanıyordu — DataTables `column().search()` çağrısındaki indeks kayması aynı gün düzeltildi.

### Gün 8 - 2026-07-09
**Yapılan iş:** CekSenet entity'si (BelgeTipi enum [Çek/Senet], BelgeNo, VadeTarihi, Tutar, BankaAdi, SubeAdi, Durum enum [Portföyde/Tahsilde/Ciro/Karşılıksız/TahsilEdildi]) modellendi; listeleme, ekleme ve düzenleme ekranları tamamlandı. `DateTimeSutunlariTimeZonesiz` migration'ı ile tüm tarih sütunları `timestamp without time zone`'a çevrildi (Postgres'in UTC zorlamasıyla yaşanan tutarsızlığı önlemek için).
**Kullanılan teknoloji/yöntem:** Enum tabanlı durum modeli, Npgsql tarih/saat tipleri.
**Karşılaşılan sorun:** Yok.

### Gün 9 - 2026-07-10
**Yapılan iş:** Çek/Senet durum akışı için aksiyon butonları eklendi: "Tahsile Ver", "Ciro Et", "Tahsil Edildi", "Karşılıksız". Durum "Tahsil Edildi"ye geçtiğinde otomatik cari/banka hareketi tetikleniyor (diğer durum değişimlerinde tetiklenmiyor — bilinçli tasarım kararı). Vadeye göre satır renklendirmesi eklendi (vadesi geçmiş kırmızı, bugün sarı, gelecek yeşil).
**Kullanılan teknoloji/yöntem:** Durum makinesi (state flow) deseni, koşullu CSS sınıfı atama.
**Karşılaşılan sorun:** Yok.

### Gün 10 - 2026-07-13
**Yapılan iş:** Cari ekstresi raporu eklendi — seçilen cari için tarih aralığında tüm borç/alacak hareketlerini kümülatif bakiye sütunuyla gösteren ekran; Excel'e ve PDF'e aktarım eklendi. Haftalık commit ve özet.
**Kullanılan teknoloji/yöntem:** QuestPDF (PDF), ClosedXML (Excel), LINQ ile kümülatif toplam hesaplama.
**Karşılaşılan sorun:** Yok.

---

## 3. HAFTA — Malzeme Kartları ve Stok Yönetimi

### Gün 11 - 2026-07-14
**Yapılan iş:** Malzeme entity'si (MalzemeKodu, Barkod, MalzemeAdi, Marka, Kalite, Tip, Birim, TeminTuru enum [Alış/Üretim], StokTipi enum [TicariMal/Hammadde/YarıMamul/Mamul], AlisFiyati, SatisFiyati, KdvOrani, MinStokMiktari, MaxStokMiktari, RafNo, Bakiye) ve hiyerarşik (parentId ile) MalzemeKategori eklendi. HarmonyERP'deki gibi çok sütunlu malzeme listesi grid'i yazıldı.
**Kullanılan teknoloji/yöntem:** Kendine referanslı (self-referencing) kategori ağacı.
**Karşılaşılan sorun:** Yok.

### Gün 12 - 2026-07-15
**Yapılan iş:** Malzeme ekle/düzenle/detay ekranları tüm alanlarla (kategori seçimi, barkod alanı dahil) tamamlandı. Excel'e aktarım ve Excel şablonundan toplu malzeme içe aktarım özelliği eklendi.
**Kullanılan teknoloji/yöntem:** ClosedXML ile Excel okuma/yazma, satır bazlı doğrulama.
**Karşılaşılan sorun:** Yok.

### Gün 13 - 2026-07-16
**Yapılan iş:** MalzemeHareketFisi (FisNo, Tarih, HareketTipi enum [Giriş/Çıkış/Transfer/Fire], SubeId, Kalemler) entity'si ve çok kalemli fiş oluşturma ekranı eklendi — dinamik satır ekleme, malzeme arama, birim otomatik doldurma.
**Kullanılan teknoloji/yöntem:** JavaScript ile dinamik form satırları, AJAX malzeme arama.
**Karşılaşılan sorun:** Yok.

### Gün 14 - 2026-07-17
**Yapılan iş:** Fiş onaylandığında tüm kalemlerin bakiyesi transaction içinde güncelleniyor. MinStokMiktari'nın altına düşen malzemeleri listeleyen kritik stok uyarı paneli (ileride Dashboard'a da yansıyacak şekilde tasarlandı) ve barkod ile malzeme arama eklendi.
**Kullanılan teknoloji/yöntem:** EF Core transaction, LINQ filtreleme.
**Karşılaşılan sorun:** Yok.

### Gün 15 - 2026-07-20
**Yapılan iş:** Üç stok raporu ekranı eklendi: (1) Anlık Stok Durumu (tüm malzemeler, bakiye, min/max), (2) Kritik Stok Listesi, (3) Malzeme Bazlı Hareket Geçmişi. Haftalık commit ve özet.
**Kullanılan teknoloji/yöntem:** LINQ agregasyon sorguları, DataTables.
**Karşılaşılan sorun:** Yok.

---

## 4. HAFTA — Satınalma Döngüsü

### Gün 16 - 2026-07-21
**Yapılan iş:** AlisSiparisi/AlisSiparisiKalemi entity'leri eklendi. Tedarikçi seçimi doğrudan Cari modülü üzerinden yapılıyor (CariTipi = Tedarikçi veya Herİkisi filtreli). Alış siparişi oluşturma ekranında kalem ekleme ve KDV/iskonto hesaplaması JS ile anlık yapılıyor.
**Kullanılan teknoloji/yöntem:** JavaScript ile anlık toplam hesaplama, entity filtreleme (CariTipi bazlı).
**Karşılaşılan sorun:** Yok.

### Gün 17 - 2026-07-22
**Yapılan iş:** Alış siparişi listeleme ekranı durum bazlı renk kodlaması ve filtrelemeyle eklendi. Sipariş onaylama/iptal aksiyonları ve kısmi teslim takibi (siparişin kalemlerinin ne kadarının teslim alındığının gösterimi) tamamlandı.
**Kullanılan teknoloji/yöntem:** Durum bazlı koşullu render, kalem bazlı teslim miktarı takibi.
**Karşılaşılan sorun:** Yok.

### Gün 18 - 2026-07-23
**Yapılan iş:** AlisIrsaliyesi/AlisIrsaliyesiKalemi entity'si eklendi — teslim alınan malları kaydediyor, siparişten otomatik oluşturuluyor. İrsaliye onaylandığında transaction içinde stok girişi yapılıyor.
**Kullanılan teknoloji/yöntem:** Belge zinciri (sipariş→irsaliye) dönüştürme deseni, EF Core transaction.
**Karşılaşılan sorun:** Yok.

### Gün 19 - 2026-07-24
**Yapılan iş:** AlisFaturasi/AlisFaturasiKalemi eklendi — irsaliyeden faturaya dönüştürme. Fatura onaylandığında cariye borç hareketi ekleniyor; QuestPDF ile alış faturası PDF çıktısı üretiliyor. `AlisTalepTeklifSiparisIskonto` ve `AddAlisIrsaliyesiNo`/`AddAlisFaturasiNoVeIskonto` migration'ları bu gün eklendi.
**Kullanılan teknoloji/yöntem:** QuestPDF, EF Core migration.
**Karşılaşılan sorun:** Yok.

### Gün 20 - 2026-07-27
**Yapılan iş:** Satınalma özet raporu eklendi (tedarikçi bazlı alış toplamları, Chart.js ile aylık alış grafiği). Satınalma modülü Talep→Sipariş→İrsaliye→Fatura zinciriyle uçtan uca test edildi. Haftalık commit ve özet.
**Kullanılan teknoloji/yöntem:** Chart.js.
**Karşılaşılan sorun:** Yok.

---

## 5. HAFTA — Satış Döngüsü (Tam Akış)

### Gün 21 - 2026-07-28
**Yapılan iş:** MusteriTalebi entity'si (CariId, Tarih, TalepNo, İçerik, Durum) ve ekranı eklendi. SatisTeklifi/SatisTeklifiKalemi — müşteri talebinden teklif oluşturma, geçerlilik süresi, birim fiyat/iskonto/KDV alanları ve teklif PDF çıktısı tamamlandı. `AddSatisTeklifiNo` migration'ı eklendi.
**Kullanılan teknoloji/yöntem:** QuestPDF, EF Core migration.
**Karşılaşılan sorun:** Yok.

### Gün 22 - 2026-07-29
**Yapılan iş:** SatisSiparisi/SatisSiparisiKalemi eklendi — tekliften siparişe dönüştürme, onay/iptal ve kısmi sevkiyat desteği (siparişin tüm kalemlerinin aynı anda sevk edilmesi zorunlu değil). `AddSatisSiparisiNo` migration'ı eklendi.
**Kullanılan teknoloji/yöntem:** JS ile anlık toplam hesaplama, kısmi sevkiyat durum takibi.
**Karşılaşılan sorun:** Yok.

### Gün 23 - 2026-07-30
**Yapılan iş:** SevkIrsaliyesi/SevkIrsaliyesiKalemi eklendi — siparişten sevk irsaliyesi oluşturma, sevk adresi ve araç/şoför bilgisi alanları. İrsaliye onaylandığında negatif stok kontrollü stok düşümü ve PDF çıktısı eklendi. `AddSevkIrsaliyesiNo` migration'ı eklendi.
**Kullanılan teknoloji/yöntem:** Transaction içinde stok yeterlilik kontrolü, QuestPDF.
**Karşılaşılan sorun:** Yok.

### Gün 24 - 2026-07-31
**Yapılan iş:** SatisFaturasi/SatisFaturasiKalemi eklendi — sevk irsaliyesinden/siparişten faturaya dönüştürme. Fatura onaylandığında (henüz düşülmediyse) stok düşümü **ve** cariye alacak hareketi **tek transaction içinde** işleniyor. HarmonyERP formatına yakın PDF çıktısı (logo, belge no, tarih, vade, kalem tablosu, iskonto, KDV, toplam) eklendi. `AddSatisFaturasiNoVeVade` migration'ı bu gün eklendi.
**Kullanılan teknoloji/yöntem:** QuestPDF, çok tablolu transaction (stok + cari hareketi aynı anda).
**Karşılaşılan sorun:** Yok.

### Gün 25 - 2026-08-03
**Yapılan iş:** Satış raporları eklendi: müşteri bazlı satış toplamları, malzeme bazlı satış toplamları, Chart.js ile aylık satış trendi. Talep→Teklif→Sipariş→Sevk→Fatura→Stok/Cari güncellemesi zinciri uçtan uca test edildi. Haftalık commit ve özet.
**Kullanılan teknoloji/yöntem:** Chart.js, LINQ agregasyon.
**Karşılaşılan sorun:** Yok.

---

## 6. HAFTA — Muhasebe, Dashboard, Login ve Canlıya Alma

### Gün 26 - 2026-08-04
**Yapılan iş:** HesapPlani entity'si (HesapKodu, HesapAdi, HesapTipi enum [Aktif/Pasif/Gelir/Gider/Özkaynak], hiyerarşik ParentId) ve 18 hesaplık temel tohum verisi eklendi. MuhasebeFisi/MuhasebeFisiKalemi (Hesap, Borç/Alacak tutarı) yazıldı; satış/alış faturası onaylandığında **otomatik yevmiye kaydı** oluşturma entegrasyonu tamamlandı — projedeki en kritik çapraz modül entegrasyonlarından biri.
**Kullanılan teknoloji/yöntem:** Hiyerarşik (parentId) hesap planı, otomatik muhasebeleştirme servisi.
**Karşılaşılan sorun:** Yok.

### Gün 27 - 2026-08-05
**Yapılan iş:** ASP.NET Core Identity entegre edildi: login/logout, "beni hatırla", MailKit ile e-posta tabanlı şifremi unuttum akışı. `AppUser`/`AppRole` ile Admin/Muhasebe/Satış rolleri tanımlandı; menü öğeleri rol bazlı görünür/gizli yapıldı (`[Authorize(Roles=...)]`).
**Kullanılan teknoloji/yöntem:** ASP.NET Core Identity, MailKit (SMTP), rol bazlı yetkilendirme.
**Karşılaşılan sorun:** Yok.

### Gün 28 - 2026-08-06
**Yapılan iş:** Dashboard ekranı eklendi: Toplam Satış/Satınalma kartları (TRY + $), Chart.js pasta grafiği (kategori bazlı satış), yıl karşılaştırmalı (2025 vs 2026) aylık satış trend çizgisi, en çok satış yapılan müşteriler ve en çok satılan malzemeler bar grafikleri, kritik stok uyarı listesi.
**Kullanılan teknoloji/yöntem:** Chart.js (pasta, çizgi, bar), çoklu para birimi gösterimi.
**Karşılaşılan sorun:** Yok.

### Gün 29 - 2026-08-07
**Yapılan iş:** Oracle Cloud Free Tier'a deploy için altyapı hazırlandı: nginx reverse proxy yapılandırması, production Docker Compose dosyası (web+db+nginx, ortam değişkenleriyle parametrik), tek seferlik demo veri seed'i (`dotnet SakaryaERP.dll --seed-demo` — 14 cari, 25 malzeme, uçtan uca örnek satış/alış zincirleri ve 14 aylık hacim verisi üreten `DemoSeeder`). `docs/deploy-oracle-cloud.md` adım adım deploy rehberi yazıldı.
**Kullanılan teknoloji/yöntem:** nginx, Docker Compose (ortam değişkeni interpolasyonu), EF Core seed script'i.
**Karşılaşılan sorun:** Yok — bu gün sadece altyapı/doküman hazırlığıydı; gerçek sunucuya çıkış Gün 30'da yapıldı.

### Gün 30 - 2026-08-10
**Yapılan iş:** Production'a geçiş öncesi kapsamlı bir son tur çalışıldı — bu, tek bir "gün"e sığdırılmış ama gerçekte birkaç günlük yoğun bir hazırlık/test sürecidir:

- **Tasarım revizyonu:** Paylaşılan HarmonyERP ekran görüntüleriyle karşılaştırmalı bir "kayıt kartı tasarım planı" hazırlandı ve uygulandı — Malzeme ve Cari kartları sekmeli (Detay/Ekle/Düzenle) tasarıma çevrildi, satış/satınalma belgelerinde üst belgeye tıklanabilir "belge zinciri" linki eklendi, Çek/Senet ve tüm Detay sayfaları eklendi, Dashboard HarmonyERP'ye yakın bir görünüme kavuşturuldu.
- **Gerçek ERP boşluk analizi:** `docs/gercek-erp-boslugu-analizi.md` yazılarak projenin gerçek bir ERP'ye göre nerede durduğu (e-Fatura, çoklu şirket, API katmanı gibi bilinçli olarak dışarıda bırakılanlar dahil) belgelendi. Bulgulara göre iki kademede iyileştirme yapıldı: **Kademe 1** — xUnit test altyapısı (118 test), GitHub Actions CI/CD, `SaveChangesAsync` override ile otomatik `AuditLog`, `VeritabaniHealthCheck`; **Kademe 2** — tutar bazlı çok kademeli onay ve proaktif kritik durum bildirimi (`BildirimService`, `GunlukBildirimHostedService`).
- **Demo veri zenginleştirmesi:** `DemoSeeder`'a +6 cari, 14 aylık rastgele (sabit seed'li, tekrarlanabilir) satış/alış hacmi, ek çek/senet ve hareket fişi örnekleri eklendi.
- **Kapsamlı güvenlik ve kalite denetimi:** Kod tabanı üzerinden birkaç tur agent/mimari incelemesi yapılarak toplam 35 bulgu giderildi — **9 kritik** (IDOR — şube filtresiz veri sızıntısı, stored XSS, şifre sıfırlama linkinde Host header injection, görevler ayrılığı bypass'ı, production'da sabit admin şifresi, dışa açık Postgres portu, Çek/Senet'te eksik şube kısıtı), **13 yüksek** (eş zamanlı güncellemede lost update riski, belge no üretiminde race condition, gerçek kardeks — Malzeme Geçmişi'nin tüm stok kaynaklarını birleştirmesi, Mizan ekranı, CSP/X-Frame-Options güvenlik header'ları, SMTP STARTTLS zorunluluğu), **8 orta** ve **5 düşük** öncelikli bulgu. Ayrıca KDV özet raporu, Kullanıcı Yönetimi ve Belge Takip ekranları eklendi; test projesindeki bilinen bağımlılık açıkları giderildi. `AddCariFisiKaynakBelgeId`, `AddXminConcurrencyToken` (optimistic concurrency için), `AddUniqueIndexesToBelgeNo`, `AddSubeIdToDocumentsAndCari`/`AddSubeIdToCekSenet` migration'ları bu turda eklendi.
- **Marka değişikliği:** Proje adı SakaryaERP'den TicariSistem'e değiştirildi (görünen metinler, e-posta/PDF marka yazıları, config değerleri ve dokümantasyon; C# namespace'leri ve `.csproj`/`.sln`/klasör adları bilinçli olarak değiştirilmedi).
- **Canlıya alma:** Oracle Cloud Console'da Always Free bir Compute instance (Ubuntu 24.04) oluşturuldu; VCN/Subnet, Security List (80/443 ingress kuralları) ve SSH erişimi yapılandırıldı. `docker compose up -d --build` ile `db`+`web`+`nginx` container'ları ayağa kaldırıldı; production image'ında EF Core migration'larının hiç uygulanmadığı (final image'da sadece runtime var, `dotnet ef` yok) kritik bir eksik bulunup `Program.cs`'e `Database.MigrateAsync()` eklenerek kalıcı olarak çözüldü. Demo veri yüklendi, Admin/Muhasebe/Satış kullanıcıları Kullanıcı Yönetimi ekranından oluşturuldu. Site `http://79.76.125.129` adresinden canlıya alındı ve doğrulandı.

**Kullanılan teknoloji/yöntem:** xUnit + EF Core InMemory, GitHub Actions, EF Core `Database.MigrateAsync()` ve optimistic concurrency (`xmin`), Docker Compose, Oracle Cloud (Compute/VCN/Security List), iptables + `netfilter-persistent`, swap dosyası.
**Karşılaşılan sorun:** (1) Production image'ında migration uygulayacak bir adım yoktu — `web` container'ı `AspNetRoles` tablosu bulunamadığı için sürekli çöküyordu (exit 139), kalıcı çözüm `Program.cs`'e eklendi. (2) Seçilen küçük VM shape'i (`VM.Standard.E2.1.Micro`, 1GB RAM) sunucu içinde `dotnet build` çalıştırırken bellek yetersizliğinden (OOM) tamamen kilitlendi — SSH bile yanıt vermez oldu; Console'dan reboot edilip 4GB'lık kalıcı bir swap dosyası eklenerek çözüldü. (3) Oracle'ın otomatik oluşturduğu VCN başta public subnet/IP içermiyordu, yeniden yapılandırıldı. (4) Security List'te 80/443 portları başta kapalıydı, ingress kuralları eklenerek açıldı. Tüm bu sorunlar `docs/deploy-oracle-cloud.md`'ye not düşülerek tekrarlanabilir hale getirildi.

---

## Kapanış Notu

Kapsam dışı bırakılan modüller (Üretim/MRP, Bordro, Kalite Kontrol, E-Ticaret,
Dış Ticaret, e-Fatura/e-Defter/e-İrsaliye entegrasyonu, çoklu şirket desteği)
ve gerekçeleri için bkz. [gercek-erp-boslugu-analizi.md](gercek-erp-boslugu-analizi.md).

**Öğrenilenler:** Katmanlı mimari (Controller → Service → DbContext) ve
transaction disiplini (özellikle SatisFaturasi/AlisFaturasi'nin stok+cari+
muhasebe fişini tek işlemde güncellemesi) proje büyüdükçe de sürdürülebilir
kaldı. En büyük teknik zorluk canlıya alma aşamasında çıktı: local'de sorunsuz
çalışan bir uygulamanın production'da (kaynak kısıtlı VM, migration'ların
otomatik uygulanmaması, ağ/firewall katmanları) tamamen farklı sorunlarla
karşılaşabildiği görüldü — bu, "çalışıyor" ile "deploy edilebilir" arasındaki
farkın en somut örneğiydi.

---

## Ek: Örnek Kod — Satış Faturası Onay Akışı

Projenin kalbi sayılabilecek entegrasyonu (stok düşümü + otomatik cari hareketi
+ otomatik yevmiye kaydı, tek transaction içinde) gösteren örnek:
`SakaryaERP/Services/SatisFaturasiService.cs`, satır 237-349.

```csharp
public async Task OnaylaAsync(int id)
{
    var fatura = await _unitOfWork.Repository<SatisFaturasi>().QueryTumu()
        .Include(f => f.Kalemler)
        .FirstOrDefaultAsync(f => f.Id == id)
        ?? throw new InvalidOperationException("Satış faturası bulunamadı.");
    _onayYetkisiService.SubeErisimKontrolEt(fatura.SubeId, "Bu fatura başka bir şubeye ait, onaylayamazsınız.");

    if (fatura.Durum != BelgeDurum.Beklemede)
        throw new InvalidOperationException("Sadece beklemede olan faturalar onaylanabilir.");

    _onayYetkisiService.YuksekTutarKontrolEt(fatura.Kalemler.Sum(FinansHesaplama.SatisFaturasiSatirToplami), "Satış Faturası");
    _onayYetkisiService.OlusturanOnaylayamazKontrolEt(fatura.CreatedBy, "Satış Faturası");

    var cari = await _unitOfWork.Repository<Cari>().GetByIdAsync(fatura.CariId)
        ?? throw new InvalidOperationException("Cari bulunamadı.");
    _onayYetkisiService.KrediLimitiKontrolEt(
        cari.Bakiye, cari.KrediLimiti, fatura.Kalemler.Sum(FinansHesaplama.SatisFaturasiSatirToplami), cari.Unvan);

    var malzemeIdleri = fatura.Kalemler.Select(k => k.MalzemeId).Distinct().ToList();
    var malzemeler = await _unitOfWork.Repository<Malzeme>().QueryTumu()
        .Where(m => malzemeIdleri.Contains(m.Id))
        .ToDictionaryAsync(m => m.Id);

    // İrsaliyeden gelen faturalarda stok zaten irsaliye onayında düşülmüştür;
    // irsaliyesiz (doğrudan siparişten veya manuel) faturalarda burada düşülür.
    if (fatura.SevkIrsaliyesiId is null)
    {
        foreach (var kalem in fatura.Kalemler)
        {
            var malzeme = malzemeler[kalem.MalzemeId];
            var yeniBakiye = malzeme.Bakiye - kalem.Miktar;
            if (yeniBakiye < 0)
                throw new InvalidOperationException(
                    $"{malzeme.MalzemeKodu} için stok yetersiz (mevcut: {malzeme.Bakiye}, istenen: {kalem.Miktar}).");

            malzeme.Bakiye = yeniBakiye;
        }
    }

    // Brüt kar/COGS hesabı satılan andaki maliyeti kullansın diye Malzeme.AlisFiyati
    // onay anında kaleme kopyalanır (sonradan AlisFiyati değişse bile geçmiş fatura etkilenmez).
    foreach (var kalem in fatura.Kalemler)
        kalem.BirimMaliyet = malzemeler[kalem.MalzemeId].AlisFiyati;

    var toplamTutar = fatura.Kalemler.Sum(FinansHesaplama.SatisFaturasiSatirToplami);
    var netTutar = fatura.Kalemler.Sum(k => k.Miktar * k.BirimFiyat * (1 - k.Iskonto / 100m));
    var kdvTutari = toplamTutar - netTutar;
    var toplamMaliyet = fatura.Kalemler.Sum(k => k.Miktar * k.BirimMaliyet);

    var toplamFisSayisi = await _unitOfWork.Repository<CariFisi>().QueryTumu().CountAsync();
    var cariFisi = new CariFisi
    {
        FisNo = $"CF-{toplamFisSayisi + 1:000000}",
        CariId = fatura.CariId,
        Tarih = fatura.Tarih,
        FisTipi = FisTipi.Borc,
        Tutar = toplamTutar,
        OdemeYontemi = OdemeYontemi.Havale,
        Aciklama = $"Satış Faturası {fatura.FaturaNo}",
        OtomatikOlusturuldu = true,
        SatisFaturasiId = fatura.Id,
        // CariFisiService.CreateAsync üzerinden geçmediği için SubeId burada elle
        // faturanın kendi şubesinden alınıyor — aksi halde şube filtresi uygulanan
        // liste/rapor ekranlarında bu otomatik fiş hiçbir şubede görünmezdi.
        SubeId = fatura.SubeId
    };

    // Borç fişi: müşteri bize borçlanır, Cari.Bakiye artar (CariFisiService'teki yön kuralıyla aynı).
    cari.Bakiye += toplamTutar;

    await _unitOfWork.Repository<CariFisi>().AddAsync(cariFisi);
    await _unitOfWork.Repository<MuhasebeFisi>().AddAsync(
        await YevmiyeKaydiOlusturAsync(fatura, netTutar, kdvTutari, toplamTutar, toplamMaliyet));

    fatura.Durum = BelgeDurum.Onaylandi;
    await _unitOfWork.SaveChangesAsync();
}

// Satış faturası onayında basit yevmiye kaydı: 120 Alıcılar borçlanır,
// 600 Yurtiçi Satışlar ve 391 Hesaplanan KDV alacaklanır; ayrıca satılan malın maliyeti
// 621 Satılan Ticari Mallar Maliyeti'ne borç, 153 Ticari Mallar'a alacak yazılır (COGS).
private async Task<MuhasebeFisi> YevmiyeKaydiOlusturAsync(SatisFaturasi fatura, decimal netTutar, decimal kdvTutari, decimal toplamTutar, decimal toplamMaliyet)
{
    var hesaplar = await _unitOfWork.Repository<HesapPlani>().QueryTumu()
        .Where(h => h.HesapKodu == "120" || h.HesapKodu == "600" || h.HesapKodu == "391" || h.HesapKodu == "621" || h.HesapKodu == "153")
        .ToDictionaryAsync(h => h.HesapKodu);

    var aciklama = $"Satış Faturası {fatura.FaturaNo}";
    var kalemler = new List<MuhasebeFisiKalemi>
    {
        new() { HesapPlaniId = hesaplar["120"].Id, Borc = toplamTutar, Alacak = 0, Aciklama = aciklama },
        new() { HesapPlaniId = hesaplar["600"].Id, Borc = 0, Alacak = netTutar, Aciklama = aciklama }
    };
    if (kdvTutari > 0)
        kalemler.Add(new MuhasebeFisiKalemi { HesapPlaniId = hesaplar["391"].Id, Borc = 0, Alacak = kdvTutari, Aciklama = aciklama });
    if (toplamMaliyet > 0)
    {
        kalemler.Add(new MuhasebeFisiKalemi { HesapPlaniId = hesaplar["621"].Id, Borc = toplamMaliyet, Alacak = 0, Aciklama = aciklama });
        kalemler.Add(new MuhasebeFisiKalemi { HesapPlaniId = hesaplar["153"].Id, Borc = 0, Alacak = toplamMaliyet, Aciklama = aciklama });
    }

    FinansHesaplama.BorcAlacakDengesiniDogrula(kalemler);

    var toplamFisSayisi = await _unitOfWork.Repository<MuhasebeFisi>().QueryTumu().CountAsync();
    return new MuhasebeFisi
    {
        FisNo = $"MF-{toplamFisSayisi + 1:000000}",
        Tarih = fatura.Tarih,
        SatisFaturasiId = fatura.Id,
        Kalemler = kalemler
    };
}
```

**Neden bu metod:** Tek bir `OnaylaAsync` çağrısında görevler ayrılığı
kontrolü, kredi limiti kontrolü, koşullu stok düşümü, maliyet (COGS)
kopyalama, otomatik Cari Fişi oluşturma ve otomatik Muhasebe Fişi (yevmiye
kaydı) oluşturma bir arada yürütülüyor; hepsi tek `SaveChangesAsync()` ile
tek transaction'da commit ediliyor — ya hepsi başarılı olur ya hiçbiri
("ya hep ya hiç" kuralı). Tam dosya için:
[SatisFaturasiService.cs](https://github.com/Furkan1785/Staj_Proje/blob/main/SakaryaERP/Services/SatisFaturasiService.cs).
