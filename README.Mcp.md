# Zuke.Mcp

`Zuke.Mcp` は、[Model Context Protocol (MCP)](https://modelcontextprotocol.io/) 対応の AI アプリから、法令標準 XML 変換・Lawtext 変換・診断を安全に呼び出すための .NET グローバルツールです。

- 対象: Codex Desktop などの MCP ホスト
- 位置づけ: `Zuke.Cli` とは別パッケージ（MCP サーバー専用）
- トランスポート: `stdio`（MCP JSON-RPC）

## インストール

```powershell
dotnet tool install --global Zuke.Mcp --version 0.1.0-preview.2
```

起動:

```powershell
zuke-mcp
```

## Codex Desktop 等の MCP ホスト設定例

Windows の例（Codex Desktop 設定ファイル）:

```toml
[mcp_servers.zuke]
command = 'C:\Users\<USER>\.dotnet\tools\zuke-mcp.exe'
args = []
```

> 補足: `command` は環境に応じて実際のインストール先パスに合わせてください。

## 提供ツール一覧（MVP）

- `zuke_convert`（Markdown/Lawtext -> 法令標準 XML）
- `zuke_lawtext`（法令標準 XML -> Lawtext）
- `zuke_doctor`（実行環境・XSD 解決可否の確認）
- `zuke.compile_lawtext`（既存互換）
- `zuke.compile_xml`（既存互換）

## `zuke_doctor` の確認方法

MCP ホストから `zuke_doctor` を実行し、以下を確認してください。

- `success` が `true`
- `hasErrors` が `false`
- `summary` にエラーがないこと
- `outputs` / `diagnostics` に XSD 解決失敗が出ていないこと

## `zuke_lawtext` / `zuke_convert` の利用例

### 例1: `zuke_lawtext`

- 入力: 法令標準 XML 文字列
- 出力: Lawtext 文字列（`content` または `outputs`）

### 例2: `zuke_convert`

- 入力: Markdown または Lawtext 文字列
- 出力: 法令標準 XML 文字列（`content` または `outputs`）
- 補足: 既定で XSD 検証を実行し、問題は `diagnostics` に格納されます

## stdout の取り扱い（重要）

`Zuke.Mcp` は **stdout を MCP JSON-RPC 専用**として扱います。プロトコル破損を防ぐため、ログを stdout に出力しない設計です。

- 通信: stdout（JSON-RPC メッセージ）
- ログ/診断: 構造化レスポンス（`diagnostics` 等）で返却

## 現時点の未対応範囲

MVP では以下は未対応です。

- `import`
- `audit`
- `diff`

## 免責・人間確認

本ツールは法令・規程の技術的変換と診断を支援するものであり、法的判断を代替しません。公開・施行・運用に関わる最終判断は、必ず人間（法務・実務担当者）が原文・出力物を確認した上で行ってください。
