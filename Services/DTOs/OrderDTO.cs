﻿using Core.Constants;
using System.Text.Json.Serialization;

namespace Services.DTOs
{
    public class OrderResponseDTO
    {
        public Guid Id { get; set; }
        public DateTimeOffset? Date { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public EnumOrderStatus? Status { get; set; }
        public double TotalPrice { get; set; } = 0;
        public Guid? UserId { get; set; }
        public List<OrderDetailResponselDTO>? OrderDetails { get; set; }
    }
    public class OrderRequestDTO
    {
        public EnumOrderStatus? Status { get; set; }
        public Guid? UserId { get; set; }
    }
}
