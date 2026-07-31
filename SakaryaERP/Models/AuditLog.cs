namespace SakaryaERP.Models;

// Diğer entity'lerin aksine BaseEntity'den türemiyor: bir denetim kaydının
// kendisi güncellenmez/silinmez, sadece CreatedAt (Tarih) ve Id yeterli.
public class AuditLog
{
    public int Id { get; set; }
    public DateTime Tarih { get; set; }
    public string EntityAdi { get; set; } = "";
    public int EntityId { get; set; }
    public string AlanAdi { get; set; } = "";
    public string? EskiDeger { get; set; }
    public string? YeniDeger { get; set; }
    public string? KullaniciAdi { get; set; }
}
