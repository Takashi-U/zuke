namespace Zuke.Core.Validation;

public static class ZukeXsdProvider
{
    public static string ResolveDefaultPath()
    {
        var candidates = new[]
        {
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "schemas", "XMLSchemaForJapaneseLaw_v3.xsd")),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "schemas", "XMLSchemaForJapaneseLaw_v3.xsd")),
            Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "schemas", "XMLSchemaForJapaneseLaw_v3.xsd"))
        };

        foreach (var c in candidates)
        {
            if (File.Exists(c)) return c;
        }

        return candidates[^1];
    }
}
