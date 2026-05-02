# Zuke.Mcp MVP

`Zuke.Mcp` は Zuke のコア変換機能を MCP ツールとして公開する最小実装です。現時点では **compile/lawtext/xml/doctor のみ対応** し、import / audit / diff は未対応です。

## インストール

```powershell
dotnet tool install --global Zuke.Mcp --version 0.1.0-preview.2
```

ローカル `.nupkg` を使う場合:

```powershell
dotnet tool install --global Zuke.Mcp --add-source ./nupkg --version 0.1.0-preview.2
```

## 起動方法

`dotnet tool` インストール後、MCP サーバーは次で起動できます。

```powershell
zuke-mcp
```

トランスポートは `stdio` 固定です。

## 提供ツール

- `zuke.compile_lawtext`（既存互換）
- `zuke.compile_xml`（既存互換）
- `zuke_lawtext`（Issue #40 名称エイリアス）
- `zuke_convert`（Issue #40 名称エイリアス）
- `zuke_doctor`

## 共通レスポンス形式

各ツールは以下を含む構造化レスポンスを返します。

- `success`
- `summary`
- `diagnostics`
- `outputs` または `content`
- `hasErrors`

## XSD 検証の扱い

- `compile_xml` / `zuke_convert` は既定で XSD 検証を実行します。
- `ZukeXsdProvider.ResolveDefaultPath()` で既定 XSD パスを解決し、`LawXmlValidator` で検証します。
- XSD 不適合は `diagnostics` にエラーとして格納され、`success` / `hasErrors` に反映されます。
- `zuke_doctor` で XSD 解決可否と解決先パスを確認できます。

## セキュリティ制約

MVP では以下を満たします。

- トランスポートは `stdio` のみ（外部ポートを開かない）。
- ファイル読み書きツールを提供しない（入力文字列のみを処理）。
- 外部プロセス実行やネットワークアクセスを行わない。
- 失敗時は例外スタックを返さず、診断情報を構造化して返す。

## 現時点の未対応範囲

以下は次段階で対応予定です。

- `import`
- `audit`
- `diff`
