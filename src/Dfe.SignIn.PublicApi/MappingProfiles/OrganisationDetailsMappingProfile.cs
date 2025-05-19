using AutoMapper;
using Dfe.SignIn.Core.ExternalModels.Organisations;
using Dfe.SignIn.Core.InternalModels.Organisations;

namespace Dfe.SignIn.PublicApi.MappingProfiles;

/// <exclude/>
public sealed class OrganisationDetailsMappingProfile : Profile
{
    /// <exclude/>
    public OrganisationDetailsMappingProfile()
    {
        this.CreateMap<OrganisationModel, OrganisationDetails>();
    }
}
