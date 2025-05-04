using System.ComponentModel.DataAnnotations;

namespace Currency.Blazor.Models;


public class LoginModel
{
    [Required]
    public string username { get; set; } = string.Empty;
    [Required]
    public string password{ get; set; } = string.Empty;
}
