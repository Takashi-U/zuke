using Xunit;
using Zuke.Core.Numbering;

namespace Zuke.Core.Tests;

public class JapaneseNumberFormatterTests
{
    [Theory]
    [InlineData(11, "十一")]
    [InlineData(12, "十二")]
    [InlineData(20, "二十")]
    [InlineData(21, "二十一")]
    [InlineData(30, "三十")]
    [InlineData(101, "百一")]
    public void FormatsKanjiNumbers(int n, string expected)
    {
        Assert.Equal(expected, JapaneseNumberFormatter.ToKanjiNumber(n));
    }

    [Theory]
    [InlineData(11, "第十一条", "第十一号")]
    [InlineData(12, "第十二条", "第十二号")]
    [InlineData(20, "第二十条", "第二十号")]
    [InlineData(21, "第二十一条", "第二十一号")]
    public void FormatsArticleAndItemReferences(int n, string article, string item)
    {
        Assert.Equal(article, JapaneseNumberFormatter.ToArticle(n, arabic: false));
        Assert.Equal(item, JapaneseNumberFormatter.ToItemReference(n, arabic: false));
    }
}
