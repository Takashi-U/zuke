using Zuke.Core.Model;

namespace Zuke.Core.Importing;

public sealed record LawtextAuditResult(IReadOnlyList<DiagnosticMessage> Diagnostics)
{
    public bool HasErrors => Diagnostics.Any(x => x.Severity == DiagnosticSeverity.Error);
}
