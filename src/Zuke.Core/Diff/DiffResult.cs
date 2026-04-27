namespace Zuke.Core.Diff; public sealed record DiffResult(bool HasChanges,string UnifiedText,IReadOnlyList<DiffHunk> Hunks);
