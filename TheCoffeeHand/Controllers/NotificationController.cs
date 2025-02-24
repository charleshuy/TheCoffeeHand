using Microsoft.AspNetCore.Mvc;
using Services.DTOs;
using Services.ServiceInterfaces;
using Services.Services;

namespace TheCoffeeHand.Controllers
{
    [Route("api/notifications")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly IFCMService _fcmService;

        public NotificationController(FCMService fcmService)
        {
            _fcmService = fcmService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] NotificationRequestDTO request)
        {
            bool success = await _fcmService.SendNotificationAsync(request.DeviceToken, request.Title, request.Body);
            if (success)
                return Ok(new { message = "Notification sent successfully!" });

            return BadRequest(new { message = "Failed to send notification." });
        }
    }

    
}
