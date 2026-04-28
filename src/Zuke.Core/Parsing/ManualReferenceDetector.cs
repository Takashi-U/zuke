using System.Text.RegularExpressions;
using Zuke.Core.Model;

namespace Zuke.Core.Parsing;

public static class ManualReferenceDetector
{
    private static readonly Regex NumericReferenceRegex = new(
        @"(第(?:\d+|[一二三四五六七八九十百千]+)条(?:第(?:\d+|[一二三四五六七八九十百千]+)項)?|第(?:\d+|[一二三四五六七八九十百千]+)項|第(?:\d+|[一二三四五六七八九十百千]+)号)",
        RegexOptions.Compiled);

    private static readonly Regex RelativeReferenceRegex = new(
        @"(前条|前項|前号|次条|次項|次号|同条|同項|同号)",
        RegexOptions.Compiled);

    public static IEnumerable<DiagnosticMessage> Detect(string text, SourceLocation loc, bool strict)
    {
        var textWithoutReferenceMacros = ReferenceParser.RefRegex.Replace(text, string.Empty);

        if (NumericReferenceRegex.IsMatch(textWithoutReferenceMacros))
        {
            yield return new(strict ? DiagnosticSeverity.Error : DiagnosticSeverity.Warning, "LMD020", "手書きの番号参照があります。", loc, Array.Empty<SourceLocation>());
        }

        if (RelativeReferenceRegex.IsMatch(textWithoutReferenceMacros))
        {
            yield return new(strict ? DiagnosticSeverity.Error : DiagnosticSeverity.Warning, "LMD028", "手書きの相対参照があります。", loc, Array.Empty<SourceLocation>());
        }
    }
}
