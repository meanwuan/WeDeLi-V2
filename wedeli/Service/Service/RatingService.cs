using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using wedeli.Models.Domain;
using wedeli.Models.DTO;
using wedeli.Models.DTO.Rating;
using wedeli.Repositories.Interface;
using wedeli.Service.Interface;
using Microsoft.Extensions.Logging;

namespace wedeli.Service.Service
{
    public class RatingService : IRatingService
    {
        private readonly IRatingRepository _ratingRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<RatingService> _logger;

        public RatingService(
            IRatingRepository ratingRepository,
            IMapper mapper,
            ILogger<RatingService> logger)
        {
            _ratingRepository = ratingRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<RatingResponseDto> CreateRatingAsync(CreateRatingDto dto)
        {
            try
            {
                var rating = _mapper.Map<Rating>(dto);
                rating.CreatedAt = DateTime.UtcNow;

                var createdRating = await _ratingRepository.AddAsync(rating);

                return _mapper.Map<RatingResponseDto>(createdRating);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating rating");
                throw;
            }
        }

        public async Task<RatingResponseDto> GetRatingByIdAsync(int ratingId)
        {
            try
            {
                var rating = await _ratingRepository.GetByIdAsync(ratingId);
                return _mapper.Map<RatingResponseDto>(rating);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rating: {RatingId}", ratingId);
                throw;
            }
        }

        public async Task<IEnumerable<RatingResponseDto>> GetRatingsByOrderAsync(int orderId)
        {
            try
            {
                var ratings = await _ratingRepository.GetAllAsync();
                var filtered = ratings.Where(r => r.OrderId == orderId);
                return _mapper.Map<IEnumerable<RatingResponseDto>>(filtered);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ratings for order: {OrderId}", orderId);
                throw;
            }
        }

        public async Task<IEnumerable<RatingResponseDto>> GetRatingsByDriverAsync(int driverId)
        {
            try
            {
                var ratings = await _ratingRepository.GetAllAsync();
                var filtered = ratings.Where(r => r.DriverId == driverId);
                return _mapper.Map<IEnumerable<RatingResponseDto>>(filtered);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ratings for driver: {DriverId}", driverId);
                throw;
            }
        }

        public async Task<decimal> GetAverageDriverRatingAsync(int driverId)
        {
            try
            {
                var ratings = await _ratingRepository.GetAllAsync();
                var driverRatings = ratings.Where(r => r.DriverId == driverId).ToList();
                
                if (!driverRatings.Any())
                    return 0m;

                return (decimal)(driverRatings.Average(r => r.RatingScore ?? 0));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting average rating for driver: {DriverId}", driverId);
                throw;
            }
        }

        public async Task<DriverRatingSummaryDto> GetDriverRatingSummaryAsync(int driverId)
        {
            try
            {
                var ratings = await _ratingRepository.GetAllAsync();
                var driverRatings = ratings.Where(r => r.DriverId == driverId).ToList();

                var averageScore = driverRatings.Count > 0 
                        ? (double)driverRatings.Sum(r => r.RatingScore ?? 0) / driverRatings.Count 
                        : 0d;

                return new DriverRatingSummaryDto
                {
                    DriverId = driverId,
                    TotalRatings = driverRatings.Count,
                    AverageRating = averageScore,
                    FiveStarCount = driverRatings.Count(r => r.RatingScore == 5),
                    FourStarCount = driverRatings.Count(r => r.RatingScore == 4),
                    ThreeStarCount = driverRatings.Count(r => r.RatingScore == 3),
                    TwoStarCount = driverRatings.Count(r => r.RatingScore == 2),
                    OneStarCount = driverRatings.Count(r => r.RatingScore == 1)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting driver rating summary: {DriverId}", driverId);
                throw;
            }
        }

        public async Task<IEnumerable<RatingResponseDto>> GetRatingsByCustomerAsync(int customerId)
        {
            try
            {
                var ratings = await _ratingRepository.GetAllAsync();
                var filtered = ratings.Where(r => r.CustomerId == customerId);
                return _mapper.Map<IEnumerable<RatingResponseDto>>(filtered);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ratings for customer: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<bool> UpdateRatingAsync(int ratingId, UpdateRatingDto dto)
        {
            try
            {
                var rating = await _ratingRepository.GetByIdAsync(ratingId);
                if (rating == null)
                    return false;

                _mapper.Map(dto, rating);
                rating.CreatedAt = DateTime.UtcNow;

                await _ratingRepository.UpdateAsync(rating);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating rating: {RatingId}", ratingId);
                throw;
            }
        }

        public async Task<bool> DeleteRatingAsync(int ratingId)
        {
            try
            {
                var rating = await _ratingRepository.GetByIdAsync(ratingId);
                if (rating == null)
                    return false;

                return await _ratingRepository.DeleteAsync(ratingId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting rating: {RatingId}", ratingId);
                throw;
            }
        }
    }
}
