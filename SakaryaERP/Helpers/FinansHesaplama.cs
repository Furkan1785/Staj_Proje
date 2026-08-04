using SakaryaERP.Models;

namespace SakaryaERP.Helpers;

// Fiyatlı kalem satır toplamı hesabı; daha önce Alış/Satış Faturası, Sipariş, Teklif ve
// rapor/dashboard katmanlarında ~8 ayrı yerde (bazısı yuvarlamalı, bazısı yuvarlamasız)
// tekrarlanıyordu — ekranda gösterilen tutarla cariye/muhasebeye işlenen tutarın
// sapmaması için tek yerde toplandı.
public static class FinansHesaplama
{
    // Alış Faturası/Siparişi, Satış Siparişi/Teklifi: tek adımda yuvarlanan basit satır toplamı.
    public static decimal SatirToplami(IFiyatliKalem kalem) =>
        Math.Round(kalem.Miktar * kalem.BirimFiyat * (1 - kalem.Iskonto / 100) * (1 + kalem.KdvOrani / 100), 2);

    // Satış Faturası: ara toplam/iskonto/KDV ayrı ayrı gösterildiği için (PDF ve Detay sayfası)
    // her ara adım kendi içinde yuvarlanır — "ara toplam - iskonto + KDV" ekrandaki genel
    // toplamla birebir tutsun diye backend de aynı zinciri kullanır.
    public static decimal SatisFaturasiSatirToplami(IFiyatliKalem kalem)
    {
        var iskontoOncesi = kalem.Miktar * kalem.BirimFiyat;
        var iskontoTutari = Math.Round(iskontoOncesi * kalem.Iskonto / 100, 2);
        var iskontolu = iskontoOncesi - iskontoTutari;
        return Math.Round(iskontolu * (1 + kalem.KdvOrani / 100), 2);
    }

    // Alış Faturası kalemi başına KDV tutarı (İndirilecek KDV raporu için).
    public static decimal KdvTutari(IFiyatliKalem kalem) =>
        Math.Round(kalem.Miktar * kalem.BirimFiyat * (1 - kalem.Iskonto / 100) * kalem.KdvOrani / 100, 2);

    // Satış Faturası kalemi başına KDV tutarı (Hesaplanan KDV raporu için) — SatisFaturasiSatirToplami
    // ile aynı ara toplam/iskonto zincirini kullanır, KDV üstüne KDV binmesin diye.
    public static decimal SatisFaturasiKdvTutari(IFiyatliKalem kalem)
    {
        var iskontoOncesi = kalem.Miktar * kalem.BirimFiyat;
        var iskontoTutari = Math.Round(iskontoOncesi * kalem.Iskonto / 100, 2);
        var iskontolu = iskontoOncesi - iskontoTutari;
        return Math.Round(iskontolu * kalem.KdvOrani / 100, 2);
    }
}
