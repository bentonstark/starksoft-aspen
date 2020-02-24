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
using System.Collections.Generic;
using System.Text;

namespace Starksoft.Aspen.Smartcard
{
    /// <summary>
    /// The Apdu Receive Data SecureCard class.  This class is used to send APDU commands to the SecureCard using the Transmit method
    /// when there data is expected to be returned from the SecureCard.
    /// To send data to the card use the SCardApduSendData class.  To send a ADPU command with without sending or receiving data use the
    /// SCardApdu class.
    /// </summary>
    public class SCardAdpuRecv : SCardAdpu
    {
        private int _numberOfBytesToReceive;
        private byte _lc;           // The number of data bytes to be transmitted during the command, per ISO 7816-4, Section 8.2.1.
        private byte[] _receiveData;

        private const int MAX_RECEIVE_DATA_SIZE = 256;  // this is no typo as the max receive data size is 256 bytes
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="command">Command string containing ADPU commands bytes in hex format.</param>
        /// <param name="numberOfBytesToReceive">Number of bytes expected to receive from the SecureCard.</param>
        public SCardAdpuRecv(string command, int numberOfBytesToReceive) : base(command)
        {
            Initialize(numberOfBytesToReceive);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="numberOfBytesToReceive">Number of bytes expected to receive from the SecureCard.</param>
        public SCardAdpuRecv(int numberOfBytesToReceive)
        {
            Initialize(numberOfBytesToReceive);
        }

        /// <summary>
        /// Gets the LC value or Content Length value.
        /// </summary>
        public byte Lc
        {
            get { return _lc; }
        }

        /// <summary>
        /// Gets the received data from the SecureCard after the APDU command has been transmitted sucessfully.
        /// </summary>
        public byte[] ReceiveData
        {
            get { return _receiveData; }
        }

        internal override byte[] GetSendBuffer()
        {
            byte[] baseSendBuffer = base.GetSendBuffer();
            ArrayBuilder b = new ArrayBuilder(baseSendBuffer.Length + 1);
            b.Append(baseSendBuffer);
            b.Append(_lc);
            return b.GetBytes();
        }

        internal override void SetReceiveData(byte[] receiveData)
        {
            // make sure the array is valid and what we are expecting
            if (receiveData == null)
                throw new ArgumentNullException("receiveData");
            // the first n-2 bytes of the array contains the data
            _receiveData = ArrayUtils.Subarray(receiveData, 0, receiveData.Length - 2);
            // the last 2 bytes contain the sw values
            byte[] swValues = ArrayUtils.Subarray(receiveData, receiveData.Length - 2, 2);
            base.SetReceiveData(swValues);
        }

        internal override int GetReceiveBufferSize()
        {
            return _numberOfBytesToReceive;
        }

        /// <summary>
        /// Clears private variables and data structures.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            _lc = 0;
            _numberOfBytesToReceive = 0;
            ArrayUtils.Clear(_receiveData);
        }
        
        /// <summary>
        /// Gets the APDU command and return data as a string.
        /// </summary>
        /// <returns>A string containing the APDU command, response code values, and received data.</returns>
        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            b.Append(base.ToString());
            b.AppendFormat(" LC={0:X02}", _lc);
            if (_receiveData != null)
                b.AppendFormat(" ReceiveData={0:X02}", ArrayUtils.HexEncode(_receiveData));
            return b.ToString();
        }

        private void Initialize(int numberOfBytesToReceive)
        {
            if (numberOfBytesToReceive <= 0 || numberOfBytesToReceive > MAX_RECEIVE_DATA_SIZE)
                throw new ArgumentOutOfRangeException("numberOfBytesToReceive", "value must be 1 to 256");
            _numberOfBytesToReceive = numberOfBytesToReceive;
            // set the LC value.  Note that if the number of bytes to receive is 256 then transmit 0 for LC value
            _lc = numberOfBytesToReceive == MAX_RECEIVE_DATA_SIZE ? (byte)0 : (byte)numberOfBytesToReceive;
        }

    }
}
