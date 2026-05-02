# Zuke.Mcp MVP

`Zuke.Mcp` は Zuke のコア変換機能を MCP ツールとして公開する最小実装です。

## 提供ツール

- `zuke.compile_lawtext`
  - 入力: `markdown`, `strict` (既定 `false`), `numberStyle` (`kanji` / `arabic`)
  - 出力: `lawtext`, `diagnostics`, `hasErrors`
- `zuke.compile_xml`
  - 入力: `markdown`, `strict` (既定 `false`), `numberStyle` (`kanji` / `arabic`), `metadataProfile` (`default` / `internal-rule`)
  - 出力: `xml`, `diagnostics`, `hasErrors`

## セキュリティ制約

MVP では以下を満たします。

- トランスポートは `stdio` のみ（外部ポートを開かない）。
- ファイル読み書きツールを提供しない（入力文字列のみを処理）。
- 外部プロセス実行やネットワークアクセスを行わない。
- 失敗時は例外スタックを返さず、診断情報を構造化して返す。

## 検証手順

1. `dotnet build zuke.sln`
2. `dotnet test tests/Zuke.Core.Tests/Zuke.Core.Tests.csproj`
3. MCP ホスト起動確認: `dotnet run --project src/Zuke.Mcp/Zuke.Mcp.csproj`

## SDK 選定

`ModelContextProtocol` (公式 C# SDK) を採用。

- 公式実装のため MCP 仕様追従コストが低い。
- `WithStdioServerTransport` と attribute ベースの tool 定義で MVP 実装を短期間に作成できる。
- .NET Generic Host / DI と統合され、将来の認証・フィルタ拡張にも接続しやすい。
