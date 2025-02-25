namespace Services.DTOs
{
    public class DrinkResponseDTO
    {
        public Guid? Id { get; set; }
        public string? Description { get; set; }
        public double Price { get; set; }
        public Guid? CategoryId { get; set; }
        public virtual CategoryResponseDTO? Category { get; set; }
        public string? Name { get; set; }
        public Boolean? isAvailable { get; set; }
    }
    public class DrinkRequestDTO
    {
        public string? Description { get; set; }
        public double Price { get; set; }
        public Guid? CategoryId { get; set; }
        public string? Name { get; set; }
        public Boolean? isAvailable { get; set; }
    }
}
