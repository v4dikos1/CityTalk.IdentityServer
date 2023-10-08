using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Helpers;

public static class UserManagerUsernameGeneratorExtensions
{
    public static string GenerateUsername(this UserManager<ApplicationUser> userManager)
    {
        var phoneNumberDigit = userManager.Users.Max(x => x.PhoneNumberSequence) + 1;
        var needZeroBefore = 10 - (int)Math.Log10(phoneNumberDigit) - 1;
        var phoneNumber = "+7" + new string('0', needZeroBefore) + phoneNumberDigit;
        return phoneNumber;
    }
}