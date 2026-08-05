using Microsoft.EntityFrameworkCore;
using SakaryaERP.Helpers;
using SakaryaERP.Data;
using SakaryaERP.Models;

namespace SakaryaERP.Services;

public class AlisFaturasiService : IAlisFaturasiService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOnayYetkisiService _onayYetkisiService;

    public AlisFaturasiService(IUnitOfWork unitOfWork, IOnayYetkisiService onayYetkisiService)
    {
        _unitOfWork = unitOfWork;
        _onayYetkisiService = onayYetkisiService;
    }

    public async Task<AlisFaturasi> CreateAsync(AlisFaturasi fatura, List<AlisFaturasiKalemi> kalemler)
    {
        if (kalemler.Count == 0)
            throw new InvalidOperationException("Faturada en az bir kalem olmalıdır.");

        if (kalemler.Any(k => k.Miktar <= 0))
            throw new InvalidOperationException("Tüm kalemlerin miktarı sıfırdan büyük olmalıdır.");

        var cari = await _unitOfWork.Repository<Cari>().GetByIdAsync(fatura.CariId)
            ?? throw new InvalidOperationException("Cari bulunamadı.");
        _onayYetkisiService.SubeErisimKontrolEt(cari.SubeId, "Bu cari başka bir şubeye ait, alış faturası açamazsınız.");
        if (cari.CariTipi != CariTipi.Tedarikci && cari.CariTipi != CariTipi.HerIkisi)
            throw new InvalidOperationException("Alış faturası sadece tedarikçi olarak işaretli bir cariye açılabilir.");

        var kalemMalzemeIdleri = kalemler.Select(k => k.MalzemeId).Distinct().ToList();
        var gecerliMalzemeSayisi = await _unitOfWork.Repository<Malzeme>().QueryTumu()
            .CountAsync(m => kalemMalzemeIdleri.Contains(m.Id));
        if (gecerliMalzemeSayisi != kalemMalzemeIdleri.Count)
            throw new InvalidOperationException("Kalemlerden biri geçersiz bir malzemeye ait.");

        if (fatura.AlisIrsaliyesiId is not null)
        {
            var irsaliye = await _unitOfWork.Repository<AlisIrsaliyesi>().QueryTumu()
                .Include(i => i.Kalemler)
                .FirstOrDefaultAsync(i => i.Id == fatura.AlisIrsaliyesiId)
                ?? throw new InvalidOperationException("Alış irsaliyesi bulunamadı.");

            if (irsaliye.Durum != BelgeDurum.Onaylandi)
                throw new InvalidOperationException("Fatura sadece onaylanmış bir irsaliyeden oluşturulabilir.");

            if (fatura.Tarih < irsaliye.Tarih)
                throw new InvalidOperationException("Fatura tarihi, alış irsaliyesi tarihinden önce olamaz.");

            if (await AktifFaturaVarMiAsync(irsaliye.Id))
                throw new InvalidOperationException("Bu irsaliye için zaten bir fatura oluşturulmuş.");

            var irsaliyeMiktarlari = irsaliye.Kalemler
                .GroupBy(k => k.MalzemeId)
                .ToDictionary(g => g.Key, g => g.Sum(k => k.Miktar));
            foreach (var kalem in kalemler)
            {
                if (!irsaliyeMiktarlari.TryGetValue(kalem.MalzemeId, out var alinanMiktar))
                    throw new InvalidOperationException("Fatura kalemleri seçilen irsaliyede olmayan bir malzeme içeremez.");
                if (kalem.Miktar > alinanMiktar)
                    throw new InvalidOperationException(
                        $"Fatura miktarı ({kalem.Miktar}) alınan miktarı ({alinanMiktar}) aşamaz.");
            }

            fatura.AlisSiparisiId = irsaliye.AlisSiparisiId;
        }

        var toplamSayi = await _unitOfWork.Repository<AlisFaturasi>().QueryTumu().CountAsync();
        fatura.FaturaNo = $"AF-{toplamSayi + 1:000000}";
        fatura.Durum = BelgeDurum.Beklemede;
        fatura.Kalemler = kalemler;
        fatura.SubeId = _onayYetkisiService.MevcutKullaniciSubeId();

        await _unitOfWork.Repository<AlisFaturasi>().AddAsync(fatura);
        await _unitOfWork.SaveChangesAsync();
        return fatura;
    }

    public async Task<(IEnumerable<AlisFaturasi> Kayitlar, int ToplamKayit, int FiltrelenmisKayit)> GetSayfaliListeAsync(
        int start, int length, string? genelArama, string?[] sutunAramalari, int siralamaSutunu, string siralamaYonu)
    {
        var query = _unitOfWork.Repository<AlisFaturasi>().QueryTumu()
            .Include(f => f.Cari)
            .Include(f => f.AlisIrsaliyesi)
            .Include(f => f.Kalemler)
            .Where(f => !f.IsDeleted);

        // Şubeye bağlı kullanıcı liste/Excel export'ta da sadece kendi şubesinin (veya şubesiz
        // eski kayıtların) faturalarını görmeli — Detay ekranı zaten SubeErisimVarMi ile
        // kapalıydı ama bu sayfalı liste ayrı bir sorgu olduğu için o kontrolden geçmiyordu.
        var etkinSubeId = _onayYetkisiService.EfektifSube(null);
        if (etkinSubeId is not null)
            query = query.Where(f => f.SubeId == null || f.SubeId == etkinSubeId);

        var toplamKayit = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(genelArama))
        {
            query = query.Where(f => EF.Functions.ILike(f.Cari.Unvan, $"%{genelArama}%"));
        }

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(0)))
            query = query.Where(f => EF.Functions.ILike(f.FaturaNo, $"%{sutunAramalari[0]}%"));

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(2)))
            query = query.Where(f => EF.Functions.ILike(f.Cari.Unvan, $"%{sutunAramalari[2]}%"));

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(4))
            && Enum.TryParse<BelgeDurum>(sutunAramalari[4], out var durumFiltre))
            query = query.Where(f => f.Durum == durumFiltre);

        var filtrelenmisKayit = await query.CountAsync();

        var azalan = siralamaYonu == "desc";
        query = siralamaSutunu switch
        {
            0 => azalan ? query.OrderByDescending(f => f.FaturaNo) : query.OrderBy(f => f.FaturaNo),
            2 => azalan ? query.OrderByDescending(f => f.Cari.Unvan) : query.OrderBy(f => f.Cari.Unvan),
            4 => azalan ? query.OrderByDescending(f => f.Durum) : query.OrderBy(f => f.Durum),
            _ => azalan ? query.OrderByDescending(f => f.Tarih) : query.OrderBy(f => f.Tarih)
        };

        var kayitlar = await query.Skip(start).Take(length).ToListAsync();
        return (kayitlar, toplamKayit, filtrelenmisKayit);
    }

    public async Task<AlisFaturasi?> GetByIdDetayAsync(int id)
    {
        var fatura = await _unitOfWork.Repository<AlisFaturasi>().QueryTumu()
            .Include(f => f.Cari)
            .Include(f => f.AlisSiparisi)
            .Include(f => f.AlisIrsaliyesi)
            .Include(f => f.Kalemler).ThenInclude(k => k.Malzeme)
            .FirstOrDefaultAsync(f => f.Id == id);
        return fatura is not null && _onayYetkisiService.SubeErisimVarMi(fatura.SubeId) ? fatura : null;
    }

    public async Task<bool> AktifFaturaVarMiAsync(int alisIrsaliyesiId)
    {
        return await _unitOfWork.Repository<AlisFaturasi>().QueryTumu()
            .AnyAsync(f => f.AlisIrsaliyesiId == alisIrsaliyesiId && f.Durum != BelgeDurum.Iptal);
    }

    public async Task<List<AlisFaturasi>> GetOnaylanmisListeAsync(DateTime? baslangic = null, DateTime? bitis = null)
    {
        var query = _unitOfWork.Repository<AlisFaturasi>().QueryTumu()
            .Include(f => f.Cari)
            .Include(f => f.Kalemler).ThenInclude(k => k.Malzeme).ThenInclude(m => m.Kategori)
            .Where(f => !f.IsDeleted && f.Durum == BelgeDurum.Onaylandi);

        // Tarih aralığı verilirse SQL'e taşınır — aksi halde tüm onaylı fatura geçmişi belleğe
        // çekilip sonra filtrelenir, veri arttıkça yavaşlar.
        if (baslangic is not null)
            query = query.Where(f => f.Tarih >= baslangic.Value);
        if (bitis is not null)
            query = query.Where(f => f.Tarih <= bitis.Value);

        return await query.ToListAsync();
    }

    public async Task<List<AlisFaturasi>> GetVadesiYaklasanListesiAsync(DateTime yaklasmaSiniri)
    {
        return await _unitOfWork.Repository<AlisFaturasi>().QueryTumu()
            .Include(f => f.Cari)
            .Where(f => f.Durum == BelgeDurum.Onaylandi && f.VadeTarihi != null && f.VadeTarihi <= yaklasmaSiniri)
            .ToListAsync();
    }

    public async Task<List<AlisFaturasiKalemi>> GetMalzemeDogrudanAlisHareketleriAsync(int malzemeId, DateTime? baslangic, DateTime? bitis, int? subeId)
    {
        var query = _unitOfWork.Repository<AlisFaturasiKalemi>().QueryTumu()
            .Include(k => k.AlisFaturasi).ThenInclude(f => f.Sube)
            .Where(k => k.MalzemeId == malzemeId
                && k.AlisFaturasi.Durum == BelgeDurum.Onaylandi
                && k.AlisFaturasi.AlisIrsaliyesiId == null);

        if (baslangic is not null)
            query = query.Where(k => k.AlisFaturasi.Tarih >= baslangic);
        if (bitis is not null)
            query = query.Where(k => k.AlisFaturasi.Tarih <= bitis);
        if (subeId is not null)
            query = query.Where(k => k.AlisFaturasi.SubeId == subeId);

        return await query.ToListAsync();
    }

    public async Task<List<AlisFaturasiKalemi>> GetMalzemeSonAlislariAsync(int malzemeId, int adet)
    {
        return await _unitOfWork.Repository<AlisFaturasiKalemi>().QueryTumu()
            .Include(k => k.AlisFaturasi).ThenInclude(f => f.Cari)
            .Where(k => k.MalzemeId == malzemeId && k.AlisFaturasi.Durum == BelgeDurum.Onaylandi && !k.AlisFaturasi.IsDeleted)
            .OrderByDescending(k => k.AlisFaturasi.Tarih)
            .Take(adet)
            .ToListAsync();
    }

    public async Task OnaylaAsync(int id)
    {
        var fatura = await _unitOfWork.Repository<AlisFaturasi>().QueryTumu()
            .Include(f => f.Kalemler)
            .FirstOrDefaultAsync(f => f.Id == id)
            ?? throw new InvalidOperationException("Alış faturası bulunamadı.");
        _onayYetkisiService.SubeErisimKontrolEt(fatura.SubeId, "Bu fatura başka bir şubeye ait, onaylayamazsınız.");

        if (fatura.Durum != BelgeDurum.Beklemede)
            throw new InvalidOperationException("Sadece beklemede olan faturalar onaylanabilir.");

        _onayYetkisiService.YuksekTutarKontrolEt(fatura.Kalemler.Sum(FinansHesaplama.SatirToplami), "Alış Faturası");
        _onayYetkisiService.OlusturanOnaylayamazKontrolEt(fatura.CreatedBy, "Alış Faturası");

        var cari = await _unitOfWork.Repository<Cari>().GetByIdAsync(fatura.CariId)
            ?? throw new InvalidOperationException("Cari bulunamadı.");

        var faturaMalzemeIdleri = fatura.Kalemler.Select(k => k.MalzemeId).Distinct().ToList();
        var faturaMalzemeleri = await _unitOfWork.Repository<Malzeme>().QueryTumu()
            .Where(m => faturaMalzemeIdleri.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id);

        // İrsaliyeden gelen faturalarda stok zaten irsaliye onayında artırılmıştır;
        // irsaliyesiz (doğrudan girilen) faturalarda burada artırılır.
        if (fatura.AlisIrsaliyesiId is null)
        {
            foreach (var kalem in fatura.Kalemler)
            {
                faturaMalzemeleri[kalem.MalzemeId].Bakiye += kalem.Miktar;
            }
        }

        // Malzeme.AlisFiyati, satış tarafındaki COGS anlık görüntüsünün kullandığı "güncel maliyet"
        // alanıdır — burada güncellenmezse zamanla eskiyip gerçek alım fiyatından sapar (satış
        // tarafında zaten fatura onayı anında BirimMaliyet'e kopyalanıyor, bkz. SatisFaturasiService).
        // İskonto hesaba katılmazsa (sadece BirimFiyat kullanılırsa) iskontolu alımlarda maliyet
        // olduğundan yüksek görünür ve COGS/brüt kar raporları hatalı çıkar.
        foreach (var kalem in fatura.Kalemler)
        {
            faturaMalzemeleri[kalem.MalzemeId].AlisFiyati = FinansHesaplama.BirimNetFiyat(kalem);
        }

        var toplamTutar = fatura.Kalemler.Sum(FinansHesaplama.SatirToplami);
        var netTutar = fatura.Kalemler.Sum(k => k.Miktar * k.BirimFiyat * (1 - k.Iskonto / 100m));
        var kdvTutari = toplamTutar - netTutar;

        var toplamFisSayisi = await _unitOfWork.Repository<CariFisi>().QueryTumu().CountAsync();
        var cariFisi = new CariFisi
        {
            FisNo = $"CF-{toplamFisSayisi + 1:000000}",
            CariId = fatura.CariId,
            Tarih = fatura.Tarih,
            FisTipi = FisTipi.Alacak,
            Tutar = toplamTutar,
            OdemeYontemi = OdemeYontemi.Havale,
            Aciklama = $"Alış Faturası {fatura.FaturaNo}",
            OtomatikOlusturuldu = true,
            AlisFaturasiId = fatura.Id,
            // CariFisiService.CreateAsync üzerinden geçmediği için SubeId burada elle
            // faturanın kendi şubesinden alınıyor — aksi halde şube filtresi uygulanan
            // liste/rapor ekranlarında bu otomatik fiş hiçbir şubede görünmezdi.
            SubeId = fatura.SubeId
        };

        // Alacak fişi: tedarikçiye olan borcumuz arttığı için Cari.Bakiye azalır
        // (CariFisiService'teki Borç/+ Alacak/- yön kuralıyla aynı).
        cari.Bakiye -= toplamTutar;

        await _unitOfWork.Repository<CariFisi>().AddAsync(cariFisi);
        await _unitOfWork.Repository<MuhasebeFisi>().AddAsync(
            await YevmiyeKaydiOlusturAsync(fatura, netTutar, kdvTutari, toplamTutar));

        fatura.Durum = BelgeDurum.Onaylandi;
        await _unitOfWork.SaveChangesAsync();
    }

    // Alış faturası onayında basit yevmiye kaydı: 153 Ticari Mallar ve 191 İndirilecek KDV
    // borçlanır, 320 Satıcılar alacaklanır.
    private async Task<MuhasebeFisi> YevmiyeKaydiOlusturAsync(AlisFaturasi fatura, decimal netTutar, decimal kdvTutari, decimal toplamTutar)
    {
        var hesaplar = await _unitOfWork.Repository<HesapPlani>().QueryTumu()
            .Where(h => h.HesapKodu == "153" || h.HesapKodu == "191" || h.HesapKodu == "320")
            .ToDictionaryAsync(h => h.HesapKodu);

        var aciklama = $"Alış Faturası {fatura.FaturaNo}";
        var kalemler = new List<MuhasebeFisiKalemi>
        {
            new() { HesapPlaniId = hesaplar["153"].Id, Borc = netTutar, Alacak = 0, Aciklama = aciklama },
            new() { HesapPlaniId = hesaplar["320"].Id, Borc = 0, Alacak = toplamTutar, Aciklama = aciklama }
        };
        if (kdvTutari > 0)
            kalemler.Add(new MuhasebeFisiKalemi { HesapPlaniId = hesaplar["191"].Id, Borc = kdvTutari, Alacak = 0, Aciklama = aciklama });

        FinansHesaplama.BorcAlacakDengesiniDogrula(kalemler);

        var toplamFisSayisi = await _unitOfWork.Repository<MuhasebeFisi>().QueryTumu().CountAsync();
        return new MuhasebeFisi
        {
            FisNo = $"MF-{toplamFisSayisi + 1:000000}",
            Tarih = fatura.Tarih,
            AlisFaturasiId = fatura.Id,
            Kalemler = kalemler
        };
    }

    public async Task IptalEtAsync(int id)
    {
        var fatura = await _unitOfWork.Repository<AlisFaturasi>().QueryTumu()
            .Include(f => f.Kalemler)
            .FirstOrDefaultAsync(f => f.Id == id)
            ?? throw new InvalidOperationException("Alış faturası bulunamadı.");
        _onayYetkisiService.SubeErisimKontrolEt(fatura.SubeId, "Bu fatura başka bir şubeye ait, iptal edemezsiniz.");

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
        _onayYetkisiService.YuksekTutarKontrolEt(fatura.Kalemler.Sum(FinansHesaplama.SatirToplami), "Alış Faturası İptali");

        // Onaylanmış fatura: onayda yapılan stok/cari/muhasebe etkisini tek transaction
        // içinde ters yönde geri al (ya hep ya hiç).
        await using var transaction = await _unitOfWork.BeginTransactionAsync();

        var cari = await _unitOfWork.Repository<Cari>().GetByIdAsync(fatura.CariId)
            ?? throw new InvalidOperationException("Cari bulunamadı.");

        // İrsaliyeden gelen faturalarda stok irsaliye onayında artırılmıştı, burada dokunulmaz;
        // irsaliyesiz faturalarda onayda artırılan stok burada geri alınır.
        if (fatura.AlisIrsaliyesiId is null)
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
                        $"{malzeme.MalzemeKodu} için fatura iptal edilemiyor: geri alınacak miktar mevcut stok bakiyesini " +
                        $"negatife düşürür (mevcut: {malzeme.Bakiye}, geri alınacak: {kalem.Miktar}). Muhtemelen bu malzemeden " +
                        "sonradan başka bir işlemle stok düşülmüş.");
                malzeme.Bakiye = yeniBakiye;
            }
        }

        var toplamTutar = fatura.Kalemler.Sum(FinansHesaplama.SatirToplami);
        cari.Bakiye += toplamTutar;

        var otomatikFis = await _unitOfWork.Repository<CariFisi>().QueryTumu()
            .FirstOrDefaultAsync(f => f.AlisFaturasiId == fatura.Id && !f.IsDeleted);
        if (otomatikFis is not null)
            otomatikFis.IsDeleted = true;

        var muhasebeFisi = await _unitOfWork.Repository<MuhasebeFisi>().QueryTumu()
            .FirstOrDefaultAsync(m => m.AlisFaturasiId == fatura.Id && !m.IsDeleted);
        if (muhasebeFisi is not null)
            muhasebeFisi.IsDeleted = true;

        fatura.Durum = BelgeDurum.Iptal;
        await _unitOfWork.SaveChangesAsync();

        await transaction.CommitAsync();
    }

}
