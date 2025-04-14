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
        this.CreateMap<OrganisationModel, SelectOrganisationCallbackId>()
            .ForMember(destination => destination.Type, options => options.Ignore());

        this.CreateMap<OrganisationModel, SelectOrganisationCallbackBasic>()
            .ForMember(destination => destination.Type, options => options.Ignore());

        this.CreateMap<OrganisationModel, SelectOrganisationCallbackExtended>()
            .ForMember(destination => destination.Type, options => options.Ignore());

        this.CreateMap<OrganisationModel, SelectOrganisationCallbackLegacy>()
            .ForMember(destination => destination.Type, options => options.Ignore());
    }
}
