namespace Zuke.Core.Rendering;

public sealed record LawXmlRenderOptions
{
    public static LawXmlRenderOptions Default { get; } = new();

    public bool ArabicNumbers { get; init; }
}
