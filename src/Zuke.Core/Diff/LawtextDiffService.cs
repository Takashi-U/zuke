using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

namespace Zuke.Core.Diff;

public sealed class LawtextDiffService
{
    public DiffResult Diff(string oldText, string newText, DiffOptions options)
    {
        var diff = new InlineDiffBuilder(new Differ()).BuildDiffModel(oldText, newText);
        var lines = diff.Lines.Select(l => new DiffLine(l.Type switch
        {
            ChangeType.Inserted => '+',
            ChangeType.Deleted => '-',
            _ => ' '
        }, l.Text)).ToList();

        var unified = string.Join('\n', lines.Select(l => $"{l.Kind}{l.Text}"));
        return new(lines.Any(x => x.Kind != ' '), unified, [new("@@", lines)]);
    }
}
