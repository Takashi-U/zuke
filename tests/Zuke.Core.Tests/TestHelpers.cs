using System.Diagnostics;
using System.Text;
using Xunit;
using Zuke.Core.Compilation;
using Zuke.Core.Rendering;

namespace Zuke.Core.Tests;

public static class TestHelpers
{
    public static string RepoRoot { get; } = ResolveRepoRoot();
    public static string CliProjectPath { get; } = Path.Combine(RepoRoot, "src", "Zuke.Cli", "Zuke.Cli.csproj");

    public static CompileResult Compile(string? markdown = null, bool strict = false, bool arabicNumbers = false)
    {
        markdown ??= ReadFixture("minimal.md");
        return new LawMarkdownCompiler().Compile(markdown, "test.md", new CompileOptions(strict, arabicNumbers));
    }

    public static string ReadFixture(string fileName)
        => File.ReadAllText(Path.Combine(RepoRoot, "tests", "Zuke.Core.Tests", "Fixtures", "Lawtext", fileName), Encoding.UTF8);

    public static string RenderLawtext(string markdown, bool arabicNumbers = false)
    {
        var result = Compile(markdown, arabicNumbers: arabicNumbers);
        if (result.HasErrors || result.Document is null)
        {
            throw new InvalidOperationException(string.Join("\n", result.Diagnostics.Select(x => $"{x.Code}:{x.Message}")));
        }

        return new LawtextRenderer().Render(result.Document, LawtextRenderOptions.Default with { ArabicNumbers = arabicNumbers });
    }

    public static string QuoteArg(string value)
        => "\"" + value.Replace("\"", "\\\"") + "\"";

    public static (int ExitCode, string StdOut, string StdErr) RunProcess(string fileName, string args, string? workdir = null)
    {
        var psi = new ProcessStartInfo(fileName, args)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = workdir ?? RepoRoot
        };
        using var process = Process.Start(psi) ?? throw new InvalidOperationException($"failed to start {fileName}");
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();
        return (process.ExitCode, stdout, stderr);
    }

    public static (int ExitCode, string StdOut, string StdErr) RunZuke(string zukeArgs)
        => RunProcess("dotnet", $"run --project {QuoteArg(CliProjectPath)} -- {zukeArgs}", RepoRoot);

    public static void AssertExitCode((int ExitCode, string StdOut, string StdErr) result, int expected)
    {
        Assert.True(
            result.ExitCode == expected,
            $"""
            Expected exit code: {expected}
            Actual exit code: {result.ExitCode}

            StdOut:
            {result.StdOut}

            StdErr:
            {result.StdErr}
            """);
    }

    private static string ResolveRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "zuke.sln")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException("repo root not found");
    }
}
