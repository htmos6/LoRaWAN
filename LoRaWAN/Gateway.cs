using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LoRaWAN
{
    public class Gateway
    {
        TcpClient client;
        NetworkStream stream;


        /// <summary>
        /// Connects to the server.
        /// </summary>
        /// <param name="ipAddress">The IP address of the server.</param>
        /// <param name="port">The port number to connect to on the server.</param>
        /// <returns>True if connection succeeds, otherwise false.</returns>
        private bool ConnectToServer(string ipAddress, int port)
        {
            try
            {
                // Create a new TcpClient instance to establish a connection
                client = new TcpClient();

                // Connect to the server using the specified IP address and port number
                client.Connect(ipAddress, port);

                // Get the network stream associated with the TcpClient for communication through Stream instance
                stream = client.GetStream();

                // Print a message indicating successful connection along with the server's endpoint
                Console.WriteLine("Connected to server " + client.Client.RemoteEndPoint);

                // Return true to indicate successful connection
                return true;
            }
            catch (Exception ex)
            {
                // Print an error message if connection attempt fails and return false
                Console.WriteLine("Error connecting to server: " + ex.Message);

                return false;
            }
        }


        /// <summary>
        /// Sends a message to the server.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>True if message sent successfully, otherwise false.</returns>
        private bool SendMessage(string message)
        {
            try
            {
                // Convert the message string into a byte array using ASCII encoding
                byte[] buffer = Encoding.ASCII.GetBytes(message);

                // Write the byte array (message) to the network stream
                stream.Write(buffer, 0, buffer.Length);

                // Print a message indicating successful sending of the message
                Console.WriteLine("Sent message: " + message);

                // Return true to indicate successful message transmission
                return true;
            }
            catch (Exception ex)
            {
                // Print an error message if sending the message fails and return false
                Console.WriteLine("Error sending message: " + ex.Message);

                return false;
            }
        }


        /// <summary>
        /// Receives a response from the server.
        /// </summary>
        /// <returns>The received response as a string.</returns>
        private string ReceiveResponse()
        {
            try
            {
                // Create a byte array to store the received data
                byte[] buffer = new byte[1024];

                // Read data from the network stream into the buffer and store the number of bytes read
                int bytesRead = stream.Read(buffer, 0, buffer.Length);

                // Convert the received byte array to a string using ASCII encoding
                string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                // Print a message indicating the received response
                Console.WriteLine("Received response: " + response);

                // Return the received response
                return response;
            }
            catch (Exception ex)
            {
                // Print an error message if receiving the response fails and return null
                Console.WriteLine("Error receiving response: " + ex.Message);

                return null;
            }
        }


        /// <summary>
        /// Disconnects from the server.
        /// </summary>
        private void DisconnectFromServer()
        {
            try
            {
                // Close the network stream
                stream.Close();

                // Close the TcpClient
                client.Close();

                // Print a message indicating successful disconnection
                Console.WriteLine("Disconnected from server.");
            }
            catch (Exception ex)
            {
                // Print an error message if disconnection fails
                Console.WriteLine("Error disconnecting from server: " + ex.Message);
            }
        }


        /// <summary>
        /// Connects to the server, sends a message, receives a response, and then disconnects.
        /// </summary>
        /// <param name="ipAddress">The IP address of the server.</param>
        /// <param name="port">The port number to connect to on the server.</param>
        /// <param name="message">The message to send to the server.</param>
        public void SendFramesToServer(string ipAddress, int port, string message)
        {
            // Connect to the server
            if (this.ConnectToServer(ipAddress, port))
            {
                // Send the message to the server
                if (this.SendMessage(message))
                {
                    // Receive the response from the server
                    _ = this.ReceiveResponse();
                }

                Console.ReadKey();
                // Disconnect from the server
                DisconnectFromServer();
            }
        }

    }
}
