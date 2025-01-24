using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopologyManager.Configuration
{
    public class RabbitMQConfig
    {
        public string HostName { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public List<ExchangeConfig> Exchanges { get; set; }
        public List<QueueConfig> Queues { get; set; }
        public List<BindingConfig> Bindings { get; set; }
    }
}
