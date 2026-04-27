# lawtext oracle

`lawtext` npm packageをテスト時の互換性確認にのみ使用します。

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
