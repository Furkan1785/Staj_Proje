using Microsoft.EntityFrameworkCore;
using SakaryaERP.Data;
using SakaryaERP.Models;

namespace SakaryaERP.Services;

public class SatisFaturasiService : ISatisFaturasiService
{
    private readonly IUnitOfWork _unitOfWork;

    public SatisFaturasiService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SatisFaturasi> CreateAsync(SatisFaturasi fatura, List<SatisFaturasiKalemi> kalemler)
    {
        if (kalemler.Count == 0)
            throw new InvalidOperationException("Faturada en az bir kalem olmalıdır.");

        if (kalemler.Any(k => k.Miktar <= 0))
            throw new InvalidOperationException("Tüm kalemlerin miktarı sıfırdan büyük olmalıdır.");

        var cari = await _unitOfWork.Repository<Cari>().GetByIdAsync(fatura.CariId)
            ?? throw new InvalidOperationException("Cari bulunamadı.");
        if (cari.CariTipi != CariTipi.Musteri && cari.CariTipi != CariTipi.HerIkisi)
            throw new InvalidOperationException("Satış faturası sadece müşteri olarak işaretli bir cariye açılabilir.");

        var kalemMalzemeIdleri = kalemler.Select(k => k.MalzemeId).Distinct().ToList();
        var gecerliMalzemeSayisi = await _unitOfWork.Repository<Malzeme>().QueryTumu()
            .CountAsync(m => kalemMalzemeIdleri.Contains(m.Id));
        if (gecerliMalzemeSayisi != kalemMalzemeIdleri.Count)
            throw new InvalidOperationException("Kalemlerden biri geçersiz bir malzemeye ait.");

        if (fatura.SevkIrsaliyesiId is not null)
        {
            var irsaliye = await _unitOfWork.Repository<SevkIrsaliyesi>().QueryTumu()
                .Include(i => i.Kalemler)
                .FirstOrDefaultAsync(i => i.Id == fatura.SevkIrsaliyesiId)
                ?? throw new InvalidOperationException("Sevk irsaliyesi bulunamadı.");

            if (irsaliye.Durum != BelgeDurum.Onaylandi)
                throw new InvalidOperationException("Fatura sadece onaylanmış bir irsaliyeden oluşturulabilir.");

            if (await AktifFaturaVarMiIrsaliyeIcinAsync(irsaliye.Id))
                throw new InvalidOperationException("Bu irsaliye için zaten bir fatura oluşturulmuş.");

            var irsaliyeMalzemeIdleri = irsaliye.Kalemler.Select(k => k.MalzemeId).ToHashSet();
            if (kalemler.Any(k => !irsaliyeMalzemeIdleri.Contains(k.MalzemeId)))
                throw new InvalidOperationException("Fatura kalemleri seçilen irsaliyede olmayan bir malzeme içeremez.");

            fatura.SatisSiparisiId = irsaliye.SatisSiparisiId;
        }
        else if (fatura.SatisSiparisiId is not null)
        {
            var siparis = await _unitOfWork.Repository<SatisSiparisi>().QueryTumu()
                .Include(s => s.Kalemler)
                .FirstOrDefaultAsync(s => s.Id == fatura.SatisSiparisiId)
                ?? throw new InvalidOperationException("Satış siparişi bulunamadı.");

            if (siparis.Durum != BelgeDurum.Onaylandi)
                throw new InvalidOperationException("Fatura sadece onaylanmış bir siparişten oluşturulabilir.");

            if (await AktifFaturaVarMiSiparisIcinAsync(siparis.Id))
                throw new InvalidOperationException("Bu sipariş için zaten bir fatura oluşturulmuş.");

            var siparisMalzemeIdleri = siparis.Kalemler.Select(k => k.MalzemeId).ToHashSet();
            if (kalemler.Any(k => !siparisMalzemeIdleri.Contains(k.MalzemeId)))
                throw new InvalidOperationException("Fatura kalemleri seçilen siparişte olmayan bir malzeme içeremez.");
        }

        var toplamSayi = await _unitOfWork.Repository<SatisFaturasi>().QueryTumu().CountAsync();
        fatura.FaturaNo = $"SF-{toplamSayi + 1:000000}";
        fatura.Durum = BelgeDurum.Beklemede;
        fatura.Kalemler = kalemler;

        await _unitOfWork.Repository<SatisFaturasi>().AddAsync(fatura);
        await _unitOfWork.SaveChangesAsync();
        return fatura;
    }

    public async Task<(IEnumerable<SatisFaturasi> Kayitlar, int ToplamKayit, int FiltrelenmisKayit)> GetSayfaliListeAsync(
        int start, int length, string? genelArama, string?[] sutunAramalari, int siralamaSutunu, string siralamaYonu)
    {
        var query = _unitOfWork.Repository<SatisFaturasi>().QueryTumu()
            .Include(f => f.Cari)
            .Include(f => f.SatisSiparisi)
            .Include(f => f.SevkIrsaliyesi)
            .Include(f => f.Kalemler)
            .Where(f => !f.IsDeleted);

        var toplamKayit = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(genelArama))
        {
            query = query.Where(f => EF.Functions.ILike(f.Cari.Unvan, $"%{genelArama}%"));
        }

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(0)))
            query = query.Where(f => EF.Functions.ILike(f.FaturaNo, $"%{sutunAramalari[0]}%"));

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(3)))
            query = query.Where(f => EF.Functions.ILike(f.Cari.Unvan, $"%{sutunAramalari[3]}%"));

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(6))
            && Enum.TryParse<BelgeDurum>(sutunAramalari[6], out var durumFiltre))
            query = query.Where(f => f.Durum == durumFiltre);

        var filtrelenmisKayit = await query.CountAsync();

        var azalan = siralamaYonu == "desc";
        query = siralamaSutunu switch
        {
            0 => azalan ? query.OrderByDescending(f => f.FaturaNo) : query.OrderBy(f => f.FaturaNo),
            3 => azalan ? query.OrderByDescending(f => f.Cari.Unvan) : query.OrderBy(f => f.Cari.Unvan),
            6 => azalan ? query.OrderByDescending(f => f.Durum) : query.OrderBy(f => f.Durum),
            _ => azalan ? query.OrderByDescending(f => f.Tarih) : query.OrderBy(f => f.Tarih)
        };

        var kayitlar = await query.Skip(start).Take(length).ToListAsync();
        return (kayitlar, toplamKayit, filtrelenmisKayit);
    }

    public async Task<SatisFaturasi?> GetByIdDetayAsync(int id)
    {
        return await _unitOfWork.Repository<SatisFaturasi>().QueryTumu()
            .Include(f => f.Cari)
            .Include(f => f.SatisSiparisi)
            .Include(f => f.SevkIrsaliyesi)
            .Include(f => f.Kalemler).ThenInclude(k => k.Malzeme)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<bool> AktifFaturaVarMiIrsaliyeIcinAsync(int sevkIrsaliyesiId)
    {
        return await _unitOfWork.Repository<SatisFaturasi>().QueryTumu()
            .AnyAsync(f => f.SevkIrsaliyesiId == sevkIrsaliyesiId && f.Durum != BelgeDurum.Iptal);
    }

    public async Task<bool> AktifFaturaVarMiSiparisIcinAsync(int satisSiparisiId)
    {
        return await _unitOfWork.Repository<SatisFaturasi>().QueryTumu()
            .AnyAsync(f => f.SatisSiparisiId == satisSiparisiId && f.SevkIrsaliyesiId == null && f.Durum != BelgeDurum.Iptal);
    }

    public async Task OnaylaAsync(int id)
    {
        var fatura = await _unitOfWork.Repository<SatisFaturasi>().QueryTumu()
            .Include(f => f.Kalemler)
            .FirstOrDefaultAsync(f => f.Id == id)
            ?? throw new InvalidOperationException("Satış faturası bulunamadı.");

        if (fatura.Durum != BelgeDurum.Beklemede)
            throw new InvalidOperationException("Sadece beklemede olan faturalar onaylanabilir.");

        var cari = await _unitOfWork.Repository<Cari>().GetByIdAsync(fatura.CariId)
            ?? throw new InvalidOperationException("Cari bulunamadı.");

        // İrsaliyeden gelen faturalarda stok zaten irsaliye onayında düşülmüştür;
        // irsaliyesiz (doğrudan siparişten veya manuel) faturalarda burada düşülür.
        if (fatura.SevkIrsaliyesiId is null)
        {
            var malzemeIdleri = fatura.Kalemler.Select(k => k.MalzemeId).Distinct().ToList();
            var malzemeler = await _unitOfWork.Repository<Malzeme>().QueryTumu()
                .Where(m => malzemeIdleri.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id);

            foreach (var kalem in fatura.Kalemler)
            {
                var malzeme = malzemeler[kalem.MalzemeId];
                var yeniBakiye = malzeme.Bakiye - kalem.Miktar;
                if (yeniBakiye < 0)
                    throw new InvalidOperationException(
                        $"{malzeme.MalzemeKodu} için stok yetersiz (mevcut: {malzeme.Bakiye}, istenen: {kalem.Miktar}).");

                malzeme.Bakiye = yeniBakiye;
            }
        }

        var toplamTutar = fatura.Kalemler.Sum(KalemToplami);

        var toplamFisSayisi = await _unitOfWork.Repository<CariFisi>().QueryTumu().CountAsync();
        var cariFisi = new CariFisi
        {
            FisNo = $"CF-{toplamFisSayisi + 1:000000}",
            CariId = fatura.CariId,
            Tarih = fatura.Tarih,
            FisTipi = FisTipi.Borc,
            Tutar = toplamTutar,
            OdemeYontemi = OdemeYontemi.Havale,
            Aciklama = $"Satış Faturası {fatura.FaturaNo}"
        };

        // Borç fişi: müşteri bize borçlanır, Cari.Bakiye artar (CariFisiService'teki yön kuralıyla aynı).
        cari.Bakiye += toplamTutar;

        await _unitOfWork.Repository<CariFisi>().AddAsync(cariFisi);

        fatura.Durum = BelgeDurum.Onaylandi;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task IptalEtAsync(int id)
    {
        var fatura = await _unitOfWork.Repository<SatisFaturasi>().GetByIdAsync(id)
            ?? throw new InvalidOperationException("Satış faturası bulunamadı.");

        if (fatura.Durum != BelgeDurum.Beklemede)
            throw new InvalidOperationException("Sadece beklemede olan faturalar iptal edilebilir.");

        fatura.Durum = BelgeDurum.Iptal;
        await _unitOfWork.SaveChangesAsync();
    }

    private static decimal KalemToplami(SatisFaturasiKalemi k)
    {
        var araToplam = k.Miktar * k.BirimFiyat;
        var iskontolu = araToplam * (1 - k.Iskonto / 100);
        return iskontolu * (1 + k.KdvOrani / 100);
    }
}
