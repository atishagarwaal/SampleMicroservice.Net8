using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.MessageContract
{
    public interface IMessage
    {
        Guid MessageId { get; }
        DateTime Timestamp { get; }
        string MessageType { get; }
        string MessageVersion { get; }
        string RoutingKey { get; }
        IDictionary<string, object> Headers { get; }
    }
}
