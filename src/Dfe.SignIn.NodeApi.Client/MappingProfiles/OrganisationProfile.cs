using AutoMapper;
using Dfe.SignIn.Core.ExternalModels.Organisations;
using Dfe.SignIn.Core.InternalModels.Organisations;
using Dfe.SignIn.NodeApi.Client.Organisations;
using Dfe.SignIn.NodeApi.Client.Organisations.Models;

namespace Dfe.SignIn.NodeApi.Client.MappingProfiles;

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
                src => (src.Category.Id == OrganisationConstants.CategoryId_Establishment && src.EstablishmentType != null)
                    ? Enum.Parse(typeof(EstablishmentType), src.EstablishmentType.Id)
                    : null
            ));

        this.CreateMap<OrganisationByIdDto, OrganisationModel>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(
                src => Enum.Parse(typeof(OrganisationCategory), src.Category)
            ))
            .ForMember(dest => dest.EstablishmentType, opt => opt.MapFrom(
                src => (src.Category == OrganisationConstants.CategoryId_Establishment && src.EstablishmentType != null)
                    ? Enum.Parse(typeof(EstablishmentType), src.EstablishmentType)
                    : null
            ));
    }
}
