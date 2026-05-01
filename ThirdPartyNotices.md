# Third-Party Notices

この文書は、zuke が利用する主な第三者パッケージのライセンス情報を整理するものです。

zuke 本体は MIT License で提供します。zuke 本体のライセンスは `LICENSE` を参照してください。

## 実行時依存

`Zuke.Cli` / `Zuke.Core` の実行時に利用する直接依存です。

| Package | Version | License | Purpose |
|---|---:|---|---|
| Markdig | 0.37.0 | BSD-2-Clause | Markdown parsing |
| DiffPlex | 1.7.2 | Apache-2.0 | Text diff generation |
| Spectre.Console | 0.49.1 | MIT | Console rendering |
| Spectre.Console.Cli | 0.49.1 | MIT | CLI command framework |
| YamlDotNet | 16.3.0 | MIT | YAML parsing |

## テスト専用依存

テストプロジェクトで利用する依存です。通常、NuGet global tool として配布する `Zuke.Cli` の実行時機能には含めません。

| Package | Version | License | Purpose |
|---|---:|---|---|
| xunit | 2.9.2 | Apache-2.0 | Unit testing |
| xunit.runner.visualstudio | 3.1.5 | Apache-2.0 | Test runner integration |
| Microsoft.NET.Test.Sdk | 17.14.1 | MIT | .NET test SDK |

## lawtext npm package

- Package: `lawtext`
- Upstream repository: https://github.com/yamachig/Lawtext
- License: MIT
- Usage in this project: テスト時のLawtext互換性確認（oracle）専用
- Note: zuke本体へ upstream ソースコードのコピー/移植は行っていません。

## 確認手順

リリース前には、直接依存だけでなく推移依存も確認してください。

```powershell
dotnet restore .\zuke.sln
dotnet list .\zuke.sln package
dotnet list .\zuke.sln package --include-transitive
dotnet list .\zuke.sln package --vulnerable
```

必要に応じて、各パッケージのNuGetページ、上流リポジトリ、同梱されるライセンス文書を確認してください。
