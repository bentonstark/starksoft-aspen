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
    /// The Adpu SecureCard class.  This class is used to send ADPU commands to the SecureCard using the Transmit method
    /// when there is no additional data to send or receive.
    /// To send data to the card use the SCardApduSendData class.  To receive data from the card via a ADPU command use the
    /// SCardApduReceiveData class.
    /// </summary>
    public class SCardAdpu
    {
        /// <summary>
        /// The array length of the return code bytes.
        /// </summary>
        private const int SW_RETURN_ARRAY_LENGTH = 2;
        private const int COMMAND_SIZE = 8;

        private byte _cla;          // The instruction class such as T=0.
        private byte _ins;          // An instruction code in the T=0 instruction class.
        private byte _p1;           // Reference codes that complete the instruction code.
        private byte _p2;           // Reference codes that complete the instruction code.

        private byte _sw1;          // Return byte 1.
        private byte _sw2;          // Return byte 2.
               
        
        /// <summary>
        /// Constructor
        /// </summary>
        public SCardAdpu()
        {   }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="command">Command string containing ADPU commands bytes in hex format.</param>
        public SCardAdpu(string command)
        {
            if (command == null || command.Length != COMMAND_SIZE)
                throw new ArgumentOutOfRangeException("command");
            SetProperties(command);
        }

        /// <summary>
        /// Gets or sets the CLA or Class byte. 
        /// </summary>
        public byte Cla
        {
            get { return _cla; }
            set { _cla = value; }
        }

        /// <summary>
        /// Gets or sets the INS or Instruction Code byte.
        /// </summary>
        public byte Ins
        {
            get { return _ins; }
            set { _ins = value; }
        }

        /// <summary>
        /// Gets or sets the P1 or first Instruction Code Reference Code bytes.
        /// </summary>
        public byte P1
        {
            get { return _p1; }
            set { _p1 = value; }
        }

        /// <summary>
        /// Gets or sets the P2 or second Instruction Code Reference Code bytes.
        /// </summary>
        public byte P2
        {
            get { return _p2; }
            set { _p2 = value; }
        }

        /// <summary>
        /// Gets SW1 or the first status code byte.
        /// </summary>
        public byte Sw1
        {
            get { return _sw1; }
        }

        /// <summary>
        /// Gets SW2 or the second status code byte.
        /// </summary>
        public byte Sw2
        {
            get { return _sw2; }
        }

        /// <summary>
        /// Gets the Status bytes combined as ushort value.
        /// </summary>
        [CLSCompliantAttribute(false)] 
        public ushort Status
        {
            get
            {
                return (ushort)(((short)_sw1 << 8) + (short)_sw2);
            }
        }

        internal virtual void SetReceiveData(byte[] receiveData)
        {
            if (receiveData == null)
                throw new ArgumentNullException("response");
            if (receiveData.Length != SW_RETURN_ARRAY_LENGTH)
                throw new SCardException("response data length invalid");
            _sw1 = receiveData[0];
            _sw2 = receiveData[1];
        }

        internal virtual byte[] GetSendBuffer()
        {
            ArrayBuilder b = new ArrayBuilder(4);
            b.Append(_cla);
            b.Append(_ins);
            b.Append(_p1);
            b.Append(_p2);
            return b.GetBytes();
        }

        internal virtual int GetReceiveBufferSize()
        {
            return SW_RETURN_ARRAY_LENGTH;
        }

        /// <summary>
        /// Clears the data structure private variables.
        /// </summary>
        public virtual void Clear()
        {
            _cla = 0;
            _ins = 0;
            _p1 = 0;
            _p2 = 0;
            _sw1 = 0;
            _sw2 = 0;
        }

        private void SetProperties(string command)
        {
            byte[] cmd = ArrayUtils.HexDecode(command);
            _cla = cmd[0];
            _ins = cmd[1];
            _p1 = cmd[2];
            _p2 = cmd[3];
        }

        /// <summary>
        /// Gets the ADPU command as a string.
        /// </summary>
        /// <returns>A string containing the ADPU command and response code values.</returns>
        public override string  ToString()
        {
            StringBuilder b = new StringBuilder();
            b.AppendFormat("Cla={0:X02} Ins={1:X02} P1={2:X02} P2={3:X02} SW1={4:X02} SW2={5:X02}", _cla, _ins, _p1, _p2, _sw1, _sw2);
            return b.ToString();
        }



    }
}
