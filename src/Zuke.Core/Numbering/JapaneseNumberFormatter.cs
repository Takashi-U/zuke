namespace Zuke.Core.Numbering;

public static class JapaneseNumberFormatter
{
    public static string ToArticle(int n, bool arabic)
        => $"第{FormatNumber(n, arabic)}条";

    public static string ToChapter(int n, bool arabic)
        => $"第{FormatNumber(n, arabic)}章";

    public static string ToSection(int n, bool arabic)
        => $"第{FormatNumber(n, arabic)}節";

    public static string ToParagraphReference(int n, bool arabic)
        => $"第{FormatNumber(n, arabic)}項";

    public static string ToItemReference(int n, bool arabic)
        => $"第{FormatNumber(n, arabic)}号";

    public static string ToItemTitle(int n, bool arabic)
        => FormatNumber(n, arabic);

    public static string ToParagraphNum(int n)
        => n == 1 ? "" : ToFullWidth(n);

    public static string ToKanjiNumber(int n)
    {
        if (n <= 0) return "零";

        if (n < 10)
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
                _ => n.ToString()
            };
        }

        if (n == 10) return "十";

        static string Under100(int value)
        {
            if (value < 10) return ToKanjiNumber(value);
            if (value == 10) return "十";
            var tens = value / 10;
            var ones = value % 10;
            var tensPart = tens == 1 ? "十" : $"{ToKanjiNumber(tens)}十";
            return ones == 0 ? tensPart : $"{tensPart}{ToKanjiNumber(ones)}";
        }

        static string Under10000(int value)
        {
            var thousands = value / 1000;
            var hundreds = (value % 1000) / 100;
            var under100 = value % 100;
            var result = string.Empty;
            if (thousands > 0) result += (thousands == 1 ? string.Empty : ToKanjiNumber(thousands)) + "千";
            if (hundreds > 0) result += (hundreds == 1 ? string.Empty : ToKanjiNumber(hundreds)) + "百";
            if (under100 > 0) result += Under100(under100);
            return result;
        }

        return Under10000(n);
    }

    private static string FormatNumber(int n, bool arabic) => arabic ? n.ToString() : ToKanjiNumber(n);

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
