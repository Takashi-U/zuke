using System.Xml.Linq;
using Zuke.Core.Compilation;
using Zuke.Core.Importing;
using Xunit;
using Zuke.Core.Rendering;
using Zuke.Core.Validation;

namespace Zuke.Core.Tests;

public class XsdValidationTests
{
    [Fact]
    public void CurrentRealXsdIsStrict()
    {
        var xsdPath = Path.Combine(TestHelpers.RepoRoot, "schemas", "XMLSchemaForJapaneseLaw_v3.xsd");
        var xsdText = File.ReadAllText(xsdPath);

        Assert.DoesNotContain("<xs:element name=\"LawBody\" type=\"xs:anyType\"", xsdText);
        Assert.Contains("<xs:element name=\"Article\"", xsdText);
        Assert.Contains("<xs:element name=\"Paragraph\"", xsdText);
        Assert.Contains("<xs:element name=\"Item\"", xsdText);
        Assert.Contains("<xs:element name=\"SupplProvision\"", xsdText);
        Assert.Contains("<xs:element name=\"List\"", xsdText);
    }

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

    [Fact]
    public void SupplementaryProvision_GeneratesValidXml()
    {
        var md = """
---
lawTitle: テスト規則
lawNum: テスト第1号
era: Reiwa
year: 6
num: 1
lawType: Misc
lang: ja
---
# テスト規則

## 第一条
本文。

## 附則
本規則は、◯年◯月◯日から適用する。
""";
        var result = TestHelpers.Compile(md);
        Assert.False(result.HasErrors);

        var xml = new LawXmlRenderer().Render(result.Document!.Document);
        Assert.DoesNotContain("<SupplProvisionSentence>", xml.ToString(SaveOptions.DisableFormatting));
        var xsd = Path.Combine(TestHelpers.RepoRoot, "schemas", "XMLSchemaForJapaneseLaw_v3.xsd");
        var diags = new LawXmlValidator().Validate(xml, xsd);
        Assert.DoesNotContain(diags, d => d.Code == "LMD044");
    }

    [Fact]
    public void IkujiKaigoLawtextImport_PassesOfficialXsdRoundTripCheck()
    {
        var source = File.ReadAllText(Path.Combine(TestHelpers.RepoRoot, "samples", "ikuji_kaigo_kyugyo_kitei_lawtext.txt"));
        var imported = new LawtextImportService().Import(source, "samples/ikuji_kaigo_kyugyo_kitei_lawtext.txt", new());
        Assert.False(imported.HasErrors);
        Assert.DoesNotContain(imported.Diagnostics, d => d.Code == "LMD044");

        var compiled = new LawMarkdownCompiler().Compile(imported.Markdown, "samples/ikuji_kaigo_kyugyo_kitei_lawtext.imported.md", new CompileOptions(false, true));
        Assert.False(compiled.HasErrors);

        var xml = new LawXmlRenderer().Render(compiled.Document!.Document);
        var xsd = Path.Combine(TestHelpers.RepoRoot, "schemas", "XMLSchemaForJapaneseLaw_v3.xsd");
        var diags = new LawXmlValidator().Validate(xml, xsd);

        Assert.DoesNotContain(diags, d => d.Code == "LMD044");
    }

    [Fact]
    public void ParagraphRawBullet_GeneratesValidXml_WithOfficialXsd()
    {
        const string markdown = """
---
lawTitle: テスト規則
lawNum: テスト第1号
era: Reiwa
year: 6
num: 1
lawType: Misc
lang: ja
---
# テスト規則

## 第一条
本文。
  - 通常勤務=...
  - 時差出勤A=...
""";
        var compiled = new LawMarkdownCompiler().Compile(markdown, "paragraph-raw-bullet.md", new CompileOptions(false, true));
        Assert.False(compiled.HasErrors);

        var xml = new LawXmlRenderer().Render(compiled.Document!.Document);
        var xmlText = xml.ToString(SaveOptions.DisableFormatting);
        Assert.DoesNotContain("<Paragraph Num=\"1\"><ParagraphNum /><ParagraphSentence><Sentence Num=\"1\">本文。</Sentence></ParagraphSentence><List>", xmlText);
        Assert.Contains("<Item Num=\"1\"><ItemSentence><Sentence Num=\"1\">通常勤務=...</Sentence></ItemSentence></Item>", xmlText);
        Assert.Contains("<Item Num=\"2\"><ItemSentence><Sentence Num=\"1\">時差出勤A=...</Sentence></ItemSentence></Item>", xmlText);
        Assert.DoesNotContain("<Subitem1Title>-</Subitem1Title>", xmlText);

        var xsd = Path.Combine(TestHelpers.RepoRoot, "schemas", "XMLSchemaForJapaneseLaw_v3.xsd");
        var diags = new LawXmlValidator().Validate(xml, xsd);
        Assert.DoesNotContain(diags, d => d.Code == "LMD044");
    }

    [Fact]
    public void ItemRawBullet_GeneratesValidXml_WithOfficialXsd()
    {
        const string markdown = """
---
lawTitle: テスト規則
lawNum: テスト第1号
era: Reiwa
year: 6
num: 1
lawType: Misc
lang: ja
---
# テスト規則

## 第一条
本文。
  一　対象従業員は...
    - 通常勤務=...
    - 時差出勤A=...
""";
        var compiled = new LawMarkdownCompiler().Compile(markdown, "item-raw-bullet.md", new CompileOptions(false, true));
        Assert.False(compiled.HasErrors);

        var xml = new LawXmlRenderer().Render(compiled.Document!.Document);
        var xmlText = xml.ToString(SaveOptions.DisableFormatting);
        Assert.Contains("<Item Num=\"1\">", xmlText);
        Assert.DoesNotContain("<Paragraph Num=\"1\"><ParagraphNum /><ParagraphSentence><Sentence Num=\"1\">本文。</Sentence></ParagraphSentence><List>", xmlText);
        Assert.DoesNotContain("<Subitem1Title>-</Subitem1Title>", xmlText);

        var xsd = Path.Combine(TestHelpers.RepoRoot, "schemas", "XMLSchemaForJapaneseLaw_v3.xsd");
        var diags = new LawXmlValidator().Validate(xml, xsd);
        Assert.DoesNotContain(diags, d => d.Code == "LMD044");
    }
}
