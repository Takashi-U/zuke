using Zuke.Core.Compilation;
using Xunit;

namespace Zuke.Core.Tests;

public class LawtextImportRoundTripTests
{
    [Fact]
    public void ImportedMarkdownCanCompile()
    {
        var text = File.ReadAllText(Path.Combine(TestHelpers.RepoRoot, "samples", "import-source.law.txt"));
        var imported = new Zuke.Core.Importing.LawtextImportService().Import(text, "sample", new());
        var compiled = new LawMarkdownCompiler().Compile(imported.Markdown, "imported.md", new CompileOptions(false, false));
        Assert.False(compiled.HasErrors);
        Assert.NotNull(compiled.Document);
    }
}
