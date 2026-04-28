using System.Text;
using Xunit;
using Zuke.Core.Rendering;

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
        var markdown = File.ReadAllText(markdownPath, Encoding.UTF8);
        var expected = File.ReadAllText(expectedPath, Encoding.UTF8);
        var actual = TestHelpers.RenderLawtext(markdown);

        var normalizer = new LawtextNormalizer();
        var normalizedExpected = normalizer.Normalize(expected, LawtextNormalizeOptions.Default);
        var normalizedActual = normalizer.Normalize(actual, LawtextNormalizeOptions.Default);

        Assert.Equal(normalizedExpected, normalizedActual);
        Assert.DoesNotContain("{{", actual, StringComparison.Ordinal);
        Assert.DoesNotContain("[条:", actual, StringComparison.Ordinal);
    }
}
