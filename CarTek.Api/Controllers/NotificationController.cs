using CarTek.Api.Model;
using CarTek.Api.Model.Dto;
using CarTek.Api.Model.Response;
using CarTek.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace CarTek.Api.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("getnotifications")]
        public IActionResult GetUserNotifications(bool isDriver, long userId, int pageNumber, int pageSize)
        {
            var result = _notificationService.GetUserNotifications(isDriver, userId, pageNumber, pageSize);

            return Ok(new PagedResult<Notification>()
            {
                TotalNumber = result.Item1,
                List = result.Item2.ToList(),
            });
        }
    }
}
