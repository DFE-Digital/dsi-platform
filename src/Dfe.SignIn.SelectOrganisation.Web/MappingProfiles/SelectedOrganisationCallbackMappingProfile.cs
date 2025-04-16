using AutoMapper;
using Dfe.SignIn.Core.ExternalModels.SelectOrganisation;
using Dfe.SignIn.Core.InternalModels.Organisations;

namespace Dfe.SignIn.SelectOrganisation.Web.MappingProfiles;

/// <exclude/>
public sealed class SelectedOrganisationCallbackMappingProfile : Profile
{
    /// <exclude/>
    public SelectedOrganisationCallbackMappingProfile()
    {
        this.CreateMap<OrganisationModel, SelectedOrganisation>();

        this.CreateMap<OrganisationModel, SelectedOrganisationBasic>();

        this.CreateMap<OrganisationModel, SelectedOrganisationExtended>();

        this.CreateMap<OrganisationModel, SelectedOrganisationLegacy>();
    }
}
