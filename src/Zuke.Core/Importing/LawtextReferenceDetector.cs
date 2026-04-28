using System.Text.RegularExpressions;
using Zuke.Core.Model;

namespace Zuke.Core.Importing;

public sealed class LawtextReferenceDetector
{
    private static readonly Regex CandidateRegex = new(@"第?[0-9０-９一二三四五六七八九十百千]+条(?:第?[0-9０-９一二三四五六七八九十百千]+項(?:第?[0-9０-９一二三四五六七八九十百千]+号)?)?|第?[0-9０-９一二三四五六七八九十百千]+項|第?[0-9０-９一二三四五六七八九十百千]+号|前条|前項|前号|次条|次項|次号|同条|同項|同号|及び|又は|から", RegexOptions.Compiled);

    public IReadOnlyList<LawtextReferenceToken> Detect(string text)
        => CandidateRegex.Matches(text).Select(m => new LawtextReferenceToken(m.Value, m.Index, m.Length)).ToList();
}

public sealed record LawtextReferenceToken(string Text, int Index, int Length);
