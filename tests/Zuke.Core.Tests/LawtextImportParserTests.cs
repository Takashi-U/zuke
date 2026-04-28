using Zuke.Core.Importing;
using Xunit;

namespace Zuke.Core.Tests;

public class LawtextImportParserTests
{
    [Fact]
    public void ParsesBasicLawtextStructure()
    {
        var text = File.ReadAllText(Path.Combine(TestHelpers.RepoRoot, "samples", "import-source.law.txt"));
        var (model, diags) = new LawtextParser().Parse(text, "sample.law.txt");
        Assert.NotEmpty(model.Chapters);
        Assert.Equal("就業規則", model.Metadata.LawTitle);
        Assert.Equal("令和六年規則第一号", model.Metadata.LawNum);
        Assert.Contains(model.Chapters[0].Sections[0].Articles, a => a.Caption == "目的");
        Assert.DoesNotContain(diags, d => d.Severity == Zuke.Core.Model.DiagnosticSeverity.Error);
    }
}
