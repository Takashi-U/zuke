namespace Zuke.Core.Importing;

public sealed record LawtextImportOptions(
    string From = "lawtext",
    string ReferenceLabels = "all",
    string ReferenceMode = "conservative",
    string IdStyle = "ascii",
    string MetadataMode = "frontmatter",
    bool Strict = false,
    bool SkipRoundtripCheck = false,
    bool ArabicNumbers = false)
{
    public bool ShouldEmitLabels => !ReferenceLabels.Equals("none", StringComparison.OrdinalIgnoreCase);
    public bool ShouldConvertReferences => !ReferenceMode.Equals("none", StringComparison.OrdinalIgnoreCase) && ShouldEmitLabels;
};
