public static class LogSanitizer
{
    public static string SanitizeForLogging<T>(T obj)
    {
        var props = typeof(T).GetProperties()
            .Where(p => !Attribute.IsDefined(p, typeof(LogIgnoreAttribute)))
            .Select(p => $"{p.Name}: {p.GetValue(obj)}");

        return string.Join(", ", props);
    }
}
