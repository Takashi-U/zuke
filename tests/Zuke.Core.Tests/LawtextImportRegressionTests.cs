using Zuke.Core.Compilation;
using Zuke.Core.Importing;
using Zuke.Core.Rendering;
using Xunit;

namespace Zuke.Core.Tests;

public class LawtextImportRegressionTests
{
    [Fact]
    public void RoundTrip_FixesKnownResidualBugs()
    {
        const string source = """
就業規則

第1条　次の各号のいずれかに該当する者は対象外とする。
一　入社1年未満の従業員
二　申出の日から1年以内に雇用関係が終了することが明らかな従業員
三　1週間の所定労働日数が2日以下の従業員

第2条　本条第1項にかかわらず、本条第1項第1号に定める。
2　会社は、本条第6項又は第7項を準用する。
3　従業員は、本条第3項第1号による。

      附則
本規程は、公布の日から施行する。
""";

        var imported = new LawtextImportService().Import(source, "residual.law.txt", new());
        Assert.Contains("paragraphNumberStyle: ascii", imported.Markdown);
        var compiled = new LawMarkdownCompiler().Compile(imported.Markdown, "imported.md", new CompileOptions(false, true));
        Assert.False(compiled.HasErrors);
        var rendered = new LawtextRenderer().Render(compiled.Document!);

        Assert.Contains("一　入社1年未満の従業員", rendered);
        Assert.DoesNotContain("一　一　入社1年未満の従業員", rendered);
        Assert.Contains("本条第1項", rendered);
        Assert.Contains("本条第1項第1号", rendered);
        Assert.Contains("本条第6項又は第7項", rendered);
        Assert.DoesNotContain("本条第5条第1項", rendered);
        Assert.Contains("会社は、本条第6項又は第7項を準用する。", rendered);
        Assert.Contains("従業員は、本条第3項第1号による。", rendered);
        Assert.DoesNotContain("\n２　会社は", rendered);
        Assert.Contains("\n      附則\n", rendered);
    }

    [Fact]
    public void RoundTrip_PreservesArabicReferencesBulletsAndSupplementaryProvision()
    {
        const string source = """
就業規則

第1条　この規則は、本条第1項及び本条第6項又は第7項に従う。

第9条　前条第3項及び次条第3項を参照する。

第9条の2　別に定める。

第10条　次のとおりとする。
  - 通常勤務=...
  - 時差出勤A=...

附則
本規則は、◯年◯月◯日から適用する。
""";

        var imported = new LawtextImportService().Import(source, "regression.law.txt", new());
        Assert.Contains("numberStyle: arabic", imported.Markdown);

        var compiled = new LawMarkdownCompiler().Compile(imported.Markdown, "imported.md", new CompileOptions(false, true));
        Assert.False(compiled.HasErrors);
        var rendered = new LawtextRenderer().Render(compiled.Document!, LawtextRenderOptions.Default with { ArabicNumbers = true });

        Assert.Contains("第1条", rendered);
        Assert.Contains("第9条の2", rendered);
        Assert.Contains("第10条", rendered);
        Assert.DoesNotContain("第一条", rendered);
        Assert.DoesNotContain("第九条の二", rendered);
        Assert.DoesNotContain("第十条", rendered);
        Assert.Contains("本条第6項又は第7項", rendered);
        Assert.DoesNotContain("前条第十七条第三項", rendered);
        Assert.Contains("- 通常勤務=...", rendered);
        Assert.Contains("- 時差出勤A=...", rendered);
        Assert.Contains("附則", rendered);
    }
}
