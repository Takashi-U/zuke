namespace Zuke.Core.Numbering;

public static class JapaneseNumberFormatter
{
    public static string ToArticle(int n, bool arabic)
        => arabic ? $"第{n}条" : $"第{ToKanjiNumber(n)}条";

    public static string ToParagraphNum(int n)
        => n == 1 ? "" : ToFullWidth(n);

    private static string ToKanjiNumber(int n)
    {
        if (n <= 0) return "零";
        if (n <= 10)
        {
            return n switch
            {
                1 => "一",
                2 => "二",
                3 => "三",
                4 => "四",
                5 => "五",
                6 => "六",
                7 => "七",
                8 => "八",
                9 => "九",
                10 => "十",
                _ => n.ToString()
            };
        }

        if (n < 20)
        {
            return "十" + ToKanjiNumber(n % 10);
        }

        if (n % 10 == 0)
        {
            return ToKanjiNumber(n / 10) + "十";
        }

        return ToKanjiNumber(n / 10) + "十" + ToKanjiNumber(n % 10);
    }

    private static string ToFullWidth(int n) => n.ToString()
        .Replace("0", "０", StringComparison.Ordinal)
        .Replace("1", "１", StringComparison.Ordinal)
        .Replace("2", "２", StringComparison.Ordinal)
        .Replace("3", "３", StringComparison.Ordinal)
        .Replace("4", "４", StringComparison.Ordinal)
        .Replace("5", "５", StringComparison.Ordinal)
        .Replace("6", "６", StringComparison.Ordinal)
        .Replace("7", "７", StringComparison.Ordinal)
        .Replace("8", "８", StringComparison.Ordinal)
        .Replace("9", "９", StringComparison.Ordinal);
}
