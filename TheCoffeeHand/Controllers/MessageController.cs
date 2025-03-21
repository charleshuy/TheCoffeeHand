using Microsoft.AspNetCore.Mvc;
using Services.Services.RabbitMqServices;

namespace TheCoffeeHand.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly RabbitMqPublisher _rabbitMqPublisher;

        public MessageController(RabbitMqPublisher rabbitMqPublisher)
        {
            _rabbitMqPublisher = rabbitMqPublisher;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] string message)
        {
            await _rabbitMqPublisher.PublishAsync(message);
            return Ok("Message sent to RabbitMQ!");
        }
    }
}
