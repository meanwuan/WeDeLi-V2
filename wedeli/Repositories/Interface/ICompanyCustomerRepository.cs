using wedeli.Models.Domain;

namespace wedeli.Repositories.Interface;

/// <summary>
/// Repository interface for CompanyCustomer - manages customer-specific pricing per company
/// </summary>
public interface ICompanyCustomerRepository
{
    /// <summary>
    /// Get company customer by ID
    /// </summary>
    Task<CompanyCustomer?> GetByIdAsync(int companyCustomerId);
    
    /// <summary>
    /// Get company customer by company ID and customer ID
    /// </summary>
    Task<CompanyCustomer?> GetByCompanyAndCustomerAsync(int companyId, int customerId);
    
    /// <summary>
    /// Get company customer by company ID and phone number
    /// </summary>
    Task<CompanyCustomer?> GetByCompanyAndPhoneAsync(int companyId, string phone);
    
    /// <summary>
    /// Get all customers for a specific company
    /// </summary>
    Task<IEnumerable<CompanyCustomer>> GetByCompanyAsync(int companyId);
    
    /// <summary>
    /// Get VIP customers for a specific company
    /// </summary>
    Task<IEnumerable<CompanyCustomer>> GetVipCustomersByCompanyAsync(int companyId);
    
    /// <summary>
    /// Create a new company customer
    /// </summary>
    Task<CompanyCustomer> CreateAsync(CompanyCustomer companyCustomer);
    
    /// <summary>
    /// Update an existing company customer
    /// </summary>
    Task<CompanyCustomer> UpdateAsync(CompanyCustomer companyCustomer);
    
    /// <summary>
    /// Delete a company customer by ID
    /// </summary>
    Task<bool> DeleteAsync(int companyCustomerId);
    
    /// <summary>
    /// Check if a company customer exists
    /// </summary>
    Task<bool> ExistsAsync(int companyId, string phone);
}
