using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;

namespace LoRaWAN
{
    public enum RFM_COMMAND
    { 
        NO_RFM_COMMAND, 
        NEW_RFM_COMMAND, 
        RFM_COMMAND_DONE, 
        JOIN, 
        NEW_ACK_COMMAND 
    };

    public class LoRaWAN
    {
        // Define max payload size used for this node
        private readonly int MAX_UPLINK_PAYLOAD_SIZE = 220;
        private readonly int MAX_DOWNLINK_PAYLOAD_SIZE = 220;

        // Declare ABP session
        private byte[] DevAddr = new byte[4];
        private byte[] NwkSKey = new byte[16]; // { 0x69, 0x93, 0xAB, 0x4F, 0x2A, 0xC1, 0x0F, 0x2D, 0x3A, 0x5B, 0x21, 0x8C, 0x4E, 0x97, 0xE9, 0x6C };
        private byte[] AppSKey = new byte[16]; // { 0x8A, 0x57, 0x6F, 0x0C, 0x45, 0x83, 0x28, 0xE0, 0x9E, 0x41, 0x23, 0x14, 0x36, 0xD7, 0xB7, 0x55 };

        private sBuffer TxData;
        private sBuffer RxData;
        private sLoRaMessage RxMessage;
        private sLoRaSession sessionData;
        private sLoRaOTAA OTAAData;
        private sSettings LoRaSettings;

        RFM_COMMAND RFMCommandStatus;
        MESSAGE_TYPES upMessageType;
        DEVICE_CLASS_TYPES deviceClass;
        RX_TYPES RxStatus;
        CHANNEL currentChannel;
        ACK_TYPES AckStatus;

        RFM95 rfm95 = new RFM95();
        AesCryptographyService aes256 = new AesCryptographyService();


        /// <summary>
        /// Initializes a new instance of the LoRaWAN class with the specified parameters.
        /// </summary>
        /// <param name="TxData">The buffer containing data to be transmitted.</param>
        /// <param name="RxData">The buffer to store received data.</param>
        /// <param name="RxMessage">The received LoRaWAN message.</param>
        /// <param name="sessionData">The LoRaWAN session data.</param>
        /// <param name="OTAAData">The OTAA (Over-the-Air Activation) data.</param>
        /// <param name="LoRaSettings">The LoRaWAN settings.</param>
        /// <param name="RFMCommandStatus">The RFM command status.</param>
        /// <param name="upMessageType">The type of message to be transmitted.</param>
        public LoRaWAN(sBuffer TxData, sBuffer RxData, sLoRaMessage RxMessage, sLoRaSession sessionData, sLoRaOTAA OTAAData, sSettings LoRaSettings, RFM_COMMAND RFMCommandStatus, MESSAGE_TYPES upMessageType)
        {
            this.TxData = TxData;                       // Data to be transmitted
            this.RxData = RxData;                       // Buffer to store received data
            this.RxMessage = RxMessage;                 // Received LoRaWAN message
            this.sessionData = sessionData;             // LoRaWAN session data
            this.OTAAData = OTAAData;                   // OTAA data
            this.LoRaSettings = LoRaSettings;           // LoRaWAN settings
            this.RFMCommandStatus = RFMCommandStatus;   // RFM command status
            this.upMessageType = upMessageType;         // Type of message to be transmitted

            this.deviceClass = DEVICE_CLASS_TYPES.CLASS_A; // Default device class
            this.RxStatus = RX_TYPES.NO_RX;             // Default receive status
            this.currentChannel = CHANNEL.CH0;          // Default current channel
            this.AckStatus = ACK_TYPES.NO_ACK;          // Default ACK status
        }                                               


        public void Cycle()
        {
            // Define constant for the delay before the first receive window
            const long ReceiveDelay1 = 1000;
            // Define constant for the delay before the second receive window, ensuring it starts after the first window
            const long ReceiveDelay2 = 2000;
            // Define constant for the duration of the first receive window
            const long RX1Window = 1000;
            // Define constant for the duration of the second receive window
            const long RX2Window = 1000;

            // Define a class-level Stopwatch variable to measure time intervals
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // Initialize a variable to store the previous time value
            long previousTime = 0;

            // Store the current RX1 channel configuration
            byte Rx1Channel = LoRaSettings.ChannelRx;
            // Store the current RX1 data rate configuration
            byte Rx1DataRate = LoRaSettings.DatarateTx;

            // Transmit data if a new RFM command is received
            if (RFMCommandStatus == RFM_COMMAND.NEW_RFM_COMMAND)
            {
                // Check the type of message to be transmitted. If it's an uplink message, send data.
                if (upMessageType == MESSAGE_TYPES.MSG_UP)
                {
                    SendData();
                }
                // If it's an acknowledgement message, send ACK
                else if (upMessageType == MESSAGE_TYPES.MSG_ACK)
                {
                    SendACK();
                }

                // Stop the stopwatch to record the elapsed time
                stopwatch.Stop();
                // Store the elapsed time in milliseconds
                previousTime = stopwatch.ElapsedMilliseconds;



                // If the device operates in Class C mode, immediately switch to RX2 for potential downlink reception.
                if (LoRaSettings.MoteClass == (byte)DEVICE_CLASS_TYPES.CLASS_C)
                {
                    SwitchToCHRX2_SF12BW125();

                    // Attempt to receive data on RX2 after transmitting.
                    ReceiveData(RxMessage);
                }

                // Wait for the duration of the RX1 window delay before proceeding.
                WaitForDelay(stopwatch, previousTime, ReceiveDelay1);

                // Restore the channel and data rate settings for RX1.
                LoRaSettings.ChannelRx = Rx1Channel;
                LoRaSettings.DatarateRx = Rx1DataRate; 

                // Continue receiving data on RX1 for the duration of RX1 window.
                WaitForRXXWindow(stopwatch, previousTime, ReceiveDelay1, RX1Window);

                // Exit the method if a message is received on RX1
                if (RxData.Counter > 0) return;



                // For Class C devices, open RX2 immediately after the end of the first RX window.
                if (LoRaSettings.MoteClass == (byte)DEVICE_CLASS_TYPES.CLASS_C)
                {
                    SwitchToCHRX2_SF12BW125();

                    // Attempt to receive data on RX2 after transmitting.
                    ReceiveData(RxMessage); 
                }

                // Wait for the duration of RX2 window delay. This is primarily used for testing whether the Class C device receives anything during RX2 window.
                WaitForDelay(stopwatch, previousTime, ReceiveDelay2);

                // Configure the channel and data rate settings for RX2 reception.
                LoRaSettings.ChannelRx = (byte)CHANNEL.CHRX2;          
                LoRaSettings.DatarateRx = (byte)DATA_RATES.SF12BW125;

                // Continue receiving data on RX2 for the duration of RX2 window.
                WaitForRXXWindow(stopwatch, previousTime, ReceiveDelay2, RX2Window);

                // Exit the method if a message is received on RX1
                if (RxData.Counter > 0) return;
            }
        }


        /// <summary>
        /// Sends data using LoRa protocol.
        /// </summary>
        public void SendData()
        {
            // Initialize RFM buffer
            byte[] RFMData = new byte[MAX_UPLINK_PAYLOAD_SIZE + 65];
            sBuffer RFMPackage = new sBuffer() { Data = RFMData, Counter = 0x00 };

            // Initialize Message struct to transmit message
            sLoRaMessage message = new sLoRaMessage();

            // MACHeader: Message Authentication Code Header
            message.MACHeader = 0x00;

            // Frame port always 1 for now
            message.FramePort = 0x01;

            // Frame Control: specifies the type of frame being transmitted
            message.FrameControl = 0x00;

            // Load device address from session data into the message
            message.DevAddr[0] = sessionData.DevAddr[0];
            message.DevAddr[1] = sessionData.DevAddr[1];
            message.DevAddr[2] = sessionData.DevAddr[2];
            message.DevAddr[3] = sessionData.DevAddr[3];

            // Set up direction: 0x00 indicates uplink transmission
            message.Direction = 0x00;

            // Load the frame counter from the session data into the message
            message.FrameCounter = sessionData.FrameCounter;

            // Set confirmation:
            // Unconfirmed: 0x00
            // Confirmed: 0x01
            // Check if confirmation setting is unconfirmed
            if (LoRaSettings.Confirm == 0x00)
            {
                // Set bit 6 to indicate unconfirmed transmission
                message.MACHeader = (byte)(message.MACHeader | 0x40);
            }
            else
            {
                // Set bit 7 to indicate confirmed transmission
                message.MACHeader = (byte)(message.MACHeader | 0x80);
            }

            // Build the Radio Package

            // Load MAC header into RFM package data
            RFMPackage.Data[0] = message.MACHeader;

            // Load device address into RFM package data
            RFMPackage.Data[1] = message.DevAddr[3];
            RFMPackage.Data[2] = message.DevAddr[2];
            RFMPackage.Data[3] = message.DevAddr[1];
            RFMPackage.Data[4] = message.DevAddr[0];

            // Load frame control into RFM package data
            RFMPackage.Data[5] = message.FrameControl;

            // Load frame counter into RFM package data
            RFMPackage.Data[6] = (byte)(sessionData.FrameCounter & 0x00FF);
            RFMPackage.Data[7] = (byte)((sessionData.FrameCounter >> 8) & 0x00FF);

            // Set data counter to 8 to indicate the number of bytes added so far
            RFMPackage.Counter = 8;

            // If there is data, load the FramePort field, encrypt the data, and load it into the RFM package
            if (TxData.Counter > 0x00)
            {
                // Load Frame port field into RFM package data
                RFMPackage.Data[8] = LoRaSettings.Mport; // Mport: Message port

                // Increment the RFM package counter to account for the additional byte
                RFMPackage.Counter++;

                // Encrypt the data using AES256 algorithm
                TxData.Data = aes256.Encrypt(TxData.Data, sessionData.NwkSKey, sessionData.AppSKey);

                // Load encrypted data into RFM package data
                for (byte i = 0; i < TxData.Counter; i++)
                {
                    RFMPackage.Data[RFMPackage.Counter++] = TxData.Data[i];
                }
            }

            // Calculate Message Integrity Code (MIC) for the transmitted data
            byte[] MICData = aes256.CalculateMIC(TxData.Data, sessionData.NwkSKey);

            // Load MIC into the RFM package data
            for (byte i = 0; i < 4; i++)
            {
                RFMPackage.Data[RFMPackage.Counter++] = message.MIC[i];
            }

            // Send package using RFM module
            rfm95.SendPackage(RFMPackage, LoRaSettings);

            // Raise Frame counter
            // Check if frame counter has not reached maximum value
            if (sessionData.FrameCounter != 0xFFFF)
            {
                // Increment frame counter
                sessionData.FrameCounter = sessionData.FrameCounter + 1;
            }
            else
            {
                // Reset frame counter to 0x0000 if it reaches the maximum value
                sessionData.FrameCounter = 0x0000;
            }

            // Change channel for the next message if channel hopping is activated
            // Check if channel hopping is enabled
            if (LoRaSettings.ChannelHopping == 0x01)
            {
                // Check if current channel is within valid range (0x00 to 0x07)
                if (LoRaSettings.ChannelTx < 0x07)
                {
                    // Increment channel number for next message
                    LoRaSettings.ChannelTx++;
                }
                else
                {
                    // Reset channel number to 0x00 if it reaches the maximum value
                    LoRaSettings.ChannelTx = 0x00;
                }
            }

        }


        /// <summary>
        /// Receives LoRaWAN data and processes it.
        /// </summary>
        /// <param name="message">The received LoRaWAN message.</param>
        void ReceiveData(sLoRaMessage message)
        {
            // Initialize RFM buffer for storing received data
            byte[] RFMData = new byte[MAX_DOWNLINK_PAYLOAD_SIZE + 65];
            sBuffer RFMPackage = new sBuffer() { Data = RFMData, Counter = 0x00 };

            // Variables for storing check results and data properties
            byte MICCheck;          // MIC check result
            byte addressCheck;      // Address check result
            byte frameOptionsLength;// Length of frame options field
            byte dataLocation;      // Data location within the RFM package

            // Initialize message status
            MESSAGE_STATUS messageStatus = MESSAGE_STATUS.NO_MESSAGE;

            // If it is a type A device, switch RFM to single receive mode
            if (LoRaSettings.MoteClass == (byte)DEVICE_CLASS_TYPES.CLASS_A)
            {
                // Set message status to result of single receive operation
                messageStatus = rfm95.SingleReceive(LoRaSettings);
            }
            else
            {
                // For non-type A devices, switch RFM to standby mode
                rfm95.SwitchMode((byte)RFM_MODES.RFM_MODE_STANDBY);

                // Set message status to indicate new message reception
                messageStatus = MESSAGE_STATUS.NEW_MESSAGE;
            }

            // If there is a message received, get the data from the RFM
            if (messageStatus == MESSAGE_STATUS.NEW_MESSAGE)
            {
                // Get the package from RFM
                messageStatus = rfm95.GetPackage(RFMPackage);

                // If it's a Class C device, switch RFM back to continuous receive mode
                if (LoRaSettings.MoteClass == (byte)DEVICE_CLASS_TYPES.CLASS_C)
                {
                    // Switch RFM to Continuous Receive
                    rfm95.ContinuousReceive(LoRaSettings);
                }
            }

            // If CRC is OK, breakdown the received package
            if (messageStatus == MESSAGE_STATUS.CRC_OK)
            {
                // Get MACHeader
                message.MACHeader = RFMData[0];

                // Data message
                if (message.MACHeader == 0x40 || message.MACHeader == 0x60 || message.MACHeader == 0x80 || message.MACHeader == 0xA0)
                {
                    // Get device address from received data
                    message.DevAddr[0] = RFMData[4];
                    message.DevAddr[1] = RFMData[3];
                    message.DevAddr[2] = RFMData[2];
                    message.DevAddr[3] = RFMData[1];

                    // Get frame control field
                    message.FrameControl = RFMData[5];

                    // Get frame counter
                    message.FrameCounter = RFMData[7];
                    message.FrameCounter = (message.FrameCounter << 8) + RFMData[6];

                    // Lower package length with 4 to remove MIC length
                    RFMPackage.Counter -= 4;

                    // Calculate MIC
                    aes256.CalculateMIC(TxData.Data, sessionData.NwkSKey);

                    MICCheck = 0x00;

                    // Compare MIC
                    for (byte i = 0x00; i < 4; i++)
                    {
                        if (RFMData[RFMPackage.Counter + i] == message.MIC[i])
                        {
                            MICCheck++;
                        }
                    }

                    // Check MIC
                    if (MICCheck == 0x04)
                    {
                        messageStatus = MESSAGE_STATUS.MIC_OK;
                    }
                    else
                    {
                        messageStatus = MESSAGE_STATUS.WRONG_MESSAGE;
                    }

                    addressCheck = 0;
                    // Check address
                    if (MICCheck == 0x04)
                    {
                        for (byte i = 0x00; i < 4; i++)
                        {
                            if (sessionData.DevAddr[i] == message.DevAddr[i])
                            {
                                addressCheck++;
                            }
                        }
                    }

                    messageStatus = (addressCheck == 0x04) ? MESSAGE_STATUS.ADDRESS_OK : MESSAGE_STATUS.WRONG_MESSAGE;

                    // If the address is OK, decrypt the data and send it to USB
                    if (messageStatus == MESSAGE_STATUS.ADDRESS_OK)
                    {
                        dataLocation = 8;

                        // Get length of frame options field
                        frameOptionsLength = (byte)(message.FrameControl & 0x0F);

                        // Add length of frame options field to data location
                        dataLocation = (byte)(dataLocation + frameOptionsLength);

                        // Check if there is data in the package
                        if (RFMPackage.Counter == dataLocation)
                        {
                            RxData.Counter = 0x00;
                        }
                        else
                        {
                            // Get port field when there is data
                            message.FramePort = RFMData[8];

                            // Calculate the amount of data in the package
                            RxData.Counter = (byte)(RFMPackage.Counter - dataLocation - 1);

                            // Correct the data location by 1 for the Fport field
                            dataLocation = (byte)(dataLocation + 1);
                        }

                        // Copy and decrypt the data
                        if (RxData.Counter != 0x00)
                        {
                            for (byte i = 0; i < RxData.Counter; i++)
                            {
                                RxData.Data[i] = RFMData[dataLocation + i];
                            }

                            // Check frame port field. When zero, it is a MAC command message encrypted with NwkSKey
                            // Since Decrption algorithm simplified, keep like that. Just to keep original structure.
                            if (message.FramePort == 0x00)
                            {
                                aes256.Decrypt(RxData.Data, sessionData.NwkSKey, sessionData.AppSKey);
                            }
                            else
                            {
                                aes256.Decrypt(RxData.Data, sessionData.NwkSKey, sessionData.AppSKey);
                            }

                            messageStatus = MESSAGE_STATUS.MESSAGE_DONE;
                        }
                    }
                }

                // If the message status is wrong, set the RxData counter to 0
                if (messageStatus == MESSAGE_STATUS.WRONG_MESSAGE)
                {
                    RxData.Counter = 0x00;
                }
            }
        }



        /// <summary>
        /// Sends ACK using LoRa protocol.
        /// </summary>
        void SendACK()
        {
            // Initialize RFM buffer
            byte[] RFMData = new byte[MAX_UPLINK_PAYLOAD_SIZE + 65];
            sBuffer RFMPackage = new sBuffer() { Data = RFMData, Counter = 0x00 };

            // Initialize Message struct to transmit message
            sLoRaMessage message = new sLoRaMessage();

            // MACHeader: Message Authentication Code Header
            message.MACHeader = 0x00;

            // Set as MAC command
            message.FramePort = 0x00;

            // Frame Control: specifies the type of frame being transmitted
            message.FrameControl = 0x00;

            // Load device address from session data into the message
            message.DevAddr[0] = sessionData.DevAddr[0];
            message.DevAddr[1] = sessionData.DevAddr[1];
            message.DevAddr[2] = sessionData.DevAddr[2];
            message.DevAddr[3] = sessionData.DevAddr[3];

            // Set up direction: 0x00 indicates uplink transmission
            message.Direction = 0x00;

            // Load the frame counter from the session data into the message
            message.FrameCounter = sessionData.FrameCounter;

            // Set(1) bit 6 to indicate unconfirmed transmission
            message.MACHeader = (byte)(message.MACHeader | 0x40);

            // Build the Radio Package

            // Load MAC header into RFM package data
            RFMPackage.Data[0] = message.MACHeader;

            // Load device address into RFM package data
            RFMPackage.Data[1] = message.DevAddr[3];
            RFMPackage.Data[2] = message.DevAddr[2];
            RFMPackage.Data[3] = message.DevAddr[1];
            RFMPackage.Data[4] = message.DevAddr[0];

            // Load frame control into RFM package data
            RFMPackage.Data[5] = (byte)(message.FrameControl | 0x20);

            // Load frame counter into RFM package data
            RFMPackage.Data[6] = (byte)(sessionData.FrameCounter & 0x00FF);
            RFMPackage.Data[7] = (byte)((sessionData.FrameCounter >> 8) & 0x00FF);

            // Set data counter to 8 to indicate the number of bytes added so far
            RFMPackage.Counter = 8;

            // If there is data, load the FramePort field, encrypt the data, and load it into the RFM package
            if (TxData.Counter > 0x00)
            {
                // Load Frame port field into RFM package data
                RFMPackage.Data[8] = 0; // Mport: Message port

                // Increment the RFM package counter to account for the additional byte
                RFMPackage.Counter++;

                // Encrypt the data using AES256 algorithm
                TxData.Data = aes256.Encrypt(TxData.Data, sessionData.NwkSKey, sessionData.AppSKey);

                // Load encrypted data into RFM package data
                for (byte i = 0; i < TxData.Counter; i++)
                {
                    RFMPackage.Data[RFMPackage.Counter++] = TxData.Data[i];
                }
            }

            // Calculate Message Integrity Code (MIC) for the transmitted data
            byte[] MICData = aes256.CalculateMIC(TxData.Data, sessionData.NwkSKey);

            // Load MIC into the RFM package data
            for (byte i = 0; i < 4; i++)
            {
                RFMPackage.Data[RFMPackage.Counter++] = message.MIC[i];
            }

            // Send package using RFM module
            rfm95.SendPackage(RFMPackage, LoRaSettings);

            // Raise Frame counter
            // Check if frame counter has not reached maximum value
            if (sessionData.FrameCounter != 0xFFFF)
            {
                // Increment frame counter
                sessionData.FrameCounter = sessionData.FrameCounter + 1;
            }
            else
            {
                // Reset frame counter to 0x0000 if it reaches the maximum value
                sessionData.FrameCounter = 0x0000;
            }

            // Change channel for the next message if channel hopping is activated
            // Check if channel hopping is enabled
            if (LoRaSettings.ChannelHopping == 0x01)
            {
                // Check if current channel is within valid range (0x00 to 0x07)
                if (LoRaSettings.ChannelTx < 0x07)
                {
                    // Increment channel number for next message
                    LoRaSettings.ChannelTx++;
                }
                else
                {
                    // Reset channel number to 0x00 if it reaches the maximum value
                    LoRaSettings.ChannelTx = 0x00;
                }
            }
        }


        /// <summary>
        /// Sets the transmit power level.
        /// </summary>
        /// <param name="level">The desired transmit power level.</param>
        public void SetTxPower(int level)
        {
            // Delegate the setting of transmit power to the RFM95 module.
            rfm95.SetTxPower(level);
        }


        /// <summary>
        /// Sets the channel for transmission and reception.
        /// </summary>
        /// <param name="channel">The channel number to set.</param>
        public void SetChannel(byte channel)
        {
            // Check if the channel is within valid range
            if (channel <= 8)
            {
                // Set the current channel
                currentChannel = (CHANNEL)channel;
                // Set the transmission channel
                LoRaSettings.ChannelTx = channel;
                // Set the reception channel
                LoRaSettings.ChannelRx = channel;
            }
            else if (channel == (byte)CHANNEL.MULTI)
            {
                // Set the channel to multi-channel mode
                currentChannel = CHANNEL.MULTI;
            }

            // Change the channel in the RFM module
            rfm95.ChangeChannel(channel);
        }


        /// <summary>
        /// Gets the current transmission channel.
        /// </summary>
        /// <returns>The current transmission channel.</returns>
        public byte GetChannel()
        {
            return (byte)LoRaSettings.ChannelTx;
        }


        /// <summary>
        /// Sets the data rate for transmission and reception.
        /// </summary>
        /// <param name="dataRate">The data rate to set.</param>
        public void SetDataRate(byte dataRate)
        {
            // Check if the data rate is within valid range
            if (dataRate >= 0x00 && dataRate <= 0x06)
            {
                // Set the transmission data rate
                LoRaSettings.DatarateTx = dataRate;
                // Set the reception data rate
                LoRaSettings.DatarateRx = dataRate;
            }
            else
            {
                // Set default data rate if out of range
                LoRaSettings.DatarateTx = (byte)DATA_RATES.SF9BW125;
                LoRaSettings.DatarateRx = (byte)DATA_RATES.SF9BW125;
            }

            // Reset RFM command status
            RFMCommandStatus = RFM_COMMAND.NO_RFM_COMMAND;

            // Change the data rate in the RFM module
            rfm95.ChangeDataRate(dataRate);
        }


        /// <summary>
        /// Gets the current transmission data rate.
        /// </summary>
        /// <returns>The current transmission data rate.</returns>
        public byte GetDataRate()
        {
            return LoRaSettings.DatarateTx;
        }


        /// <summary>
        /// Sets the Network Session Key (NwkSKey) for message encryption and decryption.
        /// </summary>
        /// <param name="key">The NwkSKey byte array.</param>
        public void SetNwkSKey(byte[] key)
        {
            // Copy the NwkSKey bytes
            for (byte i = 0; i < 16; i++)
            {
                NwkSKey[i] = key[i];
            }

            // Reset frame counter
            sessionData.FrameCounter = 0x0000;

            // Reset RFM command status
            RFMCommandStatus = RFM_COMMAND.NO_RFM_COMMAND;
        }


        /// <summary>
        /// Sets the Application Session Key (AppSKey) for message encryption and decryption.
        /// </summary>
        /// <param name="key">The AppSKey byte array.</param>
        public void SetAppSKey(byte[] key)
        {
            // Copy the AppSKey bytes
            for (byte i = 0; i < 16; i++)
            {
                AppSKey[i] = key[i];
            }

            // Reset frame counter
            sessionData.FrameCounter = 0x0000;

            // Reset RFM command status
            RFMCommandStatus = RFM_COMMAND.NO_RFM_COMMAND;
        }


        /// <summary>
        /// Sets the device address (DevAddr) for message routing.
        /// </summary>
        /// <param name="key">The DevAddr byte array.</param>
        public void SetDevAddr(byte[] key)
        {
            // Copy the DevAddr bytes
            for (byte i = 0; i < 4; i++)
            {
                DevAddr[i] = key[i];
            }

            // Reset frame counter
            sessionData.FrameCounter = 0x0000;

            // Reset RFM command status
            RFMCommandStatus = RFM_COMMAND.NO_RFM_COMMAND;
        }


        /// <summary>
        /// Retrieves the Received Signal Strength Indication (RSSI) in dBm.
        /// </summary>
        /// <returns>The RSSI value in dBm.</returns>
        public int GetRssi()
        {
            // Return the RSSI value in dBm, adjusted according to the SX1276 datasheet.
            return -157 + rfm95.GetRSSI();
        }


        /// <summary>
        /// Puts the RFM95 module to sleep mode.
        /// </summary>
        public void Sleep()
        {
            // Set the RFM95 module to sleep mode.
            rfm95.SwitchMode((byte)RFM_MODES.RFM_MODE_SLEEP);
        }


        /// <summary>
        /// Wakes up the RFM95 module from sleep mode to standby mode.
        /// </summary>
        public void WakeUp()
        {
            // Set the RFM95 module to standby mode to wake it up.
            rfm95.SwitchMode((byte)RFM_MODES.RFM_MODE_STANDBY);
        }


        /// <summary>
        /// Switches the LoRaSettings to use CHRX2 channel with SF12BW125 data rate.
        /// </summary>
        /// <param name="LoRaSettings">The LoRa settings to be configured.</param>
        private void SwitchToCHRX2_SF12BW125()
        {
            // Configure to RX2 channel for downlink reception.
            LoRaSettings.ChannelRx = (byte)CHANNEL.CHRX2;
            // Set RX2 data rate to SF12 with 125 kHz bandwidth.
            LoRaSettings.DatarateRx = (byte)DATA_RATES.SF12BW125;

            // Updates the channel and data rate settings on the RFMRegisters inside RFM95 module.
            rfm95.ChangeChannel((byte)CHANNEL.CHRX2);
            rfm95.ChangeDataRate((byte)DATA_RATES.SF12BW125);
        }


        /// <summary>
        /// Waits for the specified delay using the provided stopwatch.
        /// </summary>
        /// <param name="stopwatch">The stopwatch used to measure elapsed time.</param>
        /// <param name="previousTime">The previous time recorded.</param>
        /// <param name="delay">The delay to wait for in milliseconds.</param>
        private void WaitForDelay(Stopwatch stopwatch, long previousTime, long delay)
        {
            while (stopwatch.ElapsedMilliseconds - previousTime < delay)
            {
                // Do nothing
            }
        }


        /// <summary>
        /// Waits for the specified RX window duration using the provided stopwatch.
        /// </summary>
        /// <param name="stopwatch">The stopwatch used to measure elapsed time.</param>
        /// <param name="previousTime">The previous time recorded.</param>
        /// <param name="ReceiveDelayX">The delay before the RX window starts in milliseconds.</param>
        /// <param name="RXXWindow">The duration of the RX window in milliseconds.</param>
        /// <param name="RxData">The buffer to store received data.</param>
        /// <param name="sessionData">The LoRa session data.</param>
        /// <param name="OTAAData">The OTAA data.</param>
        /// <param name="RxMessage">The received LoRa message.</param>
        /// <param name="LoRaSettings">The LoRa settings to be used during reception.</param>
        private void WaitForRXXWindow(Stopwatch stopwatch, long previousTime, long ReceiveDelayX, long RXXWindow)
        {
            // Wait until the total time elapsed exceeds the sum of ReceiveDelayX and RXXWindow
            while (stopwatch.ElapsedMilliseconds - previousTime < ReceiveDelayX + RXXWindow)
            {
                // Continuously attempt to receive data during the RX window
                ReceiveData(RxMessage);
            }
        }
    }
}
