using Xunit;

namespace Zuke.Core.Tests;

public class LawtextImportCliTests
{
    [Fact]
    public void ImportCommandCreatesMarkdown()
    {
        var outPath = Path.GetTempFileName();
        var run = TestHelpers.RunProcess("dotnet", $"run --project src/Zuke.Cli -- import samples/import-source.law.txt -o {outPath}");
        Assert.Equal(0, run.ExitCode);
        var md = File.ReadAllText(outPath);
        Assert.Contains("---", md);
        Assert.Contains("# 総則", md);
        Assert.Contains("{{参照:", md);
    }
}
