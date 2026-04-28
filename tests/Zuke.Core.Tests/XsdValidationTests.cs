using Xunit;
using Zuke.Core.Rendering;
using Zuke.Core.Validation;

namespace Zuke.Core.Tests;

public class XsdValidationTests
{
    [Fact]
    public void WorkRules_GeneratesValidXml()
    {
        var markdown = File.ReadAllText(Path.Combine(TestHelpers.RepoRoot, "samples", "work-rules.md"));
        var result = new Zuke.Core.Compilation.LawMarkdownCompiler().Compile(markdown, "samples/work-rules.md", new());
        Assert.False(result.HasErrors);
        var xml = new LawXmlRenderer().Render(result.Document!.Document);
        var xsd = Path.Combine(TestHelpers.RepoRoot, "schemas", "XMLSchemaForJapaneseLaw_v3.xsd");
        var diags = new LawXmlValidator().Validate(xml, xsd);
        Assert.DoesNotContain(diags, d => d.Code == "LMD044");
    }

    [Fact]
    public void InvalidStructure_FailsBeforeXsd()
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
# 総則
## 節 通則
### 目的
本文
## 章直下条文
本文
""";
        var result = new Zuke.Core.Compilation.LawMarkdownCompiler().Compile(md, "test.md", new());
        Assert.Contains(result.Diagnostics, d => d.Code == "LMD041");
    }
}
