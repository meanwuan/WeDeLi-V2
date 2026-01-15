using AutoMapper;
using Microsoft.Extensions.Logging;
using wedeli.Models.Domain;
using wedeli.Models.DTO;
using wedeli.Repositories.Interface;
using wedeli.Service.Interface;

namespace wedeli.Service.Service;

/// <summary>
/// Service implementation for CompanyCustomer pricing management
/// </summary>
public class CompanyCustomerService : ICompanyCustomerService
{
    private readonly ICompanyCustomerRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<CompanyCustomerService> _logger;

    public CompanyCustomerService(
        ICompanyCustomerRepository repository,
        IMapper mapper,
        ILogger<CompanyCustomerService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<CompanyCustomerResponseDto>> GetCompanyCustomersAsync(int companyId)
    {
        var customers = await _repository.GetByCompanyAsync(companyId);
        return _mapper.Map<IEnumerable<CompanyCustomerResponseDto>>(customers);
    }

    public async Task<IEnumerable<CompanyCustomerResponseDto>> GetVipCustomersAsync(int companyId)
    {
        var customers = await _repository.GetVipCustomersByCompanyAsync(companyId);
        return _mapper.Map<IEnumerable<CompanyCustomerResponseDto>>(customers);
    }

    public async Task<CompanyCustomerResponseDto?> GetByIdAsync(int companyCustomerId)
    {
        var customer = await _repository.GetByIdAsync(companyCustomerId);
        return customer != null ? _mapper.Map<CompanyCustomerResponseDto>(customer) : null;
    }

    public async Task<CompanyCustomerResponseDto> CreateOrUpdateAsync(int companyId, CompanyCustomerRequestDto request)
    {
        var existing = await _repository.GetByCompanyAndPhoneAsync(companyId, request.Phone);
        
        if (existing != null)
        {
            // Update existing
            existing.FullName = request.FullName;
            existing.Email = request.Email;
            existing.CustomPrice = request.CustomPrice;
            existing.DiscountPercent = request.DiscountPercent;
            existing.IsVip = request.IsVip;
            existing.Notes = request.Notes;
            existing.CustomerId = request.CustomerId;
            
            var updated = await _repository.UpdateAsync(existing);
            _logger.LogInformation("Updated CompanyCustomer {Id} for company {CompanyId}", 
                updated.CompanyCustomerId, companyId);
            
            return _mapper.Map<CompanyCustomerResponseDto>(updated);
        }
        else
        {
            // Create new
            var newCustomer = new CompanyCustomer
            {
                CompanyId = companyId,
                CustomerId = request.CustomerId,
                FullName = request.FullName,
                Phone = request.Phone,
                Email = request.Email,
                CustomPrice = request.CustomPrice,
                DiscountPercent = request.DiscountPercent,
                IsVip = request.IsVip,
                Notes = request.Notes,
                TotalOrders = 0,
                TotalRevenue = 0
            };
            
            var created = await _repository.CreateAsync(newCustomer);
            _logger.LogInformation("Created CompanyCustomer {Id} for company {CompanyId}, phone {Phone}", 
                created.CompanyCustomerId, companyId, request.Phone);
            
            return _mapper.Map<CompanyCustomerResponseDto>(created);
        }
    }

    public async Task<CompanyCustomerResponseDto> SetPricingAsync(int companyCustomerId, decimal? customPrice, decimal? discountPercent)
    {
        var customer = await _repository.GetByIdAsync(companyCustomerId);
        if (customer == null)
            throw new KeyNotFoundException($"CompanyCustomer {companyCustomerId} not found");
        
        customer.CustomPrice = customPrice;
        customer.DiscountPercent = discountPercent;
        
        var updated = await _repository.UpdateAsync(customer);
        _logger.LogInformation("Updated pricing for CompanyCustomer {Id}: CustomPrice={Price}, Discount={Discount}%", 
            companyCustomerId, customPrice, discountPercent);
        
        return _mapper.Map<CompanyCustomerResponseDto>(updated);
    }

    public async Task<CompanyCustomerResponseDto> SetVipStatusAsync(int companyCustomerId, bool isVip)
    {
        var customer = await _repository.GetByIdAsync(companyCustomerId);
        if (customer == null)
            throw new KeyNotFoundException($"CompanyCustomer {companyCustomerId} not found");
        
        customer.IsVip = isVip;
        
        var updated = await _repository.UpdateAsync(customer);
        _logger.LogInformation("Updated VIP status for CompanyCustomer {Id}: IsVip={IsVip}", 
            companyCustomerId, isVip);
        
        return _mapper.Map<CompanyCustomerResponseDto>(updated);
    }

    public async Task<decimal> CalculatePriceAsync(int companyId, string phone, decimal basePrice)
    {
        var customer = await _repository.GetByCompanyAndPhoneAsync(companyId, phone);
        return CalculatePrice(basePrice, customer);
    }

    public decimal CalculatePrice(decimal basePrice, CompanyCustomer? companyCustomer)
    {
        if (companyCustomer == null)
            return basePrice;
        
        // Priority 1: Use custom fixed price if set
        if (companyCustomer.CustomPrice.HasValue && companyCustomer.CustomPrice.Value > 0)
        {
            _logger.LogDebug("Using custom price {Price} for customer {Phone}", 
                companyCustomer.CustomPrice.Value, companyCustomer.Phone);
            return companyCustomer.CustomPrice.Value;
        }
        
        // Priority 2: Apply discount percentage if set
        if (companyCustomer.DiscountPercent.HasValue && companyCustomer.DiscountPercent.Value > 0)
        {
            var discountedPrice = basePrice * (1 - companyCustomer.DiscountPercent.Value / 100);
            _logger.LogDebug("Applied {Discount}% discount for customer {Phone}: {Base} -> {Final}", 
                companyCustomer.DiscountPercent.Value, companyCustomer.Phone, basePrice, discountedPrice);
            return Math.Round(discountedPrice, 0); // Round to whole number for VND
        }
        
        // No custom pricing, return base price
        return basePrice;
    }

    public async Task<bool> DeleteAsync(int companyCustomerId)
    {
        var result = await _repository.DeleteAsync(companyCustomerId);
        if (result)
        {
            _logger.LogInformation("Deleted CompanyCustomer {Id}", companyCustomerId);
        }
        return result;
    }

    public async Task UpdateOrderStatsAsync(int companyId, string phone, decimal orderAmount)
    {
        var customer = await _repository.GetByCompanyAndPhoneAsync(companyId, phone);
        if (customer == null)
        {
            _logger.LogDebug("No CompanyCustomer found for company {CompanyId}, phone {Phone}", companyId, phone);
            return;
        }
        
        customer.TotalOrders++;
        customer.TotalRevenue += orderAmount;
        
        await _repository.UpdateAsync(customer);
        _logger.LogInformation("Updated order stats for CompanyCustomer {Id}: Orders={Orders}, Revenue={Revenue}", 
            customer.CompanyCustomerId, customer.TotalOrders, customer.TotalRevenue);
    }
}
