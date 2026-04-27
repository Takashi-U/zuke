using Zuke.Core.Model;
namespace Zuke.Core.References; public sealed class ReferenceTableBuilder { public (IReadOnlyDictionary<string,ReferenceDefinition>,IReadOnlyList<DiagnosticMessage>) Build(LawDocumentModel m)=> (new Dictionary<string,ReferenceDefinition>(), Array.Empty<DiagnosticMessage>()); }
