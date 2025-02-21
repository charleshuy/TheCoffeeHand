using Interfracture.Base;

namespace Interfracture.Entities
{
    public class Category : BaseEntity
    {
        public string? Name { get; set; }
        public virtual ICollection<Drink> Drinks { get; set; } = new List<Drink>();
    }
}
