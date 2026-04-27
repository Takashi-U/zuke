namespace Zuke.Core.Model;
public sealed record ReferenceOccurrence(string RawName,string NormalizedName,ReferenceOption Option,SourceLocation Location,int ArticleNumber,int ParagraphNumber,int? ItemNumber);
