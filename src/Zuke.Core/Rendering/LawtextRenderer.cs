using Zuke.Core.Model;
using Zuke.Core.Numbering;

namespace Zuke.Core.Rendering;

public sealed class LawtextRenderer
{
    private static readonly string[] ForbiddenTokens =
    [
        "{{参照:", "{{ref:", "[条:", "[項:", "[号:", "[a:", "[p:", "[i:"
    ];

    public string Render(CompiledLawDocument compiled, LawtextRenderOptions? options = null)
        => Render(compiled.Document, options);

    public string Render(LawDocumentModel model, LawtextRenderOptions? options = null)
    {
        options ??= LawtextRenderOptions.Default;
        var writer = new LawtextWriter();

        writer.WriteLine(LawtextLineKind.LawTitle, model.Metadata.LawTitle);
        if (options.IncludeLawNum && !string.IsNullOrWhiteSpace(model.Metadata.LawNum))
        {
            writer.WriteLine(LawtextLineKind.LawNum, $"（{model.Metadata.LawNum}）");
        }

        if (options.IncludeBlankLineBetweenBlocks)
        {
            writer.WriteBlankLine();
        }

        var hasAnyTopLevelBlock = false;
        foreach (var chapter in model.Chapters)
        {
            if (options.IncludeBlankLineBetweenBlocks && hasAnyTopLevelBlock)
            {
                writer.WriteBlankLine();
            }

            writer.WriteLine(LawtextLineKind.ChapterTitle,
                $"{options.Layout.ChapterIndent}{JapaneseNumberFormatter.ToChapter(chapter.Number, options.ArabicNumbers)}{options.Layout.Separator}{chapter.Title}");
            hasAnyTopLevelBlock = true;

            foreach (var section in chapter.Sections)
            {
                if (options.IncludeBlankLineBetweenBlocks)
                {
                    writer.WriteBlankLine();
                }

                writer.WriteLine(LawtextLineKind.SectionTitle,
                    $"{options.Layout.SectionIndent}{JapaneseNumberFormatter.ToSection(section.Number, options.ArabicNumbers)}{options.Layout.Separator}{section.Title}");

                foreach (var article in section.Articles)
                {
                    if (options.IncludeBlankLineBetweenBlocks)
                    {
                        writer.WriteBlankLine();
                    }

                    RenderArticle(writer, article, options);
                }
            }

            foreach (var article in chapter.Articles)
            {
                if (options.IncludeBlankLineBetweenBlocks)
                {
                    writer.WriteBlankLine();
                }

                RenderArticle(writer, article, options);
            }
        }

        foreach (var article in model.DirectArticles)
        {
            if (options.IncludeBlankLineBetweenBlocks && hasAnyTopLevelBlock)
            {
                writer.WriteBlankLine();
            }

            RenderArticle(writer, article, options);
            hasAnyTopLevelBlock = true;
        }

        foreach (var supplementaryProvision in model.SupplementaryProvisions)
        {
            if (options.IncludeBlankLineBetweenBlocks && hasAnyTopLevelBlock)
            {
                writer.WriteBlankLine();
            }

            writer.WriteLine(LawtextLineKind.ArticleParagraph, supplementaryProvision.Title);
            if (options.IncludeBlankLineBetweenBlocks)
            {
                writer.WriteBlankLine();
            }

            foreach (var line in supplementaryProvision.Lines)
            {
                writer.WriteLine(LawtextLineKind.Paragraph, line);
            }
            hasAnyTopLevelBlock = true;
        }

        var text = writer.ToString(options);
        text = new LawtextNormalizer().Normalize(text, new LawtextNormalizeOptions
        {
            EnsureFinalNewline = options.EnsureFinalNewline,
            NormalizeLineEndings = options.NormalizeLineEndings,
            TrimTrailingWhitespace = options.TrimTrailingWhitespace
        });

        return text;
    }

    public static IReadOnlyList<DiagnosticMessage> ValidateRenderedText(string lawtext, SourceLocation? location = null)
    {
        var diagnostics = new List<DiagnosticMessage>();
        if (ForbiddenTokens.Any(t => lawtext.Contains(t, StringComparison.Ordinal)))
        {
            diagnostics.Add(new(DiagnosticSeverity.Error, "LMD064", "Lawtextに参照名ラベルまたは参照マクロが残っています。", location, []));
        }

        if (lawtext.Contains("🍣", StringComparison.Ordinal) || lawtext.Contains("[OK]", StringComparison.Ordinal))
        {
            diagnostics.Add(new(DiagnosticSeverity.Error, "LMD065", "Lawtext本文に絵文字または機械出力に不要なステータス文字列が混入しています。", location, []));
        }

        return diagnostics;
    }

    private static void RenderArticle(LawtextWriter writer, ArticleNode article, LawtextRenderOptions options)
    {
        if (!string.IsNullOrWhiteSpace(article.Caption))
        {
            writer.WriteLine(LawtextLineKind.ArticleCaption,
                $"{options.Layout.ArticleCaptionIndent}（{article.Caption}）");
        }

        var first = article.Paragraphs.FirstOrDefault();
        var firstSentence = first?.SentenceText ?? string.Empty;
        writer.WriteLine(LawtextLineKind.ArticleParagraph,
            string.IsNullOrWhiteSpace(firstSentence)
                ? article.ArticleTitle
                : $"{article.ArticleTitle}{options.Layout.Separator}{firstSentence}");

        if (first is not null)
        {
            RenderItems(writer, first.Items, options, true);
        }

        foreach (var paragraph in article.Paragraphs.Skip(1))
        {
            var paraNum = string.IsNullOrWhiteSpace(paragraph.ParagraphNumText)
                ? ToFullWidth(paragraph.Number)
                : paragraph.ParagraphNumText;
            var sentence = paragraph.SentenceText;
            var line = string.IsNullOrWhiteSpace(sentence)
                ? paraNum
                : $"{paraNum}{options.Layout.Separator}{sentence}";
            writer.WriteLine(LawtextLineKind.Paragraph, line);
            RenderItems(writer, paragraph.Items, options, false);
        }
    }

    private static void RenderItems(LawtextWriter writer, IReadOnlyList<ItemNode> items, LawtextRenderOptions options, bool _)
    {
        foreach (var item in items)
        {
            writer.WriteLine(LawtextLineKind.Item,
                $"{options.Layout.ItemIndent}{JapaneseNumberFormatter.ToItemTitle(item.Number, options.ArabicNumbers)}{options.Layout.Separator}{item.SentenceText}");

            foreach (var subitem in item.Children)
            {
                writer.WriteLine(LawtextLineKind.Subitem1,
                    $"{options.Layout.Subitem1Indent}{subitem.ItemTitle}{options.Layout.Separator}{subitem.SentenceText}");
            }
        }
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
