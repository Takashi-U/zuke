using System.Text.Json;
using Spectre.Console;
using Spectre.Console.Cli;
using Zuke.Cli.Console;
using Zuke.Core.Importing;

namespace Zuke.Cli.Commands;

public sealed class AuditCommand : Command<AuditCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<input>")] public string Input { get; set; } = string.Empty;
        [CommandOption("--report <PATH>")] public string? Report { get; set; }
        [CommandOption("--format <FORMAT>")] public string Format { get; set; } = "text";
        [CommandOption("--strict")] public bool Strict { get; set; }
        [CommandOption("--emoji <MODE>")] public string Emoji { get; set; } = "auto";
        [CommandOption("--no-color")] public bool NoColor { get; set; }
        [CommandOption("--plain")] public bool Plain { get; set; }
    }
    public override int Execute(CommandContext context, Settings settings)
    {
        var reporter = new ConsoleReporter(AnsiConsole.Console, ConsoleOptions.From(settings.Plain, settings.Emoji, settings.NoColor));
        var result = new LawtextAuditService().Audit(File.ReadAllText(settings.Input), settings.Input, settings.Strict);
        if (settings.Format == "json") reporter.Info(JsonSerializer.Serialize(result.Diagnostics)); else reporter.ReportDiagnostics(result.Diagnostics);
        if (!string.IsNullOrWhiteSpace(settings.Report)) File.WriteAllText(settings.Report, new LawtextAuditReportRenderer().RenderMarkdown(result));
        return result.HasErrors ? 1 : 0;
    }
}
