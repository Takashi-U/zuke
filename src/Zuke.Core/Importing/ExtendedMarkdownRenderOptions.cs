namespace Zuke.Core.Importing;

public sealed record ExtendedMarkdownRenderOptions(
    string ReferenceLabels = "used",
    string MetadataMode = "frontmatter");
