﻿namespace Services.DTOs
{
    public class DrinkResponseDTO
    {
        public Guid? Id { get; set; }
        public string? Description { get; set; }
        public double Price { get; set; }
        public string? ImageUrl { get; set; }
        public Guid? CategoryId { get; set; }
        public CategoryResponseDTO? Category { get; set; }
        public List<RecipeResponseDTO>? Recipe { get; set; }
        public string? Name { get; set; }
        public Boolean? isAvailable { get; set; }
    }
    public class DrinkRequestDTO
    {
        public string? Description { get; set; }
        public double Price { get; set; }
        public string? ImageUrl { get; set; }
        public Guid? CategoryId { get; set; }
        public string? Name { get; set; }
        public Boolean? isAvailable { get; set; }
    }
}
