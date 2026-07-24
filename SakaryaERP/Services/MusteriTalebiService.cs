using Microsoft.EntityFrameworkCore;
using SakaryaERP.Data;
using SakaryaERP.Models;

namespace SakaryaERP.Services;

public class MusteriTalebiService : IMusteriTalebiService
{
    private readonly IUnitOfWork _unitOfWork;

    public MusteriTalebiService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<MusteriTalebi> CreateAsync(MusteriTalebi talep)
    {
        var cari = await _unitOfWork.Repository<Cari>().GetByIdAsync(talep.CariId)
            ?? throw new InvalidOperationException("Cari bulunamadı.");
        if (cari.CariTipi != CariTipi.Musteri && cari.CariTipi != CariTipi.HerIkisi)
            throw new InvalidOperationException("Müşteri talebi sadece müşteri olarak işaretli bir cariye açılabilir.");

        var toplamSayi = await _unitOfWork.Repository<MusteriTalebi>().QueryTumu().CountAsync();
        talep.TalepNo = $"MT-{toplamSayi + 1:000000}";
        talep.Durum = TalepDurum.Yeni;

        await _unitOfWork.Repository<MusteriTalebi>().AddAsync(talep);
        await _unitOfWork.SaveChangesAsync();
        return talep;
    }

    public async Task<(IEnumerable<MusteriTalebi> Kayitlar, int ToplamKayit, int FiltrelenmisKayit)> GetSayfaliListeAsync(
        int start, int length, string? genelArama, string?[] sutunAramalari, int siralamaSutunu, string siralamaYonu)
    {
        var query = _unitOfWork.Repository<MusteriTalebi>().QueryTumu()
            .Include(t => t.Cari)
            .Where(t => !t.IsDeleted);

        var toplamKayit = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(genelArama))
        {
            query = query.Where(t =>
                EF.Functions.ILike(t.Cari.Unvan, $"%{genelArama}%") ||
                (t.Icerik != null && EF.Functions.ILike(t.Icerik, $"%{genelArama}%")));
        }

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(0)))
            query = query.Where(t => EF.Functions.ILike(t.TalepNo, $"%{sutunAramalari[0]}%"));

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(2)))
            query = query.Where(t => EF.Functions.ILike(t.Cari.Unvan, $"%{sutunAramalari[2]}%"));

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(4))
            && Enum.TryParse<TalepDurum>(sutunAramalari[4], out var durumFiltre))
            query = query.Where(t => t.Durum == durumFiltre);

        var filtrelenmisKayit = await query.CountAsync();

        var azalan = siralamaYonu == "desc";
        query = siralamaSutunu switch
        {
            0 => azalan ? query.OrderByDescending(t => t.TalepNo) : query.OrderBy(t => t.TalepNo),
            2 => azalan ? query.OrderByDescending(t => t.Cari.Unvan) : query.OrderBy(t => t.Cari.Unvan),
            4 => azalan ? query.OrderByDescending(t => t.Durum) : query.OrderBy(t => t.Durum),
            _ => azalan ? query.OrderByDescending(t => t.Tarih) : query.OrderBy(t => t.Tarih)
        };

        var kayitlar = await query.Skip(start).Take(length).ToListAsync();
        return (kayitlar, toplamKayit, filtrelenmisKayit);
    }

    public async Task<MusteriTalebi?> GetByIdAsync(int id)
    {
        return await _unitOfWork.Repository<MusteriTalebi>().QueryTumu()
            .Include(t => t.Cari)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task IslemeAlAsync(int id)
    {
        var talep = await _unitOfWork.Repository<MusteriTalebi>().GetByIdAsync(id)
            ?? throw new InvalidOperationException("Müşteri talebi bulunamadı.");
        if (talep.Durum != TalepDurum.Yeni)
            throw new InvalidOperationException("Sadece yeni talepler işleme alınabilir.");

        talep.Durum = TalepDurum.Isleniyor;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task IptalEtAsync(int id)
    {
        var talep = await _unitOfWork.Repository<MusteriTalebi>().GetByIdAsync(id)
            ?? throw new InvalidOperationException("Müşteri talebi bulunamadı.");
        if (talep.Durum is not (TalepDurum.Yeni or TalepDurum.Isleniyor))
            throw new InvalidOperationException("Sadece yeni veya işlemedeki talepler iptal edilebilir.");

        talep.Durum = TalepDurum.Iptal;
        await _unitOfWork.SaveChangesAsync();
    }
}
