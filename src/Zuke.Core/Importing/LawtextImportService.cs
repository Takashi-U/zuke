using Zuke.Core.Compilation;
using Zuke.Core.Model;
using Zuke.Core.References;
using Zuke.Core.Rendering;
using Zuke.Core.Validation;

namespace Zuke.Core.Importing;

public sealed class LawtextImportService
{
    public LawtextImportResult Import(string lawtext, string? inputPath, LawtextImportOptions options)
    {
        var parser = new LawtextParser();
        var (parsed, parseDiags) = parser.Parse(lawtext, inputPath);

        var model = new ReferenceNameGenerator().Apply(parsed, options);
        var allDiags = new List<DiagnosticMessage>(parseDiags);

        var (table, refDiags) = new ReferenceTableBuilder().Build(model);
        allDiags.AddRange(refDiags);

        var usedRefs = new HashSet<string>(StringComparer.Ordinal);
        var resolver = new LawtextReferenceResolver();

        ArticleNode ResolveArticle(ArticleNode article)
        {
            var paragraphs = article.Paragraphs.Select(p =>
            {
                var sentence = resolver.Resolve(p.SentenceText, article, p, null, table, allDiags, p.Location, options, usedRefs);
                var items = p.Items.Select(i => i with
                {
                    SentenceText = resolver.Resolve(i.SentenceText, article, p, i, table, allDiags, i.Location, options, usedRefs),
                    Children = i.Children.Select(c => c with { SentenceText = resolver.Resolve(c.SentenceText, article, p, c, table, allDiags, c.Location, options, usedRefs) }).ToList()
                }).ToList();
                return p with { SentenceText = sentence, Items = items };
            }).ToList();

            return article with { Paragraphs = paragraphs };
        }

        model = model with
        {
            Chapters = model.Chapters.Select(ch => ch with
            {
                Sections = ch.Sections.Select(s => s with { Articles = s.Articles.Select(ResolveArticle).ToList() }).ToList(),
                Articles = ch.Articles.Select(ResolveArticle).ToList()
            }).ToList(),
            DirectArticles = model.DirectArticles.Select(ResolveArticle).ToList()
        };

        var renderOptions = new ExtendedMarkdownRenderOptions(options.ReferenceLabels, options.MetadataMode);
        var markdown = new ExtendedMarkdownRenderer().Render(model, renderOptions);

        if (!options.SkipRoundtripCheck)
        {
            var compile = new LawMarkdownCompiler().Compile(markdown, inputPath, new CompileOptions(options.Strict, options.ArabicNumbers));
            if (compile.HasErrors || compile.Document is null)
            {
                allDiags.Add(new(options.Strict ? DiagnosticSeverity.Error : DiagnosticSeverity.Warning, "LMD098", "Lawtext import後のMarkdownを再コンパイルできません。", new(inputPath, 1, 1), []));
            }
            else
            {
                var xml = new LawXmlRenderer().Render(compile.Document.Document, LawXmlRenderOptions.Default);
                var xsd = ZukeXsdProvider.ResolveDefaultPath();
                allDiags.AddRange(new LawXmlValidator().Validate(xml, xsd));
                var lawtextOut = new LawtextRenderer().Render(compile.Document);
                allDiags.AddRange(LawtextRenderer.ValidateRenderedText(lawtextOut, new(inputPath, 1, 1)));
            }
        }

        return new LawtextImportResult(markdown, allDiags);
    }
}
