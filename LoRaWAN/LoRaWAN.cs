using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace LoRaWAN
{
    public class LoRaWAN
    {
        RFM95 rfm95 = new RFM95();

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

        void SendData()
        {


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
