using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopologyManager.Arguments
{
    public class ExchangeArguments : Dictionary<string, object>
    {
        public string? AlternateExchange
        {
            get => ContainsKey("alternate-exchange") ? this["alternate-exchange"].ToString() : string.Empty;
            set => this["alternate-exchange"] = value?.ToString();
        }
    }
}
