namespace Zuke.Core.Model;
public sealed record ArticleNode(int Number,string? ReferenceName,string Caption,string ArticleTitle,SourceLocation? Location,IReadOnlyList<ParagraphNode> Paragraphs);
