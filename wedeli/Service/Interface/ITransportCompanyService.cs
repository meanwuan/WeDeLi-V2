using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Models.DTO.Company;
using wedeli.Models.DTO.Common;

namespace wedeli.Service.Interface
{
    /// <summary>
    /// Transport company service interface
    /// </summary>
    public interface ITransportCompanyService
    {
        Task<CompanyResponseDto> GetCompanyByIdAsync(int companyId);
        Task<CompanyDetailDto> GetCompanyDetailAsync(int companyId);
        Task<IEnumerable<CompanyResponseDto>> GetAllCompaniesAsync();
        Task<IEnumerable<CompanyResponseDto>> GetActiveCompaniesAsync();
        Task<CompanyResponseDto> CreateCompanyAsync(CreateCompanyDto dto);
        Task<CompanyResponseDto> UpdateCompanyAsync(int companyId, UpdateCompanyDto dto);
        Task<bool> ToggleCompanyStatusAsync(int companyId, bool isActive);
        Task<CompanyStatisticsDto> GetCompanyStatisticsAsync(int companyId);
        Task<bool> UpdateCompanyRatingAsync(int companyId);
    }
}
