using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoRaWAN
{

    #region RFM_REGISTERS
    /// <summary>
    /// Represents registers for RFM (Radio Frequency Module) communication.
    /// </summary>
    /// <remarks>
    /// These registers store various configuration settings and data used by the RFM (Radio Frequency Module).
    /// </remarks>
    public enum RFM_REGISTERS : byte
    {
        // FIFO register. Used for FIFO operations.
        RFM_REG_FIFO = 0x00,

        // Register for configuring the mode of the RFM (Radio Frequency Module).
        RFM_REG_OP_MODE = 0x01,

        // Most significant byte of the frequency setting register.
        RFM_REG_FR_MSB = 0x06,

        // Middle byte of the frequency setting register.
        RFM_REG_FR_MID = 0x07,

        // Least significant byte of the frequency setting register.
        RFM_REG_FR_LSB = 0x08,

        // Over current protection control register.
        RFM_REG_OCP = 0x0b,

        // Power amplifier configuration register.
        RFM_REG_PA_CONFIG = 0x09,

        // Low noise amplifier settings register.
        RFM_REG_LNA = 0x0C,

        // FIFO address pointer register.
        RFM_REG_FIFO_ADDR_PTR = 0x0D,

        // Interrupt flags register.
        RFM_REG_IRQ_FLAGS = 0x12,

        // RSSI (Received Signal Strength Indicator) value of the last received packet.
        RFM_REG_LAST_RSSI = 0x1A,

        // Modem configuration register 1.
        RFM_REG_MODEM_CONFIG1 = 0x1D,

        // Modem configuration register 2.
        RFM_REG_MODEM_CONFIG2 = 0x1E,

        // Symbol timeout register.
        RFM_REG_SYM_TIMEOUT_LSB = 0x1F,

        // Most significant byte of the preamble length register.
        RFM_REG_PREAMBLE_MSB = 0x20,

        // Least significant byte of the preamble length register.
        RFM_REG_PREAMBLE_LSB = 0x21,

        // Payload length register.
        RFM_REG_PAYLOAD_LENGTH = 0x22,

        // Modem configuration register 3.
        RFM_REG_MODEM_CONFIG3 = 0x26,

        // IQ polarity inversion register.
        RFM_REG_INVERT_IQ = 0x33,

        // IQ polarity inversion register 2.
        RFM_REG_INVERT_IQ2 = 0x3b,

        // Sync word register.
        RFM_REG_SYNC_WORD = 0x39,

        // Digital I/O mapping register 1.
        RFM_REG_DIO_MAPPING1 = 0x40,

        // Digital I/O mapping register 2.
        RFM_REG_DIO_MAPPING2 = 0x41,

        // Power amplifier DAC (Digital-to-Analog Converter) register.
        RFM_REG_PA_DAC = 0x4d
    }
    #endregion

    #region RFM_MODES
    /// <summary>
    /// Represents modes for RFM (Radio Frequency Module) communication.
    /// </summary>
    /// <remarks>
    /// These modes control the operational state of the RFM module.
    /// </remarks>
    public enum RFM_MODES : byte
    {
        // Indicates the sleep mode of the RFM module.
        RFM_MODE_SLEEP = 0x00,

        // Indicates the standby mode of the RFM module.
        RFM_MODE_STANDBY = 0x01,

        // Indicates the frequency synthesis TX mode of the RFM module.
        RFM_MODE_FSTX = 0x02,

        // Indicates the transmit mode of the RFM module.
        RFM_MODE_TX = 0x03,

        // Indicates the frequency synthesis RX mode of the RFM module.
        RFM_MODE_FSRX = 0x04,

        // Indicates the continuous receive mode of the RFM module.
        RFM_MODE_RXCONT = 0x05,

        // Indicates the single receive mode of the RFM module.
        RFM_MODE_RXSINGLE = 0x06,

        // Indicates the channel activity detection (CAD) mode of the RFM module.
        RFM_MODE_CAD = 0x07,

        // Indicates the LoRa mode of the RFM module.
        RFM_MODE_LORA = 0x80,
    }
    #endregion


    /// <summary>
    /// Represents an RFM95 module used for radio frequency communication.
    /// </summary>
    public class RFM95
    {
        private byte[] RFMRegisters = new byte[256];


        /// <summary>
        /// Reads data from the specified RFM register address.
        /// </summary>
        /// <param name="address">Register address to read from.</param>
        /// <returns>Data read from the register.</returns>
        public static byte Read(byte address)
        {
            byte data = 0x00;

            // Create client to connect gateway

            // Connect with TCP/IP to gateway.
            // Provide IP and port numbers to TCP/IP connection.

            // Read/Receive data from the gateway.
            // Assign it to data

            // Disconnect from gateway.

            // Return received data
            return data;
        }


        /// <summary>
        /// Writes data to the specified RFM register address.
        /// </summary>
        /// <param name="address">Register address to write to.</param>
        /// <param name="data">Data to write to the register.</param>
        public static void Write(byte address, byte data)
        {
            // Create a client to connect to the gateway.

            // Establish a TCP/IP connection to the gateway.
            // Specify the IP address and port numbers for the TCP/IP connection.

            // Send data to the gateway.

            // Disconnect from the gateway.
        }


        /// <summary>
        /// Switches the mode of the RFM module.
        /// </summary>
        /// <param name="RFMMode">New mode to set for the RFM module.</param>
        public void SwitchMode(byte RFMMode)
        {
            // If the RFM mode is RFM_MODE_SLEEP mode.
            if (RFMMode == 0)
            {
                // Update the RFM register with the new mode.
                RFMRegisters[(int)RFM_REGISTERS.RFM_REG_OP_MODE] = RFMMode;

                // Get the name of the RFM mode.
                string RFMModeName = Enum.GetName(typeof(RFM_MODES), RFMMode);

                Console.WriteLine($"RFM-95 mode changed to: LoRa-{RFMModeName}");
            }
            else
            {
                // Set the MSB of the RFM mode to enable LoRa mode.
                RFMMode |= 0x80;

                // Update the RFM register with the new mode.
                RFMRegisters[(int)RFM_REGISTERS.RFM_REG_OP_MODE] = RFMMode;

                // Get the name of the RFM mode.
                string RFMModeName = Enum.GetName(typeof(RFM_MODES), RFMMode & 0x7F);

                // Display the name of the RFM Mode.
                Console.WriteLine($"RFM-95 mode changed to: LoRa-{RFMModeName}");
            }
        }
    }
}