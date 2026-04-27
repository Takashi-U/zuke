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
