using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

namespace Zuke.Core.Diff;

public sealed class LawtextDiffService
{
    public DiffResult Diff(string oldText, string newText, DiffOptions options)
    {
        var diff = new SideBySideDiffBuilder(new Differ()).BuildDiffModel(oldText, newText);

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

            if (pair.o.Type == ChangeType.Modified || pair.n.Type == ChangeType.Modified)
            {
                lines.Add(new DiffLine('-', pair.o.Text, oldIndex++, 0));
                lines.Add(new DiffLine('+', pair.n.Text, 0, newIndex++));
                continue;
            }

            lines.Add(new DiffLine(' ', pair.n.Text, oldIndex++, newIndex++));
        }

        var hunks = BuildHunks(lines, Math.Max(0, options.Context));
        var unified = string.Join('\n', hunks.SelectMany(h => new[] { h.Header }.Concat(h.Lines.Select(l => $"{(l.Kind == '~' ? ' ' : l.Kind)}{l.Text}"))));
        return new(hunks.Any(), unified, hunks);
    }

    private static List<DiffHunk> BuildHunks(IReadOnlyList<DiffLine> lines, int context)
    {
        var changeIndexes = lines
            .Select((line, index) => (line, index))
            .Where(x => x.line.Kind is '+' or '-' or '~')
            .Select(x => x.index)
            .ToList();

        if (changeIndexes.Count == 0) return [];

        var ranges = new List<(int start, int end)>();
        foreach (var idx in changeIndexes)
        {
            var start = Math.Max(0, idx - context);
            var end = Math.Min(lines.Count - 1, idx + context);
            if (ranges.Count == 0 || start > ranges[^1].end + 1)
            {
                ranges.Add((start, end));
            }
            else
            {
                ranges[^1] = (ranges[^1].start, Math.Max(ranges[^1].end, end));
            }
        }

        var hunks = new List<DiffHunk>();
        foreach (var (start, end) in ranges)
        {
            var hunkLines = lines.Skip(start).Take(end - start + 1).ToList();
            var oldStart = hunkLines.First(l => l.OldLineNumber > 0).OldLineNumber;
            var newStart = hunkLines.First(l => l.NewLineNumber > 0).NewLineNumber;
            var oldLen = hunkLines.Count(l => l.OldLineNumber > 0);
            var newLen = hunkLines.Count(l => l.NewLineNumber > 0);
            var header = $"@@ -{oldStart},{oldLen} +{newStart},{newLen} @@";
            hunks.Add(new DiffHunk(header, hunkLines));
        }

        return hunks;
    }
}
