namespace Zuke.Core.Model;
public sealed record DiagnosticMessage(DiagnosticSeverity Severity, string Code, string Message, SourceLocation? Location, IReadOnlyList<SourceLocation> RelatedLocations);
