using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using wedeli.Models.Domain;
using wedeli.Models.DTO;
using wedeli.Models.DTO.Company;
using wedeli.Repositories.Interface;
using wedeli.Service.Interface;
using Microsoft.Extensions.Logging;

namespace wedeli.Service.Service
{
    public class CompanyService : ITransportCompanyService
    {
        private readonly ITransportCompanyRepository _companyRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CompanyService> _logger;

        public CompanyService(
            ITransportCompanyRepository companyRepository,
            IMapper mapper,
            ILogger<CompanyService> logger)
        {
            _companyRepository = companyRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CompanyResponseDto> CreateCompanyAsync(CreateCompanyDto dto)
        {
            try
            {
                var company = _mapper.Map<TransportCompany>(dto);
                company.CreatedAt = DateTime.UtcNow;
                company.IsActive = true;
                company.Rating = 0;

                var createdCompany = await _companyRepository.AddAsync(company);

                _logger.LogInformation("Transport company created: {CompanyId}", createdCompany.CompanyId);
                return _mapper.Map<CompanyResponseDto>(createdCompany);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating transport company");
                throw;
            }
        }

        public async Task<CompanyResponseDto> GetCompanyByIdAsync(int companyId)
        {
            try
            {
                var company = await _companyRepository.GetByIdAsync(companyId);
                return _mapper.Map<CompanyResponseDto>(company);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transport company: {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<CompanyDetailDto> GetCompanyDetailAsync(int companyId)
        {
            try
            {
                var company = await _companyRepository.GetByIdAsync(companyId);
                if (company == null)
                    throw new KeyNotFoundException($"Company with ID {companyId} not found.");
                
                return _mapper.Map<CompanyDetailDto>(company);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting company detail: {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<IEnumerable<CompanyResponseDto>> GetAllCompaniesAsync()
        {
            try
            {
                var companies = await _companyRepository.GetAllAsync();
                return _mapper.Map<IEnumerable<CompanyResponseDto>>(companies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all transport companies");
                throw;
            }
        }

        public async Task<IEnumerable<CompanyResponseDto>> GetActiveCompaniesAsync()
        {
            try
            {
                var companies = await _companyRepository.GetActiveCompaniesAsync();
                return _mapper.Map<IEnumerable<CompanyResponseDto>>(companies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active transport companies");
                throw;
            }
        }

        public async Task<CompanyResponseDto> UpdateCompanyAsync(int companyId, UpdateCompanyDto dto)
        {
            try
            {
                var company = await _companyRepository.GetByIdAsync(companyId);
                if (company == null)
                    throw new KeyNotFoundException($"Company with ID {companyId} not found.");

                _mapper.Map(dto, company);

                var updated = await _companyRepository.UpdateAsync(company);
                _logger.LogInformation("Transport company updated: {CompanyId}", companyId);
                return _mapper.Map<CompanyResponseDto>(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating transport company: {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<bool> ToggleCompanyStatusAsync(int companyId, bool isActive)
        {
            try
            {
                var company = await _companyRepository.GetByIdAsync(companyId);
                if (company == null)
                    return false;

                company.IsActive = isActive;
                await _companyRepository.UpdateAsync(company);

                _logger.LogInformation("Company status toggled: {CompanyId}, IsActive: {IsActive}", companyId, isActive);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling company status: {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<CompanyStatisticsDto> GetCompanyStatisticsAsync(int companyId)
        {
            try
            {
                var company = await _companyRepository.GetByIdAsync(companyId);
                if (company == null)
                    throw new KeyNotFoundException($"Company with ID {companyId} not found.");

                return new CompanyStatisticsDto
                {
                    CompanyId = companyId,
                    CompanyName = company.CompanyName,
                    TotalOrders = 0,
                    CompletedOrders = 0,
                    TotalRevenue = 0,
                    ActiveVehicles = 0,
                    ActiveDrivers = 0,
                    SuccessRate = 0,
                    AverageRating = (double)(company.Rating ?? 0)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting company statistics: {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<bool> UpdateCompanyRatingAsync(int companyId)
        {
            try
            {
                var company = await _companyRepository.GetByIdAsync(companyId);
                if (company == null)
                    return false;

                await _companyRepository.UpdateAsync(company);

                _logger.LogInformation("Company rating updated: {CompanyId}", companyId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating company rating: {CompanyId}", companyId);
                throw;
            }
        }
    }
}
