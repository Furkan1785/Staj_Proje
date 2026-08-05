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
    public static decimal SatisFaturasiSatirToplami(IFiyatliKalem kalem) =>
        Math.Round(SatisFaturasiNetTutari(kalem) * (1 + kalem.KdvOrani / 100), 2);

    // İskonto uygulanmış, KDV hariç satır tutarı (Satış Faturası) — KDV ve Brüt Kar
    // hesaplarının ikisi de aynı ara adımdan türesin diye ortak noktaya çıkarıldı.
    public static decimal SatisFaturasiNetTutari(IFiyatliKalem kalem)
    {
        var iskontoOncesi = kalem.Miktar * kalem.BirimFiyat;
        var iskontoTutari = Math.Round(iskontoOncesi * kalem.Iskonto / 100, 2);
        return iskontoOncesi - iskontoTutari;
    }

    // Alış Faturası kalemi başına KDV tutarı (İndirilecek KDV raporu için).
    public static decimal KdvTutari(IFiyatliKalem kalem) =>
        Math.Round(kalem.Miktar * kalem.BirimFiyat * (1 - kalem.Iskonto / 100) * kalem.KdvOrani / 100, 2);

    // Satış Faturası kalemi başına KDV tutarı (Hesaplanan KDV raporu için) — SatisFaturasiSatirToplami
    // ile aynı ara toplam/iskonto zincirini kullanır, KDV üstüne KDV binmesin diye.
    public static decimal SatisFaturasiKdvTutari(IFiyatliKalem kalem) =>
        Math.Round(SatisFaturasiNetTutari(kalem) * kalem.KdvOrani / 100, 2);

    // Çift taraflı muhasebenin temel invariant'ı: bir yevmiye kaydında toplam Borç toplam
    // Alacak'a eşit olmalı. Hesap kodu bulunamayınca (hesaplar sözlüğünde eksik anahtar) ya da
    // ileride bir kalem eklenip karşılığı unutulursa bu sessizce dengesiz bir fiş üretmesin diye
    // savunma amaçlı — MuhasebeFisi kaydedilmeden hemen önce çağrılır.
    public static void BorcAlacakDengesiniDogrula(IEnumerable<MuhasebeFisiKalemi> kalemler)
    {
        var borc = kalemler.Sum(k => k.Borc);
        var alacak = kalemler.Sum(k => k.Alacak);
        if (Math.Abs(borc - alacak) > 0.01m)
            throw new InvalidOperationException(
                $"Yevmiye kaydı dengesiz: Borç ({borc.ToString("N2")}) Alacak'a ({alacak.ToString("N2")}) eşit değil.");
    }
}
