using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Zuke.Core.Compilation;
using Zuke.Core.Markdown;
using Zuke.Core.Model;
using Zuke.Core.Rendering;
using Zuke.Core.Validation;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});
builder.Logging.SetMinimumLevel(LogLevel.Warning);
builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Warning);
builder.Logging.AddFilter("ModelContextProtocol", LogLevel.Warning);

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<ZukeMcpTools>();

await builder.Build().RunAsync();

[McpServerToolType]
public sealed class ZukeMcpTools
{
    [McpServerTool(Name = "zuke.compile_lawtext"), Description("Compile markdown text to Lawtext.")]
    public static ToolResponse CompileLawtext(string markdown, bool strict = false, string numberStyle = "kanji")
        => Compile(markdown, "lawtext", strict, numberStyle);

    [McpServerTool(Name = "zuke_lawtext"), Description("Compile markdown text to Lawtext.")]
    public static ToolResponse Lawtext(string markdown, bool strict = false, string numberStyle = "kanji")
        => Compile(markdown, "lawtext", strict, numberStyle);

    [McpServerTool(Name = "zuke.compile_xml"), Description("Compile markdown text to Japanese law XML.")]
    public static ToolResponse CompileXml(string markdown, bool strict = false, string numberStyle = "kanji", string metadataProfile = "default")
        => Compile(markdown, "xml", strict, numberStyle, metadataProfile);

    [McpServerTool(Name = "zuke_convert"), Description("Compile markdown text to Japanese law XML.")]
    public static ToolResponse Convert(string markdown, bool strict = false, string numberStyle = "kanji", string metadataProfile = "default")
        => Compile(markdown, "xml", strict, numberStyle, metadataProfile);

    [McpServerTool(Name = "zuke_doctor"), Description("Show MCP runtime diagnostics and environment information.")]
    public static ToolResponse Doctor()
    {
        var xsdPath = ZukeXsdProvider.ResolveDefaultPath();
        var xsdResolved = File.Exists(xsdPath);
        var outputs = new Dictionary<string, object?>
        {
            ["version"] = typeof(ZukeMcpTools).Assembly.GetName().Version?.ToString(),
            ["workingDirectory"] = Directory.GetCurrentDirectory(),
            ["xsdResolved"] = xsdResolved,
            ["xsdPath"] = xsdPath,
            ["runtime"] = RuntimeInformation.FrameworkDescription,
            ["osDescription"] = RuntimeInformation.OSDescription,
            ["processArchitecture"] = RuntimeInformation.ProcessArchitecture.ToString()
        };
        var diagnostics = xsdResolved
            ? Array.Empty<ToolDiagnostic>()
            : new[] { new ToolDiagnostic("error", "MCP001", "XSD file was not found at resolved path.", xsdPath) };

        return BuildResponse(
            success: xsdResolved,
            summary: xsdResolved ? "Environment is healthy." : "Environment has configuration issues.",
            diagnostics: diagnostics,
            outputs: outputs,
            content: null);
    }

    private static ToolResponse Compile(string markdown, string output, bool strict, string numberStyle, string metadataProfile = "default")
    {
        var arabic = string.Equals(numberStyle, "arabic", StringComparison.OrdinalIgnoreCase);
        var requireFrontMatter = output == "xml";
        var result = new LawMarkdownCompiler().Compile(markdown, filePath: null, new CompileOptions(strict, arabic, requireFrontMatter));
        var diagnostics = result.Diagnostics.Select(ToDiagnostic).ToList();
        if (result.HasErrors || result.Document is null)
        {
            return BuildResponse(false, "Compilation failed.", diagnostics, null, null);
        }

        if (string.Equals(output, "lawtext", StringComparison.OrdinalIgnoreCase))
        {
            var frontMatter = FrontMatterParser.ParseDetailed(markdown);
            var model = ApplyLawtextMetadataFallback(result.Document.Document, frontMatter);
            var lawtext = new LawtextRenderer().Render(result.Document with { Document = model }, LawtextRenderOptions.Default with { ArabicNumbers = arabic });
            var renderDiagnostics = LawtextRenderer.ValidateRenderedText(lawtext).Select(ToDiagnostic);
            diagnostics.AddRange(renderDiagnostics);
            var hasErrors = diagnostics.Any(x => x.Severity == "error");
            return BuildResponse(!hasErrors, hasErrors ? "Lawtext generation failed." : "Lawtext generated successfully.", diagnostics, new Dictionary<string, object?> { ["lawtext"] = lawtext }, lawtext);
        }

        var xmlModel = ApplyMetadataProfile(result.Document.Document, metadataProfile);
        var xml = new LawXmlRenderer().Render(xmlModel, LawXmlRenderOptions.Default with { ArabicNumbers = arabic }).ToString();

        var xsdPath = ZukeXsdProvider.ResolveDefaultPath();
        if (File.Exists(xsdPath))
        {
            var xmlDocument = XDocument.Parse(xml);
            var xsdDiagnostics = new LawXmlValidator().Validate(xmlDocument, xsdPath).Select(ToDiagnostic);
            diagnostics.AddRange(xsdDiagnostics);
        }
        else
        {
            diagnostics.Add(new ToolDiagnostic("error", "MCP001", "XSD file was not found; XML validation was not run.", xsdPath));
        }

        var xmlHasErrors = diagnostics.Any(x => x.Severity == "error");
        return BuildResponse(!xmlHasErrors, xmlHasErrors ? "XML generation failed validation." : "XML generated and validated successfully.", diagnostics, new Dictionary<string, object?> { ["xml"] = xml, ["xsdPath"] = xsdPath }, xml);
    }

    private static ToolResponse BuildResponse(bool success, string summary, IReadOnlyList<ToolDiagnostic> diagnostics, IReadOnlyDictionary<string, object?>? outputs, string? content)
    {
        var hasErrors = diagnostics.Any(x => x.Severity == "error");
        return new ToolResponse(success && !hasErrors, summary, diagnostics, outputs, content, hasErrors);
    }

    private static LawDocumentModel ApplyLawtextMetadataFallback(LawDocumentModel model, FrontMatterParseResult frontMatter)
    {
        if (frontMatter.HasFrontMatter) return model;
        return model with
        {
            Metadata = model.Metadata with
            {
                LawTitle = string.IsNullOrWhiteSpace(model.Metadata.LawTitle) ? "無題" : model.Metadata.LawTitle
            }
        };
    }

    private static LawDocumentModel ApplyMetadataProfile(LawDocumentModel model, string profile)
    {
        if (!string.Equals(profile, "internal-rule", StringComparison.OrdinalIgnoreCase)) return model;
        var m = model.Metadata;
        return model with
        {
            Metadata = m with
            {
                LawNum = string.IsNullOrWhiteSpace(m.LawNum) ? "社内規程" : m.LawNum,
                Era = string.IsNullOrWhiteSpace(m.Era) ? "Reiwa" : m.Era,
                Year = m.Year <= 0 ? 1 : m.Year,
                Num = m.Num <= 0 ? 1 : m.Num,
                LawType = string.IsNullOrWhiteSpace(m.LawType) ? "Misc" : m.LawType,
                Lang = string.IsNullOrWhiteSpace(m.Lang) ? "ja" : m.Lang
            }
        };
    }

    private static ToolDiagnostic ToDiagnostic(DiagnosticMessage d)
        => new(d.Severity.ToString().ToLowerInvariant(), d.Code, d.Message, d.Location?.ToString());
}

public sealed record ToolDiagnostic(string Severity, string Code, string Message, string? Location);

public sealed record ToolResponse(
    bool Success,
    string Summary,
    IReadOnlyList<ToolDiagnostic> Diagnostics,
    IReadOnlyDictionary<string, object?>? Outputs,
    string? Content,
    bool HasErrors);
