# SakaryaERP — Varlık-İlişki Diyagramı

```mermaid
erDiagram

    %% ── ORTAK / FİNANS ──────────────────────────────────────────
    Cari {
        int     Id          PK
        string  CariKodu
        string  Unvan
        int     CariTipi    "enum: Müşteri / Tedarikçi / HerIkisi"
        string  VergiNo
        string  Adres
        string  Telefon
        decimal Bakiye
    }

    BankaHesabi {
        int     Id          PK
        string  HesapAdi
        string  IBAN
        decimal Bakiye
        string  ParaBirimi
    }

    KasaHesabi {
        int     Id          PK
        string  KasaAdi
        decimal Bakiye
        string  ParaBirimi
    }

    CariFisi {
        int      Id       PK
        int      CariId   FK
        datetime Tarih
        int      FisTipi  "enum: Borc / Alacak"
        decimal  Tutar
        string   Aciklama
    }

    CekSenet {
        int      Id          PK
        int      BelgeTipi   "enum: Cek / Senet"
        string   BelgeNo
        int      CariId      FK
        datetime VadeTarihi
        decimal  Tutar
        int      Durum       "enum: Portfolyde/Tahsilde/Ciro/Karsiliqsiz/TahsilEdildi"
    }

    %% ── MALZEME / STOK ───────────────────────────────────────────
    Malzeme {
        int     Id           PK
        string  MalzemeKodu
        string  MalzemeAdi
        string  Marka
        string  Birim
        int     TeminTuru    "enum: Alis / Uretim / AlisUretim"
        int     StokTipi     "enum: TicariMal/Hammadde/YariMamul/Mamul"
        decimal Bakiye
    }

    MalzemeHareketFisi {
        int      Id           PK
        string   FisNo
        datetime Tarih
        int      HareketTipi  "enum: Giris / Cikis"
        int      MalzemeId    FK
        decimal  Miktar
        string   Aciklama
    }

    %% ── SATIŞ AKIŞI ─────────────────────────────────────────────
    MusteriTalebi {
        int      Id       PK
        int      CariId   FK
        datetime Tarih
        string   Aciklama
        int      Durum
    }

    SatisTeklifi {
        int      Id              PK
        int      CariId          FK
        int      MusteriTalebiId FK "nullable"
        datetime Tarih
        int      Durum
    }

    SatisTeklifiKalemi {
        int     Id             PK
        int     SatisTeklifiId FK
        int     MalzemeId      FK
        decimal Miktar
        decimal BirimFiyat
        decimal KdvOrani
    }

    SatisSiparisi {
        int      Id              PK
        int      CariId          FK
        int      SatisTeklifiId  FK "nullable"
        datetime Tarih
        int      Durum
    }

    SatisSiparisiKalemi {
        int     Id               PK
        int     SatisSiparisiId  FK
        int     MalzemeId        FK
        decimal Miktar
        decimal BirimFiyat
        decimal KdvOrani
    }

    SevkIrsaliyesi {
        int      Id               PK
        int      SatisSiparisiId  FK
        datetime Tarih
        string   SevkAdresi
    }

    SevkIrsaliyesiKalemi {
        int     Id               PK
        int     SevkIrsaliyesiId FK
        int     MalzemeId        FK
        decimal Miktar
    }

    SatisFaturasi {
        int      Id               PK
        int      CariId           FK
        int      SevkIrsaliyesiId FK "nullable"
        datetime Tarih
        int      Durum
    }

    SatisFaturasiKalemi {
        int     Id              PK
        int     SatisFaturasiId FK
        int     MalzemeId       FK
        decimal Miktar
        decimal BirimFiyat
        decimal KdvOrani
    }

    %% ── SATINALMA AKIŞI ─────────────────────────────────────────
    AlisSiparisi {
        int      Id       PK
        int      CariId   FK
        datetime Tarih
        int      Durum    "enum: Beklemede / Onaylandi / Iptal"
    }

    AlisSiparisiKalemi {
        int     Id              PK
        int     AlisSiparisiId  FK
        int     MalzemeId       FK
        decimal Miktar
        decimal BirimFiyat
        decimal KdvOrani
    }

    AlisFaturasi {
        int      Id              PK
        int      CariId          FK
        int      AlisSiparisiId  FK "nullable"
        datetime Tarih
        int      Durum
    }

    AlisFaturasiKalemi {
        int     Id             PK
        int     AlisFaturasiId FK
        int     MalzemeId      FK
        decimal Miktar
        decimal BirimFiyat
        decimal KdvOrani
    }

    %% ── MUHASEBE ─────────────────────────────────────────────────
    HesapPlani {
        int    Id         PK
        string HesapKodu
        string HesapAdi
        int    HesapTipi  "enum: Aktif / Pasif / Gelir / Gider"
    }

    MuhasebeFisi {
        int      Id              PK
        string   FisNo
        datetime Tarih
        int      SatisFaturasiId FK "nullable"
        int      AlisFaturasiId  FK "nullable"
    }

    MuhasebeFisiKalemi {
        int     Id             PK
        int     MuhasebeFisiId FK
        int     HesapPlaniId   FK
        decimal Borc
        decimal Alacak
        string  Aciklama
    }

    %% ── İLİŞKİLER ───────────────────────────────────────────────

    %% Cari merkezi
    Cari            ||--o{ CariFisi            : "fişleri"
    Cari            ||--o{ CekSenet            : "çek/senetleri"
    Cari            ||--o{ MusteriTalebi       : "talepleri"
    Cari            ||--o{ SatisTeklifi        : "teklifleri"
    Cari            ||--o{ SatisSiparisi       : "satış siparişleri"
    Cari            ||--o{ SatisFaturasi       : "satış faturaları"
    Cari            ||--o{ AlisSiparisi        : "alış siparişleri"
    Cari            ||--o{ AlisFaturasi        : "alış faturaları"

    %% Satış akışı: Talep → Teklif → Sipariş → Sevk → Fatura
    MusteriTalebi   ||--o{ SatisTeklifi        : "dönüşür"
    SatisTeklifi    ||--o{ SatisTeklifiKalemi  : "kalemleri"
    SatisTeklifi    ||--o{ SatisSiparisi       : "dönüşür"
    SatisSiparisi   ||--o{ SatisSiparisiKalemi : "kalemleri"
    SatisSiparisi   ||--o{ SevkIrsaliyesi      : "sevki"
    SevkIrsaliyesi  ||--o{ SevkIrsaliyesiKalemi: "kalemleri"
    SevkIrsaliyesi  ||--o{ SatisFaturasi       : "faturalanır"
    SatisFaturasi   ||--o{ SatisFaturasiKalemi : "kalemleri"

    %% Satınalma akışı: Sipariş → Fatura
    AlisSiparisi    ||--o{ AlisSiparisiKalemi  : "kalemleri"
    AlisSiparisi    ||--o{ AlisFaturasi        : "dönüşür"
    AlisFaturasi    ||--o{ AlisFaturasiKalemi  : "kalemleri"

    %% Malzeme bağlantıları
    Malzeme         ||--o{ MalzemeHareketFisi  : "hareketleri"
    Malzeme         ||--o{ SatisTeklifiKalemi  : ""
    Malzeme         ||--o{ SatisSiparisiKalemi : ""
    Malzeme         ||--o{ SevkIrsaliyesiKalemi: ""
    Malzeme         ||--o{ SatisFaturasiKalemi : ""
    Malzeme         ||--o{ AlisSiparisiKalemi  : ""
    Malzeme         ||--o{ AlisFaturasiKalemi  : ""

    %% Muhasebe
    SatisFaturasi   ||--o| MuhasebeFisi        : "muhasebe kaydı"
    AlisFaturasi    ||--o| MuhasebeFisi        : "muhasebe kaydı"
    MuhasebeFisi    ||--o{ MuhasebeFisiKalemi  : "kalemleri"
    HesapPlani      ||--o{ MuhasebeFisiKalemi  : ""
```

## Notlar

- `CariTipi` = Müşteri | Tedarikçi | HerIkisi — Tedarikçiler AlisSiparisi akışında, Müşteriler Satış akışında kullanılır.
- Tüm kalem tabloları (…Kalemi) ana belgeye bağlı alt satırlardır; ana belge silindiğinde cascade delete ile silinir.
- `MuhasebeFisi.SatisFaturasiId` ve `AlisFaturasiId` nullable'dır; bir fiş ya satış ya da alış faturasından üretilir.
- `SatisTeklifi.MusteriTalebiId` nullable'dır; talep olmadan da direkt teklif oluşturulabilir.
- Stok bakiyesi (`Malzeme.Bakiye`) yalnızca `MalzemeHareketFisi` onayında, alış faturası onayında ve satış faturası onayında güncellenir — doğrudan elle değiştirilmez.
