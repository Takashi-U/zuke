using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Zuke.Core.Compilation;
using Zuke.Core.Diff;
using Zuke.Core.Importing;
using Zuke.Core.Markdown;
using Zuke.Core.Model;
using Zuke.Core.Rendering;
using Zuke.Core.Validation;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole(options => { options.LogToStandardErrorThreshold = LogLevel.Trace; });
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

    [McpServerTool(Name = "zuke_convert"), Description("Compile markdown text to XML/Lawtext.")]
    public static ToolResponse Convert(string markdown, string to = "xml", bool strict = false, string numberStyle = "kanji", string metadataProfile = "default")
        => Compile(markdown, to, strict, numberStyle, metadataProfile);

    [McpServerTool(Name = "zuke_import"), Description("Import Lawtext string to extended markdown string.")]
    public static ToolResponse Import(string lawtext, string referenceLabels = "all", string referenceMode = "conservative", string idStyle = "ascii", string metadataMode = "frontmatter", bool strict = false, bool skipRoundtripCheck = false)
        => SafeExecute(() =>
        {
            var options = new LawtextImportOptions("lawtext", referenceLabels, referenceMode, idStyle, metadataMode, strict, skipRoundtripCheck);
            var result = new LawtextImportService().Import(lawtext, "mcp-input.law.txt", options);
            var diagnostics = result.Diagnostics.Select(ToDiagnostic).ToList();
            if (result.HasErrors)
            {
                return BuildResponse(false, "Import failed.", diagnostics, null, null);
            }

            var outputs = new Dictionary<string, object?>
            {
                ["markdown"] = result.Markdown,
                ["reportMarkdown"] = new ImportReportRenderer().Render("mcp-input.law.txt", "mcp-output.md", options, result)
            };
            if (result.Mapping is not null) outputs["mapping"] = result.Mapping;
            return BuildResponse(true, "Markdown generated successfully.", diagnostics, outputs, result.Markdown);
        });

    [McpServerTool(Name = "zuke_audit"), Description("Audit Lawtext string.")]
    public static ToolResponse Audit(string lawtext, bool strict = false, bool report = true)
        => SafeExecute(() =>
        {
            var result = new LawtextAuditService().Audit(lawtext, "mcp-input.law.txt", strict);
            var diagnostics = result.Diagnostics.Select(ToDiagnostic).ToList();
            var outputs = new Dictionary<string, object?>();
            if (report) outputs["reportMarkdown"] = new LawtextAuditReportRenderer().RenderMarkdown(result);
            return BuildResponse(!result.HasErrors, "Audit completed.", diagnostics, outputs, null);
        });

    [McpServerTool(Name = "zuke_diff"), Description("Diff two markdown strings using normalized Lawtext.")]
    public static ToolResponse Diff(string oldMarkdown, string newMarkdown, int context = 3, string view = "unified")
        => SafeExecute(() =>
        {
            if (!string.Equals(view, "unified", StringComparison.OrdinalIgnoreCase))
            {
                return BuildResponse(false, "Diff failed.", [new ToolDiagnostic("error", "MCP004", "Only 'unified' view is supported in MCP.", view)], null, null);
            }

            var oldCompile = new LawMarkdownCompiler().Compile(oldMarkdown, "old.md", new CompileOptions());
            var newCompile = new LawMarkdownCompiler().Compile(newMarkdown, "new.md", new CompileOptions());
            var diagnostics = oldCompile.Diagnostics.Concat(newCompile.Diagnostics).Select(ToDiagnostic).ToList();
            if (oldCompile.HasErrors || newCompile.HasErrors || oldCompile.Document is null || newCompile.Document is null)
            {
                return BuildResponse(false, "Diff failed.", diagnostics, null, null);
            }

            var renderer = new LawtextRenderer();
            var normalizer = new LawtextNormalizer();
            var oldText = normalizer.Normalize(renderer.Render(oldCompile.Document, LawtextRenderOptions.Default), LawtextNormalizeOptions.Default);
            var newText = normalizer.Normalize(renderer.Render(newCompile.Document, LawtextRenderOptions.Default), LawtextNormalizeOptions.Default);
            var result = new LawtextDiffService().Diff(oldText, newText, new DiffOptions(context));
            var unified = new UnifiedDiffRenderer().Render("old.md", "new.md", result);
            var outputs = new Dictionary<string, object?> { ["diff"] = unified, ["hasChanges"] = result.HasChanges };
            return BuildResponse(true, "Diff completed.", diagnostics, outputs, unified);
        });

    [McpServerTool(Name = "zuke_validate_xml"), Description("Validate XML string with bundled XSD.")]
    public static ToolResponse ValidateXml(string xml)
        => SafeExecute(() =>
        {
            var diagnostics = new List<ToolDiagnostic>();
            var xsdPath = ZukeXsdProvider.ResolveDefaultPath();
            if (!File.Exists(xsdPath))
            {
                diagnostics.Add(new ToolDiagnostic("error", "MCP005", "XSD file cannot be resolved.", xsdPath));
                return BuildResponse(false, "XML validation completed.", diagnostics, new Dictionary<string, object?> { ["xsdPath"] = xsdPath, ["valid"] = false }, null);
            }

            var xmlDocument = XDocument.Parse(xml);
            diagnostics.AddRange(new LawXmlValidator().Validate(xmlDocument, xsdPath).Select(ToDiagnostic));
            var valid = diagnostics.All(d => d.Severity != "error");
            return BuildResponse(valid, "XML validation completed.", diagnostics, new Dictionary<string, object?> { ["xsdPath"] = xsdPath, ["valid"] = valid }, null);
        });

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
            : [new ToolDiagnostic("error", "MCP001", "XSD file was not found at resolved path.", xsdPath)];

        return BuildResponse(xsdResolved, xsdResolved ? "Environment is healthy." : "Environment has configuration issues.", diagnostics, outputs, null);
    }

    private static ToolResponse Compile(string markdown, string output, bool strict, string numberStyle, string metadataProfile = "default")
        => SafeExecute(() =>
        {
            var arabic = string.Equals(numberStyle, "arabic", StringComparison.OrdinalIgnoreCase);
            var requireFrontMatter = string.Equals(output, "xml", StringComparison.OrdinalIgnoreCase) || string.Equals(output, "both", StringComparison.OrdinalIgnoreCase);
            var result = new LawMarkdownCompiler().Compile(markdown, filePath: null, new CompileOptions(strict, arabic, requireFrontMatter));
            var diagnostics = result.Diagnostics.Select(ToDiagnostic).ToList();
            if (result.HasErrors || result.Document is null)
            {
                return BuildResponse(false, "Compilation failed.", diagnostics, null, null);
            }

            if (string.Equals(output, "lawtext", StringComparison.OrdinalIgnoreCase))
            {
                var lawtext = RenderLawtext(markdown, result.Document, arabic, diagnostics);
                var lawtextHasErrors = diagnostics.Any(x => x.Severity == "error");
                return BuildResponse(!lawtextHasErrors, lawtextHasErrors ? "Lawtext generation failed." : "Lawtext generated successfully.", diagnostics, new Dictionary<string, object?> { ["lawtext"] = lawtext }, lawtext);
            }

            if (!string.Equals(output, "xml", StringComparison.OrdinalIgnoreCase) && !string.Equals(output, "both", StringComparison.OrdinalIgnoreCase))
            {
                diagnostics.Add(new ToolDiagnostic("error", "MCP004", "Unsupported 'to' option.", output));
                return BuildResponse(false, "Compilation failed.", diagnostics, null, null);
            }

            var outputs = new Dictionary<string, object?>();
            var xmlModel = ApplyMetadataProfile(result.Document.Document, metadataProfile);
            var xml = new LawXmlRenderer().Render(xmlModel, LawXmlRenderOptions.Default with { ArabicNumbers = arabic }).ToString();
            outputs["xml"] = xml;

            var xsdPath = ZukeXsdProvider.ResolveDefaultPath();
            outputs["xsdPath"] = xsdPath;
            if (File.Exists(xsdPath))
            {
                var xmlDocument = XDocument.Parse(xml);
                diagnostics.AddRange(new LawXmlValidator().Validate(xmlDocument, xsdPath).Select(ToDiagnostic));
            }
            else
            {
                diagnostics.Add(new ToolDiagnostic("error", "MCP001", "XSD file was not found; XML validation was not run.", xsdPath));
            }

            if (string.Equals(output, "both", StringComparison.OrdinalIgnoreCase))
            {
                outputs["lawtext"] = RenderLawtext(markdown, result.Document, arabic, diagnostics);
            }

            var hasErrors = diagnostics.Any(x => x.Severity == "error");
            var summary = hasErrors ? "XML generation failed validation." : "XML generated and validated successfully.";
            return BuildResponse(!hasErrors, summary, diagnostics, outputs, xml);
        });

    private static string RenderLawtext(string markdown, CompiledLawDocument document, bool arabic, List<ToolDiagnostic> diagnostics)
    {
        var frontMatter = FrontMatterParser.ParseDetailed(markdown);
        var model = ApplyLawtextMetadataFallback(document.Document, frontMatter);
        var lawtext = new LawtextRenderer().Render(document with { Document = model }, LawtextRenderOptions.Default with { ArabicNumbers = arabic });
        diagnostics.AddRange(LawtextRenderer.ValidateRenderedText(lawtext).Select(ToDiagnostic));
        return lawtext;
    }

    private static ToolResponse SafeExecute(Func<ToolResponse> action)
    {
        try { return action(); }
        catch (Exception ex)
        {
            return BuildResponse(false, "Unexpected error occurred.", [new ToolDiagnostic("error", "MCP999", ex.Message, null)], null, null);
        }
    }

    private static ToolResponse BuildResponse(bool success, string summary, IReadOnlyList<ToolDiagnostic> diagnostics, IReadOnlyDictionary<string, object?>? outputs, string? content)
    {
        var hasErrors = diagnostics.Any(x => x.Severity == "error");
        return new ToolResponse(success && !hasErrors, summary, diagnostics, outputs, content, hasErrors);
    }

    private static LawDocumentModel ApplyLawtextMetadataFallback(LawDocumentModel model, FrontMatterParseResult frontMatter)
    {
        if (frontMatter.HasFrontMatter) return model;
        return model with { Metadata = model.Metadata with { LawTitle = string.IsNullOrWhiteSpace(model.Metadata.LawTitle) ? "無題" : model.Metadata.LawTitle } };
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

public sealed record ToolResponse(bool Success, string Summary, IReadOnlyList<ToolDiagnostic> Diagnostics, IReadOnlyDictionary<string, object?>? Outputs, string? Content, bool HasErrors);
