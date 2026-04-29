using Spectre.Console;
using Spectre.Console.Cli;
using Zuke.Cli.Console;
using Zuke.Core.Compilation;
using Zuke.Core.Markdown;
using Zuke.Core.Rendering;
using Zuke.Core.Validation;

namespace Zuke.Cli.Commands;

public sealed class ConvertCommand : Command<ConvertCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<input>")] public string Input { get; set; } = string.Empty;
        [CommandOption("-o|--output <PATH>")] public string Output { get; set; } = string.Empty;
        [CommandOption("--to <FORMAT>")] public string To { get; set; } = "xml";
        [CommandOption("--skip-validation")] public bool SkipValidation { get; set; }
        [CommandOption("--xsd <PATH>")] public string? Xsd { get; set; }
        [CommandOption("--strict")] public bool Strict { get; set; }
        [CommandOption("--xml-output <PATH>")] public string? XmlOutput { get; set; }
        [CommandOption("--lawtext-output <PATH>")] public string? LawtextOutput { get; set; }
        [CommandOption("--number-style <STYLE>")] public string NumberStyle { get; set; } = "kanji";
        [CommandOption("--emoji <MODE>")] public string Emoji { get; set; } = "auto";
        [CommandOption("--no-color")] public bool NoColor { get; set; }
        [CommandOption("--plain")] public bool Plain { get; set; }
        [CommandOption("--metadata-profile <PROFILE>")] public string MetadataProfile { get; set; } = "default";
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var consoleOptions = ConsoleOptions.From(settings.Plain, settings.Emoji, settings.NoColor);
        var reporter = new ConsoleReporter(AnsiConsole.Console, consoleOptions);
        var text = File.ReadAllText(settings.Input);
        var frontMatter = FrontMatterParser.ParseDetailed(text);
        var requireFrontMatter = !string.Equals(settings.To, "lawtext", StringComparison.OrdinalIgnoreCase);
        var result = new LawMarkdownCompiler().Compile(text, settings.Input, new CompileOptions(settings.Strict, settings.NumberStyle == "arabic", requireFrontMatter));
        reporter.ReportDiagnostics(result.Diagnostics);
        if (result.HasErrors || result.Document is null) return 1;


        if (settings.To == "both")
        {
            if (string.IsNullOrWhiteSpace(settings.XmlOutput) || string.IsNullOrWhiteSpace(settings.LawtextOutput) || !string.IsNullOrWhiteSpace(settings.Output))
            {
                reporter.Info("--to both では --xml-output と --lawtext-output が必須で、-o は使用できません。");
                return 1;
            }
            var lawtext = new LawtextRenderer().Render(result.Document, LawtextRenderOptions.Default with { ArabicNumbers = settings.NumberStyle == "arabic" });
            var renderDiags = LawtextRenderer.ValidateRenderedText(lawtext);
            reporter.ReportDiagnostics(renderDiags);
            if (renderDiags.Any(x => x.Severity == Zuke.Core.Model.DiagnosticSeverity.Error)) return 1;
            var xmlModelBoth = ApplyMetadataProfile(result.Document.Document, settings.MetadataProfile);
            var xmlMetaDiagsBoth = FrontMatterParser.ValidateForXml(xmlModelBoth.Metadata, settings.Input);
            reporter.ReportDiagnostics(xmlMetaDiagsBoth);
            if (xmlMetaDiagsBoth.Any(x => x.Severity == Zuke.Core.Model.DiagnosticSeverity.Error)) return 1;
            var docBoth = new LawXmlRenderer().Render(xmlModelBoth, LawXmlRenderOptions.Default with { ArabicNumbers = settings.NumberStyle == "arabic" });
            if (!settings.SkipValidation)
            {
                var xsd = settings.Xsd ?? ZukeXsdProvider.ResolveDefaultPath();
                var diags = new LawXmlValidator().Validate(docBoth, xsd);
                reporter.ReportDiagnostics(diags);
                if (diags.Any(x => x.Severity == Zuke.Core.Model.DiagnosticSeverity.Error)) return 1;
            }
            docBoth.Save(settings.XmlOutput);
            File.WriteAllText(settings.LawtextOutput, lawtext, new System.Text.UTF8Encoding(false));
            return 0;
        }

        if (settings.To == "lawtext")
        {
            var lawtextModel = ApplyLawtextMetadataFallback(result.Document.Document, frontMatter, settings.Input);
            var renderer = new LawtextRenderer();
            var lawtext = renderer.Render(result.Document with { Document = lawtextModel }, LawtextRenderOptions.Default with { ArabicNumbers = settings.NumberStyle == "arabic" });
            var renderDiags = LawtextRenderer.ValidateRenderedText(lawtext);
            reporter.ReportDiagnostics(renderDiags);
            if (renderDiags.Any(x => x.Severity == Zuke.Core.Model.DiagnosticSeverity.Error)) return 1;

            File.WriteAllText(settings.Output, lawtext, new System.Text.UTF8Encoding(false));
            reporter.Info($"Lawtextを出力しました: {settings.Output}");
            return 0;
        }

        var xmlModel = ApplyMetadataProfile(result.Document.Document, settings.MetadataProfile);
        var xmlMetaDiags = FrontMatterParser.ValidateForXml(xmlModel.Metadata, settings.Input);
        reporter.ReportDiagnostics(xmlMetaDiags);
        if (xmlMetaDiags.Any(x => x.Severity == Zuke.Core.Model.DiagnosticSeverity.Error)) return 1;
        var doc = new LawXmlRenderer().Render(xmlModel, LawXmlRenderOptions.Default with { ArabicNumbers = settings.NumberStyle == "arabic" });
        if (!settings.SkipValidation)
        {
            var xsd = settings.Xsd ?? ZukeXsdProvider.ResolveDefaultPath();
            var diags = new LawXmlValidator().Validate(doc, xsd);
            reporter.ReportDiagnostics(diags);
            if (diags.Any(x => x.Severity == Zuke.Core.Model.DiagnosticSeverity.Error)) return 1;
        }
        doc.Save(settings.Output);
        reporter.Info($"法令XMLを出力しました: {settings.Output}");
        return 0;
    }

    private static Zuke.Core.Model.LawDocumentModel ApplyLawtextMetadataFallback(Zuke.Core.Model.LawDocumentModel model, FrontMatterParseResult frontMatter, string inputPath)
    {
        if (frontMatter.HasFrontMatter) return model;
        var inferredTitle = InferLawTitle(model, inputPath);
        return model with
        {
            Metadata = model.Metadata with
            {
                LawTitle = inferredTitle
            }
        };
    }

    private static string InferLawTitle(Zuke.Core.Model.LawDocumentModel model, string inputPath)
    {
        if (model.Chapters.Count > 0 && !string.IsNullOrWhiteSpace(model.Chapters[0].Title))
        {
            return model.Chapters[0].Title.Trim();
        }

        var fileName = Path.GetFileNameWithoutExtension(inputPath);
        if (!string.IsNullOrWhiteSpace(fileName))
        {
            return fileName.Trim();
        }

        return "無題";
    }

    private static Zuke.Core.Model.LawDocumentModel ApplyMetadataProfile(Zuke.Core.Model.LawDocumentModel model, string profile)
    {
        if (!string.Equals(profile, "internal-rule", StringComparison.OrdinalIgnoreCase)) return model;
        var m = model.Metadata;
        var normalized = m with
        {
            LawNum = string.IsNullOrWhiteSpace(m.LawNum) ? "社内規程" : m.LawNum,
            Era = string.IsNullOrWhiteSpace(m.Era) ? "Reiwa" : m.Era,
            Year = m.Year <= 0 ? 1 : m.Year,
            Num = m.Num <= 0 ? 1 : m.Num,
            LawType = string.IsNullOrWhiteSpace(m.LawType) ? "Misc" : m.LawType,
            Lang = string.IsNullOrWhiteSpace(m.Lang) ? "ja" : m.Lang
        };
        return model with { Metadata = normalized };
    }
}
