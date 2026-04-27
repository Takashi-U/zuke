namespace Zuke.Core.Rendering;

public sealed class LawtextNormalizer
{
    public string Normalize(string lawtext, LawtextNormalizeOptions options)
    {
        var text = lawtext ?? string.Empty;
        if (options.NormalizeLineEndings)
        {
            text = text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
        }

        var lines = text.Split('\n');
        if (options.TrimTrailingWhitespace)
        {
            for (var i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].TrimEnd();
            }
        }

        if (options.CollapseExcessBlankLines)
        {
            var collapsed = new List<string>(lines.Length);
            var blanks = 0;
            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line))
                {
                    blanks++;
                    if (blanks > options.MaxConsecutiveBlankLines)
                    {
                        continue;
                    }
                }
                else
                {
                    blanks = 0;
                }

                collapsed.Add(line);
            }

            lines = [.. collapsed];
        }

        text = string.Join("\n", lines).TrimEnd('\n');
        if (options.EnsureFinalNewline)
        {
            text += "\n";
        }

        return text;
    }
}
