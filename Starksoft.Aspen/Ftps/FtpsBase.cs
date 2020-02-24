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
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
using System.IO;
using System.Text;
using System.Threading;
using System.Globalization;
using System.ComponentModel;
using System.IO.Compression;
using System.Diagnostics;
using Starksoft.Aspen.Proxy;


namespace Starksoft.Aspen.Ftps
{
    #region Public/Internal Enums

    /// <summary>
    /// The type of data transfer mode (e.g. Active or Passive).
    /// </summary>
    /// <remarks>
    /// The default setting is Passive data transfer mode.  This mode is widely used as a
    /// firewall friendly setting for the FTP clients operating behind a firewall.
    /// </remarks>
    public enum TransferMode : int
    {
        /// <summary>
        /// Active transfer mode.  In this mode the FTP server initiates a connection to the client when transfering data.
        /// </summary>
        /// <remarks>This transfer mode may not work when the FTP client is behind a firewall and is accessing a remote FTP server.</remarks>
        Active,
        /// <summary>
        /// Passive transfer mode.  In this mode the FTP client initiates a connection to the server when transfering data.
        /// </summary>
        /// <remarks>
        /// This transfer mode is "firewall friendly" and generally allows an FTP client behind a firewall to access a remote FTP server.
        /// This mode is recommended for most data transfers.
        /// </remarks>
        Passive
    }

    /// <summary>
    /// Enumeration representing the type of integrity algorithm used to verify the integrity of the file after transfer and storage.
    /// </summary>
    public enum HashingAlgorithm : int
    {
        /// <summary>
        /// No algorithm slected.
        /// </summary>
        None,
        /// <summary>
        /// Cyclic redundancy check (CRC).  A CRC can be used in a similar way as a cryptogrphic hash to detect accidental 
        /// alteration of data during transmission or storage.  A CRC value does not provide a value for true file integrity value
        /// but rather can be used to detect transmission errors.  Use a cryptographic hashing function such as SHA-1 or higher
        /// for true cryptographic integrity.
        /// </summary>
        /// <remarks>
        /// It is often falsely assumed that when a message and its CRC are transmitted over an open channel, then when it arrives 
        /// if the CRC matches the message's calculated CRC then the message cannot have been altered in transit.  A CRC value 
        /// is produced from a cryptographic hashing algorithm but is rather just a simple computational check value.
        /// For this reason it is recommended to use a cryptographic hashing function such as SHA-1 or higher whenever possible.
        /// For new systems it is recommend to use the SHA-2 cryptographic hashing functions.
        /// </remarks>
        /// <seealso cref="Md5"/>
        /// <seealso cref="Sha1"/>
        /// <seealso cref="Sha256"/>
        /// <seealso cref="Sha512"/>
        Crc32,
        /// <summary>
        /// Message-Digest algorithm 5 (MD5).  Hashing function used to produce a 'unique' signature to detect 
        /// alternation of data during transmission or storage.
        /// </summary>
        /// <remarks>
        /// MD5 is a weak algorithm and has been show to produce collisions.  For this reason it is recommended to use SHA-1 or higher when possible.
        /// </remarks>
        /// <seealso cref="Crc32"/>
        /// <seealso cref="Sha1"/>
        /// <seealso cref="Sha256"/>
        /// <seealso cref="Sha512"/>
        Md5,
        /// <summary>
        /// Secure Hash Algorithm (SHA-1).  Cryptographic hash functions designed by the National Security Agency (NSA) and published by the NIST as a U.S. Federal Information Processing Standard.
        /// </summary>
        /// <remarks>
        /// The SHA-1 hashing algorithm has been shown to be theoritically vunerable to a mathematical weakness therefore the SHA-2 functions are recommended on new projects.
        /// </remarks>
        /// <seealso cref="Md5"/>
        /// <seealso cref="Crc32"/>
        /// <seealso cref="Sha256"/>
        /// <seealso cref="Sha512"/>
        Sha1,
        /// <summary>
        /// Secure Hash Algorithm (SHA-2).  Cryptographic hash functions designed by the National Security Agency (NSA) and published by the NIST as a U.S. Federal Information Processing Standard.
        /// </summary>
        /// <remarks>
        /// The SHA-1 hashing algorithm has been shown to be theoritically vunerable to a mathematical weakness therefore the SHA-2 functions are recommended on new projects.
        /// </remarks>
        /// <seealso cref="Md5"/>
        /// <seealso cref="Crc32"/>
        /// <seealso cref="Sha1"/>
        /// <seealso cref="Sha512"/>
        Sha256,
        /// <summary>
        /// Secure Hash Algorithm (SHA-2).  Cryptographic hash functions designed by the National Security Agency (NSA) and published by the NIST as a U.S. Federal Information Processing Standard.
        /// </summary>
        /// <remarks>
        /// The SHA-1 hashing algorithm has been shown to be theoritically vunerable to a mathematical weakness therefore the SHA-2 functions are recommended on new projects.
        /// </remarks>
        /// <seealso cref="Md5"/>
        /// <seealso cref="Crc32"/>
        /// <seealso cref="Sha1"/>
        /// <seealso cref="Sha256"/>
        Sha512
    }

    /// <summary>
    /// Defines the possible versions of FtpSecurityProtocol.
    /// </summary>
    public enum FtpsSecurityProtocol : int
    {
        /// <summary>
        /// No security protocol specified.
        /// </summary>
        None,
        /// <summary>
        /// Specifies Transport Layer Security (TLS) version 1.2 is required to secure communciations in explicit mode.  The TLS 1.2 protocol is defined in IETF RFC 5246 and supercedes the TLS 1.1 protocol.
        /// </summary>
        /// <remarks>
        /// The AUTH TLS command is sent to the FTP server to secure the connection.  
        /// </remarks>
        Tls12Explicit,
        /// <summary>
        /// Specifies Transport Layer Security (TLS) version 1.1 is required to secure communciations in explicit mode.  The TLS 1.1 protocol is defined in IETF RFC 4346 and supercedes the TLS 1.0 protocol.
        /// </summary>
        /// <remarks>
        /// The AUTH TLS command is sent to the FTP server to secure the connection.  
        /// </remarks>
        Tls11Explicit,
        /// <summary>
        /// Specifies Transport Layer Security (TLS) version 1.0 is required to secure communciations.  The TLS protocol is defined in IETF RFC 2246 and supercedes the SSL 3.0 protocol.
        /// </summary>
        /// <remarks>
        /// The AUTH TLS command is sent to the FTP server to secure the connection.  TLS protocol is the latest version of the SSL protcol and is the security protocol that should be used whenever possible.
        /// There are slight differences between SSL version 3.0 and TLS version 1.0, but the protocol remains substantially the same.
        /// </remarks>
        Tls1Explicit,
        /// <summary>
        /// Specifies Transport Layer Security (TLS) version 1.0. or Secure Socket Layer (SSL) version 3.0 is acceptable to secure communications in explicit mode.
        /// </summary>
        /// <remarks>
        /// The AUTH SSL command is sent to the FTP server to secure the connection but the security protocol is negotiated between the server and client.  
        /// TLS protocol is the latest version of the SSL 3.0 protcol and is the security protocol that should be used whenever possible.
        /// There are slight differences between SSL version 3.0 and TLS version 1.0, but the protocol remains substantially the same.
        /// </remarks>
        Tls1OrSsl3Explicit,
        /// <summary>
        /// Specifies Secure Socket Layer (SSL) version 3.0 is required to secure communications in explicit mode.  SSL 3.0 has been superseded by the TLS protocol
        /// and is provided for backward compatibility only
        /// </summary>
        /// <remarks>
        /// The AUTH SSL command is sent to the FTP server to secure the connection.  TLS protocol is the latest version of the SSL 3.0 protcol and is the security protocol that should be used whenever possible.
        /// There are slight differences between SSL version 3.0 and TLS version 1.0, but the protocol remains substantially the same.
        /// Some FTP server do not implement TLS or understand the command AUTH TLS.  In those situations you should specify the security
        /// protocol Ssl3, otherwise specify Tls1.
        /// </remarks>
        Ssl3Explicit,
        /// <summary>
        /// Specifies Secure Socket Layer (SSL) version 2.0 is required to secure communications in explicit mode.  SSL 2.0 has been superseded by the TLS protocol
        /// and is provided for backward compatibility only.  SSL 2.0 has several weaknesses and should only be used with legacy FTP server that require it.
        /// </summary>
        /// <remarks>
        /// The AUTH SSL command is sent to the FTP server to secure the connection.  TLS protocol is the latest version of the SSL 3.0 protcol and is the security protocol that should be used whenever possible.
        /// There are slight differences between SSL version 3.0 and TLS version 1.0, but the protocol remains substantially the same.
        /// Some FTP server do not implement TLS or understand the command AUTH TLS.  In those situations you should specify the security
        /// protocol Ssl3, otherwise specify Tls1.
        /// </remarks>
        Ssl2Explicit,
        /// <summary>
        /// Specifies Transport Layer Security (TLS) version 1.2 is required to secure communciations in explicit mode.  The TLS 1.2 protocol is defined in IETF RFC 5246 and supercedes the TLS 1.1 protocol.
        /// </summary>
        /// <remarks>
        /// The AUTH TLS command is sent to the FTP server to secure the connection.  TLS protocol is the latest version of the SSL protcol and is the security protocol that should be used whenever possible.
        /// There are slight differences between SSL version 3.0 and TLS version 1.0, but the protocol remains substantially the same.
        /// </remarks>
        Tls12Implicit,
        /// <summary>
        /// Specifies Transport Layer Security (TLS) version 1.1 is required to secure communciations in explicit mode.  The TLS 1.1 protocol is defined in IETF RFC 4346 and supercedes the TLS 1.0 protocol.
        /// </summary>
        /// <remarks>
        /// The AUTH TLS command is sent to the FTP server to secure the connection.  TLS protocol is the latest version of the SSL protcol and is the security protocol that should be used whenever possible.
        /// There are slight differences between SSL version 3.0 and TLS version 1.0, but the protocol remains substantially the same.
        /// </remarks>
        Tls11Implicit,
        /// <summary>
        /// Specifies Transport Layer Security (TLS) version 1.0 is required to secure communciations in explicit mode.  The TLS protocol is defined in IETF RFC 2246 and supercedes the SSL 3.0 protocol.
        /// </summary>
        /// <remarks>
        /// The AUTH TLS command is sent to the FTP server to secure the connection.  TLS protocol is the latest version of the SSL protcol and is the security protocol that should be used whenever possible.
        /// There are slight differences between SSL version 3.0 and TLS version 1.0, but the protocol remains substantially the same.
        /// </remarks>
        Tls1Implicit,
        /// <summary>
        /// Specifies Transport Layer Security (TLS) version 1.0. or Secure Socket Layer (SSL) version 3.0 is acceptable to secure communications in implicit mode.
        /// </summary>
        /// <remarks>
        /// The AUTH SSL command is sent to the FTP server to secure the connection but the security protocol is negotiated between the server and client.  
        /// TLS protocol is the latest version of the SSL 3.0 protcol and is the security protocol that should be used whenever possible.
        /// There are slight differences between SSL version 3.0 and TLS version 1.0, but the protocol remains substantially the same.
        /// </remarks>
        Tls1OrSsl3Implicit,
        /// <summary>
        /// Specifies Secure Socket Layer (SSL) version 3.0 is required to secure communications in implicit mode.  SSL 3.0 has been superseded by the TLS protocol
        /// and is provided for backward compatibility only
        /// </summary>
        /// <remarks>
        /// The AUTH SSL command is sent to the FTP server to secure the connection.  TLS protocol is the latest version of the SSL 3.0 protcol and is the security protocol that should be used whenever possible.
        /// There are slight differences between SSL version 3.0 and TLS version 1.0, but the protocol remains substantially the same.
        /// Some FTP server do not implement TLS or understand the command AUTH TLS.  In those situations you should specify the security
        /// protocol Ssl3, otherwise specify Tls1.
        /// </remarks>
        Ssl3Implicit,
        /// <summary>
        /// Specifies Secure Socket Layer (SSL) version 2.0 is required to secure communications in implicit mode.  SSL 2.0 has been superseded by the TLS protocol
        /// and is provided for backward compatibility only.  SSL 2.0 has several weaknesses and should only be used with legacy FTP server that require it.
        /// </summary>
        /// <remarks>
        /// The AUTH SSL command is sent to the FTP server to secure the connection.  TLS protocol is the latest version of the SSL 3.0 protcol and is the security protocol that should be used whenever possible.
        /// There are slight differences between SSL version 3.0 and TLS version 1.0, but the protocol remains substantially the same.
        /// Some FTP server do not implement TLS or understand the command AUTH TLS.  In those situations you should specify the security
        /// protocol Ssl3, otherwise specify Tls1.
        /// </remarks>
        Ssl2Implicit
    }

    /// <summary>
    /// IP network protocol options.
    /// </summary>
    public enum NetworkVersion : int
    {
        /// <summary>
        /// Internet Protocol version 4. 
        /// </summary>
        /// <remarks>
        /// IPv4 is the most widely deployed IP addressing protocol used for Internet communications.  A typical IP v4 address uses dot notation such as 192.168.10.4.  The IPv4 protocol has 
        /// limitations on addressing space as well as other issues and has been superceeded by the IPv6 protocol.  When choosing the IPv6 protocol for FTP communcations, the Starksoft
        /// FtpsClient will use the standard port and passive commands PORT/PASV.  Unless required, choose IPv4 for your IP network protocol addressing.
        /// </remarks>
        IPv4,
        /// <summary>
        /// Internet Protocol version 6.
        /// </summary>
        /// <remarks>
        /// IPv6 is the newest Internet protcol version and is gaining adoption.  A typical IP v6 address uses colon notation such as 2001:0db8:85a3:0042:0000:8a2e:0370:7334.  The IPv4 protocol has 
        /// limitations on addressing space as well as other issues and has been superceeded by the IPv6 protocol.  When choosing the IPv6 protocol for FTP communcations, the Starksoft
        /// FtpsClient will use the extended port and extended passive commands EPRT/EPSV which support IPv6 addressing.  Not all FTP servers support these extended commands.
        /// Unless required, choose IPv4 for your IP network protocol addressing for highest compatability with existing FTP servers.
        /// </remarks>
        IPv6
    }

    /// <summary>
    /// The data transfer directory.
    /// </summary>
    internal enum TransferDirection : int
    {
        /// <summary>
        /// Transfer data from server to client.
        /// </summary>
        ToClient,
        /// <summary>
        /// Transfer data from client to server.
        /// </summary>
        ToServer
    }

    #endregion

    #region  Public FTP Response Code Enum

    /// <summary>
    /// Enumeration representing all the various response codes from a FTP server.
    /// </summary>
    public enum FtpsResponseCode : int
    {
        /// <summary>
        /// No response was received from the server.
        /// </summary>
        None = 0,
        /// <summary>
        /// The command was executed sucessfully (200).
        /// </summary>
        CommandOkay = 200,
        /// <summary>
        /// A syntax error occurred because the command was not recognized (500).
        /// </summary>
        SyntaxErrorCommandUnrecognized = 500,
        /// <summary>
        /// A syntax error occurred because the input parameters or arguments for the command are invalid (501).
        /// </summary>
        SyntaxErrorInParametersOrArguments = 501,
        /// <summary>
        /// The command is considered superfluous and not implemented by the FTP server (202).
        /// </summary>
        CommandNotImplementedSuperfluousAtThisSite = 202,
        /// <summary>
        /// The command is not implement by the FTP server (502).
        /// </summary>
        CommandNotImplemented = 502,
        /// <summary>
        /// A bad sequence of commands was issued (503).
        /// </summary>
        BadSequenceOfCommands = 503,
        /// <summary>
        /// The command does not support the supplied parameter (504).
        /// </summary>
        CommandNotImplementedForThatParameter = 504,
        /// <summary>
        /// Restart marker reply (110).  MARK yyyy = mmmm  Where yyyy is User-process data 
        /// stream marker, and mmmm server's equivalent marker (note the spaces between
        /// markers and "=").
        /// </summary>
        RestartMarkerReply = 110,
        /// <summary>
        /// System status or system help reply (211).
        /// </summary>
        SystemStatusOrHelpReply = 211,
        /// <summary>
        /// Directory status (212).
        /// </summary>
        DirectoryStatus = 212,
        /// <summary>
        /// File status (213).
        /// </summary>
        FileStatus = 213,
        /// <summary>
        /// Help message (214).  On how to use the server or the meaning of a particular
        /// non-standard command.  This reply is useful only to the human user.
        /// </summary>
        HelpMessage = 214,
        /// <summary>
        /// Name system type where Name is an official system name from the list in the
        /// Assigned Numbers document (215).
        /// </summary>
        NameSystemType = 215,
        /// <summary>
        /// Service ready in xxx minutes (120).
        /// </summary>
        ServiceReadyInxxxMinutes = 120,
        /// <summary>
        /// Service is now ready for new user (220).
        /// </summary>
        ServiceReadyForNewUser = 220,
        /// <summary>
        /// Service is closing control connection (221).
        /// </summary>
        ServiceClosingControlConnection = 221,
        /// <summary>
        /// Service not available, closing control connection (421). This may be a reply to any 
        /// command if the service knows it must shut down.
        /// </summary>
        ServiceNotAvailableClosingControlConnection = 421,
        /// <summary>
        /// Data connection already open; transfer starting (125).
        /// </summary>
        DataConnectionAlreadyOpenSoTransferStarting = 125,
        /// <summary>
        /// Data connection open so no transfer in progress (225).
        /// </summary>
        DataConnectionOpenSoNoTransferInProgress = 225,
        /// <summary>
        /// cannot open data connection (425).
        /// </summary>
        CannotOpenDataConnection = 425,
        /// <summary>
        /// Requested file action successful (for example, file transfer or file abort) (226).
        /// </summary>
        ClosingDataConnection = 226,
        /// <summary>
        /// Connection closed therefore the transfer was aborted (426).
        /// </summary>
        ConnectionClosedSoTransferAborted = 426,
        /// <summary>
        /// Entering Passive Mode (h1,h2,h3,h4,p1,p2) (227).
        /// </summary>
        EnteringPassiveMode = 227,
        /// <summary>
        /// User logged in, proceed (230).
        /// </summary>
        UserLoggedIn = 230,
        /// <summary>
        /// User is not logged in.  Command not accepted (530).
        /// </summary>
        NotLoggedIn = 530,
        /// <summary>
        /// The user name was accepted but the password must now be supplied (331).
        /// </summary>
        UserNameOkayButNeedPassword = 331,
        /// <summary>
        /// An account is needed for login (332).
        /// </summary>
        NeedAccountForLogin = 332,
        /// <summary>
        /// An account is needed for storing file on the server (532).
        /// </summary>
        NeedAccountForStoringFiles = 532,
        /// <summary>
        /// File status okay; about to open data connection (150).
        /// </summary>
        FileStatusOkaySoAboutToOpenDataConnection = 150,
        /// <summary>
        /// Requested file action okay, completed (250).
        /// </summary>
        RequestedFileActionOkayAndCompleted = 250,
        /// <summary>
        /// The pathname was created (257).
        /// </summary>
        PathNameCreated = 257,
        /// <summary>
        /// Requested file action pending further information (350).
        /// </summary>
        RequestedFileActionPendingFurtherInformation = 350,
        /// <summary>
        /// Requested file action not taken (450).  
        /// </summary>
        RequestedFileActionNotTaken = 450,
        /// <summary>
        /// Requested file action not taken (550).  File unavailable (e.g., file busy).
        /// </summary>
        RequestedActionNotTakenFileUnavailable = 550,
        /// <summary>
        /// Requested action aborted (451). Local error in processing.
        /// </summary>
        RequestedActionAbortedDueToLocalErrorInProcessing = 451,
        /// <summary>
        /// Requested action aborted (551). Page type unknown.
        /// </summary>
        RequestedActionAbortedPageTypeUnknown = 551,
        /// <summary>
        /// Requested action not taken (452).  Insufficient storage space in system.
        /// </summary>
        RequestedActionNotTakenInsufficientStorage = 452,
        /// <summary>
        /// Requested file action aborted (552).  Exceeded storage allocation (for current directory or dataset).
        /// </summary>
        RequestedFileActionAbortedExceededStorageAllocation = 552,
        /// <summary>
        /// Requested action not taken (553).  File name not allowed.
        /// </summary>
        RequestedActionNotTakenFileNameNotAllowed = 553,
        /// <summary>
        /// Secure authentication Okay (234).
        /// </summary>
        AuthenticationCommandOkay = 234,
        /// <summary>
        /// This reply indicates that the requested security mechanism
        /// is ok, and includes security data to be used by the client
        /// to construct the next command.  The square brackets are not
        /// to be included in the reply, but indicate that
        /// security data in the reply is optional.  (334)
        /// </summary>
        /// <remarks>
        /// Example:
        ///      334 [ADAT=base64data]
        ///      ADAT base64data
        /// 
        /// See http://www.ietf.org/rfc/rfc2228.txt for more information.
        /// </remarks>
        AuthenticationCommandOkaySecurityDataOptional = 334,
        /// <summary>
        /// SSL service is Unavailable (431).
        /// </summary>
        ServiceIsUnavailable = 431
    }
    #endregion
    
    /// <summary>
    /// Base abstract class for FtpsClient.  Implements FTP network protocols.
    /// </summary>
    public abstract class FtpsBase : IDisposable
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the FtpNetworkAdapter class.
        /// </summary>
        /// <param name="port">Port number the adapter is to communicate on.</param>
        /// <param name="securityProtocol">Value indicating what secure security communications protocol should be used (if any).</param>
        internal FtpsBase(int port, FtpsSecurityProtocol securityProtocol)
        {
            _port = port;
            _securityProtocol = securityProtocol;
        }

        /// <summary>
        /// Initializes a new instance of the FtpNetworkAdapter class.
        /// </summary>
        /// <param name="host">Host the adapter is to communicate on.</param>
        /// <param name="port">Port number the adapter is to communicate on.</param>
        /// <param name="securityProtocol">Value indicating what secure security communications protocol should be used (if any).</param>
        internal FtpsBase(string host, int port, FtpsSecurityProtocol securityProtocol)
		{
			_host = host;
			_port = port;
            _securityProtocol = securityProtocol;
        }

        #endregion

        #region Private Variables and Constants

        // connection objects
        private NetworkVersion _networkProtocol = NetworkVersion.IPv4;  // default is IPv4
        private TcpClient _commandConn;
        private Stream _commandStream;
        private TcpClient _dataConn;

        // connection settings and buffers
        private int _port;
        private string _host;
        private string _client;                 // optional client IP address used with Active mode connections
        private TransferMode _dataTransferMode = TransferMode.Passive;  // default is passive mode
        private byte[] _dataBuffer;
        private long _transferSize = -1;             // transfer size in bytes - default to -1 

        // response queue objects
        private FtpsResponseQueue _responseQueue = new FtpsResponseQueue();
        private FtpsResponse _response = new FtpsResponse();
        private FtpsResponseCollection _responseList = new FtpsResponseCollection();
        
        // response monitor thread
        private Thread _responseMonitor;
        static object _reponseMonitorLock = new object();

        // proxy object 
        private IProxyClient _proxy;

        // features
        private FtpsFeatureCollection _feats;

        // general TCP/IP settings
        private int _tcpBufferSize = TCP_BUFFER_SIZE;
        private int _tcpTimeout = TCP_TIMEOUT;
        private int _transferTimeout = TRANSFER_TIMEOUT;
        private int _commandTimeout = COMMAND_TIMEOUT;

        // active data connection objects and settings
        private TcpListener _activeListener;
        private int _activePort;
        private int _activePortRangeMin = 50000;
        private int _activePortRangeMax = 50080;
        
        // secure communications specific 
        private FtpsSecurityProtocol _securityProtocol = FtpsSecurityProtocol.None;   // default is no encryption
        private X509Certificate2 _serverCertificate;
        private X509CertificateCollection _clientCertificates = new X509CertificateCollection();
        private bool _alwaysAcceptServerCertificate = false;    

        // data compresion specific
        private bool _isCompressionEnabled = false;  // default is no compression

        // SSL/TSL PROT P command option
        // default requirement for implicit TLS for Filezilla
        private bool _sendProtPForImplicitSslConnections = true;

        // data hashing specific
        private HashingAlgorithm _autoHashAlgorithm = HashingAlgorithm.None; // default is no compression (zlib)

        // character encoding object used by the client to send and receive data
        private Encoding _encode = Encoding.UTF8;   // default always to UTF-8 which is standards compliant

        // thread signal for active mode data transfer
        private ManualResetEvent _activeSignal = new ManualResetEvent(false);

        // async background worker event-based object
        private BackgroundWorker _asyncWorker;
        private bool _asyncCanceled;

        // syncronization objects
        private object _cmdStreamWriteLock = new Object();

        // data throttle settings
        private int _maxUploadSpeed;
        private int _maxDownloadSpeed;

        // constant definitions
        private const int TCP_BUFFER_SIZE = 8192; 
        private const int TCP_TIMEOUT = 30000; // 30 seconds
        private const int WAIT_FOR_DATA_INTERVAL = 10; // 10 ms
        private const int WAIT_FOR_COMMAND_RESPONSE_INTERVAL = 20; // 20 ms
        private const int TRANSFER_TIMEOUT = 30000; // 30 seconds
        private const int COMMAND_TIMEOUT = 30000; // 30 seconds
        private const string HASH_CRC_32 = "CRC-32";
        private const string HASH_MD5 = "MD5";
        private const string HASH_SHA_1 = "SHA-1";
        private const string HASH_SHA_256 = "SHA-256";
        private const string HASH_SHA_512 = "SHA-512";


        #endregion

        #region Public Events

        /// <summary>
        /// Server response event.
        /// </summary>
        public event EventHandler<FtpsResponseEventArgs> ServerResponse;

        /// <summary>
        /// Server request event.
        /// </summary>
        public event EventHandler<FtpsRequestEventArgs> ClientRequest;

        /// <summary>
        /// Data transfer progress event.
        /// </summary>
        public event EventHandler<TransferProgressEventArgs> TransferProgress;

        /// <summary>
        /// Data transfer complete event.
        /// </summary>
        public event EventHandler<TransferCompleteEventArgs> TransferComplete;

        /// <summary>
        /// Security certificate authentication event.
        /// </summary>
        public event EventHandler<ValidateServerCertificateEventArgs> ValidateServerCertificate;
        
        /// <summary>
        /// Connection closed event.
        /// </summary>
        public event EventHandler<ConnectionClosedEventArgs> ConnectionClosed;

        #endregion

        #region Public Methods

        /// <summary>
        /// Cancels any asychronous operation that is currently active.
        /// </summary>
        /// <seealso cref="FtpsClient.FxpCopyAsync"/>
        /// <seealso cref="FtpsClient.GetDirListAsync()"/>
        /// <seealso cref="FtpsClient.GetDirListAsync(string)"/>
        /// <seealso cref="FtpsClient.GetDirListDeepAsync()"/>
        /// <seealso cref="FtpsClient.GetDirListDeepAsync(string)"/>
        /// <seealso cref="FtpsClient.GetFileAsync(string, string, FileAction)"/>
        /// <seealso cref="FtpsClient.GetFileAsync(string, Stream, bool)"/>
        /// <seealso cref="FtpsClient.OpenAsync"/>
        /// <seealso cref="FtpsClient.PutFileAsync(string, string, FileAction)"/>
        /// <seealso cref="FtpsClient.PutFileAsync(string, FileAction)"/>
        public void CancelAsync()
        {
            if (_asyncWorker != null && !_asyncWorker.CancellationPending && _asyncWorker.IsBusy)
            {
                _asyncCanceled = true;
                _asyncWorker.CancelAsync();
            }
        }

        /// <summary>
        /// Gets the hash value from the FTP server for the file specified.  
        /// Use this value to compare a local hash value to determine file integrity.
        /// </summary>
        /// <param name="algorithm">Hashing function to use.</param>
        /// <param name="path">Path to the file ont the remote FTP server.</param>
        /// <returns>Hash value in a string format.</returns>
        /// <seealso cref="ComputeHash(HashingAlgorithm, string)"/>
        public string GetHash(HashingAlgorithm algorithm, string path)
        {
            return GetHash(algorithm, path, 0, 0);
        }

        /// <summary>
        /// Gets the hash value from the FTP server for the file specified.  
        /// Use this value to compare a local hash to determine file integrity.
        /// </summary>
        /// <param name="algorithm">Hashing function to use.</param>
        /// <param name="path">Path to the file on the remote FTP server.</param>
        /// <param name="startPosition">Byte position of where the server should begin computing the hash.</param>
        /// <param name="endPosition">Byte position of where the server should end computing the hash.</param>
        /// <returns>Hash value in a string format.</returns>
        /// <remarks>
        /// FTP file hashing is not a fully ratified RFC standard and therefore is not widely supported.  There is
        /// a convention that some FTP server support that allows for partial file hashing.  The command that allow
        /// partial file hashing are XCRC, XMD5, XSHA1, XSHA256, XSHA512 as well as other variations not listed.
        /// When computing a CRC, MD5 or SHA1 on a server that supports the commands, an optional startPosition
        /// and endPosition value can also be specified.  These optional parameters allow for partial file hashing 
        /// which can be used while resuming a file transfer. In that way only the additional bytes are hashed 
        /// for accuracy and not the entire file on the FTP server.
        /// 
        /// The second FTP file hashing implementation is a RFC draft specification and uses a new FTP command named HASH.
        /// The default hashing algorithm can be set with the OPTS HASH command specifying the supported algorithm as an argument.
        /// Optionally, the HASH command supports partial file hashing with another command called RANG.  The RANG command
        /// requires a byte range to use when calculating the file hash.  Partial file hashing with HASH is only supported if the 
        /// command RANG is also a supported.  The HASH command also has several additional return error codes.  For example, 
        /// an FTP server that implements the HASH command should reply with a 450 reply if the server is busy.  This signals
        /// that the client can try again some time later.  In addition, an FTP server that implements the HASH command should reply with 
        /// a 501 reply to the OPTS HASH command if the user has requested an unknown or unsupported algorithm.
        /// 
        /// The Starksoft FtpsClient will attempt to determine which hashing command features are enabled on the FTP server 
        /// and formulate the hashing request accordingly.  The default hashing command is always HASH.  If HASH is not available, the 
        /// FtpsClient will attempt to execute the appropriate alternative XCRC, XMD5, XSHA1, XSHA256 or XSHA512 command.  If the
        /// alternate command is not supported then a FileHashingException will be thrown.
        /// 
        /// For partial file hashing, if HASH is supported but RANG is not, the FtpsClient will attempted to execute the approriate XCRC, XMD5, XSHA1, 
        /// XSHA256 or XSHA512 command instead.  If the FTP server does not support the alternate hashing command then an exception will be thrown.
        /// 
        /// See RFC draft-ietf-ftpext2-hash-03 for more information            
        /// http://tools.ietf.org/html/draft-ietf-ftpext2-hash-03#section-3.2
        /// </remarks>
        /// <seealso cref="ComputeHash(HashingAlgorithm, string)"/>
        public string GetHash(HashingAlgorithm algorithm, string path, long startPosition, long endPosition)
        {
            if (algorithm == HashingAlgorithm.None)
                throw new ArgumentOutOfRangeException("algorithm", "must contain a value other than 'Unknown'");
            if (startPosition < 0)
                throw new ArgumentOutOfRangeException("startPosition", "must contain a value greater than or equal to 0");
            if (endPosition < 0)
                throw new ArgumentOutOfRangeException("startPosition", "must contain a value greater than or equal to 0");
            if (startPosition > endPosition)
                throw new ArgumentOutOfRangeException("startPosition", "must contain a value less than or equal to endPosition");

            // gather information about the hashing methods supported on the FTP server
            string hashArgText = ConvertHashAlgoToTextArg(algorithm);
            FtpsCmd hashXCmd = ConvertHashXAlgoToTextCmd(algorithm);
            FtpsFeature hf = Features.Find(FtpsCmd.Hash);
            FtpsFeatureArgument hfa = null;
            if (hf != null)
                hfa = hf.Arguments.Find(hashArgText);
            FtpsFeature rf = Features.Find(FtpsCmd.Rang);

            bool partialReq = startPosition > 0 ? true : false;
            bool canHash = hf != null && hfa != null ? true : false;
            bool canHashPartial = rf != null ? true : false;


            if (partialReq && (!canHash || !canHashPartial)
                || (!partialReq && !canHash))
            {
                try
                {
                    if (partialReq)
                    {
                        SendRequest(new FtpsRequest(_encode,
                            hashXCmd,
                            path,
                            startPosition.ToString(),
                            endPosition.ToString()));
                    }
                    else
                    {
                        SendRequest(new FtpsRequest(_encode,
                            ConvertHashXAlgoToTextCmd(algorithm),
                            path));
                    }
                }
                catch (FtpsException x)
                {
                    throw new FtpsHashingException(String.Format("Unable to partial hash file using {0} command.", 
                        hashXCmd.ToString().ToUpper()), x);
                }
            }
            else
            {
                try
                {
                    // set the hash function if it is not already set
                    if (!hfa.IsDefault)
                        SetHashOption(algorithm);

                    if (partialReq)
                    {
                        // send RANGe command if it is a partial hash
                        SendRequest(new FtpsRequest(_encode,
                            FtpsCmd.Rang,
                            startPosition.ToString(),
                            endPosition.ToString()));
                    }

                    // send the HASH command 
                    SendRequest(new FtpsRequest(_encode,
                        FtpsCmd.Hash,
                        path));
                }
                catch (FtpsException ex2)
                {
                    if (_response.Code == FtpsResponseCode.SyntaxErrorInParametersOrArguments)
                        throw new FtpsHashingInvalidAlgorithmException("FTP server does not support selected hashing algorithm.");
                    else if (_response.Code == FtpsResponseCode.RequestedFileActionNotTaken)
                        throw new FtpsHashingServerBusyException("FTP server is busy.  Please try again later.");
                    else
                        throw new FtpsHashingException("Unable to partial hash file using HASH command.", ex2);
                }

            }

            return _response.Text;
        }
        
        /// <summary>
        /// Sets the hashing algorithm option on the remote FTP server.
        /// </summary>
        /// <remarks>
        /// Not all FTP servers support the HASH feature.
        /// </remarks>
        /// <param name="algorithm">Hasing algorithm to use</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="FtpsCommandNotSupportedException"></exception>
        /// <exception cref="FtpsHashingException"></exception>
        public void SetHashOption(HashingAlgorithm algorithm)
        {
            if (algorithm == HashingAlgorithm.None)
                throw new ArgumentOutOfRangeException("algorithm");
            if (!_feats.Contains(FtpsCmd.Hash))
                throw new FtpsCommandNotSupportedException("Cannot set the HASH option on FTP server.", FtpsCmd.Hash);

            // attempt to get the hash argument feature from the features collection
            // if the feature or algorithm is not available an exception will be thrown
            GetHashFeatureArgument(algorithm);

            string hashArgText = ConvertHashAlgoToTextArg(algorithm);

            // attempt to send a request to change the option for the HASH function
            string hashCmd = String.Format("HASH {0}", hashArgText);
            try
            {
                SendRequest(new FtpsRequest(_encode, FtpsCmd.Opts, hashCmd));
            }
            catch (FtpsException ex)
            {
                throw new FtpsHashingException("Unable to set hashing option.", ex);
            }

            // refresh the features collection
            TryResetFeatures();
            // find the Hash feature argument agains and verify the option has been set properly
            FtpsFeatureArgument fa = GetHashFeatureArgument(algorithm);    
            if (!fa.IsDefault)
                throw new FtpsHashingException("Hashing option not set to default by FTP server.");
        }

        /// <summary>
        /// Computes a cryptographic hash or CRC value for a local file.
        /// </summary>
        /// <param name="hash">Hashing function to use.</param>
        /// <param name="localPath">Path to file to perform hashing operation on.</param>
        /// <returns>Hash value in a string format.</returns>
        /// <seealso cref="GetHash(HashingAlgorithm, string)"/>
        public string ComputeHash(HashingAlgorithm hash, string localPath)
        {
            if (!File.Exists(localPath))
                throw new ArgumentException("file does not exist.", "localPath");
            
            using (FileStream fileStream = File.OpenRead(localPath))
            {
                return ComputeHash(hash, fileStream);
            }
        }

        /// <summary>
        /// Computes a hash value for a Stream object.
        /// </summary>
        /// <param name="hash">Hashing function to use.</param>
        /// <param name="inputStream">Any System.IO.Stream object.</param>
        /// <returns>Hash value in a string format.</returns>
        /// <remarks>
        /// The Stream object must allow reads and must allow seeking.
        /// </remarks>
        /// <seealso cref="GetHash(HashingAlgorithm, string)"/>
        public string ComputeHash(HashingAlgorithm hash, Stream inputStream)
        {
            return ComputeHash(hash, inputStream, 0);
        }

        /// <summary>
        /// Computes a hash value for a Stream object at a specific start position.
        /// </summary>
        /// <param name="hash">Hashing function to use.</param>
        /// <param name="inputStream">Any System.IO.Stream object.</param>
        /// <param name="startPosition">Byte position of where the hash computation should begin.</param>
        /// <returns>Hash value in a string format.</returns>
        /// <remarks>
        /// The Stream object must allow reads and must allow seeking.
        /// </remarks>
        /// <seealso cref="GetHash(HashingAlgorithm, string)"/>
        public static string ComputeHash(HashingAlgorithm hash, Stream inputStream, long startPosition)
        {
            if (inputStream == null)
                throw new ArgumentNullException("inputStream");
            if (!inputStream.CanRead)
                throw new ArgumentException("must be readable.  The CanRead property must return a value of 'true'.", "inputStream");
            if (!inputStream.CanSeek)
                throw new ArgumentException("must be seekable.  The CanSeek property must return a value of 'true'.", "inputStream");
            if (startPosition < 0)
                throw new ArgumentOutOfRangeException("startPosition", "must contain a value greater than or equal to 0");

            HashAlgorithm hashAlgo = null;

            switch (hash)
            {
                case HashingAlgorithm.Crc32:
                    hashAlgo = new Starksoft.Aspen.Crc32();
                    break;
                case HashingAlgorithm.Md5:
                    hashAlgo = new MD5CryptoServiceProvider();
                    break;
                case HashingAlgorithm.Sha1:
                    hashAlgo = new SHA1CryptoServiceProvider();
                    break;
                case HashingAlgorithm.Sha256:
#if CLR_4_PLUS
                    hashAlgo = new SHA256CryptoServiceProvider();
                    break;
#else
                    throw new FtpsException("Sha256 algorithm not supported on this CLR version; recompile with .NET 4.0 or higher");
#endif
                case HashingAlgorithm.Sha512:
#if CLR_4_PLUS
                    hashAlgo = new SHA512CryptoServiceProvider();
                    break;
#else
                    throw new FtpsException("Sha256 algorithm not supported on this CLR version; recompile with .NET 4.0 or higher");
#endif

            }

            if (startPosition > 0)
                inputStream.Position = startPosition;
            else
                inputStream.Position = 0;

            byte[] hashArray = hashAlgo.ComputeHash(inputStream);

            // convert byte array to a string
            StringBuilder buffer = new StringBuilder(hashArray.Length);
            foreach (byte hashByte in hashArray)
            {
                buffer.Append(hashByte.ToString("x2"));
            }

            return buffer.ToString();
        }

        /// <summary>
        /// Get the additional features supported by the remote FTP server as a text string.  
        /// </summary>
        /// <returns>A string containing the additional features beyond the RFC 959 standard supported by the FTP server.</returns>
        /// <remarks>
        /// This command is an additional feature beyond the RFC 959 standard and therefore is not supported by all FTP servers.        
        /// </remarks>
        /// <exception cref="FtpsFeatureException"></exception>
        public string GetFeatures()
        {
            try
            {
                SendRequest(new FtpsRequest(Encoding, FtpsCmd.Feat));
            }
            catch (FtpsException fex)
            {
                throw new FtpsFeatureException("An error occurred when retrieving features.", LastResponse, fex);
            }

            return LastResponseList.GetRawText();
        }

        #endregion

        #region Internal Properties
        
        internal BackgroundWorker AsyncWorker
        {
            get { return _asyncWorker; }
        }

        /// <summary>
        /// Get a reference to the internal features list.
        /// </summary>
        internal FtpsFeatureCollection Features
        {
            get { return _feats; }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets a value indicating whether an asynchronous operation is canceled.
        /// </summary>
        /// <remarks>Returns true if an asynchronous operation is canceled; otherwise, false.
        /// </remarks>
        public bool IsAsyncCanceled
        {
            get { return _asyncCanceled; }
        }
        
        /// <summary>
        /// Gets a value indicating whether an asynchronous operation is running.
        /// </summary>
        /// <remarks>Returns true if an asynchronous operation is running; otherwise, false.
        /// </remarks>
        public bool IsBusy
        {
            get { return _asyncWorker == null ? false : _asyncWorker.IsBusy; }
        }

        /// <summary>
        /// Gets or sets the current port number used by the FtpsClient to make a connection to the FTP server.
        /// </summary>
        /// <remarks>
        /// The default value is '80'.  This setting can only be changed when the 
        /// connection to the FTP server is closed.  And FtpException is thrown if this 
        /// setting is changed when the FTP server connection is open.
        /// 
        /// Returns an integer representing the port number used to connect to a remote server.
        /// </remarks>
        public int Port
        {
            get { return _port; }
            set 
            {
                if (this.IsConnected)
                    throw new FtpsException("Port property value cannot be changed when connection is open.");
                _port = value; 
            }
        }

        /// <summary>
        /// Gets or sets a text value containing the current host used by the FtpsClient to make a connection to the FTP server.
        /// </summary>
        /// <remarks>
        /// This value may be in the form of either a host name or IP address.
        /// This setting can only be changed when the 
        /// connection to the FTP server is closed.  
        /// 
        /// Returns a string with either the host name or host ip address.
        /// </remarks>
        public string Host
        {
            get { return _host; }
            set 
            {
                if (this.IsConnected)
                    throw new FtpsException("Host property value cannot be changed when connection is open.");
                _host = value; 
            }
        }

        /// <summary>
        /// Gets or sets a text value containing the IPv4 or IPv6 client IP address used by the FtpsClient to make active connections to the FTP server.
        /// </summary>
        /// <remarks>
        /// The NetworkProtocol property must be properly specified and must match the type of client IP address specified.  For example, if an IPv6
        /// address is specified in the Client property, the NetworkProtocol must be set to IPv6.
        /// 
        /// The client IP address is not required and can be optionally provided as a IP address for the client to use when transmitted
        /// Active mode PORT command information to the FTP server.  If this value is empty the Starksoft FtpsClient will choose an 
        /// available IP address on the client machine.  In some cases it is desirable to provide a specific IP address of the external border router
        /// to an internal network.  Not all firewall devices will automatically switch the IP address for the PORT or EPRT commands.  In those cases
        /// the external facing IP address can be provided for use.  In addition, some clients may find that a specific IP address on the client
        /// local host machine should be used for making Active mode connections.  Note the IP address provided is not verified as a valid local
        /// host IP address.
        /// 
        /// This value may be in the form of either a host name or IP address.
        /// This setting can only be changed when the 
        /// connection to the FTP server is closed.  
        /// Returns a string with either the host name or host ip address.
        /// </remarks>
        /// <seealso cref="NetworkProtocol"/>
        /// <seealso cref="Host"/>
        /// <seealso cref="Port"/>
        public string Client
        {
            get { return _client; }
            set
            {
                if (this.IsConnected)
                    throw new FtpsException("Client property value cannot be changed when connection is open.");

                try
                {
                    IPAddress.Parse(value);
                }
                catch (FormatException)
                {
                    throw new ArgumentException("The Client IP address must be a valid IPv4 or IPv6 notated Internet Protocol address.");
                }
                                
                _client = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating what security protocol such as Secure Sock Layer (SSL) should be used.
        /// </summary>
        /// <remarks>
        /// The default value is 'None'.  This setting can only be changed when the 
        /// connection to the FTP server is closed.  An FtpException is thrown if this 
        /// setting is changed when the FTP server connection is open.
        /// </remarks>
        /// <seealso cref="SecurityCertificates"/>
        /// <seealso cref="ValidateServerCertificate" />
        public FtpsSecurityProtocol SecurityProtocol
        {
            get { return _securityProtocol; }
            set 
            {
                if (this.IsConnected)
                    throw new FtpsException("SecurityProtocol property value cannot be changed when connection is open.");
                _securityProtocol = value; 
            }
        }

        /// <summary>
        /// Gets or sets a value indicating what network protocol should be used..
        /// </summary>
        /// <remarks>
        /// The default value is 'Ipv4'.  This setting can only be changed when the 
        /// connection to the FTP server is closed.  An FtpException is thrown if this 
        /// setting is changed when the FTP server connection is open.
        /// </remarks>
        /// <seealso cref="Client"/>
        /// <seealso cref="Host"/>
        /// <seealso cref="Port"/>
        public NetworkVersion NetworkProtocol
        {
            get { return _networkProtocol; }
            set
            {
                if (this.IsConnected)
                    throw new FtpsException("NetworkProtocol property value cannot be changed when connection is open.");
                _networkProtocol = value;
            }
        }

        /// <summary>
        /// Get Client certificate collection used when connection with a secured SSL/TSL protocol.  Add your client certificates 
        /// if required to connect to the remote FTP server.
        /// </summary>
        /// <remarks>Returns a X509CertificateCollection list contains X.509 security certificates.</remarks>
        /// <seealso cref="SecurityProtocol"/>
        /// <seealso cref="ValidateServerCertificate" />
        public X509CertificateCollection SecurityCertificates
        {
            get { return _clientCertificates; }
        }

        /// <summary>
        /// Gets or sets a value indicating that the client will use compression when uploading and downloading
        /// data.
        /// </summary>
        /// <remarks>
        /// This value turns on or off the compression algorithm DEFLATE to facility FTP data compression which is compatible with
        /// FTP servers that implement compression via the zLib compression software library.  The default value is 'False'.  
        /// This setting can only be changed when the system is not busy conducting other operations.  
        /// 
        /// Returns True if compression is enabled; otherwise False;
        /// </remarks>
        public bool IsCompressionEnabled
        {
            get { return _isCompressionEnabled; }
            set 
            { 
                if (this.IsBusy)
                    throw new FtpsBusyException("IsCompressionEnabled");
                try
                {
                    // enable compression
                    if (this.IsConnected && value && value != _isCompressionEnabled)
                        CompressionOn();

                    // disable compression
                    if (this.IsConnected && !value && value != _isCompressionEnabled)
                        CompressionOff();
                }
                catch (FtpsException ex)
                {
                    throw new FtpsDataCompressionException("An error occurred while trying to enable or disable FTP data compression.", ex);
                }
                _isCompressionEnabled = value; 
            }
        }

        /// <summary>
        /// Gets or sets an Integer value representing the maximum upload speed allowed 
        /// for data transfers in kilobytes per second.
        /// </summary>
        /// <remarks>
        /// Set this value when you would like to throttle back any upload data transfers.
        /// A value of zero means there is no restriction on how fast data uploads are 
        /// conducted.  The default value is zero.  This setting is used to throttle data traffic so the FtpsClient does
        /// not consume all available network bandwidth.
        /// </remarks>
        /// <seealso cref="MaxDownloadSpeed"/>
        public int MaxUploadSpeed
        {
            get { return _maxUploadSpeed; }
            set 
            {
                if (this.IsBusy)
                    throw new FtpsBusyException("MaxUploadSpeed");
                if (value * 1024 > Int32.MaxValue || value < 0)
                    throw new ArgumentOutOfRangeException("value", "The MaxUploadSpeed property must have a range of 0 to 2,097,152.");
                _maxUploadSpeed = value; 
            }
        }

        /// <summary>
        /// Gets or sets an Integer value representing the maximum download speed allowed 
        /// for data transfers in kilobytes per second.
        /// </summary>
        /// <remarks>
        /// Set this value when you would like to throttle back any download data transfers.
        /// A value of zero means there is no restriction on how fast data uploads are 
        /// conducted.  The default value is zero.  This setting is used to throttle data traffic so the FtpsClient does
        /// not consume all available network bandwidth.
        /// </remarks>
        /// <seealso cref="MaxUploadSpeed"/>
        public int MaxDownloadSpeed
        {
            get { return _maxDownloadSpeed; }
            set 
            {
                if (this.IsBusy)
                    throw new FtpsBusyException("MaxDownloadSpeed");
                if (value * 1024 > Int32.MaxValue || value < 0)
                    throw new ArgumentOutOfRangeException("value", "must have a range of 0 to 2,097,152.");
                _maxDownloadSpeed = value; 
            }
        }

        /// <summary>
        /// Gets only the last response from the FTP server.
        /// </summary>
        /// <remarks>Returns a FtpResponse object containing the last FTP server response; other the value null (or Nothing in VB) is returned.</remarks>
        public FtpsResponse LastResponse
        {
            get { return _response; }
        }

        /// <summary>
        /// Gets the list of all responses since the last command was issues to the server.
        /// </summary>
        /// <remarks>Returns a FtpResponseCollection list containing all the responses.</remarks>
        public FtpsResponseCollection LastResponseList
        {
            get { return _responseList; }
        }

        /// <summary>
        /// Gets or sets the TCP buffer size used when communicating with the FTP server in bytes.
        /// </summary>
        /// <remarks>Returns an integer value representing the buffer size.  The default value is 8192.</remarks>
        public int TcpBufferSize
        {
            get { return _tcpBufferSize; }
            set 
            {
                if (this.IsConnected)
                    throw new FtpsException("TcpBufferSize property value cannot be changed when connection is open.");
                if (value < 1)
                    throw new ArgumentOutOfRangeException("value", "must be greater than 0.");
                _tcpBufferSize = value; 
            }
        }

        /// <summary>
        /// Gets or sets the TCP timeout used when communciating with the FTP server in milliseconds.
        /// </summary>
        /// <remarks>
        /// Default value is 30000 (30 seconds).
        /// </remarks>
        /// <seealso cref="TransferTimeout"/>
        /// <seealso cref="CommandTimeout"/>
        public int TcpTimeout
        {
            get { return _tcpTimeout; }
            set 
            {
                if (this.IsConnected)
                    throw new FtpsException("TcpTimeout property value cannot be changed when connection is open.");
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value", "must be greater than or equal to 0.");
                _tcpTimeout = value; 
            }
        }

        /// <summary>
        /// Gets or sets the data transfer timeout used when communicating with the FTP server in milliseconds.
        /// </summary>
        /// <remarks>
        /// Default value is 30000 (30 seconds).
        /// </remarks>
        /// <seealso cref="TcpTimeout"/>
        /// <seealso cref="CommandTimeout"/>
        public int TransferTimeout
        {
            get { return _transferTimeout; }
            set 
            {
                if (this.IsBusy)
                    throw new FtpsBusyException("TransferTimeout");
                if (value < 1)
                    throw new ArgumentOutOfRangeException("value", "must be greater than 0.");
                _transferTimeout = value; 
            }
        }

        /// <summary>
        /// Gets or sets the FTP command timeout used when communciating with the FTP server in milliseconds.
        /// </summary>
        /// <remarks>
        /// Default value is 30000 (30 seconds).
        /// </remarks>
        /// <seealso cref="TcpTimeout"/>
        /// <seealso cref="TransferTimeout"/>
        public int CommandTimeout
        {
            get { return _commandTimeout; }
            set 
            {
                if (this.IsConnected)
                    throw new FtpsException("CommandTimeout property value cannot be changed when connection is open.");
                if (value < 1)
                    throw new ArgumentOutOfRangeException("value", "must be greater than 0.");
                _commandTimeout = value; 
            }
        }             

        /// <summary>
        /// The beginning port number range used by the FtpsClient when opening a local 'Active' port.  The default value is 4051.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Value must be less than or equal to the ActivePortRangeMax value.</exception>
        /// <remarks>
        /// When the FtpsClient is in 'Active' mode a local port is opened for communications from the FTP server.
        /// The FtpsClient will attempt to open an unused TCP listener port between the ActivePortRangeMin and ActivePortRangeMax values.
        /// Default value is 50000.
        /// </remarks>
        /// <seealso cref="ActivePortRangeMax"/>
        /// <seealso cref="DataTransferMode"/>
        public int ActivePortRangeMin
        {
            get { return _activePortRangeMin; }
            set
            {
                if (this.IsBusy)
                    throw new FtpsBusyException("ActivePortRangeMin");
                if (value > _activePortRangeMin)
                    throw new ArgumentOutOfRangeException("value","must be less than the ActivePortRangeMax value.");
                if (value < 1 || value > 65534)
                    throw new ArgumentOutOfRangeException("value", "must be between 1 and 65534.");
                _activePortRangeMin = value;
            }
        }

        /// <summary>
        /// The ending port number range used by the FtpsClient when opening a local 'Active' port.  The default value is 4080.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Value must be greater than or equal to the ActivePortRangeMin value.</exception>
        /// <remarks>
        /// When the FtpsClient is in 'Active' mode a local port is opened for communications from the FTP server.
        /// The FtpsClient will attempt to open an unused TCP listener port between the ActivePortRangeMin and ActivePortRangeMax values.
        /// Default value is 50080.
        /// </remarks>
        /// <seealso cref="ActivePortRangeMin"/>
        /// <seealso cref="DataTransferMode"/>
        public int ActivePortRangeMax
        {
            get { return _activePortRangeMax; }
            set
            {
                if (this.IsBusy)
                    throw new FtpsBusyException("ActivePortRangeMax");
                if (value < _activePortRangeMin)
                    throw new ArgumentOutOfRangeException("value", "must be greater than the ActivePortRangeMin value.");
                if (value < 1 || value > 65534)
                    throw new ArgumentOutOfRangeException("value", "must be between 1 and 65534.");
                _activePortRangeMax = value;
            }
        }


        /// <summary>
        /// Gets or sets the data transfer mode to either Active or Passive.
        /// </summary>
        /// <seealso cref="ActivePortRangeMin"/>
        /// <seealso cref="ActivePortRangeMax"/>
        public TransferMode DataTransferMode
        {
            get { return _dataTransferMode; }
            set 
            {
                if (this.IsBusy)
                    throw new FtpsBusyException("DataTransferMode");
                _dataTransferMode = value; 
            }
        }

        /// <summary>
        /// Gets or sets the the proxy object to use when establishing a connection to the remote FTP server.
        /// </summary>
        /// <remarks>Create a proxy object when traversing a firewall.</remarks>
        /// <code>
        /// FtpsClient ftp = new FtpsClient();
        ///
        /// // create an instance of the client proxy factory for the an ftp client
        /// ftp.Proxy = (new ProxyClientFactory()).CreateProxyClient(ProxyType.Http, "localhost", 6588);
        ///        
        /// </code>
        /// <seealso cref="Starksoft.Aspen.Proxy.ProxyClientFactory"/>
        public IProxyClient Proxy
        {
            get { return _proxy; }
            set 
            {
                if (this.IsConnected)
                    throw new FtpsException("Proxy property value cannot be changed when connection is open.");
                _proxy = value; 
            }
        }

        /// <summary>
        /// Gets the connection status to the FTP server.
        /// </summary>
        /// <remarks>Returns True if the connection is open; otherwise False.</remarks>
        /// <seealso cref="ConnectionClosed"/>
        public bool IsConnected
        {
            get
            {
                if (_commandConn == null
                    || _commandConn.Client == null)
                {
                    return false;
                }

                Socket client = _commandConn.Client;
                if (!client.Connected)
                    return false;

                bool connected = true;
                try
                {
                    byte[] tmp = new byte[1];

                    // sychronize on streamWriteLock object to prevent non-blocking exception errors
                    lock (_cmdStreamWriteLock)
                    {
                        // attempt to write no data in hopes of getting status on the socket
                        client.Send(tmp, 0, 0);
                    }
                }
                catch (SocketException e)
                {
                    // 10035 == WSAEWOULDBLOCK
                    if (!e.NativeErrorCode.Equals (10035)) {
                        connected = false;
                    }
                }
                catch (ObjectDisposedException)
                { }

                return connected;
            }
            
        }

        /// <summary>
        /// Sets the automatic file integrity setting (cryptographic hash or CRC value) option on all data transfers (upload and download).
        /// </summary>
        /// <remarks>
        /// The FtpsClient library will throw an FtpFileIntegrityException if the file integrity value do not match.
        /// 
        /// Not all FTP servers support file integrity values such as SHA1, CRC32, or MD5.  If you server does support
        /// one of these file integrity options, you can set this property and the FtpsClient will automatically check
        /// each file that is transferred to make sure the hash values match.  If the values do not match, an exception
        /// is thrown.
        /// </remarks>
        /// <seealso cref="FtpsHashingException"/>
        /// <seealso cref="GetHash(HashingAlgorithm, string)"/>
        /// <seealso cref="ComputeHash(HashingAlgorithm, string)"/>
        public HashingAlgorithm AutomaticFileHashing
        {
            get { return _autoHashAlgorithm; }
            set 
            {
                if (this.IsBusy)
                    throw new FtpsBusyException("AutomaticFileHashing");
                _autoHashAlgorithm = value; 
            }
        }

        /// <summary>
        /// Gets or sets the boolean value indicating if the server certificate should
        /// always be accepted for TLS/SSL secure connections.
        /// </summary>
        /// <remarks>
        /// If this value is set to true, the ValidateServerCertificate event will not fire and all
        /// TLS/SSL connections will not ask the client to validate the integrity of the server certificate.
        /// </remarks>
        public bool AlwaysAcceptServerCertificate
        {
            get { return _alwaysAcceptServerCertificate; }
            set 
            {
                if (this.IsConnected)
                    throw new FtpsException("AlwaysAcceptServerCertificate property value cannot be changed when the connection is open.");
                _alwaysAcceptServerCertificate = value; 
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Starksoft.Aspen.Ftps.FtpsBase"/> send prot P for implicit
        /// ssl connections.
        /// </summary>
        /// <remarks>
        /// Filezilla requires by default PROT P for implicit TLS connections unless disabled in the server settings.  Set
        /// this option to false if the server you are connecting to can not accept PROT P for implicit connections.
        /// </remarks>
        /// <value><c>true</c> if send prot P for implicit ssl connections; otherwise, <c>false</c>.</value>
        public bool SendProtPForImplicitSslConnections
        {
            get { return _sendProtPForImplicitSslConnections; }
            set { 
                if (this.IsConnected)
                    throw new FtpsException("SendProtPForImplicitSslConnections property value cannot be changed when the connection is open.");
                _sendProtPForImplicitSslConnections = value; 
            }
        }

        /// <summary>
        /// Gets or sets the internal character encoding object use to encode the request and response data.
        /// </summary>
        /// <remarks>
        /// The encoding object used for character encoding when communicating with the FTP server
        /// is managed by the FtpsClient.  If the FTP server your are connecting to has a different encoding
        /// standard then the client machine, you can override the default behavior with a custom encoding object
        /// which the FtpsClient will then use instead of the default client Encoding object.  An example would be
        /// a US english client connecting to a FTP server than contains French UTF-8 encoding characters
        /// in file names or directories on the FTP server.
        /// </remarks>
        public Encoding Encoding
        {
            get { return _encode; }
            set { _encode = value; }
        }
        
        #endregion

        #region Internal Protected Methods

        internal void TryResetFeatures()
        {
            _feats = null;
            TrySetFeatures();
        }
        
        internal void TrySetFeatures()
        {
            // if the features collection has not be set then create it
            if (_feats == null)
            {
                string s = "";
                try
                {
                    s = GetFeatures();
                }
                catch (FtpsException)
                {
                    _feats = new FtpsFeatureCollection();
                }
                if (_feats == null)
                    _feats = new FtpsFeatureCollection(s);
            }
        }

        /// <summary>
        /// Send a FTP command request to the server.
        /// </summary>
        /// <param name="request"></param>
        internal void SendRequest(FtpsRequest request)
        {

            if (_commandConn == null || _commandConn.Connected == false)
                throw new FtpsConnectionClosedException("Connection is closed.");

            // clear out any responses that might have been pending from a previous
            // failed operation
            DontWaitForHappyCodes();

            if (ClientRequest != null)
                ClientRequest(this, new FtpsRequestEventArgs(request));

            byte[] buffer = request.GetBytes();

            // write request to debug out
            Debug.Write("S: " + _encode.GetString(buffer));

            try
            {
                // sychronize on streamWriteLock object to prevent non-blocking exception errors
                lock (_cmdStreamWriteLock)
                {
                    _commandStream.Write(buffer, 0, buffer.Length);
                }
            }
            catch (IOException ex)
            {
                throw new FtpsConnectionBrokenException("Connection is broken.  Failed to send command.", ex);
            }

            // most commands will have happy codes but the quote() command does not 
            if (request.HasHappyCodes)
            {
                WaitForHappyCodes(request.GetHappyCodes());
            }
            else
            {
                // when there are no happy codes given the we have to give the server some time to response
                // since we really don't know what response is the correct one
                if (request.Command != FtpsCmd.Quit)
                    Thread.Sleep(2000);
                DontWaitForHappyCodes();
            }
        }

        /// <summary>
        /// creates a new async worker object for the async events to use.
        /// </summary>
        internal void CreateAsyncWorker()
        {
            if (_asyncWorker != null)
                _asyncWorker.Dispose();
            _asyncWorker = null;
            _asyncCanceled = false;
            _asyncWorker = new BackgroundWorker();
        }
                
        /// <summary>
        /// Closes all connections to the FTP server.
        /// </summary>
        internal void CloseAllConnections()
        {
            CloseDataConn();
            CloseCommandConn();
            AbortMonitorThread();
        }

        /// <summary>
        /// The monitor thread should close automatically once the command connection is terminated.  If it does not close properly, force it to close.
        /// </summary>
        private void AbortMonitorThread()
        {
            if (_responseMonitor != null)
                _responseMonitor.Abort();
        }

        //  open a connection to the server
        internal void OpenCommandConn()
        {
            //  create a new tcp client object 
            CreateCommandConn();
            
            StartCommandMonitorThread();

            if (_securityProtocol == FtpsSecurityProtocol.Ssl2Explicit 
                || _securityProtocol == FtpsSecurityProtocol.Ssl3Explicit 
                || _securityProtocol == FtpsSecurityProtocol.Tls1Explicit 
                || _securityProtocol == FtpsSecurityProtocol.Tls1OrSsl3Explicit
                || _securityProtocol == FtpsSecurityProtocol.Tls11Explicit
                || _securityProtocol == FtpsSecurityProtocol.Tls12Explicit)
                CreateSslExplicitCommandStream();

            if (_securityProtocol == FtpsSecurityProtocol.Ssl2Implicit 
                || _securityProtocol == FtpsSecurityProtocol.Ssl3Implicit 
                || _securityProtocol == FtpsSecurityProtocol.Tls1Implicit 
                || _securityProtocol == FtpsSecurityProtocol.Tls1OrSsl3Implicit
                || _securityProtocol == FtpsSecurityProtocol.Tls11Implicit
                || _securityProtocol == FtpsSecurityProtocol.Tls12Implicit)
                CreateSslImplicitCommandStream();

            // test to see if this is an asychronous operation and if so make sure 
            // the user has not requested the operation to be canceled
            if (IsAsyncCancellationPending())
                return;

            // this check screws up secure connections so we have to ignore it when secure connections are enabled
            if (_securityProtocol == FtpsSecurityProtocol.None)
                WaitForHappyCodes(FtpsResponseCode.ServiceReadyForNewUser);

        }

        internal void TransferData(TransferDirection direction, FtpsRequest request, Stream data)
        {
            TransferData(direction, request, data, 0);
        }

        internal void TransferData(TransferDirection direction, FtpsRequest request, Stream data, long restartPosition)
        {
            if (_commandConn == null || _commandConn.Connected == false)
                throw new FtpsConnectionClosedException("Connection is closed.");

            if (request == null)
                throw new ArgumentNullException("request", "value is required");

            if (data == null)
                throw new ArgumentNullException("data", "value is required");

            switch (direction)
            {
                case TransferDirection.ToClient:
                    if (!data.CanWrite)
                        throw new FtpsDataTransferException("Data transfer error.  Data conn does not allow write operation.");
                    break;
                case TransferDirection.ToServer:
                    if (!data.CanRead)
                        throw new FtpsDataTransferException("Data transfer error.  Data conn does not allow read operation.");
                    break;
            }

            // if this is a restart then the data stream must support seeking
            if (restartPosition > 0 && !data.CanSeek)
                throw new FtpsDataTransferException("Data transfer restart error.  Data conn does not allow seek operation.");

            OpenDataConnAndTransferData(direction, request, data, restartPosition);
        }


        internal void OpenDataConnAndTransferData(TransferDirection direction, FtpsRequest request, Stream data, long restartPosition)
        {
            try
            {
                // create a thread to begin the porocess of opening a data connection to the remote server
                OpenDataConn();

                //  check for a restart position 
                if (restartPosition > 0)
                {
                    // instruct the server to restart file transfer at the same position where the output stream left off
                    SendRequest(new FtpsRequest(_encode, FtpsCmd.Rest, restartPosition.ToString(CultureInfo.InvariantCulture)));
                    
                    // set the data stream to the same position as the server
                    data.Position = restartPosition;
                }

                // send the data transfer command that requires a separate data connection to be established to transmit data
                SendRequest(request);

                // wait for the data connection thread to signal back that a connection has been established
                WaitForDataConn();

                // test to see if the data connection failed to be established sometime the active connection fails due to security settings on the ftp server
                if (_dataConn == null)
                    throw new FtpsDataConnectionException("Unable to establish a data connection to the destination.  The destination may have refused the connection.");

                // create the data stream object - handles creation of SslStream and DeflateStream objects as well
                Stream conn = _dataConn.GetStream();

                // test to see if we need to enable ssl/tls explicit mode
                if (_securityProtocol != FtpsSecurityProtocol.None)
                    conn = CreateSslStream(conn);
                
                // test to see if we need to enable compression by using the DeflateStream
                if (_isCompressionEnabled)
                    conn = CreateZlibStream(direction, conn);

                // based on the direction of the data transfer we need to handle the input and output streams
                switch (direction)
                {
                    case TransferDirection.ToClient:
                        TransferBytes(conn, data, _maxDownloadSpeed * 1024);
                        break;
                    case TransferDirection.ToServer:
                        TransferBytes(data, conn, _maxUploadSpeed * 1024);
                        break;
                }

            }
            finally
            {
                // reset the transfer size used to calc % completed
                SetTransferSize(-1);
                // attempt to close the data connection 
                CloseDataConn();
            }

            // if no errors occurred and this is not a quoted command then we will wait for the server to send a closing connection message
            WaitForHappyCodes(FtpsResponseCode.ClosingDataConnection);

            // automatic integrity check with file transfer
            if (_autoHashAlgorithm != HashingAlgorithm.None 
                && request.IsFileTransfer)
                DoIntegrityCheck(request, data, restartPosition);
        }

        internal string TransferText(FtpsRequest request)
        {
            Stream output = new MemoryStream();
            TransferData(TransferDirection.ToClient, request, output);
            output.Position = 0;
            StreamReader reader = new StreamReader(output, _encode);
            return reader.ReadToEnd();
        }

        internal void CompressionOn()
        {
            if (!Features.Contains(FtpsCmd.Mode, "Z"))
                throw new FtpsCommandNotSupportedException("Cannot enable data compression.", "MODE Z");

            try
            {
                SendRequest(new FtpsRequest(_encode, FtpsCmd.Mode, "Z"));
            }
            catch (Exception ex)
            {
                throw new FtpsDataCompressionException("Unable to enable compression (MODE Z) on the destination.", ex);
            }
        }

        internal void CompressionOff()
        {
            try
            {
                SendRequest(new FtpsRequest(_encode, FtpsCmd.Mode, "S"));
            }
            catch (Exception ex)
            {
                throw new FtpsDataCompressionException("Unable to disable compression (MODE S) on the destination.", ex);
            }
        }

        #endregion

        #region Private Methods

        private void StartCommandMonitorThread()
        {
            // start the monitor thread which pumps FtpResponse objects on the FtpResponseQueue
            _responseMonitor = new Thread(new ThreadStart(MonitorCommandConnection));
            _responseMonitor.Name = "FtpBase Response Monitor";
            _responseMonitor.Start();
        }

        private bool IsAsyncCancellationPending()
        {
            if (_asyncWorker != null && _asyncWorker.CancellationPending)
            {
                _asyncCanceled = true;
                return true;
            }
            return false;
        }

        private void TransferBytes(Stream input, Stream output, int maxBytesPerSecond)
        {
            long bytesTotal = 0;
            int bytesRead = 0;
            DateTime start = DateTime.Now;
            TimeSpan elapsed = new TimeSpan(0);
            int bytesPerSec = 0;
            int percentComplete = 0;

            // allocate space for buffer size if not done so already on a previous call
            // or has been updated between calls
            if (_dataBuffer == null || _dataBuffer.Length != _tcpBufferSize)
                _dataBuffer = new byte[_tcpBufferSize];

            while(true)
            {
                bytesRead = input.Read(_dataBuffer, 0, _tcpBufferSize);

                if (bytesRead == 0)
                    break;

                bytesTotal += bytesRead;
                output.Write(_dataBuffer, 0, bytesRead);

                // calculate some statistics
                elapsed = DateTime.Now.Subtract(start);
                bytesPerSec = (int)(elapsed.TotalSeconds < 1 ? bytesTotal : bytesTotal / elapsed.TotalSeconds);

                //  if the consumer subscribes to transfer progress event then fire it
                if (TransferProgress != null)
                {
                    if (_transferSize > 0)
                    {
                        int newpc = (int)(((float)bytesTotal / (float)_transferSize) * 100);
                       
                        // to preserve resources and prevent a slow down of transfers due to
                        // frequent event calls, only fire the event if the percentage has changed
                        if (newpc != percentComplete)
                        {
                            percentComplete = newpc;
                            TransferProgress(this, new TransferProgressEventArgs(bytesRead, bytesTotal, _transferSize, bytesPerSec, elapsed, percentComplete));
                        }
                    }
                    else // always fire the event if we cannot calc percentages
                    {
                        TransferProgress(this, new TransferProgressEventArgs(bytesRead, bytesTotal, _transferSize, bytesPerSec, elapsed, percentComplete));
                    }
                }

                // test to see if this is an asychronous operation and if so make sure 
                // the user has not requested the operation to be canceled
                if (IsAsyncCancellationPending())
                    throw new FtpsAsynchronousOperationException("Asynchronous operation canceled by user.");
                
                // throttle the transfer if necessary
                ThrottleByteTransfer(maxBytesPerSecond, bytesTotal, elapsed, bytesPerSec);

            } ;

            //  if the consumer subscribes to transfer complete event then fire it
            if (TransferComplete != null)
                TransferComplete(this, new TransferCompleteEventArgs(bytesTotal, bytesPerSec, elapsed));
        }

        private void ThrottleByteTransfer(int maxBytesPerSecond, long bytesTotal, TimeSpan elapsed, int bytesPerSec)
        {
            // we only throttle if the maxBytesPerSecond is not zero (zero turns off the throttle)
            if (maxBytesPerSecond > 0)
            {
                // we only throttle if our through-put is higher than what we want
                if (bytesPerSec > maxBytesPerSecond)
                {
                    double elapsedMilliSec = elapsed.TotalSeconds == 0 ? elapsed.TotalMilliseconds : elapsed.TotalSeconds * 1000;

                    // need to calc a delay in milliseconds for the throttle wait based on how fast the 
                    // transfer is relative to the speed it needs to be
                    double millisecDelay = (bytesTotal / (maxBytesPerSecond / 1000) - elapsedMilliSec);
                    
                    // can only sleep to a max of an Int32 so we need to check this since bytesTotal is a long value
                    // this should never be an issue but never say never
                    if (millisecDelay > Int32.MaxValue)
                        millisecDelay = Int32.MaxValue;

                    // go to sleep
                    Thread.Sleep((int)millisecDelay);
                }
            }
        }

        private void CreateCommandConn()
        {
            if (_host == null || _host.Length == 0)
                throw new FtpsException("An FTP Host must be specified before opening connection to FTP destination.  Set the appropriate value using the Host property on the FtpsClient object.");

            try
            {
                //  test to see if we should use the user supplied proxy object
                //  to create the connection
                if (_proxy != null)
                    _commandConn = _proxy.CreateConnection(_host, _port);
                else
                    _commandConn = new TcpClient(_host, _port);
            }
            catch (ProxyException pex)
            {
                if (_commandConn != null)
                    _commandConn.Close();

                throw new ProxyException(String.Format(CultureInfo.InvariantCulture, "A proxy error occurred while creating connection to FTP destination {0} on port {1}.", _host, _port.ToString(CultureInfo.InvariantCulture)), pex);
            }
            catch (Exception ex)
            {
                if (_commandConn != null)
                    _commandConn.Close();

                throw new FtpsConnectionOpenException(String.Format(CultureInfo.InvariantCulture, "An error occurred while opening a connection to FTP destination {0} on port {1}.", _host, _port.ToString(CultureInfo.InvariantCulture)), ex);
            }            

            // set command connection buffer sizes and timeouts
            _commandConn.ReceiveBufferSize = _tcpBufferSize;
            _commandConn.ReceiveTimeout = _tcpTimeout;
            _commandConn.SendBufferSize = _tcpBufferSize;
            _commandConn.SendTimeout = _tcpTimeout;

            // set the command stream object
            _commandStream = _commandConn.GetStream();
        }
        
        
        private void CloseCommandConn()
        {
            if (_commandConn == null) 
                return;
            try
            {
                if (_commandConn.Connected)
                {
                    //  send the quit command to the server
                    SendRequest(new FtpsRequest(_encode, FtpsCmd.Quit));
                }
                _commandConn.Close();
            }
            catch { }
            
            _commandConn = null;
        }


        private void WaitForHappyCodes(params FtpsResponseCode[] happyResponseCodes)
        {
            WaitForHappyCodes(_commandTimeout, happyResponseCodes);
        }

        /// <summary>
        /// Waits until a happy code has been returned by the FTP server or times out.
        /// </summary>
        /// <param name="timeout">Maximum time to wait before timing out.</param>
        /// <param name="happyResponseCodes">Server response codes to wait for.</param>
        internal protected  void WaitForHappyCodes(int timeout, params FtpsResponseCode[] happyResponseCodes)
        {
            _responseList.Clear();
            do
            {
                FtpsResponse response = GetNextCommandResponse(timeout);
                _responseList.Add(response);
                RaiseServerResponseEvent(new FtpsResponse(response));

                if (!response.IsInformational)
                {
                    if (IsHappyResponse(response, happyResponseCodes))
                        break;

                    if (IsUnhappyResponse(response))
                    {
                        _response = response;
                        throw new FtpsResponseException("FTP command failed.", response);
                    }
                }
            } while (true);

            _response = _responseList.GetLast();
        }

        private void RaiseServerResponseEvent(FtpsResponse response)
        {
            if (ServerResponse != null)
                ServerResponse(this, new FtpsResponseEventArgs(response));
        }

        private void RaiseConnectionClosedEvent()
        {
            if (ConnectionClosed != null)
                ConnectionClosed(this, new ConnectionClosedEventArgs());
        }

        private bool IsUnhappyResponse(FtpsResponse response)
        {
            if (
                 response.Code == FtpsResponseCode.ServiceNotAvailableClosingControlConnection
                || response.Code == FtpsResponseCode.CannotOpenDataConnection
                || response.Code == FtpsResponseCode.ConnectionClosedSoTransferAborted
                || response.Code == FtpsResponseCode.RequestedFileActionNotTaken
                || response.Code == FtpsResponseCode.RequestedActionAbortedDueToLocalErrorInProcessing
                || response.Code == FtpsResponseCode.RequestedActionNotTakenInsufficientStorage
                || response.Code == FtpsResponseCode.SyntaxErrorCommandUnrecognized
                || response.Code == FtpsResponseCode.SyntaxErrorInParametersOrArguments
                || response.Code == FtpsResponseCode.CommandNotImplemented
                || response.Code == FtpsResponseCode.BadSequenceOfCommands
                || response.Code == FtpsResponseCode.CommandNotImplementedForThatParameter
                || response.Code == FtpsResponseCode.NotLoggedIn
                || response.Code == FtpsResponseCode.NeedAccountForStoringFiles
                || response.Code == FtpsResponseCode.RequestedActionNotTakenFileUnavailable
                || response.Code == FtpsResponseCode.RequestedActionAbortedPageTypeUnknown
                || response.Code == FtpsResponseCode.RequestedFileActionAbortedExceededStorageAllocation
                || response.Code == FtpsResponseCode.RequestedActionNotTakenFileNameNotAllowed)
                return true;
            else
                return false;
        }

        private bool IsHappyResponse(FtpsResponse response, FtpsResponseCode[] happyResponseCodes)
        {
            // always return true if there are no responses to validate
            if (happyResponseCodes.Length == 0)
                return true;

            for (int j = 0; j < happyResponseCodes.Length; j++)
            {
                if (happyResponseCodes[j] == response.Code)
                    return true;
            }
            return false;
        }

        private void MonitorCommandConnection()
        {
            byte[] buffer = new byte[_tcpBufferSize];
            StringBuilder response = new StringBuilder();
            bool code421Detected = false;
            long cycles = 0;

            while (IsConnected)
            {
                // every 100 cycle sleep to give a chance for the lock
                // to be shared with the competing threads
                if (cycles++ % 100 == 0) {
                    cycles = 0; // reset cycles so we don't have a possible overflow 
                    Thread.Sleep(WAIT_FOR_COMMAND_RESPONSE_INTERVAL);
                }

                lock (_reponseMonitorLock)
                {
                    try
                    {
                        if (_commandConn != null && _commandConn.GetStream().DataAvailable)
                        {
                            int bytes = _commandStream.Read(buffer, 0, _tcpBufferSize);
                            string partial = _encode.GetString(buffer, 0, bytes);
                            response.Append(partial);

                            // write out the debug response
                            Debug.Write("R: " + partial);

                            if (!partial.EndsWith("\n"))
                            {
                                continue;
                            }

                             //  parse out the response code sent back from the server
                            //  in some cases more than one response can be sent with
                            //  each line separated with a crlf pair.
                            string[] responseArray = SplitResponse(response.ToString());
                            for (int i = 0; i < responseArray.Length; i++)
                            {
                                FtpsResponse r = new FtpsResponse(responseArray[i]);
                                _responseQueue.Enqueue(r);
                                // if a 421 response code is detected then the server has closed the connection
                                if (r.Code == FtpsResponseCode.ServiceNotAvailableClosingControlConnection)
                                    code421Detected = true;
                            }

                            response.Remove(0, response.Length);

                            // if code 421 was detected then close the command connection
                            if (code421Detected)
                                CloseCommandConn();
                        }
                    }
                    catch { }
                }
            }

            RaiseConnectionClosedEvent();

        }

        private FtpsResponse GetNextCommandResponse(int timeout)
        {
            int sleepTime = 0;
            while (_responseQueue.Count == 0)
            {
                if (!IsConnected)
                    throw new FtpsConnectionClosedException("Connection is closed.");

                if (IsAsyncCancellationPending())
                    throw new FtpsAsynchronousOperationException("Asynchronous operation canceled.");

                Thread.Sleep(WAIT_FOR_DATA_INTERVAL);
                sleepTime += WAIT_FOR_DATA_INTERVAL;
                // if we timed out make and no responses have come in then throw an exception
                if (sleepTime > timeout && _responseQueue.Count == 0)
                    throw new FtpsCommandResponseTimeoutException("A timeout occurred while waiting for the destination to send a response.  The last reponse from the destination is '" + _response.Text + "'");
            }

            // return next response object from the queue
            return _responseQueue.Dequeue();
        }

        private string[] SplitResponse(string response)
        {
            return response.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private void CreateActiveConn()
        {
            // retrieve local IPv4 or IPv6 address from local host based on user's settings
            // and available IP addresses on the local host; otherwise set to a user specific value
            IPAddress ip = GetClientIPAddress();
   
            // set the event to nonsignaled state
            _activeSignal.Reset();

            string localHostName = Dns.GetHostName();

            // using the IP Address find and open an active listener port on the local host
            // this method will use an Async callback on the listern socket to accept the connection
            // and subsequently Set the _activeSignal object to a signaled state
            int port = OpenActiveListenerPort(localHostName, ip);

            // build the port information string
            string portInfo = BuildPortInfo(port, ip);

            try
            {
                SendPortRequest(portInfo);
            }
            catch (FtpsException fex)
            {
                throw new FtpsDataConnectionException(String.Format("An error occurred while issuing active port command '{0}' on an active FTP connection.", portInfo), fex);
            }
        }

        private int GetNextActivePort()
        {
            if (_activePort < _activePortRangeMin || _activePort > _activePortRangeMax)
                _activePort = _activePortRangeMin;
            else
                _activePort++;

            return _activePort;
        }

        private void SendPortRequest(string portInfo)
        {
            // if IPv6 is selected then send EPRT request else send PORT request
            switch (_networkProtocol)
            {
                case NetworkVersion.IPv4:
                    SendRequest(new FtpsRequest(_encode, FtpsCmd.Port, portInfo));
                    break;
                case NetworkVersion.IPv6:
                    SendRequest(new FtpsRequest(_encode, FtpsCmd.Eprt, portInfo));
                    break;
                default:
                    throw new FtpsException("unexpected network protocol version");
            }
        }

        private string BuildPortInfo(int port, IPAddress ip)
        {
            switch (_networkProtocol)
            {
                case NetworkVersion.IPv4:
                    return BuildIPv4PortInfo(port, ip);
                case NetworkVersion.IPv6:
                    return BuildIPv6PortInfo(port, ip);
                default:
                    throw new FtpsException("unexpected network protocol version");
            }
        }

        private string BuildIPv4PortInfo(int port, IPAddress ip)
        {
            byte[] ipbytes = ip.GetAddressBytes();
            string portInfo = String.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3},{4},{5}",
                                    ipbytes[0].ToString(CultureInfo.InvariantCulture),
                                    ipbytes[1].ToString(CultureInfo.InvariantCulture),
                                    ipbytes[2].ToString(CultureInfo.InvariantCulture),
                                    ipbytes[3].ToString(CultureInfo.InvariantCulture),
                                    port / 256,
                                    port % 256);
            return portInfo;
        }

        private string BuildIPv6PortInfo(int port, IPAddress ip)
        {
            // (<d><net-prt><d><net-addr><d><tcp-port><d>)
            // net-prt:  1 - IPv4 or  2 - IPv6 (required)
            // net-addr:  network address IPv4 or IPv6 (required)
            // tcp-port:  listening port of FTP server (required)
            //
            // Example:
            //      Client> EPRT (|2|1080::8:800:200C:417A|5282|)
            //

            string netprt = "2";
            string addr = ip.ToString();
            // remove the ipv6 scope identifer value if it is represented in ipv6 string
            if (addr.IndexOf('%') != -1)
            {
                addr = addr.Substring(0, addr.IndexOf('%'));
            }

            string tcpport = port.ToString(CultureInfo.InvariantCulture);
            string portInfo = String.Format(CultureInfo.InvariantCulture, "|{0}|{1}|{2}|",
                                        netprt,
                                        addr,
                                        tcpport);
            return portInfo;
        }
        
        private IPAddress GetClientIPAddress()
        {
            // if the user has specified a specific client IP address to 
            // use then use that address; otherwise obtain one from the local machine.
            if (!String.IsNullOrEmpty(_client))
            {
                IPAddress ip = IPAddress.Parse(_client);
                if ((ip.AddressFamily == AddressFamily.InterNetwork && _networkProtocol != NetworkVersion.IPv4)
                    || (ip.AddressFamily == AddressFamily.InterNetworkV6 && _networkProtocol != NetworkVersion.IPv6))
                    throw new FtpsDataConnectionException("The Client IP address supplied does not match the NetworkProtocol selected.  Please check the NetworkProtcol property setting on the FtpsClient.");
                return ip;
            }

            switch (_networkProtocol)
            {
                case NetworkVersion.IPv4:
                    return GetClientIPv4Address();
                case NetworkVersion.IPv6:
                    return GetClientIPv6Address();
                default:
                    throw new FtpsException("unexpected network protocol version");
            }
        }

        /// <summary>
        /// This method is used for obtaining the IPv4 IPAddress object for FTP active
        /// connection (PORT).
        /// </summary>
        /// <returns></returns>
        private IPAddress GetClientIPv4Address()
        {
            string localHostName = Dns.GetHostName();
            IPAddress[] list = Dns.GetHostAddresses(localHostName);
            IPAddress ip = null;
            foreach (IPAddress a in list)
            {
                // look for IPv4 addresses only (InterNetwork)
                if (a.AddressFamily == AddressFamily.InterNetwork)
                {
                    ip = a;
                    break;
                }
            }
            if (ip == null)
                throw new Exception("could not find a IPv4 address on local host");
            return ip;
        }

        /// <summary>
        /// This method is used for obtaining the IPv6 IPAddress object for FTP active
        /// connection (EPRT).
        /// </summary>
        /// <returns></returns>
        private IPAddress GetClientIPv6Address()
        {
            string localHostName = Dns.GetHostName();
            IPAddress[] list = Dns.GetHostAddresses(localHostName);
            IPAddress ip = null;
            foreach (IPAddress a in list)
            {
                // look for IPv6 addresses only (InterNetworkV6)
                if (a.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    ip = a;
                    break;
                }
            }
            if (ip == null)
                throw new Exception("could not find a IPv6 address on local host");
            return ip;

        }
        
        private int OpenActiveListenerPort(string localHost, IPAddress localAddr)
        {
            bool success = false;
            int listenerPort = 0;

            do
            {
                int failureCnt = 0;
                try
                {
                    listenerPort = GetNextActivePort();
                    _activeListener = new TcpListener(localAddr, listenerPort);
                    _activeListener.Start();
                    success = true;
                }
                catch (SocketException socketError)
                {
                    // test to see if the socket is no good
                    if (socketError.ErrorCode == 10048 
                        && ++failureCnt < (_activePortRangeMax - _activePortRangeMin))
                        _activeListener.Stop();
                    else
                        throw new FtpsDataConnectionException(String.Format(CultureInfo.InvariantCulture, "An error occurred while trying to create an active connection on host {0} port {1}", localHost, listenerPort.ToString(CultureInfo.InvariantCulture)), socketError);
                }
            } while (!success);

            // begin accepting tcp/ip data from our newly created listener port
            // data is accepted by the async AcceptTcpClientCallback method
            _activeListener.BeginAcceptTcpClient(new AsyncCallback(this.AcceptTcpClientCallback), _activeListener);

            return listenerPort;
        }

        // async callback that occurs once the server has connected to the client listener data connection
        private void AcceptTcpClientCallback(IAsyncResult ar)
        {
            // Get the listener that handles the client request.
            TcpListener listener = (TcpListener)ar.AsyncState;

            // make sure that the server didn't close the connection on us or just refuse to allow an active connection
            // for security considerations and other purposes some servers will refuse active connections
            try
            {
                _dataConn = listener.EndAcceptTcpClient(ar);
            }
            catch 
            { 
                // should we catch this and do something?
            }

            // signal the calling thread to continue now that the data connection is open
            _activeSignal.Set();       
        }

        private void OpenDataConn()
        {
            //  create the approiate ftp data connection depending on how the ftp client should send 
            //  or receive data from the ftp server
            if (_dataTransferMode == TransferMode.Active)
                CreateActiveConn();
            else
                CreatePassiveConn();
        }

        private void CloseDataConn()
        {
            //  close the tcpclient data connection object
            if (_dataConn != null)
            {
                try
                {
                    _dataConn.Close();
                }
                catch  { }
                _dataConn = null;
            }

            //  stop the tcplistner object if we used it for an active data transfer where we
            //  are listing and the server makes a connection to the client and pushed data 
            if (_dataTransferMode == TransferMode.Active && _activeListener != null)
            {
                try
                {
                    _activeListener.Stop();
                }
                catch { }
                _activeListener = null;
            }
        }

        private void WaitForDataConn()
        {
            //  if the transfer mode is active then we have to open a listener and wait for the server to connect before sending
            //  or receiving data but if the transfer mode is passive then we make the connection (non blocking) 
            //  and therefore there is no need to wait for the server and our signal that the server has connected 
            //  - we already have the connection object since the tcpclient object blocks until the server accepts the connection
            if (_dataTransferMode == TransferMode.Active)
            {
                // wait until a data connection is made before continuing based on a thread blocking signal object
                if (!_activeSignal.WaitOne(_transferTimeout, false))
                {
                    if (_response.Code == FtpsResponseCode.CannotOpenDataConnection)
                        throw new FtpsDataConnectionException(String.Format(CultureInfo.InvariantCulture, "The ftp destination was unable to open a data connection to the ftp client on port {0}.", _activePort));
                    else
                        throw new FtpsDataConnectionTimeoutException("The data connection timed out waiting for data to transfer from the destination.");
                }
            }
            else
                return;
        }

        private void CreatePassiveConn()
        {
            string host;
            int port;

            SendPassiveCmd(out host, out port);

            try
            {
                //  create a new tcp client object and use proxy if supplied
                if (_proxy != null)
                    _dataConn = _proxy.CreateConnection(host, port);
                else
                    _dataConn = new TcpClient(host, port);

                _dataConn.ReceiveBufferSize = _tcpBufferSize;
                _dataConn.ReceiveTimeout = _tcpTimeout;
                _dataConn.SendBufferSize = _tcpBufferSize;
                _dataConn.SendTimeout = _tcpTimeout;
            }
            catch (Exception ex)
            {
                throw new FtpsDataConnectionException(String.Format(CultureInfo.InvariantCulture, "An error occurred while opening passive data connection to destination '{0}' on port '{1}'.", host, port), ex);
            }
        }

        private void SendPassiveCmd(out string host, out int port)
        {
            switch (_networkProtocol)
            {
                case NetworkVersion.IPv4:
                    // some FileZilla servers appear to allow EPSV but not PASV even on IPv4 so we test
                    // for EPSV support and if available use it; otherwise use PASV
                    if (Features.Count > 0 && Features.Contains(FtpsCmd.Epsv) && !Features.Contains(FtpsCmd.Pasv))
                    {
                        SendEpsvCmd(out host, out port); 
                    }
                    else
                    {
                        SendPasvCmd(out host, out port);
                    }

                    break;
                case NetworkVersion.IPv6:
                    SendEpsvCmd(out host, out port);
                    break;
                default:
                    throw new FtpsException("unexpected network protocol version");
            }

        }

        /// <summary>
        /// Send the Extended Passive Mode (EPSV) command to the FTP host server.
        /// </summary>
        /// <remarks>
        /// The EPSV command tells the server to enter a passive FTP session rather than Active. 
        /// (Its use is required for IPv6.) This allows users behind routers/firewalls to connect 
        /// over FTP when they might not be able to connect over an Active (PORT/EPRT) FTP session. 
        /// EPSV mode has the server tell the client where to connect for the data port on the server.
        /// </remarks>
        /// <param name="host">Host name returned by the FTP server or the default host name.</param>
        /// <param name="port">Port number to connect to returned by FTP server.</param>
        private void SendEpsvCmd(out string host, out int port)
        {
            //  send command to get passive port to be used from the server
            //  using the extended port command which works with IPv4 and IPv6
            //  and is defined in RFC 2428
            try
            {
                // EPSV<sp><opt-tcp-port-request>
                SendRequest(new FtpsRequest(_encode, FtpsCmd.Epsv));
            }
            catch (FtpsException fex)
            {
                throw new FtpsDataConnectionException("An error occurred while issuing up a passive FTP connection command.", fex);
            }

            // (<d><net-prt><d><net-addr><d><tcp-port><d>)
            // net-prt:  1 - IPv4 or  2 - IPv6 (optional)
            // net-addr:  network address IPv4 or IPv6 (optional)
            // tcp-port:  listening port of FTP server (required)
            //
            // Example 1:
            //      Client> EPSV
            //      Server> 229 Entering Extended Passive Mode (|1|132.235.1.2|63643|)
            //
            // Example 2:
            //      Client> EPSV
            //      Server> 229 Entering Extended Passive Mode (|2|1080::8:800:200C:417A|5282|)
            //
            // Example 3:
            //      Client> EPSV
            //      Server> 229 Entering Extended Passive Mode (|||3282|)
            string r = _response.Text;
            int startIdx = r.IndexOf("(") + 1;
            int endIdx = r.IndexOf(")");

            //  parse the transfer connection data from the ftp server response
            string[] data = r.Substring(startIdx, endIdx - startIdx).Split('|');

            // build the data host name from the server response
            // if one is supplied, otherwise use connected host info
            if (data[2].Length != 0)
                host = data[2];
            else
                host = _host;
            // extract and convert the port number from the server response
            port = Int32.Parse(data[3], CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Send the Passive Mode (PASV) command to the FTP host server.
        /// </summary>
        /// <remarks>
        /// This command tells the server to enter a passive FTP session rather than Active. 
        /// This allows users behind routers/firewalls to connect over FTP when they might not 
        /// be able to connect over an Active (PORT) FTP session. PASV mode has the server tell 
        /// the client where to connect for the data port on the server.  PASV mode only works
        /// with IPv4.  The Extended Passive Mode command must be used for IPv6 FTP host addresses.
        /// </remarks>
        /// <param name="host">Host name returned by the FTP server or the default host name.</param>
        /// <param name="port">Port number to connect to returned by FTP server.</param>
        private void SendPasvCmd(out string host, out int port)
        {
            //  send command to get passive port to be used from the server
            try
            {
                SendRequest(new FtpsRequest(_encode, FtpsCmd.Pasv));
            }
            catch (FtpsException fex)
            {
                throw new FtpsDataConnectionException("An error occurred while issuing up a passive FTP connection command.", fex);
            }

            //  get the port on the end
            //  to calculate the port number we must multiple the 5th value by 256 and then add the 6th value
            //  example:  
            //       Client> PASV
            //       Server> 227 Entering Passive Mode (123,45,67,89,158,26)
            //  In the example of PASV mode the server has said it will be listening on IP address 123.45.67.89 
            //  on TCP port 40474 for the data channel. (Note: the destinationPort is the 158,26 pair and is: 158x256 + 26 = 40474).

            //  get the begin and end positions to extract data from the response string
            string r = _response.Text;
            int startIdx = r.IndexOf("(") + 1;
            int endIdx = r.IndexOf(")");

            //  parse the transfer connection data from the ftp server response
            string[] data = r.Substring(startIdx, endIdx - startIdx).Split(',');

            // build the data host name from the server response
            host = data[0] + "." + data[1] + "." + data[2] + "." + data[3];
            // extract and convert the port number from the server response
            port = Int32.Parse(data[4], CultureInfo.InvariantCulture) * 256 
                + Int32.Parse(data[5], CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Creates an SSL or TLS secured stream.
        /// </summary>
        /// <param name="stream">Unsecured stream.</param>
        /// <returns>Secured stream</returns>
        private Stream CreateSslStream(Stream stream)
        {
            // create an SSL or TLS stream that will close the client's stream
            SslStream ssl = new SslStream(stream, true, new RemoteCertificateValidationCallback(secureStream_ValidateServerCertificate), null);

            // choose the protocol
            SslProtocols protocol = SslProtocols.None;
            switch (_securityProtocol)
            {
                case FtpsSecurityProtocol.Tls1OrSsl3Explicit:
                case FtpsSecurityProtocol.Tls1OrSsl3Implicit:
                    protocol = SslProtocols.Default;
                    break;
                case FtpsSecurityProtocol.Ssl2Explicit:
                case FtpsSecurityProtocol.Ssl2Implicit:
                    protocol = SslProtocols.Ssl2;
                    break;
                case FtpsSecurityProtocol.Ssl3Explicit:
                case FtpsSecurityProtocol.Ssl3Implicit:
                    protocol = SslProtocols.Ssl3;
                    break;
                case FtpsSecurityProtocol.Tls1Explicit:
                case FtpsSecurityProtocol.Tls1Implicit:
                    protocol = SslProtocols.Tls;
                    break;
                case FtpsSecurityProtocol.Tls11Explicit:
                case FtpsSecurityProtocol.Tls11Implicit:
                    protocol = SslProtocols.Tls11;
                    break;
                case FtpsSecurityProtocol.Tls12Explicit:
                case FtpsSecurityProtocol.Tls12Implicit:
                    protocol = SslProtocols.Tls12;
                    break;
                default:
                    throw new FtpsSecureConnectionException(String.Format("unexpected FtpSecurityProtocol type '{0}'", _securityProtocol.ToString()));
            }

            // note: the server name must match the name on the server certificate.
            try
            {
                // authenticate the client
                ssl.AuthenticateAsClient(_host, _clientCertificates, protocol, true);
            }
            catch (AuthenticationException authEx)
            {
                throw new FtpsAuthenticationException("Secure FTP session certificate authentication failed.", authEx);
            }

            return ssl;
        }

        // the following method is invoked by the RemoteCertificateValidationDelegate.
        private bool secureStream_ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // if the option is set to always accept the certificate then do not fire the validate certificate event
            if (_alwaysAcceptServerCertificate)
                return true;

            // if it is the same certificate we have already approved then don't validate it again
            if (_serverCertificate != null && certificate.GetCertHashString() == _serverCertificate.GetCertHashString())
                return true;

            // invoke the ValidateServerCertificate event if the user is subscribing to it
            // ignore our own logic and let the user decide if the certificate is valid or not
            if (ValidateServerCertificate != null)
            {
                ValidateServerCertificateEventArgs args = new ValidateServerCertificateEventArgs(new X509Certificate2(certificate.GetRawCertData()), chain, sslPolicyErrors);
                ValidateServerCertificate(this, args);
                // make a copy of the certificate due to sharing violations
                if (args.IsCertificateValid)
                    _serverCertificate = new X509Certificate2(certificate.GetRawCertData());
                return args.IsCertificateValid;
            }
            else
            {
                // analyze the policy errors and decide if the certificate should be accepted or not.
                if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNameMismatch) == SslPolicyErrors.RemoteCertificateNameMismatch)
                    throw new FtpsCertificateValidationException(String.Format("Certificate validation failed.  The host name '{0}' does not match the name on the security certificate '{1}'.  To override this behavior, subscribe to the ValidateServerCertificate event to validate certificates.", _host, certificate.Issuer));

                if (sslPolicyErrors == SslPolicyErrors.None || (sslPolicyErrors & SslPolicyErrors.RemoteCertificateChainErrors) == SslPolicyErrors.RemoteCertificateChainErrors)
                {
                    // make a copy of the server certificate due to sharing violations
                    _serverCertificate = new X509Certificate2(certificate.GetRawCertData());
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        
        private void CreateSslExplicitCommandStream()
        {
            try
            {
                // send authentication type request
                string authCommand = "";
                switch (_securityProtocol)
                {
                    case FtpsSecurityProtocol.Tls1OrSsl3Explicit:
                    case FtpsSecurityProtocol.Ssl3Explicit:
                    case FtpsSecurityProtocol.Ssl2Explicit:
                        authCommand = "SSL";
                        break;
                    case FtpsSecurityProtocol.Tls1Explicit:
                    case FtpsSecurityProtocol.Tls11Explicit:
                    case FtpsSecurityProtocol.Tls12Explicit:
                        authCommand = "TLS";
                        break;
                }

                Debug.Assert(authCommand.Length > 0, "auth command should have a value - make sure every enum option in auth command has a corresponding value");

                SendRequest(new FtpsRequest(_encode, FtpsCmd.Auth, authCommand));

                // set the active command stream to the ssl command stream object
                lock (_reponseMonitorLock)
                {
                    _commandStream = CreateSslStream(_commandConn.GetStream());
                }

                // send a command to the FTP server indicating that traffic is now protected
                SendRequest(new FtpsRequest(_encode, FtpsCmd.Pbsz, "0"));
                SendRequest(new FtpsRequest(_encode, FtpsCmd.Prot, "P"));
            }
            catch (FtpsAuthenticationException fauth)
            {
                throw new FtpsSecureConnectionException(String.Format("An ftp authentication exception occurred while setting up a explicit ssl/tls command stream.  {0}", fauth.Message), _response, fauth);                
            }
            catch (FtpsException fex)
            {
                throw new FtpsSecureConnectionException(String.Format("An error occurred while setting up a explicit ssl/tls command stream.  {0}", fex.Message), _response, fex);
            }

        }

        private void CreateSslImplicitCommandStream()
        {
            try
            {
                // set the active command stream to the ssl command stream object
                lock (_reponseMonitorLock)
                {
                    _commandStream = CreateSslStream(_commandConn.GetStream());
                }

                // send a command to the FTP server indicating that traffic is now protected
                if (_sendProtPForImplicitSslConnections) {
                    SendRequest(new FtpsRequest(_encode, FtpsCmd.Prot, "P"));
                }
            }
            catch (FtpsAuthenticationException fauth)
            {
                throw new FtpsSecureConnectionException(String.Format("An ftp authentication exception occurred while setting up a implicit ssl/tls command stream.  {0}", fauth.Message), _response, fauth);
            }
            catch (FtpsException fex)
            {
                throw new FtpsSecureConnectionException(String.Format("An error occurred while setting up a implicit ssl/tls command stream.  {0}", fex.Message), _response, fex);
            }

        }

        private void DoIntegrityCheck(FtpsRequest request, Stream stream, long restartPosition)
        {
            // get the file path from the request argument
            string path = request.Arguments[0];
            long startPos = restartPosition;
            long endPos = stream.Length;

            string streamHash = ComputeHash(_autoHashAlgorithm, stream, startPos);
            string serverHash = GetHash(_autoHashAlgorithm, path, startPos, endPos);

            streamHash = streamHash.ToLower();
            serverHash = serverHash.ToLower();

            // string compare the dataHash to the server hash value and see if they are the same
            if (serverHash.IndexOf(streamHash, 0) == -1)
                throw new FtpsHashingException(String.Format("File integrity check failed.  The destination hash value '{0}' for the file '{1}' did not match the data transfer hash value '{2}'.", serverHash, path, streamHash));

            //// string compare the dataHash to the server hash value and see if they are the same
            //if (String.Compare(streamHash, serverHash, StringComparison.InvariantCultureIgnoreCase) != 0)
            //    throw new FtpHashingException(String.Format("File integrity check failed.  The destination hash value '{0}' for the file '{1}' did not match the data transfer hash value '{2}'.", serverHash, path, streamHash));
        }

        private void DontWaitForHappyCodes()
        {
            if (_responseQueue.Count == 0)
                return;

            _responseList.Clear();
            while (_responseQueue.Count > 0)
            {
                FtpsResponse response = _responseQueue.Dequeue();
                _responseList.Add(response);
                RaiseServerResponseEvent(new FtpsResponse(response));
            }
            _response = _responseList.GetLast();
        }

        private Stream CreateZlibStream(TransferDirection direction, Stream stream)
        {
            DeflateStream zstream = null;

            switch (direction)
            {
                case TransferDirection.ToClient:
                    zstream = new DeflateStream(stream, CompressionMode.Decompress, true);
                    // zlib fix to ignore first two bytes of header data 
                    zstream.BaseStream.ReadByte();
                    zstream.BaseStream.ReadByte();
                    break;

                case TransferDirection.ToServer:
                    zstream = new DeflateStream(stream, CompressionMode.Compress, true);
                    // this is a fix for the DeflateStream class only when sending compressed data to the server.  
                    // Zlib has two bytes of data attached to the header that we have to write before processing the data stream.
                    zstream.BaseStream.WriteByte(120);
                    zstream.BaseStream.WriteByte(218);
                    break;
            }

            return zstream;
        }

         private FtpsFeatureArgument GetHashFeatureArgument(HashingAlgorithm algo)
        {
            string argText = ConvertHashAlgoToTextArg(algo);

            FtpsFeature f = Features[FtpsCmd.Hash];
            if (f == null)
                throw new FtpsCommandNotSupportedException("Cannot find hash feature.", FtpsCmd.Hash);

            FtpsFeatureArgument fa = f.Arguments[argText];
            if (fa == null)
                throw new FtpsFeatureException(String.Format("The HASH feature argument '{0}' is not supported by the FTP server.", argText));

            return fa;
        }

        private FtpsCmd ConvertHashXAlgoToTextCmd(HashingAlgorithm hash)
        {
            switch (hash)
            {
                case HashingAlgorithm.Crc32:
                    return FtpsCmd.Xcrc;
                case HashingAlgorithm.Md5:
                    return FtpsCmd.Xmd5;
                case HashingAlgorithm.Sha1:
                    return FtpsCmd.Xsha1;
                case HashingAlgorithm.Sha256:
                    return FtpsCmd.Xsha256;
                case HashingAlgorithm.Sha512:
                    return FtpsCmd.Xsha512;
                default:
                    throw new FtpsException("unsupported hash option");
            }
        }

        private string ConvertHashAlgoToTextArg(HashingAlgorithm hash)
        {
            switch (hash)
            {
                case HashingAlgorithm.Crc32:
                    return HASH_CRC_32;
                case HashingAlgorithm.Md5:
                    return HASH_MD5;
                case HashingAlgorithm.Sha1:
                    return HASH_SHA_1;
                case HashingAlgorithm.Sha256:
                    return HASH_SHA_256;
                case HashingAlgorithm.Sha512:
                    return HASH_SHA_512;
                default:
                    throw new FtpsException("unsupported hash option");
            }
        }
        
        internal bool IsTransferProgressEventSet()
        {
            return (TransferProgress != null) ? true : false;
        }

        internal void SetTransferSize(long size)
        {
            _transferSize = size;
        }

        #endregion

        #region Destructors

        /// <summary>
        /// Disposes all objects and connections.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Dispose Method.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_asyncWorker != null && _asyncWorker.IsBusy)
                    _asyncWorker.CancelAsync();

                if (_activeListener != null)
                    _activeListener.Stop();

                if (_dataConn != null && _dataConn.Connected)
                    _dataConn.Close();

                if (_commandConn != null && _commandConn.Connected)
                    _commandConn.Close();

                if (_activeSignal != null)
                    _activeSignal.Close();

                if (_responseMonitor != null && _responseMonitor.IsAlive)
                    _responseMonitor.Abort();
            }
        }

        /// <summary>
        /// Dispose deconstructor.
        /// </summary>
        ~FtpsBase()
        {
            Dispose(false);
        }

        #endregion
    }
}
