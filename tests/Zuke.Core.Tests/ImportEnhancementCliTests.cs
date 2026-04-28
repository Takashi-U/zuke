using Xunit;

namespace Zuke.Core.Tests;

public class ImportEnhancementCliTests
{
    [Fact]
    public void ImportCommandCreatesReportAndMap()
    {
        var md = Path.GetTempFileName();
        var report = Path.GetTempFileName();
        var map = Path.GetTempFileName();
        var run = TestHelpers.RunProcess("dotnet", $"run --project src/Zuke.Cli -- import samples/import-source.law.txt -o {md} --report {report} --map {map}");
        Assert.Equal(0, run.ExitCode);
        Assert.Contains("Lawtext Import Report", File.ReadAllText(report));
        var json = File.ReadAllText(map);
        Assert.Contains("\"Kind\": \"Article\"", json);
        Assert.Contains("\"MarkdownLine\":", json);
    }

    [Fact]
    public void AuditCommandWorks()
    {
        var run = TestHelpers.RunProcess("dotnet", "run --project src/Zuke.Cli -- audit samples/import-source.law.txt");
        Assert.Equal(0, run.ExitCode);
    }
}
