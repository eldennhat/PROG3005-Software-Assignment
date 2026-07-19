using System.ComponentModel.DataAnnotations;

namespace JsonWebToken.Models;

public class LoginRequest
{
    [Required(ErrorMessage = "Email khong duoc de trong")]
    [EmailAddress(ErrorMessage = "Email khong dung dinh dang")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mat khau khong duoc de trong")]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}
