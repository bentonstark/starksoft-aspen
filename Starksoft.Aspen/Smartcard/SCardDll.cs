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
using System.Text;
using System.Runtime.InteropServices;

namespace Starksoft.Aspen.Smartcard
{

    /// <summary>
    /// Wraps the SCARD_IO_STRUCTURE
    ///  
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SCARD_IO_REQUEST
    {
        /// <summary>
        /// Protocol byte.
        /// </summary>
        [CLSCompliantAttribute(false)] 
        public UInt32 dwProtocol;
        /// <summary>
        /// PCI length byte.
        /// </summary>
        [CLSCompliantAttribute(false)] 
        public UInt32 cbPciLength;
    }

    internal class SCardDll
    {
        private const string _winSCardDll = "winscard.dll";
        
        [DllImport(_winSCardDll)]
        public static extern int SCardListReadersA(IntPtr hContext, string mszGroups, IntPtr mszReaders, ref UInt32 pcchReaders);

        [DllImport(_winSCardDll)]
        public static extern int SCardListCardsA(IntPtr hContext, IntPtr pbAtr, IntPtr rgquidInterfaces, IntPtr cguidInterfaceCount, IntPtr mszCards, ref UInt32 pcchCards);

        [DllImport(_winSCardDll)]
        public static extern int SCardEstablishContext(UInt32 dwScope, IntPtr pvReserved1, IntPtr pvReserved2, ref IntPtr phContext);

        [DllImport(_winSCardDll)]
        public static extern int SCardReleaseContext(IntPtr hContext);

        [DllImport(_winSCardDll)]
        public static extern int SCardConnectA(IntPtr hContext, string szReader, UInt32 dwShareMode, UInt32 dwPreferredProtocols, ref IntPtr phCard, ref UInt32 pdwActiveProtocol);

        [DllImport(_winSCardDll)]
        public static extern int SCardDisconnect(IntPtr hCard, UInt32 dwDisposition);

        [DllImport(_winSCardDll)]
        //public static extern int SCardTransmit(IntPtr hCard, [In] ref SCard_IO_Request pioSendPci, [MarshalAs(UnmanagedType.LPArray)] byte[] pbSendBuffer, UInt32 cbSendLength, ref IntPtr pioRecvPci, [MarshalAs(UnmanagedType.LPArray)]  byte[] pbRecvBuffer, ref UInt32 pcbRecvLength);
        public static extern int SCardTransmit(IntPtr hCard, ref SCARD_IO_REQUEST pioSendPci, ref byte pbSendBuffer, int cbSendLength, IntPtr pioRecvPci, ref byte pbRecvBuffer, ref int pcbRecvLength);

        [DllImport(_winSCardDll)]
        public static extern int SCardBeginTransaction(IntPtr hContext);

        [DllImport(_winSCardDll)]
        public static extern int SCardEndTransaction(IntPtr hContext, UInt32 dwDisposition);

        [DllImport(_winSCardDll)]
        public static extern int SCardGetAttrib(IntPtr hCard, UInt32 dwAttribId, [MarshalAs(UnmanagedType.LPArray)] byte[] pbAttr, ref UInt32 pcbAttrLen);

    }


}
