using Microsoft.EntityFrameworkCore;
using SakaryaERP.Data;
using SakaryaERP.Models;

namespace SakaryaERP.Services;

public class CekSenetService : ICekSenetService
{
    private readonly IUnitOfWork _unitOfWork;

    public CekSenetService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<CekSenet>> GetAllAsync()
        => await _unitOfWork.Repository<CekSenet>().QueryTumu().Include(c => c.Cari).ToListAsync();

    public async Task<CekSenet?> GetByIdAsync(int id)
        => await _unitOfWork.Repository<CekSenet>().GetByIdAsync(id);

    public async Task<CekSenet> CreateAsync(CekSenet cekSenet)
    {
        if (!await _unitOfWork.Repository<Cari>().QueryTumu().AnyAsync(c => c.Id == cekSenet.CariId))
            throw new InvalidOperationException("Cari bulunamadı.");

        cekSenet.Durum = CekSenetDurum.Portfoyde;
        await _unitOfWork.Repository<CekSenet>().AddAsync(cekSenet);
        await _unitOfWork.SaveChangesAsync();
        return cekSenet;
    }

    public async Task UpdateAsync(CekSenet cekSenet)
    {
        var mevcut = await _unitOfWork.Repository<CekSenet>().GetByIdAsync(cekSenet.Id)
            ?? throw new InvalidOperationException("Çek/Senet kaydı bulunamadı.");

        mevcut.BelgeTipi = cekSenet.BelgeTipi;
        mevcut.BelgeNo = cekSenet.BelgeNo;
        mevcut.CariId = cekSenet.CariId;
        mevcut.CiroBilgisi = cekSenet.CiroBilgisi;
        mevcut.VadeTarihi = cekSenet.VadeTarihi;
        mevcut.Tutar = cekSenet.Tutar;
        mevcut.BankaAdi = cekSenet.BankaAdi;
        mevcut.SubeAdi = cekSenet.SubeAdi;

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<(IEnumerable<CekSenet> Kayitlar, int ToplamKayit, int FiltrelenmisKayit)> GetSayfaliListeAsync(
        int start, int length, string? genelArama, string?[] sutunAramalari, int siralamaSutunu, string siralamaYonu)
    {
        var query = _unitOfWork.Repository<CekSenet>().QueryTumu()
            .Include(c => c.Cari)
            .Where(c => !c.IsDeleted);

        var toplamKayit = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(genelArama))
        {
            query = query.Where(c =>
                EF.Functions.ILike(c.BelgeNo, $"%{genelArama}%") ||
                EF.Functions.ILike(c.Cari.Unvan, $"%{genelArama}%") ||
                (c.BankaAdi != null && EF.Functions.ILike(c.BankaAdi, $"%{genelArama}%")));
        }

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(0))
            && Enum.TryParse<BelgeTipi>(sutunAramalari[0], out var belgeTipiFiltre))
            query = query.Where(c => c.BelgeTipi == belgeTipiFiltre);

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(1)))
            query = query.Where(c => EF.Functions.ILike(c.BelgeNo, $"%{sutunAramalari[1]}%"));

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(2)))
            query = query.Where(c => EF.Functions.ILike(c.Cari.Unvan, $"%{sutunAramalari[2]}%"));

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(5)))
            query = query.Where(c => c.BankaAdi != null && EF.Functions.ILike(c.BankaAdi, $"%{sutunAramalari[5]}%"));

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(6)))
            query = query.Where(c => c.SubeAdi != null && EF.Functions.ILike(c.SubeAdi, $"%{sutunAramalari[6]}%"));

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(7))
            && Enum.TryParse<CekSenetDurum>(sutunAramalari[7], out var durumFiltre))
            query = query.Where(c => c.Durum == durumFiltre);

        var filtrelenmisKayit = await query.CountAsync();

        var azalan = siralamaYonu == "desc";
        query = siralamaSutunu switch
        {
            0 => azalan ? query.OrderByDescending(c => c.BelgeTipi) : query.OrderBy(c => c.BelgeTipi),
            1 => azalan ? query.OrderByDescending(c => c.BelgeNo) : query.OrderBy(c => c.BelgeNo),
            2 => azalan ? query.OrderByDescending(c => c.Cari.Unvan) : query.OrderBy(c => c.Cari.Unvan),
            4 => azalan ? query.OrderByDescending(c => c.Tutar) : query.OrderBy(c => c.Tutar),
            7 => azalan ? query.OrderByDescending(c => c.Durum) : query.OrderBy(c => c.Durum),
            _ => azalan ? query.OrderByDescending(c => c.VadeTarihi) : query.OrderBy(c => c.VadeTarihi)
        };

        var kayitlar = await query.Skip(start).Take(length).ToListAsync();
        return (kayitlar, toplamKayit, filtrelenmisKayit);
    }
}
