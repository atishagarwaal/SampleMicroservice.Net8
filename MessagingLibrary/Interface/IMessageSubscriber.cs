using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingLibrary.Interface
{
    public interface IMessageSubscriber
    {
        Task SubscribeAsync<T>(string queueName, string contractType, Func<T, Task> handler);
    }
}
