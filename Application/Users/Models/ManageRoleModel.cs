namespace Application.Users.Models;

public class ManageRoleModel
{
    public string UserId { get; set; }
    public IEnumerable<string> IdentityRoles { get; set; }
}