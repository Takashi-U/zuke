using Zuke.Core.Markdown;
using Zuke.Core.Model;
using Zuke.Core.Numbering;
using Zuke.Core.Parsing;
using Zuke.Core.References;
using Zuke.Core.Validation;

namespace Zuke.Core.Compilation;

public sealed class LawMarkdownCompiler
{
    public CompileResult Compile(string markdown, string? filePath, CompileOptions options)
    {
        var fm = FrontMatterParser.ParseDetailed(markdown);
        var model = new MarkdownLawParser().Parse(markdown, filePath);
        var diags = new List<DiagnosticMessage>(model.Diagnostics);
        if (options.RequireFrontMatter)
        {
            diags.AddRange(FrontMatterParser.ValidateRequired(fm, filePath));
        }
        diags.AddRange(new LawStructureValidator().Validate(model));
        foreach (var article in EnumerateArticles(model))
        {
            foreach (var paragraph in article.Paragraphs)
            {
                diags.AddRange(ManualReferenceDetector.Detect(paragraph.SentenceText, paragraph.Location ?? article.Location ?? new(filePath, 1, 1), options.Strict));
                foreach (var item in paragraph.Items)
                {
                    DetectItemRecursive(item, item.Location ?? paragraph.Location ?? article.Location ?? new(filePath, 1, 1), options.Strict, diags);
                }
            }
        }

        model = new NumberingService().Apply(model, options.ArabicNumbers);
        var (table, refDiags) = new ReferenceTableBuilder().Build(model);
        diags.AddRange(refDiags);
        var (resolved, resolveDiags) = new ReferenceResolver().Resolve(model, table, options.ArabicNumbers);
        diags.AddRange(resolveDiags);
        var compiled = new CompiledLawDocument(resolved, table, diags);
        return new CompileResult(compiled, diags);
    }

    private static IEnumerable<ArticleNode> EnumerateArticles(LawDocumentModel model)
    {
        foreach (var article in model.DirectArticles)
        {
            yield return article;
        }

        foreach (var chapter in model.Chapters)
        {
            foreach (var article in chapter.Articles)
            {
                yield return article;
            }

            foreach (var section in chapter.Sections)
            {
                foreach (var article in section.Articles)
                {
                    yield return article;
                }
            }
        }
    }

    private static void DetectItemRecursive(ItemNode item, SourceLocation location, bool strict, List<DiagnosticMessage> diags)
    {
        diags.AddRange(ManualReferenceDetector.Detect(item.SentenceText, location, strict));
        foreach (var child in item.Children)
        {
            DetectItemRecursive(child, child.Location ?? location, strict, diags);
        }
    }
}
