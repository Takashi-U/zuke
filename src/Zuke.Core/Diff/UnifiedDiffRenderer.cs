using System.Text;

namespace Zuke.Core.Diff;

public sealed class UnifiedDiffRenderer
{
    public string Render(string oldName, string newName, DiffResult r)
    {
        if (!r.HasChanges)
        {
            return $"--- {oldName}\n+++ {newName}\n差分はありません。\n";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"--- {oldName}");
        sb.AppendLine($"+++ {newName}");
        foreach (var hunk in r.Hunks)
        {
            sb.AppendLine(hunk.Header);
            foreach (var line in hunk.Lines)
            {
                var kind = line.Kind == '~' ? ' ' : line.Kind;
                sb.Append(kind).AppendLine(line.Text);
            }
        }

        return sb.ToString();
    }
}
