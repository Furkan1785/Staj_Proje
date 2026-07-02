using Microsoft.AspNetCore.Identity;

namespace SakaryaERP.Models;

public class AppRole : IdentityRole
{
    public string? Aciklama { get; set; }
}