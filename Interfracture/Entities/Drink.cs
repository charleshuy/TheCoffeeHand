using Interfracture.Base;


namespace Interfracture.Entities
{
    public class Drink : BaseEntity
    {
        public string? Description { get; set; }
        public double Price { get; set; }
        public string? CategoryId { get; set; }
        public virtual Category? Category { get; set; }
        public string? Name { get; set; }
        public Boolean? isAvailable { get; set; }
    }
}
