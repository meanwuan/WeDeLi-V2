using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
namespace wedeli.Infrastructure;

public static class JwtExtensions
{
    /// <summary>
    /// Lấy UserId từ ClaimsPrincipal
    /// </summary>
    public static int? GetUserId(this ClaimsPrincipal principal)
    {
        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (int.TryParse(userIdClaim, out var userId))
            return userId;

        return null;
    }

    /// <summary>
    /// Lấy Username từ ClaimsPrincipal (field chính trong DB)
    /// </summary>
    public static string? GetUsername(this ClaimsPrincipal principal)
    {
        return principal.FindFirst("Username")?.Value
               ?? principal.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value;
    }

    /// <summary>
    /// Lấy Email từ ClaimsPrincipal (nullable trong DB)
    /// </summary>
    public static string? GetEmail(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Email)?.Value
               ?? principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
    }

    /// <summary>
    /// Lấy Phone từ ClaimsPrincipal
    /// </summary>
    public static string? GetPhone(this ClaimsPrincipal principal)
    {
        return principal.FindFirst("Phone")?.Value;
    }

    /// <summary>
    /// Lấy FullName từ ClaimsPrincipal
    /// </summary>
    public static string? GetFullName(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Name)?.Value;
    }

    /// <summary>
    /// Lấy Role từ ClaimsPrincipal
    /// </summary>
    public static string? GetRole(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Role)?.Value;
    }

    /// <summary>
    /// Lấy RoleId từ ClaimsPrincipal
    /// </summary>
    public static int? GetRoleId(this ClaimsPrincipal principal)
    {
        var roleIdClaim = principal.FindFirst("RoleId")?.Value;

        if (int.TryParse(roleIdClaim, out var roleId))
            return roleId;

        return null;
    }

    /// <summary>
    /// Kiểm tra user có active không
    /// </summary>
    public static bool IsActive(this ClaimsPrincipal principal)
    {
        var isActiveClaim = principal.FindFirst("IsActive")?.Value;
        return bool.TryParse(isActiveClaim, out var isActive) && isActive;
    }

    /// <summary>
    /// Kiểm tra user có role cụ thể không
    /// </summary>
    public static bool HasRole(this ClaimsPrincipal principal, string role)
    {
        return principal.IsInRole(role);
    }

    /// <summary>
    /// Kiểm tra user có một trong các roles không
    /// </summary>
    public static bool HasAnyRole(this ClaimsPrincipal principal, params string[] roles)
    {
        return roles.Any(role => principal.IsInRole(role));
    }

    /// <summary>
    /// Kiểm tra user có phải admin không (role_name = 'admin')
    /// </summary>
    public static bool IsAdmin(this ClaimsPrincipal principal)
    {
        return principal.IsInRole("admin");
    }

    /// <summary>
    /// Kiểm tra user có phải driver không
    /// </summary>
    public static bool IsDriver(this ClaimsPrincipal principal)
    {
        return principal.IsInRole("driver") || principal.IsInRole("multi_role");
    }

    /// <summary>
    /// Kiểm tra user có phải warehouse staff không
    /// </summary>
    public static bool IsWarehouseStaff(this ClaimsPrincipal principal)
    {
        return principal.IsInRole("warehouse_staff") || principal.IsInRole("multi_role");
    }

    /// <summary>
    /// Kiểm tra user có phải customer không
    /// </summary>
    public static bool IsCustomer(this ClaimsPrincipal principal)
    {
        return principal.IsInRole("customer");
    }
}