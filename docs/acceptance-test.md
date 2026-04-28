# zuke MVP受入テスト実施記録

実施日: 2026-04-28 (UTC)
実施環境: `/workspace/zuke`

## 実行結果

1. `dotnet clean`  
   - ✅ 成功 (終了コード 0)
2. `dotnet restore`  
   - ✅ 成功 (終了コード 0)
3. `dotnet build -c Release`  
   - ✅ 成功 (終了コード 0)
4. `dotnet test -c Release`  
   - ✅ 成功 (Failed: 0, Passed: 86, Skipped: 0, Total: 86)
5. `dotnet pack -c Release`  
   - ✅ 成功 (終了コード 0)
6. `dotnet tool install --global Zuke.Cli --add-source ./nupkg`  
   - ✅ 成功 (version 0.1.0 をインストール)
7. `zuke convert samples/work-rules.md -o out.xml`  
   - ✅ 成功 (`out.xml` 生成)
8. `zuke convert samples/work-rules.md -o out.law.txt --to lawtext`  
   - ✅ 成功 (`out.law.txt` 生成)
9. `zuke lawtext samples/work-rules.md -o out2.law.txt`  
   - ✅ 成功 (`out2.law.txt` 生成)
10. `zuke diff samples/work-rules.md samples/work-rules-revised.md`  
    - ✅ 差分ありのため終了コード 1（想定どおり）
11. `zuke diff samples/work-rules.md samples/work-rules-revised.md --view html -o diff.html`  
    - ✅ 差分ありのため終了コード 1（想定どおり、`diff.html` 生成）

## 確認事項

- `out.xml` が `XMLSchemaForJapaneseLaw_v3.xsd` で検証成功すること  
  - ✅ `System.Xml.Schema` を使った検証プログラムで `XSD validation passed` を確認
- `out.law.txt` に参照マクロが残っていないこと  
  - ✅ `rg -n '\{\{.*\}\}' out.law.txt` の結果 0 件
- `out.law.txt` に 🍣 が混入していないこと  
  - ✅ `rg -n '🍣' out.law.txt` の結果 0 件
- `diff.html` がブラウザで見やすく表示されること  
  - ⚠️ 本実行環境はブラウザUI確認不可。`diff.html` 生成とHTML出力を確認
- 失敗時の日本語エラーが行番号付きで出ること  
  - ✅ `/tmp/invalid-zuke.md` に対する `zuke convert` で以下を確認
    - `エラー LMD045 ... /tmp/invalid-zuke.md:1:1`
    - `エラー LMD043 ... /tmp/invalid-zuke.md:3:1`

## 補足

- 手順6の前に、`dotnet pack` の成果物 `src/Zuke.Cli/bin/Release/Zuke.Cli.0.1.0.nupkg` を `./nupkg` に配置して実行。
- `zuke diff` は差分がある場合に終了コード 1 を返すため、CIで扱う場合は仕様として許容が必要。
