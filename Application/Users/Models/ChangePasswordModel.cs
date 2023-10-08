#region

using System.ComponentModel.DataAnnotations;

#endregion

namespace Application.Users.Models;

public class ChangePasswordModel
{
    [Required(ErrorMessage = "Поле \"Идентификатор\" обязательно для ввода")]
    public string UserId { get; set; }

    [Required(ErrorMessage = "Поле \"Новый пароль\" обязательно для ввода")]
    public string NewPassword { get; set; }

    [Required(ErrorMessage = "Поле \"Старый пароль\" обязательно для ввода")]
    public string OldPassword { get; set; }
}