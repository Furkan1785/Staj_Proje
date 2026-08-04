using System.Globalization;
using SakaryaERP.Helpers;
using SakaryaERP.Models;
using SakaryaERP.ViewModels;

namespace SakaryaERP.Services;

public class DashboardService : IDashboardService
{
    // Dış Ticaret modülü kapsam dışı bırakıldığı için sistemde canlı kur akışı yok;
    // kartlardaki $ tutarı sadece sabit bir kurla yaklaşık gösterim amaçlıdır.
    private const decimal UsdKuru = 35m;
    private const int TopN = 5;
    private const int MaxKategoriDilimi = 8;

    private readonly ISatisFaturasiService _satisFaturasiService;
    private readonly IAlisFaturasiService _alisFaturasiService;
    private readonly IMalzemeService _malzemeService;

    public DashboardService(ISatisFaturasiService satisFaturasiService, IAlisFaturasiService alisFaturasiService, IMalzemeService malzemeService)
    {
        _satisFaturasiService = satisFaturasiService;
        _alisFaturasiService = alisFaturasiService;
        _malzemeService = malzemeService;
    }

    public async Task<DashboardViewModel> GetDashboardAsync(DashboardAralik aralik, IReadOnlyCollection<int> kategoriIdler)
    {
        var satisFaturalari = await _satisFaturasiService.GetOnaylanmisListeAsync();
        var alisFaturalari = await _alisFaturasiService.GetOnaylanmisListeAsync();
        var kritikStok = await _malzemeService.GetKritikStokListesiAsync();

        var (tarihBaslangic, tarihBitis) = AralikTarihleri(aralik);
        var kategoriFiltreAktif = kategoriIdler.Count > 0;

        bool KategoriEslesir(int? kategoriId) =>
            !kategoriFiltreAktif || (kategoriId is not null && kategoriIdler.Contains(kategoriId.Value));
        bool TarihEslesir(DateTime tarih) =>
            (tarihBaslangic is null || tarih >= tarihBaslangic) && (tarihBitis is null || tarih <= tarihBitis);

        var satisKalemleri = satisFaturalari
            .Where(f => TarihEslesir(f.Tarih))
            .SelectMany(f => f.Kalemler.Where(k => KategoriEslesir(k.Malzeme.KategoriId)).Select(k => (Fatura: f, Kalem: k)))
            .ToList();

        var alisKalemleri = alisFaturalari
            .Where(f => TarihEslesir(f.Tarih))
            .SelectMany(f => f.Kalemler.Where(k => KategoriEslesir(k.Malzeme.KategoriId)).Select(k => (Fatura: f, Kalem: k)))
            .ToList();

        // Aylık trend her zaman iki tam yılı (önceki/bu yıl) karşılaştırır; seçili tarih
        // aralığı burada uygulanmaz (aksi halde "Bu Ay" seçiliyken grafik neredeyse boş
        // görünürdü), ama kategori filtresi uygulanır.
        var buYil = DateTime.Today.Year;
        var oncekiYil = buYil - 1;
        var turkce = new CultureInfo("tr-TR");

        var satisKalemleriKategoriFiltreli = satisFaturalari
            .SelectMany(f => f.Kalemler.Where(k => KategoriEslesir(k.Malzeme.KategoriId)).Select(k => (Fatura: f, Kalem: k)))
            .ToList();
        var alisKalemleriKategoriFiltreli = alisFaturalari
            .SelectMany(f => f.Kalemler.Where(k => KategoriEslesir(k.Malzeme.KategoriId)).Select(k => (Fatura: f, Kalem: k)))
            .ToList();

        var genelSatisToplami = satisKalemleri.Sum(x => KalemToplami(x.Kalem));
        var genelSatinalmaToplami = alisKalemleri.Sum(x => KalemToplami(x.Kalem));

        // Brüt kar: KDV hariç net satış - satılan malın maliyeti (Malzeme.AlisFiyati'nin
        // fatura onayı anındaki anlık görüntüsü, bkz. SatisFaturasiKalemi.BirimMaliyet).
        var netSatisToplami = satisKalemleri.Sum(x => FinansHesaplama.SatisFaturasiNetTutari(x.Kalem));
        var satilanMalMaliyeti = satisKalemleri.Sum(x => x.Kalem.Miktar * x.Kalem.BirimMaliyet);
        var brutKar = netSatisToplami - satilanMalMaliyeti;

        var kritikStokFiltreli = kritikStok.Where(m => KategoriEslesir(m.KategoriId)).ToList();

        return new DashboardViewModel
        {
            GenelSatisToplamiTRY = genelSatisToplami,
            GenelSatisToplamiUSD = genelSatisToplami / UsdKuru,
            GenelSatinalmaToplamiTRY = genelSatinalmaToplami,
            GenelSatinalmaToplamiUSD = genelSatinalmaToplami / UsdKuru,
            BrutKar = brutKar,

            OncekiYil = oncekiYil,
            BuYil = buYil,

            KategoriBazliSatis = TopKategorilereIndir(
                satisKalemleri
                    .GroupBy(x => x.Kalem.Malzeme.Kategori?.KategoriAdi ?? "Kategorisiz")
                    .Select(g => new KategoriToplamViewModel { KategoriAdi = g.Key, ToplamTutar = g.Sum(x => KalemToplami(x.Kalem)) })
                    .OrderByDescending(k => k.ToplamTutar)
                    .ToList()),

            KategoriBazliAlis = TopKategorilereIndir(
                alisKalemleri
                    .GroupBy(x => x.Kalem.Malzeme.Kategori?.KategoriAdi ?? "Kategorisiz")
                    .Select(g => new KategoriToplamViewModel { KategoriAdi = g.Key, ToplamTutar = g.Sum(x => KalemToplami(x.Kalem)) })
                    .OrderByDescending(k => k.ToplamTutar)
                    .ToList()),

            AylikSatisTrendi = Enumerable.Range(1, 12)
                .Select(ay => new AylikTrendViewModel
                {
                    Ay = ay,
                    AyAdi = new DateTime(buYil, ay, 1).ToString("MMM", turkce),
                    OncekiYilToplam = satisKalemleriKategoriFiltreli
                        .Where(x => x.Fatura.Tarih.Year == oncekiYil && x.Fatura.Tarih.Month == ay)
                        .Sum(x => KalemToplami(x.Kalem)),
                    BuYilToplam = satisKalemleriKategoriFiltreli
                        .Where(x => x.Fatura.Tarih.Year == buYil && x.Fatura.Tarih.Month == ay)
                        .Sum(x => KalemToplami(x.Kalem))
                })
                .ToList(),

            AylikAlisTrendi = Enumerable.Range(1, 12)
                .Select(ay => new AylikTrendViewModel
                {
                    Ay = ay,
                    AyAdi = new DateTime(buYil, ay, 1).ToString("MMM", turkce),
                    OncekiYilToplam = alisKalemleriKategoriFiltreli
                        .Where(x => x.Fatura.Tarih.Year == oncekiYil && x.Fatura.Tarih.Month == ay)
                        .Sum(x => KalemToplami(x.Kalem)),
                    BuYilToplam = alisKalemleriKategoriFiltreli
                        .Where(x => x.Fatura.Tarih.Year == buYil && x.Fatura.Tarih.Month == ay)
                        .Sum(x => KalemToplami(x.Kalem))
                })
                .ToList(),

            EnCokSatisYapilanMusteriler = satisKalemleri
                .GroupBy(x => x.Fatura.Cari.Unvan)
                .Select(g => new MusteriToplamViewModel { CariUnvan = g.Key, ToplamTutar = g.Sum(x => KalemToplami(x.Kalem)) })
                .OrderByDescending(m => m.ToplamTutar)
                .Take(TopN)
                .ToList(),

            EnCokAlisYapilanTedarikciler = alisKalemleri
                .GroupBy(x => x.Fatura.Cari.Unvan)
                .Select(g => new MusteriToplamViewModel { CariUnvan = g.Key, ToplamTutar = g.Sum(x => KalemToplami(x.Kalem)) })
                .OrderByDescending(m => m.ToplamTutar)
                .Take(TopN)
                .ToList(),

            EnCokSatilanMalzemeler = satisKalemleri
                .GroupBy(x => x.Kalem.Malzeme.MalzemeAdi)
                .Select(g => new MalzemeToplamViewModel { MalzemeAdi = g.Key, ToplamTutar = g.Sum(x => KalemToplami(x.Kalem)) })
                .OrderByDescending(m => m.ToplamTutar)
                .Take(TopN)
                .ToList(),

            EnCokAlinanMalzemeler = alisKalemleri
                .GroupBy(x => x.Kalem.Malzeme.MalzemeAdi)
                .Select(g => new MalzemeToplamViewModel { MalzemeAdi = g.Key, ToplamTutar = g.Sum(x => KalemToplami(x.Kalem)) })
                .OrderByDescending(m => m.ToplamTutar)
                .Take(TopN)
                .ToList(),

            KritikStokListesi = kritikStokFiltreli.Select(MalzemeyiKritikStokVmYap).ToList()
        };
    }

    private static (DateTime? Baslangic, DateTime? Bitis) AralikTarihleri(DashboardAralik aralik)
    {
        var bugun = DateTime.Today;
        return aralik switch
        {
            DashboardAralik.BuAy => (new DateTime(bugun.Year, bugun.Month, 1), bugun),
            DashboardAralik.BuYil => (new DateTime(bugun.Year, 1, 1), bugun),
            _ => (null, null)
        };
    }

    private static List<KategoriToplamViewModel> TopKategorilereIndir(List<KategoriToplamViewModel> kategoriToplamlari)
    {
        if (kategoriToplamlari.Count <= MaxKategoriDilimi)
            return kategoriToplamlari;

        var ilkler = kategoriToplamlari.Take(MaxKategoriDilimi - 1).ToList();
        var digerToplami = kategoriToplamlari.Skip(MaxKategoriDilimi - 1).Sum(k => k.ToplamTutar);
        ilkler.Add(new KategoriToplamViewModel { KategoriAdi = "Diğer", ToplamTutar = digerToplami });
        return ilkler;
    }

    private static AnlikStokViewModel MalzemeyiKritikStokVmYap(Malzeme m) => new()
    {
        Id = m.Id,
        MalzemeKodu = m.MalzemeKodu,
        MalzemeAdi = m.MalzemeAdi,
        KategoriAdi = m.Kategori?.KategoriAdi,
        Birim = m.Birim,
        Bakiye = m.Bakiye,
        MinStokMiktari = m.MinStokMiktari,
        MaxStokMiktari = m.MaxStokMiktari,
        DurumText = "Kritik Stok",
        DurumSinifi = "bg-danger"
    };

    private static decimal KalemToplami(SatisFaturasiKalemi k) => FinansHesaplama.SatisFaturasiSatirToplami(k);

    private static decimal KalemToplami(AlisFaturasiKalemi k) => FinansHesaplama.SatirToplami(k);
}
