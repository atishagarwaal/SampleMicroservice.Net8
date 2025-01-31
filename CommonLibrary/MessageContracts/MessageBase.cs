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
    }
}
