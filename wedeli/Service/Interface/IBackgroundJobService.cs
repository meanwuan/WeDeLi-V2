using System;
using System.Threading.Tasks;

namespace wedeli.Service.Interface
{
    /// <summary>
    /// Background job service interface
    /// </summary>
    public interface IBackgroundJobService
    {
        Task ProcessPendingOrdersAsync();
        Task UpdateVehicleStatusAsync();
        Task GenerateDailyReportsAsync();
        Task SendScheduledNotificationsAsync();
        Task CleanupOldDataAsync();
        Task ReconcileDailyCODAsync();
        Task UpdateCustomerRegularStatusAsync();
        Task CheckOverdueInvoicesAsync();
    }
}
