using Microsoft.EntityFrameworkCore;
using SakaryaERP.Data;
using SakaryaERP.Models;

namespace SakaryaERP.Services;

public class AlisSiparisiService : IAlisSiparisiService
{
    private readonly IUnitOfWork _unitOfWork;

    public AlisSiparisiService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AlisSiparisi> CreateAsync(AlisSiparisi siparis, List<AlisSiparisiKalemi> kalemler)
    {
        if (kalemler.Count == 0)
            throw new InvalidOperationException("Siparişte en az bir kalem olmalıdır.");

        if (kalemler.Any(k => k.Miktar <= 0))
            throw new InvalidOperationException("Tüm kalemlerin miktarı sıfırdan büyük olmalıdır.");

        var cari = await _unitOfWork.Repository<Cari>().GetByIdAsync(siparis.CariId)
            ?? throw new InvalidOperationException("Cari bulunamadı.");
        if (cari.CariTipi != CariTipi.Tedarikci && cari.CariTipi != CariTipi.HerIkisi)
            throw new InvalidOperationException("Alış siparişi sadece tedarikçi olarak işaretli bir cariye açılabilir.");

        var subeVarMi = await _unitOfWork.Repository<Sube>().QueryTumu().AnyAsync(s => s.Id == siparis.SubeId);
        if (!subeVarMi)
            throw new InvalidOperationException("Şube bulunamadı.");

        var kalemMalzemeIdleri = kalemler.Select(k => k.MalzemeId).Distinct().ToList();
        var gecerliMalzemeSayisi = await _unitOfWork.Repository<Malzeme>().QueryTumu()
            .CountAsync(m => kalemMalzemeIdleri.Contains(m.Id));
        if (gecerliMalzemeSayisi != kalemMalzemeIdleri.Count)
            throw new InvalidOperationException("Kalemlerden biri geçersiz bir malzemeye ait.");

        var toplamSayi = await _unitOfWork.Repository<AlisSiparisi>().QueryTumu().CountAsync();
        siparis.SiparisNo = $"AS-{toplamSayi + 1:000000}";
        siparis.Durum = BelgeDurum.Beklemede;
        siparis.Kalemler = kalemler;

        await _unitOfWork.Repository<AlisSiparisi>().AddAsync(siparis);
        await _unitOfWork.SaveChangesAsync();
        return siparis;
    }

    public async Task<(IEnumerable<AlisSiparisi> Kayitlar, int ToplamKayit, int FiltrelenmisKayit)> GetSayfaliListeAsync(
        int start, int length, string? genelArama, string?[] sutunAramalari, int siralamaSutunu, string siralamaYonu)
    {
        var query = _unitOfWork.Repository<AlisSiparisi>().QueryTumu()
            .Include(s => s.Cari)
            .Include(s => s.Sube)
            .Include(s => s.Kalemler)
            .Where(s => !s.IsDeleted);

        var toplamKayit = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(genelArama))
        {
            query = query.Where(s => EF.Functions.ILike(s.Cari.Unvan, $"%{genelArama}%"));
        }

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(1)))
            query = query.Where(s => EF.Functions.ILike(s.Cari.Unvan, $"%{sutunAramalari[1]}%"));

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(2)))
            query = query.Where(s => EF.Functions.ILike(s.Sube.SubeAdi, $"%{sutunAramalari[2]}%"));

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(4))
            && Enum.TryParse<BelgeDurum>(sutunAramalari[4], out var durumFiltre))
            query = query.Where(s => s.Durum == durumFiltre);

        var filtrelenmisKayit = await query.CountAsync();

        var azalan = siralamaYonu == "desc";
        query = siralamaSutunu switch
        {
            1 => azalan ? query.OrderByDescending(s => s.Cari.Unvan) : query.OrderBy(s => s.Cari.Unvan),
            2 => azalan ? query.OrderByDescending(s => s.Sube.SubeAdi) : query.OrderBy(s => s.Sube.SubeAdi),
            4 => azalan ? query.OrderByDescending(s => s.Durum) : query.OrderBy(s => s.Durum),
            _ => azalan ? query.OrderByDescending(s => s.Tarih) : query.OrderBy(s => s.Tarih)
        };

        var kayitlar = await query.Skip(start).Take(length).ToListAsync();
        return (kayitlar, toplamKayit, filtrelenmisKayit);
    }
}
