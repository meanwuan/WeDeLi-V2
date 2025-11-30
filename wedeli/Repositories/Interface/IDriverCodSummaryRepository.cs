using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.Domain;

namespace wedeli.Repositories.Interface
{
    public interface IDriverCodSummaryRepository : IBaseRepository<DriverCodSummary>
    {
        Task<DriverCodSummary> GetByDriverAndDateAsync(int driverId, DateTime date);
        Task<IEnumerable<DriverCodSummary>> GetByDriverIdAsync(int driverId, DateTime? startDate = null, DateTime? endDate = null);
        Task<bool> UpdateSummaryAsync(int driverId, DateTime date);
        Task<bool> ReconcileAsync(int summaryId, int reconciledBy);
        Task<IEnumerable<DriverCodSummary>> GetPendingReconciliationsAsync(int? companyId = null);
    }
}
