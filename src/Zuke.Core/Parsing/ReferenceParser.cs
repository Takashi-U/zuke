using System.Text.RegularExpressions;
using Zuke.Core.Model;

namespace Zuke.Core.Parsing;

public static class ReferenceParser
{
    private static readonly Regex RefRegex = new(@"\{\{(?:参照|ref):(?<name>[^|}]+)(?:\|(?<opt>[^}]+))?\}\}", RegexOptions.Compiled);

    public static string ResolveInline(string text, Func<string, ReferenceOption, string> resolver)
    {
        return RefRegex.Replace(text, m => resolver(m.Groups["name"].Value.Trim(), ParseOption(m.Groups["opt"].Value.Trim())));
    }

    private static ReferenceOption ParseOption(string opt) => opt switch
    {
        "" or "自動" or "auto" => ReferenceOption.Auto,
        "完全" or "full" => ReferenceOption.Full,
        "相対" or "relative" or "rel" or "前" => ReferenceOption.Relative,
        "条のみ" or "article" => ReferenceOption.ArticleOnly,
        _ => ReferenceOption.Auto
    };
}
