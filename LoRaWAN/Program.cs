using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace LoRaWAN
{
    internal class Program
    {
        static void Main()
        {
            
            byte[] key = new byte[16] { 0x69, 0x93, 0xAB, 0x4F, 0x2A, 0xC1, 0x0F, 0x2D, 0x3A, 0x5B, 0x21, 0x8C, 0x4E, 0x97, 0xE9, 0x6C };
            byte[] iv = new byte[16] { 0x8A, 0x57, 0x6F, 0x0C, 0x45, 0x83, 0x28, 0xE0, 0x9E, 0x41, 0x23, 0x14, 0x36, 0xD7, 0xB7, 0x55 };

            /// <summary>
            /// Initializes a new LoRa session with the provided keys and device address.
            /// </summary>
            /// <remarks>
            /// Initializes a new LoRa session with the given network session key (NwkSKey), application session key (AppSKey),
            /// and device address (DevAddr).
            /// </remarks>
            sLoRaSession sessionData = new sLoRaSession()
            {
                NwkSKey = key,                                     // Network session key
                AppSKey = iv,                                      // Application session key
                DevAddr = new byte[4] { 0x7F, 0x00, 0x00, 0x01 }   // Device address 127.0.0.1
            };

            /// <summary>
            /// Initializes LoRa settings for the device.
            /// </summary>
            /// <remarks>
            /// Initializes LoRa settings including the device class (MoteClass), RX data rate (DatarateRx), RX channel (ChannelRx),
            /// TX data rate (DatarateTx), TX channel (ChannelTx), confirmation status (Confirm), and channel hopping behavior (ChannelHopping).
            /// </remarks>
            sSettings LoRaSettings = new sSettings()
            {
                Mport = 123,
                MoteClass = 0x00,           // Device class: 0x00 for Type A, 0x01 for Type C
                DatarateRx = 0x03,          // RX data rate: SF9 BW 125 kHz
                ChannelRx = 0x08,           // RX channel: Receiver Channel
                DatarateTx = 0x00,          // TX data rate: SF12 BW 125 kHz
                ChannelTx = 0x00,           // TX channel: Channel0
                Confirm = 0x00,             // Confirmation status: 0x00 for unconfirmed, 0x01 for confirmed
                ChannelHopping = 0x00       // Channel hopping behavior: 0x00 for no channel hopping, 0x01 for channel hopping
            };

            /// <summary>
            /// Initializes transmit data buffer.
            /// </summary>
            /// <remarks>
            /// Initializes transmit data buffer with an empty byte array and zero counter.
            /// </remarks>
            sBuffer TxData = new sBuffer()
            {
                Data = Encoding.UTF8.GetBytes("Hello World!"),    // Transmit data
                Counter = 12                // Counter
            };

            /// <summary>
            /// Initializes receive data buffer.
            /// </summary>
            /// <remarks>
            /// Initializes receive data buffer with an empty byte array and zero counter.
            /// </remarks>
            sBuffer RxData = new sBuffer()
            {
                Data = new byte[] { 0x00 },    // Receive data
                Counter = 0x00                 // Counter
            };

            /// <summary>
            /// Initializes a new LoRa message.
            /// </summary>
            /// <remarks>
            /// Initializes a new LoRa message with the specified direction.
            /// </remarks>
            sLoRaMessage RxMessage = new sLoRaMessage()
            {
                Direction = 0x01    // Direction: 0x01 for uplink, 0x02 for downlink
            };

            LoRaWAN loRaWAN = new LoRaWAN(TxData, RxData, RxMessage, sessionData, new sLoRaOTAA(), LoRaSettings, RFM_COMMAND.NO_RFM_COMMAND, MESSAGE_TYPES.MSG_UP);
            loRaWAN.rfm95.Init();
            loRaWAN.Cycle();
            loRaWAN.SendData();
            Console.ReadKey();

            /*
            // Define the input string
            string message = "Hello World!";


            // Convert the input string to byte array using UTF-8 encoding
            byte[] input = Encoding.UTF8.GetBytes("Hello World!<EOF>");

            var crypto = new AesCryptographyService();

            var encrypted = crypto.Encrypt(input, key, iv);


            var str = BitConverter.ToString(encrypted).Replace("-", "");

         
            Console.WriteLine(str);
            Console.WriteLine(Encoding.ASCII.GetString(crypto.Decrypt(encrypted, key, iv)));
            Console.WriteLine(BitConverter.ToString(crypto.CalculateMIC(encrypted, key)).Replace("-", ""));
  */
            RFM95 rfm95 = new RFM95();

            rfm95.SwitchMode((byte)RFM_MODES.RFM_MODE_SLEEP);


            Console.ReadKey();

            /*
            string sslCertificate = "C:\\Users\\Legion\\projects\\LoRaWAN-Server\\LoRaWAN.pfx";
            string sslPassword = "sTrongPassW1";

            Gateway gateway = new Gateway(sslCertificate, sslPassword);

            gateway.SendFramesToServer("127.0.0.1", 8080, "Hello, LoRaWAN Community!<EOF>");
            */

        }
    }
}