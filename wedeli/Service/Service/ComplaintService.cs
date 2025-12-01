using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using wedeli.Models.Domain;
using wedeli.Models.DTO;
using wedeli.Models.DTO.Complaint;
using wedeli.Repositories.Interface;
using wedeli.Service.Interface;
using Microsoft.Extensions.Logging;

namespace wedeli.Service.Service
{
    public class ComplaintService : IComplaintService
    {
        private readonly IComplaintRepository _complaintRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ComplaintService> _logger;

        public ComplaintService(
            IComplaintRepository complaintRepository,
            IMapper mapper,
            ILogger<ComplaintService> logger)
        {
            _complaintRepository = complaintRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ComplaintResponseDto> CreateComplaintAsync(CreateComplaintDto dto)
        {
            try
            {
                var complaint = _mapper.Map<Complaint>(dto);
                complaint.CreatedAt = DateTime.UtcNow;
                complaint.ComplaintStatus = "pending";

                var createdComplaint = await _complaintRepository.AddAsync(complaint);

                _logger.LogInformation("Complaint created for order: {OrderId}", dto.OrderId);
                return _mapper.Map<ComplaintResponseDto>(createdComplaint);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating complaint");
                throw;
            }
        }

        public async Task<ComplaintResponseDto> GetComplaintByIdAsync(int complaintId)
        {
            try
            {
                var complaint = await _complaintRepository.GetByIdAsync(complaintId);
                return _mapper.Map<ComplaintResponseDto>(complaint);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting complaint: {ComplaintId}", complaintId);
                throw;
            }
        }

        public async Task<IEnumerable<ComplaintResponseDto>> GetComplaintsByOrderAsync(int orderId)
        {
            try
            {
                var complaints = await _complaintRepository.GetAllAsync();
                var filtered = complaints.Where(c => c.OrderId == orderId);
                return _mapper.Map<IEnumerable<ComplaintResponseDto>>(filtered);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting complaints for order: {OrderId}", orderId);
                throw;
            }
        }

        public async Task<IEnumerable<ComplaintResponseDto>> GetComplaintsByCustomerAsync(int customerId)
        {
            try
            {
                var complaints = await _complaintRepository.GetAllAsync();
                var filtered = complaints.Where(c => c.CustomerId == customerId);
                return _mapper.Map<IEnumerable<ComplaintResponseDto>>(filtered);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting complaints for customer: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<IEnumerable<ComplaintResponseDto>> GetComplaintsByStatusAsync(string status, int? companyId)
        {
            try
            {
                var complaints = await _complaintRepository.GetAllAsync();
                var filtered = complaints.Where(c => c.ComplaintStatus == status);

                return _mapper.Map<IEnumerable<ComplaintResponseDto>>(filtered);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting complaints by status: {Status}", status);
                throw;
            }
        }

        public async Task<IEnumerable<ComplaintResponseDto>> GetPendingComplaintsAsync(int? companyId)
        {
            try
            {
                var complaints = await _complaintRepository.GetAllAsync();
                var filtered = complaints.Where(c => c.ComplaintStatus == "pending");

                return _mapper.Map<IEnumerable<ComplaintResponseDto>>(filtered);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending complaints");
                throw;
            }
        }

        public async Task<bool> ResolveComplaintAsync(ResolveComplaintDto dto)
        {
            try
            {
                var complaint = await _complaintRepository.GetByIdAsync(dto.ComplaintId);
                if (complaint == null)
                    return false;

                complaint.ComplaintStatus = "resolved";
                complaint.ResolutionNotes = "";
                complaint.ResolvedAt = DateTime.UtcNow;

                await _complaintRepository.UpdateAsync(complaint);

                _logger.LogInformation("Complaint resolved: {ComplaintId}", dto.ComplaintId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving complaint: {ComplaintId}", dto.ComplaintId);
                throw;
            }
        }

        public async Task<bool> RejectComplaintAsync(int complaintId, string reason, int rejectedBy)
        {
            try
            {
                var complaint = await _complaintRepository.GetByIdAsync(complaintId);
                if (complaint == null)
                    return false;

                complaint.ComplaintStatus = "rejected";
                complaint.ResolutionNotes = reason;
                complaint.ResolvedAt = DateTime.UtcNow;

                await _complaintRepository.UpdateAsync(complaint);

                _logger.LogInformation("Complaint rejected: {ComplaintId} by {RejectedBy}", complaintId, rejectedBy);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting complaint: {ComplaintId}", complaintId);
                throw;
            }
        }

        public async Task<bool> UpdateComplaintStatusAsync(int complaintId, string status, string? resolutionNotes = null, int? resolvedBy = null)
        {
            try
            {
                var complaint = await _complaintRepository.GetByIdAsync(complaintId);
                if (complaint == null)
                    return false;

                complaint.ComplaintStatus = status;
                if (!string.IsNullOrEmpty(resolutionNotes))
                    complaint.ResolutionNotes = resolutionNotes;
                
                if (status == "resolved")
                    complaint.ResolvedAt = DateTime.UtcNow;

                await _complaintRepository.UpdateAsync(complaint);

                _logger.LogInformation("Complaint status updated: {ComplaintId} to {Status}", complaintId, status);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating complaint status: {ComplaintId}", complaintId);
                throw;
            }
        }
    }
}
