using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLibrary.Arguments;

namespace CommonLibrary.Configuration
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
        public List<BindingConfig> Bindings { get; set; }
        
        // Enhanced properties for priority, TTL, and max length support
        public bool EnablePriority { get; set; } = false;
        public int MaxPriority { get; set; } = 10;
        public int MessageTTL { get; set; } = 86400000; // 24 hours in milliseconds
        public int MaxLength { get; set; } = 1000;
    }
}
