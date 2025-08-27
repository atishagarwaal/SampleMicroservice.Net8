using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.MessageContract
{
    public abstract class MessageBase : IMessage
    {
        public Guid MessageId { get; private set; } = Guid.NewGuid();
        public DateTime Timestamp { get; private set; } = DateTime.UtcNow;
        public string ServiceName { get; set; }
        
        // Enhanced properties
        public Guid CorrelationId { get; set; } = Guid.NewGuid();
        public int Priority { get; set; } = 0;
        public int TTL { get; set; } = 86400000; // 24 hours in milliseconds
        public string? BusinessContext { get; set; }
        
        // Enhanced properties
        public virtual string MessageType => GetType().Name;
        public virtual string MessageVersion => "1.0";
        public virtual string RoutingKey => $"{MessageType}.{BusinessContext ?? "default"}";
        public virtual IDictionary<string, object> Headers => new Dictionary<string, object>
        {
            ["X-Message-Type"] = MessageType,
            ["X-Message-Version"] = MessageVersion,
            ["X-Routing-Key"] = RoutingKey,
            ["Content-Type"] = "application/json",
            ["X-Service-Name"] = ServiceName ?? "Unknown",
            ["X-Correlation-Id"] = CorrelationId.ToString(),
            ["X-Priority"] = Priority.ToString(),
            ["X-TTL"] = TTL.ToString(),
            ["X-Business-Context"] = BusinessContext ?? "default",
            ["X-Timestamp"] = Timestamp.ToString("O")
        };
    }
}
