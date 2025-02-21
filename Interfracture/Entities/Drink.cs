using Interfracture.Base;


namespace Interfracture.Entities
{
    public class Drink : BaseEntity
    {
        public string? Description { get; set; }
        public double Price { get; set; }
        public Guid? CategoryId { get; set; }
        public virtual Category? Category { get; set; }
        public string? Name { get; set; }
        public Boolean? isAvailable { get; set; }
        // Navigation properties
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public virtual ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();

    }
}
