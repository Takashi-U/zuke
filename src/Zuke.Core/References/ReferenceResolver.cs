using Zuke.Core.Model;
using Zuke.Core.Parsing;
namespace Zuke.Core.References; public sealed class ReferenceResolver { public (LawDocumentModel,IReadOnlyList<DiagnosticMessage>) Resolve(LawDocumentModel m,IReadOnlyDictionary<string,ReferenceDefinition> t)=> (m, Array.Empty<DiagnosticMessage>()); }
