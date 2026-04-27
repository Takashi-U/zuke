namespace Zuke.Core.Model;
public sealed record ChapterNode(int Number,string Title,SourceLocation? Location,IReadOnlyList<SectionNode> Sections,IReadOnlyList<ArticleNode> Articles);
