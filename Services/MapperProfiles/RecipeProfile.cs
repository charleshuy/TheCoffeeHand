using AutoMapper;
using Interfracture.Entities;
using Services.DTOs;

namespace Services.MapperProfiles
{
    public class RecipeProfile : Profile
    {
        public RecipeProfile()
        {
            CreateMap<Recipe, RecipeResponseDTO>()
                .ReverseMap();
            CreateMap<RecipeRequestDTO, Recipe>()
                .ReverseMap();
        }
    }
}
