// Purpose: Separates closed query languages from validated document and source-declared BCP 47 values without registry, provider, resource, or locale inference.
namespace RagChallenge.Domain.CorpusCatalog;

public sealed record DocumentContentLanguage
{
    public static readonly DocumentContentLanguage PtBr = new("pt-BR");
    public static readonly DocumentContentLanguage EnGb = new("en-GB");

    public DocumentContentLanguage(string value)
    {
        CanonicalTag = Bcp47LanguageTag.Canonicalise(value, nameof(value));
    }

    public string CanonicalTag { get; }

    public bool IsSupportedByV1 =>
        string.Equals(CanonicalTag, "pt-BR", StringComparison.Ordinal) ||
        string.Equals(CanonicalTag, "en-GB", StringComparison.Ordinal);

    public string ToCanonicalTag() => CanonicalTag;

    public override string ToString() => CanonicalTag;
}

public sealed record SourceDeclaredLanguage
{
    public SourceDeclaredLanguage(string observedTag)
    {
        CanonicalTag = Bcp47LanguageTag.Canonicalise(observedTag, nameof(observedTag));
        ObservedTag = observedTag;
    }

    public string ObservedTag { get; }

    public string CanonicalTag { get; }

    public override string ToString() => ObservedTag;
}

internal static class Bcp47LanguageTag
{
    private const int MaximumLength = 128;

    private static readonly Dictionary<string, string> GrandfatheredTags =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["art-lojban"] = "art-lojban",
            ["cel-gaulish"] = "cel-gaulish",
            ["en-gb-oed"] = "en-GB-oed",
            ["i-ami"] = "i-ami",
            ["i-bnn"] = "i-bnn",
            ["i-default"] = "i-default",
            ["i-enochian"] = "i-enochian",
            ["i-hak"] = "i-hak",
            ["i-klingon"] = "i-klingon",
            ["i-lux"] = "i-lux",
            ["i-mingo"] = "i-mingo",
            ["i-navajo"] = "i-navajo",
            ["i-pwn"] = "i-pwn",
            ["i-tao"] = "i-tao",
            ["i-tay"] = "i-tay",
            ["i-tsu"] = "i-tsu",
            ["no-bok"] = "no-bok",
            ["no-nyn"] = "no-nyn",
            ["sgn-be-fr"] = "sgn-BE-FR",
            ["sgn-be-nl"] = "sgn-BE-NL",
            ["sgn-ch-de"] = "sgn-CH-DE",
            ["zh-guoyu"] = "zh-guoyu",
            ["zh-hakka"] = "zh-hakka",
            ["zh-min"] = "zh-min",
            ["zh-min-nan"] = "zh-min-nan",
            ["zh-xiang"] = "zh-xiang",
        };

    internal static string Canonicalise(string value, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);

        if (value.Length is 0 or > MaximumLength ||
            value.Any(character => !char.IsAsciiLetterOrDigit(character) && character != '-'))
        {
            throw Invalid(parameterName);
        }

        if (GrandfatheredTags.TryGetValue(value, out var grandfathered))
        {
            return grandfathered;
        }

        var subtags = value.Split('-');

        if (subtags.Any(subtag => subtag.Length == 0))
        {
            throw Invalid(parameterName);
        }

        if (string.Equals(subtags[0], "x", StringComparison.OrdinalIgnoreCase))
        {
            if (subtags.Length == 1 || subtags.Skip(1).Any(subtag =>
                subtag.Length is < 1 or > 8 || !IsAlphaNumeric(subtag)))
            {
                throw Invalid(parameterName);
            }

            return string.Join('-', subtags.Select(subtag => subtag.ToLowerInvariant()));
        }

        var primaryLanguage = subtags[0];

        if (primaryLanguage.Length is < 2 or > 8 || !IsAlpha(primaryLanguage))
        {
            throw Invalid(parameterName);
        }

        var canonical = new List<string> { primaryLanguage.ToLowerInvariant() };
        var index = 1;
        var extlangCount = 0;

        if (primaryLanguage.Length is 2 or 3)
        {
            while (index < subtags.Length && extlangCount < 3 &&
                subtags[index].Length == 3 && IsAlpha(subtags[index]))
            {
                canonical.Add(subtags[index].ToLowerInvariant());
                index++;
                extlangCount++;
            }
        }

        if (index < subtags.Length && subtags[index].Length == 4 && IsAlpha(subtags[index]))
        {
            var script = subtags[index].ToLowerInvariant();
            canonical.Add(char.ToUpperInvariant(script[0]) + script[1..]);
            index++;
        }

        if (index < subtags.Length &&
            (subtags[index].Length == 2 && IsAlpha(subtags[index]) ||
             subtags[index].Length == 3 && IsNumeric(subtags[index])))
        {
            canonical.Add(subtags[index].ToUpperInvariant());
            index++;
        }

        var variants = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        while (index < subtags.Length && IsVariant(subtags[index]))
        {
            if (!variants.Add(subtags[index]))
            {
                throw Invalid(parameterName);
            }

            canonical.Add(subtags[index].ToLowerInvariant());
            index++;
        }

        var extensionSingletons = new HashSet<char>();

        while (index < subtags.Length && subtags[index].Length == 1 &&
            IsAlphaNumeric(subtags[index]) &&
            !string.Equals(subtags[index], "x", StringComparison.OrdinalIgnoreCase))
        {
            var singleton = char.ToLowerInvariant(subtags[index][0]);

            if (!extensionSingletons.Add(singleton))
            {
                throw Invalid(parameterName);
            }

            canonical.Add(singleton.ToString());
            index++;
            var extensionStart = index;

            while (index < subtags.Length && subtags[index].Length is >= 2 and <= 8 &&
                IsAlphaNumeric(subtags[index]))
            {
                canonical.Add(subtags[index].ToLowerInvariant());
                index++;
            }

            if (index == extensionStart)
            {
                throw Invalid(parameterName);
            }
        }

        if (index < subtags.Length &&
            string.Equals(subtags[index], "x", StringComparison.OrdinalIgnoreCase))
        {
            canonical.Add("x");
            index++;
            var privateUseStart = index;

            while (index < subtags.Length && subtags[index].Length is >= 1 and <= 8 &&
                IsAlphaNumeric(subtags[index]))
            {
                canonical.Add(subtags[index].ToLowerInvariant());
                index++;
            }

            if (index == privateUseStart)
            {
                throw Invalid(parameterName);
            }
        }

        if (index != subtags.Length)
        {
            throw Invalid(parameterName);
        }

        return string.Join('-', canonical);
    }

    private static bool IsVariant(string value) =>
        value.Length is >= 5 and <= 8 && IsAlphaNumeric(value) ||
        value.Length == 4 && char.IsAsciiDigit(value[0]) && IsAlphaNumeric(value);

    private static bool IsAlpha(string value) => value.All(char.IsAsciiLetter);

    private static bool IsNumeric(string value) => value.All(char.IsAsciiDigit);

    private static bool IsAlphaNumeric(string value) => value.All(char.IsAsciiLetterOrDigit);

    private static ArgumentException Invalid(string parameterName) =>
        new(
            "A BCP 47 language tag must be 1..128 ASCII characters and satisfy the local fail-closed grammar.",
            parameterName);
}
