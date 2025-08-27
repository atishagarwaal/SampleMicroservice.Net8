using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingInfrastructure
{
    public static class RabbitmqConstants
    {
        // Event types
        public const string OrderCreated = "OrderCreated";
        public const string InventoryUpdated = "InventoryUpdated";
        public const string InventoryError = "InventoryError";
        
        // Exchange and routing
        public const string DeadLetterExchange = "dlx.topic.exchange";
        public const string DeadLetterRoutingKey = "failure";
        public const string DefaultExchange = "amq.topic";
        
        // Standard header names
        public const string MessageTypeHeader = "X-Message-Type";
        public const string MessageVersionHeader = "X-Message-Version";
        public const string RoutingKeyHeader = "X-Routing-Key";
        public const string ServiceNameHeader = "X-Service-Name";
        public const string ContentTypeHeader = "Content-Type";
        public const string CorrelationIdHeader = "X-Correlation-Id";
        public const string PriorityHeader = "X-Priority";
        public const string TTLHeader = "X-TTL";
        
        // Default values
        public const string DefaultContentType = "application/json";
        public const string DefaultMessageVersion = "1.0";
        public const int DefaultPriority = 0;
        public const int DefaultTTL = 86400000; // 24 hours in milliseconds
    }
}
