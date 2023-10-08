using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string Surname { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Patronymic { get; set; } = string.Empty;
    public DateTime? DateBirth { get; set; }
    public long PhoneNumberSequence { get; set; }
    public DateTime? SendingCodeTime { get; set; }
    public bool IsActivated { get; set; }
}