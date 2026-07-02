namespace SakaryaERP.Models;

public class Sube : BaseEntity
{
    public string SubeAdi { get; set; } = "";
    public string? Adres { get; set; }

    public ICollection<AppUser> Kullanicilar { get; set; } = new List<AppUser>();
    public ICollection<MalzemeHareketFisi> MalzemeHareketFisleri { get; set; } = new List<MalzemeHareketFisi>();
    public ICollection<AlisSiparisi> AlisSiparisleri { get; set; } = new List<AlisSiparisi>();
    public ICollection<AlisIrsaliyesi> AlisIrsaliyeleri { get; set; } = new List<AlisIrsaliyesi>();
    public ICollection<SevkIrsaliyesi> SevkIrsaliyeleri { get; set; } = new List<SevkIrsaliyesi>();
}