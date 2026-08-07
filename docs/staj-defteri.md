# Staj Defteri — TicariSistem (30 İş Günü)

Format CLAUDE.md'de tanımlandığı gibi: her gün için Yapılan iş / Kullanılan
teknoloji-yöntem / Karşılaşılan sorun. Görevler GOREVLER.md'deki 30 günlük
plana birebir karşılık gelir.

---

## 1. HAFTA — Altyapı ve Finans

### Gün 1 - 2026-06-30
**Yapılan iş:** .NET SDK/PostgreSQL/Docker kurulumu doğrulandı, `dotnet new mvc` ile proje oluşturuldu, git init ve ilk commit yapıldı, Docker Compose dosyası (web+postgres) yazıldı.
**Kullanılan teknoloji/yöntem:** ASP.NET Core 8 MVC, Docker Compose.
**Karşılaşılan sorun:** Yok.

### Gün 2 - 2026-07-01
**Yapılan iş:** Tüm entity'lerin kapsamlı ER diyagramı (mermaid) `docs/er-diyagram.md` olarak oluşturuldu; kapsam genişletilerek Şube, BaseEntity, AlışIrsaliyesi ve MalzemeKategori de eklendi.
**Kullanılan teknoloji/yöntem:** Mermaid ER diyagramı.
**Karşılaşılan sorun:** İlk taslakta bazı ilişkiler eksik kalmıştı, ikinci geçişte tamamlandı.

### Gün 3 - 2026-07-02
**Yapılan iş:** Mimari katmanlar kuruldu — `BaseEntity` (Id, CreatedAt, UpdatedAt, CreatedBy, IsDeleted), generic `IRepository<T>`/`BaseRepository<T>`, `IUnitOfWork` deseni, `AppDbContext` ve ilk migration.
**Kullanılan teknoloji/yöntem:** EF Core Code First, Repository/UnitOfWork deseni.
**Karşılaşılan sorun:** Yok.

### Gün 4 - 2026-07-03
**Yapılan iş:** Global exception-handling ve request/response logging middleware'leri eklendi; Cari entity'si (CariKodu, Unvan, CariTipi, VergiNo, Bakiye, KrediLimiti vb.) + CariService/CariRepository yazıldı.
**Kullanılan teknoloji/yöntem:** ASP.NET Core middleware pipeline, Serilog.
**Karşılaşılan sorun:** Yok.

### Gün 5 - 2026-07-06
**Yapılan iş:** Cari listesi için server-side DataTables grid (sayfalama/sıralama/filtreleme), Cari ekle/düzenle CRUD ve soft-delete aktif/pasif işlemi eklendi. Haftalık commit.
**Kullanılan teknoloji/yöntem:** DataTables server-side processing, soft delete.
**Karşılaşılan sorun:** Grid'de sütunlar auto-detect yerine açıkça tanımlanmayınca hizalama bozuluyordu; ayrıca gereksiz bir `tablo.draw()` çağrısı çift render'a yol açıyordu — ikisi de aynı gün düzeltildi.

---

## 2. HAFTA — Finans (Banka, Kasa, Cari Fişleri) ve Çek/Senet

### Gün 6 - 2026-07-07
**Yapılan iş:** BankaHesabi ve KasaHesabi entity'leri + CRUD ekranları, çok şubeli firma desteği için Şube entity'si eklendi.
**Kullanılan teknoloji/yöntem:** EF Core migration, client-side DataTables.
**Karşılaşılan sorun:** Yok.

### Gün 7 - 2026-07-08
**Yapılan iş:** CariFisi entity'si (FisNo, FisTipi, Tutar, ÖdemeYöntemi) eklendi; fiş kaydedildiğinde Cari bakiyesi ve seçilen banka/kasa bakiyesi tek transaction içinde güncelleniyor.
**Kullanılan teknoloji/yöntem:** EF Core transaction.
**Karşılaşılan sorun:** Grid sütun filtreleri yanlış sütuna uygulanıyordu, aynı gün düzeltildi.

### Gün 8 - 2026-07-09
**Yapılan iş:** CekSenet entity'si (BelgeTipi, VadeTarihi, Tutar, Durum enum) ve listeleme/ekleme/düzenleme ekranları eklendi.
**Kullanılan teknoloji/yöntem:** Enum tabanlı durum modeli.
**Karşılaşılan sorun:** Yok.

### Gün 9 - 2026-07-10
**Yapılan iş:** Çek/Senet durum akışı — "Tahsile Ver", "Ciro Et", "Tahsil Edildi", "Karşılıksız" aksiyonları ve durum değişiminde otomatik cari/banka hareketi; vadeye göre renk kodlaması eklendi.
**Kullanılan teknoloji/yöntem:** Durum makinesi (state flow), koşullu UI renklendirme.
**Karşılaşılan sorun:** Yok.

### Gün 10 - 2026-07-13
**Yapılan iş:** Cari ekstresi raporu — tarih aralığında kümülatif bakiye gösterimi, Excel/PDF çıktısı. Haftalık commit.
**Kullanılan teknoloji/yöntem:** QuestPDF, ClosedXML.
**Karşılaşılan sorun:** Yok.

---

## 3. HAFTA — Malzeme Kartları ve Stok Yönetimi

### Gün 11 - 2026-07-14
**Yapılan iş:** Malzeme entity'si (MalzemeKodu, Barkod, Birim, TeminTürü, StokTipi, MinMax stok vb.) ve hiyerarşik MalzemeKategori eklendi; çok sütunlu malzeme listesi grid'i yazıldı.
**Kullanılan teknoloji/yöntem:** Kendine referanslı (parentId) kategori ağacı.
**Karşılaşılan sorun:** Yok.

### Gün 12 - 2026-07-15
**Yapılan iş:** Malzeme ekle/düzenle/detay ekranları tüm alanlarla tamamlandı; Excel'e aktarım ve şablondan toplu içe aktarım eklendi.
**Kullanılan teknoloji/yöntem:** ClosedXML ile Excel okuma/yazma.
**Karşılaşılan sorun:** Yok.

### Gün 13 - 2026-07-16
**Yapılan iş:** MalzemeHareketFisi (Giriş/Çıkış/Transfer/Fire) entity'si ve çok kalemli, dinamik satır eklemeli fiş oluşturma ekranı eklendi.
**Kullanılan teknoloji/yöntem:** JS ile dinamik form satırları.
**Karşılaşılan sorun:** Yok.

### Gün 14 - 2026-07-17
**Yapılan iş:** Fiş onaylandığında kalemlerin bakiyesi transaction içinde güncellendi; MinStokMiktari altına düşen malzemeler için kritik stok uyarı paneli ve barkod ile arama eklendi.
**Kullanılan teknoloji/yöntem:** EF Core transaction, LINQ filtreleme.
**Karşılaşılan sorun:** Yok.

### Gün 15 - 2026-07-20
**Yapılan iş:** Stok raporu ekranları — Anlık Stok Durumu, Kritik Stok Listesi, Malzeme Bazlı Hareket Geçmişi eklendi. Haftalık commit.
**Kullanılan teknoloji/yöntem:** LINQ agregasyon sorguları.
**Karşılaşılan sorun:** Yok.

---

## 4. HAFTA — Satınalma Döngüsü

### Gün 16 - 2026-07-21
**Yapılan iş:** AlisSiparisi/AlisSiparisiKalemi entity'leri; tedarikçi seçimi (Cari modülü üzerinden), kalem ekleme, KDV/iskonto hesaplamalı alış siparişi oluşturma ekranı eklendi.
**Kullanılan teknoloji/yöntem:** JS ile anlık toplam hesaplama.
**Karşılaşılan sorun:** Yok.

### Gün 17 - 2026-07-22
**Yapılan iş:** Alış siparişi listeleme ekranı (durum bazlı renk/filtre), onaylama/iptal aksiyonları ve kısmi teslim takibi eklendi.
**Kullanılan teknoloji/yöntem:** Durum bazlı koşullu render.
**Karşılaşılan sorun:** Yok.

### Gün 18 - 2026-07-23
**Yapılan iş:** AlisIrsaliyesi entity'si — siparişten otomatik oluşturma, onaylandığında transaction içinde stok girişi.
**Kullanılan teknoloji/yöntem:** Belge zinciri (sipariş→irsaliye) dönüştürme deseni.
**Karşılaşılan sorun:** Yok.

### Gün 19 - 2026-07-24
**Yapılan iş:** AlisFaturasi — irsaliyeden faturaya dönüştürme, onayda cariye borç hareketi, PDF çıktısı (QuestPDF) eklendi.
**Kullanılan teknoloji/yöntem:** QuestPDF.
**Karşılaşılan sorun:** Yok.

### Gün 20 - 2026-07-27
**Yapılan iş:** Satınalma özet raporu (tedarikçi bazlı toplamlar, aylık alış grafiği) eklendi; satınalma modülü uçtan uca test edildi. Haftalık commit.
**Kullanılan teknoloji/yöntem:** Chart.js.
**Karşılaşılan sorun:** Yok.

---

## 5. HAFTA — Satış Döngüsü (Tam Akış)

### Gün 21 - 2026-07-28
**Yapılan iş:** MusteriTalebi entity'si ve ekranı; SatisTeklifi/SatisTeklifiKalemi — müşteri talebinden teklif oluşturma, geçerlilik süresi, teklif PDF çıktısı eklendi.
**Kullanılan teknoloji/yöntem:** QuestPDF.
**Karşılaşılan sorun:** Yok.

### Gün 22 - 2026-07-29
**Yapılan iş:** SatisSiparisi — tekliften siparişe dönüştürme, onay/iptal ve kısmi sevkiyat desteği eklendi.
**Kullanılan teknoloji/yöntem:** JS ile anlık toplam hesaplama.
**Karşılaşılan sorun:** Yok.

### Gün 23 - 2026-07-30
**Yapılan iş:** SevkIrsaliyesi — siparişten oluşturma, sevk adresi/araç-şoför bilgisi, onayda negatif stok kontrollü stok düşümü ve PDF çıktısı eklendi.
**Kullanılan teknoloji/yöntem:** Transaction içinde stok kontrolü.
**Karşılaşılan sorun:** Yok.

### Gün 24 - 2026-07-31
**Yapılan iş:** SatisFaturasi — sevk irsaliyesinden/siparişten faturaya dönüştürme; onayda (henüz düşülmediyse) stok düşümü + cariye alacak hareketi tek transaction içinde; HarmonyERP formatına yakın PDF çıktısı eklendi.
**Kullanılan teknoloji/yöntem:** QuestPDF, transaction.
**Karşılaşılan sorun:** Yok.

### Gün 25 - 2026-08-03
**Yapılan iş:** Satış raporları (müşteri bazlı, malzeme bazlı, aylık trend) eklendi; Talep→Teklif→Sipariş→Sevk→Fatura→Stok/Cari zinciri uçtan uca test edildi. Haftalık commit.
**Kullanılan teknoloji/yöntem:** Chart.js.
**Karşılaşılan sorun:** Yok.

---

## 6. HAFTA — Muhasebe, Dashboard, Login ve Canlıya Alma

### Gün 26 - 2026-08-04
**Yapılan iş:** HesapPlani (hiyerarşik) entity'si ve 18 hesaplık tohum verisi; MuhasebeFisi/MuhasebeFisiKalemi eklendi. Satış/alış faturası onaylandığında otomatik yevmiye kaydı entegrasyonu tamamlandı.
**Kullanılan teknoloji/yöntem:** Hiyerarşik (parentId) hesap planı, otomatik muhasebeleştirme.
**Karşılaşılan sorun:** Yok.

### Gün 27 - 2026-08-05
**Yapılan iş:** ASP.NET Core Identity ile login/logout, "beni hatırla", MailKit ile şifremi unuttum akışı; Admin/Muhasebe/Satış rolleri ve rol bazlı menü görünürlüğü eklendi.
**Kullanılan teknoloji/yöntem:** ASP.NET Core Identity, MailKit.
**Karşılaşılan sorun:** Yok.

### Gün 28 - 2026-08-06
**Yapılan iş:** Dashboard eklendi — Toplam Satış/Satınalma kartları, kategori bazlı satış pastası, yıl karşılaştırmalı aylık trend, en çok satış yapılan müşteri/malzeme grafikleri, kritik stok uyarı listesi.
**Kullanılan teknoloji/yöntem:** Chart.js.
**Karşılaşılan sorun:** Yok.

### Gün 29 - 2026-08-07
**Yapılan iş:** Oracle Cloud Free Tier'a deploy altyapısı kuruldu — nginx reverse proxy, production Docker Compose yapılandırması, tek seferlik demo veri seed'i (`--seed-demo`) ve `docs/deploy-oracle-cloud.md` adım adım deploy rehberi yazıldı.
**Kullanılan teknoloji/yöntem:** nginx, Docker Compose, EF Core seed.
**Karşılaşılan sorun:** Yok — bu gün altyapı hazırlığıydı, gerçek canlıya alma Gün 30'da yapıldı.

### Gün 30 - 2026-08-10
**Yapılan iş:** Production'a geçiş öncesi kapsamlı son tur çalışıldı:
- **Tasarım:** Kayıt kartı tasarım planı uygulandı (Malzeme/Cari sekmeli tasarım, belge zinciri linkleri, tüm Detay sayfaları, HarmonyERP'ye yakın Dashboard görünümü).
- **Boşluk analizi:** `docs/gercek-erp-boslugu-analizi.md` yazıldı; test altyapısı (xUnit, 118 test), CI/CD, AuditLog, health check eklendi (Kademe 1); çok kademeli onay + proaktif bildirim eklendi (Kademe 2).
- **Demo veri:** +6 cari, 14 aylık hacim verisi, ek çek/senet ve hareket fişleriyle zenginleştirildi.
- **Kapsamlı güvenlik/kalite denetimi:** 9 kritik (IDOR, stored XSS, host header injection, görevler ayrılığı bypass, hardcoded prod admin şifresi, dışa açık Postgres portu, şube kısıtı eksikleri), 13 yüksek (race condition, lost update, kredi limiti, gerçek kardeks, Mizan ekranı, güvenlik header'ları, STARTTLS), 8 orta ve 5 düşük öncelikli bulgu tespit edilip düzeltildi; KDV özet raporu, Kullanıcı Yönetimi ve Belge Takip ekranları eklendi; bağımlılıklardaki bilinen açıklar giderildi.
- **Marka adı** SakaryaERP'den TicariSistem'e değiştirildi (görünen isim + config/doküman).
- **Canlıya alma:** Oracle Cloud'da instance oluşturuldu, VCN/Security List/SSH yapılandırıldı, Docker Compose ile `db`+`web`+`nginx` ayağa kaldırıldı; production'da EF Core migration'larının hiç uygulanmadığı bug'ı bulunup `Database.MigrateAsync()` ile kalıcı olarak düzeltildi; demo veri yüklendi, Admin/Muhasebe/Satış kullanıcıları oluşturuldu. Site `http://79.76.125.129` üzerinden canlıya alındı.
**Kullanılan teknoloji/yöntem:** xUnit, GitHub Actions CI, EF Core `Database.MigrateAsync()`, Docker Compose, Oracle Cloud (Compute/VCN/Security List), iptables, swap file.
**Karşılaşılan sorun:** Production image'ında migration'ları uygulayacak bir adım yoktu (final image'da sadece runtime var, SDK/`dotnet ef` değil) — `web` container'ı `AspNetRoles` tablosu olmadığı için sürekli çöküyordu, `Program.cs`'e otomatik migration eklenerek çözüldü. Ayrıca seçilen küçük VM shape'i (1GB RAM) sunucu içi `dotnet build` sırasında bellek yetersizliğinden kilitlendi, 4GB swap eklenerek çözüldü. Oracle Security List'te 80/443 portları başta kapalıydı, ingress kuralları eklenerek açıldı.

---

## Kapanış Notu

Kapsam dışı bırakılan modüller (Üretim/MRP, Bordro, Kalite Kontrol, E-Ticaret,
Dış Ticaret, e-Fatura/e-Defter entegrasyonu) ve gerekçeleri için bkz.
[gercek-erp-boslugu-analizi.md](gercek-erp-boslugu-analizi.md). Öğrenilenler:
katmanlı mimari (Controller→Service→DbContext) ve transaction disiplini büyük
ölçekte de sürdürülebilir kaldı; en büyük teknik zorluk canlıya alma
aşamasında (migration, kaynak kısıtlı VM, ağ/firewall katmanları) çıktı —
bunlar dokümana (`deploy-oracle-cloud.md`) işlenerek tekrarlanabilir hale
getirildi.
