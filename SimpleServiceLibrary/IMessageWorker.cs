using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleServiceLibrary
{
    public interface IMessageWorker
    {
        void PrintMessage(string data);
    }
}
