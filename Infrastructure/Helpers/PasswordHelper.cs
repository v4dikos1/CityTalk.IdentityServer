using IdentityModel;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Helpers;

public static class PasswordHelper
{
    public static string GeneratePassword(int length = 32, int countUniqueChars = 10, bool requireUppercase = true,
        bool requireLowercase = true, bool requireDigit = true)
    {
        var randomChars = new[]
        {
            "ABCDEFGHJKLMNOPQRSTUVWXYZ", // uppercase 
            "abcdefghijkmnopqrstuvwxyz", // lowercase
            "0123456789" // digits
        };
        var rand = new CryptoRandom();
        var chars = new List<char>();


        if (requireUppercase)
            chars.Insert(rand.Next(0, chars.Count),
                randomChars[0][rand.Next(0, randomChars[0].Length)]);

        if (requireLowercase)
            chars.Insert(rand.Next(0, chars.Count),
                randomChars[1][rand.Next(0, randomChars[1].Length)]);

        if (requireDigit)
            chars.Insert(rand.Next(0, chars.Count),
                randomChars[2][rand.Next(0, randomChars[2].Length)]);

        for (var i = chars.Count;
             i < length
             || chars.Distinct().Count() < countUniqueChars;
             i++)
        {
            var rcs = randomChars[rand.Next(0, randomChars.Length)];
            chars.Insert(rand.Next(0, chars.Count),
                rcs[rand.Next(0, rcs.Length)]);
        }

        return new string(chars.ToArray());
    }

    public static string GeneratePassword(PasswordOptions options)
    {
        return GeneratePassword(options.RequiredLength, options.RequiredUniqueChars, options.RequireUppercase,
            options.RequireLowercase, options.RequireDigit);
    }
}