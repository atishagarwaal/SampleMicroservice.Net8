using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CommonLibrary.MessageContract
{
    /// <summary>
    /// Base class for all messages in the system.
    /// </summary>
    public abstract class MessageBase : IMessage
    {
        private readonly ILogger<MessageBase>? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBase"/> class.
        /// </summary>
        /// <param name="logger">Optional logger instance.</param>
        protected MessageBase(ILogger<MessageBase>? logger = null)
        {
            _logger = logger;
            _logger?.LogDebug("Message created. MessageType={MessageType}, MessageId={MessageId}", 
                GetType().Name, MessageId);
        }

        /// <summary>
        /// Gets the unique identifier for this message.
        /// </summary>
        public Guid MessageId { get; private set; } = Guid.NewGuid();

        /// <summary>
        /// Gets the timestamp when this message was created.
        /// </summary>
        public DateTime Timestamp { get; private set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the name of the service that created this message.
        /// </summary>
        public string ServiceName { get; set; }
        
        /// <summary>
        /// Gets or sets the correlation identifier for distributed tracing.
        /// </summary>
        public Guid CorrelationId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the priority of this message.
        /// </summary>
        public int Priority { get; set; } = 0;

        /// <summary>
        /// Gets or sets the time-to-live for this message in milliseconds.
        /// </summary>
        public int TTL { get; set; } = 86400000; // 24 hours in milliseconds

        /// <summary>
        /// Gets or sets the business context for this message.
        /// </summary>
        public string? BusinessContext { get; set; }
        
        /// <summary>
        /// Gets the message type name.
        /// </summary>
        public virtual string MessageType => GetType().Name;

        /// <summary>
        /// Gets the message version.
        /// </summary>
        public virtual string MessageVersion => "1.0";

        /// <summary>
        /// Gets the routing key for this message.
        /// </summary>
        public virtual string RoutingKey
        {
            get
            {
                var routingKey = $"{MessageType}.{BusinessContext ?? "default"}";
                _logger?.LogDebug("Routing key generated. MessageType={MessageType}, BusinessContext={BusinessContext}, RoutingKey={RoutingKey}", 
                    MessageType, BusinessContext, routingKey);
                return routingKey;
            }
        }

        /// <summary>
        /// Gets the headers for this message.
        /// </summary>
        public virtual IDictionary<string, object> Headers
        {
            get
            {
                try
                {
                    var headers = new Dictionary<string, object>
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

                    _logger?.LogDebug("Message headers generated. MessageType={MessageType}, HeaderCount={HeaderCount}", 
                        MessageType, headers.Count);
                    return headers;
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Failed to generate message headers. MessageType={MessageType}", MessageType);
                    throw;
                }
            }
        }
    }
}
