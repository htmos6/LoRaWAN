#define US902


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;


// Region specified as United States.
// Code refactored according to it.
namespace LoRaWAN
{
    #region RFM_MODES
    /// <summary>
    /// Represents modes for RFM (Radio Frequency Module) communication.
    /// </summary>
    /// <remarks>
    /// These modes control the operational state of the RFM module.
    /// </remarks>
    public enum RFM_MODES : byte
    {
        // Indicates the low-power mode of the RFM module.
        MODE_SLEEP = (byte)0x00,

        // Indicates the LoRa (Long Range) operating mode of the RFM module.
        MODE_LORA = (byte)0x80,

        // Indicates the standby mode of the RFM module.
        // Oscillator and baseband functions of the RFM module are disabled.
        MODE_STDBY = (byte)0x01,

        // Indicates the transmit mode of the RFM module.
        // In this mode, the RFM module is configured to transmit data packets.
        MODE_TX = (byte)0x83,
    }
    #endregion

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
        MULTI,
    }
    #endregion

    #region RFM_DATARATES
    /// <summary>
    /// Represents data rates for RFM communication, with predefined values dependent on the region.
    /// The rates provided below adhere to EU region standards.
    /// Data rates 8-15 are reserved.
    /// </summary>
    public enum RFM_DATARATES : Int16
    {
        // Data rates are defined with respect to spreading factor (SF) and bandwidth (BW).
        SF7BW125 = 0,       // Spreading Factor 7, Bandwidth 125 kHz (5470 bits/sec)
        SF7BW250,           // Spreading Factor 7, Bandwidth 250 kHz (11000 bits/sec)
        SF8BW125,           // Spreading Factor 8, Bandwidth 125 kHz (3125 bits/sec)
        SF9BW125,           // Spreading Factor 9, Bandwidth 125 kHz (1760 bits/sec)
        SF10BW125,          // Spreading Factor 10, Bandwidth 125 kHz (980 bits/sec)
        SF11BW125,          // Spreading Factor 11, Bandwidth 125 kHz (440 bits/sec)
        SF12BW125,          // Spreading Factor 12, Bandwidth 125 kHz (250 bits/sec)
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
        // Register for configuring the mode of the RFM (Radio Frequency Module).
        REG_RFM_MODE = (byte)0x01,

        // Register for controlling PA (Power Amplifier) selection and output power.
        REG_PA_CONFIG = (byte)0x09,

        // Register for configuring higher power settings for the PA (Power Amplifier).
        REG_PA_DAC = (byte)0x4D,

        // Register for setting the most significant bits (MSB) of the preamble length.
        REG_PREAMBLE_MSB = (byte)0x20,

        // Register for setting the least significant bits (LSB) of the preamble length.
        REG_PREAMBLE_LSB = (byte)0x21,

        // Register for storing the most significant bits (MSB) of the RF carrier frequency.
        REG_FRF_MSB = (byte)0x06,

        // Register for storing intermediate bits of the RF carrier frequency.
        REG_FRF_MID = (byte)0x07,

        // Register for storing the least significant bits (LSB) of the RF carrier frequency.
        REG_FRF_LSB = (byte)0x08,

        // Register for storing information from the previous header.
        REG_FEI_LSB = (byte)0x1E,

        // Register for storing the number of received bytes.
        REG_FEI_MSB = (byte)0x1D,

        // Register for configuring the modem.
        REG_MODEM_CONFIG = (byte)0x26,

        // Register for storing the RFM9x version information.
        REG_VER = (byte)0x42,
    }
#endregion


    public class LoRa
    {
        public delegate void ISR(int num);

        public byte[] deviceAddress = new byte[4];

        private byte[] RfmRegister = new byte[256];

        private byte spreadingFactor;
        private byte bandWidth;
        private byte modemConfiguration;

        private byte rfmFrequencyLSB;
        private byte rfmFrequencyMID;
        private byte rfmFrequencyMSB;

        private int randomTransmissionChannelId;

        private bool isMultiChannel;
        public UInt16 transmissionAESCryptNumber { get; set; }
        public UInt16 frameCounter { get; set; }

        public void SendData(byte[] data, byte dataLength, UInt32 transmissionFrameCounter, byte framePort)
        {
            // Direction of frame is up
            byte direction = 0x00;

            byte[] MIC = new byte[4];
            byte[] rfmData = new byte[64];

            byte rfmPackageLength;

            // Unconfirmed data up
            byte macHeader = 0x40;

            byte frameControl = 0x00;

            byte[] temporaryData = new byte[dataLength];
            temporaryData.CopyTo(data, 0);

            /// EncryptPayload

            // Build the Radio Package
            rfmData[0] = macHeader;
            rfmData[1] = deviceAddress[3];
            rfmData[2] = deviceAddress[2];
            rfmData[3] = deviceAddress[1];
            rfmData[4] = deviceAddress[0];
            rfmData[5] = frameControl;
            rfmData[6] = (byte)(transmissionFrameCounter & 0x00FF);
            rfmData[7] = (byte)((transmissionFrameCounter >> 8) & 0x00FF);
            rfmData[8] = framePort;

            // Set Current package length
            rfmPackageLength = 9;

            // Start storing temporaryData elements from index = rfmPackageLength
            // temporaryData.length = data.length = dataLength
            // Copy temporaryData[0,dataLength) elements to end of the rfmData[rfmPackageLength,..)
            Array.Copy(temporaryData, 0, rfmData, rfmPackageLength, dataLength);

            // Add data Lenth to package length
            rfmPackageLength += dataLength;

            /// Calculate MIC



            rfmPackageLength += 4;

            // Send Package
            RfmSendPackage(rfmData, rfmPackageLength);
        }

        /// <summary>
        /// Writes data to the specified RFM register.
        /// </summary>
        /// <param name="RfmAddress">The address of the register to be written.</param>
        /// <param name="RfmData">The data to be written to the register.</param>
        private void RfmRegisterWrite(byte RfmAddress, byte RfmData)
        {
            RfmRegister[RfmAddress] = RfmData;

            // SendData to Gateway
        }

        /// <summary>
        /// Reads data from the specified RFM register.
        /// </summary>
        /// <param name="RfmAddress">The address of the register to be read.</param>
        /// <returns>The data read from the register.</returns>
        private byte RfmRegisterRead(byte RfmAddress)
        {
            return RfmRegister[RfmAddress];
        }

        /// <summary>
        /// Sends a package using the RFM module.
        /// </summary>
        /// <param name="RfmPackage">The package to be sent.</param>
        /// <param name="RfmPackageLength">The length of the package.</param>
        private void RfmSendPackage(byte[] RfmPackage, byte rfmPackageLength)
        {
            // Set RFM in Standby mode and wait for mode readiness
            RfmRegisterWrite((byte)RFM_REGISTERS.REG_RFM_MODE, (byte)RFM_MODES.MODE_STDBY);

            // Wait for standby mode for 5 seconds
            Thread.Sleep(5000);

            // Deactivate interrupt method, inform transmission completion, and run normally

            // Select RFM channel
            if (isMultiChannel == true)
            {
                RfmRegisterWrite((byte)RFM_REGISTERS.REG_FRF_MSB, RegionalFrequencyPlan[randomTransmissionChannelId, 0]);
                RfmRegisterWrite((byte)RFM_REGISTERS.REG_FRF_MID, RegionalFrequencyPlan[randomTransmissionChannelId, 1]);
                RfmRegisterWrite((byte)RFM_REGISTERS.REG_FRF_LSB, RegionalFrequencyPlan[randomTransmissionChannelId, 2]);
            }
            else
            {
                RfmRegisterWrite((byte)RFM_REGISTERS.REG_FRF_MSB, rfmFrequencyMSB);
                RfmRegisterWrite((byte)RFM_REGISTERS.REG_FRF_MID, rfmFrequencyMID);
                RfmRegisterWrite((byte)RFM_REGISTERS.REG_FRF_LSB, rfmFrequencyLSB);
            }

            // Configure RFM settings
            RfmRegisterWrite((byte)RFM_REGISTERS.REG_FEI_LSB, spreadingFactor);
            RfmRegisterWrite((byte)RFM_REGISTERS.REG_FEI_MSB, bandWidth);
            RfmRegisterWrite((byte)RFM_REGISTERS.REG_MODEM_CONFIG, modemConfiguration);

            // Write payload to FIFO
            foreach (byte data in RfmPackage)
            {
                RfmRegisterWrite(0x00, data);
            }

            // Switch RFM to Tx
            RfmRegisterWrite((byte)RFM_REGISTERS.REG_RFM_MODE, (byte)RFM_MODES.MODE_TX);

            // Wait till all transmissions are completed

            // Switch RFM to Sleep
            RfmRegisterWrite((byte)RFM_REGISTERS.REG_RFM_MODE, (byte)RFM_MODES.MODE_SLEEP);
        }



        #region SetDataRate
        /// <summary>
        /// Sets the data rate for RFM (Radio Frequency Module) communication.
        /// </summary>
        /// <param name="dataRate">The data rate to be set.</param>
        /// <remarks>
        /// This method configures the RFM module with the specified data rate, spreading factor, bandwidth, and modem configuration.
        /// </remarks>
        public void SetDataRate(RFM_DATARATES dataRate)
        {
            switch (dataRate)
            {
                case RFM_DATARATES.SF7BW125:
                {
                    spreadingFactor = (byte)0x74; // Spreading Factor 7
                    bandWidth = (byte)0x72;       // Bandwidth 125 kHz
                    modemConfiguration = (byte)0x04;

                    break;
                }
                case RFM_DATARATES.SF7BW250:
                {
                    spreadingFactor = (byte)0x74; // Spreading Factor 7
                    bandWidth = (byte)0x82;       // Bandwidth 250 kHz
                    modemConfiguration = (byte)0x04;

                    break;
                }
                case RFM_DATARATES.SF8BW125:
                {
                    spreadingFactor = (byte)0x84; // Spreading Factor 8
                    bandWidth = (byte)0x72;       // Bandwidth 125 kHz
                    modemConfiguration = (byte)0x04;

                    break;
                }
                case RFM_DATARATES.SF9BW125:
                {
                    spreadingFactor = (byte)0x94; // Spreading Factor 9
                    bandWidth = (byte)0x72;       // Bandwidth 125 kHz
                    modemConfiguration = (byte)0x04;

                    break;
                }
                case RFM_DATARATES.SF10BW125:
                {
                    spreadingFactor = (byte)0xA4; // Spreading Factor 10
                    bandWidth = (byte)0x72;       // Bandwidth 125 kHz
                    modemConfiguration = (byte)0x04;

                    break;
                }
                case RFM_DATARATES.SF11BW125:
                {
                    spreadingFactor = (byte)0xB4; // Spreading Factor 11
                    bandWidth = (byte)0x72;       // Bandwidth 125 kHz
                    modemConfiguration = (byte)0x0C;

                    break;
                }
                case RFM_DATARATES.SF12BW125:
                {
                    spreadingFactor = (byte)0xC4; // Spreading Factor 12
                    bandWidth = (byte)0x72;       // Bandwidth 125 kHz
                    modemConfiguration = (byte)0x0C;

                    break;
                }
                default:
                {
                    spreadingFactor = (byte)0x74; // Default: SF7, BW125
                    bandWidth = (byte)0x72;
                    modemConfiguration = (byte)0x04;

                    break;
                }
            }
        }
        #endregion

        #region SetChannel
        /// <summary>
        /// Sets the channel for RFM (Radio Frequency Module) communication.
        /// </summary>
        /// <param name="channel">The channel to be set.</param>
        /// <remarks>
        /// This method configures the RFM module with the specified channel frequency.
        /// If the channel is set to MULTI, it enables multi-channel mode.
        /// </remarks>
        public void SetChannel(RFM_CHANNELS channel)
        {
            switch (channel)
            {
                case RFM_CHANNELS.CH0:
                {
                    rfmFrequencyLSB = RegionalFrequencyPlan[0, 2]; // Frequency LSB of Channel 0
                    rfmFrequencyMID = RegionalFrequencyPlan[0, 1]; // Frequency MID of Channel 0
                    rfmFrequencyMSB = RegionalFrequencyPlan[0, 0]; // Frequency MSB of Channel 0
                    isMultiChannel = false;

                    break;
                }
                case RFM_CHANNELS.CH1:
                {
                    rfmFrequencyLSB = RegionalFrequencyPlan[1, 2]; // Frequency LSB of Channel 1
                    rfmFrequencyMID = RegionalFrequencyPlan[1, 1]; // Frequency MID of Channel 1
                    rfmFrequencyMSB = RegionalFrequencyPlan[1, 0]; // Frequency MSB of Channel 1
                    isMultiChannel = false;

                    break;
                }
                case RFM_CHANNELS.CH2:
                {
                    rfmFrequencyLSB = RegionalFrequencyPlan[2, 2]; // Frequency LSB of Channel 2
                    rfmFrequencyMID = RegionalFrequencyPlan[2, 1]; // Frequency MID of Channel 2
                    rfmFrequencyMSB = RegionalFrequencyPlan[2, 0]; // Frequency MSB of Channel 2
                    isMultiChannel = false;

                    break;
                }
                case RFM_CHANNELS.CH3:
                {
                    rfmFrequencyLSB = RegionalFrequencyPlan[3, 2]; // Frequency LSB of Channel 3
                    rfmFrequencyMID = RegionalFrequencyPlan[3, 1]; // Frequency MID of Channel 3
                    rfmFrequencyMSB = RegionalFrequencyPlan[3, 0]; // Frequency MSB of Channel 3
                    isMultiChannel = false;

                    break;
                }
                case RFM_CHANNELS.CH4:
                {
                    rfmFrequencyLSB = RegionalFrequencyPlan[4, 2]; // Frequency LSB of Channel 4
                    rfmFrequencyMID = RegionalFrequencyPlan[4, 1]; // Frequency MID of Channel 4
                    rfmFrequencyMSB = RegionalFrequencyPlan[4, 0]; // Frequency MSB of Channel 4
                    isMultiChannel = false;

                    break;
                }
                case RFM_CHANNELS.CH5:
                {
                    rfmFrequencyLSB = RegionalFrequencyPlan[5, 2]; // Frequency LSB of Channel 5
                    rfmFrequencyMID = RegionalFrequencyPlan[5, 1]; // Frequency MID of Channel 5
                    rfmFrequencyMSB = RegionalFrequencyPlan[5, 0]; // Frequency MSB of Channel 5
                    isMultiChannel = false;

                    break;
                }
                case RFM_CHANNELS.CH6:
                {
                    rfmFrequencyLSB = RegionalFrequencyPlan[6, 2]; // Frequency LSB of Channel 6
                    rfmFrequencyMID = RegionalFrequencyPlan[6, 1]; // Frequency MID of Channel 6
                    rfmFrequencyMSB = RegionalFrequencyPlan[6, 0]; // Frequency MSB of Channel 6
                    isMultiChannel = false;

                    break;
                }
                case RFM_CHANNELS.CH7:
                {
                    rfmFrequencyLSB = RegionalFrequencyPlan[7, 2]; // Frequency LSB of Channel 7
                    rfmFrequencyMID = RegionalFrequencyPlan[7, 1]; // Frequency MID of Channel 7
                    rfmFrequencyMSB = RegionalFrequencyPlan[7, 0]; // Frequency MSB of Channel 7
                    isMultiChannel = false;

                    break;
                }
                case RFM_CHANNELS.MULTI:
                {
                    isMultiChannel = true; // Enables multi-channel mode

                    break;
                }
                default:
                {
                    isMultiChannel = true; // Default: Enables multi-channel mode

                    break;
                }
            }
        }
        #endregion


        
        public LoRa()
        {
            // Generate number between [0,7]
            Random rnd = new Random();
            randomTransmissionChannelId = rnd.Next(8);
        }

        /*
        public void SetPower(Int16 transmissionPower = 17);

        // Lorawan message types used to transmit application data or MAC commands. 
        // FPort (Frame port) indicates the type or purpose of the message payload.
        // The FRMPayload field can contain MAC Commands or application data.
        // If the FRMPayload field is not empty, the FPort field must be present.
        // If the FPort field is present,
        //
        // 0	       MAC commands only
        // 1 - 223	   Application-specific data
        // 224	       LoRaWAN MAC layer test protocol
        // 255	       Reserved for Future Use(RFU)
        public void SendData(unsigned char* Data, unsigned char Data_Length, unsigned int Frame_Counter_Tx, uint8_t Frame_Port = 1);
         */

        // Determine signal bandwith and data rate in


        #region RegionalFrequencyPlan
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
        private readonly byte[,] RegionalFrequencyPlan = new byte[,]
        {
            {0xE1, 0xF9, 0xC0}, // Channel 0 903.900 MHz / 61.035 Hz = 14809536 = 0xE1F9C0
            {0xE2, 0x06, 0x8C}, // Channel 1 904.100 MHz / 61.035 Hz = 14812812 = 0xE2068C
            {0xE2, 0x13, 0x59}, // Channel 2 904.300 MHz / 61.035 Hz = 14816089 = 0xE21359
            {0xE2, 0x20, 0x26}, // Channel 3 904.500 MHz / 61.035 Hz = 14819366 = 0xE22026
            {0xE2, 0x2C, 0xF3}, // Channel 4 904.700 MHz / 61.035 Hz = 14822643 = 0xE22CF3
            {0xE2, 0x39, 0xC0}, // Channel 5 904.900 MHz / 61.035 Hz = 14825920 = 0xE239C0
            {0xE2, 0x46,0x8C},  // Channel 6 905.100 MHz / 61.035 Hz = 14829196 = 0xE2468C
            {0xE2, 0x53, 0x59}  // Channel 7 905.300 MHz / 61.035 Hz = 14832473 = 0xE25359
        };
        #endregion

        #region AESSubstitutionTable
        /// <summary>
        /// Represents the cryptography substitution table for AES (Advanced Encryption Standard) algorithm.
        /// </summary>
        /// <remarks>
        /// This table is used in the AES encryption algorithm to substitute bytes in a non-linear
        /// manneer during encryption and decryption processes.
        /// Each element in the table represents a substitution byte.
        /// </remarks>
        private readonly byte[,] AESSubstitutionTable = new byte[,]
        {
            {0x63, 0x7C, 0x77, 0x7B, 0xF2, 0x6B, 0x6F, 0xC5, 0x30, 0x01, 0x67, 0x2B,0xFE, 0xD7, 0xAB, 0x76},
            {0xCA, 0x82, 0xC9, 0x7D, 0xFA, 0x59, 0x47, 0xF0, 0xAD, 0xD4, 0xA2, 0xAF,0x9C, 0xA4, 0x72, 0xC0},
            {0xB7, 0xFD, 0x93, 0x26, 0x36, 0x3F, 0xF7, 0xCC, 0x34, 0xA5, 0xE5, 0xF1,0x71, 0xD8, 0x31, 0x15},
            {0x04, 0xC7, 0x23, 0xC3, 0x18, 0x96, 0x05, 0x9A, 0x07, 0x12, 0x80, 0xE2,0xEB, 0x27, 0xB2, 0x75},
            {0x09, 0x83, 0x2C, 0x1A, 0x1B, 0x6E, 0x5A, 0xA0, 0x52, 0x3B, 0xD6, 0xB3,0x29, 0xE3, 0x2F, 0x84},
            {0x53, 0xD1, 0x00, 0xED, 0x20, 0xFC, 0xB1, 0x5B, 0x6A, 0xCB, 0xBE, 0x39,0x4A, 0x4C, 0x58, 0xCF},
            {0xD0, 0xEF, 0xAA, 0xFB, 0x43, 0x4D, 0x33, 0x85, 0x45, 0xF9, 0x02, 0x7F,0x50, 0x3C, 0x9F, 0xA8},
            {0x51, 0xA3, 0x40, 0x8F, 0x92, 0x9D, 0x38, 0xF5, 0xBC, 0xB6, 0xDA, 0x21,0x10, 0xFF, 0xF3, 0xD2},
            {0xCD, 0x0C, 0x13, 0xEC, 0x5F, 0x97, 0x44, 0x17, 0xC4, 0xA7, 0x7E, 0x3D,0x64, 0x5D, 0x19, 0x73},
            {0x60, 0x81, 0x4F, 0xDC, 0x22, 0x2A, 0x90, 0x88, 0x46, 0xEE, 0xB8, 0x14,0xDE, 0x5E, 0x0B, 0xDB},
            {0xE0, 0x32, 0x3A, 0x0A, 0x49, 0x06, 0x24, 0x5C, 0xC2, 0xD3, 0xAC, 0x62,0x91, 0x95, 0xE4, 0x79},
            {0xE7, 0xC8, 0x37, 0x6D, 0x8D, 0xD5, 0x4E, 0xA9, 0x6C, 0x56, 0xF4, 0xEA,0x65, 0x7A, 0xAE, 0x08},
            {0xBA, 0x78, 0x25, 0x2E, 0x1C, 0xA6, 0xB4, 0xC6, 0xE8, 0xDD, 0x74, 0x1F,0x4B, 0xBD, 0x8B, 0x8A},
            {0x70, 0x3E, 0xB5, 0x66, 0x48, 0x03, 0xF6, 0x0E, 0x61, 0x35, 0x57, 0xB9,0x86, 0xC1, 0x1D, 0x9E},
            {0xE1, 0xF8, 0x98, 0x11, 0x69, 0xD9, 0x8E, 0x94, 0x9B, 0x1E, 0x87, 0xE9,0xCE, 0x55, 0x28, 0xDF},
            {0x8C, 0xA1, 0x89, 0x0D, 0xBF, 0xE6, 0x42, 0x68, 0x41, 0x99, 0x2D, 0x0F,0xB0, 0x54, 0xBB, 0x16}
        };
        #endregion
    }

}
