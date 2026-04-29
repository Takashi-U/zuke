using Xunit;

namespace Zuke.Core.Tests;

public class ImportEnhancementCliTests
{
    [Fact]
    public void ImportCommandCreatesReportAndMap()
    {
        var input = Path.Combine(TestHelpers.RepoRoot, "samples", "import-source.law.txt");
        var md = Path.GetTempFileName();
        var report = Path.GetTempFileName();
        var map = Path.GetTempFileName();
        var run = TestHelpers.RunZuke($"import {TestHelpers.QuoteArg(input)} -o {TestHelpers.QuoteArg(md)} --report {TestHelpers.QuoteArg(report)} --map {TestHelpers.QuoteArg(map)}");
        TestHelpers.AssertExitCode(run, 0);
        Assert.Contains("Lawtext Import Report", File.ReadAllText(report));
        var json = File.ReadAllText(map);
        Assert.Contains("\"Kind\": \"Article\"", json);
        Assert.Contains("\"MarkdownLine\":", json);
    }

    [Fact]
    public void AuditCommandWorks()
    {
        var input = Path.Combine(TestHelpers.RepoRoot, "samples", "import-source.law.txt");
        var run = TestHelpers.RunZuke($"audit {TestHelpers.QuoteArg(input)}");
        TestHelpers.AssertExitCode(run, 0);
    }
}
