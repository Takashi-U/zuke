using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using Zuke.Cli.Console;
using Zuke.Core.Compilation;
using Zuke.Core.Diff;
using Zuke.Core.Rendering;

namespace Zuke.Cli.Commands;

public sealed class DiffCommand : Command<DiffCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<old>")] public string OldPath { get; set; } = string.Empty;
        [CommandArgument(1, "<new>")] public string NewPath { get; set; } = string.Empty;
        [CommandOption("--view <VIEW>")] [DefaultValue("unified")] public string View { get; set; } = "unified";
        [CommandOption("-o|--output <PATH>")] public string? Output { get; set; }
        [CommandOption("--open")] public bool Open { get; set; }
        [CommandOption("--context <N>")] [DefaultValue(3)] public int Context { get; set; } = 3;
        [CommandOption("--no-color")] public bool NoColor { get; set; }
        [CommandOption("--plain")] public bool Plain { get; set; }
        [CommandOption("--emoji <MODE>")] public string Emoji { get; set; } = "auto";
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var consoleOptions = ConsoleOptions.From(settings.Plain, settings.Emoji, settings.NoColor);
        var reporter = new ConsoleReporter(AnsiConsole.Console, consoleOptions);

        var oldCompile = new LawMarkdownCompiler().Compile(File.ReadAllText(settings.OldPath), settings.OldPath, new CompileOptions());
        var newCompile = new LawMarkdownCompiler().Compile(File.ReadAllText(settings.NewPath), settings.NewPath, new CompileOptions());
        reporter.ReportDiagnostics(oldCompile.Diagnostics.Concat(newCompile.Diagnostics));
        if (oldCompile.HasErrors || newCompile.HasErrors || oldCompile.Document is null || newCompile.Document is null) return 2;

        var renderer = new LawtextRenderer();
        var normalizer = new LawtextNormalizer();
        var oldText = normalizer.Normalize(renderer.Render(oldCompile.Document, LawtextRenderOptions.Default), LawtextNormalizeOptions.Default);
        var newText = normalizer.Normalize(renderer.Render(newCompile.Document, LawtextRenderOptions.Default), LawtextNormalizeOptions.Default);
        var result = new LawtextDiffService().Diff(oldText, newText, new DiffOptions(settings.Context));

        if (settings.View == "terminal")
            new TerminalDiffRenderer().Render(AnsiConsole.Console, result, consoleOptions.ColorEnabled);
        else if (settings.View == "html")
        {
            var path = settings.Output ?? "diff.html";
            File.WriteAllText(path, new HtmlDiffRenderer().Render(settings.OldPath, settings.NewPath, result));
            if (settings.Open)
            {
                try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true }); } catch { }
            }
        }
        else
        {
            var text = new UnifiedDiffRenderer().Render(settings.OldPath, settings.NewPath, result);
            if (string.IsNullOrWhiteSpace(settings.Output)) System.Console.Write(text);
            else File.WriteAllText(settings.Output, text);
        }

        return result.HasChanges ? 1 : 0;
    }
}
