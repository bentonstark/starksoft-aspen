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
    /// The Apdu Send Data SecureCard class.  This class is used to send APDU commands to the SecureCard using the Transmit method
    /// when data needs to also be transmitted to the card.
    /// To receive data from the card use the SCardApduReceiveData class.  To send a ADPU command with without sending or receiving data use the
    /// SCardApdu class.
    /// </summary>
    public class SCardAdpuSend : SCardAdpu
    {
        private byte _lc;           // The number of data bytes to be transmitted during the command, per ISO 7816-4, Section 8.2.1.
        private byte[] _sendData;

        private const int MAX_SEND_DATA_SIZE = 255;

        /// <summary>
        /// Contructor.
        /// </summary>
        /// <param name="sendData">Byte containing data to send.</param>
        public SCardAdpuSend(byte sendData)
        {
            byte[] data = new byte[1];
            data[0] = sendData;
            Initialize(data);
        }

        /// <summary>
        /// Contructor.
        /// </summary>
        /// <param name="sendData">Byte array containing data to send.</param>
        public SCardAdpuSend(byte[] sendData)
        {
            if (sendData == null)
                throw new ArgumentNullException("sendData");
            if (sendData.Length > MAX_SEND_DATA_SIZE)
                throw new ArgumentOutOfRangeException("sendData", "length must be 1 to 255");
            Initialize(sendData);
        }

        /// <summary>
        /// Contructor.
        /// </summary>
        /// <param name="command">Command</param>
        /// <param name="sendData">Byte array containing data to send.</param>
        public SCardAdpuSend(string command, byte[] sendData) : base(command)
        {
            if (sendData == null)
                throw new ArgumentNullException("sendData");
            if (sendData.Length > MAX_SEND_DATA_SIZE)
                throw new ArgumentOutOfRangeException("sendData", "length must be 1 to 255");
            Initialize(sendData);
        }

        /// <summary>
        /// Contructor.
        /// </summary>
        /// <param name="command">Command</param>
        /// <param name="sendData">Byte containing data to send.</param>
        public SCardAdpuSend(string command, byte sendData)
            : base(command)
        {
            byte[] data = new byte[1];
            data[0] = sendData;
            Initialize(data);
        }

        /// <summary>
        /// Gets the LC value or Content Length value.
        /// </summary>
        public byte Lc
        {
            get { return _lc; }
        }

        /// <summary>
        /// Gets the data to transmit to the SecureCard.
        /// </summary>
        public byte[] SendData
        {
            get { return _sendData; }
        }

        internal override byte[] GetSendBuffer()
        {
            byte[] baseSendBuffer = base.GetSendBuffer();

            ArrayBuilder b = new ArrayBuilder(baseSendBuffer.Length + 1 + _sendData.Length);
            b.Append(baseSendBuffer);
            b.Append(_lc);
            b.Append(_sendData);
            return b.GetBytes();
        }

        /// <summary>
        /// Clears private variables and data structures.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            _lc = 0;
            ArrayUtils.Clear(_sendData);
        }

        /// <summary>
        /// Gets the APDU command and data to send as a string.
        /// </summary>
        /// <returns>A string containing the APDU command, response code values, and send data.</returns>
        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            b.Append(base.ToString());
            b.AppendFormat(" LC={0:X02} SendData={0}", _lc, ArrayUtils.HexEncode(_sendData));
            return b.ToString();
        }

        private void Initialize(byte[] sendData)
        {
            _sendData = ArrayUtils.Clone(sendData);
            _lc = (byte)_sendData.Length;
        }

    }
}
