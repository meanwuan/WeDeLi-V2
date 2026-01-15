using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using wedeli.Models.Domain;
using wedeli.Models.DTO.Order;
using wedeli.Models.DTO.Common;
using wedeli.Repositories.Interface;
using wedeli.Service.Interface;

namespace wedeli.Service.Service
{
    /// <summary>
    /// Order photo service for managing order photos
    /// </summary>
    public class OrderPhotoService : IOrderPhotoService
    {
        private readonly IOrderPhotoRepository _photoRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderPhotoService> _logger;

        public OrderPhotoService(
            IOrderPhotoRepository photoRepository,
            IOrderRepository orderRepository,
            IMapper mapper,
            ILogger<OrderPhotoService> logger)
        {
            _photoRepository = photoRepository;
            _orderRepository = orderRepository;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get all photos for an order
        /// </summary>
        public async Task<IEnumerable<OrderPhotoDto>> GetOrderPhotosAsync(int orderId)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                    throw new KeyNotFoundException($"Order {orderId} not found");

                var photos = await _photoRepository.GetByOrderIdAsync(orderId);
                var photoDtos = _mapper.Map<IEnumerable<OrderPhotoDto>>(photos);
                
                _logger.LogInformation("Retrieved photos for order: {OrderId}", orderId);
                return photoDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving photos for order: {OrderId}", orderId);
                throw;
            }
        }

        /// <summary>
        /// Get photos by type
        /// </summary>
        public async Task<IEnumerable<OrderPhotoDto>> GetPhotosByTypeAsync(int orderId, string photoType)
        {
            try
            {
                if (string.IsNullOrEmpty(photoType))
                    throw new ArgumentNullException(nameof(photoType));

                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                    throw new KeyNotFoundException($"Order {orderId} not found");

                var photos = await _photoRepository.GetByTypeAsync(orderId, photoType);
                var photoDtos = _mapper.Map<IEnumerable<OrderPhotoDto>>(photos);
                
                _logger.LogInformation("Retrieved photos by type - OrderId: {OrderId}, Type: {PhotoType}", orderId, photoType);
                return photoDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving photos by type - OrderId: {OrderId}, Type: {PhotoType}", orderId, photoType);
                throw;
            }
        }

        /// <summary>
        /// Upload photo for order
        /// </summary>
        public async Task<OrderPhotoDto> UploadPhotoAsync(UploadOrderPhotoDto dto)
        {
            try
            {
                if (dto == null)
                    throw new ArgumentNullException(nameof(dto));

                var order = await _orderRepository.GetByIdAsync(dto.OrderId);
                if (order == null)
                    throw new KeyNotFoundException($"Order {dto.OrderId} not found");

                var photo = await _photoRepository.UploadPhotoAsync(
                    dto.OrderId,
                    dto.PhotoType,
                    dto.PhotoUrl,
                    dto.FileName,
                    dto.UploadedBy);

                var photoDto = _mapper.Map<OrderPhotoDto>(photo);
                
                _logger.LogInformation("Uploaded photo for order - OrderId: {OrderId}, Type: {PhotoType}", dto.OrderId, dto.PhotoType);
                return photoDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading photo for order: {OrderId}", dto?.OrderId);
                throw;
            }
        }

        /// <summary>
        /// Delete photo
        /// </summary>
        public async Task<bool> DeletePhotoAsync(int photoId)
        {
            try
            {
                var result = await _photoRepository.DeletePhotoAsync(photoId);
                
                _logger.LogInformation("Deleted photo: {PhotoId}", photoId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting photo: {PhotoId}", photoId);
                throw;
            }
        }
    }
}
