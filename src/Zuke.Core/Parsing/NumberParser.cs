namespace Zuke.Core.Parsing; public static class NumberParser { public static int ParseInt(string t)=>int.TryParse(t,out var n)?n:0; }
