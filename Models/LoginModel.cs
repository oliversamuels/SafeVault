using System.ComponentModel.DataAnnotations;

namespace SafeVault;

public class LoginModel
{
    [Required]
    required public string UsernameOrEmail { get; set; }

    [Required]
    [DataType(DataType.Password)]
    required public string Password { get; set; }
}