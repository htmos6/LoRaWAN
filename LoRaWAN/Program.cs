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
            string sslCertificate = "C:\\Users\\Legion\\projects\\LoRaWAN-Server\\LoRaWAN.pfx";
            string sslPassword = "sTrongPassW1";

            Gateway gateway = new Gateway(sslCertificate, sslPassword);

            gateway.SendFramesToServer("127.0.0.1", 8080, "Hello, LoRaWAN Community!<EOF>");
        }
    }
}
