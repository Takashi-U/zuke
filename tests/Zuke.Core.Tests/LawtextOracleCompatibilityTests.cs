using System.Text;
using Xunit;

namespace Zuke.Core.Tests;

public class LawtextOracleCompatibilityTests
{
    [Fact]
    public void OracleCompatibility_IsOptional()
    {
        if (Environment.GetEnvironmentVariable("ZUKE_RUN_LAWTEXT_ORACLE") != "1")
        {
            return;
        }

        var nodeCheck = TestHelpers.RunProcess("bash", "-lc 'command -v node >/dev/null && command -v npm >/dev/null'");
        if (nodeCheck.ExitCode != 0)
        {
            return;
        }

        var markdown = TestHelpers.ReadFixture("minimal.md");
        var lawtext = TestHelpers.RenderLawtext(markdown);
        var temp = Path.GetTempFileName();
        File.WriteAllText(temp, lawtext, new UTF8Encoding(false));

        var verify = TestHelpers.RunProcess("node", $"tools/lawtext-oracle/verify-lawtext.mjs \"{temp}\"");
        Assert.True(verify.ExitCode == 0, verify.StdErr + verify.StdOut);
    }
}
