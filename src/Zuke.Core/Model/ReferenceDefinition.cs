using Zuke.Core.Numbering;

namespace Zuke.Core.Model;
public sealed record ReferenceDefinition(string RawName,string NormalizedName,LawElementKind Kind,SourceLocation Location,int ArticleNumber,ArticleNumber ArticleNumberValue,int? ParagraphNumber,int? ItemNumber,int DocumentArticleIndex);
