using Microsoft.EntityFrameworkCore;
using SakaryaERP.Data;
using SakaryaERP.Models;

namespace SakaryaERP.Services;

public class SatisTeklifiService : ISatisTeklifiService
{
    private readonly IUnitOfWork _unitOfWork;

    private readonly IOnayYetkisiService _onayYetkisiService;

    public SatisTeklifiService(IUnitOfWork unitOfWork, IOnayYetkisiService onayYetkisiService)
    {
        _unitOfWork = unitOfWork;
        _onayYetkisiService = onayYetkisiService;
    }

    public async Task<SatisTeklifi> CreateAsync(SatisTeklifi teklif, List<SatisTeklifiKalemi> kalemler)
    {
        if (kalemler.Count == 0)
            throw new InvalidOperationException("Teklifte en az bir kalem olmalıdır.");

        if (kalemler.Any(k => k.Miktar <= 0))
            throw new InvalidOperationException("Tüm kalemlerin miktarı sıfırdan büyük olmalıdır.");

        var cari = await _unitOfWork.Repository<Cari>().GetByIdAsync(teklif.CariId)
            ?? throw new InvalidOperationException("Cari bulunamadı.");
        _onayYetkisiService.SubeErisimKontrolEt(cari.SubeId, "Bu cari başka bir şubeye ait, satış teklifi açamazsınız.");
        if (cari.CariTipi != CariTipi.Musteri && cari.CariTipi != CariTipi.HerIkisi)
            throw new InvalidOperationException("Satış teklifi sadece müşteri olarak işaretli bir cariye açılabilir.");

        var kalemMalzemeIdleri = kalemler.Select(k => k.MalzemeId).Distinct().ToList();
        var gecerliMalzemeSayisi = await _unitOfWork.Repository<Malzeme>().QueryTumu()
            .CountAsync(m => kalemMalzemeIdleri.Contains(m.Id));
        if (gecerliMalzemeSayisi != kalemMalzemeIdleri.Count)
            throw new InvalidOperationException("Kalemlerden biri geçersiz bir malzemeye ait.");

        MusteriTalebi? talep = null;
        if (teklif.MusteriTalebiId is not null)
        {
            talep = await _unitOfWork.Repository<MusteriTalebi>().GetByIdAsync(teklif.MusteriTalebiId.Value)
                ?? throw new InvalidOperationException("Müşteri talebi bulunamadı.");
            _onayYetkisiService.SubeErisimKontrolEt(talep.SubeId, "Bu müşteri talebi başka bir şubeye ait, teklif oluşturamazsınız.");
            if (talep.Durum is not (TalepDurum.Yeni or TalepDurum.Isleniyor))
                throw new InvalidOperationException("Sadece yeni veya işlemedeki bir talepten teklif oluşturulabilir.");
        }

        var toplamSayi = await _unitOfWork.Repository<SatisTeklifi>().QueryTumu().CountAsync();
        teklif.TeklifNo = $"ST-{toplamSayi + 1:000000}";
        teklif.Durum = BelgeDurum.Beklemede;
        teklif.Kalemler = kalemler;
        teklif.SubeId = _onayYetkisiService.MevcutKullaniciSubeId();

        await _unitOfWork.Repository<SatisTeklifi>().AddAsync(teklif);

        // Talep artık bir teklife dönüştüğü için işlemi tamamlanmış sayılır.
        if (talep is not null)
            talep.Durum = TalepDurum.Tamamlandi;

        await _unitOfWork.SaveChangesAsync();
        return teklif;
    }

    public async Task<(IEnumerable<SatisTeklifi> Kayitlar, int ToplamKayit, int FiltrelenmisKayit)> GetSayfaliListeAsync(
        int start, int length, string? genelArama, string?[] sutunAramalari, int siralamaSutunu, string siralamaYonu)
    {
        var query = _unitOfWork.Repository<SatisTeklifi>().QueryTumu()
            .Include(t => t.Cari)
            .Include(t => t.MusteriTalebi)
            .Include(t => t.Kalemler)
            .Where(t => !t.IsDeleted);

        // Şubeye bağlı kullanıcı liste/Excel export'ta da sadece kendi şubesinin (veya şubesiz
        // eski kayıtların) tekliflerini görmeli — Detay ekranı zaten SubeErisimVarMi ile
        // kapalıydı ama bu sayfalı liste ayrı bir sorgu olduğu için o kontrolden geçmiyordu.
        var etkinSubeId = _onayYetkisiService.EfektifSube(null);
        if (etkinSubeId is not null)
            query = query.Where(t => t.SubeId == null || t.SubeId == etkinSubeId);

        var toplamKayit = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(genelArama))
        {
            query = query.Where(t => EF.Functions.ILike(t.Cari.Unvan, $"%{genelArama}%"));
        }

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(0)))
            query = query.Where(t => EF.Functions.ILike(t.TeklifNo, $"%{sutunAramalari[0]}%"));

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(3)))
            query = query.Where(t => EF.Functions.ILike(t.Cari.Unvan, $"%{sutunAramalari[3]}%"));

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(5))
            && Enum.TryParse<BelgeDurum>(sutunAramalari[5], out var durumFiltre))
            query = query.Where(t => t.Durum == durumFiltre);

        var filtrelenmisKayit = await query.CountAsync();

        var azalan = siralamaYonu == "desc";
        query = siralamaSutunu switch
        {
            0 => azalan ? query.OrderByDescending(t => t.TeklifNo) : query.OrderBy(t => t.TeklifNo),
            3 => azalan ? query.OrderByDescending(t => t.Cari.Unvan) : query.OrderBy(t => t.Cari.Unvan),
            5 => azalan ? query.OrderByDescending(t => t.Durum) : query.OrderBy(t => t.Durum),
            _ => azalan ? query.OrderByDescending(t => t.Tarih) : query.OrderBy(t => t.Tarih)
        };

        var kayitlar = await query.Skip(start).Take(length).ToListAsync();
        return (kayitlar, toplamKayit, filtrelenmisKayit);
    }

    public async Task<SatisTeklifi?> GetByIdDetayAsync(int id)
    {
        var teklif = await _unitOfWork.Repository<SatisTeklifi>().QueryTumu()
            .Include(t => t.Cari)
            .Include(t => t.MusteriTalebi)
            .Include(t => t.Kalemler).ThenInclude(k => k.Malzeme)
            .FirstOrDefaultAsync(t => t.Id == id);
        return teklif is not null && _onayYetkisiService.SubeErisimVarMi(teklif.SubeId) ? teklif : null;
    }

    public async Task<List<SatisTeklifi>> GetBeklemedeListesiAsync()
    {
        return await _unitOfWork.Repository<SatisTeklifi>().QueryTumu()
            .Include(t => t.Cari)
            .Where(t => t.Durum == BelgeDurum.Beklemede)
            .ToListAsync();
    }

    public async Task OnaylaAsync(int id)
    {
        var teklif = await _unitOfWork.Repository<SatisTeklifi>().GetByIdAsync(id)
            ?? throw new InvalidOperationException("Satış teklifi bulunamadı.");
        _onayYetkisiService.SubeErisimKontrolEt(teklif.SubeId, "Bu teklif başka bir şubeye ait, onaylayamazsınız.");
        if (teklif.Durum != BelgeDurum.Beklemede)
            throw new InvalidOperationException("Sadece beklemede olan teklifler onaylanabilir.");

        _onayYetkisiService.OlusturanOnaylayamazKontrolEt(teklif.CreatedBy, "Satış Teklifi");

        teklif.Durum = BelgeDurum.Onaylandi;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task IptalEtAsync(int id)
    {
        var teklif = await _unitOfWork.Repository<SatisTeklifi>().GetByIdAsync(id)
            ?? throw new InvalidOperationException("Satış teklifi bulunamadı.");
        _onayYetkisiService.SubeErisimKontrolEt(teklif.SubeId, "Bu teklif başka bir şubeye ait, iptal edemezsiniz.");
        if (teklif.Durum != BelgeDurum.Beklemede)
            throw new InvalidOperationException("Sadece beklemede olan teklifler iptal edilebilir.");

        teklif.Durum = BelgeDurum.Iptal;

        // Teklif bir talepten türediyse ve bu talepten başka aktif (iptal olmayan) bir
        // teklif kalmadıysa, talep "Tamamlandı" kilidinde kalıp yeniden teklif
        // üretilememesin diye "İşleniyor"a geri döner.
        if (teklif.MusteriTalebiId is not null)
        {
            var talep = await _unitOfWork.Repository<MusteriTalebi>().GetByIdAsync(teklif.MusteriTalebiId.Value);
            if (talep is not null && talep.Durum == TalepDurum.Tamamlandi)
            {
                var baskaAktifTeklifVarMi = await _unitOfWork.Repository<SatisTeklifi>().QueryTumu()
                    .AnyAsync(t => t.MusteriTalebiId == talep.Id && t.Id != teklif.Id && t.Durum != BelgeDurum.Iptal);
                if (!baskaAktifTeklifVarMi)
                    talep.Durum = TalepDurum.Isleniyor;
            }
        }

        await _unitOfWork.SaveChangesAsync();
    }
}
