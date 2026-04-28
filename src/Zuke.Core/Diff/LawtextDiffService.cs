using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

namespace Zuke.Core.Diff;

public sealed class LawtextDiffService
{
    public DiffResult Diff(string oldText, string newText, DiffOptions options)
    {
        var diff = new SideBySideDiffBuilder(new Differ()).BuildDiffModel(oldText, newText);
        var oldLines = oldText.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        var newLines = newText.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');

        var lines = new List<DiffLine>();
        var oldIndex = 1;
        var newIndex = 1;

        foreach (var pair in diff.OldText.Lines.Zip(diff.NewText.Lines, (o, n) => (o, n)))
        {
            if (pair.o.Type == ChangeType.Deleted || pair.n.Type == ChangeType.Imaginary)
            {
                lines.Add(new DiffLine('-', pair.o.Text, oldIndex++, 0));
                continue;
            }

            if (pair.n.Type == ChangeType.Inserted || pair.o.Type == ChangeType.Imaginary)
            {
                lines.Add(new DiffLine('+', pair.n.Text, 0, newIndex++));
                continue;
            }

            var kind = pair.o.Type == ChangeType.Unchanged && pair.n.Type == ChangeType.Unchanged ? ' ' : '~';
            lines.Add(new DiffLine(kind, pair.n.Text, oldIndex++, newIndex++));
        }

        var header = $"@@ -1,{Math.Max(1, oldLines.Length)} +1,{Math.Max(1, newLines.Length)} @@";
        var unified = string.Join('\n', lines.Select(l => $"{(l.Kind == '~' ? ' ' : l.Kind)}{l.Text}"));
        return new(lines.Any(x => x.Kind != ' '), unified, [new(header, lines)]);
    }
}
