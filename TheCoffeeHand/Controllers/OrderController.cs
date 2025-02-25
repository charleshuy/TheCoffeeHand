using Microsoft.AspNetCore.Mvc;
using Services.DTOs;
using Services.ServiceInterfaces;

namespace TheCoffeeHand.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequestDTO orderDTO)
        {
            if (orderDTO == null)
                return BadRequest("Invalid order data.");

            var createdOrder = await _orderService.CreateOrderAsync(orderDTO);
            return CreatedAtAction(nameof(GetOrderById), new { id = createdOrder.Id }, createdOrder);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound("Order not found.");

            return Ok(order);
        }

        [HttpGet("paginated")]
        public async Task<IActionResult> GetOrders([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber <= 0 || pageSize <= 0)
                return BadRequest("Page number and page size must be greater than zero.");

            var paginatedOrders = await _orderService.GetOrdersAsync(pageNumber, pageSize);
            return Ok(paginatedOrders);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(Guid id, [FromBody] OrderRequestDTO orderDTO)
        {
            var updatedOrder = await _orderService.UpdateOrderAsync(id, orderDTO);
            if (updatedOrder == null)
                return NotFound("Order not found.");

            return Ok(updatedOrder);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(Guid id)
        {
            await _orderService.DeleteOrderAsync(id);
            return NoContent();
        }
    }
}
