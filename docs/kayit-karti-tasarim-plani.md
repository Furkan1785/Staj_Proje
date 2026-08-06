# Kayıt Ekranları Tasarım Planı — Tüm Modüller

> **Durum:** Bölüm 6'daki 7 madde de uygulandı. Uygulama sırasında netleşen
> iki küçük kapsam ayarlaması: (1) Belge zinciri, planda tarif edilen tam
> 5 adımlı zincir yerine sadece **bir üst seviye linki** olarak kuruldu —
> alt seviyeler zaten mevcut "Siparişe Dönüştür" gibi aksiyon butonlarıyla
> karşılanıyordu, çok seviyeli zincir mevcut servis Include yapısını
> derinden değiştirmeyi gerektirirdi. (2) Malzeme Kartı formunda kimlik
> paneli, Detay'daki gibi salt-okunur değil, gerçek input alanlarından
> oluşuyor (referans ekranla birebir aynı: sol panel zorunlu/kimlik
> alanlarını, sağ sekmeler geri kalanını tutuyor).

## 1. Amaç

Paylaşılan HarmonyERP "Malzeme Kartı" ekran görüntüsündeki profesyonellik seviyesini
(bilgi yoğunluğu, düzen, gezinme) TicariSistem'in **tüm tekil kayıt ekranlarına**
(Detay/Ekle/Düzenle) kazandırmak. Bu doküman projedeki her modülü tek tek ele alır.

**Kapsam:** Sadece mevcut alanların/ekranların *düzeni ve sunumu*. Yeni entity, yeni
alan, yeni modül (Üretim, İhracat vb.) eklenmiyor — bkz. Bölüm 5 "Kapsam Dışı". Her
modülün önerisi o modülün gerçek entity/ViewModel alanlarına bakılarak çıkarıldı
(bkz. `Models/`, `ViewModels/`, mevcut `Views/*/Detay.cshtml`).

## 2. Referans Ekranın Çözümlemesi

HarmonyERP Malzeme Kartı ekranı 5 yapısal bölgeden oluşuyor:

| Bölge | İçerik | Amaç |
|---|---|---|
| Sol kimlik paneli | Id, Malzeme Kodu, Malzeme Adı, Kodu-2/Adı-2, Stok Tipi | Hangi sekmede olursan ol, kaydın kimliği hep görünür kalsın |
| Sekme çubuğu | Genel / Kategori ve Özellikler / Özel Kodlar / Fiyatlar / Birimler-Barkodlar / Üretim / İhracat / Boyut Bilgileri / Depo-Stok / Muhasebe | ~40 alanı tek ekrana sıkıştırmadan mantıksal gruplara ayırmak |
| Sağ üst görsel paneli | Ürün resmi | (Bizim kapsamımızda yok, bkz. Bölüm 5) |
| Alt işlem çubuğu | "Temel İşlemler ▾", Analiz, Alternatif Malzemeler, Resimler, Belgeler | İkincil aksiyonları ana formdan ayırmak |
| Kayıt gezinme + kaydet | ◄◄ ◄ 421/1035 ► ►►, Kaydet, Vazgeç | Listeye dönmeden kayıtlar arası gezinme |

Projedeki modülleri incelediğimde iki farklı kayıt tipi olduğunu gördüm — ikisi de
aynı desenden ("kimlik + gruplama + aksiyon ayrımı") besleniyor ama farklı şekilde
uygulanmalı:

## 3. İki Ekran Deseni

### 3.1. "Kart" deseni — tanımlayıcı/ana veri kayıtları

Malzeme, Cari, Banka/Kasa Hesabı, Çek/Senet gibi kayıtlar: çok sayıda bağımsız alan,
kalem/satır tablosu yok. HarmonyERP referansındaki **sekmeli** yapı buraya uygulanır.

```
┌─────────────────────────────────────────────────────────────┐
│ [Kod] Başlık (Ad)                    Durum Rozeti  Kaydet  Vazgeç │
├───────────────┬───────────────────────────────────────────────┤
│ Kimlik Paneli │ [Genel] [Grup2] [Grup3] [Geçmiş]               │
│ (kod, ad,     │ ┌─────────────────────────────────────────┐   │
│ durum rozeti) │ │  seçili sekmenin form alanları / bilgisi │   │
│               │ └─────────────────────────────────────────┘   │
├───────────────┴───────────────────────────────────────────────┤
│ İşlemler ▾  |  Excel'e Aktar  |  Yazdır                        │
└─────────────────────────────────────────────────────────────┘
```

- Bootstrap 5 `nav nav-tabs` + `tab-content` — ek kütüphane gerekmez.
- Kimlik paneli `col-md-3` sabit sütun, `position-sticky top-0`.
- Ekle ve Düzenle aynı sekmeli `_Form.cshtml`'i paylaşır (mevcut yapı zaten böyle);
  Detay ekranı aynı sekme başlıklarını salt-okunur modda gösterir.
- Alan sayısı azsa (≤6-7 alan, örn. Banka/Kasa Hesabı) sekme **açılmaz** — boş sekme
  daha az profesyonel görünür. Tek panel + kimlik başlığı yeterli.

### 3.2. "Belge" deseni — akış içindeki ticari belgeler

Sipariş, İrsaliye, Fatura, Teklif, Talep, Hareket Fişi gibi kayıtlar: bir "kalemler"
tablosu ve toplamlar (ara toplam/KDV/genel toplam) baskın içerik. Bunu sekmeye
bölmek, tabloyu daraltıp kullanışsızlaştırır. Bu yüzden mevcut `SatisFaturasi/Detay.cshtml`
gibi ekranlarda zaten uygulanan yapı korunur, üç noktada güçlendirilir:

```
┌─────────────────────────────────────────────────────────────┐
│ Belge No / Tipi                    Durum Rozeti  [Onayla][İptal][PDF] │
├─────────────────────────────────────────────────────────────┤
│ Belge Zinciri:  Talep ──▸ Teklif ──▸ Sipariş ──▸ Sevk ──▸ Fatura (●) │  <- YENİ
├─────────────────────────────────────────────────────────────┤
│ Genel Bilgiler kartı (Cari, Tarih, Vade, Açıklama)             │
├─────────────────────────────────────────────────────────────┤
│ Kalemler tablosu (tam genişlik) + Toplamlar                    │
└─────────────────────────────────────────────────────────────┘
```

1. **Belge zinciri** (yeni): Talep→Teklif→Sipariş→Sevk İrsaliyesi→Fatura akışının
   hangi adımda olunduğunu gösteren yatay bir stepper/breadcrumb. Geçmiş adımlar
   tıklanabilir link (örn. Fatura'dan "Sipariş No: SP-000123" metnine tıklayınca
   ilgili siparişin Detay sayfası açılır). Şu an bu bilgi düz metin olarak
   gösteriliyor (bkz. `SatisFaturasi/Detay.cshtml:56-59`), sadece linke çevrilecek —
   yeni veri gerekmiyor, FK'lar zaten mevcut (`SatisSiparisiId`, `SevkIrsaliyesiId`).
2. **Durum + aksiyon ayrımı**: Onayla/İptal/PDF gibi butonlar zaten üstte toplanmış
   durumda (iyi bir başlangıç) — bu düzen tüm belge ekranlarında birebir tekrar
   edilecek (bazılarında hâlâ karışık, bkz. Bölüm 4).
3. **Ortak partial**: `_BelgeZinciri.cshtml` adında tek bir partial, adım listesini
   ve aktif adımı parametre alacak; 7 belge ekranının hepsi aynı partial'i kullanacak
   (kopyala-yapıştır yerine tek kaynak).

## 4. Modül Modül Uygulama

### 4.1. Malzeme (pilot)

**Malzeme Kartı** — Kart deseni. Mevcut alanlar (`Malzeme.cs`,
`MalzemeFormViewModel`, `MalzemeDetayViewModel`) değiştirilmeden yeniden gruplanıyor:

| Sekme | Alanlar |
|---|---|
| **Genel** | MalzemeKodu, Barkod, MalzemeAdi, Marka, Kalite, Tip, Birim |
| **Kategori ve Fiyat** | KategoriId, TeminTuru, StokTipi, AlisFiyati, SatisFiyati, KdvOrani |
| **Depo / Stok** | Bakiye (salt okunur), MinStokMiktari, MaxStokMiktari, RafNo, stok durum rozeti |
| **Hareket Geçmişi** (sadece Detay) | Son N hareket — `IMalzemeHareketFisiService.GetMalzemeGecmisiAsync` zaten var, `MalzemeController.Detay`'e enjekte edilip özet gösterilecek, "Tümünü Gör" linki mevcut `StokRaporu/MalzemeGecmisi` sayfasına gider |

Kimlik paneli: MalzemeKodu, MalzemeAdi, stok durum rozeti (zaten hesaplanıyor,
`Detay.cshtml:5-21`). Alt toolbar: Düzenle, Excel'e Aktar, "Stok Hareketi Oluştur"
(malzeme önceden seçili `MalzemeHareketFisi/Ekle` yönlendirmesi), Listeye Dön.

**Malzeme Hareket Fişi** — Belge deseni. Şu an sadece `Index` (liste) ve `Ekle`
var, **Detay sayfası yok** — onaylanmış bir fişi tekrar açıp incelemek için ekran
eksik. Öneri: `Detay.cshtml` eklensin — Genel bilgi kartı (FisNo, Tarih, HareketTipi
rozeti, Şube, Durum rozeti) + Kalemler tablosu (Malzeme, Miktar, Açıklama). Belge
zinciri gerekmez (bu belge türü başka bir belgeden türemiyor).

### 4.2. Finans

| Modül | Deseni | Detay |
|---|---|---|
| **Cari** | Kart | Genel (Unvan, CariTipi, VergiNo) · İletişim (Adres, Telefon, EMail) · Bakiye/Limit (Bakiye, KrediLimiti — limit aşımında kırmızı rozet) · Hareketler (mevcut `Ekstre` sayfasına link, veya son 10 hareket gömülü özet). Kimlik paneli: CariKodu, Unvan, Bakiye rozeti. |
| **Banka Hesabı** | Kart (sekmesiz) | Sadece 5 alan (HesapAdi, BankaAdi, IBAN, Bakiye, ParaBirimi) — sekme açmaya değmez, kimlik başlığı + tek panel yeterli. Profesyonellik burada sekmeden değil tutarlı spacing/rozet kullanımından gelir. |
| **Kasa Hesabı** | Kart (sekmesiz) | Aynı gerekçe (HesapAdi yerine KasaAdi, IBAN yok) — tek panel. |
| **Cari Fişi** | Belge (kalemsiz) | Tek satırlık fiş (FisNo, Tarih, FisTipi, Tutar, OdemeYontemi, Banka/Kasa, Açıklama) — sekme veya kalemler tablosu gerekmez, mevcut form yeterli, sadece stil tutarlılığı. |

### 4.3. Çek/Senet

**Çek/Senet Kartı** — Kart deseni ama **Detay sayfası şu an yok**: durum
aksiyonları (`Tahsile Ver`/`Ciro Et`/`Tahsil Edildi`/`Karşılıksız`) doğrudan Index
grid'inde modal ile yapılıyor (bkz. `Views/CekSenet/Index.cshtml`). Alan sayısı az
(8 alan) olduğundan tam sekme yapısına gerek yok. Öneri: hafif bir `Detay.cshtml`
eklensin —

- Kimlik paneli: BelgeNo, BelgeTipi, Durum rozeti (mevcut vade renk mantığı:
  kırmızı/sarı/yeşil, `Index.cshtml:199-211`'deki JS mantığının sunucu tarafı karşılığı)
- Tek panel: CariId, VadeTarihi, Tutar, BankaAdi, SubeAdi, CiroBilgisi
- Alt toolbar: aynı 4 durum aksiyonu (artık grid modalından değil buradan da
  tetiklenebilir), Düzenle, Listeye Dön

Not: Uygulamada durum geçmişi (hangi tarihte hangi duruma geçti) ayrı bir tabloda
tutulmuyor, sadece güncel `Durum` alanı var — bu yüzden "Durum Geçmişi" sekmesi/
zaman çizelgesi **eklenmeyecek** (veri yok, uydurma bilgi gösterilmez).

### 4.4. Satınalma

**Alış Siparişi / Alış İrsaliyesi / Alış Faturası** — Belge deseni. Üçü de zaten
`Detay.cshtml` içeriyor, `SatisFaturasi/Detay.cshtml`'e benzer yapıda (Genel bilgi
kartı + Kalemler + Toplamlar). Eklenecek: belge zinciri partial'i (Sipariş → İrsaliye
→ Fatura, FK'lar zaten `AlisSiparisiId`/`AlisIrsaliyesiId` olarak mevcut) ve düz
metin olan "Sipariş No"/"İrsaliye No" alanlarının linke çevrilmesi.

`AlisTalebi` ve `AlisTeklifi` entity'leri kodda var ama hiçbir Controller/View'a
bağlı değil (kullanılmıyor) — bu plana dahil edilmiyor, ayrı bir karar/onay konusu.

### 4.5. Satış

| Modül | Deseni | Not |
|---|---|---|
| **Müşteri Talebi** | Belge (kalemsiz) | CariId, TalepNo, Tarih, İçerik, Durum — sekme/kalemler gerekmez, sadece durum rozeti ve stil tutarlılığı. Detay sayfası şu an yok (sadece Ekle+Index) — Talep'ten Teklif'e dönüşüm akışı düşünülünce hafif bir Detay eklenmesi faydalı olur. |
| **Satış Teklifi / Satış Siparişi / Sevk İrsaliyesi / Satış Faturası** | Belge | Zaten `Detay.cshtml` var, `SatisFaturasi/Detay.cshtml` en olgun örnek. Belge zinciri partial'i buraya da eklenir: Talep → Teklif → Sipariş → Sevk İrsaliyesi → Fatura (5 adım, en uzun zincir bu). |

### 4.6. Muhasebe

| Modül | Deseni | Not |
|---|---|---|
| **Hesap Planı** | Ağaç (kendine özgü) | Zaten hiyerarşik liste, sekmeye çevrilmez — mevcut yapı korunur. |
| **Muhasebe Fişi** | — | Şu an hiç UI'ı yok (Controller/View yok), sadece fatura onayında otomatik oluşuyor. Bu plan kapsamında yeni bir Detay ekranı **eklenmiyor** (CLAUDE.md: "muhasebe entegrasyonunu tam bir genel muhasebe sistemine çevirmeye çalışma") — istenirse ayrı bir görev olarak ele alınmalı. |

## 5. Kapsam Dışı (onay gerekmeden eklenmeyecek)

- **Üretim, İhracat, Boyut Bilgileri sekmeleri** — ilgili modül/veri modeli yok.
- **Resim/Belge yükleme paneli** — dosya depolama altyapısı gerektirir.
- **Kayıt gezinme (◄◄ ◄ N/M ► ►►)** — filtreli liste bağlamını taşımak gerekir,
  orta karmaşıklık; pilot sonrası ayrı görev.
- **Üstte açık-sekme (MDI tarzı üst çubuk)** — masaüstü uygulama alışkanlığı, web
  MVC sayfa modeline uymuyor.
- **Çek/Senet durum geçmişi zaman çizelgesi** — veri modelinde tutulmuyor, uydurma
  veri gösterilmeyecek.
- **AlisTalebi / AlisTeklifi ekranları** — bu entity'ler hiç kullanılmıyor, bu
  planla birlikte aktifleştirilmeyecek (scope büyütme, ayrı onay gerekir).
- **Muhasebe Fişi Detay ekranı** — şu an hiç UI'ı yok, bu plana dahil değil.

## 6. Uygulama Sırası

1. Malzeme Kartı (Detay + Ekle/Düzenle) — pilot, Kart deseni
2. Cari Kartı — Kart deseni, ikinci örnek
3. `_BelgeZinciri.cshtml` partial'i + Satış belgelerine (Teklif→Sipariş→Sevk→Fatura) uygulanması
4. Aynı partial'in Satınalma belgelerine (Sipariş→İrsaliye→Fatura) uygulanması
5. Çek/Senet'e hafif Detay sayfası eklenmesi
6. Malzeme Hareket Fişi'ne Detay sayfası eklenmesi
7. Banka Hesabı / Kasa Hesabı / Cari Fişi — sadece stil tutarlılığı (sekme yok)

Her modül tek başına bitirilip commit'lenecek (CLAUDE.md: "bir günde birden fazla
modülü yarım yamalak başlatma" kuralı burada da geçerli), her biri ayrı feature
branch'te.

## 7. Kabul Kriterleri

- `dotnet build` 0 hata
- Tüm sekme/alan/buton etiketleri Türkçe, HarmonyERP'deki isimlerle birebir
- Mevcut DataTables liste ekranları etkilenmiyor (bu plan sadece tekil kayıt
  ekranlarını kapsıyor)
- Mobil/dar ekranda sekmeler ve belge zinciri yatayda kaydırılabilir
  (`overflow-x`) şekilde bozulmadan çalışıyor
- Belge zinciri linkleri gerçek FK ilişkilerine dayanıyor, hiçbir adımda uydurma
  veri/geçmiş gösterilmiyor
- Kart deseni Malzeme'de, Belge deseni Satış Faturası'nda onaylandıktan sonra
  diğer modüllere ortak partial/CSS ile uygulanıyor (kopyala-yapıştır değil)
