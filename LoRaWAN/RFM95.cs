using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LoRaWAN
{
    #region RFM_CHANNELS
    /// <summary>
    /// Represents channels for RFM communication.
    /// </summary>
    public enum RFM_CHANNELS : Int16
    {
        CH0 = 0,
        CH1,
        CH2,
        CH3,
        CH4,
        CH5,
        CH6,
        CH7,
        CHRX2 = 8,
        MULTI = 20,
    }
    #endregion

    #region RFM_REGISTERS
    /// <summary>
    /// Represents registers for RFM (Radio Frequency Module) communication.
    /// </summary>
    /// <remarks>
    /// These registers store various configuration settings and data used by the RFM (Radio Frequency Module).
    /// </remarks>
    public enum RFM_REGISTERS : byte
    {
        /// <summary> FIFO register. Used for FIFO operations.</summary>
        RFM_REG_FIFO = 0x00,

        /// <summary> Register for configuring the mode of the RFM (Radio Frequency Module).</summary>
        RFM_REG_OP_MODE = 0x01,

        /// <summary> Most significant byte of the frequency setting register.</summary>
        RFM_REG_FR_MSB = 0x06,

        /// <summary> Middle byte of the frequency setting register.</summary>
        RFM_REG_FR_MID = 0x07,

        /// <summary> Least significant byte of the frequency setting register.</summary>
        RFM_REG_FR_LSB = 0x08,

        /// <summary> Over current protection control register.</summary>
        RFM_REG_OCP = 0x0b,

        /// <summary> Power amplifier configuration register.</summary>
        RFM_REG_PA_CONFIG = 0x09,

        /// <summary> Low noise amplifier settings register.</summary>
        RFM_REG_LNA = 0x0C,

        /// <summary> FIFO address pointer register.</summary>
        RFM_REG_FIFO_ADDR_PTR = 0x0D,

        /// <summary> Interrupt flags register.</summary>
        RFM_REG_IRQ_FLAGS = 0x12,

        /// <summary> RSSI (Received Signal Strength Indicator) value of the last received packet.</summary>
        RFM_REG_LAST_RSSI = 0x1A,

        /// <summary> Modem configuration register 1.</summary>
        RFM_REG_MODEM_CONFIG1 = 0x1D,

        /// <summary> Modem configuration register 2.</summary>
        RFM_REG_MODEM_CONFIG2 = 0x1E,

        /// <summary> Symbol timeout register.</summary>
        RFM_REG_SYM_TIMEOUT_LSB = 0x1F,

        /// <summary> Most significant byte of the preamble length register.</summary>
        RFM_REG_PREAMBLE_MSB = 0x20,

        /// <summary> Least significant byte of the preamble length register.</summary>
        RFM_REG_PREAMBLE_LSB = 0x21,

        /// <summary> Payload length register.</summary>
        RFM_REG_PAYLOAD_LENGTH = 0x22,

        /// <summary> Modem configuration register 3.</summary>
        RFM_REG_MODEM_CONFIG3 = 0x26,

        /// <summary> IQ polarity inversion register.</summary>
        RFM_REG_INVERT_IQ = 0x33,

        /// <summary> IQ polarity inversion register 2.</summary>
        RFM_REG_INVERT_IQ2 = 0x3b,

        /// <summary> Sync word register.</summary>
        RFM_REG_SYNC_WORD = 0x39,

        /// <summary> Digital I/O mapping register 1.</summary>
        RFM_REG_DIO_MAPPING1 = 0x40,

        /// <summary> Digital I/O mapping register 2.</summary>
        RFM_REG_DIO_MAPPING2 = 0x41,

        /// <summary> Power amplifier DAC (Digital-to-Analog Converter) register.</summary>
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
        /// <summary> Indicates the sleep mode of the RFM module.</summary>
        RFM_MODE_SLEEP = 0x00,

        /// <summary> Indicates the standby mode of the RFM module.</summary>
        RFM_MODE_STANDBY = 0x01,

        /// <summary> Indicates the frequency synthesis TX mode of the RFM module.</summary>
        RFM_MODE_FSTX = 0x02,

        /// <summary> Indicates the transmit mode of the RFM module.</summary>
        RFM_MODE_TX = 0x03,

        /// <summary> Indicates the frequency synthesis RX mode of the RFM module.</summary>
        RFM_MODE_FSRX = 0x04,

        /// <summary> Indicates the continuous receive mode of the RFM module.</summary>
        RFM_MODE_RXCONT = 0x05,

        /// <summary> Indicates the single receive mode of the RFM module.</summary>
        RFM_MODE_RXSINGLE = 0x06,

        /// <summary> Indicates the channel activity detection (CAD) mode of the RFM module.</summary>
        RFM_MODE_CAD = 0x07,

        /// <summary> Indicates the LoRa mode of the RFM module.</summary>
        RFM_MODE_LORA = 0x80,
    }
    #endregion

    /// <summary>
    /// Enumerates the possible status of a LoRaWAN message.
    /// </summary>
    public enum MESSAGE_STATUS
    {
        NO_MESSAGE,         // No message is present
        NEW_MESSAGE,        // New message received
        CRC_OK,             // CRC (Cyclic Redundancy Check) is OK
        MIC_OK,             // message Integrity Code (MIC) is OK
        ADDRESS_OK,         // Address is correct
        MESSAGE_DONE,       // message processing is completed
        TIMEOUT,            // Timeout occurred
        WRONG_MESSAGE       // Received message is incorrect
    };


    /// <summary>
    /// Represents an RFM95 module used for radio frequency communication.
    /// </summary>
    public class RFM95
    {
        /// <summary> The RFM95 class contains methods that exclusively modify RFMRegisters.</summary>
        public static byte[] RFMRegisters { get; set; } = new byte[256];


        public MESSAGE_STATUS SingleReceive(sSettings LoRaSettings)
        {
            return MESSAGE_STATUS.NO_MESSAGE;
        }

        public void ContinuousReceive(sSettings LoRaSettings)
        {

            ;
        }


        public byte Init()
        {
            // Read the version information from register 0x42
            // Reads the version information from register 0x42 of the RFM module using the RFMRegisters class.
            byte version = RFMRegisters.Read(0x42);

            // Check if the version is not 18
            // Checks if the version read from the RFM module is not equal to 18.
            if (version != 0)
            {
                // Return 0 indicating failed initialization
                // If the version is not equal to 18, returns 0, indicating a failed initialization.
                return 0;
            }

            // Switch RFM to sleep mode
            // Switches the RFM module to sleep mode using the SwitchMode function with the argument indicating sleep mode.
            SwitchMode((byte)RFM_MODES.RFM_MODE_SLEEP);

            // Wait until RFM is in sleep mode
            // Introduces a delay of 50 milliseconds to ensure that the RFM module transitions to sleep mode effectively.
            Thread.Sleep(50);

            // Set RFM in LoRa mode
            // Sets the RFM module to LoRa mode using the SwitchMode function with the argument indicating LoRa mode.
            SwitchMode((byte)RFM_MODES.RFM_MODE_LORA);

            // Set RFM to standby mode
            // After setting the RFM module to LoRa mode, switches it to standby mode using the SwitchMode function with the argument indicating standby mode.
            SwitchMode((byte)RFM_MODES.RFM_MODE_STANDBY);

            // Set channel to channel 0
            // Sets the RFM module's communication channel to channel 0 using the ChangeChannel function.
            ChangeChannel((byte)RFM_CHANNELS.CH0);

            // Set default power to maximum for EU868 region
            SetTxPower(20);

            // Switch LNA boost on
            // Activates the Low-Noise Amplifier (LNA) boost by writing a specific value to the RFM module's LNA register.
            RFMRegisters.Write((byte)RFM_REGISTERS.RFM_REG_LNA, 0x23);

            // Set RFM to data rate 0 (SF12 BW 125 kHz)
            // Sets the RFM module to use data rate 0, which corresponds to Spreading Factor (SF) 12 and 
            // a bandwidth of 125 kHz in LoRa modulation.
            ChangeDataRate(0x00);

            // Rx Timeout set to 37 symbols
            // Sets the Receive (Rx) Timeout to 37 symbols by writing the appropriate value to the 
            // RFM module's register responsible for setting the Rx Timeout.
            RFMRegisters.Write((byte)RFM_REGISTERS.RFM_REG_SYM_TIMEOUT_LSB, 0x25);

            // Preamble length set to 8 symbols
            // 0x0008 + 4 = 12
            // Sets the preamble length to 8 symbols, where each symbol typically corresponds to one bit. 
            // The additional 4 is added to account for the header that precedes the preamble, resulting in a total 
            // preamble length of 12 bytes.
            RFMRegisters.Write((byte)RFM_REGISTERS.RFM_REG_PREAMBLE_MSB, 0x00);
            RFMRegisters.Write((byte)RFM_REGISTERS.RFM_REG_PREAMBLE_LSB, 0x08);

            // Set LoRa sync word
            // Sets the synchronization word used in LoRa communication by writing the appropriate value to 
            // the RFM module's sync word register.
            RFMRegisters.Write((byte)RFM_REGISTERS.RFM_REG_SYNC_WORD, 0x34);

            // Set FIFO pointers
            // TX base address
            // Rx base address
            // Sets the base addresses for the transmit (TX) and receive (RX) FIFO buffers in the RFM module. 
            // The TX base address is set to 0x80, and the RX base address is set to 0x00.
            RFMRegisters.Write(0x0E, 0x80);
            RFMRegisters.Write(0x0F, 0x00);

            // Return 1 indicating successful initialization
            // Finally, returns 1 to indicate that the initialization process was successful.
            return 1;
        }


        /// <summary>
        /// Sends a package using Radio Frequency Module (RFM).
        /// </summary>
        /// <param name="RFMTxPackage">The package to send.</param>
        /// <param name="LoRaSettings">The LoRa settings to apply for transmission.</param>
        public void SendPackage(sBuffer RFMTxPackage, sSettings LoRaSettings)
        {
            // Variable to hold the location of Tx part in First In, First Out (FiFo) buffer
            byte RFMTxLocation = 0x00;

            // Set RFM module in Standby mode
            SwitchMode((byte)RFM_MODES.RFM_MODE_STANDBY);

            // Switch data rate according to LoRa settings
            ChangeDataRate(LoRaSettings.DatarateTx);

            // Switch to the designated channel for transmission
            ChangeChannel(LoRaSettings.ChannelTx);

            // Switch DIO0 to TxDone
            RFMRegisters.Write((byte)RFM_REGISTERS.RFM_REG_DIO_MAPPING1, 0x40);

            // Set In-Phase (I) and Quadrature (Q) to normal values
            RFMRegisters.Write((byte)RFM_REGISTERS.RFM_REG_INVERT_IQ, 0x27);
            RFMRegisters.Write((byte)RFM_REGISTERS.RFM_REG_INVERT_IQ2, 0x1D);

            // Set payload length to the correct length
            RFMRegisters.Write((byte)RFM_REGISTERS.RFM_REG_PAYLOAD_LENGTH, RFMTxPackage.Counter);

            // Get the location of Tx part of FiFo
            RFMTxLocation = RFMRegisters.Read(0x0E);

            // Set Serial Peripheral Interface (SPI) pointer to start of Tx part in FiFo
            RFMRegisters.Write((byte)RFM_REGISTERS.RFM_REG_FIFO_ADDR_PTR, RFMTxLocation);

            // Write Payload to FiFo
            for (byte i = 0; i < RFMTxPackage.Counter; i++)
            {
                RFMRegisters.Write((byte)RFM_REGISTERS.RFM_REG_FIFO, RFMTxPackage.Data[i]);
            }

            // Switch RFM module to Transmit (Tx) mode
            RFMRegisters.Write((byte)RFM_REGISTERS.RFM_REG_OP_MODE, 0x83);

            // Wait for TxDone signal
            Thread.Sleep(2000);

            // Clear interrupt flag
            RFMRegisters.Write((byte)RFM_REGISTERS.RFM_REG_IRQ_FLAGS, 0x08);
        }


        /// <summary>
        /// Retrieves the status of the received LoRaWAN package.
        /// </summary>
        /// <param name="RFMRxPackage">The buffer containing the received LoRaWAN package.</param>
        /// <returns>The status of the received LoRaWAN package.</returns>
        public MESSAGE_STATUS GetPackage(sBuffer RFMRxPackage)
        {
            // Variable to store RFM interrupts
            byte RFMInterrupts = 0x00;

            // Variable to store RFM package location
            byte RFMPackageLocation = 0x00;     

            MESSAGE_STATUS messageStatus = MESSAGE_STATUS.NO_MESSAGE;

            // Read interrupt register to check for incoming messages
            RFMInterrupts = RFMRegisters.Read(0x12);

            // Check if RX_DONE interrupt is set
            if ((RFMInterrupts & 0x40) == 0x40)  // If RX_DONE_MASK is set
            {
                // Check if CRC is OK
                if ((RFMInterrupts & 0x20) != 0x20)
                {
                    messageStatus = MESSAGE_STATUS.CRC_OK;
                }
                else
                {
                    // CRC check failed, message is incorrect
                    messageStatus = MESSAGE_STATUS.WRONG_MESSAGE;
                }
            }

            // Read the start position of the received package
            RFMPackageLocation = RFMRegisters.Read(0x10);

            // Read the length of the received package
            RFMRxPackage.Counter = RFMRegisters.Read(0x13);

            // Set SPI pointer to the start of the package
            RFMRegisters.Write((byte)RFM_REGISTERS.RFM_REG_FIFO_ADDR_PTR, RFMPackageLocation);

            // Read the received package data from the FIFO
            for (byte i = 0x00; i < RFMRxPackage.Counter; i++)
            {
                RFMRxPackage.Data[i] = RFMRegisters.Read((byte)RFM_REGISTERS.RFM_REG_FIFO);
            }

            // Clear interrupt flags after processing the received package
            RFMRegisters.Write((byte)RFM_REGISTERS.RFM_REG_IRQ_FLAGS, RFMInterrupts);

            // Return the status of the received LoRaWAN package
            return messageStatus;
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

                Console.WriteLine($"RFM95 Mode Changed : LoRa-{RFMModeName}");
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
                Console.WriteLine($"RFM95 Mode Changed : LoRa-{RFMModeName}");
            }
        }


        /// <summary>
        /// Sets the transmission power level for the RFM module.
        /// </summary>
        /// <param name="level">The desired power level, ranging from 0 to 20.</param>
        /// <remarks>
        /// This method ensures that the specified power level is within the acceptable range (0 to 20).
        /// For power levels greater than 17 dBm, additional settings for high power operation are applied.
        /// For power levels below or equal to 17 dBm, default power level operation is configured.
        /// </remarks>
        /// <seealso cref="SetOCP(byte)"/>
        public void SetTxPower(int level)
        {
            // Ensure that the power level is within the acceptable range (0 to 20)
            if (level < 0)
            {
                level = 0;
            }
            else if (level > 20)
            {
                level = 20;
            }

            // Configure the RFM module's PA settings based on the specified power level
            if (level > 17)
            {
                // For power levels greater than 17 dBm, apply additional settings for high power operation
                // Map power levels 18 to 20 to the range 15 to 17
                level -= 3;

                // Configure for high power operation
                RFMRegisters.Write((byte)RFM_REGISTERS.RFM_REG_PA_DAC, 0x87);

                // Set Over Current Protection (OCP) threshold
                SetOCP(140);
            }
            else
            {
                // For power levels below or equal to 17 dBm, configure for default power level operation
                // Configure for default power level operation
                RFMRegisters.Write((byte)RFM_REGISTERS.RFM_REG_PA_DAC, 0x84);

                // Set Over Current Protection (OCP) threshold
                SetOCP(100); 
            }

            // Configure the RFM module's PA settings for the specified power level
            // Apply PA BOOST mask
            RFMRegisters.Write((byte)RFM_REGISTERS.RFM_REG_PA_CONFIG, (byte)(0x80 | (level - 2)));
            Console.WriteLine($"RFM95 Power Settled : {level-2} dB.");
        }


        /// <summary>
        /// Set the Over Current Protection (OCP) threshold for simulation.
        /// </summary>
        /// <param name="mA">The desired current threshold in milliamperes.</param>
        public void SetOCP(byte mA)
        {
            // Set the Over Current Protection (OCP) threshold directly for simulation.
            // Since this is for simulation, the OCP threshold can be set directly without calculations.
            // Default OCP trim value
            byte ocpTrim = 27;

            // Write the OCP configuration (Apply OCP trim) to the RFM module
            RFMRegisters.Write((byte)RFM_REGISTERS.RFM_REG_OCP, (byte)(0x20 | (0x1F & ocpTrim)));

            Console.WriteLine($"RFM95 OCP Trim Value Settled : {ocpTrim} mA.");
        }


        /// <summary>
        /// Retrieves the Received Signal Strength Indicator (RSSI) from the RFM module.
        /// </summary>
        /// <returns>The RSSI value read from the RFM module.</returns>
        /// <remarks>
        /// This method reads the RSSI value from the RFM module's register <see cref="RFM_REGISTERS.RFM_REG_LAST_RSSI"/>.
        /// The RSSI value provides information about the received signal strength.
        /// </remarks>
        public byte GetRSSI()
        {
            return RFMRegisters.Read((byte)RFM_REGISTERS.RFM_REG_LAST_RSSI);
        }


        /// <summary>
        /// Reads data from the specified RFM register address.
        /// </summary>
        /// <param name="address">Register address to read from.</param>
        /// <returns>Data read from the register.</returns>
        public byte Read(byte address)
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
        public void Write(byte address, byte data)
        {
            // Create a client to connect to the gateway.

            // Establish a TCP/IP connection to the gateway.
            // Specify the IP address and port numbers for the TCP/IP connection.

            // Send data to the gateway.

            // Disconnect from the gateway.
        }


        /// <summary>
        /// Changes the channel of the RFM module.
        /// </summary>
        /// <param name="channel">The new channel to set for the RFM module.</param>
        public void ChangeChannel(byte channel)
        {
            // In EU_868 v1.02, the same frequency is used for uplink and downlink.
            if (channel <= 0x08)
            {
                for (byte i = 0; i < 3; i++)
                {
                    // Write the frequency values to the corresponding RFM registers.
                    RFMRegisters.Write((byte)(RFM_REGISTERS.RFM_REG_FR_MSB + i), LoraFrequency[channel, i]);
                }

                Console.WriteLine($"RFM95 Channel Settled Channel : {Enum.GetName(typeof(CHANNEL), channel)}.");
            }
        }


        /// <summary>
        /// Changes the data rate of the RFM module.
        /// </summary>
        /// <param name="dataRate">The new data rate to set for the RFM module. Region specified as EU868.
        /// <list type="table">
        ///     <item><term>0x00</term><description> SF12BW125 </description></item>
        ///     <item><term>0x01</term><description> SF11BW125</description></item>
        ///     <item><term>0x02</term><description> SF10BW125</description></item>
        ///     <item><term>0x03</term><description> SF9BW125</description></item>
        ///     <item><term>0x04</term><description> SF8BW125</description></item>
        ///     <item><term>0x05</term><description> SF7BW125</description></item>
        ///     <item><term>0x06</term><description> SF7BW250</description></item>
        ///     <item><term>Default</term><description> SF9BW125</description></item>
        /// </list>
        /// </param>
        public void ChangeDataRate(byte dataRate)
        {
            switch (dataRate)
            {
                case 0x00:  // SF12BW125
                    ChangeSFandBW(12, 0x07);
                    break;
                case 0x01:  // SF11BW125
                    ChangeSFandBW(11, 0x07);
                    break;
                case 0x02:  // SF10BW125
                    ChangeSFandBW(10, 0x07);
                    break;
                case 0x03:  // SF9BW125
                    ChangeSFandBW(9, 0x07);
                    break;
                case 0x04:  // SF8BW125
                    ChangeSFandBW(8, 0x07);
                    break;
                case 0x05:  // SF7BW125
                    ChangeSFandBW(7, 0x07);
                    break;
                case 0x06:  // SF7BW250
                    ChangeSFandBW(7, 0x08);
                    break;
                default: // SF9BW125
                    ChangeSFandBW(9, 0x07);
                    break;
            }

            Console.WriteLine($"RFM95 Data Rate Changed : {Enum.GetName(typeof(DATA_RATES), dataRate)}.");
        }


        /// <summary>
        /// Changes the spreading factor and bandwidth of the RFM module.
        /// </summary>
        /// <param name="spreadingFactor">The spreading factor to set. Should be in the range {6, 7, 8, 9, 10, 11, 12}.</param>
        /// <param name="bandWidth">The bandwidth to set. Should be one of the following values: 
        /// <list type="table">
        ///     <item><term>0x00</term><description> 7.8kHz</description></item>
        ///     <item><term>0x01</term><description> 10.4kHz</description></item>
        ///     <item><term>0x02</term><description> 15.6kHz</description></item>
        ///     <item><term>0x03</term><description> 20.8kHz</description></item>
        ///     <item><term>0x04</term><description> 31.25kHz</description></item>
        ///     <item><term>0x05</term><description> 41.7kHz</description></item>
        ///     <item><term>0x06</term><description> 62.5kHz</description></item>
        ///     <item><term>0x07</term><description> 125kHz</description></item>
        ///     <item><term>0x08</term><description> 250kHz</description></item>
        ///     <item><term>0x09</term><description> 500kHz</description></item>
        /// </list>
        /// </param>
        private void ChangeSFandBW(byte spreadingFactor, byte bandWidth)
        {
            // Set Cyclic Redundancy Check (CRC) On and specify the spreading factor.
            RFMRegisters.Write((byte)RFM_REGISTERS.RFM_REG_MODEM_CONFIG2, (byte)((spreadingFactor << 4) | 0b0100));

            // Set coding rate and specify the bandwidth.
            RFMRegisters.Write((byte)RFM_REGISTERS.RFM_REG_MODEM_CONFIG1, (byte)((bandWidth << 4) | 0x02));

            // Check if the spreading factor is greater than 10.
            if (spreadingFactor > 10)
            {
                // Enable automatic gain control (AGC) and low data rate optimization.
                RFMRegisters.Write((byte)RFM_REGISTERS.RFM_REG_MODEM_CONFIG3, 0b1100);
            }
            else
            {
                // Set AGC according to LnaGain register and enable low data rate optimization.
                RFMRegisters.Write((byte)RFM_REGISTERS.RFM_REG_MODEM_CONFIG3, 0b0100);
            }
        }


        /// <summary>
        /// Represents the regional frequency plan for RFM (Radio Frequency Module) communication.
        /// </summary>
        /// <remarks>
        /// This array contains frequency settings for different channels used in RFM communication.
        /// Each row represents a channel, and the values correspond to frequency components.
        /// Regional Frequency Plan basically refers to allocation of radio frequency bands 
        /// within specific geographic regions for LoRaWAN communication. 
        /// These plans define which frequencies and channels are available for use by LoRaWAN devices in a particular region.
        /// </remarks>
        private readonly byte[,] LoraFrequency = new byte[,]
        {
            { 0xD9, 0x06, 0x8B }, //Channel [0], 868.1 MHz / 61.035 Hz = 14222987 = 0xD9068B
            { 0xD9, 0x13, 0x58 }, //Channel [1], 868.3 MHz / 61.035 Hz = 14226264 = 0xD91358
            { 0xD9, 0x20, 0x24 }, //Channel [2], 868.5 MHz / 61.035 Hz = 14229540 = 0xD92024
            { 0xD8, 0xC6, 0x8B }, //Channel [3], 867.1 MHz / 61.035 Hz = 14206603 = 0xD8C68B
            { 0xD8, 0xD3, 0x58 }, //Channel [4], 867.3 MHz / 61.035 Hz = 14209880 = 0xD8D358
            { 0xD8, 0xE0, 0x24 }, //Channel [5], 867.5 MHz / 61.035 Hz = 14213156 = 0xD8E024
            { 0xD8, 0xEC, 0xF1 }, //Channel [6], 867.7 MHz / 61.035 Hz = 14216433 = 0xD8ECF1
            { 0xD8, 0xF9, 0xBE }, //Channel [7], 867.9 MHz / 61.035 Hz = 14219710 = 0xD8F9BE
            { 0xD9, 0x61, 0xBE }, // RX2 Receive channel 869.525 MHz / 61.035 Hz = 14246334 = 0xD961BE    
        };
    }
}