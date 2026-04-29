namespace Zuke.Core.Numbering;

public sealed record ArticleNumber(int BaseNumber, IReadOnlyList<int> BranchNumbers)
{
    public bool HasBranch => BranchNumbers.Count > 0;

    public static ArticleNumber FromBase(int n) => new(n, []);
}
