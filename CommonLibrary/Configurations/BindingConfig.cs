using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Configuration
{
    public class BindingConfig
    {
        public string? ExchangeName { get; set; }
        public string? RoutingKey { get; set; }
        public IDictionary<string, object>? Arguments { get; set; }
    }
}
