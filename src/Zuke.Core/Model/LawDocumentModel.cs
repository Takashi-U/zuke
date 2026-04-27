namespace Zuke.Core.Model;
public sealed record LawDocumentModel(LawMetadata Metadata,IReadOnlyList<ChapterNode> Chapters,IReadOnlyList<ArticleNode> DirectArticles,IReadOnlyList<DiagnosticMessage> Diagnostics);
