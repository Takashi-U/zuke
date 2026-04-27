namespace Zuke.Core.Model;
public sealed record ReferenceDefinition(string RawName,string NormalizedName,LawElementKind Kind,SourceLocation Location,int ArticleNumber,int? ParagraphNumber,int? ItemNumber);
