using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopologyManager.Configuration
{
    public class BindingConfig
    {
        public string? QueueName { get; set; }
        public string? ExchangeName { get; set; }
        public string? RoutingKey { get; set; }
        public IDictionary<string, object>? Arguments { get; set; }
    }
}
