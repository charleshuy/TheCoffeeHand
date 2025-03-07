using Domain.Base;

namespace Domain.Entities
{
    public class Ingredient : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; } = 0;
        public double Price { get; set; } = 0;
        // Navigation properties
        public virtual ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
    }
}
