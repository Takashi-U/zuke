using Zuke.Core.Model;

namespace Zuke.Core.Importing;

public sealed record LawtextImportResult(string Markdown, IReadOnlyList<DiagnosticMessage> Diagnostics, ImportMapping? Mapping = null)
{
    public bool HasErrors => Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
}
