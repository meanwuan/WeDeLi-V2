using Microsoft.EntityFrameworkCore;
using wedeli.Models.Domain;
using wedeli.Models.Domain.Data;
using wedeli.Repositories.Interface;

namespace wedeli.Repositories.Repo;

/// <summary>
/// Repository implementation for CompanyCustomer
/// </summary>
public class CompanyCustomerRepository : ICompanyCustomerRepository
{
    private readonly AppDbContext _context;

    public CompanyCustomerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CompanyCustomer?> GetByIdAsync(int companyCustomerId)
    {
        return await _context.CompanyCustomers
            .FirstOrDefaultAsync(cc => cc.CompanyCustomerId == companyCustomerId);
    }

    public async Task<CompanyCustomer?> GetByCompanyAndCustomerAsync(int companyId, int customerId)
    {
        return await _context.CompanyCustomers
            .FirstOrDefaultAsync(cc => cc.CompanyId == companyId && cc.CustomerId == customerId);
    }

    public async Task<CompanyCustomer?> GetByCompanyAndPhoneAsync(int companyId, string phone)
    {
        return await _context.CompanyCustomers
            .FirstOrDefaultAsync(cc => cc.CompanyId == companyId && cc.Phone == phone);
    }

    public async Task<IEnumerable<CompanyCustomer>> GetByCompanyAsync(int companyId)
    {
        return await _context.CompanyCustomers
            .Where(cc => cc.CompanyId == companyId)
            .OrderByDescending(cc => cc.IsVip)
            .ThenByDescending(cc => cc.TotalOrders)
            .ToListAsync();
    }

    public async Task<IEnumerable<CompanyCustomer>> GetVipCustomersByCompanyAsync(int companyId)
    {
        return await _context.CompanyCustomers
            .Where(cc => cc.CompanyId == companyId && cc.IsVip)
            .OrderByDescending(cc => cc.TotalRevenue)
            .ToListAsync();
    }

    public async Task<CompanyCustomer> CreateAsync(CompanyCustomer companyCustomer)
    {
        companyCustomer.CreatedAt = DateTime.UtcNow;
        companyCustomer.UpdatedAt = DateTime.UtcNow;
        
        await _context.CompanyCustomers.AddAsync(companyCustomer);
        await _context.SaveChangesAsync();
        
        return companyCustomer;
    }

    public async Task<CompanyCustomer> UpdateAsync(CompanyCustomer companyCustomer)
    {
        companyCustomer.UpdatedAt = DateTime.UtcNow;
        
        _context.CompanyCustomers.Update(companyCustomer);
        await _context.SaveChangesAsync();
        
        return companyCustomer;
    }

    public async Task<bool> DeleteAsync(int companyCustomerId)
    {
        var entity = await GetByIdAsync(companyCustomerId);
        if (entity == null) return false;
        
        _context.CompanyCustomers.Remove(entity);
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> ExistsAsync(int companyId, string phone)
    {
        return await _context.CompanyCustomers
            .AnyAsync(cc => cc.CompanyId == companyId && cc.Phone == phone);
    }
}
