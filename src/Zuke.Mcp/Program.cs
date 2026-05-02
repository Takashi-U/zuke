using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;
using Zuke.Core.Compilation;
using Zuke.Core.Markdown;
using Zuke.Core.Model;
using Zuke.Core.Rendering;

var builder = Host.CreateApplicationBuilder(args);
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<ZukeMcpTools>();

await builder.Build().RunAsync();

[McpServerToolType]
public sealed class ZukeMcpTools
{
    [McpServerTool(Name = "zuke.compile_lawtext"), Description("Compile markdown text to Lawtext.")]
    public static CompileToolResponse CompileLawtext(string markdown, bool strict = false, string numberStyle = "kanji")
        => Compile(markdown, "lawtext", strict, numberStyle);

    [McpServerTool(Name = "zuke.compile_xml"), Description("Compile markdown text to Japanese law XML.")]
    public static CompileToolResponse CompileXml(string markdown, bool strict = false, string numberStyle = "kanji", string metadataProfile = "default")
        => Compile(markdown, "xml", strict, numberStyle, metadataProfile);

    private static CompileToolResponse Compile(string markdown, string output, bool strict, string numberStyle, string metadataProfile = "default")
    {
        var arabic = string.Equals(numberStyle, "arabic", StringComparison.OrdinalIgnoreCase);
        var requireFrontMatter = output == "xml";
        var result = new LawMarkdownCompiler().Compile(markdown, filePath: null, new CompileOptions(strict, arabic, requireFrontMatter));
        var diagnostics = result.Diagnostics.Select(ToDiagnostic).ToArray();
        if (result.HasErrors || result.Document is null)
        {
            return new CompileToolResponse(null, null, diagnostics, true);
        }

        if (string.Equals(output, "lawtext", StringComparison.OrdinalIgnoreCase))
        {
            var frontMatter = FrontMatterParser.ParseDetailed(markdown);
            var model = ApplyLawtextMetadataFallback(result.Document.Document, frontMatter);
            var lawtext = new LawtextRenderer().Render(result.Document with { Document = model }, LawtextRenderOptions.Default with { ArabicNumbers = arabic });
            var renderDiagnostics = LawtextRenderer.ValidateRenderedText(lawtext).Select(ToDiagnostic).ToArray();
            return new CompileToolResponse(lawtext, null, diagnostics.Concat(renderDiagnostics).ToArray(), renderDiagnostics.Any(x => x.Severity == "error"));
        }

        var xmlModel = ApplyMetadataProfile(result.Document.Document, metadataProfile);
        var xml = new LawXmlRenderer().Render(xmlModel, LawXmlRenderOptions.Default with { ArabicNumbers = arabic }).ToString();
        return new CompileToolResponse(null, xml, diagnostics, false);
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

public sealed record CompileToolResponse(string? Lawtext, string? Xml, IReadOnlyList<ToolDiagnostic> Diagnostics, bool HasErrors);
