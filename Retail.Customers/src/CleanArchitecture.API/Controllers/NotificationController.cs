using Microsoft.AspNetCore.Mvc;
using Retail.Api.Customers.src.CleanArchitecture.Application.Dto;
using Retail.Api.Customers.src.CleanArchitecture.Application.Interfaces;
using Retail.Api.Customers.src.CleanArchitecture.Infrastructure.Interfaces;
using Retail.Api.Customers.src.CleanArchitecture.Application.Mappings;
using AutoMapper;

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
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationController"/> class.
        /// </summary>
        /// <param name="notificationRepository">Instance of notification repository class.</param>
        /// <param name="mapper">Instance of AutoMapper.</param>
        public NotificationController(INotificationRepository notificationRepository, IMapper mapper)
        {
            _notificationRepository = notificationRepository;
            _mapper = mapper;
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
                // Get all notifications from repository
                var notifications = await _notificationRepository.GetAllAsync();

                // Check if list is null
                if (notifications == null)
                {
                    return NotFound();
                }

                // Map to DTOs using AutoMapper
                var notificationDtos = _mapper.Map<IEnumerable<NotificationDto>>(notifications).ToList();

                // Return list
                return Ok(notificationDtos);
            }
            catch (Exception ex)
            {
                // Log the exception and return internal server error
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
                // Validate parameters
                if (id == 0)
                {
                    return BadRequest("Invalid parameter");
                }

                // Get notification by ID
                var notification = await _notificationRepository.GetByIdAsync(id);

                // Check if object is null
                if (notification == null)
                {
                    return NotFound();
                }

                // Map to DTO using AutoMapper
                var notificationDto = _mapper.Map<NotificationDto>(notification);

                // Return object
                return Ok(notificationDto);
            }
            catch (Exception ex)
            {
                // Log the exception and return internal server error
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
