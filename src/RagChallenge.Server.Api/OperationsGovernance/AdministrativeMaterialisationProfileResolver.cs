// Purpose: Selects exactly one explicitly named administrative materialisation profile while leaving every capability unavailable when no profile is configured.
using Microsoft.Extensions.Configuration;

using RagChallenge.Infrastructure.Persistence;

namespace RagChallenge.Server.Api.OperationsGovernance;

internal static class AdministrativeMaterialisationProfileResolver
{
    internal static AdministrativeMaterialisationPorts? Resolve(
        IConfiguration configuration,
        SqliteStoreOptions storeOptions)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(storeOptions);
        var profile = configuration[
            SyntheticAdministrativeMaterialisationProfile.ProfileKey];

        if (string.IsNullOrWhiteSpace(profile))
        {
            return null;
        }

        return profile switch
        {
            SyntheticAdministrativeMaterialisationProfile.ProfileName =>
                SyntheticAdministrativeMaterialisationProfile.Resolve(configuration),
            ProductAdministrativeMaterialisationProfile.ProfileName =>
                ProductAdministrativeMaterialisationProfile.Resolve(
                    configuration,
                    storeOptions),
            _ => throw new ArgumentException(
                "The administrative materialisation profile is not supported.",
                nameof(configuration)),
        };
    }
}
