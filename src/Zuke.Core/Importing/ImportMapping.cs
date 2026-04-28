namespace Zuke.Core.Importing;

public sealed record ImportMapping(string Source, string Output, IReadOnlyList<ImportMappingItem> Items);

public sealed record ImportMappingItem(
    string Kind,
    int LawtextLine,
    int MarkdownLine,
    string Number,
    string? ReferenceName,
    string? Caption);
