using Microsoft.EntityFrameworkCore;
using SakaryaERP.Data;
using SakaryaERP.Models;

namespace SakaryaERP.Services;

public class AlisSiparisiService : IAlisSiparisiService
{
    private readonly IUnitOfWork _unitOfWork;

    private readonly IOnayYetkisiService _onayYetkisiService;

    public AlisSiparisiService(IUnitOfWork unitOfWork, IOnayYetkisiService onayYetkisiService)
    {
        _unitOfWork = unitOfWork;
        _onayYetkisiService = onayYetkisiService;
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
            .Include(s => s.AlisIrsaliyeleri).ThenInclude(i => i.Kalemler)
            .Where(s => !s.IsDeleted);

        var toplamKayit = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(genelArama))
        {
            query = query.Where(s => EF.Functions.ILike(s.Cari.Unvan, $"%{genelArama}%"));
        }

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(0)))
            query = query.Where(s => EF.Functions.ILike(s.SiparisNo, $"%{sutunAramalari[0]}%"));

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(2)))
            query = query.Where(s => EF.Functions.ILike(s.Cari.Unvan, $"%{sutunAramalari[2]}%"));

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(3)))
            query = query.Where(s => EF.Functions.ILike(s.Sube.SubeAdi, $"%{sutunAramalari[3]}%"));

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(4))
            && Enum.TryParse<BelgeDurum>(sutunAramalari[4], out var durumFiltre))
            query = query.Where(s => s.Durum == durumFiltre);

        var filtrelenmisKayit = await query.CountAsync();

        var azalan = siralamaYonu == "desc";
        query = siralamaSutunu switch
        {
            2 => azalan ? query.OrderByDescending(s => s.Cari.Unvan) : query.OrderBy(s => s.Cari.Unvan),
            3 => azalan ? query.OrderByDescending(s => s.Sube.SubeAdi) : query.OrderBy(s => s.Sube.SubeAdi),
            4 => azalan ? query.OrderByDescending(s => s.Durum) : query.OrderBy(s => s.Durum),
            _ => azalan ? query.OrderByDescending(s => s.Tarih) : query.OrderBy(s => s.Tarih)
        };

        var kayitlar = await query.Skip(start).Take(length).ToListAsync();
        return (kayitlar, toplamKayit, filtrelenmisKayit);
    }

    public async Task<AlisSiparisi?> GetByIdDetayAsync(int id)
    {
        return await _unitOfWork.Repository<AlisSiparisi>().QueryTumu()
            .Include(s => s.Cari)
            .Include(s => s.Sube)
            .Include(s => s.Kalemler).ThenInclude(k => k.Malzeme)
            .Include(s => s.AlisIrsaliyeleri).ThenInclude(i => i.Kalemler)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<List<AlisSiparisi>> GetBeklemedeListesiAsync()
    {
        return await _unitOfWork.Repository<AlisSiparisi>().QueryTumu()
            .Include(s => s.Cari)
            .Where(s => s.Durum == BelgeDurum.Beklemede)
            .ToListAsync();
    }

    public async Task OnaylaAsync(int id)
    {
        var siparis = await _unitOfWork.Repository<AlisSiparisi>().GetByIdAsync(id)
            ?? throw new InvalidOperationException("Alış siparişi bulunamadı.");
        if (siparis.Durum != BelgeDurum.Beklemede)
            throw new InvalidOperationException("Sadece beklemede olan siparişler onaylanabilir.");

        _onayYetkisiService.OlusturanOnaylayamazKontrolEt(siparis.CreatedBy, "Alış Siparişi");

        siparis.Durum = BelgeDurum.Onaylandi;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task IptalEtAsync(int id)
    {
        var siparis = await _unitOfWork.Repository<AlisSiparisi>().GetByIdAsync(id)
            ?? throw new InvalidOperationException("Alış siparişi bulunamadı.");
        if (siparis.Durum != BelgeDurum.Beklemede)
            throw new InvalidOperationException("Sadece beklemede olan siparişler iptal edilebilir.");

        siparis.Durum = BelgeDurum.Iptal;
        await _unitOfWork.SaveChangesAsync();
    }

    public Dictionary<int, decimal> TeslimMiktarlariHesapla(AlisSiparisi siparis)
    {
        return siparis.AlisIrsaliyeleri
            .Where(i => i.Durum == BelgeDurum.Onaylandi)
            .SelectMany(i => i.Kalemler)
            .GroupBy(k => k.MalzemeId)
            .ToDictionary(g => g.Key, g => g.Sum(k => k.Miktar));
    }
}
