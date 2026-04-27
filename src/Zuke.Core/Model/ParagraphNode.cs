namespace Zuke.Core.Model;
public sealed record ParagraphNode(int Number,string? ReferenceName,string? ParagraphNumText,string SentenceText,SourceLocation? Location,IReadOnlyList<ItemNode> Items);
