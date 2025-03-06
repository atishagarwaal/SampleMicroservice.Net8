using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingInfrastructure
{
    public static class RabbitmqConstants
    {
        public const string OrderCreated = "OrderCreated";
        public const string InventoryUpdated = "InventoryUpdated";
        public const string InventoryError = "InventoryError";
    }
}
