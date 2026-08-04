using Microsoft.EntityFrameworkCore;
using SakaryaERP.Data;
using SakaryaERP.Models;

namespace SakaryaERP.Services;

public class CariFisiService : ICariFisiService
{
    private readonly IUnitOfWork _unitOfWork;

    public CariFisiService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CariFisi> CreateAsync(CariFisi fis)
    {
        var cari = await _unitOfWork.Repository<Cari>().GetByIdAsync(fis.CariId)
            ?? throw new InvalidOperationException("Cari bulunamadı.");

        if (fis.Tutar <= 0)
            throw new InvalidOperationException("Tutar sıfırdan büyük olmalıdır.");

        if (fis.FisTipi == FisTipi.Mahsup)
        {
            fis.BankaHesabiId = null;
            fis.KasaHesabiId = null;
        }
        else if (fis.OdemeYontemi == OdemeYontemi.Nakit)
        {
            if (fis.KasaHesabiId is null)
                throw new InvalidOperationException("Nakit ödeme yönteminde kasa hesabı seçilmelidir.");
            fis.BankaHesabiId = null;
        }
        else
        {
            if (fis.BankaHesabiId is null)
                throw new InvalidOperationException("Havale/Kredi kartı ödeme yönteminde banka hesabı seçilmelidir.");
            fis.KasaHesabiId = null;
        }

        // Borç fişi carinin bize olan borcunu artırır (bakiye +); Alacak/Mahsup azaltır (bakiye -).
        // Karşılığında ilişkili banka/kasa hesabında ters yönde hareket olur (Borç = ödeme yapıldı/çıkış, Alacak = tahsilat/giriş).
        var cariYonu = CariYonKatsayisi(fis.FisTipi);
        cari.Bakiye += cariYonu * fis.Tutar;

        if (fis.BankaHesabiId is not null)
        {
            var banka = await _unitOfWork.Repository<BankaHesabi>().GetByIdAsync(fis.BankaHesabiId.Value)
                ?? throw new InvalidOperationException("Banka hesabı bulunamadı.");
            banka.Bakiye -= cariYonu * fis.Tutar;
        }
        else if (fis.KasaHesabiId is not null)
        {
            var kasa = await _unitOfWork.Repository<KasaHesabi>().GetByIdAsync(fis.KasaHesabiId.Value)
                ?? throw new InvalidOperationException("Kasa hesabı bulunamadı.");
            kasa.Bakiye -= cariYonu * fis.Tutar;
        }

        var toplamSayi = await _unitOfWork.Repository<CariFisi>().QueryTumu().CountAsync();
        fis.FisNo = $"CF-{toplamSayi + 1:000000}";

        await _unitOfWork.Repository<CariFisi>().AddAsync(fis);
        await _unitOfWork.SaveChangesAsync();
        return fis;
    }

    public async Task<CariFisi?> GetByIdDetayAsync(int id)
    {
        return await _unitOfWork.Repository<CariFisi>().QueryTumu()
            .Include(f => f.Cari)
            .Include(f => f.BankaHesabi)
            .Include(f => f.KasaHesabi)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    // Fiş tutarı/carisi sonradan değiştirilemez (yanlış düzeltme bakiyeleri bozar); tek güvenli
    // yol fişi iptal edip (bakiye etkisini ters yönde geri alıp) doğrusunu yeniden girmektir.
    public async Task IptalEtAsync(int id)
    {
        var fis = await _unitOfWork.Repository<CariFisi>().GetByIdAsync(id)
            ?? throw new InvalidOperationException("Cari fişi bulunamadı.");

        if (fis.IsDeleted)
            throw new InvalidOperationException("Bu fiş zaten iptal edilmiş.");

        if (fis.OtomatikOlusturuldu)
            throw new InvalidOperationException(
                "Bu fiş bir belgeden (fatura/çek-senet tahsilatı) otomatik oluşturulmuştur, doğrudan iptal edilemez. " +
                "Kaynak belgeyi iptal etmeniz gerekir.");

        await using var transaction = await _unitOfWork.BeginTransactionAsync();

        var cari = await _unitOfWork.Repository<Cari>().GetByIdAsync(fis.CariId)
            ?? throw new InvalidOperationException("Cari bulunamadı.");

        var cariYonu = CariYonKatsayisi(fis.FisTipi);
        cari.Bakiye -= cariYonu * fis.Tutar;

        if (fis.BankaHesabiId is not null)
        {
            var banka = await _unitOfWork.Repository<BankaHesabi>().GetByIdAsync(fis.BankaHesabiId.Value)
                ?? throw new InvalidOperationException("Banka hesabı bulunamadı.");
            banka.Bakiye += cariYonu * fis.Tutar;
        }
        else if (fis.KasaHesabiId is not null)
        {
            var kasa = await _unitOfWork.Repository<KasaHesabi>().GetByIdAsync(fis.KasaHesabiId.Value)
                ?? throw new InvalidOperationException("Kasa hesabı bulunamadı.");
            kasa.Bakiye += cariYonu * fis.Tutar;
        }

        fis.IsDeleted = true;
        await _unitOfWork.SaveChangesAsync();

        await transaction.CommitAsync();
    }

    public async Task<(IEnumerable<CariFisi> Kayitlar, int ToplamKayit, int FiltrelenmisKayit)> GetSayfaliListeAsync(
        int start, int length, string? genelArama, string?[] sutunAramalari, int siralamaSutunu, string siralamaYonu)
    {
        var query = _unitOfWork.Repository<CariFisi>().QueryTumu()
            .Include(f => f.Cari)
            .Include(f => f.BankaHesabi)
            .Include(f => f.KasaHesabi)
            .Where(f => !f.IsDeleted);

        var toplamKayit = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(genelArama))
        {
            query = query.Where(f =>
                EF.Functions.ILike(f.FisNo, $"%{genelArama}%") ||
                EF.Functions.ILike(f.Cari.Unvan, $"%{genelArama}%") ||
                (f.Aciklama != null && EF.Functions.ILike(f.Aciklama, $"%{genelArama}%")));
        }

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(0)))
            query = query.Where(f => EF.Functions.ILike(f.FisNo, $"%{sutunAramalari[0]}%"));

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(2)))
            query = query.Where(f => EF.Functions.ILike(f.Cari.Unvan, $"%{sutunAramalari[2]}%"));

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(3))
            && Enum.TryParse<FisTipi>(sutunAramalari[3], out var fisTipiFiltre))
            query = query.Where(f => f.FisTipi == fisTipiFiltre);

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(5))
            && Enum.TryParse<OdemeYontemi>(sutunAramalari[5], out var odemeYontemiFiltre))
            query = query.Where(f => f.OdemeYontemi == odemeYontemiFiltre);

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(7)))
            query = query.Where(f => f.Aciklama != null && EF.Functions.ILike(f.Aciklama, $"%{sutunAramalari[7]}%"));

        var filtrelenmisKayit = await query.CountAsync();

        var azalan = siralamaYonu == "desc";
        query = siralamaSutunu switch
        {
            0 => azalan ? query.OrderByDescending(f => f.FisNo) : query.OrderBy(f => f.FisNo),
            1 => azalan ? query.OrderByDescending(f => f.Tarih) : query.OrderBy(f => f.Tarih),
            2 => azalan ? query.OrderByDescending(f => f.Cari.Unvan) : query.OrderBy(f => f.Cari.Unvan),
            3 => azalan ? query.OrderByDescending(f => f.FisTipi) : query.OrderBy(f => f.FisTipi),
            4 => azalan ? query.OrderByDescending(f => f.Tutar) : query.OrderBy(f => f.Tutar),
            5 => azalan ? query.OrderByDescending(f => f.OdemeYontemi) : query.OrderBy(f => f.OdemeYontemi),
            _ => query.OrderByDescending(f => f.Tarih)
        };

        var kayitlar = await query.Skip(start).Take(length).ToListAsync();
        return (kayitlar, toplamKayit, filtrelenmisKayit);
    }

    public async Task<(Cari Cari, decimal DevirBakiye, List<(CariFisi Fis, decimal KumulatifBakiye)> Satirlar)> GetEkstreAsync(
        int cariId, DateTime baslangic, DateTime bitis)
    {
        var cari = await _unitOfWork.Repository<Cari>().GetByIdAsync(cariId)
            ?? throw new InvalidOperationException("Cari bulunamadı.");

        var tumFisler = await _unitOfWork.Repository<CariFisi>().QueryTumu()
            .Where(f => f.CariId == cariId && !f.IsDeleted)
            .OrderBy(f => f.Tarih).ThenBy(f => f.Id)
            .ToListAsync();

        var devirBakiye = tumFisler
            .Where(f => f.Tarih < baslangic)
            .Sum(f => CariYonKatsayisi(f.FisTipi) * f.Tutar);

        var kumulatif = devirBakiye;
        var satirlar = new List<(CariFisi Fis, decimal KumulatifBakiye)>();
        foreach (var fis in tumFisler.Where(f => f.Tarih >= baslangic && f.Tarih <= bitis))
        {
            kumulatif += CariYonKatsayisi(fis.FisTipi) * fis.Tutar;
            satirlar.Add((fis, kumulatif));
        }

        return (cari, devirBakiye, satirlar);
    }

    private static int CariYonKatsayisi(FisTipi fisTipi) => fisTipi == FisTipi.Borc ? 1 : -1;
}
