using System.Text.Json;
using Spectre.Console;
using Spectre.Console.Cli;
using Zuke.Cli.Console;
using Zuke.Core.Importing;

namespace Zuke.Cli.Commands;

public sealed class ImportCommand : Command<ImportCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<input>")] public string Input { get; set; } = string.Empty;
        [CommandOption("-o|--output <PATH>")] public string Output { get; set; } = string.Empty;
        [CommandOption("--from <FORMAT>")] public string From { get; set; } = "lawtext";
        [CommandOption("--reference-labels <MODE>")] public string ReferenceLabels { get; set; } = "all";
        [CommandOption("--reference-mode <MODE>")] public string ReferenceMode { get; set; } = "conservative";
        [CommandOption("--id-style <STYLE>")] public string IdStyle { get; set; } = "ascii";
        [CommandOption("--metadata-mode <MODE>")] public string MetadataMode { get; set; } = "frontmatter";
        [CommandOption("--strict")] public bool Strict { get; set; }
        [CommandOption("--skip-roundtrip-check")] public bool SkipRoundtripCheck { get; set; }
        [CommandOption("--report <PATH>")] public string? Report { get; set; }
        [CommandOption("--map <PATH>")] public string? Map { get; set; }
        [CommandOption("--emoji <MODE>")] public string Emoji { get; set; } = "auto";
        [CommandOption("--no-color")] public bool NoColor { get; set; }
        [CommandOption("--plain")] public bool Plain { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var reporter = new ConsoleReporter(AnsiConsole.Console, ConsoleOptions.From(settings.Plain, settings.Emoji, settings.NoColor));
        var input = File.ReadAllText(settings.Input);
        var result = new LawtextImportService().Import(input, settings.Input, new LawtextImportOptions(settings.From, settings.ReferenceLabels, settings.ReferenceMode, settings.IdStyle, settings.MetadataMode, settings.Strict, settings.SkipRoundtripCheck));
        reporter.ReportDiagnostics(result.Diagnostics);
        if (result.HasErrors) return 1;
        File.WriteAllText(settings.Output, result.Markdown, new System.Text.UTF8Encoding(false));
        if (!string.IsNullOrWhiteSpace(settings.Report)) File.WriteAllText(settings.Report, new ImportReportRenderer().Render(settings.Input, settings.Output, new LawtextImportOptions(settings.From, settings.ReferenceLabels, settings.ReferenceMode, settings.IdStyle, settings.MetadataMode, settings.Strict, settings.SkipRoundtripCheck), result));
        if (!string.IsNullOrWhiteSpace(settings.Map) && result.Mapping is not null)
        {
            var m = result.Mapping with { Output = settings.Output, Source = settings.Input };
            File.WriteAllText(settings.Map, JsonSerializer.Serialize(m, new JsonSerializerOptions { WriteIndented = true }));
        }
        reporter.Info($"Markdownを出力しました: {settings.Output}");
        return 0;
    }
}
