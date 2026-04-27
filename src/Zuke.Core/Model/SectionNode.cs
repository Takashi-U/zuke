namespace Zuke.Core.Model;
public sealed record SectionNode(int Number,string Title,SourceLocation? Location,IReadOnlyList<ArticleNode> Articles);
