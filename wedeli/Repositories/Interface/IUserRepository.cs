using wedeli.Models.Domain;
using wedeli.Models.Domain.Data;
namespace wedeli.Repositories.Interface;
public interface IUserRepository
{

    /// <summary>
    /// Lấy user theo ID
    /// </summary>
    Task<User?> GetByIdAsync(int userId);
    
    /// <summary>
    /// Lấy user theo Username (cho login)
    /// </summary>
    Task<User?> GetByUsernameAsync(string username);
    
    /// <summary>
    /// Lấy user theo Phone
    /// </summary>
    Task<User?> GetByPhoneAsync(string phone);
    
    /// <summary>
    /// Lấy user theo Email (nếu có)
    /// </summary>
    Task<User?> GetByEmailAsync(string email);
    
    /// <summary>
    /// Lấy tất cả users (có phân trang)
    /// </summary>
    Task<List<User>> GetAllAsync(int page = 1, int pageSize = 50);
    
    /// <summary>
    /// Tạo user mới
    /// </summary>
    Task<User> CreateAsync(User user);
    
    /// <summary>
    /// Cập nhật user
    /// </summary>
    Task<User> UpdateAsync(User user);
    
    /// <summary>
    /// Xóa user (soft delete bằng is_active = false)
    /// </summary>
    Task<bool> DeleteAsync(int userId);
    
    // ========== VALIDATION METHODS ==========
    
    /// <summary>
    /// Kiểm tra username đã tồn tại chưa
    /// </summary>
    Task<bool> UsernameExistsAsync(string username);
    
    /// <summary>
    /// Kiểm tra phone đã tồn tại chưa
    /// </summary>
    Task<bool> PhoneExistsAsync(string phone);
    
    /// <summary>
    /// Kiểm tra email đã tồn tại chưa (nếu có)
    /// </summary>
    Task<bool> EmailExistsAsync(string email);
    
    // ========== QUERY METHODS ==========
    
    /// <summary>
    /// Lấy users theo role ID
    /// </summary>
    Task<List<User>> GetByRoleAsync(int roleId);
    
    /// <summary>
    /// Lấy users theo role name (Admin, CompanyAdmin, etc.)
    /// </summary>
    Task<List<User>> GetByRoleAsync(string roleName);
    
    /// <summary>
    /// Lấy users active/inactive
    /// </summary>
    Task<List<User>> GetByActiveStatusAsync(bool isActive);
    
    /// <summary>
    /// Tìm kiếm users theo keyword (username, full_name, phone)
    /// </summary>
    Task<List<User>> SearchAsync(string keyword);
    
    /// <summary>
    /// Đếm tổng số users
    /// </summary>
    Task<int> CountAsync();
    
    /// <summary>
    /// Đếm users theo role
    /// </summary>
    Task<int> CountByRoleAsync(int roleId);
}
