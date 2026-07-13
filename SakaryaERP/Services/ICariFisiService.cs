using SakaryaERP.Models;

namespace SakaryaERP.Services;

public interface ICariFisiService
{
    Task<CariFisi> CreateAsync(CariFisi fis);

    Task<(IEnumerable<CariFisi> Kayitlar, int ToplamKayit, int FiltrelenmisKayit)> GetSayfaliListeAsync(
        int start, int length, string? genelArama, string?[] sutunAramalari, int siralamaSutunu, string siralamaYonu);

    // Devir bakiyesi: tarih aralığı başlamadan önceki tüm hareketlerin toplamı.
    // Satırlar: aralık içindeki fişler, her biri kendinden önceki kümülatif bakiyeye eklenerek sıralı hesaplanır.
    Task<(Cari Cari, decimal DevirBakiye, List<(CariFisi Fis, decimal KumulatifBakiye)> Satirlar)> GetEkstreAsync(
        int cariId, DateTime baslangic, DateTime bitis);
}
