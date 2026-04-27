using Zuke.Core.Model;
namespace Zuke.Core.Compilation; public sealed record CompileResult(CompiledLawDocument? Document,IReadOnlyList<DiagnosticMessage> Diagnostics){ public bool HasErrors => Diagnostics.Any(x=>x.Severity==DiagnosticSeverity.Error); }
