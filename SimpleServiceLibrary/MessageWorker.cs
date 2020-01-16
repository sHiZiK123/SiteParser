using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleServiceLibrary
{
    public class MessageWorker : IMessageWorker
    {
        public void PrintMessage(string data)
        {
            Console.WriteLine(data);
        }
    }
}
