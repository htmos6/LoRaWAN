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
            var key = new byte[16] { 0x69, 0x93, 0xAB, 0x4F, 0x2A, 0xC1, 0x0F, 0x2D, 0x3A, 0x5B, 0x21, 0x8C, 0x4E, 0x97, 0xE9, 0x6C };
            var iv = new byte[16] { 0x8A, 0x57, 0x6F, 0x0C, 0x45, 0x83, 0x28, 0xE0, 0x9E, 0x41, 0x23, 0x14, 0x36, 0xD7, 0xB7, 0x55 };

            // Define the input string
            string message = "Hello World!";

            // Convert the input string to byte array using UTF-8 encoding
            byte[] input = Encoding.UTF8.GetBytes(message);

            var crypto = new AesCryptographyService();

            var encrypted = crypto.Encrypt(input, key, iv);


            var str = BitConverter.ToString(encrypted).Replace("-", "");

           
            Console.WriteLine(str);
            Console.WriteLine(Encoding.ASCII.GetString(crypto.Decrypt(encrypted, key, iv)));
            Console.WriteLine(BitConverter.ToString(crypto.CalculateMIC(encrypted, key)).Replace("-", ""));

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


/*
 * setup
 // define multi-channel sending
  lora.setChannel(MULTI);
  // set datarate
  lora.setDatarate(SF7BW125);
  if(!lora.begin())
  {
    Serial.println("Failed");
    Serial.println("Check your radio");
    while(true);
  }

  // Optional set transmit power. If not set default is +17 dBm.
  // Valid options are: -80, 1 to 17, 20 (dBm).
  // For safe operation in 20dBm: your antenna must be 3:1 VWSR or better
  // and respect the 1% duty cycle.

  // lora.setPower(17);


void loop()
{
  Serial.println("Sending LoRa Data...");
  lora.sendData(loraData, sizeof(loraData), lora.frameCounter);
  // Optionally set the Frame Port (1 to 255)
  // uint8_t framePort = 1;
  // lora.sendData(loraData, sizeof(loraData), lora.frameCounter, framePort);
  Serial.print("Frame Counter: ");Serial.println(lora.frameCounter);
  lora.frameCounter++;

  // blink LED to indicate packet sent
  digitalWrite(LED_BUILTIN, HIGH);
  delay(1000);
  digitalWrite(LED_BUILTIN, LOW);
  
  Serial.println("delaying...");
  delay(sendInterval * 1000);
}
 */