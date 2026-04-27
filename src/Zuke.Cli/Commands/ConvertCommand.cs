using Spectre.Console;
using Spectre.Console.Cli;
using Zuke.Cli.Console;
using Zuke.Core.Compilation;
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
        [CommandOption("--number-style <STYLE>")] public string NumberStyle { get; set; } = "kanji";
        [CommandOption("--emoji <MODE>")] public string Emoji { get; set; } = "auto";
        [CommandOption("--no-color")] public bool NoColor { get; set; }
        [CommandOption("--plain")] public bool Plain { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var consoleOptions = ConsoleOptions.From(settings.Plain, settings.Emoji, settings.NoColor);
        var reporter = new ConsoleReporter(AnsiConsole.Console, consoleOptions);
        var text = File.ReadAllText(settings.Input);
        var result = new LawMarkdownCompiler().Compile(text, settings.Input, new CompileOptions(settings.Strict, settings.NumberStyle == "arabic"));
        reporter.ReportDiagnostics(result.Diagnostics);
        if (result.HasErrors || result.Document is null) return 1;

        if (settings.To == "lawtext")
        {
            var renderer = new LawtextRenderer();
            var lawtext = renderer.Render(result.Document, LawtextRenderOptions.Default);
            var renderDiags = LawtextRenderer.ValidateRenderedText(lawtext);
            reporter.ReportDiagnostics(renderDiags);
            if (renderDiags.Any(x => x.Severity == Zuke.Core.Model.DiagnosticSeverity.Error)) return 1;

            File.WriteAllText(settings.Output, lawtext, new System.Text.UTF8Encoding(false));
            reporter.Info($"Lawtextを出力しました: {settings.Output}");
            return 0;
        }

        var doc = new LawXmlRenderer().Render(result.Document.Document);
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
}
