using System.ComponentModel.DataAnnotations;

namespace SafeVault;

public class UserViewModel
{
    [Required]
    [StringLength(50, MinimumLength = 5)]
    [RegularExpression(@"^[a-zA-Z0-9]*$", ErrorMessage = "Username can only contain letters and numbers.")]
    required public string Username { get; set; }
    [Required]
    [EmailAddress]
    required public string Email { get; set; }
    [Required]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6)]
    required public string Password { get; set; }
    [Required]
    [DataType(DataType.Password)]
    [Compare ("Password", ErrorMessage = "Password does not match")]
    required public string ConfrimPassword { get; set; }
    public bool isAdmin { get; set; }

}