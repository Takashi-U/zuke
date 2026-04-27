namespace Zuke.Core.Model;
public sealed record CompiledLawDocument(LawDocumentModel Document,IReadOnlyDictionary<string,ReferenceDefinition> References,IReadOnlyList<DiagnosticMessage> Diagnostics){ public bool HasErrors => Diagnostics.Any(x=>x.Severity==DiagnosticSeverity.Error); }
