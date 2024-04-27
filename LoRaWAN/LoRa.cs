#define US902


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// Region specified as United States.
// Code refactored according to it.
namespace LoRaWAN
{ 

    /// <summary>
    /// Represents channels for RFM communication.
    /// </summary>
    public enum RfmChannels : int
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


    /// <summary>
    /// Represents data rates for RFM communication, with predefined values dependent on the region.
    /// The rates provided below adhere to United States standards.
    /// Data rates 5-7 and 12, 14-15 are reserved.
    /// </summary>
    public enum RfmDataRates
    {
        // Data rates are defined with respect to spreading factor (SF) and bandwidth (BW).

        SF10BW125 = 0,  // Spreading Factor 10, Bandwidth 125 kHz (980 bits/sec)
        SF9BW125 = 1,   // Spreading Factor 9, Bandwidth 125 kHz (1760 bits/sec)
        SF8BW125 = 2,   // Spreading Factor 8, Bandwidth 125 kHz (3125 bits/sec)
        SF7BW125 = 3,   // Spreading Factor 7, Bandwidth 125 kHz (5470 bits/sec)
        SF8BW500 = 4,   // Spreading Factor 8, Bandwidth 500 kHz (12500 bits/sec)
        SF12BW500 = 8,  // Spreading Factor 12, Bandwidth 500 kHz (980 bits/sec)
        SF11BW500 = 9,  // Spreading Factor 11, Bandwidth 500 kHz (1760 bits/sec)
        SF10BW500 = 10, // Spreading Factor 10, Bandwidth 500 kHz (3900 bits/sec)
        SF9BW500 = 11,  // Spreading Factor 9, Bandwidth 500 kHz (7000 bits/sec)
        SF7BW500 = 13   // Spreading Factor 7, Bandwidth 500 kHz (21900 bits/sec)
    }

    public class LoRa
    {
        



    }
}
