using System.Text;
using Xunit;

namespace Zuke.Core.Tests;

public class LawtextCliIntegrationTests
{
    [Fact]
    public void LawtextAndConvertOutputsMatch()
    {
        var input = Path.Combine(TestHelpers.RepoRoot, "samples", "work-rules.md");
        Assert.True(File.Exists(input), $"Missing sample file: {input}");
        var out1 = Path.GetTempFileName();
        var out2 = Path.GetTempFileName();

        var run1 = TestHelpers.RunZuke($"lawtext {TestHelpers.QuoteArg(input)} -o {TestHelpers.QuoteArg(out1)}");
        var run2 = TestHelpers.RunZuke($"convert {TestHelpers.QuoteArg(input)} -o {TestHelpers.QuoteArg(out2)} --to lawtext");

        TestHelpers.AssertExitCode(run1, 0);
        TestHelpers.AssertExitCode(run2, 0);

        var bytes = File.ReadAllBytes(out1);
        Assert.False(bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF);

        var text1 = File.ReadAllText(out1, Encoding.UTF8);
        var text2 = File.ReadAllText(out2, Encoding.UTF8);
        Assert.Equal(text1, text2);
        Assert.DoesNotContain("\r", text1);
        Assert.Contains("就業規則", text1);
        Assert.Contains("第一条", text1);
        Assert.DoesNotContain("{{参照:", text1);
        Assert.DoesNotContain("🍣", text1);
    }

    [Fact]
    public void DiffUsesNormalizedLawtext()
    {
        var oldMd = Path.GetTempFileName();
        var newMd = Path.GetTempFileName();
        File.WriteAllText(oldMd, TestHelpers.ReadFixture("references.md"));
        File.WriteAllText(newMd, TestHelpers.ReadFixture("references.md").Replace("届け出なければ", "提出しなければ"));

        var run = TestHelpers.RunZuke($"diff {TestHelpers.QuoteArg(oldMd)} {TestHelpers.QuoteArg(newMd)}");
        TestHelpers.AssertExitCode(run, 1);
        Assert.Contains("+", run.StdOut);
    }
}
