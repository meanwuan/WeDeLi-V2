using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.DTO.Rating;
using wedeli.Models.DTO.Common;

namespace wedeli.Service.Interface
{
    /// <summary>
    /// Rating service interface
    /// </summary>
    public interface IRatingService
    {
        Task<RatingResponseDto> GetRatingByIdAsync(int ratingId);
        Task<IEnumerable<RatingResponseDto>> GetRatingsByOrderAsync(int orderId);
        Task<IEnumerable<RatingResponseDto>> GetRatingsByDriverAsync(int driverId);
        Task<IEnumerable<RatingResponseDto>> GetRatingsByCustomerAsync(int customerId);
        
        Task<RatingResponseDto> CreateRatingAsync(CreateRatingDto dto);
        Task<bool> UpdateRatingAsync(int ratingId, UpdateRatingDto dto);
        Task<bool> DeleteRatingAsync(int ratingId);
        
        Task<decimal> GetAverageDriverRatingAsync(int driverId);
        Task<DriverRatingSummaryDto> GetDriverRatingSummaryAsync(int driverId);
    }
}