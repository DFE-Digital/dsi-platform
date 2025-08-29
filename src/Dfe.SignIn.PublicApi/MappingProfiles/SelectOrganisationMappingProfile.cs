using AutoMapper;
using Dfe.SignIn.Core.InternalModels.SelectOrganisation.Interactions;
using Dfe.SignIn.PublicApi.Client.SelectOrganisation;

namespace Dfe.SignIn.PublicApi.MappingProfiles;

/// <exclude/>
public sealed class SelectOrganisationMappingProfile : Profile
{
    /// <exclude/>
    public SelectOrganisationMappingProfile()
    {
        this.CreateMap<CreateSelectOrganisationSessionResponse, CreateSelectOrganisationSessionApiResponse>();

        this.CreateMap<CreateSelectOrganisationSessionApiRequestBody, CreateSelectOrganisationSessionRequest>()
            .ForMember(destination => destination.ClientId, options => options.Ignore());
    }
}
