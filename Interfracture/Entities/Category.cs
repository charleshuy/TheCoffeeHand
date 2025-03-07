using Domain.Base;

namespace Domain.Entities
{
    public class Category : BaseEntity
    {
        public string? Name { get; set; }
        public virtual ICollection<Drink> Drinks { get; set; } = new List<Drink>();
    }
}
