namespace Services.DTOs
{
    public class OrderDetailResponselDTO
    {
        public Guid Id { get; set; }
        public int Total { get; set; } = 0;
        public string Note { get; set; } = string.Empty;
        public Guid? OrderId { get; set; }
        public Guid? DrinkId { get; set; }
    }
    public class OrderDetailRequestDTO
    {
        public int Total { get; set; } = 0;
        public string Note { get; set; } = string.Empty;
        public Guid? OrderId { get; set; }
        public Guid? DrinkId { get; set; }
    }
}
