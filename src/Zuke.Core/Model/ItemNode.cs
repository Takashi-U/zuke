namespace Zuke.Core.Model;
public sealed record ItemNode(int Number,string? ReferenceName,string ItemTitle,string SentenceText,SourceLocation? Location,IReadOnlyList<ItemNode> Children);
