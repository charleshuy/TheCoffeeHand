

using AutoMapper;
using Interfracture.DTOs;
using Interfracture.Entities;

namespace Services.Services.MapperProfiles
{
    public class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            CreateMap<Category, CategoryResponseDTO>().ReverseMap();
            CreateMap<Category, CategoryRequestDTO>().ReverseMap();
        }
    }
}
