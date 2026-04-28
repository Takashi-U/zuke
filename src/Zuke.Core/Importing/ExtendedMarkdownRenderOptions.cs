namespace Zuke.Core.Importing;

public sealed record ExtendedMarkdownRenderOptions(
    string ReferenceLabels = "all",
    string MetadataMode = "frontmatter",
    ISet<string>? UsedRefs = null);
