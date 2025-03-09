using Domain.Base;

namespace Domain.Entities
{
    public class Recipe : BaseEntity
    {
        public required int Quantity { get; set; }
        public required Guid IngredientId { get; set; }
        public required Guid DrinkId { get; set; }
        public virtual Drink? Drink { get; set; }
        public virtual Ingredient? Ingredient { get; set; }
    }
}
