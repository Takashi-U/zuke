using System.Text.Json;
using Xunit;
using Zuke.Core.Importing;

namespace Zuke.Core.Tests;

public class LawtextAuditTests
{
    [Theory]
    [InlineData("第一条　前条による。", "LMD091")]
    [InlineData("第一条\n  一　前号による。", "LMD091")]
    [InlineData("第一条\n前項による。", "LMD091")]
    [InlineData("第一条　第三条第一項による。", "LMD092")]
    [InlineData("第一条　第一号から第三号まで。", "LMD095")]
    [InlineData("第一条　第一条及び第二条による。", "LMD094")]
    [InlineData("第9条のA　追加条文。", "LMD101")]
    public void ImportDiagnosticsForKnownPatterns(string body, string expectedCode)
    {
        var lawtext = $"題名\n（令和六年規則第一号）\n\n{body}\n";
        var import = new LawtextImportService().Import(lawtext, "x.law.txt", new());
        var audit = new LawtextAuditService().Audit(lawtext, "x.law.txt", false);
        Assert.True(import.Diagnostics.Any(d => d.Code == expectedCode) || audit.Diagnostics.Any(d => d.Code == expectedCode));
    }

    [Theory]
    [InlineData("（目的）\n\n", "LMD099")]
    [InlineData("題名\n（令和六年規則第一号）\n\n- 1 -\n", "LMD099")]
    [InlineData("題名\n（令和六年規則第一号）\n\nページ 1\n", "LMD099")]
    [InlineData("題名\n（令和六年規則第一号）\n\nこれは孤立本文です。\n", "LMD096")]
    public void AuditDiagnosticsForMalformedBody(string lawtext, string code)
    {
        var result = new LawtextAuditService().Audit(lawtext, "x.law.txt", false);
        Assert.Contains(result.Diagnostics, d => d.Code == code || d.Code == "LMD099");
    }

    [Theory]
    [InlineData("第9条の2　本文。", false)]
    [InlineData("第9条の2 本文。", false)]
    [InlineData("第九条の二 本文。", false)]
    [InlineData("第38条の3の2 本文。", false)]
    [InlineData("第9条のA 本文。", true)]
    [InlineData("第9条の0 本文。", true)]
    [InlineData("第9条の二の 本文。", true)]
    public void Audit_BranchArticleValidation_Works(string articleLine, bool expectLmd101)
    {
        var lawtext = $"題名\n（令和六年規則第一号）\n\n{articleLine}\n";
        var result = new LawtextAuditService().Audit(lawtext, "x.law.txt", false);
        Assert.Equal(expectLmd101, result.Diagnostics.Any(d => d.Code == "LMD101"));
    }
}

public class ImportReportTests
{
    [Fact]
    public void ReportContainsOperationalSections()
    {
        var md = Path.GetTempFileName();
        var report = Path.GetTempFileName();
        var input = Path.Combine(TestHelpers.RepoRoot, "samples", "import-source.law.txt");
        var run = TestHelpers.RunZuke($"import {TestHelpers.QuoteArg(input)} -o {TestHelpers.QuoteArg(md)} --report {TestHelpers.QuoteArg(report)}");
        TestHelpers.AssertExitCode(run, 0);
        var text = File.ReadAllText(report);
        Assert.Contains("Input:", text);
        Assert.Contains("Output:", text);
        Assert.Contains("Reference Labels", text);
        Assert.Contains("診断一覧", text);
        Assert.Contains("生成参照名一覧", text);
        Assert.Contains("変換した参照表現一覧", text);
        Assert.Contains("未変換の参照表現一覧", text);
        Assert.Contains("roundtrip check結果", text);
        Assert.Contains("XSD検証結果", text);
        Assert.Contains("再実行コマンド例", text);
    }
}

public class ImportMapTests
{
    [Fact]
    public void MapContainsReadableAndTraceableItems()
    {
        var src = Path.GetTempFileName();
        File.WriteAllText(src, "題名\n（令和六年規則第一号）\n\n第一条\n  一　項本文\n    イ　子号本文\n");
        var md = Path.GetTempFileName();
        var map = Path.GetTempFileName();
        var run = TestHelpers.RunZuke($"import {TestHelpers.QuoteArg(src)} -o {TestHelpers.QuoteArg(md)} --map {TestHelpers.QuoteArg(map)} --reference-labels used");
        TestHelpers.AssertExitCode(run, 0);
        var json = File.ReadAllText(map);
        Assert.Contains("\"Kind\": \"Article\"", json);
        Assert.Contains("\"Kind\": \"Paragraph\"", json);
        Assert.Contains("\"Kind\": \"Item\"", json);
        Assert.Contains("\"LawtextLine\":", json);
        Assert.Contains("\"MarkdownLine\":", json);
        Assert.Contains("\"Number\":", json);
        Assert.Contains("\"ReferenceName\":", json);
    }
}

public class ConvertBothTests
{
    [Fact]
    public void ConvertBothCreatesXmlAndLawtext()
    {
        var xml = Path.GetTempFileName();
        var law = Path.GetTempFileName();
        var input = Path.Combine(TestHelpers.RepoRoot, "samples", "work-rules.md");
        var run = TestHelpers.RunZuke($"convert {TestHelpers.QuoteArg(input)} --to both --xml-output {TestHelpers.QuoteArg(xml)} --lawtext-output {TestHelpers.QuoteArg(law)}");
        TestHelpers.AssertExitCode(run, 0);
        Assert.True(File.Exists(xml));
        Assert.True(File.Exists(law));
        var lawtext = File.ReadAllText(law);
        Assert.DoesNotContain("{{参照:", lawtext);
        Assert.DoesNotContain("🍣", lawtext);
    }

    [Fact]
    public void ConvertBothRejectsInvalidOutputOptions()
    {
        var input = Path.Combine(TestHelpers.RepoRoot, "samples", "work-rules.md");
        var run1 = TestHelpers.RunZuke($"convert {TestHelpers.QuoteArg(input)} --to both -o out.xml");
        var run2 = TestHelpers.RunZuke($"convert {TestHelpers.QuoteArg(input)} --to both --xml-output out.xml");
        var run3 = TestHelpers.RunZuke($"convert {TestHelpers.QuoteArg(input)} --to both --lawtext-output out.law.txt");
        Assert.NotEqual(0, run1.ExitCode);
        Assert.NotEqual(0, run2.ExitCode);
        Assert.NotEqual(0, run3.ExitCode);
    }
}

public class WordToZukeWorkflowAcceptanceTests
{
    [Fact]
    public void EndToEndWorkflowWorks()
    {
        var md = Path.GetTempFileName();
        var xml = Path.GetTempFileName();
        var law = Path.GetTempFileName();

        var inputLaw = Path.Combine(TestHelpers.RepoRoot, "samples", "import-source.law.txt");
        var inputMd = Path.Combine(TestHelpers.RepoRoot, "samples", "work-rules.md");
        var audit = TestHelpers.RunZuke($"audit {TestHelpers.QuoteArg(inputLaw)}");
        TestHelpers.AssertExitCode(audit, 0);

        var import = TestHelpers.RunZuke($"import {TestHelpers.QuoteArg(inputLaw)} -o {TestHelpers.QuoteArg(md)}");
        TestHelpers.AssertExitCode(import, 0);

        var convert = TestHelpers.RunZuke($"convert {TestHelpers.QuoteArg(md)} --to both --xml-output {TestHelpers.QuoteArg(xml)} --lawtext-output {TestHelpers.QuoteArg(law)}");
        TestHelpers.AssertExitCode(convert, 0);
        var lawtext = File.ReadAllText(law);
        Assert.DoesNotContain("{{参照:", lawtext);
        Assert.DoesNotContain("🍣", lawtext);

        File.AppendAllText(md, "\n### （追加条） [条:追加条]\n追加本文\n");
        var diff = TestHelpers.RunZuke($"diff {TestHelpers.QuoteArg(inputMd)} {TestHelpers.QuoteArg(md)}");
        Assert.NotEqual(0, diff.ExitCode);
    }
}
