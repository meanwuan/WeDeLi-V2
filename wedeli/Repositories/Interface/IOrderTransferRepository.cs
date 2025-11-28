using wedeli.Models.Domain;

namespace wedeli.Repositories.Interface
{
    /// <summary>
    /// Interface for Order Transfer repository operations.
    /// </summary>
    public interface IOrderTransferRepository
    {
        /// <summary>
        /// Creates a new order transfer record in the database.
        /// </summary>
        Task<OrderTransfer> CreateTransferAsync(OrderTransfer transfer);

        /// <summary>
        /// Retrieves a transfer by its unique ID, including related entities.
        /// </summary>
        Task<OrderTransfer?> GetTransferByIdAsync(int transferId);

        /// <summary>
        /// Retrieves all transfers (incoming and outgoing) for a specific company.
        /// </summary>
        Task<List<OrderTransfer>> GetCompanyTransfersAsync(int companyId, string? status = null);

        /// <summary>
        /// Retrieves all pending transfers for a specific company to accept or reject.
        /// </summary>
        Task<List<OrderTransfer>> GetPendingTransfersForCompanyAsync(int companyId);

        /// <summary>
        /// Marks a transfer as accepted.
        /// </summary>
        Task<bool> AcceptTransferAsync(int transferId, int acceptedByUserId, int? newVehicleId);

        /// <summary>
        /// Marks a transfer as rejected with a reason.
        /// </summary>
        Task<bool> RejectTransferAsync(int transferId, string rejectionReason);
    }
}
