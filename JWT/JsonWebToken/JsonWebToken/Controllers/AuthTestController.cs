using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JsonWebToken.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthTestController : ControllerBase
{
    // Role-based authorization is enforced from the JWT role claim.
    // Call with: Authorization: Bearer <JWT>
    // Customer tokens must contain ClaimTypes.Role = "KhachHang".
    // The role is not "User" because the current database CHECK constraint uses KhachHang.
    [Authorize(Roles = "KhachHang")]
    [HttpGet("user-only")]
    public IActionResult UserOnly()
    {
        return Ok(new { message = "truy cap role khach hang thanh cong." });
    }

    // Only tokens containing ClaimTypes.Role = "Admin" can call this endpoint.
    // A valid token with another role receives 403 automatically.
    [Authorize(Roles = "Admin")]
    [HttpGet("admin-only")]
    public IActionResult AdminOnly()
    {
        return Ok(new { message = "truy cap role admin thanh cong." });
    }

    // Any authenticated API client can read its own profile claims from the bearer token.
    [Authorize]
    [HttpGet("profile")]
    public IActionResult Profile()
    {
        return Ok(new
        {
            userID = User.FindFirstValue(ClaimTypes.NameIdentifier),
            email = User.FindFirstValue(ClaimTypes.Email),
            role = User.FindFirstValue(ClaimTypes.Role)
        });
    }
}
