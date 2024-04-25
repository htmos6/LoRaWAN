using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LoRaWAN
{
    internal class Program
    {
        static void Main()
        {
            Gateway gateway = new Gateway();

            gateway.SendFramesToServer("127.0.0.1", 8080, "Hello, LoRaWAN Community!");
        }
    }
}
