using System.Text;
using Xunit;
using Zuke.Core.Diff;
using Zuke.Core.Rendering;

namespace Zuke.Core.Tests;

public class LawtextGoldenTests
{
    private static readonly string[] RequiredFixtures =
    [
        "minimal", "chapter-section", "paragraphs", "items", "references", "relative-references", "direct-articles", "article-reference", "item-reference", "subitem1"
    ];

    public static IEnumerable<object[]> Cases()
    {
        var baseDir = Path.Combine(TestHelpers.RepoRoot, "tests", "Zuke.Core.Tests", "Fixtures", "Lawtext");
        foreach (var name in RequiredFixtures)
        {
            yield return new object[]
            {
                Path.Combine(baseDir, $"{name}.md"),
                Path.Combine(baseDir, $"{name}.expected.law.txt")
            };
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

        if (!string.Equals(normalizedExpected, normalizedActual, StringComparison.Ordinal))
        {
            var diff = new UnifiedDiffRenderer().Render(expectedPath, markdownPath, new LawtextDiffService().Diff(normalizedExpected, normalizedActual, new DiffOptions(3)));
            throw new Xunit.Sdk.XunitException("Lawtext golden mismatch:\n" + diff);
        }

        Assert.Equal(normalizedExpected, normalizedActual);
        Assert.DoesNotContain("{{", actual, StringComparison.Ordinal);
        Assert.DoesNotContain("[条:", actual, StringComparison.Ordinal);
        Assert.DoesNotContain("🍣", actual, StringComparison.Ordinal);
    }
}
