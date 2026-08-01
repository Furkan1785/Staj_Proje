namespace SakaryaERP.Services;

public interface IBildirimService
{
    // Kritik stoktaki malzemeleri ve vadesi yaklaşan (Portföyde/Tahsilde) çek/senetleri
    // tek bir özet e-postada Admin rolündeki kullanıcılara gönderir. İkisi de boşsa
    // e-posta gönderilmez (gereksiz "her şey yolunda" maili atmıyoruz).
    Task<BildirimSonucu> KritikDurumBildirimGonderAsync();
}

public class BildirimSonucu
{
    public int KritikStokSayisi { get; set; }
    public int VadesiYaklasanCekSenetSayisi { get; set; }
    public int GonderilenEpostaSayisi { get; set; }
    public bool GonderildiMi => GonderilenEpostaSayisi > 0;
}
