using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingLibrary.Interface
{
    public interface IMessagePublisher
    {
        Task PublishAsync<T>(T message, string exchangeName, string routingKey);
    }
}
