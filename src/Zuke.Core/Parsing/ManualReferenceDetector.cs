using System.Text.RegularExpressions;
using Zuke.Core.Model;

namespace Zuke.Core.Parsing;

public static class ManualReferenceDetector
{
    private static readonly Regex R = new(@"(第\d+条(第\d+項)?|前条|前項|前号)", RegexOptions.Compiled);

    public static IEnumerable<DiagnosticMessage> Detect(string text, SourceLocation loc, bool strict)
    {
        if (R.IsMatch(text))
        {
            yield return new(strict ? DiagnosticSeverity.Error : DiagnosticSeverity.Warning, "LMD020", "手書きの番号参照があります。", loc, Array.Empty<SourceLocation>());
        }
    }
}
