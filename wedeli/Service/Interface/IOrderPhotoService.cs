using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.DTO.Order;
using wedeli.Models.DTO.Common;

namespace wedeli.Service.Interface
{
    /// <summary>
    /// Order photo service interface
    /// </summary>
    public interface IOrderPhotoService
    {
        Task<IEnumerable<OrderPhotoDto>> GetOrderPhotosAsync(int orderId);
        Task<IEnumerable<OrderPhotoDto>> GetPhotosByTypeAsync(int orderId, string photoType);
        Task<OrderPhotoDto> UploadPhotoAsync(UploadOrderPhotoDto dto);
        Task<bool> DeletePhotoAsync(int photoId);
    }
}
