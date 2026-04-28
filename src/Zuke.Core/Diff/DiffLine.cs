namespace Zuke.Core.Diff;
public sealed record DiffLine(char Kind,string Text,int OldLineNumber,int NewLineNumber);
