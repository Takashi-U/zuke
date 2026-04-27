namespace Zuke.Cli.Console;
public sealed record ConsoleOptions(bool EmojiEnabled, bool ColorEnabled)
{
    public static ConsoleOptions From(bool plain, string emoji, bool noColor)
    {
        if (plain) return new(false, false);
        var em = emoji switch { "on" => true, "off" => false, _ => !System.Console.IsOutputRedirected && string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI")) };
        var color = !noColor && string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NO_COLOR"));
        return new(em, color);
    }
}
