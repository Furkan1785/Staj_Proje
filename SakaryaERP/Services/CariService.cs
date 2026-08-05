using Microsoft.EntityFrameworkCore;
using SakaryaERP.Data;
using SakaryaERP.Data.Repositories;
using SakaryaERP.Models;

namespace SakaryaERP.Services;

public class CariService : ICariService
{
    private readonly ICariRepository _cariRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOnayYetkisiService _onayYetkisiService;

    public CariService(ICariRepository cariRepository, IUnitOfWork unitOfWork, IOnayYetkisiService onayYetkisiService)
    {
        _cariRepository = cariRepository;
        _unitOfWork = unitOfWork;
        _onayYetkisiService = onayYetkisiService;
    }

    public async Task<IEnumerable<Cari>> GetAllAsync()
        => await _cariRepository.GetAllAsync();

    public async Task<Cari?> GetByIdAsync(int id)
    {
        var cari = await _cariRepository.GetByIdAsync(id);
        return cari is not null && _onayYetkisiService.SubeErisimVarMi(cari.SubeId) ? cari : null;
    }

    public async Task<Cari> CreateAsync(Cari cari)
    {
        if (await _cariRepository.KoduKullanimdaMiAsync(cari.CariKodu))
            throw new InvalidOperationException("Bu cari kodu zaten kullanılıyor.");

        cari.SubeId = _onayYetkisiService.MevcutKullaniciSubeId();
        await _cariRepository.AddAsync(cari);
        await _unitOfWork.SaveChangesAsync();
        return cari;
    }

    public async Task UpdateAsync(Cari cari)
    {
        if (await _cariRepository.KoduKullanimdaMiAsync(cari.CariKodu, cari.Id))
            throw new InvalidOperationException("Bu cari kodu zaten kullanılıyor.");

        var mevcut = await _cariRepository.GetByIdAsync(cari.Id)
            ?? throw new InvalidOperationException("Cari bulunamadı.");
        _onayYetkisiService.SubeErisimKontrolEt(mevcut.SubeId, "Bu cari başka bir şubeye ait, düzenleyemezsiniz.");

        if (cari.CariTipi != mevcut.CariTipi)
            await TipDaraltmaKontrolEtAsync(mevcut.Id, mevcut.CariTipi, cari.CariTipi);

        // Bakiye/CreatedAt gibi alanlar forma hiç gelmediği için, mevcut (tracked)
        // entity üzerinde sadece düzenlenebilir alanları güncelliyoruz; aksi halde
        // reconstruct edilmiş nesneyi Update() ile kaydetmek Bakiye'yi sıfırlardı.
        mevcut.CariKodu = cari.CariKodu;
        mevcut.Unvan = cari.Unvan;
        mevcut.CariTipi = cari.CariTipi;
        mevcut.VergiNo = cari.VergiNo;
        mevcut.Adres = cari.Adres;
        mevcut.Telefon = cari.Telefon;
        mevcut.EMail = cari.EMail;
        mevcut.KrediLimiti = cari.KrediLimiti;

        await _unitOfWork.SaveChangesAsync();
    }

    // Bir carinin tipi, üzerinde zaten var olan belgelerle uyumsuz hale getirilemez —
    // aksi halde o cari için sonraki satış/alış belgesi oluşturma işlemleri (CariTipi
    // kontrolü nedeniyle) başarısız olur ama var olan geçmiş belgeler ortada kalır.
    private async Task TipDaraltmaKontrolEtAsync(int cariId, CariTipi eskiTip, CariTipi yeniTip)
    {
        var musteriRoluKayboluyor = eskiTip is CariTipi.Musteri or CariTipi.HerIkisi && yeniTip == CariTipi.Tedarikci;
        if (musteriRoluKayboluyor)
        {
            var satisBelgesiVarMi = await _unitOfWork.Repository<SatisSiparisi>().QueryTumu().AnyAsync(s => s.CariId == cariId)
                || await _unitOfWork.Repository<SatisFaturasi>().QueryTumu().AnyAsync(f => f.CariId == cariId)
                || await _unitOfWork.Repository<SatisTeklifi>().QueryTumu().AnyAsync(t => t.CariId == cariId)
                || await _unitOfWork.Repository<MusteriTalebi>().QueryTumu().AnyAsync(t => t.CariId == cariId)
                || await _unitOfWork.Repository<CekSenet>().QueryTumu().AnyAsync(c => c.CariId == cariId);
            if (satisBelgesiVarMi)
                throw new InvalidOperationException(
                    "Bu carinin satış belgesi (talep/teklif/sipariş/fatura/çek-senet) bulunduğu için tipi " +
                    "'Tedarikçi' olarak daraltılamaz.");
        }

        var tedarikciRoluKayboluyor = eskiTip is CariTipi.Tedarikci or CariTipi.HerIkisi && yeniTip == CariTipi.Musteri;
        if (tedarikciRoluKayboluyor)
        {
            var alisBelgesiVarMi = await _unitOfWork.Repository<AlisSiparisi>().QueryTumu().AnyAsync(s => s.CariId == cariId)
                || await _unitOfWork.Repository<AlisIrsaliyesi>().QueryTumu().AnyAsync(i => i.CariId == cariId)
                || await _unitOfWork.Repository<AlisFaturasi>().QueryTumu().AnyAsync(f => f.CariId == cariId);
            if (alisBelgesiVarMi)
                throw new InvalidOperationException(
                    "Bu carinin alış belgesi (sipariş/irsaliye/fatura) bulunduğu için tipi 'Müşteri' olarak daraltılamaz.");
        }
    }

    public async Task PasifYapAsync(int id)
    {
        var cari = await _cariRepository.GetByIdAsync(id)
            ?? throw new InvalidOperationException("Cari bulunamadı.");
        _onayYetkisiService.SubeErisimKontrolEt(cari.SubeId, "Bu cari başka bir şubeye ait, pasif yapamazsınız.");
        if (cari.Bakiye != 0)
            throw new InvalidOperationException(
                $"Bu carinin kapanmamış bir bakiyesi var ({cari.Bakiye.ToString("N2")} TL), pasif yapılamaz.");

        _cariRepository.SoftDelete(cari);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task AktifEtAsync(int id)
    {
        var cari = await _cariRepository.GetByIdTumuAsync(id)
            ?? throw new InvalidOperationException("Cari bulunamadı.");
        _onayYetkisiService.SubeErisimKontrolEt(cari.SubeId, "Bu cari başka bir şubeye ait, aktif yapamazsınız.");

        cari.IsDeleted = false;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<(IEnumerable<Cari> Kayitlar, int ToplamKayit, int FiltrelenmisKayit)> GetSayfaliListeAsync(
        int start, int length, string? genelArama, string?[] sutunAramalari, int siralamaSutunu, string siralamaYonu)
    {
        var query = _cariRepository.QueryTumu();

        // Şubeye bağlı kullanıcı liste/Excel export'ta da sadece kendi şubesinin (veya şubesiz
        // eski kayıtların) carilerini görmeli — Detay ekranı zaten SubeErisimVarMi ile
        // kapalıydı ama bu sayfalı liste ayrı bir sorgu olduğu için o kontrolden geçmiyordu.
        var etkinSubeId = _onayYetkisiService.EfektifSube(null);
        if (etkinSubeId is not null)
            query = query.Where(c => c.SubeId == null || c.SubeId == etkinSubeId);

        var toplamKayit = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(genelArama))
        {
            query = query.Where(c =>
                EF.Functions.ILike(c.CariKodu, $"%{genelArama}%") ||
                EF.Functions.ILike(c.Unvan, $"%{genelArama}%") ||
                (c.Telefon != null && EF.Functions.ILike(c.Telefon, $"%{genelArama}%")) ||
                (c.EMail != null && EF.Functions.ILike(c.EMail, $"%{genelArama}%")));
        }

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(0)))
            query = query.Where(c => EF.Functions.ILike(c.CariKodu, $"%{sutunAramalari[0]}%"));

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(1)))
            query = query.Where(c => EF.Functions.ILike(c.Unvan, $"%{sutunAramalari[1]}%"));

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(2))
            && Enum.TryParse<CariTipi>(sutunAramalari[2], out var tipiFiltre))
            query = query.Where(c => c.CariTipi == tipiFiltre);

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(3)))
            query = query.Where(c => c.Telefon != null && EF.Functions.ILike(c.Telefon, $"%{sutunAramalari[3]}%"));

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(4)))
            query = query.Where(c => c.EMail != null && EF.Functions.ILike(c.EMail, $"%{sutunAramalari[4]}%"));

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(6))
            && bool.TryParse(sutunAramalari[6], out var pasifFiltre))
            query = query.Where(c => c.IsDeleted == pasifFiltre);

        var filtrelenmisKayit = await query.CountAsync();

        var azalan = siralamaYonu == "desc";
        query = siralamaSutunu switch
        {
            0 => azalan ? query.OrderByDescending(c => c.CariKodu) : query.OrderBy(c => c.CariKodu),
            1 => azalan ? query.OrderByDescending(c => c.Unvan) : query.OrderBy(c => c.Unvan),
            2 => azalan ? query.OrderByDescending(c => c.CariTipi) : query.OrderBy(c => c.CariTipi),
            3 => azalan ? query.OrderByDescending(c => c.Telefon) : query.OrderBy(c => c.Telefon),
            4 => azalan ? query.OrderByDescending(c => c.EMail) : query.OrderBy(c => c.EMail),
            5 => azalan ? query.OrderByDescending(c => c.Bakiye) : query.OrderBy(c => c.Bakiye),
            6 => azalan ? query.OrderByDescending(c => c.IsDeleted) : query.OrderBy(c => c.IsDeleted),
            _ => query.OrderBy(c => c.CariKodu)
        };

        var kayitlar = await query.Skip(start).Take(length).ToListAsync();
        return (kayitlar, toplamKayit, filtrelenmisKayit);
    }
}
