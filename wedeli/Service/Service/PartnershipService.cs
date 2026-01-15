using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using wedeli.Models.Domain;
using wedeli.Models.DTO.Company;
using wedeli.Models.DTO.Partnership;
using wedeli.Models.DTO.Common;
using wedeli.Repositories.Interface;
using wedeli.Service.Interface;

namespace wedeli.Service.Service
{
    /// <summary>
    /// Partnership service for managing company partnerships
    /// </summary>
    public class PartnershipService : IPartnershipService
    {
        private readonly ICompanyPartnershipRepository _partnershipRepository;
        private readonly ITransportCompanyRepository _companyRepository;
        private readonly IOrderTransferRepository _transferRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<PartnershipService> _logger;

        public PartnershipService(
            ICompanyPartnershipRepository partnershipRepository,
            ITransportCompanyRepository companyRepository,
            IOrderTransferRepository transferRepository,
            IMapper mapper,
            ILogger<PartnershipService> logger)
        {
            _partnershipRepository = partnershipRepository;
            _companyRepository = companyRepository;
            _transferRepository = transferRepository;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get partnership by ID
        /// </summary>
        public async Task<PartnershipResponseDto> GetPartnershipByIdAsync(int partnershipId)
        {
            try
            {
                var partnership = await _partnershipRepository.GetByIdAsync(partnershipId);
                var partnershipDto = _mapper.Map<PartnershipResponseDto>(partnership);

                _logger.LogInformation("Retrieved partnership: {PartnershipId}", partnershipId);
                return partnershipDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving partnership: {PartnershipId}", partnershipId);
                throw;
            }
        }

        /// <summary>
        /// Get company partnerships
        /// </summary>
        public async Task<IEnumerable<PartnershipResponseDto>> GetCompanyPartnershipsAsync(int companyId)
        {
            try
            {
                var partnerships = await _partnershipRepository.GetByCompanyIdAsync(companyId);
                var partnershipDtos = _mapper.Map<IEnumerable<PartnershipResponseDto>>(partnerships);

                _logger.LogInformation("Retrieved {Count} partnerships for company: {CompanyId}", partnerships.Count(), companyId);
                return partnershipDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving company partnerships: {CompanyId}", companyId);
                throw;
            }
        }

        /// <summary>
        /// Get partners by level
        /// </summary>
        public async Task<IEnumerable<PartnershipResponseDto>> GetPartnersByLevelAsync(int companyId, string level)
        {
            try
            {
                var partnerships = await _partnershipRepository.GetPartnersByLevelAsync(companyId, level);
                var partnershipDtos = _mapper.Map<IEnumerable<PartnershipResponseDto>>(partnerships);

                _logger.LogInformation("Retrieved {Count} {Level} partners for company: {CompanyId}", 
                    partnerships.Count(), level, companyId);
                return partnershipDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving partners by level: {CompanyId}, {Level}", companyId, level);
                throw;
            }
        }

        /// <summary>
        /// Get preferred partners
        /// </summary>
        public async Task<IEnumerable<PartnershipResponseDto>> GetPreferredPartnersAsync(int companyId)
        {
            try
            {
                var partnerships = await _partnershipRepository.GetPreferredPartnersAsync(companyId);
                var partnershipDtos = _mapper.Map<IEnumerable<PartnershipResponseDto>>(partnerships);

                _logger.LogInformation("Retrieved {Count} preferred partners for company: {CompanyId}", 
                    partnerships.Count(), companyId);
                return partnershipDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving preferred partners: {CompanyId}", companyId);
                throw;
            }
        }

        /// <summary>
        /// Create partnership
        /// </summary>
        public async Task<PartnershipResponseDto> CreatePartnershipAsync(CreatePartnershipDto dto)
        {
            try
            {
                if (dto == null)
                    throw new ArgumentNullException(nameof(dto));

                if (dto.CompanyId == dto.PartnerCompanyId)
                    throw new InvalidOperationException("Company cannot be a partner with itself");

                // Verify both companies exist
                var company = await _companyRepository.GetByIdAsync(dto.CompanyId);
                if (company == null)
                    throw new KeyNotFoundException($"Company not found with ID: {dto.CompanyId}");

                var partnerCompany = await _companyRepository.GetByIdAsync(dto.PartnerCompanyId);
                if (partnerCompany == null)
                    throw new KeyNotFoundException($"Partner company not found with ID: {dto.PartnerCompanyId}");

                var partnership = _mapper.Map<CompanyPartnership>(dto);
                var createdPartnership = await _partnershipRepository.AddAsync(partnership);
                var partnershipDto = _mapper.Map<PartnershipResponseDto>(createdPartnership);

                _logger.LogInformation("Created partnership: {PartnershipId} between companies {CompanyId} and {PartnerCompanyId}",
                    createdPartnership.PartnershipId, dto.CompanyId, dto.PartnerCompanyId);
                return partnershipDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating partnership");
                throw;
            }
        }

        /// <summary>
        /// Update partnership
        /// </summary>
        public async Task<PartnershipResponseDto> UpdatePartnershipAsync(int partnershipId, UpdatePartnershipDto dto)
        {
            try
            {
                if (dto == null)
                    throw new ArgumentNullException(nameof(dto));

                var partnership = await _partnershipRepository.GetByIdAsync(partnershipId);
                if (partnership == null)
                    throw new KeyNotFoundException($"Partnership not found with ID: {partnershipId}");

                _mapper.Map(dto, partnership);
                var updatedPartnership = await _partnershipRepository.UpdateAsync(partnership);
                var partnershipDto = _mapper.Map<PartnershipResponseDto>(updatedPartnership);

                _logger.LogInformation("Updated partnership: {PartnershipId}", partnershipId);
                return partnershipDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating partnership: {PartnershipId}", partnershipId);
                throw;
            }
        }

        /// <summary>
        /// Delete partnership
        /// </summary>
        public async Task<bool> DeletePartnershipAsync(int partnershipId)
        {
            try
            {
                var result = await _partnershipRepository.DeleteAsync(partnershipId);

                _logger.LogInformation("Deleted partnership: {PartnershipId}", partnershipId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting partnership: {PartnershipId}", partnershipId);
                throw;
            }
        }

        /// <summary>
        /// Update priority
        /// </summary>
        public async Task<bool> UpdatePriorityAsync(int partnershipId, int priority)
        {
            try
            {
                var result = await _partnershipRepository.UpdatePriorityAsync(partnershipId, priority);

                _logger.LogInformation("Updated partnership {PartnershipId} priority to: {Priority}", partnershipId, priority);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating priority: {PartnershipId}", partnershipId);
                throw;
            }
        }

        /// <summary>
        /// Update commission rate
        /// </summary>
        public async Task<bool> UpdateCommissionRateAsync(int partnershipId, decimal rate)
        {
            try
            {
                var result = await _partnershipRepository.UpdateCommissionRateAsync(partnershipId, rate);

                _logger.LogInformation("Updated partnership {PartnershipId} commission rate to: {Rate}%", partnershipId, rate);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating commission rate: {PartnershipId}", partnershipId);
                throw;
            }
        }
    }
}
