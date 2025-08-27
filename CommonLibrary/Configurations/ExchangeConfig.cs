using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopologyManager.Arguments;

namespace TopologyManager.Configuration
{
    public class ExchangeConfig
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool Durable { get; set; }
        public bool AutoDelete { get; set; }
        public bool Internal { get; set; }
        public IDictionary<string, object> Arguments { get; set; } = new ExchangeArguments();
        
        // Enhanced properties for priority and TTL support
        public bool EnablePriority { get; set; } = false;
        public int MaxPriority { get; set; } = 10;
        public int MessageTTL { get; set; } = 86400000; // 24 hours in milliseconds
    }
}
