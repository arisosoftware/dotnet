using System.ComponentModel.DataAnnotations;
namespace WebSeriLogApi.Models;
public class UserDto
{
    public string Name { get; set; }

    [SensitiveData]
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [SensitiveData]
    public string PhoneNumber { get; set; }

    public string Address { get; set; }

    [SensitiveData]
    public string SSN { get; set; }

    [MinLength(6)]
    public string Password { get; set; }

    public string ConfirmPassword { get; set; }
}
