using System.Text.RegularExpressions;

public class EmailSensitiveDataRule : ISensitiveDataRule
{
    private static readonly Regex EmailRegex = new Regex(@"^[^@]+@[^@]+\.[^@]+$", RegexOptions.IgnoreCase);

    public bool IsSensitive(string data)
    {
        return EmailRegex.IsMatch(data);
    }

    public string MaskData(string data)
    {
        return "***MASKED_EMAIL***";
    }
}
