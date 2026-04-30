namespace Zuke.Core.Rendering;

public sealed record LawtextRenderOptions
{
    public static LawtextRenderOptions Default { get; } = new();

    public LawtextLayoutOptions Layout { get; init; } = LawtextLayoutOptions.Default;
    public bool EnsureFinalNewline { get; init; } = true;
    public bool NormalizeLineEndings { get; init; } = true;
    public bool TrimTrailingWhitespace { get; init; } = true;
    public bool IncludeLawNum { get; init; } = true;
    public bool IncludeBlankLineBetweenBlocks { get; init; } = true;
    public bool ArabicNumbers { get; init; }
}

public sealed record LawtextLayoutOptions
{
    public static LawtextLayoutOptions Default { get; } = new();

    public string ChapterIndent { get; init; } = "      ";
    public string SectionIndent { get; init; } = "        ";
    public string ArticleCaptionIndent { get; init; } = "  ";
    public string ItemIndent { get; init; } = "  ";
    public string Subitem1Indent { get; init; } = "    ";
    public string Separator { get; init; } = "　";
}

public sealed record LawtextNormalizeOptions
{
    public static LawtextNormalizeOptions Default { get; } = new();

    public bool NormalizeLineEndings { get; init; } = true;
    public bool TrimTrailingWhitespace { get; init; } = true;
    public bool EnsureFinalNewline { get; init; } = true;
    public bool CollapseExcessBlankLines { get; init; }
    public int MaxConsecutiveBlankLines { get; init; } = 2;
}
