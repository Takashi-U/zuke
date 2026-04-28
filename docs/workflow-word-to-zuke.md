# Word就業規則をzukeで安全に編集するワークフロー

1. Word就業規則をAIでLawtext化する
2. zuke auditでAI生成Lawtextを検査する
3. zuke importで拡張Markdown化する
4. import後Markdownをzuke convertでXML/Lawtextへ再変換して検証する
5. 拡張Markdownを編集する
6. zuke diffで変更差分を見る
7. zuke convertでLawtext/XMLへ出力する
8. Lawtext-app等でWordへ戻す

## 実行コマンド例

```bash
zuke audit samples/import-source.law.txt
zuke import samples/import-source.law.txt -o imported.md --report import-report.md --map import-map.json
zuke convert imported.md --to both --xml-output imported.xml --lawtext-output imported.law.txt
zuke diff before.md imported.md --view unified
```

## 注意点
- zuke importは元Markdownの完全復元ではない
- 参照名は自動生成される
- Wordの体裁・脚注・変更履歴は保持しない
- 表・別表・附則・様式はMVPでは手動確認が必要
- AI変換結果は必ず人間が確認する
- Wordは最終出力形式とし、編集元はMarkdownに寄せることを推奨する
