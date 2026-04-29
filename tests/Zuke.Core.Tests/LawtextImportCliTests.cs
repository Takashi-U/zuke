using Xunit;

namespace Zuke.Core.Tests;

public class LawtextImportCliTests
{
    [Fact]
    public void ImportCommandCreatesMarkdown()
    {
        var input = Path.Combine(TestHelpers.RepoRoot, "samples", "import-source.law.txt");
        Assert.True(File.Exists(input), $"Missing sample file: {input}");
        var outPath = Path.GetTempFileName();
        var run = TestHelpers.RunZuke($"import {TestHelpers.QuoteArg(input)} -o {TestHelpers.QuoteArg(outPath)}");
        TestHelpers.AssertExitCode(run, 0);
        var md = File.ReadAllText(outPath);
        Assert.Contains("---", md);
        Assert.Contains("# 総則", md);
        Assert.Contains("{{参照:", md);
    }
}
