using Microsoft.AspNetCore.Identity;

namespace SakaryaERP.Models;

public class AppUser : IdentityUser
{
    public string AdSoyad { get; set; } = "";
    public int? SubeId { get; set; }
    public Sube? Sube { get; set; }
}