using Microsoft.EntityFrameworkCore;
using SakaryaERP.Data;
using SakaryaERP.Models;

namespace SakaryaERP.Services;

public class SatisSiparisiService : ISatisSiparisiService
{
    private readonly IUnitOfWork _unitOfWork;

    public SatisSiparisiService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SatisSiparisi> CreateAsync(SatisSiparisi siparis, List<SatisSiparisiKalemi> kalemler)
    {
        if (kalemler.Count == 0)
            throw new InvalidOperationException("Siparişte en az bir kalem olmalıdır.");

        if (kalemler.Any(k => k.Miktar <= 0))
            throw new InvalidOperationException("Tüm kalemlerin miktarı sıfırdan büyük olmalıdır.");

        var cari = await _unitOfWork.Repository<Cari>().GetByIdAsync(siparis.CariId)
            ?? throw new InvalidOperationException("Cari bulunamadı.");
        if (cari.CariTipi != CariTipi.Musteri && cari.CariTipi != CariTipi.HerIkisi)
            throw new InvalidOperationException("Satış siparişi sadece müşteri olarak işaretli bir cariye açılabilir.");

        var kalemMalzemeIdleri = kalemler.Select(k => k.MalzemeId).Distinct().ToList();
        var gecerliMalzemeSayisi = await _unitOfWork.Repository<Malzeme>().QueryTumu()
            .CountAsync(m => kalemMalzemeIdleri.Contains(m.Id));
        if (gecerliMalzemeSayisi != kalemMalzemeIdleri.Count)
            throw new InvalidOperationException("Kalemlerden biri geçersiz bir malzemeye ait.");

        if (siparis.SatisTeklifiId is not null)
        {
            var teklif = await _unitOfWork.Repository<SatisTeklifi>().GetByIdAsync(siparis.SatisTeklifiId.Value)
                ?? throw new InvalidOperationException("Satış teklifi bulunamadı.");
            if (teklif.Durum != BelgeDurum.Onaylandi)
                throw new InvalidOperationException("Sipariş sadece onaylanmış bir teklifden oluşturulabilir.");
            if (teklif.GecerlilikTarihi is not null && teklif.GecerlilikTarihi.Value.Date < DateTime.Today)
                throw new InvalidOperationException(
                    $"Teklifin geçerlilik süresi {teklif.GecerlilikTarihi.Value:dd.MM.yyyy} tarihinde dolmuş, siparişe dönüştürülemez.");
        }

        var toplamSayi = await _unitOfWork.Repository<SatisSiparisi>().QueryTumu().CountAsync();
        siparis.SiparisNo = $"SS-{toplamSayi + 1:000000}";
        siparis.Durum = BelgeDurum.Beklemede;
        siparis.Kalemler = kalemler;

        await _unitOfWork.Repository<SatisSiparisi>().AddAsync(siparis);
        await _unitOfWork.SaveChangesAsync();
        return siparis;
    }

    public async Task<(IEnumerable<SatisSiparisi> Kayitlar, int ToplamKayit, int FiltrelenmisKayit)> GetSayfaliListeAsync(
        int start, int length, string? genelArama, string?[] sutunAramalari, int siralamaSutunu, string siralamaYonu)
    {
        var query = _unitOfWork.Repository<SatisSiparisi>().QueryTumu()
            .Include(s => s.Cari)
            .Include(s => s.SatisTeklifi)
            .Include(s => s.Kalemler)
            .Include(s => s.SevkIrsaliyeleri).ThenInclude(i => i.Kalemler)
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

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(4))
            && Enum.TryParse<BelgeDurum>(sutunAramalari[4], out var durumFiltre))
            query = query.Where(s => s.Durum == durumFiltre);

        var filtrelenmisKayit = await query.CountAsync();

        var azalan = siralamaYonu == "desc";
        query = siralamaSutunu switch
        {
            0 => azalan ? query.OrderByDescending(s => s.SiparisNo) : query.OrderBy(s => s.SiparisNo),
            2 => azalan ? query.OrderByDescending(s => s.Cari.Unvan) : query.OrderBy(s => s.Cari.Unvan),
            4 => azalan ? query.OrderByDescending(s => s.Durum) : query.OrderBy(s => s.Durum),
            _ => azalan ? query.OrderByDescending(s => s.Tarih) : query.OrderBy(s => s.Tarih)
        };

        var kayitlar = await query.Skip(start).Take(length).ToListAsync();
        return (kayitlar, toplamKayit, filtrelenmisKayit);
    }

    public async Task<SatisSiparisi?> GetByIdDetayAsync(int id)
    {
        return await _unitOfWork.Repository<SatisSiparisi>().QueryTumu()
            .Include(s => s.Cari)
            .Include(s => s.SatisTeklifi)
            .Include(s => s.Kalemler).ThenInclude(k => k.Malzeme)
            .Include(s => s.SevkIrsaliyeleri).ThenInclude(i => i.Kalemler)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task OnaylaAsync(int id)
    {
        var siparis = await _unitOfWork.Repository<SatisSiparisi>().GetByIdAsync(id)
            ?? throw new InvalidOperationException("Satış siparişi bulunamadı.");
        if (siparis.Durum != BelgeDurum.Beklemede)
            throw new InvalidOperationException("Sadece beklemede olan siparişler onaylanabilir.");

        siparis.Durum = BelgeDurum.Onaylandi;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task IptalEtAsync(int id)
    {
        var siparis = await _unitOfWork.Repository<SatisSiparisi>().GetByIdAsync(id)
            ?? throw new InvalidOperationException("Satış siparişi bulunamadı.");
        if (siparis.Durum != BelgeDurum.Beklemede)
            throw new InvalidOperationException("Sadece beklemede olan siparişler iptal edilebilir.");

        siparis.Durum = BelgeDurum.Iptal;
        await _unitOfWork.SaveChangesAsync();
    }

    public Dictionary<int, decimal> SevkMiktarlariHesapla(SatisSiparisi siparis)
    {
        return siparis.SevkIrsaliyeleri
            .Where(i => i.Durum == BelgeDurum.Onaylandi)
            .SelectMany(i => i.Kalemler)
            .GroupBy(k => k.MalzemeId)
            .ToDictionary(g => g.Key, g => g.Sum(k => k.Miktar));
    }
}
