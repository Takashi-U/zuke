using System.Xml.Linq;
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

    [Theory]
    [InlineData("minimal.md")]
    [InlineData("chapter-section.md")]
    [InlineData("items.md")]
    public void Fixtures_GenerateValidXml(string fixture)
    {
        var md = TestHelpers.ReadFixture(fixture);
        var result = TestHelpers.Compile(md);
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
        var result = TestHelpers.Compile(md);
        Assert.Contains(result.Diagnostics, d => d.Code is "LMD040" or "LMD041");
    }

    [Fact]
    public void BrokenXml_IsLmd044()
    {
        var broken = XDocument.Parse("<Law><LawBody/></Law>");
        var xsd = Path.Combine(TestHelpers.RepoRoot, "schemas", "XMLSchemaForJapaneseLaw_v3.xsd");
        var diags = new LawXmlValidator().Validate(broken, xsd);
        Assert.Contains(diags, d => d.Code == "LMD044");
    }
}
