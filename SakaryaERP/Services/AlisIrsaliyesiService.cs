using Microsoft.EntityFrameworkCore;
using SakaryaERP.Data;
using SakaryaERP.Models;

namespace SakaryaERP.Services;

public class AlisIrsaliyesiService : IAlisIrsaliyesiService
{
    private readonly IUnitOfWork _unitOfWork;

    public AlisIrsaliyesiService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AlisIrsaliyesi> CreateAsync(AlisIrsaliyesi irsaliye, List<AlisIrsaliyesiKalemi> kalemler)
    {
        if (kalemler.Count == 0)
            throw new InvalidOperationException("İrsaliyede en az bir kalem olmalıdır.");

        if (kalemler.Any(k => k.Miktar <= 0))
            throw new InvalidOperationException("Tüm kalemlerin miktarı sıfırdan büyük olmalıdır.");

        var cari = await _unitOfWork.Repository<Cari>().GetByIdAsync(irsaliye.CariId)
            ?? throw new InvalidOperationException("Cari bulunamadı.");
        if (cari.CariTipi != CariTipi.Tedarikci && cari.CariTipi != CariTipi.HerIkisi)
            throw new InvalidOperationException("Alış irsaliyesi sadece tedarikçi olarak işaretli bir cariye açılabilir.");

        var subeVarMi = await _unitOfWork.Repository<Sube>().QueryTumu().AnyAsync(s => s.Id == irsaliye.SubeId);
        if (!subeVarMi)
            throw new InvalidOperationException("Şube bulunamadı.");

        var kalemMalzemeIdleri = kalemler.Select(k => k.MalzemeId).Distinct().ToList();
        var malzemeler = await _unitOfWork.Repository<Malzeme>().QueryTumu()
            .Where(m => kalemMalzemeIdleri.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id);
        if (malzemeler.Count != kalemMalzemeIdleri.Count)
            throw new InvalidOperationException("Kalemlerden biri geçersiz bir malzemeye ait.");

        if (irsaliye.AlisSiparisiId is not null)
        {
            var siparis = await _unitOfWork.Repository<AlisSiparisi>().QueryTumu()
                .Include(s => s.Kalemler)
                .Include(s => s.AlisIrsaliyeleri).ThenInclude(i => i.Kalemler)
                .FirstOrDefaultAsync(s => s.Id == irsaliye.AlisSiparisiId)
                ?? throw new InvalidOperationException("Alış siparişi bulunamadı.");

            if (siparis.Durum != BelgeDurum.Onaylandi)
                throw new InvalidOperationException("İrsaliye sadece onaylanmış bir siparişten oluşturulabilir.");

            var siparisKalemMiktarlari = siparis.Kalemler.ToDictionary(k => k.MalzemeId, k => k.Miktar);
            if (kalemler.Any(k => !siparisKalemMiktarlari.ContainsKey(k.MalzemeId)))
                throw new InvalidOperationException("İrsaliye kalemleri seçilen siparişte olmayan bir malzeme içeremez.");

            // Kısmi teslimat: aynı siparişten daha önce (iptal hariç) teslim alınan miktarlar düşülüp
            // kalan teslim alınabilir miktar bulunur — aksi halde sipariş miktarının üzerinde teslimat mümkün olurdu.
            var teslimAlinanMiktarlar = siparis.AlisIrsaliyeleri
                .Where(i => i.Durum != BelgeDurum.Iptal)
                .SelectMany(i => i.Kalemler)
                .GroupBy(k => k.MalzemeId)
                .ToDictionary(g => g.Key, g => g.Sum(k => k.Miktar));

            foreach (var kalem in kalemler)
            {
                var kalanMiktar = siparisKalemMiktarlari[kalem.MalzemeId] - teslimAlinanMiktarlar.GetValueOrDefault(kalem.MalzemeId);
                if (kalem.Miktar > kalanMiktar)
                    throw new InvalidOperationException(
                        $"{malzemeler[kalem.MalzemeId].MalzemeKodu} için teslim alınabilecek miktar sipariş miktarını aşıyor (kalan: {kalanMiktar}).");
            }
        }

        var toplamSayi = await _unitOfWork.Repository<AlisIrsaliyesi>().QueryTumu().CountAsync();
        irsaliye.IrsaliyeNo = $"AI-{toplamSayi + 1:000000}";
        irsaliye.Durum = BelgeDurum.Beklemede;
        irsaliye.Kalemler = kalemler;

        await _unitOfWork.Repository<AlisIrsaliyesi>().AddAsync(irsaliye);
        await _unitOfWork.SaveChangesAsync();
        return irsaliye;
    }

    public async Task<(IEnumerable<AlisIrsaliyesi> Kayitlar, int ToplamKayit, int FiltrelenmisKayit)> GetSayfaliListeAsync(
        int start, int length, string? genelArama, string?[] sutunAramalari, int siralamaSutunu, string siralamaYonu)
    {
        var query = _unitOfWork.Repository<AlisIrsaliyesi>().QueryTumu()
            .Include(i => i.Cari)
            .Include(i => i.Sube)
            .Include(i => i.AlisSiparisi)
            .Include(i => i.Kalemler)
            .Where(i => !i.IsDeleted);

        var toplamKayit = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(genelArama))
        {
            query = query.Where(i => EF.Functions.ILike(i.Cari.Unvan, $"%{genelArama}%"));
        }

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(0)))
            query = query.Where(i => EF.Functions.ILike(i.IrsaliyeNo, $"%{sutunAramalari[0]}%"));

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(2)))
            query = query.Where(i => EF.Functions.ILike(i.Cari.Unvan, $"%{sutunAramalari[2]}%"));

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(3)))
            query = query.Where(i => EF.Functions.ILike(i.Sube.SubeAdi, $"%{sutunAramalari[3]}%"));

        if (!string.IsNullOrWhiteSpace(sutunAramalari.ElementAtOrDefault(5))
            && Enum.TryParse<BelgeDurum>(sutunAramalari[5], out var durumFiltre))
            query = query.Where(i => i.Durum == durumFiltre);

        var filtrelenmisKayit = await query.CountAsync();

        var azalan = siralamaYonu == "desc";
        query = siralamaSutunu switch
        {
            0 => azalan ? query.OrderByDescending(i => i.IrsaliyeNo) : query.OrderBy(i => i.IrsaliyeNo),
            2 => azalan ? query.OrderByDescending(i => i.Cari.Unvan) : query.OrderBy(i => i.Cari.Unvan),
            3 => azalan ? query.OrderByDescending(i => i.Sube.SubeAdi) : query.OrderBy(i => i.Sube.SubeAdi),
            5 => azalan ? query.OrderByDescending(i => i.Durum) : query.OrderBy(i => i.Durum),
            _ => azalan ? query.OrderByDescending(i => i.Tarih) : query.OrderBy(i => i.Tarih)
        };

        var kayitlar = await query.Skip(start).Take(length).ToListAsync();
        return (kayitlar, toplamKayit, filtrelenmisKayit);
    }

    public async Task<AlisIrsaliyesi?> GetByIdDetayAsync(int id)
    {
        return await _unitOfWork.Repository<AlisIrsaliyesi>().QueryTumu()
            .Include(i => i.Cari)
            .Include(i => i.Sube)
            .Include(i => i.AlisSiparisi)
            .Include(i => i.Kalemler).ThenInclude(k => k.Malzeme)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task OnaylaAsync(int id)
    {
        var irsaliye = await _unitOfWork.Repository<AlisIrsaliyesi>().QueryTumu()
            .Include(i => i.Kalemler)
            .FirstOrDefaultAsync(i => i.Id == id)
            ?? throw new InvalidOperationException("Alış irsaliyesi bulunamadı.");

        if (irsaliye.Durum != BelgeDurum.Beklemede)
            throw new InvalidOperationException("Sadece beklemede olan irsaliyeler onaylanabilir.");

        var malzemeIdleri = irsaliye.Kalemler.Select(k => k.MalzemeId).Distinct().ToList();
        var malzemeler = await _unitOfWork.Repository<Malzeme>().QueryTumu()
            .Where(m => malzemeIdleri.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id);

        foreach (var kalem in irsaliye.Kalemler)
        {
            malzemeler[kalem.MalzemeId].Bakiye += kalem.Miktar;
        }

        irsaliye.Durum = BelgeDurum.Onaylandi;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task IptalEtAsync(int id)
    {
        var irsaliye = await _unitOfWork.Repository<AlisIrsaliyesi>().GetByIdAsync(id)
            ?? throw new InvalidOperationException("Alış irsaliyesi bulunamadı.");

        if (irsaliye.Durum != BelgeDurum.Beklemede)
            throw new InvalidOperationException("Sadece beklemede olan irsaliyeler iptal edilebilir.");

        irsaliye.Durum = BelgeDurum.Iptal;
        await _unitOfWork.SaveChangesAsync();
    }
}
