﻿using AutoMapper;
using Interfracture.Entities;
using Services.DTOs;

namespace Services.MapperProfiles
{
    public class IngredientProfile : Profile
    {
        public IngredientProfile()
        {
            CreateMap<Ingredient, IngredientResponseDTO>().ReverseMap();
            CreateMap<Ingredient, IngredientRequestDTO>().ReverseMap();
        }
    }
}
