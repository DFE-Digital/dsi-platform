using AutoMapper;
using Dfe.SignIn.Core.Models.Organisations;
using Dfe.SignIn.Core.PublicModels.SelectOrganisation;

/// <exclude/>
public sealed class SelectedOrganisationCallbackMappingProfile : Profile
{
    /// <exclude/>
    public SelectedOrganisationCallbackMappingProfile()
    {
        this.CreateMap<OrganisationModel, SelectOrganisationCallbackId>();
        this.CreateMap<OrganisationModel, SelectOrganisationCallbackBasic>();
        this.CreateMap<OrganisationModel, SelectOrganisationCallbackExtended>();
        this.CreateMap<OrganisationModel, SelectOrganisationCallbackLegacy>();
    }
}
