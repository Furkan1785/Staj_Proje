using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SakaryaERP.Data;
using SakaryaERP.Models;

namespace SakaryaERP.Services;

public class MalzemeService : IMalzemeService
{
    private readonly IUnitOfWork _unitOfWork;

    public MalzemeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Malzeme> CreateAsync(Malzeme malzeme)
    {
        var koduKullanimda = await _unitOfWork.Repository<Malzeme>().QueryTumu()
            .AnyAsync(m => m.MalzemeKodu == malzeme.MalzemeKodu);
        if (koduKullanimda)
            throw new InvalidOperationException("Bu malzeme kodu zaten kullanımda.");

        await _unitOfWork.Repository<Malzeme>().AddAsync(malzeme);
        await _unitOfWork.SaveChangesAsync();
        return malzeme;
    }

    public async Task<Malzeme?> GetByIdAsync(int id)
        => await _unitOfWork.Repository<Malzeme>().QueryTumu()
            .Include(m => m.Kategori)
            .FirstOrDefaultAsync(m => m.Id == id);

    public async Task<IEnumerable<Malzeme>> GetKritikStokListesiAsync()
        => await _unitOfWork.Repository<Malzeme>().QueryTumu()
            .Include(m => m.Kategori)
            .Where(m => !m.IsDeleted && m.Bakiye < m.MinStokMiktari)
            .OrderBy(m => m.MalzemeKodu)
            .ToListAsync();

    public async Task UpdateAsync(Malzeme malzeme)
    {
        var mevcut = await _unitOfWork.Repository<Malzeme>().GetByIdAsync(malzeme.Id)
            ?? throw new InvalidOperationException("Malzeme bulunamadı.");

        var koduBaskaKayittaKullanimda = await _unitOfWork.Repository<Malzeme>().QueryTumu()
            .AnyAsync(m => m.MalzemeKodu == malzeme.MalzemeKodu && m.Id != malzeme.Id);
        if (koduBaskaKayittaKullanimda)
            throw new InvalidOperationException("Bu malzeme kodu zaten kullanımda.");

        mevcut.MalzemeKodu = malzeme.MalzemeKodu;
        mevcut.Barkod = malzeme.Barkod;
        mevcut.MalzemeAdi = malzeme.MalzemeAdi;
        mevcut.Marka = malzeme.Marka;
        mevcut.Kalite = malzeme.Kalite;
        mevcut.Tip = malzeme.Tip;
        mevcut.Birim = malzeme.Birim;
        mevcut.KategoriId = malzeme.KategoriId;
        mevcut.TeminTuru = malzeme.TeminTuru;
        mevcut.StokTipi = malzeme.StokTipi;
        mevcut.AlisFiyati = malzeme.AlisFiyati;
        mevcut.SatisFiyati = malzeme.SatisFiyati;
        mevcut.KdvOrani = malzeme.KdvOrani;
        mevcut.MinStokMiktari = malzeme.MinStokMiktari;
        mevcut.MaxStokMiktari = malzeme.MaxStokMiktari;
        mevcut.RafNo = malzeme.RafNo;

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<Malzeme>> GetTumListeAsync()
        => await _unitOfWork.Repository<Malzeme>().QueryTumu()
            .Include(m => m.Kategori)
            .Where(m => !m.IsDeleted)
            .OrderBy(m => m.MalzemeKodu)
            .ToListAsync();

    public async Task<MalzemeIceAktarSonucu> TopluIceAktarAsync(List<MalzemeImportSatiri> satirlar)
    {
        var sonuc = new MalzemeIceAktarSonucu();
        var kategoriler = (await _unitOfWork.Repository<MalzemeKategori>().GetAllAsync()).ToList();
        var mevcutKodlar = (await _unitOfWork.Repository<Malzeme>().QueryTumu()
            .Select(m => m.MalzemeKodu).ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var satir in satirlar)
        {
            var hata = SatiriDogrulaVeOlustur(satir, kategoriler, mevcutKodlar, out var malzeme);
            if (hata is not null)
            {
                sonuc.Hatalar.Add($"Satır {satir.SatirNo}: {hata}");
                continue;
            }

            await _unitOfWork.Repository<Malzeme>().AddAsync(malzeme!);
            mevcutKodlar.Add(malzeme!.MalzemeKodu);
            sonuc.BasariliSayisi++;
        }

        if (sonuc.BasariliSayisi > 0)
            await _unitOfWork.SaveChangesAsync();

        return sonuc;
    }

    private static string? SatiriDogrulaVeOlustur(
        MalzemeImportSatiri satir, List<MalzemeKategori> kategoriler, HashSet<string> mevcutKodlar, out Malzeme? malzeme)
    {
        malzeme = null;

        if (string.IsNullOrWhiteSpace(satir.MalzemeKodu))
            return "Malzeme kodu zorunludur.";
        if (mevcutKodlar.Contains(satir.MalzemeKodu.Trim()))
            return $"Bu malzeme kodu zaten kullanımda: {satir.MalzemeKodu}";

        if (string.IsNullOrWhiteSpace(satir.MalzemeAdi))
            return "Malzeme adı zorunludur.";

        if (string.IsNullOrWhiteSpace(satir.Birim))
            return "Birim zorunludur.";

        int? kategoriId = null;
        if (!string.IsNullOrWhiteSpace(satir.KategoriAdi))
        {
            var kategori = kategoriler.FirstOrDefault(k =>
                string.Equals(k.KategoriAdi, satir.KategoriAdi.Trim(), StringComparison.OrdinalIgnoreCase));
            if (kategori is null)
                return $"Kategori bulunamadı: {satir.KategoriAdi}";
            kategoriId = kategori.Id;
        }

        if (!EnumDisplayMetniniCoz<TeminTuru>(satir.TeminTuru, TeminTuru.Alis, out var teminTuru))
            return $"Geçersiz temin türü: {satir.TeminTuru}";

        if (!EnumDisplayMetniniCoz<StokTipi>(satir.StokTipi, StokTipi.TicariMal, out var stokTipi))
            return $"Geçersiz stok tipi: {satir.StokTipi}";

        if (!OndalikDeneParse(satir.AlisFiyati, out var alisFiyati))
            return $"Geçersiz sayı (Alış Fiyatı): {satir.AlisFiyati}";
        if (!OndalikDeneParse(satir.SatisFiyati, out var satisFiyati))
            return $"Geçersiz sayı (Satış Fiyatı): {satir.SatisFiyati}";
        if (!OndalikDeneParse(satir.KdvOrani, out var kdvOrani))
            return $"Geçersiz sayı (KDV Oranı): {satir.KdvOrani}";
        if (!OndalikDeneParse(satir.MinStokMiktari, out var minStok))
            return $"Geçersiz sayı (Min Stok): {satir.MinStokMiktari}";
        if (!OndalikDeneParse(satir.MaxStokMiktari, out var maxStok))
            return $"Geçersiz sayı (Max Stok): {satir.MaxStokMiktari}";

        malzeme = new Malzeme
        {
            MalzemeKodu = satir.MalzemeKodu.Trim(),
            Barkod = string.IsNullOrWhiteSpace(satir.Barkod) ? null : satir.Barkod.Trim(),
            MalzemeAdi = satir.MalzemeAdi.Trim(),
            Marka = string.IsNullOrWhiteSpace(satir.Marka) ? null : satir.Marka.Trim(),
            Kalite = string.IsNullOrWhiteSpace(satir.Kalite) ? null : satir.Kalite.Trim(),
            Tip = string.IsNullOrWhiteSpace(satir.Tip) ? null : satir.Tip.Trim(),
            Birim = satir.Birim.Trim(),
            KategoriId = kategoriId,
            TeminTuru = teminTuru,
            StokTipi = stokTipi,
            AlisFiyati = alisFiyati,
            SatisFiyati = satisFiyati,
            KdvOrani = kdvOrani,
            MinStokMiktari = minStok,
            MaxStokMiktari = maxStok,
            RafNo = string.IsNullOrWhiteSpace(satir.RafNo) ? null : satir.RafNo.Trim()
        };
        return null;
    }

    private static bool OndalikDeneParse(string? metin, out decimal sonuc)
    {
        sonuc = 0;
        if (string.IsNullOrWhiteSpace(metin)) return true;
        return decimal.TryParse(metin, NumberStyles.Any, CultureInfo.InvariantCulture, out sonuc)
            || decimal.TryParse(metin, NumberStyles.Any, new CultureInfo("tr-TR"), out sonuc);
    }

    // Enum'daki [Display(Name=...)] metnini geri çözer (ör. "Alış+Üretim" -> TeminTuru.AlisUretim).
    // Bu sayede Excel şablonundaki başlıklarla ekranda gösterilen metinler tek bir yerden (enum'ın
    // kendi Display attribute'u) türetilmiş olur, iki ayrı metin listesi birbirinden kopmaz.
    private static bool EnumDisplayMetniniCoz<TEnum>(string? metin, TEnum varsayilan, out TEnum sonuc) where TEnum : struct, Enum
    {
        sonuc = varsayilan;
        if (string.IsNullOrWhiteSpace(metin))
            return true;

        foreach (var deger in Enum.GetValues<TEnum>())
        {
            var alan = typeof(TEnum).GetField(deger.ToString());
            var gorunenAd = alan?.GetCustomAttribute<DisplayAttribute>()?.Name;
            if (string.Equals(gorunenAd, metin.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                sonuc = deger;
                return true;
            }
        }
        return false;
    }

    public async Task<(IEnumerable<Malzeme> Kayitlar, int ToplamKayit, int FiltrelenmisKayit)> GetSayfaliListeAsync(
        int start, int length, string? genelArama, string?[] sutunAramalari, int siralamaSutunu, string siralamaYonu)
    {
        var query = _unitOfWork.Repository<Malzeme>().QueryTumu()
            .Include(m => m.Kategori)
            .Where(m => !m.IsDeleted);

        var toplamKayit = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(genelArama))
        {
            query = query.Where(m =>
                EF.Functions.ILike(m.MalzemeKodu, $"%{genelArama}%") ||
                EF.Functions.ILike(m.MalzemeAdi, $"%{genelArama}%") ||
                (m.Barkod != null && EF.Functions.ILike(m.Barkod, $"%{genelArama}%")));
        }

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(0)))
            query = query.Where(m => EF.Functions.ILike(m.MalzemeKodu, $"%{sutunAramalari[0]}%"));

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(1)))
            query = query.Where(m => m.Barkod != null && EF.Functions.ILike(m.Barkod, $"%{sutunAramalari[1]}%"));

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(2)))
            query = query.Where(m => EF.Functions.ILike(m.MalzemeAdi, $"%{sutunAramalari[2]}%"));

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(3)))
            query = query.Where(m => m.Marka != null && EF.Functions.ILike(m.Marka, $"%{sutunAramalari[3]}%"));

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(4)))
            query = query.Where(m => m.Kalite != null && EF.Functions.ILike(m.Kalite, $"%{sutunAramalari[4]}%"));

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(5)))
            query = query.Where(m => m.Tip != null && EF.Functions.ILike(m.Tip, $"%{sutunAramalari[5]}%"));

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(7)))
            query = query.Where(m => m.Kategori != null && EF.Functions.ILike(m.Kategori.KategoriAdi, $"%{sutunAramalari[7]}%"));

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(8))
            && Enum.TryParse<TeminTuru>(sutunAramalari[8], out var teminTuruFiltre))
            query = query.Where(m => m.TeminTuru == teminTuruFiltre);

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(9))
            && Enum.TryParse<StokTipi>(sutunAramalari[9], out var stokTipiFiltre))
            query = query.Where(m => m.StokTipi == stokTipiFiltre);

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(15)))
            query = query.Where(m => m.RafNo != null && EF.Functions.ILike(m.RafNo, $"%{sutunAramalari[15]}%"));

        var filtrelenmisKayit = await query.CountAsync();

        var azalan = siralamaYonu == "desc";
        query = siralamaSutunu switch
        {
            2 => azalan ? query.OrderByDescending(m => m.MalzemeAdi) : query.OrderBy(m => m.MalzemeAdi),
            3 => azalan ? query.OrderByDescending(m => m.Marka) : query.OrderBy(m => m.Marka),
            7 => azalan ? query.OrderByDescending(m => m.Kategori!.KategoriAdi) : query.OrderBy(m => m.Kategori!.KategoriAdi),
            8 => azalan ? query.OrderByDescending(m => m.TeminTuru) : query.OrderBy(m => m.TeminTuru),
            9 => azalan ? query.OrderByDescending(m => m.StokTipi) : query.OrderBy(m => m.StokTipi),
            10 => azalan ? query.OrderByDescending(m => m.AlisFiyati) : query.OrderBy(m => m.AlisFiyati),
            11 => azalan ? query.OrderByDescending(m => m.SatisFiyati) : query.OrderBy(m => m.SatisFiyati),
            13 => azalan ? query.OrderByDescending(m => m.MinStokMiktari) : query.OrderBy(m => m.MinStokMiktari),
            14 => azalan ? query.OrderByDescending(m => m.MaxStokMiktari) : query.OrderBy(m => m.MaxStokMiktari),
            16 => azalan ? query.OrderByDescending(m => m.Bakiye) : query.OrderBy(m => m.Bakiye),
            _ => azalan ? query.OrderByDescending(m => m.MalzemeKodu) : query.OrderBy(m => m.MalzemeKodu)
        };

        var kayitlar = await query.Skip(start).Take(length).ToListAsync();
        return (kayitlar, toplamKayit, filtrelenmisKayit);
    }
}
