using System.ComponentModel.DataAnnotations;

namespace Currency.Wasm.Models;

public class PasswordChangeModel
{
    [Required(ErrorMessage ="هذا الحقل مطلوب")]
    public string oldPassword { get; set; }
    [Required(ErrorMessage ="هذا الحقل مطلوب")]
    public string newPassword { get; set; }

    [Compare(nameof(newPassword), ErrorMessage ="كلمة المرور غير متطابقة")]
    public  string ConfirmPassword { get; set; }
}
