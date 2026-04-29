namespace Zuke.Core.Model;

public sealed record SupplementaryProvisionNode(
    string Title,
    SourceLocation? Location,
    IReadOnlyList<string> Lines
);
