using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.MessageContract
{
    public abstract class CommandBase : MessageBase
    {
        public string CommandType => GetType().Name;
    }
}
