using System.Text;

namespace Zuke.Core.References;

public static class ReferenceNameNormalizer
{
    public static bool TryNormalize(string raw, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(raw)) return false;

        var folded = ToAscii(raw.Trim().Normalize(NormalizationForm.FormKC));
        if (folded.IndexOfAny([' ', '\u3000', '\r', '\n', ':', '|', '{', '}', '[', ']', '(', ')', '：', '（', '）']) >= 0)
        {
            return false;
        }

        normalized = folded.ToLowerInvariant();
        return normalized.Length > 0;
    }

    private static string ToAscii(string value)
    {
        Span<char> buffer = stackalloc char[value.Length];
        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            buffer[i] = c switch
            {
                >= '０' and <= '９' => (char)('0' + (c - '０')),
                >= 'Ａ' and <= 'Ｚ' => (char)('A' + (c - 'Ａ')),
                >= 'ａ' and <= 'ｚ' => (char)('a' + (c - 'ａ')),
                _ => c
            };
        }

        return buffer.ToString();
    }
}
