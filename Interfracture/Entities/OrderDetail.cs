using Interfracture.Base;


namespace Interfracture.Entities
{
    public class OrderDetail : BaseEntity
    {
        public int Total { get; set; } = 0; //Quantity
        public string Note { get; set; } = string.Empty;
        public string? OrderId { get; set; } 
        public virtual Order? Order { get; set; }
        public string? DrinkId { get; set; }
        public virtual Drink? Drink { get; set; }
    }
}
