using System.ComponentModel.DataAnnotations;

namespace SafeVault;

public class User
{
    public int UserID { get; set; }
    [Required]
    required public string Username { get; set; }
    [Required]
    [EmailAddress]
    required public string Email { get; set; }
    [Required]
    required public string PasswordHash { get; set; }

}