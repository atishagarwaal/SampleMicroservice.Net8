using MessagingLibrary.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingLibrary.Service
{
    public class RabbitMqMessageSubscriber : IMessageSubscriber
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMqMessageSubscriber(IConnection connection)
        {
            _connection = connection;
            _channel = _connection.CreateModel();
        }

        public async Task SubscribeAsync<T>(string queueName, string contractType, Func<T, Task> handler)
        {
            _channel.QueueDeclare(queueName, true, false, false, null);
            _channel.QueueBind(queueName, "Symbotic.SRE", contractType);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(body));

                await handler(message);
            };

            _channel.BasicConsume(queueName, true, consumer);

            await Task.CompletedTask;
        }
    }
}
