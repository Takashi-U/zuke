using System.Text.RegularExpressions;

namespace Zuke.Core.Importing;

public sealed class LawtextReferenceDetector
{
    private static readonly Regex[] ProtectedCompositeRegexes =
    [
        new Regex(@"(?:本条|同条|前条|次条)第[0-9０-９一二三四五六七八九十百千]+項、第[0-9０-９一二三四五六七八九十百千]+項から第[0-9０-９一二三四五六七八九十百千]+項", RegexOptions.Compiled),
        new Regex(@"(?:本条|同条|前条|次条)第[0-9０-９一二三四五六七八九十百千]+項、第[0-9０-９一二三四五六七八九十百千]+項又は第[0-9０-９一二三四五六七八九十百千]+項", RegexOptions.Compiled),
        new Regex(@"第[0-9０-９一二三四五六七八九十百千]+項から第[0-9０-９一二三四五六七八九十百千]+項", RegexOptions.Compiled),
        new Regex(@"第[0-9０-９一二三四五六七八九十百千]+項又は第[0-9０-９一二三四五六七八九十百千]+項", RegexOptions.Compiled),
        new Regex(@"第[0-9０-９一二三四五六七八九十百千]+項、第[0-9０-９一二三四五六七八九十百千]+項", RegexOptions.Compiled),
        new Regex(@"第[0-9０-９一二三四五六七八九十百千]+条第[0-9０-９一二三四五六七八九十百千]+項から第[0-9０-９一二三四五六七八九十百千]+項", RegexOptions.Compiled),
        new Regex(@"第[0-9０-９一二三四五六七八九十百千]+条第[0-9０-９一二三四五六七八九十百千]+項又は第[0-9０-９一二三四五六七八九十百千]+項", RegexOptions.Compiled)
    ];
    private static readonly Regex CandidateRegex = new(@"第?[0-9０-９一二三四五六七八九十百千]+条(?:の[0-9０-９一二三四五六七八九十百千]+)*(?:第?[0-9０-９一二三四五六七八九十百千]+項(?:第?[0-9０-９一二三四五六七八九十百千]+号)?)?|第?[0-9０-９一二三四五六七八九十百千]+項|第?[0-9０-９一二三四五六七八九十百千]+号|前条|前項|前号|次条|次項|次号|同条|同項|同号|及び|又は|から", RegexOptions.Compiled);

    public IReadOnlyList<LawtextReferenceToken> Detect(string text)
    {
        var protectedRanges = ProtectedCompositeRegexes
            .SelectMany(regex => regex.Matches(text).Select(m => (start: m.Index, end: m.Index + m.Length)))
            .ToList();
        return CandidateRegex.Matches(text)
            .Where(m => !protectedRanges.Any(r => m.Index >= r.start && (m.Index + m.Length) <= r.end))
            .Select(m => new LawtextReferenceToken(m.Value, m.Index, m.Length))
            .ToList();
    }
}

public sealed record LawtextReferenceToken(string Text, int Index, int Length);
