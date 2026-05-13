using System.Text.RegularExpressions;

namespace WahadiniCryptoQuest.Core.ValueObjects;

/// <summary>
/// Value object representing an email address with validation
/// Ensures email addresses are always in a valid format
/// </summary>
public class Email
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; private set; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (!EmailRegex.IsMatch(email))
            throw new ArgumentException("Invalid email format", nameof(email));

        if (email.Length > 320)
            throw new ArgumentException("Email cannot exceed 320 characters", nameof(email));

        return new Email(email.ToLowerInvariant());
    }

    public override string ToString() => Value;

    public override bool Equals(object? obj)
    {
        if (obj is Email other)
            return Value == other.Value;
        return false;
    }

    public override int GetHashCode() => Value.GetHashCode();
}

/// <summary>
/// Value object representing a YouTube URL with validation
/// Ensures URLs are valid YouTube links
/// </summary>
public class YouTubeUrl
{
    private static readonly Regex YouTubeRegex = new(
        @"^(https?://)?(www\.)?(youtube\.com|youtu\.be)/.+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; private set; }

    private YouTubeUrl(string value)
    {
        Value = value;
    }

    public static YouTubeUrl Create(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("YouTube URL cannot be empty", nameof(url));

        if (!YouTubeRegex.IsMatch(url))
            throw new ArgumentException("Invalid YouTube URL format", nameof(url));

        return new YouTubeUrl(url);
    }

    public string ExtractVideoId()
    {
        // Extract video ID from various YouTube URL formats
        var uri = new Uri(Value);
        if (uri.Host.Contains("youtu.be"))
            return uri.AbsolutePath.TrimStart('/');

        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
        return query["v"] ?? throw new InvalidOperationException("Could not extract video ID");
    }

    public override string ToString() => Value;

    public override bool Equals(object? obj)
    {
        if (obj is YouTubeUrl other)
            return Value == other.Value;
        return false;
    }

    public override int GetHashCode() => Value.GetHashCode();
}
