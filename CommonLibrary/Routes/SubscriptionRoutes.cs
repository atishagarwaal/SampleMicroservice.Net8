using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Routes
{
    public class SubscriptionRoutes
    {
        public string QueueName { get; set; }
        public string Exchange { get; set; }
        public string RoutingKey { get; set; }
        public string? DeadLetterExchange { get; set; }
        public string? DeadLetterRoutingKey { get; set; }
        public int? MessageTTL { get; set; }
        public int? MaxLength { get; set; }
        public bool? EnablePriority { get; set; }
        public int? MaxPriority { get; set; }
    }
}
