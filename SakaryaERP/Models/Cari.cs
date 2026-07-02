namespace SakaryaERP.Models;

public class Cari : BaseEntity
{
    public string CariKodu { get; set; } = "";
    public string Unvan { get; set; } = "";
    public CariTipi CariTipi { get; set; }
    public string? VergiNo { get; set; }
    public string? Adres { get; set; }
    public string? Telefon { get; set; }
    public string? EMail { get; set; }
    public decimal Bakiye { get; set; }
    public decimal KrediLimiti { get; set; }

    public ICollection<CariFisi> CariFisleri { get; set; } = new List<CariFisi>();
    public ICollection<CekSenet> CekSenetler { get; set; } = new List<CekSenet>();
    public ICollection<MusteriTalebi> MusteriTalepleri { get; set; } = new List<MusteriTalebi>();
    public ICollection<SatisTeklifi> SatisTeklifleri { get; set; } = new List<SatisTeklifi>();
    public ICollection<SatisSiparisi> SatisSiparisleri { get; set; } = new List<SatisSiparisi>();
    public ICollection<SatisFaturasi> SatisFaturalari { get; set; } = new List<SatisFaturasi>();
    public ICollection<AlisSiparisi> AlisSiparisleri { get; set; } = new List<AlisSiparisi>();
    public ICollection<AlisIrsaliyesi> AlisIrsaliyeleri { get; set; } = new List<AlisIrsaliyesi>();
    public ICollection<AlisFaturasi> AlisFaturalari { get; set; } = new List<AlisFaturasi>();
}