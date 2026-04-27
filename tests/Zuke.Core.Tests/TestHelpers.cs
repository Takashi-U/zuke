using Zuke.Core.Compilation;

namespace Zuke.Core.Tests;

public static class TestHelpers
{
    public static CompileResult Compile(string? markdown = null)
    {
        markdown ??= """
---
lawTitle: 就業規則
lawNum: 令和六年規則第一号
era: Reiwa
year: 6
num: 1
lawType: Misc
lang: ja
---
# 総則
## 節 通則
### 目的
この規則は、従業員の就業に関する事項を定める。
""";
        return new LawMarkdownCompiler().Compile(markdown, "test.md", new CompileOptions());
    }
}
