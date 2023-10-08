#region

using System.ComponentModel.DataAnnotations;

#endregion

namespace Application.Users.Models;

public class UpdateIdentityUserModel
{
    [Required(ErrorMessage = "Поле \"Имя\" обязательно для ввода")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Поле \"Фамилия\" обязательно для ввода")]
    public string Surname { get; set; }

    public string Patronymic { get; set; }
    public string Email { get; set; }
    public DateTime? DateBirth { get; set; }
    public IEnumerable<string> Roles { get; set; }
}