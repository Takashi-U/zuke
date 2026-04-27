using System.Xml.Linq;
using System.Xml.Schema;
using Zuke.Core.Model;

namespace Zuke.Core.Validation;

public sealed class LawXmlValidator
{
    public IReadOnlyList<DiagnosticMessage> Validate(XDocument doc, string xsdPath)
    {
        var diags = new List<DiagnosticMessage>();
        var set = new XmlSchemaSet();
        set.Add(string.Empty, xsdPath);
        doc.Validate(set, (_, e) => diags.Add(new(DiagnosticSeverity.Error, "LMD044", $"生成XMLが法令標準XMLスキーマに適合しません: {e.Message}", null, Array.Empty<SourceLocation>())));
        return diags;
    }
}
