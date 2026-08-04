namespace SakaryaERP.Models;

public class SatisFaturasiKalemi : BaseEntity, IFiyatliKalem
{
    public int SatisFaturasiId { get; set; }
    public int MalzemeId { get; set; }
    public decimal Miktar { get; set; }
    public decimal BirimFiyat { get; set; }
    public decimal KdvOrani { get; set; }
    public decimal Iskonto { get; set; }

    // Fatura onaylandığı andaki Malzeme.AlisFiyati'nin anlık görüntüsü (COGS/Brüt Kar
    // hesabı için) — Malzeme.AlisFiyati sonradan değişse bile geçmiş faturanın maliyeti
    // sabit kalsın diye onay anında buraya kopyalanır.
    public decimal BirimMaliyet { get; set; }

    public SatisFaturasi SatisFaturasi { get; set; } = null!;
    public Malzeme Malzeme { get; set; } = null!;
}