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

## --plain

`--plain` は絵文字装飾と色付き表示を抑制し、CIログ向けのプレーン出力にします。

## 著作権・ライセンス上の注意

- 実在法令・公式サンプルの長文コピーは避け、テストデータは自作文言を使用。
- 依存ライブラリのライセンスは `ThirdPartyNotices.md` を参照。

## 未対応範囲

- 表や複雑なHTMLブロックなど、法令標準XMLへ安全に落とし込めないMarkdown要素は未対応です。
- 未対応要素は診断（例: LMD046）で停止させる方針です。
