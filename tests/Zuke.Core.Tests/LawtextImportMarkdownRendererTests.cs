using Zuke.Core.Importing;
using Xunit;

namespace Zuke.Core.Tests;

public class LawtextImportMarkdownRendererTests
{
    [Fact]
    public void RendersChapterSectionAndLabels()
    {
        var text = File.ReadAllText(Path.Combine(TestHelpers.RepoRoot, "samples", "import-source.law.txt"));
        var result = new LawtextImportService().Import(text, "sample", new());
        Assert.Contains("# 総則", result.Markdown);
        Assert.Contains("## 節 通則", result.Markdown);
        Assert.Contains("[条:article-1]", result.Markdown);
        Assert.Contains("{{参照:article-2-p1|相対}}", result.Markdown);
        Assert.Contains("{{参照:article-2-p1|完全}}", result.Markdown);
    }
}
