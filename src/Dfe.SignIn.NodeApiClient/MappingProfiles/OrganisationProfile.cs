using AutoMapper;
using Dfe.SignIn.Core.Models.Organisations;
using Dfe.SignIn.NodeApiClient.Organisations.Models;

namespace Dfe.SignIn.NodeApiClient.MappingProfiles;

internal class OrganisationProfile : Profile
{
    public OrganisationProfile()
    {
        this.CreateMap<UserOrganisationDto, OrganisationModel>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(scr => scr.Status.Id))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category == null ? null : src.Category.Id));

        this.CreateMap<OrganisationByIdDto, OrganisationModel>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(scr => scr.Status))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category));
    }
}
