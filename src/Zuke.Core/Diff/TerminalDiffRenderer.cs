using Spectre.Console;

namespace Zuke.Core.Diff;

public sealed class TerminalDiffRenderer
{
    public void Render(IAnsiConsole c, DiffResult r, bool color)
    {
        var inline = c.Profile.Width < 100;
        foreach (var l in r.Hunks.SelectMany(x => x.Lines))
        {
            var prefix = inline ? string.Empty : $"{(l.OldLineNumber == 0 ? "" : l.OldLineNumber.ToString()),4} {(l.NewLineNumber == 0 ? "" : l.NewLineNumber.ToString()),4} ";
            if (color && l.Kind == '+') c.MarkupLine($"[green]{Markup.Escape(prefix + "+" + l.Text)}[/]");
            else if (color && l.Kind == '-') c.MarkupLine($"[red]{Markup.Escape(prefix + "-" + l.Text)}[/]");
            else c.WriteLine($"{prefix}{(l.Kind == '~' ? ' ' : l.Kind)}{l.Text}");
        }
    }
}
