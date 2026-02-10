using Dfe.SignIn.Base.Framework.Internal;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Entities.Organisations;
using Dfe.SignIn.Core.Public;

namespace Dfe.SignIn.Core.UseCases.Organisations;

internal static class OrganisationHelpers
{
    /// <summary>
    /// The unique ID of the "Establishment" category in DfE Sign-in.
    /// </summary>
    public const string CategoryId_Establishment = "001";

    public static Organisation OrganisationFromEntity(OrganisationEntity entity)
    {
        EstablishmentType? actualEstablishmentType = (entity.Category == CategoryId_Establishment && entity.Type != null)
            ? EnumHelpers.MapEnum<EstablishmentType>(entity.Type)
            : null;

        return new Organisation {
            Id = entity.Id,
            Status = EnumHelpers.MapEnum<OrganisationStatus>(entity.Status),
            Name = entity.Name,
            LegalName = entity.LegalName,
            Category = EnumHelpers.MapEnum<OrganisationCategory>(entity.Category),
            EstablishmentType = actualEstablishmentType,
            EstablishmentNumber = entity.EstablishmentNumber,
            Urn = entity.Urn,
            Uid = entity.Uid,
            Upin = entity.Upin,
            Ukprn = entity.Ukprn,
            ClosedOn = entity.ClosedOn,
            Address = entity.Address,
            SourceSystem = entity.SourceSystem,
            ProviderTypeName = entity.ProviderTypeName,
            ProviderTypeCode = entity.ProviderTypeCode,
            GiasProviderType = entity.GiasProviderType,
            PimsProviderType = entity.PimsProviderType,
            PimsProviderTypeCode = entity.PimsProviderTypeCode,
            PimsStatusName = entity.PimsStatusName,
            PimsStatus = entity.PimsStatus,
            GiasStatus = entity.GiasStatus,
            GiasStatusName = entity.GiasStatusName,
            MasterProviderStatusCode = entity.MasterProviderStatusCode,
            MasterProviderStatusName = entity.MasterProviderStatusName,
            OpenedOn = entity.OpenedOn,
            DistrictAdministrativeName = entity.DistrictAdministrativeName,
            DistrictAdministrativeCode = entity.DistrictAdministrativeCode,
            IsOnApar = entity.IsOnApar,
        };
    }
}
