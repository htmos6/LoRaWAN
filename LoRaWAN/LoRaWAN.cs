using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime;
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
        public readonly int MAX_UPLINK_PAYLOAD_SIZE = 220;
        public readonly int MAX_DOWNLINK_PAYLOAD_SIZE = 220;

        byte[] key = new byte[16] { 0x69, 0x93, 0xAB, 0x4F, 0x2A, 0xC1, 0x0F, 0x2D, 0x3A, 0x5B, 0x21, 0x8C, 0x4E, 0x97, 0xE9, 0x6C };
        byte[] iv = new byte[16] { 0x8A, 0x57, 0x6F, 0x0C, 0x45, 0x83, 0x28, 0xE0, 0x9E, 0x41, 0x23, 0x14, 0x36, 0xD7, 0xB7, 0x55 };

        // Define the input string
        string message = "Hello World!";

        RFM95 rfm95 = new RFM95();
        AesCryptographyService aes256 = new AesCryptographyService();


        /// <summary>
        /// Performs a LoRaWAN communication cycle, including transmission and reception of data.
        /// </summary>
        /// <param name="TxData">The data to be transmitted.</param>
        /// <param name="RxData">The buffer to store received data.</param>
        /// <param name="RFMCommand">The RFM command type.</param>
        /// <param name="sessionData">The LoRaWAN session data.</param>
        /// <param name="OTAAData">The OTAA (Over-the-Air Activation) data.</param>
        /// <param name="RxMessage">The received LoRaWAN message.</param>
        /// <param name="LoRaSettings">The LoRaWAN settings.</param>
        /// <param name="upMessageType">The type of message to be transmitted.</param>
        public void Cycle(sBuffer TxData, sBuffer RxData, RFM_COMMAND RFMCommand, sLoRaSession sessionData, sLoRaOTAA OTAAData, sLoRaMessage RxMessage, sSettings LoRaSettings, MESSAGE_TYPES upMessageType)
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
            if (RFMCommand == RFM_COMMAND.NEW_RFM_COMMAND)
            {
                // Check the type of message to be transmitted. If it's an uplink message, send data.
                if (upMessageType == MESSAGE_TYPES.MSG_UP)
                {
                    SendData(TxData, sessionData, LoRaSettings);
                }
                // If it's an acknowledgement message, send ACK
                else if (upMessageType == MESSAGE_TYPES.MSG_ACK)
                {
                    SendACK(TxData, sessionData, LoRaSettings);
                }

                // Stop the stopwatch to record the elapsed time
                stopwatch.Stop();
                // Store the elapsed time in milliseconds
                previousTime = stopwatch.ElapsedMilliseconds;



                // If the device operates in Class C mode, immediately switch to RX2 for potential downlink reception.
                if (LoRaSettings.MoteClass == (byte)DEVICE_CLASS_TYPES.CLASS_C)
                {
                    SwitchToCHRX2_SF12BW125(LoRaSettings);

                    // Attempt to receive data on RX2 after transmitting.
                    ReceiveData(RxData, sessionData, OTAAData, RxMessage, LoRaSettings);
                }

                // Wait for the duration of the RX1 window delay before proceeding.
                WaitForDelay(stopwatch, previousTime, ReceiveDelay1);

                // Restore the channel and data rate settings for RX1.
                LoRaSettings.ChannelRx = Rx1Channel;
                LoRaSettings.DatarateRx = Rx1DataRate; 

                // Continue receiving data on RX1 for the duration of RX1 window.
                WaitForRXXWindow(stopwatch, previousTime, ReceiveDelay1, RX1Window, RxData, sessionData, OTAAData, RxMessage, LoRaSettings);

                // Exit the method if a message is received on RX1
                if (RxData.Counter > 0) return;



                // For Class C devices, open RX2 immediately after the end of the first RX window.
                if (LoRaSettings.MoteClass == (byte)DEVICE_CLASS_TYPES.CLASS_C)
                {
                    SwitchToCHRX2_SF12BW125(LoRaSettings);

                    // Attempt to receive data on RX2 after transmitting.
                    ReceiveData(RxData, sessionData, OTAAData, RxMessage, LoRaSettings); 
                }

                // Wait for the duration of RX2 window delay. This is primarily used for testing whether the Class C device receives anything during RX2 window.
                WaitForDelay(stopwatch, previousTime, ReceiveDelay2);

                // Configure the channel and data rate settings for RX2 reception.
                LoRaSettings.ChannelRx = (byte)CHANNEL.CHRX2;          
                LoRaSettings.DatarateRx = (byte)DATA_RATES.SF12BW125;

                // Continue receiving data on RX2 for the duration of RX2 window.
                WaitForRXXWindow(stopwatch, previousTime, ReceiveDelay2, RX2Window, RxData, sessionData, OTAAData, RxMessage, LoRaSettings);

                // Exit the method if a message is received on RX1
                if (RxData.Counter > 0) return;
            }
        }


        /// <summary>
        /// Sends data using LoRa protocol.
        /// </summary>
        /// <param name="TxData">The data to be transmitted.</param>
        /// <param name="sessionData">Session data including device address and frame counter.</param>
        /// <param name="LoRaSettings">LoRa settings for transmission.</param>
        public void SendData(sBuffer TxData, sLoRaSession sessionData, sSettings LoRaSettings)
        {
            // Initialize RFM buffer
            byte[] RFMData = new byte[MAX_UPLINK_PAYLOAD_SIZE + 65];
            sBuffer RFMPackage = new sBuffer() { Data = RFMData, Counter = 0x00 };

            // Initialize Message struct to transmit message
            sLoRaMessage message = new sLoRaMessage();

            sessionData.AppSKey = key;

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

            // If there is data, load the Frame_Port field, encrypt the data, and load it into the RFM package
            if (TxData.Counter > 0x00)
            {
                // Load Frame port field into RFM package data
                RFMPackage.Data[8] = LoRaSettings.Mport; // Mport: Message port

                // Increment the RFM package counter to account for the additional byte
                RFMPackage.Counter++;

                // Encrypt the data using AES256 algorithm
                TxData.Data = aes256.Encrypt(TxData.Data, sessionData.AppSKey, iv);

                // Load encrypted data into RFM package data
                for (byte i = 0; i < TxData.Counter; i++)
                {
                    RFMPackage.Data[RFMPackage.Counter++] = TxData.Data[i];
                }
            }

            // Calculate Message Integrity Code (MIC) for the transmitted data
            byte[] MICData = aes256.CalculateMIC(TxData.Data, sessionData.AppSKey);

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


        void ReceiveData(sBuffer RxData, sLoRaSession sessionData, sLoRaOTAA OTAAData, sLoRaMessage RxMessage, sSettings LoRaSettings)
        {

        }

        /*
        void JoinAccept()
        {


        }


        void SendJoinRequest()
        {


        }
        */


        /// <summary>
        /// Sends ACK using LoRa protocol.
        /// </summary>
        /// <param name="TxData">The data to be transmitted.</param>
        /// <param name="sessionData">Session data including device address and frame counter.</param>
        /// <param name="LoRaSettings">LoRa settings for transmission.</param>
        void SendACK(sBuffer TxData, sLoRaSession sessionData, sSettings LoRaSettings)
        {
            // Initialize RFM buffer
            byte[] RFMData = new byte[MAX_UPLINK_PAYLOAD_SIZE + 65];
            sBuffer RFMPackage = new sBuffer() { Data = RFMData, Counter = 0x00 };

            // Initialize Message struct to transmit message
            sLoRaMessage message = new sLoRaMessage();

            // Initialize sessionData Application Security Key for Encrption of the message.
            sessionData.AppSKey = key;

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

            // If there is data, load the Frame_Port field, encrypt the data, and load it into the RFM package
            if (TxData.Counter > 0x00)
            {
                // Load Frame port field into RFM package data
                RFMPackage.Data[8] = 0; // Mport: Message port

                // Increment the RFM package counter to account for the additional byte
                RFMPackage.Counter++;

                // Encrypt the data using AES256 algorithm
                TxData.Data = aes256.Encrypt(TxData.Data, sessionData.AppSKey, iv);

                // Load encrypted data into RFM package data
                for (byte i = 0; i < TxData.Counter; i++)
                {
                    RFMPackage.Data[RFMPackage.Counter++] = TxData.Data[i];
                }
            }

            // Calculate Message Integrity Code (MIC) for the transmitted data
            byte[] MICData = aes256.CalculateMIC(TxData.Data, sessionData.AppSKey);

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
        private void SwitchToCHRX2_SF12BW125(sSettings LoRaSettings)
        {
            // Configure to RX2 channel for downlink reception.
            LoRaSettings.ChannelRx = (byte)CHANNEL.CHRX2;
            // Set RX2 data rate to SF12 with 125 kHz bandwidth.
            LoRaSettings.DatarateRx = (byte)DATA_RATES.SF12BW125;
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
        private void WaitForRXXWindow(Stopwatch stopwatch, long previousTime, long ReceiveDelayX, long RXXWindow, sBuffer RxData, sLoRaSession sessionData, sLoRaOTAA OTAAData, sLoRaMessage RxMessage, sSettings LoRaSettings)
        {
            // Wait until the total time elapsed exceeds the sum of ReceiveDelayX and RXXWindow
            while (stopwatch.ElapsedMilliseconds - previousTime < ReceiveDelayX + RXXWindow)
            {
                // Continuously attempt to receive data during the RX window
                ReceiveData(RxData, sessionData, OTAAData, RxMessage, LoRaSettings);
            }
        }
    }
}
