using System.Text;

namespace Zuke.Core.Rendering;

public sealed class LawtextWriter
{
    private readonly List<LawtextLine> _lines = [];

    public void WriteLine(LawtextLineKind kind, string text)
    {
        _lines.Add(new LawtextLine(kind, text));
    }

    public void WriteBlankLine()
    {
        if (_lines.Count == 0 || _lines[^1].Kind == LawtextLineKind.Blank)
        {
            return;
        }

        _lines.Add(new LawtextLine(LawtextLineKind.Blank, string.Empty));
    }

    public override string ToString() => ToString(LawtextRenderOptions.Default);

    public string ToString(LawtextRenderOptions options)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < _lines.Count; i++)
        {
            var line = _lines[i];
            var text = line.Text;
            if (options.NormalizeLineEndings)
            {
                text = text.Replace("\r\n", "\n", StringComparison.Ordinal)
                    .Replace('\r', '\n');
            }

            if (options.TrimTrailingWhitespace)
            {
                text = text.TrimEnd();
            }

            if (text.Contains('\n'))
            {
                foreach (var chunk in text.Split('\n'))
                {
                    sb.Append(chunk);
                    sb.Append('\n');
                }
            }
            else
            {
                sb.Append(text);
                sb.Append('\n');
            }
        }

        var output = sb.ToString();
        if (!options.EnsureFinalNewline)
        {
            return output.TrimEnd('\n');
        }

        return output.Length == 0 || output.EndsWith('\n') ? output : output + "\n";
    }
}
