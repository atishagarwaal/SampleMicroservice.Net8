using CommonLibrary;
using CommonLibrary.Handlers.Dto;
using CommonLibrary.MessageContract;

namespace CommonLibrary.MessageContract
{
    public class OrderCreatedEvent : EventBase
    {
        /// <summary>
        /// Gets or sets the customer Id.
        /// </summary>
        public long CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the order Id.
        /// </summary>
        public long OrderId { get; set; }

        /// <summary>
        /// Gets or sets the order date.
        /// </summary>
        public DateTime OrderDate { get; set; }

        /// <summary>
        /// Gets or sets the total amount.
        /// </summary>
        public double TotalAmount { get; set; }

        /// <summary>
        /// Gets or sets the Line Items.
        /// </summary>
        public IEnumerable<LineItemDto>? LineItems { get; set; }
    }
}
