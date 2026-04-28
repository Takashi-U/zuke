using Zuke.Core.Importing;
using Xunit;

namespace Zuke.Core.Tests;

public class LawtextImportNegativeTests
{
    [Fact]
    public void UnresolvedRelativeReferenceProducesDiagnostic()
    {
        var lawtext = "題名\n（令和六年規則第一号）\n\n第一条　前項に基づく。\n";
        var result = new LawtextImportService().Import(lawtext, "bad.law.txt", new());
        Assert.Contains(result.Diagnostics, d => d.Code == "LMD091");
    }
}
