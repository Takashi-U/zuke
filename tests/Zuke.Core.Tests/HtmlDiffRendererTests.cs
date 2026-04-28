using Xunit;
using Zuke.Core.Diff;

namespace Zuke.Core.Tests;

public class HtmlDiffRendererTests
{
    [Fact]
    public void Render_GeneratesSideBySideHtml()
    {
        var result = new LawtextDiffService().Diff("a\nb\n", "a\nc\n", new DiffOptions(3));
        var html = new HtmlDiffRenderer().Render("old.md", "new.md", result);

        Assert.Contains("変更前", html);
        Assert.Contains("変更後", html);
        Assert.Contains("class='del'", html);
        Assert.Contains("class='add'", html);
        Assert.Contains("<style>", html);
        Assert.Contains("Diff:", html);
    }

    [Fact]
    public void Render_SingleLineChange_IsRenderedInSameRow()
    {
        var html = Render("before\n", "after\n");

        Assert.Contains("<td class='ln'>1</td><td class='del'>", html);
        Assert.Contains("</td><td class='ln'>1</td><td class='add'>", html);
    }

    [Fact]
    public void Render_TwoDeletesOneAdd_SecondDeleteHasEmptyRightSide()
    {
        var html = Render("a\nb\n", "x\n");

        Assert.Contains("<td class='ln'>1</td><td class='del'>", html);
        Assert.Contains("<td class='ln'>1</td><td class='add'>", html);
        Assert.Contains("<td class='ln'>2</td><td class='del'>b</td><td class='ln'></td><td class='ctx'></td>", html);
    }

    [Fact]
    public void Render_OneDeleteTwoAdds_SecondAddHasEmptyLeftSide()
    {
        var html = Render("a\n", "x\ny\n");

        Assert.Contains("<td class='ln'>1</td><td class='del'>", html);
        Assert.Contains("<td class='ln'>1</td><td class='add'>", html);
        Assert.Contains("<td class='ln'></td><td class='ctx'></td><td class='ln'>2</td><td class='add'>y</td>", html);
    }

    [Fact]
    public void Render_ContextLine_IsShownOnBothSides()
    {
        var html = Render("same\nold\n", "same\nnew\n");

        Assert.Contains("<td class='ln'>1</td><td class='ctx'>same</td><td class='ln'>1</td><td class='ctx'>same</td>", html);
    }

    [Fact]
    public void Render_PreservesHtmlEscaping()
    {
        var html = Render("<a>&</a>\n", "<b>&</b>\n");

        Assert.Contains("&lt;", html);
        Assert.Contains("&gt;", html);
        Assert.Contains("&amp;", html);
        Assert.DoesNotContain("<a>&</a>", html);
        Assert.DoesNotContain("<b>&</b>", html);
        Assert.DoesNotContain("<script>", html);
    }

    private static string Render(string oldText, string newText)
    {
        var result = new LawtextDiffService().Diff(oldText, newText, new DiffOptions(3));
        return new HtmlDiffRenderer().Render("old.md", "new.md", result);
    }
}
