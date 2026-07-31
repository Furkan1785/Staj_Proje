using SakaryaERP.ViewModels;

namespace SakaryaERP.Services;

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardAsync(DashboardAralik aralik, IReadOnlyCollection<int> kategoriIdler);
}
