using Microsoft.EntityFrameworkCore;
using SakaryaERP.Data;
using SakaryaERP.Models;

namespace SakaryaERP.Services;

public class MalzemeHareketFisiService : IMalzemeHareketFisiService
{
    private readonly IUnitOfWork _unitOfWork;

    private readonly IOnayYetkisiService _onayYetkisiService;

    public MalzemeHareketFisiService(IUnitOfWork unitOfWork, IOnayYetkisiService onayYetkisiService)
    {
        _unitOfWork = unitOfWork;
        _onayYetkisiService = onayYetkisiService;
    }

    public async Task<MalzemeHareketFisi> CreateAsync(MalzemeHareketFisi fis, List<MalzemeHareketFisiKalemi> kalemler)
    {
        if (kalemler.Count == 0)
            throw new InvalidOperationException("Fişte en az bir kalem olmalıdır.");

        if (kalemler.Any(k => k.Miktar <= 0))
            throw new InvalidOperationException("Tüm kalemlerin miktarı sıfırdan büyük olmalıdır.");

        var subeVarMi = await _unitOfWork.Repository<Sube>().QueryTumu().AnyAsync(s => s.Id == fis.SubeId);
        if (!subeVarMi)
            throw new InvalidOperationException("Şube bulunamadı.");

        var kalemMalzemeIdleri = kalemler.Select(k => k.MalzemeId).Distinct().ToList();
        var gecerliMalzemeSayisi = await _unitOfWork.Repository<Malzeme>().QueryTumu()
            .CountAsync(m => kalemMalzemeIdleri.Contains(m.Id));
        if (gecerliMalzemeSayisi != kalemMalzemeIdleri.Count)
            throw new InvalidOperationException("Kalemlerden biri geçersiz bir malzemeye ait.");

        var toplamSayi = await _unitOfWork.Repository<MalzemeHareketFisi>().QueryTumu().CountAsync();
        fis.FisNo = $"MH-{toplamSayi + 1:000000}";
        fis.Durum = BelgeDurum.Beklemede;
        fis.Kalemler = kalemler;

        await _unitOfWork.Repository<MalzemeHareketFisi>().AddAsync(fis);
        await _unitOfWork.SaveChangesAsync();
        return fis;
    }

    public async Task<(IEnumerable<MalzemeHareketFisi> Kayitlar, int ToplamKayit, int FiltrelenmisKayit)> GetSayfaliListeAsync(
        int start, int length, string? genelArama, string?[] sutunAramalari, int siralamaSutunu, string siralamaYonu)
    {
        var query = _unitOfWork.Repository<MalzemeHareketFisi>().QueryTumu()
            .Include(f => f.Sube)
            .Include(f => f.Kalemler)
            .Where(f => !f.IsDeleted);

        var toplamKayit = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(genelArama))
        {
            query = query.Where(f =>
                EF.Functions.ILike(f.FisNo, $"%{genelArama}%") ||
                EF.Functions.ILike(f.Sube.SubeAdi, $"%{genelArama}%"));
        }

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(0)))
            query = query.Where(f => EF.Functions.ILike(f.FisNo, $"%{sutunAramalari[0]}%"));

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(2))
            && Enum.TryParse<HareketTipi>(sutunAramalari[2], out var hareketTipiFiltre))
            query = query.Where(f => f.HareketTipi == hareketTipiFiltre);

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(3)))
            query = query.Where(f => EF.Functions.ILike(f.Sube.SubeAdi, $"%{sutunAramalari[3]}%"));

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(5))
            && Enum.TryParse<BelgeDurum>(sutunAramalari[5], out var durumFiltre))
            query = query.Where(f => f.Durum == durumFiltre);

        var filtrelenmisKayit = await query.CountAsync();

        var azalan = siralamaYonu == "desc";
        query = siralamaSutunu switch
        {
            0 => azalan ? query.OrderByDescending(f => f.FisNo) : query.OrderBy(f => f.FisNo),
            2 => azalan ? query.OrderByDescending(f => f.HareketTipi) : query.OrderBy(f => f.HareketTipi),
            3 => azalan ? query.OrderByDescending(f => f.Sube.SubeAdi) : query.OrderBy(f => f.Sube.SubeAdi),
            5 => azalan ? query.OrderByDescending(f => f.Durum) : query.OrderBy(f => f.Durum),
            _ => azalan ? query.OrderByDescending(f => f.Tarih) : query.OrderBy(f => f.Tarih)
        };

        var kayitlar = await query.Skip(start).Take(length).ToListAsync();
        return (kayitlar, toplamKayit, filtrelenmisKayit);
    }

    public async Task<MalzemeHareketFisi?> GetByIdDetayAsync(int id)
    {
        return await _unitOfWork.Repository<MalzemeHareketFisi>().QueryTumu()
            .Include(f => f.Sube)
            .Include(f => f.Kalemler)
            .ThenInclude(k => k.Malzeme)
            .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted);
    }

    // Transfer, aynı şirket içinde şubeler arası taşımayı temsil eder — Malzeme'nin
    // tek (şubeye bölünmemiş) Bakiye alanını etkilemez, sadece Giriş/Çıkış/Fire etkiler.
    public async Task OnaylaAsync(int id)
    {
        var fis = await _unitOfWork.Repository<MalzemeHareketFisi>().QueryTumu()
            .Include(f => f.Kalemler)
            .FirstOrDefaultAsync(f => f.Id == id)
            ?? throw new InvalidOperationException("Malzeme hareket fişi bulunamadı.");

        if (fis.Durum != BelgeDurum.Beklemede)
            throw new InvalidOperationException("Sadece beklemedeki bir fiş onaylanabilir.");

        _onayYetkisiService.OlusturanOnaylayamazKontrolEt(fis.CreatedBy, "Malzeme Hareket Fişi");

        var malzemeIdleri = fis.Kalemler.Select(k => k.MalzemeId).Distinct().ToList();
        var malzemeler = await _unitOfWork.Repository<Malzeme>().QueryTumu()
            .Where(m => malzemeIdleri.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id);

        var yon = HareketYonKatsayisi(fis.HareketTipi);

        foreach (var kalem in fis.Kalemler)
        {
            var malzeme = malzemeler[kalem.MalzemeId];
            var yeniBakiye = malzeme.Bakiye + yon * kalem.Miktar;
            if (yeniBakiye < 0)
                throw new InvalidOperationException(
                    $"{malzeme.MalzemeKodu} için stok yetersiz (mevcut: {malzeme.Bakiye}, istenen: {kalem.Miktar}).");

            malzeme.Bakiye = yeniBakiye;
        }

        fis.Durum = BelgeDurum.Onaylandi;
        await _unitOfWork.SaveChangesAsync();
    }

    // Sadece Beklemede'deki fişler iptal edilebilir; Onaylandı fişin bakiye etkisi
    // zaten oluşmuş olduğundan burada geri alma yapılmıyor (bu durum kapsam dışı).
    public async Task IptalEtAsync(int id)
    {
        var fis = await _unitOfWork.Repository<MalzemeHareketFisi>().GetByIdAsync(id)
            ?? throw new InvalidOperationException("Malzeme hareket fişi bulunamadı.");

        if (fis.Durum != BelgeDurum.Beklemede)
            throw new InvalidOperationException("Sadece beklemede olan fişler iptal edilebilir.");

        fis.Durum = BelgeDurum.Iptal;
        await _unitOfWork.SaveChangesAsync();
    }

    private static int HareketYonKatsayisi(HareketTipi tip) => tip switch
    {
        HareketTipi.Giris => 1,
        HareketTipi.Cikis => -1,
        HareketTipi.Fire => -1,
        HareketTipi.Transfer => 0,
        _ => 0
    };

    public async Task<List<MalzemeHareketFisiKalemi>> GetMalzemeGecmisiAsync(int malzemeId, DateTime? baslangic, DateTime? bitis, int? subeId = null)
    {
        var query = _unitOfWork.Repository<MalzemeHareketFisiKalemi>().QueryTumu()
            .Include(k => k.MalzemeHareketFisi)
            .ThenInclude(f => f.Sube)
            .Where(k => k.MalzemeId == malzemeId && k.MalzemeHareketFisi.Durum == BelgeDurum.Onaylandi);

        if (baslangic is not null)
            query = query.Where(k => k.MalzemeHareketFisi.Tarih >= baslangic);
        if (bitis is not null)
            query = query.Where(k => k.MalzemeHareketFisi.Tarih <= bitis);
        if (subeId is not null)
            query = query.Where(k => k.MalzemeHareketFisi.SubeId == subeId);

        return await query.OrderBy(k => k.MalzemeHareketFisi.Tarih).ThenBy(k => k.Id).ToListAsync();
    }
}
