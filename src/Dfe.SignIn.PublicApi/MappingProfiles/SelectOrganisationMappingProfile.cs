using AutoMapper;
using Dfe.SignIn.Core.Models.SelectOrganisation.Interactions;
using Dfe.SignIn.PublicApi.Endpoints.SelectOrganisation.Models;

/// <exclude/>
public sealed class SelectOrganisationMappingProfile : Profile
{
    /// <exclude/>
    public SelectOrganisationMappingProfile()
    {
        this.CreateMap<CreateSelectOrganisationSessionResponse, CreateSelectOrganisationSession_PublicApiResponse>();

        this.CreateMap<CreateSelectOrganisationSession_PublicApiRequest, CreateSelectOrganisationSessionRequest>()
            .ForMember(destination => destination.ClientId, options => options.Ignore());
    }
}
