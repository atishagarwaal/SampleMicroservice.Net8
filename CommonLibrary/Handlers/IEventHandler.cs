using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Handlers
{
    public interface IEventHandler<T>
    {
        Task HandleAsync(T eventMessage);
    }
}
