using System.Globalization;
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

    public async Task<DashboardViewModel> GetDashboardAsync()
    {
        var satisFaturalari = await _satisFaturasiService.GetOnaylanmisListeAsync();
        var alisFaturalari = await _alisFaturasiService.GetOnaylanmisListeAsync();
        var kritikStok = await _malzemeService.GetKritikStokListesiAsync();

        var genelSatisToplami = satisFaturalari.Sum(f => f.Kalemler.Sum(KalemToplami));
        var genelSatinalmaToplami = alisFaturalari.Sum(f => f.Kalemler.Sum(KalemToplami));

        var buYil = DateTime.Today.Year;
        var oncekiYil = buYil - 1;
        var turkce = new CultureInfo("tr-TR");

        var kategoriToplamlari = satisFaturalari
            .SelectMany(f => f.Kalemler)
            .GroupBy(k => k.Malzeme.Kategori?.KategoriAdi ?? "Kategorisiz")
            .Select(g => new KategoriToplamViewModel { KategoriAdi = g.Key, ToplamTutar = g.Sum(KalemToplami) })
            .OrderByDescending(k => k.ToplamTutar)
            .ToList();

        if (kategoriToplamlari.Count > MaxKategoriDilimi)
        {
            var ilkSekiz = kategoriToplamlari.Take(MaxKategoriDilimi - 1).ToList();
            var digerToplami = kategoriToplamlari.Skip(MaxKategoriDilimi - 1).Sum(k => k.ToplamTutar);
            ilkSekiz.Add(new KategoriToplamViewModel { KategoriAdi = "Diğer", ToplamTutar = digerToplami });
            kategoriToplamlari = ilkSekiz;
        }

        return new DashboardViewModel
        {
            GenelSatisToplamiTRY = genelSatisToplami,
            GenelSatisToplamiUSD = genelSatisToplami / UsdKuru,
            GenelSatinalmaToplamiTRY = genelSatinalmaToplami,
            GenelSatinalmaToplamiUSD = genelSatinalmaToplami / UsdKuru,

            OncekiYil = oncekiYil,
            BuYil = buYil,

            KategoriToplamlari = kategoriToplamlari,

            AylikTrend = Enumerable.Range(1, 12)
                .Select(ay => new AylikTrendViewModel
                {
                    Ay = ay,
                    AyAdi = new DateTime(buYil, ay, 1).ToString("MMM", turkce),
                    OncekiYilToplam = satisFaturalari
                        .Where(f => f.Tarih.Year == oncekiYil && f.Tarih.Month == ay)
                        .Sum(f => f.Kalemler.Sum(KalemToplami)),
                    BuYilToplam = satisFaturalari
                        .Where(f => f.Tarih.Year == buYil && f.Tarih.Month == ay)
                        .Sum(f => f.Kalemler.Sum(KalemToplami))
                })
                .ToList(),

            EnCokSatisYapilanMusteriler = satisFaturalari
                .GroupBy(f => f.Cari.Unvan)
                .Select(g => new MusteriToplamViewModel { CariUnvan = g.Key, ToplamTutar = g.Sum(f => f.Kalemler.Sum(KalemToplami)) })
                .OrderByDescending(m => m.ToplamTutar)
                .Take(TopN)
                .ToList(),

            EnCokSatilanMalzemeler = satisFaturalari
                .SelectMany(f => f.Kalemler)
                .GroupBy(k => k.Malzeme.MalzemeAdi)
                .Select(g => new MalzemeToplamViewModel { MalzemeAdi = g.Key, ToplamTutar = g.Sum(KalemToplami) })
                .OrderByDescending(m => m.ToplamTutar)
                .Take(TopN)
                .ToList(),

            KritikStokListesi = kritikStok.Select(MalzemeyiKritikStokVmYap).ToList()
        };
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

    private static decimal KalemToplami(SatisFaturasiKalemi k)
    {
        var araToplam = k.Miktar * k.BirimFiyat;
        var iskontolu = araToplam * (1 - k.Iskonto / 100);
        return iskontolu * (1 + k.KdvOrani / 100);
    }

    private static decimal KalemToplami(AlisFaturasiKalemi k)
    {
        var araToplam = k.Miktar * k.BirimFiyat;
        var iskontolu = araToplam * (1 - k.Iskonto / 100);
        return iskontolu * (1 + k.KdvOrani / 100);
    }
}
