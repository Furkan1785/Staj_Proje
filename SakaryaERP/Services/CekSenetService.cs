using Microsoft.EntityFrameworkCore;
using SakaryaERP.Data;
using SakaryaERP.Models;

namespace SakaryaERP.Services;

public class CekSenetService : ICekSenetService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICariFisiService _cariFisiService;

    public CekSenetService(IUnitOfWork unitOfWork, ICariFisiService cariFisiService)
    {
        _unitOfWork = unitOfWork;
        _cariFisiService = cariFisiService;
    }

    public async Task<IEnumerable<CekSenet>> GetAllAsync()
        => await _unitOfWork.Repository<CekSenet>().QueryTumu().Include(c => c.Cari).ToListAsync();

    public async Task<CekSenet?> GetByIdAsync(int id)
        => await _unitOfWork.Repository<CekSenet>().GetByIdAsync(id);

    public async Task<CekSenet> CreateAsync(CekSenet cekSenet)
    {
        var cari = await _unitOfWork.Repository<Cari>().GetByIdAsync(cekSenet.CariId)
            ?? throw new InvalidOperationException("Cari bulunamadı.");
        if (cari.CariTipi != CariTipi.Musteri && cari.CariTipi != CariTipi.HerIkisi)
            throw new InvalidOperationException("Çek/Senet sadece müşteri olarak işaretli bir cariye kayıt edilebilir.");

        cekSenet.Durum = CekSenetDurum.Portfoyde;
        await _unitOfWork.Repository<CekSenet>().AddAsync(cekSenet);
        await _unitOfWork.SaveChangesAsync();
        return cekSenet;
    }

    public async Task UpdateAsync(CekSenet cekSenet)
    {
        var mevcut = await _unitOfWork.Repository<CekSenet>().GetByIdAsync(cekSenet.Id)
            ?? throw new InvalidOperationException("Çek/Senet kaydı bulunamadı.");

        // Durum Portföyde'den ilerlediyse (Tahsilde/Ciro/TahsilEdildi/Karşılıksız) belge zaten
        // bir cari harekete bağlanmış veya el değiştirmiş olabilir; tutar/cari gibi alanların
        // sonradan değiştirilmesi geçmiş kayıtlarla tutarsızlık yaratır.
        if (mevcut.Durum != CekSenetDurum.Portfoyde)
            throw new InvalidOperationException(
                $"Durumu '{DurumMetni(mevcut.Durum)}' olan bir çek/senet düzenlenemez, sadece Portföyde durumundaki belgeler düzenlenebilir.");

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

    public async Task TahsileVerAsync(int id)
    {
        var cekSenet = await _unitOfWork.Repository<CekSenet>().GetByIdAsync(id)
            ?? throw new InvalidOperationException("Çek/Senet kaydı bulunamadı.");

        if (cekSenet.Durum != CekSenetDurum.Portfoyde)
            throw new InvalidOperationException("Sadece portföydeki bir belge tahsile verilebilir.");

        cekSenet.Durum = CekSenetDurum.Tahsilde;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task CiroEtAsync(int id, string ciroBilgisi)
    {
        var cekSenet = await _unitOfWork.Repository<CekSenet>().GetByIdAsync(id)
            ?? throw new InvalidOperationException("Çek/Senet kaydı bulunamadı.");

        if (cekSenet.Durum != CekSenetDurum.Portfoyde)
            throw new InvalidOperationException("Sadece portföydeki bir belge ciro edilebilir.");

        if (string.IsNullOrWhiteSpace(ciroBilgisi))
            throw new InvalidOperationException("Ciro bilgisi girilmelidir.");

        cekSenet.Durum = CekSenetDurum.Ciro;
        cekSenet.CiroBilgisi = ciroBilgisi;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task KarsiliksizYapAsync(int id)
    {
        var cekSenet = await _unitOfWork.Repository<CekSenet>().GetByIdAsync(id)
            ?? throw new InvalidOperationException("Çek/Senet kaydı bulunamadı.");

        if (cekSenet.Durum != CekSenetDurum.Tahsilde)
            throw new InvalidOperationException("Sadece tahsildeki bir belge karşılıksız işaretlenebilir.");

        cekSenet.Durum = CekSenetDurum.Karsiliksiz;
        await _unitOfWork.SaveChangesAsync();
    }

    // Sadece bu geçişte otomatik cari hareketi oluşur (CLAUDE.md kuralı): belge fiilen
    // tahsil edildiğinde cari alacaklanır ve seçilen banka/kasa hesabına para girer.
    // CariFisi oluşturma + Durum güncellemesi tek transaction'da, ya hep ya hiç yapılır.
    public async Task TahsilEdildiYapAsync(int id, int? bankaHesabiId, int? kasaHesabiId)
    {
        var cekSenet = await _unitOfWork.Repository<CekSenet>().GetByIdAsync(id)
            ?? throw new InvalidOperationException("Çek/Senet kaydı bulunamadı.");

        if (cekSenet.Durum != CekSenetDurum.Tahsilde)
            throw new InvalidOperationException("Sadece tahsildeki bir belge tahsil edildi yapılabilir.");

        if (bankaHesabiId is null && kasaHesabiId is null)
            throw new InvalidOperationException("Tahsilatın yapıldığı banka veya kasa hesabı seçilmelidir.");

        await using var transaction = await _unitOfWork.BeginTransactionAsync();

        await _cariFisiService.CreateAsync(new CariFisi
        {
            CariId = cekSenet.CariId,
            Tarih = DateTime.Today,
            FisTipi = FisTipi.Alacak,
            Tutar = cekSenet.Tutar,
            OdemeYontemi = bankaHesabiId is not null ? OdemeYontemi.Havale : OdemeYontemi.Nakit,
            BankaHesabiId = bankaHesabiId,
            KasaHesabiId = kasaHesabiId,
            Aciklama = $"{(cekSenet.BelgeTipi == BelgeTipi.Cek ? "Çek" : "Senet")} tahsilatı - Belge No: {cekSenet.BelgeNo}"
        });

        cekSenet.Durum = CekSenetDurum.TahsilEdildi;
        await _unitOfWork.SaveChangesAsync();

        await transaction.CommitAsync();
    }

    private static string DurumMetni(CekSenetDurum durum) => durum switch
    {
        CekSenetDurum.Portfoyde => "Portföyde",
        CekSenetDurum.Tahsilde => "Tahsilde",
        CekSenetDurum.Ciro => "Ciro",
        CekSenetDurum.Karsiliksiz => "Karşılıksız",
        CekSenetDurum.TahsilEdildi => "Tahsil Edildi",
        _ => durum.ToString()
    };
}
