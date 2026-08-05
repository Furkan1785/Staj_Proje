using Microsoft.EntityFrameworkCore;
using SakaryaERP.Data;
using SakaryaERP.Models;

namespace SakaryaERP.Services;

public class SevkIrsaliyesiService : ISevkIrsaliyesiService
{
    private readonly IUnitOfWork _unitOfWork;

    private readonly IOnayYetkisiService _onayYetkisiService;

    public SevkIrsaliyesiService(IUnitOfWork unitOfWork, IOnayYetkisiService onayYetkisiService)
    {
        _unitOfWork = unitOfWork;
        _onayYetkisiService = onayYetkisiService;
    }

    public async Task<SevkIrsaliyesi> CreateAsync(SevkIrsaliyesi irsaliye, List<SevkIrsaliyesiKalemi> kalemler)
    {
        if (kalemler.Count == 0)
            throw new InvalidOperationException("Sevk irsaliyesinde en az bir kalem olmalıdır.");

        if (kalemler.Any(k => k.Miktar <= 0))
            throw new InvalidOperationException("Tüm kalemlerin miktarı sıfırdan büyük olmalıdır.");

        var siparis = await _unitOfWork.Repository<SatisSiparisi>().QueryTumu()
            .Include(s => s.Kalemler)
            .Include(s => s.SevkIrsaliyeleri).ThenInclude(i => i.Kalemler)
            .FirstOrDefaultAsync(s => s.Id == irsaliye.SatisSiparisiId)
            ?? throw new InvalidOperationException("Satış siparişi bulunamadı.");

        if (siparis.Durum != BelgeDurum.Onaylandi)
            throw new InvalidOperationException("Sevk irsaliyesi sadece onaylanmış bir siparişten oluşturulabilir.");

        var subeVarMi = await _unitOfWork.Repository<Sube>().QueryTumu().AnyAsync(s => s.Id == irsaliye.SubeId);
        if (!subeVarMi)
            throw new InvalidOperationException("Şube bulunamadı.");

        var kalemMalzemeIdleri = kalemler.Select(k => k.MalzemeId).Distinct().ToList();
        var malzemeler = await _unitOfWork.Repository<Malzeme>().QueryTumu()
            .Where(m => kalemMalzemeIdleri.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id);
        if (malzemeler.Count != kalemMalzemeIdleri.Count)
            throw new InvalidOperationException("Kalemlerden biri geçersiz bir malzemeye ait.");

        var siparisKalemMiktarlari = siparis.Kalemler.ToDictionary(k => k.MalzemeId, k => k.Miktar);
        if (kalemler.Any(k => !siparisKalemMiktarlari.ContainsKey(k.MalzemeId)))
            throw new InvalidOperationException("Sevk irsaliyesi kalemleri seçilen siparişte olmayan bir malzeme içeremez.");

        // Kısmi sevkiyat: aynı siparişten daha önce (iptal hariç) sevk edilen miktarlar düşülüp
        // kalan sevk edilebilir miktar bulunur — aksi halde sipariş miktarının üzerinde sevkiyat mümkün olurdu.
        var sevkEdilenMiktarlar = siparis.SevkIrsaliyeleri
            .Where(i => i.Durum != BelgeDurum.Iptal)
            .SelectMany(i => i.Kalemler)
            .GroupBy(k => k.MalzemeId)
            .ToDictionary(g => g.Key, g => g.Sum(k => k.Miktar));

        foreach (var kalem in kalemler)
        {
            var kalanMiktar = siparisKalemMiktarlari[kalem.MalzemeId] - sevkEdilenMiktarlar.GetValueOrDefault(kalem.MalzemeId);
            if (kalem.Miktar > kalanMiktar)
                throw new InvalidOperationException(
                    $"{malzemeler[kalem.MalzemeId].MalzemeKodu} için sevk edilebilecek miktar sipariş miktarını aşıyor (kalan: {kalanMiktar}).");
        }

        var toplamSayi = await _unitOfWork.Repository<SevkIrsaliyesi>().QueryTumu().CountAsync();
        irsaliye.IrsaliyeNo = $"SI-{toplamSayi + 1:000000}";
        irsaliye.Durum = BelgeDurum.Beklemede;
        irsaliye.Kalemler = kalemler;

        await _unitOfWork.Repository<SevkIrsaliyesi>().AddAsync(irsaliye);
        await _unitOfWork.SaveChangesAsync();
        return irsaliye;
    }

    public async Task<(IEnumerable<SevkIrsaliyesi> Kayitlar, int ToplamKayit, int FiltrelenmisKayit)> GetSayfaliListeAsync(
        int start, int length, string? genelArama, string?[] sutunAramalari, int siralamaSutunu, string siralamaYonu)
    {
        var query = _unitOfWork.Repository<SevkIrsaliyesi>().QueryTumu()
            .Include(i => i.SatisSiparisi).ThenInclude(s => s.Cari)
            .Include(i => i.Sube)
            .Include(i => i.Kalemler)
            .Where(i => !i.IsDeleted);

        var toplamKayit = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(genelArama))
        {
            query = query.Where(i => EF.Functions.ILike(i.SatisSiparisi.Cari.Unvan, $"%{genelArama}%"));
        }

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(0)))
            query = query.Where(i => EF.Functions.ILike(i.IrsaliyeNo, $"%{sutunAramalari[0]}%"));

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(2)))
            query = query.Where(i => EF.Functions.ILike(i.SatisSiparisi.Cari.Unvan, $"%{sutunAramalari[2]}%"));

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(4)))
            query = query.Where(i => EF.Functions.ILike(i.Sube.SubeAdi, $"%{sutunAramalari[4]}%"));

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(5))
            && Enum.TryParse<BelgeDurum>(sutunAramalari[5], out var durumFiltre))
            query = query.Where(i => i.Durum == durumFiltre);

        var filtrelenmisKayit = await query.CountAsync();

        var azalan = siralamaYonu == "desc";
        query = siralamaSutunu switch
        {
            0 => azalan ? query.OrderByDescending(i => i.IrsaliyeNo) : query.OrderBy(i => i.IrsaliyeNo),
            2 => azalan ? query.OrderByDescending(i => i.SatisSiparisi.Cari.Unvan) : query.OrderBy(i => i.SatisSiparisi.Cari.Unvan),
            4 => azalan ? query.OrderByDescending(i => i.Sube.SubeAdi) : query.OrderBy(i => i.Sube.SubeAdi),
            5 => azalan ? query.OrderByDescending(i => i.Durum) : query.OrderBy(i => i.Durum),
            _ => azalan ? query.OrderByDescending(i => i.Tarih) : query.OrderBy(i => i.Tarih)
        };

        var kayitlar = await query.Skip(start).Take(length).ToListAsync();
        return (kayitlar, toplamKayit, filtrelenmisKayit);
    }

    public async Task<SevkIrsaliyesi?> GetByIdDetayAsync(int id)
    {
        var irsaliye = await _unitOfWork.Repository<SevkIrsaliyesi>().QueryTumu()
            .Include(i => i.SatisSiparisi).ThenInclude(s => s.Cari)
            .Include(i => i.Sube)
            .Include(i => i.Kalemler).ThenInclude(k => k.Malzeme)
            .FirstOrDefaultAsync(i => i.Id == id);
        return irsaliye is not null && _onayYetkisiService.SubeErisimVarMi(irsaliye.SubeId) ? irsaliye : null;
    }

    public async Task OnaylaAsync(int id)
    {
        var irsaliye = await _unitOfWork.Repository<SevkIrsaliyesi>().QueryTumu()
            .Include(i => i.Kalemler)
            .FirstOrDefaultAsync(i => i.Id == id)
            ?? throw new InvalidOperationException("Sevk irsaliyesi bulunamadı.");
        if (!_onayYetkisiService.SubeErisimVarMi(irsaliye.SubeId))
            throw new InvalidOperationException("Bu irsaliye başka bir şubeye ait, onaylayamazsınız.");

        if (irsaliye.Durum != BelgeDurum.Beklemede)
            throw new InvalidOperationException("Sadece beklemede olan irsaliyeler onaylanabilir.");

        _onayYetkisiService.OlusturanOnaylayamazKontrolEt(irsaliye.CreatedBy, "Sevk İrsaliyesi");

        var malzemeIdleri = irsaliye.Kalemler.Select(k => k.MalzemeId).Distinct().ToList();
        var malzemeler = await _unitOfWork.Repository<Malzeme>().QueryTumu()
            .Where(m => malzemeIdleri.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id);

        foreach (var kalem in irsaliye.Kalemler)
        {
            var malzeme = malzemeler[kalem.MalzemeId];
            var yeniBakiye = malzeme.Bakiye - kalem.Miktar;
            if (yeniBakiye < 0)
                throw new InvalidOperationException(
                    $"{malzeme.MalzemeKodu} için stok yetersiz (mevcut: {malzeme.Bakiye}, istenen: {kalem.Miktar}).");

            malzeme.Bakiye = yeniBakiye;
        }

        irsaliye.Durum = BelgeDurum.Onaylandi;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task IptalEtAsync(int id)
    {
        var irsaliye = await _unitOfWork.Repository<SevkIrsaliyesi>().GetByIdAsync(id)
            ?? throw new InvalidOperationException("Sevk irsaliyesi bulunamadı.");
        if (!_onayYetkisiService.SubeErisimVarMi(irsaliye.SubeId))
            throw new InvalidOperationException("Bu irsaliye başka bir şubeye ait, iptal edemezsiniz.");

        if (irsaliye.Durum != BelgeDurum.Beklemede)
            throw new InvalidOperationException("Sadece beklemede olan irsaliyeler iptal edilebilir.");

        irsaliye.Durum = BelgeDurum.Iptal;
        await _unitOfWork.SaveChangesAsync();
    }
}
