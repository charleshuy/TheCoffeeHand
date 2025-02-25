using AutoMapper;
using Services.DTOs;

namespace Services.MapperProfiles
{
    public class DrinkProfile : Profile
    {
        public DrinkProfile()
        {
            CreateMap<Interfracture.Entities.Drink, DrinkResponseDTO>()
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
                .ReverseMap();
            CreateMap<DrinkRequestDTO, Interfracture.Entities.Drink>()
               .ReverseMap();
        }
    }
}
