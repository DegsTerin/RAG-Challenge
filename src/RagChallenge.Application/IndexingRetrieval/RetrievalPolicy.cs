// Purpose: Owns the typed retrieval-v2 policy boundary, deterministic ranking validation and pre-generation evidence selection; vector stores remain replaceable outer adapters.
using System.Buffers.Binary;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Domain.IndexingRetrieval;

namespace RagChallenge.Application.IndexingRetrieval;

public enum RetrievalPolicyOutcome
{
    Succeeded,
    NoSelectedEvidenceUnderPolicy,
    InvalidQueryVector,
    GenerationUnavailable,
    InvalidIndexData,
    ContractViolation,
    InvalidConfiguration,
    OperationCancelled,
    UnexpectedFailure,
}

public enum RetrievalNoEvidenceReason
{
    NoRawHits,
    BelowMinimumScore,
    ScalarBudgetExcludedAll,
}

public sealed record RetrievalPolicyConfiguration(
    string RetrievalPolicyVersion,
    string MinimumScorePolicyVersion,
    int MaximumResults,
    double MinimumScore,
    int MaximumSelectedEvidence,
    int MaximumSelectedEvidenceScalars,
    EmbeddingProviderDescriptor ExpectedEmbeddingDescriptor,
    IndexCompatibilityKey ExpectedIndexCompatibilityKey)
{
    public const string RetrievalV2 = "retrieval-v2";
    public const string MinimumScoreV1 = "minimum-score-v1";
    public const string QueryVectorRepresentation = "ieee754-binary32-little-endian-v1";
    public const int RetrievalV2MaximumResults = 8;
    public const double RetrievalV2MinimumScore = 0.25;
    public const int RetrievalV2MaximumSelectedEvidence = 6;
    public const int RetrievalV2MaximumSelectedEvidenceScalars = 16000;

    public static RetrievalPolicyConfiguration CreateRetrievalV2(
        EmbeddingProviderDescriptor expectedEmbeddingDescriptor,
        IndexCompatibilityKey expectedIndexCompatibilityKey) =>
        new(
            RetrievalV2,
            MinimumScoreV1,
            RetrievalV2MaximumResults,
            RetrievalV2MinimumScore,
            RetrievalV2MaximumSelectedEvidence,
            RetrievalV2MaximumSelectedEvidenceScalars,
            expectedEmbeddingDescriptor,
            expectedIndexCompatibilityKey);

    internal bool IsCanonicalRetrievalV2 =>
        string.Equals(RetrievalPolicyVersion, RetrievalV2, StringComparison.Ordinal) &&
        string.Equals(MinimumScorePolicyVersion, MinimumScoreV1, StringComparison.Ordinal) &&
        MaximumResults == RetrievalV2MaximumResults &&
        MinimumScore == RetrievalV2MinimumScore &&
        MaximumSelectedEvidence == RetrievalV2MaximumSelectedEvidence &&
        MaximumSelectedEvidenceScalars == RetrievalV2MaximumSelectedEvidenceScalars &&
        ExpectedEmbeddingDescriptor is not null &&
        ExpectedIndexCompatibilityKey is not null;
}

public sealed class RetrievalPolicyRequest
{
    public RetrievalPolicyRequest(
        QueryActivationSnapshot activationSnapshot,
        IReadOnlyCollection<QueryEvidenceBinding> eligibleBindings,
        ReadOnlyMemory<float> queryVector,
        EmbeddingProviderDescriptor observedEmbeddingDescriptor,
        SupportedQueryLanguage questionLanguage,
        QueryContractVersion eligibilityPolicyVersion,
        RetrievalPolicyConfiguration applicablePolicy,
        IReadOnlyCollection<DatabaseProductId>? databaseProductFilters = null,
        IReadOnlyCollection<DocumentId>? documentFilters = null)
    {
        ActivationSnapshot = activationSnapshot ??
            throw new ArgumentNullException(nameof(activationSnapshot));
        ArgumentNullException.ThrowIfNull(eligibleBindings);
        EligibleBindings = Array.AsReadOnly(eligibleBindings.ToArray());
        QueryVector = queryVector.ToArray();
        QueryVectorRepresentation =
            RetrievalPolicyConfiguration.QueryVectorRepresentation;
        QueryVectorSha256 = ComputeQueryVectorSha256(QueryVector.Span);
        ObservedEmbeddingDescriptor = observedEmbeddingDescriptor ??
            throw new ArgumentNullException(nameof(observedEmbeddingDescriptor));
        QuestionLanguage = questionLanguage;
        EligibilityPolicyVersion = eligibilityPolicyVersion;
        ApplicablePolicy = applicablePolicy ??
            throw new ArgumentNullException(nameof(applicablePolicy));
        DatabaseProductFilters = Array.AsReadOnly(
            databaseProductFilters?.ToArray() ?? []);
        DocumentFilters = Array.AsReadOnly(documentFilters?.ToArray() ?? []);
    }

    public QueryActivationSnapshot ActivationSnapshot { get; }

    public ReadOnlyCollection<QueryEvidenceBinding> EligibleBindings { get; }

    public ReadOnlyMemory<float> QueryVector { get; }

    public string QueryVectorRepresentation { get; }

    public string QueryVectorSha256 { get; }

    public EmbeddingProviderDescriptor ObservedEmbeddingDescriptor { get; }

    public SupportedQueryLanguage QuestionLanguage { get; }

    public QueryContractVersion EligibilityPolicyVersion { get; }

    public RetrievalPolicyConfiguration ApplicablePolicy { get; }

    public ReadOnlyCollection<DatabaseProductId> DatabaseProductFilters { get; }

    public ReadOnlyCollection<DocumentId> DocumentFilters { get; }

    private static string ComputeQueryVectorSha256(ReadOnlySpan<float> vector)
    {
        var bytes = new byte[checked(vector.Length * sizeof(float))];

        for (var index = 0; index < vector.Length; index++)
        {
            BinaryPrimitives.WriteSingleLittleEndian(
                bytes.AsSpan(index * sizeof(float), sizeof(float)),
                vector[index]);
        }

        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }
}

public sealed record RetrievalPolicyIdentity(
    CorpusId CorpusId,
    ActivationRecordRevision ActivationRecordRevision,
    IndexGenerationId IndexGenerationId,
    IndexCompatibilityKey IndexCompatibilityKey,
    QueryContractVersion EligibilityPolicyVersion,
    string RetrievalPolicyVersion,
    string MinimumScorePolicyVersion,
    EmbeddingProviderDescriptor EmbeddingDescriptor,
    GenerationContentDigest GenerationManifestDigest,
    SourceBindingSetDigest SourceBindingSetDigest,
    ActivationBindingSetDigest ActivationBindingSetDigest,
    string QueryVectorRepresentation,
    string QueryVectorSha256,
    string PolicyManifestSha256);

public sealed record RetrievalRankedHit(
    int Rank,
    CandidateBuildId CandidateBuildId,
    VectorSearchBindingSelector BindingSelector,
    long ChunkOrdinal,
    LogicalArtifactDigest ChunkDigest,
    double Score);

public sealed record RetrievalSelectedEvidence(
    int RawRank,
    string ChunkId,
    VectorSearchHit Hit,
    QueryEvidenceBinding Binding);

public sealed class RetrievalPolicyResult
{
    private RetrievalPolicyResult(
        RetrievalPolicyOutcome outcome,
        RetrievalNoEvidenceReason? noEvidenceReason,
        IEnumerable<RetrievalRankedHit>? rankedHits,
        IEnumerable<RetrievalSelectedEvidence>? selectedEvidence,
        RetrievalPolicyIdentity? identity,
        string? failureIdentity)
    {
        var materialisedRankedHits = rankedHits?.ToArray() ?? [];
        var materialisedSelectedEvidence = selectedEvidence?.ToArray() ?? [];

        if (!Enum.IsDefined(outcome) ||
            outcome == RetrievalPolicyOutcome.Succeeded &&
                (noEvidenceReason is not null ||
                 materialisedRankedHits.Length == 0 ||
                 materialisedSelectedEvidence.Length == 0 ||
                 identity is null ||
                 failureIdentity is not null) ||
            outcome == RetrievalPolicyOutcome.NoSelectedEvidenceUnderPolicy &&
                (noEvidenceReason is null ||
                 !Enum.IsDefined(noEvidenceReason.Value) ||
                 materialisedSelectedEvidence.Length != 0 ||
                 identity is null ||
                 failureIdentity is not null) ||
            outcome is not RetrievalPolicyOutcome.Succeeded and
                not RetrievalPolicyOutcome.NoSelectedEvidenceUnderPolicy &&
                (noEvidenceReason is not null ||
                 materialisedRankedHits.Length != 0 ||
                 materialisedSelectedEvidence.Length != 0 ||
                 string.IsNullOrWhiteSpace(failureIdentity)))
        {
            throw new ArgumentException(
                "A retrieval-policy result must represent one coherent typed outcome.");
        }

        Outcome = outcome;
        NoEvidenceReason = noEvidenceReason;
        RankedHits = Array.AsReadOnly(materialisedRankedHits);
        SelectedEvidence = Array.AsReadOnly(materialisedSelectedEvidence);
        Identity = identity;
        FailureIdentity = failureIdentity;
    }

    public RetrievalPolicyOutcome Outcome { get; }

    public RetrievalNoEvidenceReason? NoEvidenceReason { get; }

    public ReadOnlyCollection<RetrievalRankedHit> RankedHits { get; }

    public ReadOnlyCollection<RetrievalSelectedEvidence> SelectedEvidence { get; }

    public RetrievalPolicyIdentity? Identity { get; }

    public string? FailureIdentity { get; }

    public static RetrievalPolicyResult Successful(
        IEnumerable<RetrievalRankedHit> rankedHits,
        IEnumerable<RetrievalSelectedEvidence> selectedEvidence,
        RetrievalPolicyIdentity identity) =>
        new(
            RetrievalPolicyOutcome.Succeeded,
            noEvidenceReason: null,
            rankedHits,
            selectedEvidence,
            identity,
            failureIdentity: null);

    public static RetrievalPolicyResult NoEvidence(
        RetrievalNoEvidenceReason reason,
        IEnumerable<RetrievalRankedHit> rankedHits,
        RetrievalPolicyIdentity identity) =>
        new(
            RetrievalPolicyOutcome.NoSelectedEvidenceUnderPolicy,
            reason,
            rankedHits,
            selectedEvidence: null,
            identity,
            failureIdentity: null);

    public static RetrievalPolicyResult Failed(
        RetrievalPolicyOutcome outcome,
        RetrievalPolicyIdentity? identity)
    {
        if (outcome is RetrievalPolicyOutcome.Succeeded or
            RetrievalPolicyOutcome.NoSelectedEvidenceUnderPolicy)
        {
            throw new ArgumentOutOfRangeException(nameof(outcome));
        }

        return new RetrievalPolicyResult(
            outcome,
            noEvidenceReason: null,
            rankedHits: null,
            selectedEvidence: null,
            identity,
            $"RETRIEVAL_{ToUpperSnakeCase(outcome.ToString())}");
    }

    private static string ToUpperSnakeCase(string value)
    {
        var result = new StringBuilder(value.Length + 8);

        foreach (var character in value)
        {
            if (char.IsUpper(character) && result.Length != 0)
            {
                result.Append('_');
            }

            result.Append(char.ToUpperInvariant(character));
        }

        return result.ToString();
    }
}

public interface IRetrievalPolicyExecutor
{
    Task<RetrievalPolicyResult> ExecuteAsync(
        RetrievalPolicyRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class RetrievalV2PolicyExecutor(
    IVectorIndexStore vectorStore,
    RetrievalPolicyConfiguration? configuration) : IRetrievalPolicyExecutor
{
    private readonly IVectorIndexStore vectorStore = vectorStore ??
        throw new ArgumentNullException(nameof(vectorStore));
    private readonly RetrievalPolicyConfiguration? configuration = configuration;

    public async Task<RetrievalPolicyResult> ExecuteAsync(
        RetrievalPolicyRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (configuration is null ||
            !configuration.IsCanonicalRetrievalV2 ||
            !MatchesConfiguredPolicy(request.ApplicablePolicy, configuration))
        {
            return RetrievalPolicyResult.Failed(
                RetrievalPolicyOutcome.InvalidConfiguration,
                identity: null);
        }

        var generationFailure = ValidateGeneration(request, configuration);

        if (generationFailure is not null)
        {
            return RetrievalPolicyResult.Failed(
                generationFailure.Value,
                identity: null);
        }

        var identity = CreateIdentity(request, configuration);

        if (!IsValidQueryVector(request, configuration))
        {
            return RetrievalPolicyResult.Failed(
                RetrievalPolicyOutcome.InvalidQueryVector,
                identity);
        }

        if (!TryCreateEligibleBindings(request, out var bindings))
        {
            return RetrievalPolicyResult.Failed(
                RetrievalPolicyOutcome.InvalidConfiguration,
                identity);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return RetrievalPolicyResult.Failed(
                RetrievalPolicyOutcome.OperationCancelled,
                identity);
        }

        VectorSearchResult searchResult;

        try
        {
            searchResult = await vectorStore.SearchExactAsync(
                new VectorSearchRequest(
                    request.ActivationSnapshot.ActivationRecord.CorpusId,
                    request.ActivationSnapshot.ActivationRecord.IndexGenerationId,
                    configuration.ExpectedIndexCompatibilityKey,
                    request.QueryVector,
                    configuration.MaximumResults,
                    bindings.Keys.ToArray(),
                    request.DatabaseProductFilters,
                    request.DocumentFilters),
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return RetrievalPolicyResult.Failed(
                RetrievalPolicyOutcome.OperationCancelled,
                identity);
        }
        catch (KeyNotFoundException)
        {
            return RetrievalPolicyResult.Failed(
                RetrievalPolicyOutcome.GenerationUnavailable,
                identity);
        }
        catch (InvalidDataException)
        {
            return RetrievalPolicyResult.Failed(
                RetrievalPolicyOutcome.InvalidIndexData,
                identity);
        }
        catch (ArgumentException)
        {
            return RetrievalPolicyResult.Failed(
                RetrievalPolicyOutcome.ContractViolation,
                identity);
        }
        catch (Exception)
        {
            return RetrievalPolicyResult.Failed(
                RetrievalPolicyOutcome.UnexpectedFailure,
                identity);
        }

        if (searchResult is null || searchResult.Outcome != VectorSearchOutcome.Succeeded)
        {
            return RetrievalPolicyResult.Failed(
                MapSearchFailure(searchResult?.Outcome),
                identity);
        }

        if (!TryValidateHits(
                searchResult.Hits,
                bindings,
                request.ActivationSnapshot.ActivationRecord.CorpusId,
                request.ActivationSnapshot.ActivationRecord.IndexGenerationId,
                configuration.MaximumResults,
                out var rankedHits,
                out var orderedHits,
                out var validationFailure))
        {
            return RetrievalPolicyResult.Failed(validationFailure, identity);
        }

        var selected = SelectEvidence(orderedHits, configuration);

        if (!HasValidSelectedOrder(selected, configuration))
        {
            return RetrievalPolicyResult.Failed(
                RetrievalPolicyOutcome.ContractViolation,
                identity);
        }

        if (selected.Count != 0)
        {
            return RetrievalPolicyResult.Successful(rankedHits, selected, identity!);
        }

        var reason = orderedHits.Count == 0
            ? RetrievalNoEvidenceReason.NoRawHits
            : orderedHits.All(item => item.Hit.Score < configuration.MinimumScore)
                ? RetrievalNoEvidenceReason.BelowMinimumScore
                : RetrievalNoEvidenceReason.ScalarBudgetExcludedAll;
        return RetrievalPolicyResult.NoEvidence(reason, rankedHits, identity!);
    }

    private static bool MatchesConfiguredPolicy(
        RetrievalPolicyConfiguration requested,
        RetrievalPolicyConfiguration configured) =>
        string.Equals(
            requested.RetrievalPolicyVersion,
            configured.RetrievalPolicyVersion,
            StringComparison.Ordinal) &&
        string.Equals(
            requested.MinimumScorePolicyVersion,
            configured.MinimumScorePolicyVersion,
            StringComparison.Ordinal) &&
        requested.MaximumResults == configured.MaximumResults &&
        requested.MinimumScore == configured.MinimumScore &&
        requested.MaximumSelectedEvidence == configured.MaximumSelectedEvidence &&
        requested.MaximumSelectedEvidenceScalars ==
            configured.MaximumSelectedEvidenceScalars &&
        requested.ExpectedEmbeddingDescriptor == configured.ExpectedEmbeddingDescriptor &&
        requested.ExpectedIndexCompatibilityKey == configured.ExpectedIndexCompatibilityKey;

    private static RetrievalPolicyIdentity? CreateIdentity(
        RetrievalPolicyRequest request,
        RetrievalPolicyConfiguration value)
    {
        var snapshot = request.ActivationSnapshot;
        var manifest = snapshot.FinalisedGenerationManifest;

        if (manifest is null)
        {
            return null;
        }

        return new RetrievalPolicyIdentity(
            snapshot.ActivationRecord.CorpusId,
            snapshot.ActivationRecord.RecordRevision,
            manifest.IndexGenerationId,
            manifest.IndexCompatibilityKey,
            request.EligibilityPolicyVersion,
            value.RetrievalPolicyVersion,
            value.MinimumScorePolicyVersion,
            value.ExpectedEmbeddingDescriptor,
            manifest.GenerationContentDigest,
            manifest.SourceBindingSetDigest,
            snapshot.ActivationRecord.ActivationBindingSetDigest,
            request.QueryVectorRepresentation,
            request.QueryVectorSha256,
            ComputePolicyManifestSha256(value));
    }

    private static RetrievalPolicyOutcome? ValidateGeneration(
        RetrievalPolicyRequest request,
        RetrievalPolicyConfiguration value)
    {
        var activation = request.ActivationSnapshot.ActivationRecord;
        var manifest = request.ActivationSnapshot.FinalisedGenerationManifest;

        if (manifest is null ||
            manifest.IndexCompatibilityKey != value.ExpectedIndexCompatibilityKey)
        {
            return RetrievalPolicyOutcome.GenerationUnavailable;
        }

        if (manifest.CorpusId != activation.CorpusId ||
            manifest.IndexGenerationId != activation.IndexGenerationId ||
            manifest.CatalogueRevision != activation.CatalogueRevision)
        {
            return RetrievalPolicyOutcome.InvalidIndexData;
        }

        try
        {
            return manifest.ActiveDocumentSetDigest == BindingDigestCanonicalizer
                    .CanonicaliseActiveDocumentSet(activation.DocumentBindings).Digest &&
                manifest.SourceBindingSetDigest == BindingDigestCanonicalizer
                    .CanonicaliseSourceBindingSet(activation.DocumentBindings).Digest &&
                activation.ActivationBindingSetDigest == BindingDigestCanonicalizer
                    .CanonicaliseActivationBindingSet(activation.DocumentBindings).Digest
                ? null
                : RetrievalPolicyOutcome.InvalidIndexData;
        }
        catch (ArgumentException)
        {
            return RetrievalPolicyOutcome.InvalidIndexData;
        }
    }

    private static bool IsValidQueryVector(
        RetrievalPolicyRequest request,
        RetrievalPolicyConfiguration value)
    {
        if (request.ObservedEmbeddingDescriptor != value.ExpectedEmbeddingDescriptor ||
            request.QueryVector.Length != value.ExpectedEmbeddingDescriptor.Dimensions)
        {
            return false;
        }

        double squaredMagnitude = 0;

        foreach (var component in request.QueryVector.Span)
        {
            if (!float.IsFinite(component))
            {
                return false;
            }

            float product = component * component;

            if (!float.IsFinite(product))
            {
                return false;
            }

            squaredMagnitude += product;

            if (!double.IsFinite(squaredMagnitude))
            {
                return false;
            }
        }

        var magnitude = Math.Sqrt(squaredMagnitude);
        return double.IsFinite(magnitude) && magnitude > 0;
    }

    private static bool TryCreateEligibleBindings(
        RetrievalPolicyRequest request,
        out Dictionary<VectorSearchBindingSelector, QueryEvidenceBinding> bindings)
    {
        bindings = [];

        if (!Enum.IsDefined(request.QuestionLanguage) ||
            !Enum.IsDefined(request.EligibilityPolicyVersion) ||
            request.DatabaseProductFilters.Any(filter => filter is null) ||
            request.DocumentFilters.Any(filter => filter is null) ||
            request.EligibleBindings.Any(binding => binding is null))
        {
            return false;
        }

        var databaseFilters = request.DatabaseProductFilters
            .Select(identifier => identifier.Value)
            .ToHashSet(StringComparer.Ordinal);
        var documentFilters = request.DocumentFilters
            .Select(identifier => identifier.Value)
            .ToHashSet(StringComparer.Ordinal);
        var expected = request.ActivationSnapshot.EvidenceBindings.Where(binding =>
                binding.IsEligible &&
                (request.EligibilityPolicyVersion == QueryContractVersion.V2 ||
                    binding.ContentLanguage.IsSupportedByV1) &&
                (databaseFilters.Count == 0 ||
                    databaseFilters.Contains(binding.Binding.DatabaseProductId.Value)) &&
                (documentFilters.Count == 0 ||
                    documentFilters.Contains(binding.Binding.DocumentId.Value)))
            .ToArray();

        if (expected.Length != request.EligibleBindings.Count)
        {
            return false;
        }

        for (var index = 0; index < expected.Length; index++)
        {
            var expectedBinding = expected[index];
            var actualBinding = request.EligibleBindings[index];
            var expectedSelector = VectorSearchBindingSelector.FromBinding(
                expectedBinding.Binding);
            var actualSelector = VectorSearchBindingSelector.FromBinding(
                actualBinding.Binding);

            if (expectedSelector != actualSelector ||
                expectedBinding.ContentLanguage != actualBinding.ContentLanguage ||
                expectedBinding.EvidenceBinding != actualBinding.EvidenceBinding ||
                !bindings.TryAdd(actualSelector, expectedBinding))
            {
                return false;
            }
        }

        return bindings.Count != 0;
    }

    private static bool TryValidateHits(
        ReadOnlyCollection<VectorSearchHit> hits,
        Dictionary<VectorSearchBindingSelector, QueryEvidenceBinding> bindings,
        CorpusId expectedCorpusId,
        IndexGenerationId expectedGenerationId,
        int maximumResults,
        out ReadOnlyCollection<RetrievalRankedHit> rankedHits,
        out ReadOnlyCollection<(VectorSearchHit Hit, QueryEvidenceBinding Binding, int Rank)>
            orderedHits,
        out RetrievalPolicyOutcome failure)
    {
        rankedHits = Array.AsReadOnly(Array.Empty<RetrievalRankedHit>());
        orderedHits = Array.AsReadOnly(
            Array.Empty<(VectorSearchHit, QueryEvidenceBinding, int)>());
        failure = RetrievalPolicyOutcome.InvalidIndexData;

        if (hits is null || hits.Count > maximumResults)
        {
            failure = RetrievalPolicyOutcome.ContractViolation;
            return false;
        }

        var materialised = hits.ToArray();
        var ordinals = new HashSet<long>();
        var ranked = new List<RetrievalRankedHit>(materialised.Length);
        var ordered = new List<(VectorSearchHit, QueryEvidenceBinding, int)>(
            materialised.Length);
        CandidateBuildId? candidateBuildId = null;
        VectorSearchHit? previous = null;

        for (var index = 0; index < materialised.Length; index++)
        {
            var hit = materialised[index];

            if (hit is null ||
                hit.CandidateBuildId is null ||
                hit.CorpusId != expectedCorpusId ||
                hit.IndexGenerationId != expectedGenerationId ||
                hit.BindingSelector is null ||
                !bindings.TryGetValue(hit.BindingSelector, out var binding) ||
                hit.ContentLanguage != binding.ContentLanguage ||
                hit.ChunkOrdinal < 0 ||
                !ordinals.Add(hit.ChunkOrdinal) ||
                hit.ChunkDigest is null ||
                string.IsNullOrWhiteSpace(hit.ChunkText) ||
                hit.Columns is null ||
                hit.PageNumber is <= 0 ||
                hit.RecordNumber is <= 0 ||
                !double.IsFinite(hit.Score) ||
                hit.Score is < -1 or > 1)
            {
                return false;
            }

            candidateBuildId ??= hit.CandidateBuildId;

            if (candidateBuildId != hit.CandidateBuildId)
            {
                return false;
            }

            if (previous is not null &&
                (previous.Score < hit.Score ||
                 previous.Score == hit.Score &&
                    previous.ChunkOrdinal >= hit.ChunkOrdinal))
            {
                failure = RetrievalPolicyOutcome.ContractViolation;
                return false;
            }

            var rank = index + 1;
            ranked.Add(new RetrievalRankedHit(
                rank,
                hit.CandidateBuildId,
                hit.BindingSelector,
                hit.ChunkOrdinal,
                hit.ChunkDigest,
                hit.Score));
            ordered.Add((hit, binding, rank));
            previous = hit;
        }

        rankedHits = ranked.AsReadOnly();
        orderedHits = ordered.AsReadOnly();
        return true;
    }

    private static ReadOnlyCollection<RetrievalSelectedEvidence> SelectEvidence(
        IReadOnlyCollection<(VectorSearchHit Hit, QueryEvidenceBinding Binding, int Rank)> hits,
        RetrievalPolicyConfiguration value)
    {
        var result = new List<RetrievalSelectedEvidence>();
        var scalars = 0;

        foreach (var item in hits.Where(item => item.Hit.Score >= value.MinimumScore))
        {
            var count = item.Hit.ChunkText.EnumerateRunes().Count();

            if (count > value.MaximumSelectedEvidenceScalars - scalars)
            {
                continue;
            }

            result.Add(new RetrievalSelectedEvidence(
                item.Rank,
                $"chunk-{item.Hit.ChunkDigest.Value}",
                item.Hit,
                item.Binding));
            scalars += count;

            if (result.Count == value.MaximumSelectedEvidence)
            {
                break;
            }
        }

        return result.AsReadOnly();
    }

    private static bool HasValidSelectedOrder(
        ReadOnlyCollection<RetrievalSelectedEvidence> selected,
        RetrievalPolicyConfiguration value)
    {
        if (selected.Count > value.MaximumSelectedEvidence ||
            selected.Sum(item => item.Hit.ChunkText.EnumerateRunes().Count()) >
                value.MaximumSelectedEvidenceScalars)
        {
            return false;
        }

        RetrievalSelectedEvidence? previous = null;

        foreach (var item in selected)
        {
            if (item.Hit.Score < value.MinimumScore ||
                previous is not null &&
                (previous.RawRank >= item.RawRank ||
                 previous.Hit.Score < item.Hit.Score ||
                 previous.Hit.Score == item.Hit.Score &&
                    previous.Hit.ChunkOrdinal >= item.Hit.ChunkOrdinal))
            {
                return false;
            }

            previous = item;
        }

        return true;
    }

    private static RetrievalPolicyOutcome MapSearchFailure(VectorSearchOutcome? outcome) =>
        outcome switch
        {
            VectorSearchOutcome.InvalidQueryVector =>
                RetrievalPolicyOutcome.InvalidQueryVector,
            VectorSearchOutcome.GenerationUnavailable =>
                RetrievalPolicyOutcome.GenerationUnavailable,
            VectorSearchOutcome.InvalidIndexData =>
                RetrievalPolicyOutcome.InvalidIndexData,
            VectorSearchOutcome.ContractViolation =>
                RetrievalPolicyOutcome.ContractViolation,
            VectorSearchOutcome.OperationCancelled =>
                RetrievalPolicyOutcome.OperationCancelled,
            _ => RetrievalPolicyOutcome.UnexpectedFailure,
        };

    private static string ComputePolicyManifestSha256(
        RetrievalPolicyConfiguration value)
    {
        var canonical = new StringBuilder();
        AppendCanonical(canonical, "domain", "rag-challenge/retrieval-policy-manifest/v1");
        AppendCanonical(canonical, "retrieval-policy", value.RetrievalPolicyVersion);
        AppendCanonical(canonical, "minimum-score-policy", value.MinimumScorePolicyVersion);
        AppendCanonical(canonical, "ranking", "Score DESC, global ChunkOrdinal ASC");
        AppendCanonical(canonical, "search", "exact-cosine");
        AppendCanonical(
            canonical,
            "query-vector-representation",
            RetrievalPolicyConfiguration.QueryVectorRepresentation);
        AppendCanonical(
            canonical,
            "maximum-results",
            value.MaximumResults.ToString(CultureInfo.InvariantCulture));
        AppendCanonical(
            canonical,
            "minimum-score-bits",
            BitConverter.DoubleToInt64Bits(value.MinimumScore).ToString(
                "x16",
                CultureInfo.InvariantCulture));
        AppendCanonical(canonical, "minimum-score-comparison", "inclusive");
        AppendCanonical(
            canonical,
            "maximum-selected-evidence",
            value.MaximumSelectedEvidence.ToString(CultureInfo.InvariantCulture));
        AppendCanonical(
            canonical,
            "maximum-selected-evidence-scalars",
            value.MaximumSelectedEvidenceScalars.ToString(CultureInfo.InvariantCulture));
        AppendCanonical(canonical, "invalid-ranking-state", "typed-fail-closed");
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical.ToString())))
            .ToLowerInvariant();
    }

    private static void AppendCanonical(StringBuilder target, string name, string value)
    {
        target.Append(name);
        target.Append(':');
        target.Append(Encoding.UTF8.GetByteCount(value).ToString(CultureInfo.InvariantCulture));
        target.Append(':');
        target.Append(value);
        target.Append('\n');
    }
}
