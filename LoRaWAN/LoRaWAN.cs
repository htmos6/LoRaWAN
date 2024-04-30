using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace LoRaWAN
{
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

        /*
        // Messages
        byte Data_Tx[MAX_UPLINK_PAYLOAD_SIZE];
        sBuffer Buffer_Tx;
        byte Data_Rx[MAX_DOWNLINK_PAYLOAD_SIZE];
        sBuffer Buffer_Rx;
        sLoRa_Message Message_Rx;

        //Callback function variable
        void (* messageCallback) (sBuffer* Data_Rx, bool isConfirmed, uint8_t fPort) = NULL;

        // Declare ABP session
        byte Address_Tx[4];
        byte NwkSKey[16];
        byte AppSKey[16];
        unsigned int Frame_Counter_Tx;
        sLoRa_Session Session_Data;

        // Declare OTAA data struct
        byte DevEUI[8];
        byte AppEUI[8];
        byte AppKey[16];
        byte DevNonce[2];
        byte AppNonce[3];
        byte NetID[3];
        sLoRa_OTAA OTAA_Data;

        // Declare LoRA settings struct
        sSettings LoRa_Settings;
        sRFM_pins LoRa_Pins;

        byte drate_common;

        // Lora Setting Class
        devclass_t dev_class;

        // channel mode
        byte currentChannel;

        // UART
        RFM_command_t RFM_Command_Status;
        rx_t Rx_Status;

        // ACK reception
        ack_t Ack_Status;

        msg_t upMsg_Type;
        */


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
                TxData.Data = aes256.Encrypt(TxData.Data, key, iv);

                // Load encrypted data into RFM package data
                for (byte i = 0; i < TxData.Counter; i++)
                {
                    RFMPackage.Data[RFMPackage.Counter++] = TxData.Data[i];
                }
            }

            // Calculate Message Integrity Code (MIC) for the transmitted data
            byte[] MICData = aes256.CalculateMIC(TxData.Data, key);

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


        void ReceiveData()
        {


        }


        void JoinAccept()
        {


        }


        void SendJoinRequest()
        {


        }


        void SendACK()
        {


        }


        void SetTxPower(int level)
        {
            rfm95.SetTxPower(level);
        }

        int GetRssi()
        {
            // return rssi value in dBm - convertion according to sx1276 datasheet
            return -157 + rfm95.GetRSSI();
        }

        public void Sleep()
        {
            rfm95.SwitchMode((byte)RFM_MODES.RFM_MODE_SLEEP);
        }

        void WakeUp()
        {
            rfm95.SwitchMode((byte)RFM_MODES.RFM_MODE_STANDBY);
        }

    }
}
