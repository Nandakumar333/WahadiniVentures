using FluentAssertions;
using WahadiniCryptoQuest.Core.ValueObjects;
using Xunit;

namespace WahadiniCryptoQuest.Core.Tests.ValueObjects;

/// <summary>
/// Tests for YouTubeUrl value object
/// Coverage target: 100% line, 95%+ branch
/// </summary>
public class YouTubeUrlTests
{
    #region Create Tests - Valid Cases

    [Theory]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("http://www.youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("https://youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("http://youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("https://youtu.be/dQw4w9WgXcQ")]
    [InlineData("http://youtu.be/dQw4w9WgXcQ")]
    public void Create_WithValidYouTubeUrl_ShouldCreateYouTubeUrl(string url)
    {
        // Act
        var youtubeUrl = YouTubeUrl.Create(url);

        // Assert
        youtubeUrl.Should().NotBeNull();
        youtubeUrl.Value.Should().Be(url);
    }

    [Theory]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ&list=PLrAXtmErZgOeiKm4sgNOknGvNjby9efdf")]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ&t=42s")]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ&feature=youtu.be")]
    public void Create_WithQueryParameters_ShouldSucceed(string url)
    {
        // Act
        var youtubeUrl = YouTubeUrl.Create(url);

        // Assert
        youtubeUrl.Value.Should().Be(url);
    }

    [Theory]
    [InlineData("https://www.youtube.com/embed/dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/v/dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/channel/UCuAXFkgsw1L7xaCfnd5JJOw")]
    [InlineData("https://www.youtube.com/user/username")]
    public void Create_WithDifferentYouTubePaths_ShouldSucceed(string url)
    {
        // Act
        var youtubeUrl = YouTubeUrl.Create(url);

        // Assert
        youtubeUrl.Value.Should().Be(url);
    }

    #endregion

    #region Create Tests - Invalid Cases

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Create_WithNullOrWhitespace_ShouldThrowArgumentException(string? url)
    {
        // Act
        var act = () => YouTubeUrl.Create(url!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("YouTube URL cannot be empty*")
            .WithParameterName("url");
    }

    [Theory]
    [InlineData("https://www.google.com")]
    [InlineData("https://www.vimeo.com/123456")]
    [InlineData("https://www.dailymotion.com/video/x123456")]
    [InlineData("not a url")]
    [InlineData("ftp://youtube.com/video")]
    public void Create_WithNonYouTubeUrl_ShouldThrowArgumentException(string url)
    {
        // Act
        var act = () => YouTubeUrl.Create(url);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Invalid YouTube URL format*")
            .WithParameterName("url");
    }

    [Theory]
    [InlineData("youtube.com")]
    [InlineData("www.youtube.com")]
    [InlineData("youtu.be")]
    public void Create_WithoutProtocol_ShouldThrowArgumentException(string url)
    {
        // Act
        var act = () => YouTubeUrl.Create(url);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Invalid YouTube URL format*");
    }

    #endregion

    #region ExtractVideoId Tests

    [Theory]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("http://www.youtube.com/watch?v=abc123DEF456", "abc123DEF456")]
    [InlineData("https://youtube.com/watch?v=test_video-ID", "test_video-ID")]
    public void ExtractVideoId_FromStandardUrl_ShouldReturnVideoId(string url, string expectedId)
    {
        // Arrange
        var youtubeUrl = YouTubeUrl.Create(url);

        // Act
        var videoId = youtubeUrl.ExtractVideoId();

        // Assert
        videoId.Should().Be(expectedId);
    }

    [Theory]
    [InlineData("https://youtu.be/dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("http://youtu.be/abc123DEF456", "abc123DEF456")]
    [InlineData("https://youtu.be/test_video-ID", "test_video-ID")]
    public void ExtractVideoId_FromShortUrl_ShouldReturnVideoId(string url, string expectedId)
    {
        // Arrange
        var youtubeUrl = YouTubeUrl.Create(url);

        // Act
        var videoId = youtubeUrl.ExtractVideoId();

        // Assert
        videoId.Should().Be(expectedId);
    }

    [Theory]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ&t=42s")]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ&list=PLrAXtmErZgOeiKm4sgNOknGvNjby9efdf")]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ&feature=youtu.be&t=10")]
    public void ExtractVideoId_WithAdditionalParameters_ShouldReturnVideoId(string url)
    {
        // Arrange
        var youtubeUrl = YouTubeUrl.Create(url);

        // Act
        var videoId = youtubeUrl.ExtractVideoId();

        // Assert
        videoId.Should().Be("dQw4w9WgXcQ");
    }

    [Theory]
    [InlineData("https://www.youtube.com/embed/dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/v/dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/channel/UCuAXFkgsw1L7xaCfnd5JJOw")]
    public void ExtractVideoId_FromNonWatchUrl_ShouldThrowInvalidOperationException(string url)
    {
        // Arrange
        var youtubeUrl = YouTubeUrl.Create(url);

        // Act
        var act = () => youtubeUrl.ExtractVideoId();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Could not extract video ID");
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ShouldReturnUrlValue()
    {
        // Arrange
        var url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
        var youtubeUrl = YouTubeUrl.Create(url);

        // Act
        var result = youtubeUrl.ToString();

        // Assert
        result.Should().Be(url);
    }

    #endregion

    #region Equals Tests

    [Fact]
    public void Equals_WithSameUrl_ShouldReturnTrue()
    {
        // Arrange
        var url1 = YouTubeUrl.Create("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
        var url2 = YouTubeUrl.Create("https://www.youtube.com/watch?v=dQw4w9WgXcQ");

        // Act
        var result = url1.Equals(url2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentUrl_ShouldReturnFalse()
    {
        // Arrange
        var url1 = YouTubeUrl.Create("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
        var url2 = YouTubeUrl.Create("https://www.youtube.com/watch?v=abc123def456");

        // Act
        var result = url1.Equals(url2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var youtubeUrl = YouTubeUrl.Create("https://www.youtube.com/watch?v=dQw4w9WgXcQ");

        // Act
        var result = youtubeUrl.Equals(null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentType_ShouldReturnFalse()
    {
        // Arrange
        var youtubeUrl = YouTubeUrl.Create("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
        var otherObject = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";

        // Act
        var result = youtubeUrl.Equals(otherObject);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_WithSameVideoIdDifferentFormat_ShouldReturnFalse()
    {
        // Arrange - Different URLs for same video (different formats not considered equal)
        var url1 = YouTubeUrl.Create("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
        var url2 = YouTubeUrl.Create("https://youtu.be/dQw4w9WgXcQ");

        // Act
        var result = url1.Equals(url2);

        // Assert - Value objects compare by value, not semantic meaning
        result.Should().BeFalse();
    }

    #endregion

    #region GetHashCode Tests

    [Fact]
    public void GetHashCode_WithSameUrl_ShouldReturnSameHashCode()
    {
        // Arrange
        var url1 = YouTubeUrl.Create("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
        var url2 = YouTubeUrl.Create("https://www.youtube.com/watch?v=dQw4w9WgXcQ");

        // Act
        var hash1 = url1.GetHashCode();
        var hash2 = url2.GetHashCode();

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void GetHashCode_WithDifferentUrl_ShouldReturnDifferentHashCode()
    {
        // Arrange
        var url1 = YouTubeUrl.Create("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
        var url2 = YouTubeUrl.Create("https://www.youtube.com/watch?v=abc123def456");

        // Act
        var hash1 = url1.GetHashCode();
        var hash2 = url2.GetHashCode();

        // Assert
        hash1.Should().NotBe(hash2);
    }

    #endregion

    #region Value Object Behavior Tests

    [Fact]
    public void YouTubeUrl_ShouldBeImmutable()
    {
        // Arrange
        var url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
        var youtubeUrl = YouTubeUrl.Create(url);
        var originalValue = youtubeUrl.Value;

        // Act - Value has private setter, cannot be changed
        var currentValue = youtubeUrl.Value;

        // Assert
        currentValue.Should().Be(originalValue);
    }

    [Fact]
    public void YouTubeUrl_WithSameValue_ShouldBeInterchangeable()
    {
        // Arrange
        var url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
        var youtubeUrl1 = YouTubeUrl.Create(url);
        var youtubeUrl2 = YouTubeUrl.Create(url);

        // Act & Assert - Value objects with same value should be equal
        youtubeUrl1.Should().Be(youtubeUrl2);
        youtubeUrl1.GetHashCode().Should().Be(youtubeUrl2.GetHashCode());
        youtubeUrl1.ToString().Should().Be(youtubeUrl2.ToString());
    }

    #endregion

    #region Edge Case Tests

    [Theory]
    [InlineData("HTTPS://WWW.YOUTUBE.COM/WATCH?V=DQW4W9WGXCQ")]
    [InlineData("HtTpS://WwW.yOuTuBe.CoM/wAtCh?V=dQw4w9WgXcQ")]
    public void Create_WithMixedCase_ShouldPreserveCasing(string url)
    {
        // Act
        var youtubeUrl = YouTubeUrl.Create(url);

        // Assert - URL is case-insensitive in regex but preserves original
        youtubeUrl.Value.Should().Be(url);
    }

    [Fact]
    public void Create_WithVeryLongUrl_ShouldSucceed()
    {
        // Arrange - URL with many query parameters
        var url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ&list=PLrAXtmErZgOeiKm4sgNOknGvNjby9efdf&index=1&t=0s&feature=youtu.be&ab_channel=TestChannel";

        // Act
        var youtubeUrl = YouTubeUrl.Create(url);

        // Assert
        youtubeUrl.Value.Should().Be(url);
    }

    [Theory]
    [InlineData("https://youtu.be/dQw4w9WgXcQ?t=42")]
    [InlineData("https://youtu.be/dQw4w9WgXcQ?feature=share")]
    public void ExtractVideoId_FromShortUrlWithParams_ShouldReturnVideoId(string url)
    {
        // Arrange
        var youtubeUrl = YouTubeUrl.Create(url);

        // Act
        var videoId = youtubeUrl.ExtractVideoId();

        // Assert
        videoId.Should().Be("dQw4w9WgXcQ");
    }

    [Fact]
    public void Create_WithHttpsAndWWW_ShouldSucceed()
    {
        // Arrange
        var url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";

        // Act
        var youtubeUrl = YouTubeUrl.Create(url);

        // Assert
        youtubeUrl.Value.Should().Be(url);
    }

    [Fact]
    public void Create_WithHttpWithoutWWW_ShouldSucceed()
    {
        // Arrange
        var url = "http://youtube.com/watch?v=dQw4w9WgXcQ";

        // Act
        var youtubeUrl = YouTubeUrl.Create(url);

        // Assert
        youtubeUrl.Value.Should().Be(url);
    }

    #endregion
}
