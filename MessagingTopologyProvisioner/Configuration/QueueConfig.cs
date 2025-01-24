using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopologyManager.Arguments;

namespace TopologyManager.Configuration
{
    public class QueueConfig
    {
        public string Name { get; set; }
        public string VirtualHost { get; set; }
        public string Type { get; set; }
        public bool Exclusive { get; set; }
        public bool Durable { get; set; }
        public bool AutoDelete { get; set; }
        public IDictionary<string, object> Arguments { get; set; } = new QueueArguments();
    }
}
