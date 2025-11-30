using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.Domain;

namespace wedeli.Repositories.Interface
{
    public interface IOrderPhotoRepository : IBaseRepository<OrderPhoto>
    {
        Task<IEnumerable<OrderPhoto>> GetByOrderIdAsync(int orderId);
        Task<IEnumerable<OrderPhoto>> GetByTypeAsync(int orderId, string photoType);
        Task<OrderPhoto> UploadPhotoAsync(int orderId, string photoType, string photoUrl, string fileName, int? uploadedBy = null);
        Task<bool> DeletePhotoAsync(int photoId);
    }
}
