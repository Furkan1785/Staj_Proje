using Microsoft.EntityFrameworkCore;
using SakaryaERP.Helpers;
using SakaryaERP.Data;
using SakaryaERP.Models;

namespace SakaryaERP.Services;

public class SatisFaturasiService : ISatisFaturasiService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOnayYetkisiService _onayYetkisiService;

    public SatisFaturasiService(IUnitOfWork unitOfWork, IOnayYetkisiService onayYetkisiService)
    {
        _unitOfWork = unitOfWork;
        _onayYetkisiService = onayYetkisiService;
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
        fatura.SubeId = _onayYetkisiService.MevcutKullaniciSubeId();

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
        var fatura = await _unitOfWork.Repository<SatisFaturasi>().QueryTumu()
            .Include(f => f.Cari)
            .Include(f => f.SatisSiparisi)
            .Include(f => f.SevkIrsaliyesi)
            .Include(f => f.Kalemler).ThenInclude(k => k.Malzeme)
            .FirstOrDefaultAsync(f => f.Id == id);
        return fatura is not null && _onayYetkisiService.SubeErisimVarMi(fatura.SubeId) ? fatura : null;
    }

    public async Task<bool> AktifFaturaVarMiIrsaliyeIcinAsync(int sevkIrsaliyesiId)
    {
        return await _unitOfWork.Repository<SatisFaturasi>().QueryTumu()
            .AnyAsync(f => f.SevkIrsaliyesiId == sevkIrsaliyesiId && f.Durum != BelgeDurum.Iptal);
    }

    public async Task<bool> AktifFaturaVarMiSiparisIcinAsync(int satisSiparisiId)
    {
        // Kasıtlı olarak SevkIrsaliyesiId'ye bakılmıyor: sipariş irsaliye üzerinden zaten
        // faturalandıysa (CreateAsync SatisSiparisiId'yi irsaliyenin siparişine eşitler),
        // aynı siparişten bir de "irsaliyesiz" fatura açılırsa stok ve cari hareketi çift sayılır.
        return await _unitOfWork.Repository<SatisFaturasi>().QueryTumu()
            .AnyAsync(f => f.SatisSiparisiId == satisSiparisiId && f.Durum != BelgeDurum.Iptal);
    }

    public async Task<List<SatisFaturasi>> GetOnaylanmisListeAsync()
    {
        return await _unitOfWork.Repository<SatisFaturasi>().QueryTumu()
            .Include(f => f.Cari)
            .Include(f => f.Kalemler).ThenInclude(k => k.Malzeme).ThenInclude(m => m.Kategori)
            .Where(f => !f.IsDeleted && f.Durum == BelgeDurum.Onaylandi)
            .ToListAsync();
    }

    public async Task OnaylaAsync(int id)
    {
        var fatura = await _unitOfWork.Repository<SatisFaturasi>().QueryTumu()
            .Include(f => f.Kalemler)
            .FirstOrDefaultAsync(f => f.Id == id)
            ?? throw new InvalidOperationException("Satış faturası bulunamadı.");
        if (!_onayYetkisiService.SubeErisimVarMi(fatura.SubeId))
            throw new InvalidOperationException("Bu fatura başka bir şubeye ait, onaylayamazsınız.");

        if (fatura.Durum != BelgeDurum.Beklemede)
            throw new InvalidOperationException("Sadece beklemede olan faturalar onaylanabilir.");

        _onayYetkisiService.YuksekTutarKontrolEt(fatura.Kalemler.Sum(FinansHesaplama.SatisFaturasiSatirToplami), "Satış Faturası");
        _onayYetkisiService.OlusturanOnaylayamazKontrolEt(fatura.CreatedBy, "Satış Faturası");

        var cari = await _unitOfWork.Repository<Cari>().GetByIdAsync(fatura.CariId)
            ?? throw new InvalidOperationException("Cari bulunamadı.");

        var malzemeIdleri = fatura.Kalemler.Select(k => k.MalzemeId).Distinct().ToList();
        var malzemeler = await _unitOfWork.Repository<Malzeme>().QueryTumu()
            .Where(m => malzemeIdleri.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id);

        // İrsaliyeden gelen faturalarda stok zaten irsaliye onayında düşülmüştür;
        // irsaliyesiz (doğrudan siparişten veya manuel) faturalarda burada düşülür.
        if (fatura.SevkIrsaliyesiId is null)
        {
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

        // Brüt kar/COGS hesabı satılan andaki maliyeti kullansın diye Malzeme.AlisFiyati
        // onay anında kaleme kopyalanır (sonradan AlisFiyati değişse bile geçmiş fatura etkilenmez).
        foreach (var kalem in fatura.Kalemler)
            kalem.BirimMaliyet = malzemeler[kalem.MalzemeId].AlisFiyati;

        var toplamTutar = fatura.Kalemler.Sum(FinansHesaplama.SatisFaturasiSatirToplami);
        var netTutar = fatura.Kalemler.Sum(k => k.Miktar * k.BirimFiyat * (1 - k.Iskonto / 100m));
        var kdvTutari = toplamTutar - netTutar;
        var toplamMaliyet = fatura.Kalemler.Sum(k => k.Miktar * k.BirimMaliyet);

        var toplamFisSayisi = await _unitOfWork.Repository<CariFisi>().QueryTumu().CountAsync();
        var cariFisi = new CariFisi
        {
            FisNo = $"CF-{toplamFisSayisi + 1:000000}",
            CariId = fatura.CariId,
            Tarih = fatura.Tarih,
            FisTipi = FisTipi.Borc,
            Tutar = toplamTutar,
            OdemeYontemi = OdemeYontemi.Havale,
            Aciklama = $"Satış Faturası {fatura.FaturaNo}",
            OtomatikOlusturuldu = true,
            SatisFaturasiId = fatura.Id
        };

        // Borç fişi: müşteri bize borçlanır, Cari.Bakiye artar (CariFisiService'teki yön kuralıyla aynı).
        cari.Bakiye += toplamTutar;

        await _unitOfWork.Repository<CariFisi>().AddAsync(cariFisi);
        await _unitOfWork.Repository<MuhasebeFisi>().AddAsync(
            await YevmiyeKaydiOlusturAsync(fatura, netTutar, kdvTutari, toplamTutar, toplamMaliyet));

        fatura.Durum = BelgeDurum.Onaylandi;
        await _unitOfWork.SaveChangesAsync();
    }

    // Satış faturası onayında basit yevmiye kaydı: 120 Alıcılar borçlanır,
    // 600 Yurtiçi Satışlar ve 391 Hesaplanan KDV alacaklanır; ayrıca satılan malın maliyeti
    // 621 Satılan Ticari Mallar Maliyeti'ne borç, 153 Ticari Mallar'a alacak yazılır (COGS).
    private async Task<MuhasebeFisi> YevmiyeKaydiOlusturAsync(SatisFaturasi fatura, decimal netTutar, decimal kdvTutari, decimal toplamTutar, decimal toplamMaliyet)
    {
        var hesaplar = await _unitOfWork.Repository<HesapPlani>().QueryTumu()
            .Where(h => h.HesapKodu == "120" || h.HesapKodu == "600" || h.HesapKodu == "391" || h.HesapKodu == "621" || h.HesapKodu == "153")
            .ToDictionaryAsync(h => h.HesapKodu);

        var aciklama = $"Satış Faturası {fatura.FaturaNo}";
        var kalemler = new List<MuhasebeFisiKalemi>
        {
            new() { HesapPlaniId = hesaplar["120"].Id, Borc = toplamTutar, Alacak = 0, Aciklama = aciklama },
            new() { HesapPlaniId = hesaplar["600"].Id, Borc = 0, Alacak = netTutar, Aciklama = aciklama }
        };
        if (kdvTutari > 0)
            kalemler.Add(new MuhasebeFisiKalemi { HesapPlaniId = hesaplar["391"].Id, Borc = 0, Alacak = kdvTutari, Aciklama = aciklama });
        if (toplamMaliyet > 0)
        {
            kalemler.Add(new MuhasebeFisiKalemi { HesapPlaniId = hesaplar["621"].Id, Borc = toplamMaliyet, Alacak = 0, Aciklama = aciklama });
            kalemler.Add(new MuhasebeFisiKalemi { HesapPlaniId = hesaplar["153"].Id, Borc = 0, Alacak = toplamMaliyet, Aciklama = aciklama });
        }

        var toplamFisSayisi = await _unitOfWork.Repository<MuhasebeFisi>().QueryTumu().CountAsync();
        return new MuhasebeFisi
        {
            FisNo = $"MF-{toplamFisSayisi + 1:000000}",
            Tarih = fatura.Tarih,
            SatisFaturasiId = fatura.Id,
            Kalemler = kalemler
        };
    }

    public async Task IptalEtAsync(int id)
    {
        var fatura = await _unitOfWork.Repository<SatisFaturasi>().QueryTumu()
            .Include(f => f.Kalemler)
            .FirstOrDefaultAsync(f => f.Id == id)
            ?? throw new InvalidOperationException("Satış faturası bulunamadı.");
        if (!_onayYetkisiService.SubeErisimVarMi(fatura.SubeId))
            throw new InvalidOperationException("Bu fatura başka bir şubeye ait, iptal edemezsiniz.");

        if (fatura.Durum == BelgeDurum.Iptal)
            throw new InvalidOperationException("Bu fatura zaten iptal edilmiş.");

        if (fatura.Durum == BelgeDurum.Beklemede)
        {
            fatura.Durum = BelgeDurum.Iptal;
            await _unitOfWork.SaveChangesAsync();
            return;
        }

        // Onaylanmış yüksek tutarlı bir faturayı geri almak, onaylamak kadar hassas bir
        // finansal işlem — aynı Admin-only eşiğine tabi olsun.
        _onayYetkisiService.YuksekTutarKontrolEt(fatura.Kalemler.Sum(FinansHesaplama.SatisFaturasiSatirToplami), "Satış Faturası İptali");

        // Onaylanmış fatura: onayda yapılan stok/cari/muhasebe etkisini tek transaction
        // içinde ters yönde geri al (ya hep ya hiç).
        await using var transaction = await _unitOfWork.BeginTransactionAsync();

        var cari = await _unitOfWork.Repository<Cari>().GetByIdAsync(fatura.CariId)
            ?? throw new InvalidOperationException("Cari bulunamadı.");

        // İrsaliyeden gelen faturalarda stok irsaliye onayında düşülmüştü, burada dokunulmaz;
        // irsaliyesiz faturalarda onayda düşülen stok burada geri eklenir (negatife düşme riski yok).
        if (fatura.SevkIrsaliyesiId is null)
        {
            var malzemeIdleri = fatura.Kalemler.Select(k => k.MalzemeId).Distinct().ToList();
            var malzemeler = await _unitOfWork.Repository<Malzeme>().QueryTumu()
                .Where(m => malzemeIdleri.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id);

            foreach (var kalem in fatura.Kalemler)
            {
                malzemeler[kalem.MalzemeId].Bakiye += kalem.Miktar;
            }
        }

        var toplamTutar = fatura.Kalemler.Sum(FinansHesaplama.SatisFaturasiSatirToplami);
        cari.Bakiye -= toplamTutar;

        var otomatikFis = await _unitOfWork.Repository<CariFisi>().QueryTumu()
            .FirstOrDefaultAsync(f => f.SatisFaturasiId == fatura.Id && !f.IsDeleted);
        if (otomatikFis is not null)
            otomatikFis.IsDeleted = true;

        var muhasebeFisi = await _unitOfWork.Repository<MuhasebeFisi>().QueryTumu()
            .FirstOrDefaultAsync(m => m.SatisFaturasiId == fatura.Id && !m.IsDeleted);
        if (muhasebeFisi is not null)
            muhasebeFisi.IsDeleted = true;

        fatura.Durum = BelgeDurum.Iptal;
        await _unitOfWork.SaveChangesAsync();

        await transaction.CommitAsync();
    }

}
