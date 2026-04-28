using Xunit;

namespace Zuke.Core.Tests;

public class MarkdownListParsingTests
{
    [Fact]
    public void BulletAndNumberedLists_AreParsedAsItems()
    {
        var md = """
---
lawTitle: T
lawNum: N
era: Reiwa
year: 6
num: 1
lawType: Misc
lang: ja
---
## 第一条
- [号:料金支払] 利用料金を期限までに支払うこと。
- [i:pay-fee] Pay the fee.
- 契約区画以外に駐車しないこと。

1. 利用料金を支払うこと。
2. 契約区画を適正に利用すること。
""";
        var r = TestHelpers.Compile(md);
        Assert.False(r.HasErrors);
        var article = Assert.Single(r.Document!.Document.DirectArticles);
        var para = article.Paragraphs.First();
        var allItems = article.Paragraphs.SelectMany(p => p.Items).ToList();
        Assert.True(allItems.Count >= 5);
        Assert.Equal("一", allItems[0].ItemTitle);
        Assert.Equal("二", allItems[1].ItemTitle);
    }

    [Fact]
    public void JapaneseMiddleDotBullets_AreParsedAsItems()
    {
        var md = """
---
lawTitle: T
lawNum: N
era: Reiwa
year: 6
num: 1
lawType: Misc
lang: ja
---
## 第一条
・ 通常勤務=午前8時30分始業、午後5時30分終業
・ 時差出勤A=午前8時始業、午後5時終業
・ [号:pattern-c] 時差出勤C=午前10時始業、午後7時終業
""";
        var r = TestHelpers.Compile(md);
        Assert.False(r.HasErrors);
        var article = Assert.Single(r.Document!.Document.DirectArticles);
        var items = Assert.Single(article.Paragraphs).Items;
        Assert.Equal(3, items.Count);
        Assert.Equal("通常勤務=午前8時30分始業、午後5時30分終業", items[0].SentenceText);
        Assert.Equal("pattern-c", items[2].ReferenceName);
    }
}
