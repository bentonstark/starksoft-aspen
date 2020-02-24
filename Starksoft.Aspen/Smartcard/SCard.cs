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
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Starksoft.Aspen.Smartcard
{

    /// <summary>
    /// SCard share mode options.
    /// </summary>
    public enum ShareModeOptions
    {
        /// <summary>
        /// Exclusive mode only.  Do not allow other processes to access the Smartcard device.
        /// </summary>
        Exclusive = 0x0001,
        /// <summary>
        /// Shared mode only.  Allow other processes to access the Smartcard device.
        /// </summary>
        Shared = 0x0002,
        /// <summary>
        /// Direct mode only.  The reader is for private use, and the process will be controlling it directly. No other processes are allowed access.
        /// This mode may only be available when using raw protocol communications.
        /// </summary>
        Direct = 0x0003
    }

    /// <summary>
    /// SCard disposition action enumeration.
    /// </summary>
    public enum DispositionOptions
    {
        /// <summary>
        /// Do nothing to the card on close.
        /// </summary>
        LeaveCard = 0x0000,
        /// <summary>
        /// Reset the card on close.
        /// </summary>
        ResetCard = 0x0001,
        /// <summary>
        /// Power down the card on close.
        /// </summary>
        UnpowerCard = 0x0002,
        /// <summary>
        /// Eject the card on close.
        /// </summary>
        EjectCard = 0x0003
    }


    /// <summary>
    /// SCard protocol enumeration.
    /// </summary>
    [Flags]
    public enum ProtocolOptions
    {
        /// <summary>
        /// Protocol could not be established.  Card may be absent from reader.
        /// </summary>
        Undefined = 0x00000000,
        /// <summary>
        /// Set the communication protocol to T=0.  T=0 is defined as an asynchronous, character-oriented half-duplex transmission protocol.
        /// </summary>
        T0 = 0x00000001,
        /// <summary>
        /// Set the communication protocol to T=1.  T=1 is an asynchronous, block-oriented half-duplex transmission protocol.
        /// </summary>
        T1 = 0x00000002,
        /// <summary>
        /// Set the communication protocol to a raw mode.
        /// </summary>
        Raw = 0x00010000,
        //Default = unchecked((int)0x80000000),  // Use implicit PTS.
    }
    
    /// <summary>
    /// SCard scope enumeration.
    /// </summary>
    public enum ScopeOptions
    {
        /// <summary>
        /// The context is a user context, and any database operations are performed within the
        /// domain of the user.
        /// </summary>
        User = 0x0000,
        /// <summary>
        /// The context is that of the current terminal, and any database operations are performed
        /// within the domain of that terminal.  (The calling application must have appropriate
        /// access permissions for any database actions.)
        /// </summary>
        Terminal = 0x0001,
        /// <summary>
        /// The context is the system context, and any database operations are performed within the
        /// domain of the system.  (The calling application must have appropriate access
        /// permissions for any database actions.)
        /// </summary>
        System = 0x00002
    }

    /// <summary>
    /// Action return code values.
    /// </summary>
    [CLSCompliantAttribute(false)] 
    public enum ActionReturnCodes : uint
    {
        /// <summary>
        /// Unknown action return code.
        /// </summary>
        Unknown = 0xFFFFFFFF,
        /// <summary>
        /// Action was sucessful.
        /// </summary>
        SCARD_S_SUCCESS = 0x00000000,
        /// <summary>
        ///  An internal consistency check failed.
        /// </summary>
        SCARD_F_INTERNAL_ERROR = 0x80100001,
        /// <summary>
        ///  The action was cancelled by an SCardCancel request.
        ///  </summary>
        SCARD_E_CANCELLED = 0x80100002,
        /// <summary>
        ///  The supplied handle was invalid.
        /// </summary>
        SCARD_E_INVALID_HANDLE = 0x80100003,
        /// <summary>
        ///  One or more of the supplied parameters could not be properly interpreted.
        /// </summary>
        SCARD_E_INVALID_PARAMETER = 0x80100004,
        /// <summary>
        ///  Registry startup information is missing or invalid.
        /// </summary>
        SCARD_E_INVALID_TARGET = 0x80100005,
        /// <summary>
        ///  Not enough memory available to complete this command.
        /// </summary>
        SCARD_E_NO_MEMORY = 0x80100006,
        /// <summary>
        /// An internal consistency timer has expired.
        /// </summary>
        SCARD_F_WAITED_TOO_LONG = 0x80100007,
        /// <summary>
        /// The data buffer to receive returned data is too small for the returned data.
        /// </summary>
        SCARD_E_INSUFFICIENT_BUFFER = 0x80100008,
        /// <summary>
        /// The specified reader name is not recognized.
        /// </summary>
        SCARD_E_UNKNOWN_READER = 0x80100009,
        /// <summary>
        /// The user-specified timeout value has expired.
        /// </summary>
        SCARD_E_TIMEOUT = 0x8010000A,
        /// <summary>
        /// The smart card cannot be accessed because of other connections outstanding.
        /// </summary>
        SCARD_E_SHARING_VIOLATION = 0x8010000B,
        /// <summary>
        /// The operation requires a Smart Card, but no Smart Card is currently in the device.
        /// </summary>
        SCARD_E_NO_SMARTCARD = 0x8010000C,
        /// <summary>
        /// The specified smart card name is not recognized.
        /// </summary>
        SCARD_E_UNKNOWN_CARD = 0x8010000D,
        /// <summary>
        /// The system could not dispose of the media in the requested manner.
        /// </summary>
        SCARD_E_CANT_DISPOSE = 0x8010000E,
        /// <summary>
        /// The requested protocols are incompatible with the protocol currently in use with the smart card.
        /// </summary>
        SCARD_E_PROTO_MISMATCH = 0x8010000F,
        /// <summary>
        /// The reader or smart card is not ready to accept commands.
        /// </summary>
        SCARD_E_NOT_READY = 0x80100010,
        /// <summary>
        /// One or more of the supplied parameters values could not be properly interpreted.
        /// </summary>
        SCARD_E_INVALID_VALUE = 0x80100011,
        /// <summary>
        /// The action was cancelled by the system, presumably to log off or shut down.
        /// </summary>
        SCARD_E_SYSTEM_CANCELLED = 0x80100012,
        /// <summary>
        /// An internal communications error has been detected.
        /// </summary>
        SCARD_F_COMM_ERROR = 0x80100013,
        /// <summary>
        /// An internal error has been detected, but the source is unknown.
        /// </summary>
        SCARD_F_UNKNOWN_ERROR = 0x80100014,
        /// <summary>
        /// An ATR obtained from the registry is not a valid ATR string.
        /// </summary>
        SCARD_E_INVALID_ATR = 0x80100015,
        /// <summary>
        /// An attempt was made to end a non-existent transaction.
        /// </summary>
        SCARD_E_NOT_TRANSACTED = 0x80100016,
        /// <summary>
        /// The specified reader is not currently available for use.
        /// </summary>
        SCARD_E_READER_UNAVAILABLE = 0x80100017,
        /// <summary>
        /// The operation has been aborted to allow the server application to exit.
        /// </summary>
        SCARD_P_SHUTDOWN = 0x80100018,
        /// <summary>
        /// The PCI Receive buffer was too small.
        /// </summary>
        SCARD_E_PCI_TOO_SMALL = 0x80100019,
        /// <summary>
        /// The reader driver does not meet minimal requirements for support.
        /// </summary>
        SCARD_E_READER_UNSUPPORTED = 0x8010001A,
        /// <summary>
        /// The reader driver did not produce a unique reader name.
        /// </summary>
        SCARD_E_DUPLICATE_READER = 0x8010001B,
        /// <summary>
        /// The smart card does not meet minimal requirements for support.
        /// </summary>
        SCARD_E_CARD_UNSUPPORTED = 0x8010001C,
        /// <summary>
        /// The Smart card resource manager is not running.
        /// </summary>
        SCARD_E_NO_SERVICE = 0x8010001D,
        /// <summary>
        /// The Smart card resource manager has shut down.
        /// </summary>
        SCARD_E_SERVICE_STOPPED = 0x8010001E,
        /// <summary>
        /// An unexpected card error has occurred.
        /// </summary>
        SCARD_E_UNEXPECTED = 0x8010001F,
        /// <summary>
        /// No Primary Provider can be found for the smart card.
        /// </summary>
        SCARD_E_ICC_INSTALLATION = 0x80100020,
        /// <summary>
        /// The requested order of object creation is not supported.
        /// </summary>
        SCARD_E_ICC_CREATEORDER = 0x80100021,
        /// <summary>
        /// This smart card does not support the requested feature.
        /// </summary>
        SCARD_E_UNSUPPORTED_FEATURE = 0x80100022,
        /// <summary>
        /// The identified directory does not exist in the smart card.
        /// </summary>
        SCARD_E_DIR_NOT_FOUND = 0x80100023,
        /// <summary>
        /// The identified file does not exist in the smart card.
        /// </summary>
        SCARD_E_FILE_NOT_FOUND = 0x80100024,
        /// <summary>
        /// The supplied path does not represent a smart card directory.
        /// </summary>
        SCARD_E_NO_DIR = 0x80100025,
        /// <summary>
        /// The supplied path does not represent a smart card file.
        /// </summary>
        SCARD_E_NO_FILE = 0x80100026,
        /// <summary>
        /// Access is denied to this file.
        /// </summary>
        SCARD_E_NO_ACCESS = 0x80100027,
        /// <summary>
        /// The smartcard does not have enough memory to store the information.
        /// </summary>
        SCARD_E_WRITE_TOO_MANY = 0x80100028,
        /// <summary>
        /// There was an error trying to set the smart card file object pointer.
        /// </summary>
        SCARD_E_BAD_SEEK = 0x80100029,
        /// <summary>
        /// The supplied PIN is incorrect.
        /// </summary>
        SCARD_E_INVALID_CHV = 0x8010002A,
        /// <summary>
        /// An unrecognized error code was returned from a layered component.
        /// </summary>
        SCARD_E_UNKNOWN_RES_MNG = 0x8010002B,
        /// <summary>
        /// The requested certificate does not exist.
        /// </summary>
        SCARD_E_NO_SUCH_CERTIFICATE = 0x8010002C,
        /// <summary>
        /// The requested certificate could not be obtained.
        /// </summary>
        SCARD_E_CERTIFICATE_UNAVAILABLE = 0x8010002D,
        /// <summary>
        /// Cannot find a smart card reader.
        /// </summary>
        SCARD_E_NO_READERS_AVAILABLE = 0x8010002E,
        /// <summary>
        /// A communications error with the smart card has been detected.  Retry the operation.
        /// </summary>
        SCARD_E_COMM_DATA_LOST = 0x8010002F,
        /// <summary>
        /// The requested key container does not exist on the smart card.
        /// </summary>
        SCARD_E_NO_KEY_CONTAINER = 0x80100030,
        /// <summary>
        /// The Smart card resource manager is too busy to complete this operation.
        /// </summary>
        SCARD_E_SERVER_TOO_BUSY = 0x80100031,
        /// <summary>
        /// The reader cannot communicate with the smart card, due to ATR configuration conflicts.
        /// </summary>
        SCARD_W_UNSUPPORTED_CARD = 0x80100065,
        /// <summary>
        /// The smart card is not responding to a reset.
        /// </summary>
        SCARD_W_UNRESPONSIVE_CARD = 0x80100066,
        /// <summary>
        /// Power has been removed from the smart card, so that further communication is not possible.
        /// </summary>
        SCARD_W_UNPOWERED_CARD = 0x80100067,
        /// <summary>
        /// The smart card has been reset, so any shared state information is invalid.
        /// </summary>
        SCARD_W_RESET_CARD = 0x80100068,
        /// <summary>
        /// The smart card has been removed, so that further communication is not possible.
        /// </summary>
        SCARD_W_REMOVED_CARD = 0x80100069,
        /// <summary>
        /// Access was denied because of a security violation.
        /// </summary>
        SCARD_W_SECURITY_VIOLATION = 0x8010006A,
        /// <summary>
        /// The card cannot be accessed because the wrong PIN was presented.
        /// </summary>
        SCARD_W_WRONG_CHV = 0x8010006B,
        /// <summary>
        /// The card cannot be accessed because the maximum number of PIN entry attempts has been reached.
        /// </summary>
        SCARD_W_CHV_BLOCKED = 0x8010006C,
        /// <summary>
        /// The end of the smart card file has been reached.
        /// </summary>
        SCARD_W_EOF = 0x8010006D,
        /// <summary>
        /// The action was cancelled by the user.
        /// </summary>
        SCARD_W_CANCELLED_BY_USER = 0x8010006E,
        /// <summary>
        /// No PIN was presented to the smart card.
        /// </summary> 
        SCARD_W_CARD_NOT_AUTHENTICATED = 0x8010006F
    }

    /// <summary>
    /// SecureCard class which allows the user to access physical secure cards on the local machine and send commands to the cards.
    /// </summary>
    public class SCard : IDisposable 
    {
        private IntPtr _hContext = IntPtr.Zero;
        private IntPtr _hCard = IntPtr.Zero;
        private UInt32 _protocol = 0;

        /// <summary>
        /// Provides the list of readers eliminating duplicates.
        /// </summary>
        /// <remarks>
        /// These is a Window specific implementation and will not work on Linux under mono.
        /// 
        /// SmartCard readers may not be available by Windows when accessing the machine via terminal services.</remarks>
        /// <returns>An array of strings contains the name of each SmartCard reader.</returns>
        public string[] ListReaders()
        {
            if (_hContext == IntPtr.Zero)
                throw new SCardException("No card reader context established.  Must call SCard.EstablishContext() first.");
            
            // make the first call to get the size of the string we need to allocate and use in the second call
            UInt32 pchReaders = 0;
            int rtn1 = SCardDll.SCardListReadersA(_hContext, null, IntPtr.Zero, ref pchReaders);
            EvalReturnCode(rtn1);

            IntPtr szListReaders = IntPtr.Zero;
            StringBuilder b = new StringBuilder();
            try
            {
                // allocate an unmanaged string 
                szListReaders = Marshal.AllocHGlobal((int)pchReaders);
                int rtn2 = SCardDll.SCardListReadersA(_hContext, null, szListReaders, ref pchReaders);
                EvalReturnCode(rtn2);

                for (int i = 0; i < pchReaders; i++)
                {
                    b.Append((char)Marshal.ReadByte(szListReaders, i));
                }
            }
            finally
            {
                if (szListReaders != IntPtr.Zero)
                    Marshal.FreeHGlobal(szListReaders);
            }
            
            return b.ToString().Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Provides the list of named smart cards previously introduced to the system by the user.
        /// </summary>
        /// <remarks>SmartCards may not be available by Windows when accessing the machine via terminal services.</remarks>
        /// <returns>An array of strings contains the name of each SmartCard.</returns>
        public string[] ListCards()
        {
            if (_hContext == IntPtr.Zero)
                throw new SCardException("No card reader context established.  Must call SCard.EstablishContext() first.");

            // make the first call to get the size of the string we need to allocate and use in the second call
            UInt32 pchCards = 0;
            int rtn1 = SCardDll.SCardListCardsA(_hContext, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, ref pchCards);
            EvalReturnCode(rtn1);

            IntPtr szListCards = IntPtr.Zero;
            StringBuilder b = new StringBuilder();
            try
            {
                // allocate an unmanaged string 
                szListCards = Marshal.AllocHGlobal((int)pchCards);
                int rtn2 = SCardDll.SCardListCardsA(_hContext, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, szListCards, ref pchCards);
                EvalReturnCode(rtn2);

                for (int i = 0; i < pchCards; i++)
                {
                    b.Append((char)Marshal.ReadByte(szListCards, i));
                }
            }
            finally
            {
                if (szListCards != IntPtr.Zero)
                    Marshal.FreeHGlobal(szListCards);
            }

            return b.ToString().Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
        }
        

        /// <summary>
        /// Establishes the resource manager context (the scope) within which database operations are performed.
        /// </summary>
        /// <param name="Scope">Scope of the resource manager context.</param>
        public void EstablishContext(ScopeOptions Scope)
        {
            if (_hContext != IntPtr.Zero)
                throw new SCardException("Context already established.  Must call SCard.ReleaseContext() first.");
            IntPtr hContext = IntPtr.Zero;
            int rtn = SCardDll.SCardEstablishContext((UInt32)Scope, IntPtr.Zero, IntPtr.Zero, ref hContext);
            EvalReturnCode(rtn);
            _hContext = hContext;
        }
        
        /// <summary>
        /// Closes an established resource manager context, freeing any resources allocated under that context.
        /// </summary>
        public void ReleaseContext()
        {
            if (_hContext != IntPtr.Zero)
            {
                int rtn = SCardDll.SCardReleaseContext(_hContext);
                EvalReturnCode(rtn);
                _hContext = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Establishes a connection (using a specific resource manager context) between the calling application and a smart card contained by a specific reader. If no card exists in the specified reader, an error is returned.
        /// </summary>
        /// <param name="readerName">The name of the reader that contains the target card.</param>
        /// <param name="shareMode">A flag that indicates whether other applications may form connections to the card.</param>
        /// <param name="preferredProtocols">A bitmask of acceptable protocols for the connection. Possible values may be combined with the OR operation.</param>
        public void Connect(string readerName, ShareModeOptions shareMode, ProtocolOptions preferredProtocols)
        {
            if (_hContext == IntPtr.Zero)
                throw new SCardException("No card reader context established.  Must call SCard.EstablishContext() first.");
            if (_hCard != IntPtr.Zero)
                throw new SCardException("Already connected to card.  Must call SCard.Disconnect() first.");
            IntPtr hCard = IntPtr.Zero;
            int rtn = SCardDll.SCardConnectA(_hContext, readerName, (UInt32)shareMode, (UInt32)preferredProtocols, ref hCard, ref _protocol);
            EvalReturnCode(rtn);
            _hCard = hCard;
        }

        /// <summary>
        /// Terminates a connection previously opened between the calling application and a smart card in the target reader.
        /// </summary>
        /// <param name="disposition">Action to take on the card in the connected reader on close.</param>
        public void Disconnect(DispositionOptions disposition)
        {
            if (_hCard != IntPtr.Zero)
            {
                int rtn = SCardDll.SCardDisconnect(_hCard, (UInt32)disposition);
                EvalReturnCode(rtn);
                _hCard = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Sends a service request to the smart card and expects to receive data back from the card.
        /// </summary>
        /// <param name="apdu">The SmartCard ADPU command and response object.</param>
        /// <returns>adpu</returns>
        public SCardAdpu Transmit(SCardAdpu apdu)
        {
            if (_hContext == IntPtr.Zero)
                throw new SCardException("No card reader context established.  Must call SCard.EstablishContext() first.");
            if (_hCard == IntPtr.Zero)
                throw new SCardException("Not connected to card. Must call SCard.Connect() method first.");

            byte[] send = apdu.GetSendBuffer();
            byte[] recv = new byte[apdu.GetReceiveBufferSize()];
            int recvLen = recv.Length;  // this is an in/out value

            SCARD_IO_REQUEST ioRequest = new SCARD_IO_REQUEST();
            ioRequest.dwProtocol = (uint)_protocol;
            ioRequest.cbPciLength = 8;
            IntPtr pioRecvPci = IntPtr.Zero;

            Debug.WriteLine("SCardTransmit-ADPU-send: " + ArrayUtils.HexEncode(send));

            int rtn = SCardDll.SCardTransmit(_hCard, ref ioRequest, ref send[0], send.Length, pioRecvPci, ref recv[0], ref recvLen);
            EvalReturnCode(rtn);

            // resize the array if the bytes received are less than the buffer size
            if (recvLen < recv.Length)
                Array.Resize<byte>(ref recv, (int)recvLen);

            Debug.WriteLine("SCardTransmit-ADPU-recv: " + ArrayUtils.HexEncode(recv));

            // update the apdu object with the receive data
            apdu.SetReceiveData(recv);

            return apdu;
        }

        /// <summary>
        /// Waits for the completion of all other transactions before it begins. After the transaction starts, all other applications are blocked from accessing the smart card while the transaction is in progress.
        /// </summary>
        public void BeginTransaction()
        {
            if (_hContext == IntPtr.Zero)
                throw new SCardException("No card reader context established.  Must call SCard.EstablishContext() first.");
            if (_hCard == IntPtr.Zero)
                throw new SCardException("Not connected to card. Must call SCard.Connect() method first.");
            int rtn = SCardDll.SCardBeginTransaction(_hCard);
            EvalReturnCode(rtn);
        }

        /// <summary>
        /// Completes a previously declared transaction, allowing other applications to resume interactions with the card.
        /// </summary>
        /// <param name="disposition">Action to take on the card in the connected reader on close.</param>
        public void EndTransaction(DispositionOptions disposition)
        {
            if (_hContext == IntPtr.Zero)
                throw new SCardException("No card reader context established.  Must call SCard.EstablishContext() first.");
            if (_hCard == IntPtr.Zero)
                throw new SCardException("Not connected to card. Must call SCard.Connect() method first.");
            int rtn = SCardDll.SCardEndTransaction(_hCard, (UInt32)disposition);
            EvalReturnCode(rtn);
        }

        /// <summary>
        /// Retrieves the current reader attributes for the given handle. It does not affect the state of the reader, driver, or card.
        /// </summary>
        /// <param name="attribId">Identifier for the attribute to get.</param>
        /// <returns></returns>
        [CLSCompliantAttribute(false)] 
        public byte[] GetAttribute(uint attribId)
        {
            if (_hContext == IntPtr.Zero)
                throw new SCardException("No card reader context established.  Must call SCard.EstablishContext() first.");
            if (_hCard == IntPtr.Zero)
                throw new SCardException("Not connected to card. Must call SCard.Connect() method first.");
            
            byte[] attr = null;
            UInt32 attrLen = 0;

            // call get attrib to locate the attribute and receive the size of 
            // the buffer that needs to be allocated
            int rtn = SCardDll.SCardGetAttrib(_hCard, attribId, attr, ref attrLen);
            EvalReturnCode(rtn);
            
            if (attrLen != 0)
            {
                // allocate the buffer based on size returned by previous call
                attr = new byte[attrLen];
                // call again to get the actual data this time copied into our buffer
                int rtn2 = SCardDll.SCardGetAttrib(_hCard, attribId, attr, ref attrLen);
                EvalReturnCode(rtn2);
            }

            return attr;
        }


        /// <summary>
        /// Evaluate the return code from the SCard DLL.
        /// </summary>
        /// <param name="code">Code value.</param>
        private void EvalReturnCode(int code)
        {
            ActionReturnCodes acode = ActionReturnCodes.Unknown;
            acode = (ActionReturnCodes)code;
            if (acode == ActionReturnCodes.SCARD_S_SUCCESS)
                return;
            throw new SCardException(string.Format("Secure Card code error: {0} value={1}", acode.ToString(), code));
        }

        /// <summary>
        /// Dispose SCard object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            // Use SupressFinalize in case a subclass
            // of this type implements a finalizer.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Release any resources held by the SCard object.
        /// </summary>
        /// <param name="disposing">Flag indicating object is already disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Disconnect(DispositionOptions.LeaveCard);
                ReleaseContext();
            }
        }
    
    
    }
}
