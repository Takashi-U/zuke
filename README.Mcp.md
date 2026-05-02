# Zuke.Mcp

`Zuke.Mcp` は、MCP ホスト（Codex Desktop など）から Zuke.Core の主要変換機能を **文字列入力・文字列出力** で安全に呼び出すための .NET ツールです。

- `Zuke.Cli` には依存せず、`Zuke.Core` を直接利用
- トランスポートは `stdio`（MCP JSON-RPC）
- stdout は JSON-RPC 専用（`Console.WriteLine` などで汚さない）
- ファイル I/O / 任意コマンド実行 / Git / ネットワーク操作ツールは未提供

## 提供ツール

- `zuke_import`（Lawtext → 拡張 Markdown）
- `zuke_audit`（Lawtext 監査）
- `zuke_diff`（Markdown 同士を Lawtext 正規化して unified diff 生成）
- `zuke_validate_xml`（XML を同梱 XSD で検証）
- `zuke_convert`（`to=xml|lawtext|both`）
- `zuke_lawtext`（既存互換）
- `zuke_doctor`
- `zuke.compile_lawtext`（既存互換）
- `zuke.compile_xml`（既存互換）

## MCP ホストからの自然言語利用例

- 「この Lawtext を zuke_import で Markdown に変換して」
- 「この Lawtext を zuke_audit で strict=true で監査して、reportMarkdown も返して」
- 「この old/new Markdown を zuke_diff で比較して（context=5）」
- 「この XML を zuke_validate_xml で検証して」
- 「この Markdown を zuke_convert で to=both にして XML と Lawtext 両方出して」

## 診断コード

- `MCP004`: unsupported option or mode
- `MCP005`: XSD cannot be resolved
- `MCP999`: unexpected exception

## 免責

法的判断を代替するものではありません。公開・施行・運用前に必ず人間が原文・変換結果を確認してください。
