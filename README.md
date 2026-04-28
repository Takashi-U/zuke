# zuke

zuke は、**独自拡張Markdown** から **法令標準XML（XSD検証付き）** と **Lawtext** を生成し、差分表示（unified / terminal / HTML）まで行う .NET CLI ツールです。

## zukeとは何か

- Markdownで法令風文書を記述
- 参照名ラベルと参照マクロを解決
- 法令標準XML `XMLSchemaForJapaneseLaw_v3.xsd` に検証
- Lawtextへ整形出力
- 変更差分を unified / terminal / HTML で確認

## インストール

### 1) ビルド

```bash
dotnet restore
dotnet build -c Release
dotnet test -c Release
dotnet pack -c Release
```

### 2) ツールとしてインストール

```bash
dotnet tool install --global Zuke.Cli --add-source ./src/Zuke.Cli/bin/Release
```

## 基本コマンド

```bash
# XMLへ変換（既定でXSD検証あり）
zuke convert input.md -o output.xml --to xml

# Lawtextへ変換
zuke convert input.md -o output.law.txt --to lawtext

# Lawtextコマンド（ショートカット）
zuke lawtext input.md -o output.law.txt

# 番号表記（既定: kanji）
zuke convert input.md -o output.xml --to xml --number-style kanji
zuke convert input.md -o output.xml --to xml --number-style arabic

# diff
zuke diff old.md new.md --view unified
zuke diff old.md new.md --view terminal
zuke diff old.md new.md --view html -o diff.html
```

## Markdown独自構文

### 節の書き方

- `# 総則` / `# 章 総則` → 章
- `## 節 通則` → 節
- `### 目的` → 条
- 章直下の `## 目的` も条

### 参照名ラベル

条ラベル:
- `[条:届出]`
- `[a:notification]`

項ラベル:
- `[項:届出義務]`
- `[p:notification-duty]`

号ラベル:
- `[号:料金支払]`
- `[i:pay-fee]`

### 日本語参照 / 英語参照

- `{{参照:届出義務}}`
- `{{参照:届出義務|完全}}`
- `{{参照:届出義務|相対}}`
- `{{ref:notification-duty}}`
- `{{ref:notification-duty|full}}`
- `{{ref:notification-duty|relative}}`

### 相対参照

- 条: `前条`
- 項: `前項`
- 号: `前号`

成立しない相対参照はエラー（LMD027）になります。

### 手書き参照の警告

手書きの「前条」「前項」等は、参照マクロ利用に比べて保守性が低く、strictモードでは診断対象です。

## XML出力とXSD検証

`zuke convert input.md -o output.xml` は既定で XSD 検証を実行します。`--skip-validation` 指定時のみ省略されます。

## Lawtext出力

Lawtext出力後、未解決の参照マクロ・参照ラベル・絵文字（🍣）が残っているとエラー（LMD064 / LMD065）になります。

## --number-style

- 既定値は `kanji` です。
- `--number-style kanji` の場合、条・章・節・号・参照表現は漢数字で出力されます（例: `第一条`）。
- `--number-style arabic` の場合、条・章・節・号・参照表現は算用数字で出力されます（例: `第1条`）。
- この挙動は法令XML出力とLawtext出力で一貫します。

## diff

### unified diff

- old/newファイル名
- hunkヘッダ
- `+` 追加 / `-` 削除 / 空白 文脈行

### terminal diff

- Spectre.Consoleによる色付き表示
- `--plain` / `--no-color` で無彩色表示
- 画面幅が狭い場合は簡易表示にフォールバック

### HTML diff

- 単一HTML（外部CDNなし）
- side-by-side表示
- 行番号・差分サマリー付き
- 追加: 緑背景 / 削除: 赤背景


## 代表的な診断コード

- `LMD022`: 参照名重複（関連位置つき）
- `LMD026`: 未対応参照オプション
- `LMD027`: 相対参照が成立しない
- `LMD040`: MainProvision直下で章構成/条構成が混在
- `LMD041`: Chapter配下で節構成/条直下構成が混在
- `LMD044`: 生成XMLがXSD不適合
- `LMD046`: 未対応Markdown要素
- `LMD064`/`LMD065`: Lawtextに未解決マクロや絵文字が残存

## --plain

`--plain` は絵文字装飾と色付き表示を抑制し、CIログ向けのプレーン出力にします。

## 著作権・ライセンス上の注意

- 実在法令・公式サンプルの長文コピーは避け、テストデータは自作文言を使用。
- 依存ライブラリのライセンスは `ThirdPartyNotices.md` を参照。

## 未対応範囲

- 表や複雑なHTMLブロックなど、法令標準XMLへ安全に落とし込めないMarkdown要素は未対応です。
- 未対応要素は診断（例: LMD046）で停止させる方針です。

## Lawtext import

```bash
zuke import input.law.txt -o output.md
```

- Lawtextをzuke独自拡張Markdownへ変換します。
- 元Markdownの完全復元ではなく、import用途です。
- 参照名は自動生成されます（既定: ascii）。
- Lawtext中の前項・前条・前号・第二条第一項などは可能な範囲で参照マクロ化されます。
- 曖昧・未対応・未解決参照は日本語診断を出します。
- import後Markdownは通常の`zuke convert`でXML/Lawtextへ再変換できます。

主なオプション:

- `--reference-labels used|all|none`
- `--reference-mode conservative|aggressive|none`
- `--id-style ascii|japanese`（japanese は実験的）
- `--metadata-mode frontmatter|none`
- `--strict`
- `--skip-roundtrip-check`

## Lawtext audit

```bash
zuke audit input.law.txt
```

- AIが生成したLawtextの構造と参照を検査します。
- Word由来の変換ミスを検出する補助機能です。
- 完全な法的正確性を保証するものではありません。
- 人間による確認を前提とします。

## Lawtext import（補強）

- `--reference-labels` の既定値は `all` です。
- `--reference-labels used` は参照されている項・号を中心にラベルを出力します（条は常に出力）。
- `--report <PATH>` でインポート結果レポート（Markdown）を出力できます。
- `--map <PATH>` で Lawtext行↔Markdown行 の対応JSONを出力できます。
- import後Markdownは再コンパイル・再レンダリング検証されます。
- `--skip-roundtrip-check` は非推奨です。

## XMLとLawtextの同時出力

```bash
zuke convert input.md --to both --xml-output output.xml --lawtext-output output.law.txt
```
