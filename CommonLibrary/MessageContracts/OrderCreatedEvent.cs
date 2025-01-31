using CommonLibrary;
using CommonLibrary.MessageContract;

namespace Retail.Api.Orders.MessageContract
{
    public class OrderCreatedEvent : EventBase
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the customer Id.
        /// </summary>
        public long CustomerId { get; set; }

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
        public IList<object>? LineItems { get; set; }
    }
}
