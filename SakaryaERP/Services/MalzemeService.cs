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
