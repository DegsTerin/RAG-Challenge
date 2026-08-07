// Purpose: Defines the provider-neutral catalogue states and classifications owned by Domain; persistence and transport mappings remain outside this module.
namespace RagChallenge.Domain.CorpusCatalog;

public enum CatalogueItemStatus
{
    Candidate,
    Active,
    Deactivated,
    Removed,
}

public enum DocumentFormat
{
    Pdf,
    Csv,
}

public enum SupportedQueryLanguage
{
    PtBr,
    EnGb,
}

public enum SourceTrustClass
{
    LocalAuthorised,
    OfficialExternal,
}

public enum OfficialObservationState
{
    Current,
    Stale,
    Withdrawn,
    Deactivated,
}

public static class SupportedQueryLanguageExtensions
{
    public static string ToCanonicalTag(this SupportedQueryLanguage language) =>
        language switch
        {
            SupportedQueryLanguage.PtBr => "pt-BR",
            SupportedQueryLanguage.EnGb => "en-GB",
            _ => throw new ArgumentOutOfRangeException(
                nameof(language),
                language,
                "The query language must belong to the closed API v1 set."),
        };
}

public static class CatalogueLifecycle
{
    public static bool CanTransition(
        CatalogueItemStatus current,
        CatalogueItemStatus next) =>
        (current, next) switch
        {
            (CatalogueItemStatus.Candidate, CatalogueItemStatus.Active) => true,
            (CatalogueItemStatus.Candidate, CatalogueItemStatus.Removed) => true,
            (CatalogueItemStatus.Active, CatalogueItemStatus.Deactivated) => true,
            (CatalogueItemStatus.Deactivated, CatalogueItemStatus.Active) => true,
            (CatalogueItemStatus.Deactivated, CatalogueItemStatus.Removed) => true,
            _ => false,
        };

    public static void EnsureTransition(
        CatalogueItemStatus current,
        CatalogueItemStatus next)
    {
        if (!CanTransition(current, next))
        {
            throw new InvalidOperationException(
                $"Catalogue lifecycle transition '{current}' to '{next}' is not permitted.");
        }
    }
}
