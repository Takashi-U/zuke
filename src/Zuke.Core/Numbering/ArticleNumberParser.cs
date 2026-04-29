namespace Zuke.Core.Numbering;

public static class ArticleNumberParser
{
    public static bool TryParseArticleNumber(string text, out ArticleNumber number)
        => ArticleNumberFormatter.TryParseArticleNumber(text, out number);

    public static ArticleNumber ParseArticleNumber(string text)
        => ArticleNumberFormatter.ParseArticleNumber(text);
}
