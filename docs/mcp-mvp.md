# Zuke.Mcp MVP

`Zuke.Mcp` は Zuke.Core 機能を MCP ツールとして提供する最小実装です。

## 現在対応している機能

- compile/lawtext/xml/doctor
- import
- audit
- diff
- validate_xml

すべて **文字列入力・文字列出力** で動作し、ファイル I/O は提供しません。

## 提供ツール

- `zuke_import`
- `zuke_audit`
- `zuke_diff`（`view=unified` のみ）
- `zuke_validate_xml`
- `zuke_convert`（`to=xml|lawtext|both`）
- `zuke_lawtext`
- `zuke_doctor`
- `zuke.compile_lawtext`
- `zuke.compile_xml`

## セキュリティ制約

- `stdio` のみ（stdout は MCP JSON-RPC 専用）
- ファイル I/O ツールなし
- 外部プロセス実行なし
- ネットワークアクセスなし
- 例外時は `MCP999` で構造化返却（スタックトレース非返却）

## 既知の未対応

- `zuke_diff` の `view=html|terminal`（`MCP004` を返却）
