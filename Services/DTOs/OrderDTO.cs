using Core.Constants;

namespace Services.DTOs
{
    public class OrderResponseDTO
    {
        public Guid Id { get; set; }
        public DateTimeOffset? Date { get; set; }
        public EnumOrderStatus? Status { get; set; }
        public double TotalPrice { get; set; } = 0;
        public Guid? UserId { get; set; }
        public List<OrderDetailResponselDTO>? OrderDetails { get; set; }
    }
    public class OrderRequestDTO
    {
        public DateTimeOffset? Date { get; set; }
        public EnumOrderStatus? Status { get; set; }
        public Guid? UserId { get; set; }
    }
}
