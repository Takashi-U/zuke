# zuke

独自拡張Markdownを法令標準XML/Lawtext/diffへ変換するCLIです。

## Build

```bash
dotnet build zuke.sln
dotnet test zuke.sln
dotnet pack src/Zuke.Cli/Zuke.Cli.csproj -c Release -o ./nupkg
```

## Install tool

```bash
dotnet tool install --global Zuke.Cli --add-source ./nupkg
```

## Lawtext出力

```bash
zuke lawtext samples/work-rules.md -o out.law.txt
zuke convert samples/work-rules.md -o out.law.txt --to lawtext
```

Lawtext出力はLF改行・末尾改行あり・UTF-8 no BOMで出力されます。

## Lawtext oracle compatibility test (optional)

Lawtext oracle互換テストは任意で、zuke本体の実行時依存ではありません。

```bash
cd tools/lawtext-oracle
npm ci
cd ../..
ZUKE_RUN_LAWTEXT_ORACLE=1 dotnet test
```

PowerShell:

```powershell
$env:ZUKE_RUN_LAWTEXT_ORACLE="1"
dotnet test
```

## Copyright / License notes

- 公式Lawtext実装のコードをzuke本体へコピー・移植しない方針です。
- テストデータは原則として自作の短文を使用します。
- `lawtext` npm packageは互換性確認（テスト時のみ）で利用します。詳細は `ThirdPartyNotices.md` を参照してください。
