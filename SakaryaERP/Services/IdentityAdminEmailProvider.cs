using Microsoft.AspNetCore.Identity;
using SakaryaERP.Models;

namespace SakaryaERP.Services;

public class IdentityAdminEmailProvider : IAdminEmailProvider
{
    private readonly UserManager<AppUser> _userManager;

    public IdentityAdminEmailProvider(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<List<string>> AdminEpostalariniGetirAsync()
    {
        var adminler = await _userManager.GetUsersInRoleAsync("Admin");
        return adminler.Where(a => !string.IsNullOrWhiteSpace(a.Email)).Select(a => a.Email!).ToList();
    }
}
