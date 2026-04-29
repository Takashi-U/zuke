using Zuke.Core.Numbering;

namespace Zuke.Core.Model;
public sealed record ArticleNode(int Number,string? ReferenceName,string Caption,string ArticleTitle,SourceLocation? Location,IReadOnlyList<ParagraphNode> Paragraphs)
{
    public ArticleNumber ArticleNumber { get; init; } = ArticleNumber.FromBase(Number);
}
