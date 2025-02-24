using AutoMapper;
using Interfracture.Entities;
using Services.DTOs;


namespace Services.Services.MapperProfiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<ApplicationUser, UserDTO>()
                .ForMember(dest => dest.Roles, opt => opt.Ignore()).ReverseMap();
        }
    }
}
