using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoRaWAN
{
    /// <summary>
    /// Provides extension methods for byte arrays to read from and write to RFM registers.
    /// </summary>
    public static class Extension
    {
        /// <summary>
        /// Reads data from the specified RFM register address in the byte array.
        /// </summary>
        /// <param name="RFMRegisters">The byte array representing RFM registers.</param>
        /// <param name="address">The address of the register to read from.</param>
        /// <returns>The data read from the register.</returns>
        public static byte Read(this byte[] RFMRegisters, byte address)
        {
            // Return read data from RFM register.
            return RFMRegisters[address];
        }


        /// <summary>
        /// Writes data to the specified RFM register address in the byte array.
        /// </summary>
        /// <param name="RFMRegisters">The byte array representing RFM registers.</param>
        /// <param name="address">The address of the register to write to.</param>
        /// <param name="data">The data to write to the register.</param>
        public static void Write(this byte[] RFMRegisters, byte address, byte data)
        {
            // Write data to RFM register.
            RFMRegisters[address] = data;
        }
    }
}
