using Spectre.Console.Cli;
using Zuke.Cli.Commands;

var app = new CommandApp();
app.Configure(c =>
{
    c.AddCommand<ConvertCommand>("convert");
    c.AddCommand<LawtextCommand>("lawtext");
    c.AddCommand<DiffCommand>("diff");
    c.AddCommand<ImportCommand>("import");
});
return app.Run(args);
