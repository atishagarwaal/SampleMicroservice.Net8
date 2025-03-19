namespace Retail.Api.Customers.src.CleanArchitecture.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;

    public class Notification
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        [Key]
        public long NotificationId { get; set; }

        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        [Required]
        public long CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        [Required]
        public long OrderId { get; set; }

        /// <summary>
        /// Gets or sets the order date.
        /// </summary>
        [Required]
        public DateTime OrderDate { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        [Required]
        public string Message { get; set; }

    }
}
