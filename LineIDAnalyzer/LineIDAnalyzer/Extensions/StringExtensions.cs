namespace LineIDAnalyzer
{
    internal static class StringExtensions
    {
        public static string? NullIfWhiteSpace(this string? s) =>
            string.IsNullOrWhiteSpace(s) ? null : s;
    }
}
