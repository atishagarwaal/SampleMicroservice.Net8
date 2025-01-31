using MessagingLibrary.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingLibrary.Service
{
    public class RabbitMqMessagePublisher : IMessagePublisher
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMqMessagePublisher(IConnection connection)
        {
            _connection = connection;
            _channel = _connection.CreateModel();
        }

        public async Task PublishAsync<T>(T message, string exchangeName, string routingKey)
        {
            var messageBody = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

            _channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, true);

            _channel.BasicPublish(exchangeName, routingKey, null, messageBody);

            await Task.CompletedTask;
        }
    }
}
