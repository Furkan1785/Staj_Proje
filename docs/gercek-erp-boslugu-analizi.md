# TicariSistem — "Gerçek Bir ERP" Olma Yolunda Kapsamlı Boşluk Analizi

## 1. Amaç ve Yöntem

Bu doküman iki soruya cevap arıyor: **(1) Ticari/kurumsal bir ERP'yi ERP yapan
şey tam olarak nedir?** ve **(2) TicariSistem bugün bu tanımın neresinde duruyor?**

Yöntem: Güncel (2026) ERP mimarisi, modül kapsamı ve Türkiye'ye özgü mevzuat
gereksinimleri üzerine araştırma yapıldı (kaynaklar Bölüm 7'de), ardından
TicariSistem'nin kod tabanı (Controllers/, Services/, Models/, Program.cs,
appsettings.json, docker-compose.yml, csproj) satır satır tarandı. Bu ikisi
karşılaştırılarak Bölüm 4'teki boşluk analizi çıkarıldı.

**Bu doküman bir "başarısızlık raporu" değil.** TicariSistem bilinçli olarak
kapsamı daraltılmış bir staj projesi (bkz. CLAUDE.md, GÖREVLER.md); buradaki
boşlukların çoğu zaten bilerek alınmış kararlar. Amaç, bu kararları **gerçek
ERP standartlarına karşı çerçeveleyip** hem staj defterine hem de projenin
gelecekte gerçek bir ürüne evrilmesi ihtimaline karşı net bir yol haritası
üretmek.

## 2. "Gerçek" Bir ERP'yi Tanımlayan Özellikler

### 2.1. Fonksiyonel genişlik

Güncel ERP literatürü genelde 13 çekirdek modül etrafında toplanıyor: Finans,
Satınalma, Satış, Stok/Depo, Üretim, Proje Yönetimi, İK/Bordro, Müşteri
Hizmetleri, Sabit Kıymet Yönetimi, Tedarik Zinciri, Kalite, Uyumluluk/Risk,
Raporlama ve Analitik. Türkiye pazarındaki yerleşik oyuncular (Logo/Netsis,
Mikro) da bu modülleri "ihtiyaca göre eklenebilen" paketler olarak sunuyor ve
üçü de **e-Fatura/e-Arşiv/e-Defter entegrasyonunu paketin ayrılmaz bir parçası**
olarak veriyor (bkz. Bölüm 2.4 — bu, Türkiye'de bir ERP'yi "gerçek" yapan asgari
şart).

### 2.2. Mimari standartlar

2026 itibarıyla modern ERP'lerin ortak paydası: **modüler/API-first mimari** —
her iş fonksiyonu kendi modülünde yaşıyor ve modüller birbiriyle paylaşılan bir
API seti üzerinden konuşuyor. Bu, dış sistemlerle (muhasebeci yazılımı, e-ticaret
sitesi, kargo entegrasyonu, mobil uygulama) entegrasyonu ucuzlatıyor. Ayrıca
"gömülü AI, mobil öncelikli erişim ve açık API'ler artık opsiyonel değil"
deniyor — yani sadece web arayüzü sunan, dışarıya hiç API açmayan bir sistem
2026 standardına göre eksik sayılıyor.

### 2.3. Kesişen kurumsal gereksinimler (cross-cutting)

Modülden bağımsız, her ciddi ERP'de bulunması beklenen ortak katmanlar:

- **Audit trail**: Sistem tarafından otomatik üretilen, kullanıcının elle
  girmediği, **değiştirilemez (append-only)** kayıtlar. Alan adları standart:
  `user_id`, `entity_id`, `action_type`, `before_state`/`after_state`. Amaç:
  "bu tutarı kim, ne zaman, hangi değerden hangi değere değiştirdi?" sorusuna
  her zaman cevap verebilmek.
- **Workflow/onay motoru**: Talepleri, onayları, son tarihleri, eskalasyonları
  yöneten bir katman. Örn. "500.000 TL üstü sipariş müdür onayı gerektirir."
- **Rol bazlı erişim kontrolü (RBAC) + görevler ayrılığı (segregation of
  duties)**: Her kullanıcıya işini yapması için gereken minimum yetki
  (least privilege) verilir; ERP yönetimi ile denetim-log yönetimi ayrı
  kişilerde olur; tek bir kişi bir finansal işlemin baştan sona her adımını
  tek başına yapamaz.
- Bunların yanında: bildirim sistemi, doküman/dosya ekleme, çoklu para
  birimi/şirket/dil desteği, raporlama/BI katmanı, arka plan iş işleme,
  önbellekleme, tam metin arama.

### 2.4. Türkiye'ye özgü yasal zorunluluklar (2026 itibarıyla)

Bu, araştırmanın en kritik bulgusu: **2026'da bilanço esasına göre defter tutan
birinci sınıf tüccarlar, ciro sınırı olmaksızın tüm faturalarını e-Arşiv Fatura
olarak düzenlemek zorunda** — kağıt fatura dönemi bu mükellefler için fiilen
bitti. Ayrıca e-Fatura'ya kayıtlı ve belirli ciro üzerindeki mükellefler için
e-İrsaliye zorunluluğu var (2025 cirosu 10M TL+ olanlar için 1 Temmuz 2026'ya
kadar geçiş şartı). Teknik olarak bu şu anlama geliyor:

- ERP'den çıkan veri (JSON/XML/DB tablosu), bir ara katman tarafından **GİB
  uyumlu UBL-TR formatına** dönüştürülmeli.
- Ya bir **özel entegratör** üzerinden ya da **doğrudan entegrasyon
  modeliyle** GİB'e bağlanılmalı.
- **Mali mühür veya nitelikli elektronik imza** gerekiyor.
- Zamanında geçiş yapılmazsa VUK kapsamında özel usulsüzlük cezaları var.

**Sonuç: Türkiye'de gerçek anlamda ticari kullanılabilecek bir ERP/muhasebe
yazılımı, bugün itibarıyla e-Fatura/e-Arşiv/e-Defter/e-İrsaliye entegrasyonu
olmadan fiilen var olamaz.** Bu, TicariSistem'nin GOREVLER.md'de "kapsam dışı"
diye not edilen maddeler arasında en ağır basanı.

### 2.5. Kalite ve DevOps standartları

Ciddi bir ERP'nin arkasında beklenen (kullanıcı hiç görmez ama "gerçek ürün"
ile "demo" arasındaki farkı yaratan) katman: otomatik test (unit + integration
+ uçtan uca), CI/CD pipeline, yapılandırılmış loglama ve gözlemlenebilirlik
(APM, health check endpoint), sağlam güvenlik (gizli anahtar yönetimi, güçlü
şifre politikası, 2FA, KVKK/GDPR uyumu), yedekleme ve felaket kurtarma planı.

## 3. TicariSistem'nin Bugünkü Envanteri

Kod tabanından çıkarılan özet (bu doküman için yeniden tarandı):

| Katman | Durum |
|---|---|
| Stack | ASP.NET Core 8 MVC, EF Core, PostgreSQL, Identity, QuestPDF, ClosedXML, Chart.js, Docker |
| Modüller | Cari/Finans, Çek-Senet, Malzeme/Stok, Satınalma (Sipariş→İrsaliye→Fatura), Satış (Talep→Teklif→Sipariş→Sevk→Fatura), sade Muhasebe (otomatik yevmiye), Dashboard |
| Kimlik doğrulama | Identity, 3 rol (Admin/Muhasebe/Satış), şifre politikası: min 6 karakter, büyük harf/özel karakter zorunlu değil, 2FA yok |
| API katmanı | Yok — sadece MVC View'lar + birkaç ad-hoc JSON endpoint'i (DataTables `ListeVerisi` aksiyonları). Dışarıya açık, belgelenmiş bir REST/OData API yok |
| Audit/history | `BaseEntity` içinde `CreatedAt`, `UpdatedAt`, `CreatedBy`, `IsDeleted` var — ama alan bazlı önce/sonra değeri tutan ayrı bir audit log tablosu yok, silen/pasifleştiren kişi tutulmuyor |
| Workflow/onay | Tek adımlı Onayla/İptal Et aksiyonları var (fatura, irsaliye, hareket fişi) — çok kademeli onay zinciri, tutar bazlı yetkilendirme yok |
| Bildirim | Sadece "şifremi unuttum" için MailKit ile email var — proaktif bildirim (kritik stok, vadesi gelen çek/senet) yok |
| Test | **Hiç test projesi yok** (`find` ile `*Tests.csproj` sıfır sonuç) |
| CI/CD | Yok (`.github/`, `.gitlab-ci.yml` yok) |
| Loglama | Varsayılan `ILogger` + özel `RequestResponseLoggingMiddleware`/`ExceptionHandlingMiddleware` — yapılandırılmış log (Serilog vb.), merkezi log toplama, APM yok |
| Health check | Yok |
| e-Fatura/e-Defter/e-İrsaliye | Yok — PDF çıktısı var (QuestPDF), ama GİB'e giden bir entegrasyon yok |
| Deploy | Docker Compose (web+db+nginx), tek sunucu, tek şirket varsayımı; `EnableHttpsRedirection:false` (domain gelene kadar geçici, nginx.conf'ta not düşülmüş) |
| Çoklu para birimi | Sadece Dashboard'da sabit kurla ($) yaklaşık gösterim; belge bazında gerçek döviz/kur alanı yok |

## 4. Boşluk Analizi

Her madde için: gerçek ERP'de ne var → TicariSistem'de ne var/yok → önem
derecesi → staj kapsamında gerçekçi mi.

### 4.1. Fonksiyonel derinlik boşlukları (modül düzeyinde)

| Boşluk | Gerçek ERP'de | TicariSistem'de | Önem | Staj kapsamında gerçekçi mi |
|---|---|---|---|---|
| Üretim/MRP | Ürün ağacı, iş emri, kapasite planlama | Yok (bilerek) | Yüksek (üretici firmalar için) | Hayır — CLAUDE.md'de bilinçli kapsam dışı |
| Bordro/İK | Personel kartı, puantaj, bordro hesaplama | Yok (bilerek) | Yüksek (her firma için) | Hayır — ayrı bir uzmanlık alanı, 30 günde gerçekçi değil |
| Proje Yönetimi | Görev, zaman çizelgesi, bütçe takibi | Yok (bilerek) | Orta | Hayır |
| CRM (bağımsız modül) | Fırsat/pipeline yönetimi, aktivite takibi | Satış içine gömülü (Talep/Teklif) | Düşük-Orta | Hayır, mevcut haliyle yeterli |
| Kalite Kontrol | Muayene planı, uygunsuzluk takibi | Yok (bilerek) | Orta (üretim firmaları için) | Hayır |
| E-Ticaret / Dış Ticaret | Pazaryeri entegrasyonu, gümrük/GTİP | Yok (bilerek) | Orta | Hayır |

Bu satır, GOREVLER.md'nin zaten yaptığı kapsam dışı kararların gerçek ERP
standartlarına göre "neden gerçekten eksik ama neden de haklı bir karar
olduğunun" teyididir.

### 4.2. Yasal/uyumluluk boşluğu (en kritik madde)

| Boşluk | Gerçek ERP'de | TicariSistem'de | Önem | Staj kapsamında gerçekçi mi |
|---|---|---|---|---|
| e-Fatura / e-Arşiv | UBL-TR formatına dönüşüm + GİB'e iletim | Yok, sadece PDF çıktısı var | **Kritik** — bugün Türkiye'de bilanço usulü mükellefler için yasal zorunluluk | Kısmen — bir "sandbox/test entegratörü" ile temel akışı göstermek 2-3 günlük iş, tam üretim entegrasyonu değil |
| e-Defter | Yevmiye/kebir'in GİB formatında dışa aktarımı | Yok | Yüksek | Hayır, basit muhasebe kapsamının çok ötesinde |
| e-İrsaliye | Sevkiyat belgesinin elektronik iletimi | Yok, sadece PDF | Yüksek (10M+ ciro için 2026'da zorunlu) | Hayır |
| Mali mühür / e-imza | Belgeleri imzalamak için gerekli | Yok | Yüksek | Hayır (donanım/sertifika gerektirir) |

**Bu bölüm staj defterinde ayrıca vurgulanmalı**: "Neden e-Fatura eklemedik"
sorusunun cevabı sadece "vaktimiz yoktu" değil, "gerçek bir e-Fatura
entegrasyonu mali mühür, özel entegratör sözleşmesi ve UBL-TR şema uyumluluğu
gerektiriyor — bu bir öğrenci projesinin kapsamının doğası gereği dışında."

### 4.3. Mimari/altyapı boşlukları

| Boşluk | Gerçek ERP'de | TicariSistem'de | Önem | Staj kapsamında gerçekçi mi |
|---|---|---|---|---|
| API katmanı | REST/OData + webhook, mobil/3. parti entegrasyon | Yok, sadece View'lar | Orta-Yüksek | **Evet** — mevcut Service katmanının üstüne ince bir `[ApiController]` katmanı eklemek, mevcut mimariyi bozmadan yapılabilir |
| Çok kiracılılık (multi-tenant) | Tek kurulumda birden çok firma | Tek şirket varsayımı (Sube var ama tek firma) | Düşük (KOBİ hedefi için gerekli değil) | Hayır, gerekli de değil |
| Arka plan iş işleme | Zamanlanmış görevler (vade hatırlatma, gecikmiş rapor emaili) | Yok, her şey senkron HTTP isteği içinde | Orta | Kısmen — Hangfire/`IHostedService` ile "vadesi 3 gün kalan çek/senet" günlük emaili gerçekçi bir eklenti |
| Önbellekleme | Sık okunan referans verisi (kategori, hesap planı) için cache | Yok | Düşük (bu veri hacminde önemsiz) | Hayır, gerekli değil |
| Tam metin arama | Elasticsearch/DB tam metin indeksleri | Yok, sadece `ILIKE` ile basit arama | Düşük | Hayır, gerekli değil |

### 4.4. Kurumsal cross-cutting boşluklar

| Boşluk | Gerçek ERP'de | TicariSistem'de | Önem | Staj kapsamında gerçekçi mi |
|---|---|---|---|---|
| Audit trail (alan bazlı geçmiş) | Her değişiklik `before/after` ile append-only log'da | Sadece `UpdatedAt`/`CreatedBy`, hangi alanın ne olduğu geçmişi yok | Yüksek (finansal sistemde denetim için) | **Evet** — genel bir `AuditLog` tablosu + `SaveChangesAsync` override ile EF Core `ChangeTracker` üzerinden otomatik yakalama, orta efor |
| Workflow/çok kademeli onay | Tutar/rol bazlı onay zinciri | Tek adımlı Onayla/İptal | Orta | Kısmen — "X TL üstü faturayı sadece Admin onaylayabilir" gibi tek kural eklemek küçük bir iş |
| Granüler RBAC | Aksiyon/alan bazlı izin (permission tablosu) | 3 sabit rol, `[Authorize(Roles=...)]` | Orta | Kısmen — mevcut 3 rol küçük ölçek için savunulabilir, ama "hangi rol hangi CRUD'u yapabilir" tablosu yok |
| Bildirim sistemi | Proaktif email/push (kritik stok, vade) | Yok (email sadece şifre sıfırlama) | Orta | **Evet** — kritik stok listesi zaten hesaplanıyor, günlük email'e çevirmek küçük bir eklenti |
| Doküman/dosya ekleme | Fatura/sipariş üzerine sözleşme/imzalı irsaliye taraması | Yok | Düşük-Orta | Kısmen — basit bir dosya yükleme alanı eklenebilir ama depolama altyapısı (S3/disk) kararı gerektirir |
| Segregation of duties | Aynı kişi hem oluşturup hem onaylayamaz | Yok — aynı kullanıcı fatura oluşturup kendi onaylayabiliyor | Orta | Kısmen — "CreatedBy != OnaylayanKullanici" kontrolü küçük bir kural |

### 4.5. Kalite/DevOps boşlukları

| Boşluk | Gerçek ERP'de | TicariSistem'de | Önem | Staj kapsamında gerçekçi mi |
|---|---|---|---|---|
| Otomatik test | Unit + integration + e2e, CI'da zorunlu | **Hiç yok** — CLAUDE.md "her değişiklikten sonra dotnet test" diyor ama test projesi hiç oluşturulmamış | **Yüksek** — bu bir tutarsızlık, kendi kuralımızı bile karşılamıyoruz | **Evet, kesinlikle** — en azından Service katmanı için (stok düşümü, cari bakiye, negatif stok kontrolü gibi kritik iş kuralları) xUnit ile birkaç test |
| CI/CD | Her push'ta build+test+deploy | Yok | Orta-Yüksek | **Evet** — GitHub Actions ile `dotnet build` + `dotnet test` çalıştıran basit bir workflow, yarım gün iş |
| Yapılandırılmış loglama | Serilog/structured log + merkezi toplama | Varsayılan `ILogger`, sadece konsola | Orta | **Evet** — Serilog + dosyaya JSON log, düşük efor |
| Health check endpoint | `/health` — DB bağlantısı, disk, bağımlılıklar | Yok | Düşük-Orta | **Evet** — `AddHealthChecks()` ile 15 dakikalık iş |
| Şifre politikası | Min 8-10 karakter, karmaşıklık, 2FA opsiyonu | Min 6 karakter, büyük harf/özel karakter zorunlu değil, 2FA yok | Orta (güvenlik) | **Evet** — `Program.cs`'te birkaç satır konfigürasyon |
| HTTPS zorunluluğu | Her zaman aktif | `EnableHttpsRedirection: false` (domain/SSL gelene kadar bilinçli geçici) | Düşük (zaten planlı) | Zaten Gün 29 planında var |
| Backup/DR | Otomatik yedekleme + test edilmiş geri yükleme | Docker volume var ama dokümante edilmiş bir yedekleme stratejisi yok | Orta | Kısmen — `pg_dump` cron job'ı + README'ye yedekleme talimatı düşük efor |

### 4.6. Kullanıcı deneyimi boşlukları (daha önce konuşulan, henüz uygulanmayan)

- ~~Kaydedilmeden çıkış uyarısı~~ ve ~~zorunlu alan görsel işareti~~ —
  **tamamlandı** (`wwwroot/js/site.js` + `wwwroot/css/site.css`, Kademe 1
  commit'inde eklendi). Bu bölümün eski hali "docs/kayit-karti-tasarim-plani.md
  Bölüm 8.2" diye bir kaynak gösteriyordu; o doküman incelendiğinde böyle bir
  bölüm hiç var olmamış — yanlış/uydurma bir atıftı, düzeltildi.
- **Toplu işlem** (checkbox ile çoklu seçip toplu onaylama/silme) — grid'lerde
  satır bazlı aksiyon var, toplu seçim yok.
- **Kayıt versiyon karşılaştırma / "kim ne zaman değiştirdi" ekranı** — 4.4'teki
  audit trail'in UI tarafı.

## 5. Önceliklendirilmiş Öneri Listesi

### Kademe 1 — Staj kapsamında bile gerçekçi, düşük efor / yüksek "gerçek ürün" hissi

1. **Test projesi kurmak** (`SakaryaERP.Tests`, xUnit) — en azından
   `MalzemeHareketFisiService.OnaylaAsync` (negatif stok kontrolü),
   `CariFisiService` (bakiye güncelleme yönü), `SevkIrsaliyesiService`
   (sipariş miktarını aşan sevkiyat engeli) gibi kritik iş kurallarını test
   etmek. CLAUDE.md zaten "dotnet test" adımını öngörüyor, sadece proje eksik.
2. **CI pipeline** — GitHub Actions ile her push'ta `dotnet build` + `dotnet
   test`.
3. **Yapılandırılmış loglama** — Serilog, dosyaya JSON formatında log.
4. **Health check endpoint** (`/health`) — DB bağlantısını kontrol eden basit
   bir uç nokta, Docker/nginx health check'iyle entegre edilebilir.
5. **Şifre politikası sıkılaştırma** — min 8 karakter + en az bir rakam/harf
   karışımı zorunluluğu (`Program.cs`'te birkaç satır).
6. ~~Kaydedilmeden çıkış uyarısı + zorunlu alan işareti~~ — **tamamlandı**
   (bkz. 4.6).
7. **Basit audit log tablosu** — `AuditLog` (EntityAdi, EntityId, Alan,
   EskiDeger, YeniDeger, KullaniciAdi, Tarih), `DbContext.SaveChangesAsync`
   override'ında `ChangeTracker.Entries()` üzerinden otomatik doldurulur.

### Kademe 2 — Staj sonrası "gerçek ürün"e evrilme için gerekli

8. **e-Fatura/e-Arşiv entegrasyonu** — bir test/sandbox entegratörü (GİB'in
   kendi test ortamı veya bir özel entegratörün sandbox API'si) ile temel
   "fatura gönder → UBL-TR'ye çevir → durumu sorgula" akışını göstermek.
9. **Granüler RBAC** — `Permission` tablosu, rol yerine/rolün yanında aksiyon
   bazlı yetkilendirme (`Cari.Sil`, `Fatura.Onayla` gibi).
10. **Çok kademeli onay zinciri** — tutar eşiğine göre ikinci bir onay adımı.
11. **Proaktif bildirim** — kritik stok ve vadesi yaklaşan çek/senet için
    günlük email (arka plan job + mevcut MailKit altyapısı).
12. **Basit REST API katmanı** — mevcut Service arayüzlerinin üzerine ince bir
    `[ApiController]` katmanı, gelecekte mobil/3. parti entegrasyon için.

### Kademe 3 — Tam kurumsal ölçek (muhtemelen bu ürünün hedefi değil)

13. Çok kiracılılık, mikroservis mimarisi, tam Üretim/MRP, Bordro, gelişmiş
    BI/veri ambarı, e-Defter, mali mühür entegrasyonu.

## 6. Staj Defteri İçin Kapanış Notu Önerisi

> "TicariSistem'i HarmonyERP gibi ticari bir ERP'nin gerçek modül yapısına
> sadık kalarak, ama kapsamını bilinçli daralttım. Araştırmam sonucunda gördüm
> ki gerçek bir ERP'yi 'gerçek' yapan şey sadece daha fazla ekran değil; asıl
> fark denetlenebilirlik (audit trail), yetkilendirme derinliği, otomatik test
> altyapısı ve — Türkiye'de en kritik olanı — e-Fatura/e-Defter/e-İrsaliye gibi
> yasal zorunluluklara uyum. Bu son madde tek başına, bugün Türkiye'de ticari
> olarak kullanılabilecek bir ERP'nin üzerine inşa edildiği temel bir gerekliliği
> oluşturuyor ve bir öğrenci projesinin doğal kapsamının ötesinde. Bunun yerine
> 30 günlük sürede uçtan uca çalışan bir ticari döngü + bu döngünün üzerine
> kurulacak denetlenebilirlik/test altyapısının temellerini (Kademe 1 önerileri)
> hedefledim."

## 7. Kaynaklar

- [ERP Requirements Checklist & Guide (2026)](https://www.erpresearch.com/en-us/erp-requirements)
- [ERP Modules Explained — Core ERP System Modules Guide 2026](https://www.erpresearch.com/en-us/erp-modules)
- [e-Fatura ve e-Defter Geçiş Rehberi: Ciro Limitleri ve Kritik Tarihler (2026)](https://www.qnbesolutions.com.tr/blog/2026-efatura-ve-edefter-gecis-rehberi)
- [e-Fatura, e-Arşiv ve e-İrsaliye 2026: Geçiş Hadleri ve ERP Entegrasyonu](https://qera.com.tr/blog/e-fatura-e-arsiv-e-irsaliye-2026-gecis-hadleri-ve-erp-entegrasyonu)
- [2026 e-Dönüşüm Takvimi ve Zorunluluk Rehberi — Sovos Türkiye](https://sovos.com/tr/blog/kdv/sirketler-icin-2026-e-donusum-takvimi-ve-zorunluluklar/)
- [Audit Trail Review: A Step-by-Step Guide with Best Practices](https://www.accountablehq.com/post/audit-trail-review-a-step-by-step-guide-with-best-practices-and-a-compliance-checklist)
- [Role-based access control in ERP systems](https://mysoftheaven.com/pages/role-based-access-control-in-erp-systems)
- [Best Practices for Managing Audit Trails in ERP](https://blubanyan.com/best-practices-for-managing-audit-trails-in-erp/)
- [Logo — Mikro — Zirve Karşılaştırması (Detaylı Rehber – 2026)](https://turkmuhasebe.com/logo-mikro-zirve-karsilastirmasi-detayli-rehber/)
