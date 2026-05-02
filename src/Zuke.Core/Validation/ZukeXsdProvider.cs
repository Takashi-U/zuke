namespace Zuke.Core.Validation;

public static class ZukeXsdProvider
{
    private const string XsdFileName = "XMLSchemaForJapaneseLaw_v3.xsd";

    public static string ResolveDefaultPath()
    {
        var candidates = new[]
        {
            // dotnet global tool / packaged output
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, XsdFileName)),

            // local build output when the XSD is copied under schemas/
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "schemas", XsdFileName)),

            // repository layout during development
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "schemas", XsdFileName)),

            // explicit working-directory fallback
            Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "schemas", XsdFileName))
        };

        foreach (var c in candidates)
        {
            if (File.Exists(c)) return c;
        }

        return candidates[^1];
    }
}
