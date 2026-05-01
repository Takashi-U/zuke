# zuke

zuke は、**独自拡張Markdown** から **法令標準XML（XSD検証付き）** と **Lawtext** を生成し、Lawtext から独自拡張Markdownへの import、構造検査、差分表示まで行う .NET CLI ツールです。

社内規程、就業規則、法令風の文書を、Markdownで管理しながら XML / Lawtext へ変換する用途を想定しています。

## 主な機能

- 独自拡張Markdownから法令標準XMLを生成
- 法令標準XMLスキーマ v3（`XMLSchemaForJapaneseLaw_v3.xsd`）によるXSD検証
- 独自拡張MarkdownからLawtextを生成
- Lawtextから独自拡張Markdownを生成
- Lawtextの構造・参照・変換崩れを検査
- 変更差分を unified / terminal / HTML で表示
- 参照名ラベルと参照マクロによる条・項・号参照の管理
- 社内規程向けのメタデータ補完

## 重要な注意事項・免責

zuke は、文書変換と構造検査を補助するためのツールです。法令、就業規則、社内規程その他の文書について、法的正確性、行政手続上の適合性、労務管理上の妥当性、提出先システムでの受理、特定目的への適合性を保証するものではありません。

本ツールを直接または間接に利用したこと、利用できなかったこと、出力結果を利用したこと、または出力結果に誤りが含まれていたことによって発生した一切の損害について、作者および貢献者は責任を負いません。ここでいう損害には、通常損害、特別損害、間接損害、付随的損害、派生的損害、逸失利益、業務停止、データ消失、行政上・法的・契約上の不利益を含みますが、これらに限りません。

重要な文書を利用・提出・公開する前には、必ず人間による確認を行ってください。必要に応じて、弁護士、社会保険労務士、行政機関、提出先システムの仕様管理者等に確認してください。

## 動作環境

- .NET 10 SDK
- PowerShell

このREADMEのコマンド例は、PowerShell環境を前提にしています。

## インストール

### NuGet.orgからインストール

正式公開後は、次のコマンドでインストールします。

```powershell
dotnet tool install --global Zuke.Cli
```

プレビュー版を指定して入れる場合は、バージョンを明示します。

```powershell
dotnet tool install --global Zuke.Cli --version 0.1.0-preview.1
```

インストール後、次で確認します。

```powershell
zuke --help
```

### 更新

```powershell
dotnet tool update --global Zuke.Cli
```

プレビュー版を指定して更新する場合は次のようにします。

```powershell
dotnet tool update --global Zuke.Cli --version 0.1.0-preview.1
```

### アンインストール

```powershell
dotnet tool uninstall --global Zuke.Cli
```

## ソースからビルドしてローカルインストールする

NuGet.orgに公開する前、または手元で修正版を確認する場合は、リポジトリをクローンしてローカルの `.nupkg` からインストールします。

```powershell
git clone https://github.com/Takashi-U/zuke.git
Set-Location .\zuke

dotnet restore .\zuke.sln
dotnet build .\zuke.sln -c Release
dotnet test .\zuke.sln -c Release
dotnet pack .\src\Zuke.Cli\Zuke.Cli.csproj -c Release -o .\nupkg
```

既に `Zuke.Cli` をインストールしている場合は、先に削除します。

```powershell
dotnet tool uninstall --global Zuke.Cli
```

ローカルパッケージからインストールします。

```powershell
$source = (Resolve-Path .\nupkg).Path
dotnet tool install --global Zuke.Cli --add-source $source --version 0.1.0-preview.1
```

動作確認します。

```powershell
zuke --help
```

## 最初の変換例

次の例では、簡単な規程Markdownを作成し、LawtextとXMLへ変換します。

```powershell
@'
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

### 目的 [条:目的]

この規則は、従業員の就業に関する事項を定める。

### 届出 [条:届出]

[項:届出義務]
従業員は、必要な事項を会社に届け出なければならない。

[項:届出方法]
{{参照:届出義務|相対}}に基づく届出は、会社が指定する方法により行う。
'@ | Set-Content -Path .\sample.md -Encoding UTF8
```

Lawtextへ変換します。

```powershell
zuke convert .\sample.md -o .\sample.law.txt --to lawtext
```

XMLへ変換します。XML出力では、既定で同梱XSDによる検証も行います。

```powershell
zuke convert .\sample.md -o .\sample.xml --to xml
```

XMLとLawtextを同時に出力する場合は、`--to both` を使います。

```powershell
zuke convert .\sample.md --to both --xml-output .\sample.xml --lawtext-output .\sample.law.txt
```

## コマンド一覧

zuke は次のコマンドを提供します。

```powershell
zuke convert <input>
zuke lawtext <input>
zuke diff <old> <new>
zuke import <input>
zuke audit <input>
```

### `convert`

MarkdownをXMLまたはLawtextへ変換します。

```powershell
zuke convert .\input.md -o .\output.xml --to xml
zuke convert .\input.md -o .\output.law.txt --to lawtext
```

主なオプションは次のとおりです。

| オプション | 説明 |
|---|---|
| `-o`, `--output <PATH>` | 出力先パス |
| `--to <FORMAT>` | `xml`、`lawtext`、`both` を指定。既定は `xml` |
| `--xml-output <PATH>` | `--to both` のXML出力先 |
| `--lawtext-output <PATH>` | `--to both` のLawtext出力先 |
| `--skip-validation` | XML出力時のXSD検証を省略 |
| `--xsd <PATH>` | 使用するXSDを明示 |
| `--strict` | 手書き参照などを厳格に診断 |
| `--number-style <STYLE>` | `auto`、`kanji`、`arabic` を指定 |
| `--metadata-profile <PROFILE>` | `default` または `internal-rule` |
| `--plain` | 絵文字装飾と色付き表示を抑制 |
| `--no-color` | 色付き表示を抑制 |
| `--emoji <MODE>` | 絵文字表示モードを指定 |

### `lawtext`

Lawtext出力用のショートカットです。

```powershell
zuke lawtext .\input.md -o .\output.law.txt
```

これは概ね次と同じです。

```powershell
zuke convert .\input.md -o .\output.law.txt --to lawtext
```

### `import`

Lawtextを zuke 独自拡張Markdownへ変換します。

```powershell
zuke import .\input.law.txt -o .\output.md
```

主なオプションは次のとおりです。

| オプション | 説明 |
|---|---|
| `--from <FORMAT>` | 入力形式。現在の既定は `lawtext` |
| `--reference-labels <MODE>` | `all`、`used`、`none`。既定は `all` |
| `--reference-mode <MODE>` | `conservative`、`aggressive`、`none` |
| `--id-style <STYLE>` | `ascii` または `japanese` |
| `--metadata-mode <MODE>` | `frontmatter` または `none` |
| `--strict` | import時の診断を厳格化 |
| `--skip-roundtrip-check` | import後の再変換チェックを省略。通常は非推奨 |
| `--report <PATH>` | import結果レポートをMarkdownで出力 |
| `--map <PATH>` | Lawtext行とMarkdown行の対応JSONを出力 |
| `--plain` | CIログ向けのプレーン出力 |

例です。

```powershell
zuke import .\input.law.txt -o .\imported.md --report .\import-report.md --map .\import-map.json
```

import後のMarkdownは、通常の `convert` で再変換できます。

```powershell
zuke convert .\imported.md -o .\roundtrip.law.txt --to lawtext
```

### `audit`

Lawtextの構造や参照を検査します。AI生成文書、Wordから変換した文書、手修正したLawtextの確認に使います。

```powershell
zuke audit .\input.law.txt
```

Markdownレポートを出力する場合は次のようにします。

```powershell
zuke audit .\input.law.txt --report .\audit-report.md
```

JSON形式で診断を出す場合は次のようにします。

```powershell
zuke audit .\input.law.txt --format json
```

### `diff`

2つのMarkdownをLawtext相当に正規化したうえで差分表示します。

```powershell
zuke diff .\old.md .\new.md --view unified
zuke diff .\old.md .\new.md --view terminal
zuke diff .\old.md .\new.md --view html -o .\diff.html
```

HTMLを生成して開く場合は次のようにします。

```powershell
zuke diff .\old.md .\new.md --view html -o .\diff.html --open
```

差分がある場合、`diff` は終了コード `1` を返します。CIで使う場合は、この挙動に注意してください。

## Markdown独自構文

### 章・節・条

zukeでは、見出しから章・節・条を判定します。

```markdown
# 総則

## 節 通則

### 目的
```

基本的な対応は次のとおりです。

| Markdown | 意味 |
|---|---|
| `# 総則` | 章 |
| `# 章 総則` | 章 |
| `## 節 通則` | 節 |
| `### 目的` | 条 |
| 章直下の `## 目的` | 条 |

節は、`## 節 通則` のように「節」を明示した場合に節として扱います。番号付きの `第1節` ではなく、番号なしの役割マーカーとして `節` を書く形式を推奨します。

### 参照名ラベル

条・項・号には参照名ラベルを付けられます。

```markdown
### 届出 [条:届出]

[項:届出義務]
従業員は、必要な事項を届け出なければならない。

- [号:料金支払] 料金を支払うこと。
```

英語風の短いIDも使えます。

```markdown
### Notification [a:notification]

[p:notification-duty]
The employee must notify the company.
```

### 参照マクロ

参照マクロを使うと、採番後の条・項・号番号に応じて参照表現を生成できます。

```markdown
{{参照:届出義務}}
{{参照:届出義務|完全}}
{{参照:届出義務|相対}}

{{ref:notification-duty}}
{{ref:notification-duty|full}}
{{ref:notification-duty|relative}}
```

相対参照では、文脈に応じて次のような表現を生成します。

- 前条
- 前項
- 前号

成立しない相対参照は `LMD027` として診断されます。

### 箇条書き

通常の号として扱いたい場合は、番号付きの項目を書きます。

```markdown
- [号:服務] 服務に関すること。
- [号:届出] 届出に関すること。
```

原文の `-` や `・` をできるだけ残したい場合は、raw bullet として扱われることがあります。raw bullet は、XML出力時のXSD適合と原文再現性の両方に関係するため、変換後のXMLとLawtextを必ず確認してください。

## メタデータ

XML出力では、Front Matterに法令XML用のメタデータが必要です。

```yaml
---
lawTitle: 就業規則
lawNum: 令和六年規則第一号
era: Reiwa
year: 6
num: 1
lawType: Misc
lang: ja
---
```

Lawtext出力では、少なくとも `lawTitle` が重要です。`lawNum` が空の場合、Lawtext冒頭の法令番号行は出力されません。

### 社内規程向けメタデータ補完

社内規程や就業規則には、正式な法令番号がない場合があります。その場合は、`--metadata-profile internal-rule` を使うと、不足メタデータを次の値で補完します。

| 項目 | 補完値 |
|---|---|
| `lawNum` | `社内規程` |
| `era` | `Reiwa` |
| `year` | `1` |
| `num` | `1` |
| `lawType` | `Misc` |
| `lang` | `ja` |

例です。

```powershell
zuke convert .\work-rules.md -o .\work-rules.xml --to xml --metadata-profile internal-rule
```

`lawTitle` は補完されません。文書名として重要なので、必ず明示してください。

## XML出力とXSD検証

`zuke convert .\input.md -o .\output.xml --to xml` は、既定でXSD検証を行います。

- zuke は `schemas/XMLSchemaForJapaneseLaw_v3.xsd` を同梱します。
- XSDは e-Gov 法令標準XMLスキーマ v3 を前提にしています。
- 公式XSD取得元は `https://laws.e-gov.go.jp/file/XMLSchemaForJapaneseLaw_v3.xsd` です。
- このリポジトリでの取得日は 2026-04-30 です。
- XSDに準拠しないXMLは `LMD044` として診断されます。

検証を一時的に省略する場合は、次のようにします。

```powershell
zuke convert .\input.md -o .\output.xml --to xml --skip-validation
```

別のXSDを指定する場合は、次のようにします。

```powershell
zuke convert .\input.md -o .\output.xml --to xml --xsd .\schemas\XMLSchemaForJapaneseLaw_v3.xsd
```

## 番号表記

`--number-style` で番号表記を指定できます。

| 値 | 説明 |
|---|---|
| `auto` | Front Matterやimport結果の指定を尊重。`convert` の既定 |
| `kanji` | `第一条`、`第一項` のような漢数字表記 |
| `arabic` | `第1条`、`第1項` のような算用数字表記 |

例です。

```powershell
zuke convert .\input.md -o .\kanji.law.txt --to lawtext --number-style kanji
zuke convert .\input.md -o .\arabic.law.txt --to lawtext --number-style arabic
```

## 代表的な診断コード

| コード | 意味 |
|---|---|
| `LMD022` | 参照名重複 |
| `LMD026` | 未対応参照オプション |
| `LMD027` | 相対参照が成立しない |
| `LMD040` | MainProvision直下で章構成と条構成が混在 |
| `LMD041` | Chapter配下で節構成と条直下構成が混在 |
| `LMD044` | 生成XMLがXSD不適合 |
| `LMD046` | 未対応Markdown要素 |
| `LMD064` | Lawtextに未解決マクロが残存 |
| `LMD065` | Lawtextに未解決ラベルや絵文字が残存 |

## CI向けの使い方

CIログでは、装飾を抑制するため `--plain` を使うことを推奨します。

```powershell
zuke convert .\input.md -o .\output.xml --to xml --plain
zuke audit .\input.law.txt --plain
```

`diff` は差分がある場合に終了コード `1` を返すため、差分検出を失敗扱いにしたくない場合は、CI側で終了コードを処理してください。

## NuGet配布前の確認

NuGet.orgへ公開する前に、少なくとも次を確認してください。

```powershell
dotnet restore .\zuke.sln
dotnet build .\zuke.sln -c Release
dotnet test .\zuke.sln -c Release
dotnet pack .\src\Zuke.Cli\Zuke.Cli.csproj -c Release -o .\nupkg
```

依存パッケージと脆弱性の確認は次のように行います。

```powershell
dotnet list .\zuke.sln package
dotnet list .\zuke.sln package --include-transitive
dotnet list .\zuke.sln package --vulnerable
```

パッケージ内容も確認してください。

```powershell
Get-ChildItem .\nupkg\*.nupkg
```

`.nupkg` には、少なくとも次が含まれていることを確認してください。

- `Zuke.Cli`
- `Zuke.Core`
- `README.md`
- `LICENSE`
- `ThirdPartyNotices.md`
- `schemas/XMLSchemaForJapaneseLaw_v3.xsd`

## リリース手順

以下は、PowerShellでの作業例です。

```powershell
git switch main
git pull origin main

dotnet restore .\zuke.sln
dotnet build .\zuke.sln -c Release
dotnet test .\zuke.sln -c Release
dotnet pack .\src\Zuke.Cli\Zuke.Cli.csproj -c Release -o .\nupkg
```

タグを作成してpushします。

```powershell
git tag v0.1.0-preview.1
git push origin v0.1.0-preview.1
```

GitHub ActionsでNuGet公開ワークフローを設定している場合は、タグpushを契機にNuGet.orgへ公開します。公開後、別環境で次を確認してください。

```powershell
dotnet tool install --global Zuke.Cli --version 0.1.0-preview.1
zuke --help
```

## ライセンス

zuke 本体は MIT License で提供します。詳細は `LICENSE` を参照してください。

依存パッケージのライセンスは `ThirdPartyNotices.md` を参照してください。主な実行時依存は次のとおりです。

| パッケージ | ライセンス |
|---|---|
| Markdig | BSD-2-Clause |
| DiffPlex | Apache-2.0 |
| Spectre.Console | MIT |
| Spectre.Console.Cli | MIT |
| YamlDotNet | MIT |

## 未対応範囲

- 表や複雑なHTMLブロックなど、法令標準XMLへ安全に落とし込めないMarkdown要素は未対応です。
- 未対応要素は診断で停止させる方針です。
- 本ツールは、法令・社内規程・就業規則の内容そのものを法的に検証するものではありません。
