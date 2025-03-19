using AutoMapper;
using Dfe.SignIn.Core.Models.Organisations;
using Dfe.SignIn.Core.PublicModels.Organisations;
using Dfe.SignIn.NodeApiClient.Organisations.Models;

namespace Dfe.SignIn.NodeApiClient.MappingProfiles;

internal class OrganisationProfile : Profile
{
    public OrganisationProfile()
    {
        this.CreateMap<UserOrganisationDto, OrganisationModel>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(scr => scr.Status.Id))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(
                src => Enum.Parse(typeof(OrganisationCategory), src.Category.Id)
            ))
            .ForMember(dest => dest.EstablishmentType, opt => opt.MapFrom(
                src => (src.Category.Id == "001" && src.EstablishmentType != null)
                    ? Enum.Parse(typeof(EstablishmentType), src.EstablishmentType.Id)
                    : null
            ));

        this.CreateMap<OrganisationByIdDto, OrganisationModel>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(
                src => Enum.Parse(typeof(OrganisationCategory), src.Category)
            ))
            .ForMember(dest => dest.EstablishmentType, opt => opt.MapFrom(
                src => (src.Category == "001" && src.EstablishmentType != null)
                    ? Enum.Parse(typeof(EstablishmentType), src.EstablishmentType)
                    : null
            ));
    }
}
