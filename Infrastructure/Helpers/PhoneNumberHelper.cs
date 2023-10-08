using System.Text.RegularExpressions;

namespace Infrastructure.Helpers;

public static class PhoneNumberHelper
{
    public static string ConvertPhoneNumberFormat(this string input)
    {
        input = new string(input.Where(char.IsDigit).ToArray());
        if (input.Length == 10)
        {
            return $"+7{input}";
        }

        if (input.Length == 11)
        {
            return $"+7{input.Substring(1, 10)}";
        }

        throw new ApplicationException("Неверный формат номера телефона!");
    }

    public static bool IsPhoneValidFormat(this string input)
    {
        return Regex.IsMatch(input, @"\+7\d{10}");
    }
}