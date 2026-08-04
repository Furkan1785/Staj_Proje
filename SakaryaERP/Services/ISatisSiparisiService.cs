using SakaryaERP.Models;

namespace SakaryaERP.Services;

public interface ISatisSiparisiService
{
    Task<SatisSiparisi> CreateAsync(SatisSiparisi siparis, List<SatisSiparisiKalemi> kalemler);

    Task<(IEnumerable<SatisSiparisi> Kayitlar, int ToplamKayit, int FiltrelenmisKayit)> GetSayfaliListeAsync(
        int start, int length, string? genelArama, string?[] sutunAramalari, int siralamaSutunu, string siralamaYonu);

    Task<SatisSiparisi?> GetByIdDetayAsync(int id);
    Task OnaylaAsync(int id);
    Task IptalEtAsync(int id);

    // Belge takip ekranı: teslim tarihi geçen/yaklaşan siparişlerin tespiti için.
    Task<List<SatisSiparisi>> GetBeklemedeListesiAsync();

    // Bir teklif için zaten iptal edilmemiş bir sipariş oluşturulmuş mu (tekrar dönüştürmeyi engellemek için).
    Task<bool> AktifSiparisVarMiTeklifIcinAsync(int satisTeklifiId);

    // Onaylanmış sevk irsaliyesi kalemlerinden malzeme bazında sevk edilen toplam miktarı hesaplar.
    Dictionary<int, decimal> SevkMiktarlariHesapla(SatisSiparisi siparis);
}
