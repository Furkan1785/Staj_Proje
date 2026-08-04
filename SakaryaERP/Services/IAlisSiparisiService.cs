using SakaryaERP.Models;

namespace SakaryaERP.Services;

public interface IAlisSiparisiService
{
    Task<AlisSiparisi> CreateAsync(AlisSiparisi siparis, List<AlisSiparisiKalemi> kalemler);

    Task<(IEnumerable<AlisSiparisi> Kayitlar, int ToplamKayit, int FiltrelenmisKayit)> GetSayfaliListeAsync(
        int start, int length, string? genelArama, string?[] sutunAramalari, int siralamaSutunu, string siralamaYonu);

    Task<AlisSiparisi?> GetByIdDetayAsync(int id);
    Task OnaylaAsync(int id);
    Task IptalEtAsync(int id);

    // Belge takip ekranı: teslim tarihi geçen/yaklaşan siparişlerin tespiti için.
    Task<List<AlisSiparisi>> GetBeklemedeListesiAsync();

    // Onaylanmış alış irsaliyesi kalemlerinden malzeme bazında teslim alınan toplam miktarı hesaplar.
    Dictionary<int, decimal> TeslimMiktarlariHesapla(AlisSiparisi siparis);
}
