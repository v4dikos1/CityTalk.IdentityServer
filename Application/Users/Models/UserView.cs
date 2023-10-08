namespace Application.Users.Models;

public class UserView
{
    public string IdentityId { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Patronymic { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string DateBirth { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool EmailConfirmed { get; set; }
}