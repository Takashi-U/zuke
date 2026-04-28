using Xunit;
using Zuke.Core.Model;
using Zuke.Core.Parsing;

namespace Zuke.Core.Tests;

public class ReferenceParserTests
{
    [Theory]
    [InlineData("auto", ReferenceOption.Auto)]
    [InlineData("相対", ReferenceOption.Relative)]
    [InlineData("full", ReferenceOption.Full)]
    [InlineData("article", ReferenceOption.ArticleOnly)]
    public void SupportedOptions_Parse(string raw, ReferenceOption expected)
    {
        var ok = ReferenceParser.TryParseOption(raw, out var opt);
        Assert.True(ok);
        Assert.Equal(expected, opt);
    }

    [Fact]
    public void UnsupportedOption_ReturnsFalse()
    {
        var ok = ReferenceParser.TryParseOption("謎オプション", out _);
        Assert.False(ok);
    }
}
