using System.Text.RegularExpressions;
using Zuke.Core.Model;

namespace Zuke.Core.Parsing;

public static class ReferenceParser
{
    public static readonly Regex RefRegex = new(@"\{\{(?:参照|ref):(?<name>[^|}]+)(?:\|(?<opt>[^}]+))?\}\}", RegexOptions.Compiled);

    public static bool TryParseOption(string opt, out ReferenceOption option)
    {
        option = ReferenceOption.Auto;
        switch (opt)
        {
            case "":
            case "自動":
            case "auto":
                option = ReferenceOption.Auto;
                return true;
            case "完全":
            case "full":
                option = ReferenceOption.Full;
                return true;
            case "相対":
            case "relative":
            case "rel":
            case "前":
                option = ReferenceOption.Relative;
                return true;
            case "条のみ":
            case "article":
                option = ReferenceOption.ArticleOnly;
                return true;
            default:
                return false;
        }
    }
}
