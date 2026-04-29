using Zuke.Core.References;
using Zuke.Core.Importing;
using Zuke.Core.Markdown;
using Zuke.Core.Model;
using Zuke.Core.Rendering;
using Xunit;

namespace Zuke.Core.Tests;

public class LawtextAllowsMissingLawNumTests
{
    [Fact]
    public void FrontMatterWithLawTitleOnly_CanRenderLawtext()
    {
        var md = "---\nlawTitle: 育児・介護休業等に関する規則\n---\n\n## 目的\n本文\n";
        var r = TestHelpers.Compile(md);
        Assert.DoesNotContain(r.Diagnostics, d => d.Code == "LMD045");
        var lawtext = new LawtextRenderer().Render(r.Document!, LawtextRenderOptions.Default);
        Assert.DoesNotContain("（", lawtext.Split('\n').Take(2));
    }

    [Fact]
    public void LawNumEmpty_DoesNotRenderLawNumLine()
    {
        var model = new LawDocumentModel(new LawMetadata("T", "", "Reiwa", 1, 1, "Misc", "ja"), [], [], []);
        var text = new LawtextRenderer().Render(new CompiledLawDocument(model, new Dictionary<string, ReferenceDefinition>(), []), LawtextRenderOptions.Default);
        var lines = text.Split('\n', StringSplitOptions.None);
        Assert.Equal("T", lines[0]);
        Assert.DoesNotContain(lines, l => l.StartsWith("（") && l.EndsWith("）"));
    }
}

public class XmlRequiresMetadataTests
{
    [Theory]
    [InlineData("", "N", "Reiwa", 1, 1, "Misc", "ja")]
    [InlineData("T", "", "Reiwa", 1, 1, "Misc", "ja")]
    [InlineData("T", "N", "", 1, 1, "Misc", "ja")]
    [InlineData("T", "N", "Reiwa", 0, 1, "Misc", "ja")]
    [InlineData("T", "N", "Reiwa", 1, 0, "Misc", "ja")]
    public void MissingOrInvalidMetadata_IsLmd045(string t, string n, string e, int y, int num, string lt, string lang)
    {
        var diags = FrontMatterParser.ValidateForXml(new LawMetadata(t, n, e, y, num, lt, lang), "x.md");
        Assert.Contains(diags, d => d.Code == "LMD045");
    }
}

public class ImportWithoutLawNumTests
{
    [Fact]
    public void MissingLawNum_ImportsWithDefaultsAndWarning()
    {
        var src = "育児・介護休業等に関する規則\n\n第一章　目的\n第一条　目的本文\n";
        var (model, diags) = new LawtextParser().Parse(src, "in.law.txt");
        Assert.Equal(1, model.Metadata.Year);
        Assert.Equal(1, model.Metadata.Num);
        Assert.Contains(diags, d => d.Code == "LMD090" && d.Message.Contains("LawNum がありません"));
    }
}
