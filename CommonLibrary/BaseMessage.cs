using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary
{
    public class BaseMessage
    {
        public Guid CoorelationId { get; set; }
        public string ServiceName { get; set; }
        public DateTime CreatedDate { get; set; }

    }
}
