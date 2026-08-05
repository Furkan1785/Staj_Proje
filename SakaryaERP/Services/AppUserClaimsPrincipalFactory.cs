using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SakaryaERP.Models;

namespace SakaryaERP.Services;

// AppUser.SubeId'yi login cookie'sindeki claim'lere ekler ki şube bazlı erişim
// kontrolü (IOnayYetkisiService.SubeErisimVarMi) her istekte veritabanına gitmeden
// çalışabilsin. Kullanıcının şubesi değişirse, SecurityStampValidator'ın periyodik
// yeniden doğrulaması (bkz. Program.cs ValidationInterval) bu claim'i de tazeler.
public class AppUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<AppUser, AppRole>
{
    public AppUserClaimsPrincipalFactory(
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager,
        IOptions<IdentityOptions> options)
        : base(userManager, roleManager, options)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(AppUser user)
    {
        var kimlik = await base.GenerateClaimsAsync(user);
        if (user.SubeId is not null)
            kimlik.AddClaim(new Claim("SubeId", user.SubeId.Value.ToString()));
        return kimlik;
    }
}
