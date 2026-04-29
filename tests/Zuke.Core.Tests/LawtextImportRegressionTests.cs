using Zuke.Core.Compilation;
using Zuke.Core.Importing;
using Zuke.Core.Rendering;
using Xunit;

namespace Zuke.Core.Tests;

public class LawtextImportRegressionTests
{
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
