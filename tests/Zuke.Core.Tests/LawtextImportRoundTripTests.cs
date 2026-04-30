using Zuke.Core.Compilation;
using Xunit;

namespace Zuke.Core.Tests;

public class LawtextImportRoundTripTests
{
    [Fact]
    public void ImportedMarkdownCanCompile()
    {
        var text = File.ReadAllText(Path.Combine(TestHelpers.RepoRoot, "samples", "import-source.law.txt"));
        var imported = new Zuke.Core.Importing.LawtextImportService().Import(text, "sample", new());
        var compiled = new LawMarkdownCompiler().Compile(imported.Markdown, "imported.md", new CompileOptions(false, false));
        Assert.False(compiled.HasErrors);
        Assert.NotNull(compiled.Document);
    }

    [Fact]
    public void RoundTrip_Preserves_ItemAndSubitemIndent()
    {
        var input = "テスト規程\n\n第1条　本文。\n  一　第一号\n    イ　イ号\n    ロ　ロ号\n";
        var imported = new Zuke.Core.Importing.LawtextImportService().Import(input, "sample", new());
        var roundTrip = TestHelpers.RenderLawtext(imported.Markdown);
        Assert.Contains("\n  一　第一号\n", roundTrip);
        Assert.Contains("\n    イ　イ号\n", roundTrip);
        Assert.Contains("\n    ロ　ロ号\n", roundTrip);
    }

    [Fact]
    public void RoundTrip_Preserves_HyphenBulletOrderUnderItem()
    {
        var input = "テスト規程\n\n第1条　本文。\n  一　対象従業員は...\n    - 通常勤務=...\n    - 時差出勤A=...\n  二　次の号\n";
        var imported = new Zuke.Core.Importing.LawtextImportService().Import(input, "sample", new());
        var roundTrip = TestHelpers.RenderLawtext(imported.Markdown);
        var idxItem = roundTrip.IndexOf("  一　対象従業員は...", StringComparison.Ordinal);
        var idxBullet1 = roundTrip.IndexOf("    - 通常勤務=...", StringComparison.Ordinal);
        var idxBullet2 = roundTrip.IndexOf("    - 時差出勤A=...", StringComparison.Ordinal);
        var idxNextItem = roundTrip.IndexOf("\n  二　次の号", StringComparison.Ordinal);
        Assert.True(idxItem >= 0 && idxBullet1 > idxItem);
        Assert.True(idxBullet2 > idxBullet1);
        Assert.True(idxNextItem > idxBullet2);
    }

    [Fact]
    public void RoundTrip_Preserves_ParagraphLevelRawBulletIndent()
    {
        var input = "テスト規程\n\n第1条　本文。\n  - 通常勤務=...\n  - 時差出勤A=...\n";
        var imported = new Zuke.Core.Importing.LawtextImportService().Import(input, "sample", new());
        var roundTrip = TestHelpers.RenderLawtext(imported.Markdown);
        Assert.Contains("\n  - 通常勤務=...\n", roundTrip);
        Assert.Contains("\n  - 時差出勤A=...\n", roundTrip);
    }

    [Fact]
    public void RoundTrip_DoesNotExpandShortReferences()
    {
        var input = "テスト規程\n\n第1条　第1号及び第2号の措置を実施する。\n  一　第一号\n  二　第二号\n";
        var imported = new Zuke.Core.Importing.LawtextImportService().Import(input, "sample", new());
        var roundTrip = TestHelpers.RenderLawtext(imported.Markdown);
        Assert.Contains("第1号及び第2号の措置を実施する。", roundTrip);
        Assert.DoesNotContain("第1条第1項第1号", roundTrip);
    }

    [Fact]
    public void RoundTrip_KeepsShortReferenceForms_AsOriginalText()
    {
        var input = "テスト規程\n\n第1条　第4項及び第7項を確認する。\n第2条　第4項から第7項を準用する。\n";
        var imported = new Zuke.Core.Importing.LawtextImportService().Import(input, "sample", new());
        var roundTrip = TestHelpers.RenderLawtext(imported.Markdown);
        Assert.Contains("第4項及び第7項を確認する。", roundTrip);
        Assert.Contains("第4項から第7項を準用する。", roundTrip);
        Assert.DoesNotContain("第1条第4項", roundTrip);
        Assert.DoesNotContain("第2条第7項", roundTrip);
    }
}
