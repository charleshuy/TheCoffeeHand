using Microsoft.AspNetCore.Mvc;
using Services.DTOs;
using Services.ServiceInterfaces;

namespace TheCoffeeHand.Controllers
{
    [Route("api/order-details")]
    [ApiController]
    public class OrderDetailController : ControllerBase
    {
        private readonly IOrderDetailService _orderDetailService;

        public OrderDetailController(IOrderDetailService orderDetailService)
        {
            _orderDetailService = orderDetailService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrderDetail([FromBody] OrderDetailRequestDTO dto)
        {
            var createdOrderDetail = await _orderDetailService.CreateOrderDetailAsync(dto);
            return CreatedAtAction(nameof(GetOrderDetailById), new { id = createdOrderDetail.Id }, createdOrderDetail);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderDetailById(Guid id)
        {
            var orderDetail = await _orderDetailService.GetOrderDetailByIdAsync(id);
            if (orderDetail == null)
                return NotFound("Order detail not found.");

            return Ok(orderDetail);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrderDetails()
        {
            var orderDetails = await _orderDetailService.GetOrderDetailsAsync();
            return Ok(orderDetails);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaginatedOrderDetails([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var paginatedOrderDetails = await _orderDetailService.GetOrderDetailsAsync(pageNumber, pageSize);
            return Ok(paginatedOrderDetails);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrderDetail(Guid id, [FromBody] OrderDetailRequestDTO dto)
        {
            var updatedOrderDetail = await _orderDetailService.UpdateOrderDetailAsync(id, dto);
            return Ok(updatedOrderDetail);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrderDetail(Guid id)
        {
            await _orderDetailService.DeleteOrderDetailAsync(id);
            return NoContent();
        }
    }
}
