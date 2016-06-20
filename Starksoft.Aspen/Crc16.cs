//
//  Author:
//       Benton Stark <benton.stark@gmail.com>
//
//  Copyright (c) 2016 Benton Stark
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Security.Cryptography;

namespace Starksoft.Aspen
{


    /// <summary>
    /// Compute standard CRC-16 hash value.  This class
    /// inherits from the standard .NET HashAlgorithm
    /// class.
    /// </summary>
    public class Crc16 : HashAlgorithm
    {
        // constants
        private const ushort Polynomial = 0xA001;
        private const int HashTableSize = 256;

        // hash lookup table
        private ushort[] _table = new ushort[HashTableSize];
        private ushort _crc = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        public Crc16()
        {
            Initialize();
        }

        /// <summary>
        /// Hash core override function.
        /// </summary>
        /// <param name="buffer">Data buffer to hash.</param>
        /// <param name="offset">Offset value in the data buffer.</param>
        /// <param name="count">Number of bytes to hash.</param>
        protected override void HashCore(byte[] buffer, int offset, int count)
        {
            // computer the hash on the bytes given
            for (int i = offset; i < count; i++)
            {
                // xor byte with existing crc value
                byte index = (byte)(_crc ^ buffer[i]);
                // update crc via xor with lookup table
                _crc = (ushort)((_crc >> 8) ^ _table[index]);
            }
        }

        /// <summary>
        /// Hash final bytes.  Nothing to do except return the hash result.
        /// </summary>
        /// <returns>Final hashed bytes.</returns>
        protected override byte[] HashFinal()
        {
            return BitConverter.GetBytes(_crc);
        }

        /// <summary>
        /// Initialize the CRC16 hashing function by building a
        /// hash value table for fast lookup.
        /// </summary>
        public override void Initialize()
        {
            ushort value;
            ushort temp;
            // for each byte in the table loop
            for (ushort i = 0; i < _table.Length; ++i)
            {
                value = 0;
                temp = i;
                // set the bits of each byte
                for (byte j = 0; j < 8; ++j)
                {
                    if (((value ^ temp) & 0x0001) != 0)
                        value = (ushort)((value >> 1) ^ Polynomial);
                    else
                        value >>= 1;

                    temp >>= 1;
                }
                _table[i] = value;
            }
        }
    }

}