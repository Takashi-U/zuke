using Zuke.Core.Importing;
using Xunit;

namespace Zuke.Core.Tests;

public class LawtextImportReferenceDetectorTests
{
    [Theory]
    [InlineData("第二条")]
    [InlineData("第二条第一項")]
    [InlineData("第二条第一項第二号")]
    [InlineData("第2条")]
    [InlineData("第2条第1項")]
    [InlineData("前条")]
    [InlineData("前項")]
    [InlineData("前号")]
    [InlineData("第二項")]
    [InlineData("第二号")]
    public void DetectsReferenceToken(string token)
    {
        var found = new LawtextReferenceDetector().Detect($"{token}に基づく");
        Assert.Contains(found, t => t.Text == token);
    }
}
