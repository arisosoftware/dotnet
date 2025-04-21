    using System.Text.RegularExpressions;

public class PhoneNumberSensitiveDataRule : ISensitiveDataRule
{
    // Regex to match North American phone numbers, including optional +1, dashes, spaces, parentheses
    private static readonly Regex PhoneRegex = new Regex(
        @"(?:(?:\+?1[-.\s]*)?)?(?:\(?\d{3}\)?[-.\s]*)\d{3}[-.\s]*\d{4}",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public bool IsSensitive(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        return PhoneRegex.IsMatch(input);
    }

    public string MaskData(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        return PhoneRegex.Replace(input, "***-***-****");
    }
}
