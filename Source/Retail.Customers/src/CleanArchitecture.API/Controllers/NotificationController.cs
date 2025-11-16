using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Retail.Api.Customers.src.CleanArchitecture.Application.Dto;
using Retail.Api.Customers.src.CleanArchitecture.Application.Interfaces;
using Retail.Api.Customers.src.CleanArchitecture.Infrastructure.Interfaces;
using Retail.Api.Customers.src.CleanArchitecture.Application.Converters.Interfaces;
using Retail.Api.Customers.src.CleanArchitecture.Domain.Entities;

namespace Retail.Api.Customers.src.CleanArchitecture.API.Controllers
{
    /// <summary>
    /// Notification controller class.
    /// </summary>
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IConverter<Notification, NotificationDto> _notificationDtoConverter;
        private readonly ILogger<NotificationController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationController"/> class.
        /// </summary>
        /// <param name="notificationRepository">Instance of notification repository class.</param>
        /// <param name="notificationDtoConverter">Instance of notification DTO converter.</param>
        /// <param name="logger">Instance of logger.</param>
        public NotificationController(
            INotificationRepository notificationRepository,
            IConverter<Notification, NotificationDto> notificationDtoConverter,
            ILogger<NotificationController> logger)
        {
            _notificationRepository = notificationRepository;
            _notificationDtoConverter = notificationDtoConverter;
            _logger = logger;
        }

        /// <summary>
        /// Method to return list of all notifications.
        /// </summary>
        /// <returns>List of notifications.</returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                _logger.LogInformation("Retrieving all notifications");
                
                // Get all notifications from repository
                var notifications = await _notificationRepository.GetAllAsync();

                // Check if list is null
                if (notifications == null)
                {
                    _logger.LogWarning("Notifications list is null");
                    return NotFound();
                }

                // Map to DTOs using converter
                var notificationDtos = notifications
                    .Where(notification => notification != null)
                    .Select(notification => _notificationDtoConverter.Convert(notification))
                    .ToList();

                _logger.LogInformation("Retrieved {Count} notifications", notificationDtos.Count);
                
                // Return list
                return Ok(notificationDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notifications");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Method to fetch notification record based on Id.
        /// </summary>
        /// <param name="id">Notification Id.</param>
        /// <returns>Notification object.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(long id)
        {
            try
            {
                _logger.LogInformation("Retrieving notification by ID. NotificationId: {NotificationId}", id);
                
                // Validate parameters
                if (id == 0)
                {
                    _logger.LogWarning("Invalid notification ID provided. NotificationId: {NotificationId}", id);
                    return BadRequest("Invalid parameter");
                }

                // Get notification by ID
                var notification = await _notificationRepository.GetByIdAsync(id);

                // Check if object is null
                if (notification == null)
                {
                    _logger.LogWarning("Notification not found. NotificationId: {NotificationId}", id);
                    return NotFound();
                }

                // Map to DTO using converter
                var notificationDto = _notificationDtoConverter.Convert(notification);

                _logger.LogInformation("Notification retrieved successfully. NotificationId: {NotificationId}, OrderId: {OrderId}, CustomerId: {CustomerId}",
                    id, notificationDto.OrderId, notificationDto.CustomerId);
                
                // Return object
                return Ok(notificationDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notification. NotificationId: {NotificationId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
