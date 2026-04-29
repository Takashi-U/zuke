namespace Zuke.Core.Model;

public sealed record LawDocumentModel(
    LawMetadata Metadata,
    IReadOnlyList<ChapterNode> Chapters,
    IReadOnlyList<ArticleNode> DirectArticles,
    IReadOnlyList<SupplementaryProvisionNode> SupplementaryProvisions,
    IReadOnlyList<DiagnosticMessage> Diagnostics)
{
    public LawDocumentModel(
        LawMetadata metadata,
        IReadOnlyList<ChapterNode> chapters,
        IReadOnlyList<ArticleNode> directArticles,
        IReadOnlyList<DiagnosticMessage> diagnostics)
        : this(metadata, chapters, directArticles, [], diagnostics)
    {
    }
}
