using Xunit;

namespace Zuke.Core.Tests;

public class LawtextGoldenTests
{
    public static IEnumerable<object[]> Cases()
    {
        var baseDir = Path.Combine(TestHelpers.RepoRoot, "tests", "Zuke.Core.Tests", "Fixtures", "Lawtext");
        foreach (var md in Directory.GetFiles(baseDir, "*.md"))
        {
            var expected = Path.ChangeExtension(md, "expected.law.txt");
            if (File.Exists(expected))
            {
                yield return new object[] { md, expected };
            }
        }
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void Golden(string markdownPath, string expectedPath)
    {
        var markdown = File.ReadAllText(markdownPath);
        var expected = File.ReadAllText(expectedPath).Replace("\r\n", "\n");
        var actual = TestHelpers.RenderLawtext(markdown);

        foreach (var line in expected.Split('\n').Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            Assert.Contains(line.TrimEnd(), actual);
        }

        Assert.DoesNotContain("{{", actual);
        Assert.DoesNotContain("[条:", actual);
    }
}
