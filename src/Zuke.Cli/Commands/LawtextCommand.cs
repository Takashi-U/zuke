using Spectre.Console.Cli;

namespace Zuke.Cli.Commands;

public sealed class LawtextCommand : Command<LawtextCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<input>")] public string Input { get; set; } = string.Empty;
        [CommandOption("-o|--output <PATH>")] public string Output { get; set; } = string.Empty;
        [CommandOption("--strict")] public bool Strict { get; set; }
        [CommandOption("--number-style <STYLE>")] public string NumberStyle { get; set; } = "kanji";
        [CommandOption("--emoji <MODE>")] public string Emoji { get; set; } = "auto";
        [CommandOption("--no-color")] public bool NoColor { get; set; }
        [CommandOption("--plain")] public bool Plain { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var convert = new ConvertCommand();
        var mapped = new ConvertCommand.Settings
        {
            Input = settings.Input,
            Output = settings.Output,
            Strict = settings.Strict,
            NumberStyle = settings.NumberStyle,
            Emoji = settings.Emoji,
            NoColor = settings.NoColor,
            Plain = settings.Plain,
            To = "lawtext"
        };
        return convert.Execute(context, mapped);
    }
}
