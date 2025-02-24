

namespace Services.DTOs
{
    public class IngredientResponseDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; } = 0;
        public double Price { get; set; } = 0;
    }
    
    public class IngredientRequestDTO
    {
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; } = 0;
        public double Price { get; set; } = 0;
    }
}
