#region

using System.ComponentModel.DataAnnotations;

#endregion

namespace Application.Users.Models;

public class UpdateIdentityUserBatchModel : UpdateIdentityUserModel
{
    [Required]
    public string IdentityUserId { get; set; }
}