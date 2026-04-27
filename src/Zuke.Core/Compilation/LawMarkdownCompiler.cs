using Zuke.Core.Model;
using Zuke.Core.Numbering;
using Zuke.Core.Parsing;
using Zuke.Core.References;
using Zuke.Core.Validation;

namespace Zuke.Core.Compilation;

public sealed class LawMarkdownCompiler
{
    public CompileResult Compile(string markdown, string? filePath, CompileOptions options)
    {
        var model = new MarkdownLawParser().Parse(markdown, filePath);
        var diags = new List<DiagnosticMessage>(model.Diagnostics);
        diags.AddRange(new LawStructureValidator().Validate(model));
        foreach (var a in model.DirectArticles)
            foreach (var p in a.Paragraphs)
                diags.AddRange(ManualReferenceDetector.Detect(p.SentenceText, p.Location ?? new(filePath,1,1), options.Strict));

        model = new NumberingService().Apply(model, options.ArabicNumbers);
        var (table, refDiags) = new ReferenceTableBuilder().Build(model);
        diags.AddRange(refDiags);
        var (resolved, resolveDiags) = new ReferenceResolver().Resolve(model, table);
        diags.AddRange(resolveDiags);
        var compiled = new CompiledLawDocument(resolved, table, diags);
        return new CompileResult(compiled, diags);
    }
}
