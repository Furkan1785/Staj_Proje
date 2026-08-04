namespace SakaryaERP.Models;

// Alış/Satış Faturası, Sipariş ve Teklif kalemlerinin ortak fiyatlandırma alanları.
// Satır toplamı formülü (Miktar*BirimFiyat*(1-Iskonto/100)*(1+KdvOrani/100)) bu arayüz
// üzerinden Helpers/FinansHesaplama.SatirToplami'de tek yerde hesaplanır.
public interface IFiyatliKalem
{
    decimal Miktar { get; }
    decimal BirimFiyat { get; }
    decimal Iskonto { get; }
    decimal KdvOrani { get; }
}
