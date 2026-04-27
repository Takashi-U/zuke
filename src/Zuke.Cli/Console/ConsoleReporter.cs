using Spectre.Console;
using Zuke.Core.Model;

namespace Zuke.Cli.Console;

public sealed class ConsoleReporter(IAnsiConsole console, ConsoleOptions options)
{
    private string Prefix => options.EmojiEnabled ? "🍣 zuke: " : "zuke: ";
    public void Info(string msg) => console.MarkupLine($"[green]{Markup.Escape(Prefix + msg)}[/]");
    public void ReportDiagnostics(IEnumerable<DiagnosticMessage> diags)
    {
        foreach (var d in diags)
        {
            var loc = d.Location is null ? "" : $" {d.Location.FilePath}:{d.Location.Line}:{d.Location.Column}";
            console.WriteLine($"{(d.Severity==DiagnosticSeverity.Error?"エラー":"警告")} {d.Code}: {d.Message}{loc}");
        }
    }
}
