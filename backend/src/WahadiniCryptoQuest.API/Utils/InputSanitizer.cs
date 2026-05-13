using System.Text.RegularExpressions;

namespace WahadiniCryptoQuest.API.Utils;

/// <summary>
/// Utility class for sanitizing user inputs to prevent XSS attacks (T180)
/// Removes potentially dangerous HTML/JavaScript content from user-provided strings
/// </summary>
public static class InputSanitizer
{
    // Regex patterns for detecting XSS attempts
    private static readonly Regex ScriptTagPattern = new(@"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex OnEventPattern = new(@"\bon\w+\s*=", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex JavaScriptProtocolPattern = new(@"javascript\s*:", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex DataProtocolPattern = new(@"data\s*:", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex VbScriptProtocolPattern = new(@"vbscript\s*:", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex HtmlTagPattern = new(@"<[^>]+>", RegexOptions.Compiled);

    /// <summary>
    /// Checks if input contains potentially dangerous XSS content
    /// </summary>
    public static bool ContainsDangerousContent(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        return ScriptTagPattern.IsMatch(input)
            || OnEventPattern.IsMatch(input)
            || JavaScriptProtocolPattern.IsMatch(input)
            || DataProtocolPattern.IsMatch(input)
            || VbScriptProtocolPattern.IsMatch(input);
    }

    /// <summary>
    /// Sanitizes input by removing HTML tags and dangerous content
    /// Used for fields that should contain plain text only (titles, descriptions)
    /// </summary>
    public static string SanitizePlainText(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Remove script tags
        string sanitized = ScriptTagPattern.Replace(input, string.Empty);

        // Remove event handlers
        sanitized = OnEventPattern.Replace(sanitized, string.Empty);

        // Remove dangerous protocols
        sanitized = JavaScriptProtocolPattern.Replace(sanitized, string.Empty);
        sanitized = DataProtocolPattern.Replace(sanitized, string.Empty);
        sanitized = VbScriptProtocolPattern.Replace(sanitized, string.Empty);

        // Remove all HTML tags
        sanitized = HtmlTagPattern.Replace(sanitized, string.Empty);

        // Decode HTML entities and trim
        sanitized = System.Net.WebUtility.HtmlDecode(sanitized);

        return sanitized.Trim();
    }

    /// <summary>
    /// Validates that a URL is safe and doesn't contain XSS attempts
    /// </summary>
    public static bool IsSafeUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return true;

        // Check for dangerous protocols
        if (JavaScriptProtocolPattern.IsMatch(url)
            || DataProtocolPattern.IsMatch(url)
            || VbScriptProtocolPattern.IsMatch(url))
        {
            return false;
        }

        // Must be a valid HTTP/HTTPS URL
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
    }
}
