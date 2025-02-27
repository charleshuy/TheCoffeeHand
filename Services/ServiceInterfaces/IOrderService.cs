using Interfracture.PaggingItems;
using Services.DTOs;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Services.ServiceInterfaces
{
    public interface IOrderService
    {
        Task<OrderResponseDTO> CreateOrderAsync(OrderRequestDTO orderDTO);
        Task<OrderResponseDTO> GetOrderByIdAsync(Guid id);
        Task<PaginatedList<OrderResponseDTO>> GetOrdersAsync(int pageNumber, int pageSize, Guid? userId, DateTimeOffset? date);
        Task<OrderResponseDTO> UpdateOrderAsync(Guid id, OrderRequestDTO orderDTO);
        Task DeleteOrderAsync(Guid id);
    }
}
