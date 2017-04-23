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
using System.IO;
using System.Text;
using System.Threading;
using System.Globalization;
using System.ComponentModel;
using Starksoft.Aspen.Proxy;

namespace Starksoft.Aspen.Ftps
{
    
#region Public Enums
    /// <summary>
    /// Enumeration representing type of file transfer mode.
    /// </summary>
    public enum TransferType : int
    {
        /// <summary>
        /// No transfer type.
        /// </summary>
        None,
        /// <summary>
        /// Transfer mode of type 'A' (ascii).
        /// </summary>
        Ascii, 
        /// <summary>
        /// Transfer mode of type 'I' (image or binary)
        /// </summary>
        Binary,
        /// <summary>
        /// Unknown transfer mode.
        /// </summary>
        Unknown
    }

    /// <summary>
    /// Enumeration representing the three types of actions that FTP supports when
    /// uploading or 'putting' a file on an FTP server from the FTP client.  
    /// </summary>
    public enum FileAction : int
    {
        /// <summary>
        /// No action.
        /// </summary>
        None,
        /// <summary>
        /// Create a new file or overwrite an existing file.
        /// </summary>
        Create,
        /// <summary>
        /// Create a new file.  Do not overwrite an existing file.
        /// </summary>
        CreateNew,
        /// <summary>
        /// Create a new file or append an existing file.
        /// </summary>
        CreateOrAppend,
        /// <summary>
        /// Resume a file transfer.
        /// </summary>
        Resume,
        /// <summary>
        /// Resume a file transfer if the file already exists.  Otherwise, create a new file.
        /// </summary>
        ResumeOrCreate
    }

    /// <summary>
    /// FTP command and response encoding.
    /// </summary>
    public enum CharEncoding : int
    {
        /// <summary>
        /// Use ASCII encoding to send commands and receive responses.
        /// </summary>
        Ascii,
        /// <summary>
        /// Use UTF-8 encoding to send commands and receive responses.  
        /// </summary>
        Utf8
    }

    /// <summary>
    /// Enumeration representing the type of file and directory listing method to use and commands to issue
    /// to the FTP server to retrieve file and directory information.
    /// </summary>
    public enum ListingMethod : int
    {
        /// <summary>
        /// Always use the standard LIST command with no additional argument information when retrieving a directory listing from the FTP server.  
        /// </summary>
        List,
        /// <summary>
        /// Always use the standard LIST command with the additional argument '-al' when retrieving a directory listing from the FTP server.  
        /// </summary>
        ListAl,
        /// <summary>
        /// Always use the RFC 3659 MLSx commands when retrieving a directory listing from the FTP server.  
        /// </summary>
        Mlsx,
        /// <summary>
        /// Let the Starksoft FtpsClient figure out which option to use when retrieve a directory listing from the FTP server.  
        /// </summary>
        /// <remarks>
        /// The Starksoft FtpsClient will first attempt to use the MLSx command, if that command is not available it will automatically downgrade to the
        /// LIST 'al' command.  
        /// </remarks>
        Automatic
    }

#endregion

/// <summary>
/// The Startsoft FtpsClient library for .NET is a fully .NET coded RFC 959 compatible FTP object component that supports the RFC 959, SOCKS and HTTP proxies, SSLv2, SSLv3, and TLSv1
/// (explicit and implicit) as well as automatic file integrity checks on all data transfers.  The component library also supports a pluggable directory listing parser that can be extended 
/// by the developer to parse unique directory formats returned by odd or old FTP servers.  Most methods are available to call in a standard blocking pattern or the non-blocking .NET 2.0 
/// Async pattern which features a callback event handler that returns the result on background thread.
/// </summary>
/// <remarks>
/// <para>
/// The FtpsClient supports following FTP commands: 
///     USER    RMD     CDUP    CWD     STOU    RETR    AUTH    XSHA512  CLNT
///     PASS    RETR    DELE    PORT    APPE    MDTM    PROT    OPTS     HASH
///     QUIT    PWD     TYPE    PASV    REST    SIZE    MODE    SITE     RANG
///     MKD     SYST    MODE    STOR    RNFR    FEAT    XSHA1   CHMOD    MLST
///     NLST    HELP    RNTO    SITE    ALLO    QUIT    XMD5    MFMT     MLSD
///     ABORT   STAT    LIST    NOOP    PBSZ    XCRC    XSHA256 MFCT
/// </para>
/// <para>
/// Custom FTP server commands can be executed using the Quote() method.  This allows the FtpsClient object to handle
/// certain custom commands that are not supported by the RFC 959 standard but are required by specific FTP server
/// implementations for various tasks.  
/// </para>
    /// <code>
    /// string result = "";
    /// using (FtpsClient ftp = new FtpsClient("ftp.gnu.org"))
    /// {
    ///     result = ftp.Quote("SITE ZONE"); 
    /// } // using end bracket closes connection
    /// </code>
    /// <para>
/// The Starksoft FtpsClient Component for .NET supports SOCKS v4, SOCKS v4a, SOCKS v5, and HTTP proxy servers.  The proxy settings are not read
/// from the local web browser proxy settings so deployment issues are not a problem with using proxy connections.  In addition the library also
/// supports active and passive (firewall friendly) mode transfers.  The Starksoft FtpsClient Component for .NET supports data compression, bandwidth throttling,
/// and secure connections through SSL (Secure Socket Layer) and TLS.  The Starksoft FtpsClient Component for .NET also supports automatic transfer integrity checks via 
/// CRC, MD5, SHA1, SHA-256, and SHA-512.  The FtpsClient object can parse many different directory listings from various FTP server implementations.  But for those servers that are difficult to 
/// parse of produce strange directory listings you can write your own ftp item parser.  See the IFtpItemParser interface
/// for more information and an example parser.     
/// </para>
/// <para>
/// The FtpsClient libary has been tested with the following FTP servers and file formats.
/// <list type="">
///     <item>FileZilla</item>
///     <item>IIS Microsoft Windows 2000 and Windows 2003 server</item>
///     <item>Microsoft FTP server running IIS 5.0</item>
///     <item>Gene6FTP Server</item>
///     <item>ProFTPd</item>
///     <item>Wu-FTPd</item>
///     <item>WS_FTP Server (by Ipswitch)</item>
///     <item>Serv-U FTP Server</item>
///     <item>GNU FTP server</item>
///     <item>Many public FTP servers</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// using (FtpsClient ftp = new FtpsClient("ftp.gnu.org"))
/// {
///     ftp.DataTransferMode = DataTransferMode.Passive; 
///     ftp.Open("anonymous", "myemail@host.com");
///     ftp.GetFile("somefile", "c:\\somefile"); 
/// } // using end bracket closes connection
/// </code>
/// </example>
	public class FtpsClient : FtpsBase
    {
#region Contructors

        /// <summary>
        /// FtpsClient default constructor.
        /// </summary>
        public FtpsClient() : base(DEFAULT_FTP_PORT, FtpsSecurityProtocol.None) 
        {  }

        /// <summary>
        /// Constructor method for FtpsClient.  
        /// </summary>
        /// <param name="host">String containing the host name or ip address of the remote FTP server.</param>
        /// <remarks>
        /// This method takes one parameter to specify
        /// the host name (or ip address).
        /// </remarks>
        public FtpsClient(string host) : this(host, DEFAULT_FTP_PORT, FtpsSecurityProtocol.None) 
        {   }

        /// <summary>
        /// Constructor method for FtpsClient.  
        /// </summary>
        /// <param name="host">String containing the host name or ip address of the remote FTP server.</param>
        /// <param name="port">Port number used to connect to the remote FTP server.</param>
        /// <remarks>
        /// This method takes two parameters that specify 
        /// the host name (or ip address) and the port to connect to the host.
        /// </remarks>
        public FtpsClient(string host, int port) : base(host, port, FtpsSecurityProtocol.None) 
        {  }

        /// <summary>
        /// Constructor method for FtpsClient.  
        /// </summary>
        /// <param name="host">String containing the host name or ip address of the remote FTP server.</param>
        /// <param name="port">Port number used to connect to the remote FTP server.</param>
        /// <param name="securityProtocol">Enumeration value indicating what security protocol (such as SSL) should be enabled for this connection.</param>
        /// <remarks>
        /// This method takes three parameters that specify 
        /// the host name (or ip address), port to connect to and what security protocol should be used when establishing the connection.
        /// </remarks>
        public FtpsClient(string host, int port, FtpsSecurityProtocol securityProtocol) : base(host, port, securityProtocol) 
        {  }

#endregion

#region Private Variables and Constants

        private const string FTP_CLIENT_NAME = "Starksoft";
        private const int DEFAULT_FTP_PORT = 21; // default port is 21
        private const int FXP_TRANSFER_TIMEOUT = 600000; // 10 minutes
        private const string TYPE_BINARY_I = "I";
        private const string TYPE_ASCII_A = "A";

        // file and directory listing variables
        private TransferType _fileTransferType = TransferType.Binary;
        private ListingMethod _dirListingMethod = ListingMethod.Automatic;
        private IFtpsItemParser _listItemParser;
        private IFtpsItemParser _mlsxItemParser;
        private string _currentDirectory;

        private string _user;
        private string _password;
        private bool _opened;
        private int _fxpTransferTimeout = FXP_TRANSFER_TIMEOUT;

        // transfer log
        private Stream _log = new MemoryStream();
        private bool _isLoggingOn;

#endregion

#region Public Properties

        /// <summary>
        /// Gets the FTP character encoding used in transfers.
        /// </summary>
        public CharEncoding CharacterEncoding
        {
            get
            {
                if (base.Encoding == Encoding.ASCII)
                    return CharEncoding.Ascii;
                else if (base.Encoding == Encoding.UTF8)
                    return CharEncoding.Utf8;
                else
                    throw new FtpsException("unexpected encoding type");
            }
        }

        /// <summary>
        /// Gets or sets the file transfer item.
        /// </summary>
        public TransferType FileTransferType
        {
            get { return _fileTransferType; }
            set
            {
                if (this.IsBusy)
                    throw new FtpsBusyException("FileTransferType");
                if (this.IsConnected == true)
                {
                    //  update the server with the new file transfer type
                    SetFileTransferType(value);
                }
                _fileTransferType = value;
            }
        }

        /// <summary>
        /// Gets or sets the the directory item parser to use when parsing directory listing data from the FTP server.
        /// This parser is used by the GetDirList() and GetDirList(string) methods.  
        /// </summary>
        /// <remarks>
        /// You can create your own custom directory listing parser by creating an object that implements the 
        /// IFtpItemParser interface.  This is particular useful when parsing exotic file directory listing
        /// formats from specific FTP servers.
        /// 
        /// This parser is only used when the LIST command is executed.  You can indicate the type of command
        /// behavior by setting the DirListingMethod property.  This ItemParser may be used if the DirListingMethod
        /// property is set to List, ListAl, or Automatic.
        /// </remarks>
        public IFtpsItemParser ItemParser
        {
            get { return _listItemParser; }
            set 
            {
                if (this.IsBusy)
                    throw new FtpsBusyException("ItemParser");
                _listItemParser = value; 
            
            }
        }

        /// <summary>
        /// Gets or sets logging of file transfers.
        /// </summary>
        /// <remarks>
        /// All data transfer activity can be retrieved from the Log property.
        /// </remarks>
        public bool IsLoggingOn
        {
            get { return _isLoggingOn; }
            set 
            {
                if (this.IsBusy)
                    throw new FtpsBusyException("IsLoggingOn");
                _isLoggingOn = value; 
            }
        }

        /// <summary>
        /// Gets or sets the Stream object used for logging data transfer activity.
        /// </summary>
        /// <remarks>
        /// By default a MemoryStream object is created to log all data transfer activity.  Any 
        /// Stream object that can be written to may be used in place of the MemoryStream object.
        /// </remarks>
        /// <seealso cref="IsLoggingOn"/>
        public Stream Log
        {
            get { return _log; }
            set
            {
                if (this.IsBusy)
                    throw new FtpsBusyException("Log");
                if (((Stream)value).CanWrite == false)
                    throw new ArgumentException("must be writable. The property CanWrite must have a value equals to 'true'.", "value");
                _log = value;
            }
        }

        /// <summary>
        /// Gets or sets the timeout value in miliseconds when waiting for an FXP server to server transfer to complete.
        /// </summary>
        /// <remarks>By default this timeout value is set to 600000 (10 minutes).  For large FXP file transfers you may need to adjust this number higher.</remarks>
        public int FxpTransferTimeout
        {
            get { return _fxpTransferTimeout; }
            set 
            {
                if (this.IsBusy)
                    throw new FtpsBusyException("FxpTransferTimeout");
                _fxpTransferTimeout = value; 
            }
        }

        /// <summary>
        /// Gets the current directory path without sending having to send a request to the server.
        /// </summary>
        /// <seealso cref="GetWorkingDirectory"/>
        public string CurrentDirectory
        {
            get { return _currentDirectory; }
        }

        /// <summary>
        /// Gets or sets the method how directory and file listing commands will be executed on the FTP server.
        /// </summary>
        /// <remarks>
        /// The enmeration options for the listing method affects how the methods GetDirList(), GetDirListAsText(), GetDirListAsync(), GetDirListDeep(),
        /// and GetDirListDeepAsync() function.  Different options will result in different commands being executed on the FTP server in order to option
        /// directory listing information.  The most widely flexible option is the List option which will executed the LIST command on the FTP server 
        /// and option unstructured or semistructured file and directory information.  The Starksoft FtpsClient will then parse that data as and present
        /// a FtpListCollection that contains FtpListItem objects.  
        /// 
        /// Some FTP servers will not return detailed listing data with the UNIX file permissions
        /// either because they do not support them or those FTP servers require an argument to be specified with the LIST command.  That argument is 
        /// the value '-al' which is a UNIX specific argument to instruct the server to list all the information about the files and not simply the file
        /// names.  The problem is that not all FTP servers will accept this argument.  To Use the optional argument choose the enumeration option ListAll.
        /// 
        /// More modern FTP servers implement the a much more structured method to retrieve file and directory listings from a FTP server.  Two new commands 
        /// were defined in RFC 3659.  Those new commands are MLST and MLSD or MLSx for short.  The MLSx commands instruct the FTP server to send much more
        /// structured data about the files and directories located on the FTP server.  This helps reduce or eliminate the errors that are common when 
        /// parsing different type types of directory listing supported by various FTP servers.  Unfortunately, many FTP serves do not support the MLSx
        /// commands.  
        /// 
        /// You can specify that this type of command should be used when retrieving a directory listing from a specific FTP server by
        /// selecting the enumeration option Mlsx.  This option will instruct the Starksoft FtpsClient to use the MLSx commands instead of the LIST command
        /// to retrive directory listing information.  The standard FtpItemCollection will be returned but it will now include objects of class type
        /// FtpMlsxItem which is a sub-class of the FtpListItem.  The FtpMlsxItem class contains additional information that may be available and supported
        /// by your FTP server.  Yet, not all FTP servers that support MLSx commands support all the file and directory facts defined by RFC 3659.  Each
        /// FTP server will support a different range of facts.  In order to support these differences, the FtpMlsxItem class has many nullable data types.
        /// </remarks>
        /// <seealso cref="ListingMethod"/>
        /// <seealso cref="GetDirList()"/>
        /// <seealso cref="GetDirList(string)"/>
        /// <seealso cref="GetDirListAsText()"/>
        /// <seealso cref="GetDirListAsText(string)"/>
        /// <seealso cref="GetDirListAsync()"/>
        /// <seealso cref="GetDirListAsync(string)"/>
        /// <seealso cref="GetDirListDeep(string)"/>
        /// <seealso cref="GetDirListDeepAsync(string)"/>
        /// <seealso cref="GetNameList()"/>
        /// <seealso cref="GetNameList(string)"/>
        public ListingMethod DirListingMethod
        {
            get { return _dirListingMethod; }
            set 
            {
                if (base.IsBusy)
                    throw new FtpsBusyException("DirListingMethod");
                _dirListingMethod = value; 
            }
        }

#endregion

#region Public Methods

        /// <summary>
        /// Opens a connection to the remote FTP server and log in with user name and password credentials.
        /// </summary>
        /// <param name="user">User name.  Many public ftp allow for this value to 'anonymous'.</param>
        /// <param name="password">Password.  Anonymous public ftp servers generally require a valid email address for a password.</param>
        /// <remarks>Use the Close() method to log off and close the connection to the FTP server.</remarks>
        /// <seealso cref="OpenAsync"/>
        /// <seealso cref="Close"/>
        /// <seealso cref="Reopen"/>
        public void Open(string user, string password)
        {
            if (user == null)
                throw new ArgumentNullException("user", "must have a value");

            if (user.Length == 0)
                throw new ArgumentException("must have a value", "user");

            if (password == null)
                throw new ArgumentNullException("password", "must have a value or an empty string");

            // if the command connection is not already open then open a new command connect
            if (!this.IsConnected)
                base.OpenCommandConn();

            // mark the connection as opened
            _opened = true;

            // create the LIST and MLSx item parsers
            CreateItemParsers();

            if (CatchUserCancel()) return;

            _user = user;
            _password = password;
            _currentDirectory = "/";

            SendUser(user);

            // wait for user to log into system and all response messages to be transmitted
            Thread.Sleep(500);

            if (CatchUserCancel()) return;

            SendPassword(password);

            if (CatchUserCancel()) return;

            TrySetFeatures();
            TrySetClient(); 
            TrySetUtf8On();
            TrySetFileTransferType();
            SetCompression();
            
            if (CatchUserCancel()) return;

        }

        private void VerifyOpened()
        {
            if (!_opened || !base.IsConnected)
                throw new FtpsException("Connection is closed.  Unable to perform action.");
        }

        /// <summary>
        /// Reopens a lost ftp connection.
        /// </summary>
        /// <remarks>
        /// If the connection is currently open or the connection has never been open and FtpException is thrown.
        /// </remarks>
        public void Reopen()
        {
            VerifyOpened();            
            // reopen the connection with the same username and password
            Open(_user, _password);
        }
        
        /// <summary>
        /// Change the currently logged in user to another user on the FTP server.
        /// </summary>
        /// <param name="user">The name of user.</param>
        /// <param name="password">The password for the user.</param>
        public void ChangeUser(string user, string password)
        {
            if (user == null)
                throw new ArgumentNullException("user", "must have a value");
            if (user.Length == 0)
                throw new ArgumentException("must have a value", "user");
            if (password == null)
                throw new ArgumentNullException("password", "must have a value");
            VerifyOpened();            

            try
            {
                base.SendRequest(new FtpsRequest(base.Encoding, FtpsCmd.User, user));
            }
            catch (FtpsException fex)
            {
                throw new FtpsException("An error occurred when sending user information.", base.LastResponse, fex);
            }

            // wait for user to log into system and all response messages to be transmitted
            Thread.Sleep(500);

            // test to see if this is an asychronous operation and if so make sure 
            // the user has not requested the operation to be canceled
            if (base.AsyncWorker != null && base.AsyncWorker.CancellationPending)
            {
                base.CloseAllConnections();
                return;
            }

            // some ftp servers do not require passwords for users and will log you in immediately - no password command is required
            if (base.LastResponse.Code != FtpsResponseCode.UserLoggedIn)
            {
                try
                {
                    base.SendRequest(new FtpsRequest(base.Encoding, FtpsCmd.Pass, password));
                }
                catch (FtpsException fex)
                {
                    throw new FtpsException("An error occurred when sending password information.", base.LastResponse, fex);
                }

                if (base.LastResponse.Code == FtpsResponseCode.NotLoggedIn)
                    throw new FtpsLoginException("Unable to log into FTP destination with supplied username and password.");
            }
        }

        /// <summary>
        /// Closes connection to the FTP server.
        /// </summary>
        /// <seealso cref="FtpsBase.ConnectionClosed"/>
        /// <seealso cref="Reopen"/>
        /// <seealso cref="Open"/>
        public void Close()
        {
            base.CloseAllConnections();
            _opened = false;
        }

        /// <summary>
        /// Changes the current working directory on older FTP servers that cannot handle a full path containing
        /// multiple subdirectories.  This method will separate the full path into separate change directory commands
        /// to support such systems.
        /// </summary>
        /// <param name="path">Path of the new directory to change to.</param>
        /// <remarks>Accepts both foward slash '/' and back slash '\' path names.</remarks>
        /// <seealso cref="ChangeDirectory"/>
        /// <seealso cref="GetWorkingDirectory"/>
        public void ChangeDirectoryMultiPath(string path)
        {
            // the change working dir command can generally handle all the weird directory name spaces
            // which is nice but frustrating that the ftp server implementors did not fix it for other commands

            if (path == null)
                throw new ArgumentNullException("path");
            if (path.Length == 0)
                throw new ArgumentException("must have a value", "path");
            VerifyOpened();            

            // replace the windows style directory delimiter with a unix style delimiter
            path = path.Replace("\\", "/");

            string[] dirs = path.Split(new char[] {'/'}, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                // issue a single CWD command for each directory
                // this is a very reliable method to change directories on all FTP servers
                // because some systems do not all a full path to be specified when changing directories
                foreach (string dir in dirs)
                {
                    base.SendRequest(new FtpsRequest(base.Encoding, FtpsCmd.Cwd, dir));
                }
            }
            catch (FtpsException fex)
            {
                throw new FtpsException(String.Format("Could not change working directory to '{0}'.", path), fex);
            }

            _currentDirectory = GetWorkingDirectory();
        }

        /// <summary>
        /// Changes the current working directory on the server.  Some FTP server will not accept this command 
        /// if the path contains mutiple directories.  For those FTP server implementations see the method
        /// ChangeDirectoryMultiPath(string).
        /// </summary>
        /// <param name="path">Path of the new directory to change to.</param>
        /// <remarks>Accepts both foward slash '/' and back slash '\' path names.</remarks>
        /// <seealso cref="ChangeDirectoryMultiPath(string)"/>
        /// <seealso cref="GetWorkingDirectory"/>
        public void ChangeDirectory(string path)
        {
            // the change working dir command can generally handle all the weird directory name spaces
            // which is nice but frustrating that the ftp server implementors did not fix it for other commands
            if (path == null)
                throw new ArgumentNullException("path");
            if (path.Length == 0)
                throw new ArgumentException("must have a value", "path"); 
            // replace the windows style directory delimiter with a unix style delimiter
            path = path.Replace("\\", "/");
            VerifyOpened();            

            try
            {
                base.SendRequest(new FtpsRequest(base.Encoding, FtpsCmd.Cwd, path));
            }
            catch (FtpsException fex)
            {
                throw new FtpsException(String.Format("Could not change working directory to '{0}'.", path), fex);
            }

            _currentDirectory = GetWorkingDirectory();
        }
        

        /// <summary>
        /// Gets the current working directory.
        /// </summary>
        /// <returns>A string value containing the current full working directory path on the FTP server.</returns>
        /// <seealso cref="ChangeDirectory"/>
        /// <seealso cref="ChangeDirectoryUp"/>
        public string GetWorkingDirectory()
        {
            VerifyOpened();            
            try
            {
                base.SendRequest(new FtpsRequest(base.Encoding, FtpsCmd.Pwd));
            }
            catch (FtpsException fex)
            {
                throw new FtpsException("Could not retrieve working directory.", base.LastResponse, fex);
            }

            //  now we have to fix the directory due to formatting
            //  most ftp servers send something like this:  257 "/awg/inbound" is current directory.
            string dir = base.LastResponse.Text;

            //  if the pwd is in quotes, then extract it
            if (dir.Substring(0, 1) == "\"")
                dir = dir.Substring(1, dir.IndexOf("\"", 1) - 1);
                return dir;
            }

        /// <summary>
        /// Deletes a file on the remote FTP server.  
        /// </summary>
        /// <param name="path">The path name of the file to delete.</param>
        /// <remarks>
        /// The file is deleted in the current working directory if no path information 
        /// is specified.  Otherwise a full absolute path name can be specified.
        /// Note that some FTP servers will not accept a full path.  On those systems you must navigate to
        /// the directory you wish to delete the file using with the ChangeDirectory() or ChangeDirectoryMultiPath()
        /// method.
        /// </remarks>
        /// <seealso cref="DeleteDirectory"/>
        public void DeleteFile(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            if (path.Length == 0)
                throw new ArgumentException("must have a value", "path");
            VerifyOpened();            

            try
            {
                base.SendRequest(new FtpsRequest(base.Encoding, FtpsCmd.Dele, path));
            }
            catch (FtpsException fex)
            {
                throw new FtpsException(String.Format("Unable to the delete file {0}.", path), base.LastResponse,fex);
            }
        }

        /// <summary>
        /// Aborts an action such as transferring a file to or from the server.  
        /// </summary>
        /// <remarks>
        /// The abort command is sent up to the server signaling the server to abort the current activity.
        /// </remarks>
        public void Abort()
        {
            VerifyOpened();           
            try
            {
                base.SendRequest(new FtpsRequest(base.Encoding, FtpsCmd.Abor));
            }
            catch (FtpsException fex)
            {
                throw new FtpsException("Abort command failed or was unable to be issued.", base.LastResponse, fex);
            }
        }

        /// <summary>
        /// Creates a new directory on the remote FTP server.  
        /// </summary>
        /// <param name="path">The name of a new directory or an absolute path name for a new directory.</param>
        /// <remarks>
        /// If a directory name is given for path then the server will create a new subdirectory 
        /// in the current working directory.  Optionally, a full absolute path may be given.
        /// Note that some FTP servers will not accept a full path.  On those systems you must navigate to
        /// the directory you wish to make the subdirectory using with the ChangeDirectory() or ChangeDirectoryMultiPath()
        /// method.
        /// </remarks>
        public void MakeDirectory(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            if (path.Length == 0)
                throw new ArgumentException("must contain a value", "path");
            VerifyOpened();            

            try
            {
                base.SendRequest(new FtpsRequest(base.Encoding, FtpsCmd.Mkd, path));
            }
            catch (FtpsException fex)
            {
                throw new FtpsException(String.Format("The directory {0} could not be created.", path), base.LastResponse, fex);
            }
        }

        /// <summary>
        /// Moves a file on the remote FTP server from one directory to another.  
        /// </summary>
        /// <param name="fromPath">Path and/or file name to be moved.</param>
        /// <param name="toPath">Destination path specifying the directory where the file will be moved to.</param>
        /// <remarks>
        /// This method actually results in several FTP commands being issued to the server to perform the physical file move.  
        /// This method is available for your convenience when performing common tasks such as moving processed files out of a pickup directory
        /// and into a archive directory.
        /// Note that some older FTP server implementations will not accept a full path to a filename.  On those systems this method may not work
        /// properly.
        /// </remarks>
        public void MoveFile(string fromPath, string toPath)
        {
            if (fromPath == null)
                throw new ArgumentNullException("fromPath");
            if (fromPath.Length == 0)
                throw new ArgumentException("must contain a value", "fromPath");
            if (toPath == null)
                throw new ArgumentNullException("toPath");
            if (fromPath.Length == 0)
                throw new ArgumentException("must contain a value", "toPath");
            VerifyOpened();           	

            //  retrieve the server file from the current working directory
            MemoryStream ms = new MemoryStream();
            GetFile(fromPath, ms, false);

            //  create the remote file in the new location
            ms.Position = 0;    // reset stream position to zero
            this.PutFile(ms, toPath, FileAction.Create);

            //  delete the original file from the original location
            this.DeleteFile(fromPath);
        }
        
        /// <summary>
        /// Deletes a directory from the FTP server.
        /// </summary>
        /// <param name="path">Directory to delete.</param>
        /// <remarks>
        /// The path can be either a specific subdirectory relative to the 
        /// current working directory on the server or an absolute path to 
        /// the directory to remove.
        /// Note that some FTP servers will not accept a full path.  On those systems you must navigate to
        /// the parent directory of the directory you wish to delete using with the ChangeDirectory() or ChangeDirectoryMultiPath()
        /// method.
        /// </remarks>
        /// <seealso cref="DeleteFile"/>
        public void DeleteDirectory(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            if (path.Length == 0)
                throw new ArgumentException("must have a value", "path");
            VerifyOpened();            

            try
            {
                base.SendRequest(new FtpsRequest(base.Encoding, FtpsCmd.Rmd, path));
            }
            catch (FtpsException fex)
            {
                throw new FtpsException(String.Format("The FTP destination was unable to delete the directory '{0}'.", path), base.LastResponse, fex);
            }
        }
        
        /// <summary>
        /// Executes the specific help dialog on the FTP server.  
        /// </summary>
        /// <returns>
        /// A string contains the help dialog from the FTP server.
        /// </returns>
        /// <remarks>
        /// Every FTP server supports a different set of commands and this commands 
        /// can be obtained by the FTP HELP command sent to the FTP server.  The information sent
        /// back is not parsed or processed in any way by the FtpsClient object.  
        /// </remarks>
        public string GetHelp()
	    {
            VerifyOpened();           
            try
            {
                base.SendRequest(new FtpsRequest(base.Encoding, FtpsCmd.Help));
            }
            catch (FtpsException fex)
            {
                throw new FtpsException("An error occurred while getting the system help.", base.LastResponse, fex);
            }

            return base.LastResponse.Text;
        }
        
        /// <summary>
        /// Retrieves the data and time for a specific file on the ftp server as a Coordinated Universal Time (UTC) value (formerly known as GMT). 
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="adjustToLocalTime">Specifies if modified date and time as reported on the FTP server should be adjusted to the local time zone with daylight savings on the client.</param>
        /// <returns>
        /// A date time value.
        /// </returns>
        /// <remarks>
        /// This function uses the MDTM command which is an additional feature command and therefore not supported
        /// by all FTP servers.
        /// </remarks>
        /// <seealso cref="GetFileSize"/>
        public DateTime GetFileDateTime(string fileName, bool adjustToLocalTime)
        {
            if (String.IsNullOrEmpty(fileName))
                throw new ArgumentException("must contain a value", "fileName");
            if (!base.Features.Contains(FtpsCmd.Mdtm))
                throw new FtpsCommandNotSupportedException("Cannot get the file date and time information.", FtpsCmd.Mdtm);
            VerifyOpened();            

            try
            {
                base.SendRequest(new FtpsRequest(base.Encoding, FtpsCmd.Mdtm, fileName));
            }
            catch (FtpsException fex)
            {
                throw new FtpsException(String.Format("An error occurred when retrieving file date and time for '{0}'.", fileName), base.LastResponse, fex);
            }

            string response = base.LastResponse.Text;
            
            int year = int.Parse(response.Substring(0, 4), CultureInfo.InvariantCulture);
            int month = int.Parse(response.Substring(4, 2), CultureInfo.InvariantCulture);
            int day = int.Parse(response.Substring(6, 2), CultureInfo.InvariantCulture);
            int hour = int.Parse(response.Substring(8, 2), CultureInfo.InvariantCulture);
            int minute = int.Parse(response.Substring(10, 2), CultureInfo.InvariantCulture);
            int second = int.Parse(response.Substring(12, 2), CultureInfo.InvariantCulture);

            DateTime dateUtc = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);

            if (adjustToLocalTime)
                return new DateTime(dateUtc.ToLocalTime().Ticks);
            else
                return new DateTime(dateUtc.Ticks);
        }

        /// <summary>
        /// Set the modified date and time value for a specific file.  Not all FTP servers support this command.  In addition
        /// some FTP servers do not support a qualified or relative path value.
        /// </summary>
        /// <param name="path">Filename or fully qualified or partial path.  Note that note all FTP server support a qualified or partial path</param>
        /// <param name="dateTime">New modified date and time value.</param>
        /// <exception cref="FtpsCommandNotSupportedException"></exception>
        public void SetModifiedDateTime(string path, DateTime dateTime)
        {
            VerifyOpened();           
            SetDateTime(path, dateTime, FtpsCmd.Mfmt);
        }

        /// <summary>
        /// Set the created date and time value for a specific file.  Not all FTP servers support this command.  In addition
        /// some FTP servers do not support a qualified or relative path value.
        /// </summary>
        /// <param name="path">Filename or fully qualified or partial path.  Note that note all FTP server support a qualified or partial path</param>
        /// <param name="dateTime">New created date and time value.</param>
        /// <exception cref="FtpsCommandNotSupportedException"></exception>
        public void SetCreatedDateTime(string path, DateTime dateTime)
        {
            VerifyOpened();                        
            SetDateTime(path, dateTime, FtpsCmd.Mfct);
        }        


        /// <summary>
        /// Retrieves the specific status for the FTP server.  
        /// </summary>
        /// <remarks>
        /// Each FTP server may return different status dialog information.  The status information sent
        /// back is not parsed or processed in any way by the FtpsClient object. 
        /// </remarks>
        /// <returns>
        /// A string containing the status of the FTP server.
        /// </returns>
        public string GetStatus()
        {
            VerifyOpened();           
            try
            {
                base.SendRequest(new FtpsRequest(base.Encoding, FtpsCmd.Stat));
            }
            catch (FtpsException fex)
            {
                throw new FtpsException("An error occurred while getting the system status.", base.LastResponse, fex);
            }

            return base.LastResponse.Text;
        }

        /// <summary>
        /// Changes the current working directory on the FTP server to the parent directory.  
        /// </summary>
        /// <remarks>
        /// If there is no parent directory then ChangeDirectoryUp() will not have 
        /// any affect on the current working directory.
        /// </remarks>
        /// <seealso cref="ChangeDirectory"/>
        /// <seealso cref="GetWorkingDirectory"/>
        public void ChangeDirectoryUp()
        {
            VerifyOpened();            			
            try
            {
                base.SendRequest(new FtpsRequest(base.Encoding, FtpsCmd.Cdup));
            }
            catch (FtpsException fex)
            {
                throw new FtpsException("An error occurred when changing directory to the parent (ChangeDirectoryUp).", base.LastResponse, fex);
            }
            _currentDirectory = GetWorkingDirectory();
        }

        /// <summary>
        /// Get the file size for a file on the remote FTP server.  
        /// </summary>
        /// <param name="path">The name and/or path to the file.</param>
        /// <param name="size">Size of the file.</param>
        /// <returns>An integer specifying the file size; otherwise -1</returns>
        /// <remarks>
        /// This method will attempt to acquire the size of the file using server commands in the following order.
        /// (1) Use the SIZE command if it is a supported feature.
        /// (2) Use the MLST command if it is a supported feature and the DirListingMethod is set to Mlsx or Automatic.
        /// (3) Use the LIST al command if DirListingMethod is set to ListAl.
        /// (4) Use the LIST command with no optional parameters.
        /// 
        /// The path can be file name relative to the current working directory or an absolute path.  This command is an additional feature 
        /// that is not supported by all FTP servers.
        /// 
        /// Note that some FTP servers will not accept a full path.  On those systems you must navigate to
        /// the directory you wish to get the file size using with the ChangeDirectory() or ChangeDirectoryMultiPath()
        /// method.
        /// </remarks>
        /// <seealso cref="GetFileSize(string)"/>
        /// <seealso cref="GetFileDateTime"/>
        public bool TryGetFileSize(string path, out long size)
        {
            size = -1;
            try
            {
                size = GetFileSize(path);
            }
            catch
            {
                return false;
            }
            return true;
        }
        
        /// <summary>
        /// Get the file size for a file on the remote FTP server.  
        /// </summary>
        /// <param name="path">The name and/or path to the file.</param>
        /// <returns>An integer specifying the file size.</returns>
        /// <remarks>
        /// This method will attempt to acquire the size of the file using server commands in the following order.
        /// (1) Use the SIZE command if it is a supported feature.
        /// (2) Use the MLST command if it is a supported feature and the DirListingMethod is set to Mlsx or Automatic.
        /// (3) Use the LIST al command if DirListingMethod is set to ListAl.
        /// (4) Use the LIST command with no optional parameters.
        /// 
        /// The path can be file name relative to the current working directory or an absolute path.  This command is an additional feature 
        /// that is not supported by all FTP servers.
        /// 
        /// Note that some FTP servers will not accept a full path.  On those systems you must navigate to
        /// the directory you wish to get the file size using with the ChangeDirectory() or ChangeDirectoryMultiPath()
        /// method.
        /// </remarks>
        /// <seealso cref="TryGetFileSize(string, out long)"/>
        /// <seealso cref="GetFileDateTime"/>
        public long GetFileSize(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            if (path.Length == 0)
                throw new ArgumentException("must contain a value", "path");
            VerifyOpened();            

            long size = 0;

            // test to see if SIZE is a supported feature
            if (base.Features.Contains(FtpsCmd.Size))
            {
                try
                {
                    // if the SIZE command is supported always use it 
                    base.SendRequest(new FtpsRequest(base.Encoding, FtpsCmd.Size, path));
                }
                catch (FtpsException fex)
                {
                    throw new FtpsException(String.Format("An error occurred while attempting to get size of file '{0}' using the SIZE command.", path), base.LastResponse, fex);
                }
                size = Int64.Parse(base.LastResponse.Text, CultureInfo.InvariantCulture);
            }
            else  // SIZE command not supported
            {
                FtpsItem item = null;
                try
                {
                    // attempt to get the file size by executing the method GetFileInfo()
                    // which will go through a decision tree to determine which method
                    // is best to get the file size data
                    item = GetFileInfo(path);
                }
                catch (FtpsException fex)
                {
                    throw new FtpsException(String.Format("An error occurred while attempting to get size of file {0} using the GetFileInfo() method.", path), base.LastResponse, fex);
                }
                size = item.Size;
            }

            return size;
        }

        /// <summary>
        /// Retrieves the specific status for a file on the FTP server.  
        /// </summary>
        /// <param name="path">
        /// The path to the file.
        /// </param>
        /// <returns>
        /// A string containing the status for the file.
        /// </returns>
        /// <remarks>
        /// Each FTP server may return different status dialog information.  The status information sent
        /// back is not parsed or processed in any way by the FtpsClient object. 
        /// Note that some FTP servers will not accept a full path.  On those systems you must navigate to
        /// the directory you wish to get the status of the file using with the ChangeDirectory() or ChangeDirectoryMultiPath()
        /// method.
        /// </remarks>
        public string GetStatus(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            if (path.Length == 0)
                throw new ArgumentException("must contain a value", "path");
            VerifyOpened();            

            try
            {
                base.SendRequest(new FtpsRequest(base.Encoding, FtpsCmd.Stat, path));
            }
            catch (FtpsException fex)
            {
                throw new FtpsException(String.Format("An error occurred when retrieving file status for file '{0}'.", path), base.LastResponse, fex);
            }

            return base.LastResponse.Text;
        }
        
        /// <summary>
        /// Allocates storage for a file on the FTP server prior to data transfer from the FTP client.  
        /// </summary>
        /// <param name="size">
        /// The storage size to allocate on the FTP server.
        /// </param>
        /// <remarks>
        /// Some FTP servers may return the client to specify the storage size prior to data transfer from the FTP client to the FTP server.
        /// </remarks>
        public void AllocateStorage(long size)
        {
            VerifyOpened();            			
            try
            {
                base.SendRequest(new FtpsRequest(base.Encoding, FtpsCmd.Allo, size.ToString()));
            }
            catch (FtpsException fex)
            {
                throw new FtpsException("An error occurred when trying to allocate storage on the destination.", base.LastResponse, fex);
            }
        }

        /// <summary>
        /// Retrieves a string identifying the remote FTP system.  
        /// </summary>
        /// <returns>
        /// A string contains the server type.
        /// </returns>
        /// <remarks>
        /// The string contains the word "Type:", and the default transfer type 
        /// For example a UNIX FTP server will return 'UNIX Type: L8'.  A Windows 
        /// FTP server will return 'WINDOWS_NT'.
        /// </remarks>
        public string GetSystemType()
        {
            VerifyOpened();            			
            try
            {
                base.SendRequest(new FtpsRequest(base.Encoding, FtpsCmd.Syst));
            }
            catch (FtpsException fex)
            {
                throw new FtpsException("An error occurred while getting the system type.", base.LastResponse, fex);
            }

            return base.LastResponse.Text;
        }

        /// <summary>
        /// Uploads a local file specified in the path parameter to the remote FTP server.  
        /// </summary>
        /// <param name="localPath">Path to a file on the local machine.</param>
        /// <remarks>
        /// The file is uploaded to the current working directory on the remote server.  
        /// A unique file name is created by the server.    
        /// </remarks>
        /// <returns>
        /// Name of the unique file created on the FTP server is a 36 character hex GUID value which may not
        /// be valid for use on all FTP servers.
        /// </returns>
        /// <seealso cref="PutFile(string, string, FileAction)"/>
        /// <seealso cref="PutFileAsync(string, string, FileAction)"/>
        /// <seealso cref="GetFile(string, string)"/>
        /// <seealso cref="GetFileAsync(string, string, FileAction)"/>
        /// <seealso cref="MoveFile"/>
        /// <seealso cref="FxpCopy"/>
        /// <seealso cref="FxpCopyAsync"/>  
        public string PutFileUnique(string localPath)
        {
            if (localPath == null)
                throw new ArgumentNullException("localPath");
            VerifyOpened();            			

            string fname = "";
            try
            {
                using (FileStream fileStream = File.OpenRead(localPath))
                {
                    fname = PutFileUnique(fileStream);
                }
            }
            catch (FtpsException fex)
            {
                WriteToLog(String.Format("Action='PutFileUnique';Action='TransferError';LocalPath='{0}';CurrentDirectory='{1}';ErrorMessage='{2}'", localPath, _currentDirectory, fex.Message));
                throw new FtpsException("An error occurred while executing PutFileUnique() on the remote FTP destination.", base.LastResponse, fex);
            }

            return fname;
        }

        /// <summary>
        /// Uploads any stream object to the remote FTP server and stores the data under a unique file name assigned by the FTP server
        /// into the current working directory.
        /// </summary>
        /// <param name="inputStream">Any stream object on the local client machine.</param>
        /// <remarks>
        /// The stream is uploaded to the current working directory on the remote server.  
        /// A unique file name is created by the server to store the data uploaded from the stream.
        /// </remarks>
        /// <returns>
        /// Name of the unique file created on the FTP server is a 36 character hex GUID value which may not
        /// be valid for use on all FTP servers.
        /// </returns>
        /// <seealso cref="PutFile(string, string, FileAction)"/>
        /// <seealso cref="PutFileAsync(string, string, FileAction)"/>
        /// <seealso cref="GetFile(string, string)"/>
        /// <seealso cref="GetFileAsync(string, string, FileAction)"/>
        /// <seealso cref="MoveFile"/>
        /// <seealso cref="FxpCopy"/>
        /// <seealso cref="FxpCopyAsync"/>  
        public string PutFileUnique(Stream inputStream)
        {
            if (inputStream == null)
                throw new ArgumentNullException("inputStream");
            if (!inputStream.CanRead)
                throw new ArgumentException("must be readable.  The CanRead property must return a value of 'true'.", "inputStream");
            VerifyOpened();            

            string fname = Guid.NewGuid().ToString();
            WriteToLog(String.Format("Action='PutFileUnique';Action='TransferBegin';CurrentDirectory='{0}';FileName='{1}", _currentDirectory, fname));

            try
            {
                base.TransferData(TransferDirection.ToServer, new FtpsRequest(base.Encoding, FtpsCmd.Stor, fname), inputStream);
            }
            catch (Exception ex)
            {
                WriteToLog(String.Format("Action='PutFileUnique';Action='TransferError';CurrentDirectory='{0}';FileName='{1};ErrorMessage='{1}'", _currentDirectory, fname, ex.Message));
                throw new FtpsException("An error occurred while executing PutFileUnique() on the remote FTP destination.", base.LastResponse, ex);
            }

            WriteToLog(String.Format("Action='PutFileUnique';Action='TransferSuccess';CurrentDirectory='{0}';FileName='{1}'", _currentDirectory, fname));

            return fname;
        }

        /// <summary>
        /// Retrieves a remote file from the FTP server and writes the data to a local file
        /// specfied in the localPath parameter.  If the local file already exists a System.IO.IOException is thrown.
        /// </summary>
        /// <remarks>
        /// To retrieve a remote file that you need to overwrite an existing file with or append to an existing file
        /// see the method GetFile(string, string, FileAction).
        /// </remarks>
        /// <param name="remotePath">A path of the remote file.</param>
        /// <param name="localPath">A fully qualified local path to a file on the local machine.</param>
        /// <seealso cref="GetFileAsync(string, string, FileAction)"/>
        /// <seealso cref="PutFile(string, string, FileAction)"/>
        /// <seealso cref="PutFileAsync(string, string, FileAction)"/>
        /// <seealso cref="MoveFile"/>
        /// <seealso cref="FxpCopy"/>
        /// <seealso cref="FxpCopyAsync"/>
        public void GetFile(string remotePath, string localPath)
        {
            GetFile(remotePath, localPath, FileAction.CreateNew);
        }

        /// <summary>
        /// Retrieves a remote file from the FTP server and writes the data to a local file
        /// specfied in the localPath parameter.
        /// </summary>
        /// <remarks>
        /// Note that some FTP servers will not accept a full path.  On those systems you must navigate to
        /// the directory you wish to get the file using with the ChangeDirectory() or ChangeDirectoryMultiPath()
        /// method.
        /// </remarks>
        /// <param name="remotePath">A path and/or file name to the remote file.</param>
        /// <param name="localPath">A fully qualified local path to a file on the local machine.</param>
        /// <param name="action">The type of action to take.</param>
        /// <seealso cref="GetFileAsync(string, string, FileAction)"/>
        /// <seealso cref="PutFile(string, string, FileAction)"/>
        /// <seealso cref="PutFileAsync(string, string, FileAction)"/>
        /// <seealso cref="MoveFile"/>
        /// <seealso cref="FxpCopy"/>
        /// <seealso cref="FxpCopyAsync"/>
        public void GetFile(string remotePath, string localPath, FileAction action)
        {
            if (remotePath == null)
                throw new ArgumentNullException("remotePath");
            if (remotePath.Length == 0)
                throw new ArgumentException("must contain a value", "remotePath");
            if (localPath == null)
                throw new ArgumentNullException("localPath");
            if (localPath.Length == 0)
                throw new ArgumentException("must contain a value", "localPath");
            if (action == FileAction.None)
                throw new ArgumentOutOfRangeException("action", "must contain a value other than 'Unknown'");
            VerifyOpened();            

            // if the transfer event is being subscribed or this is a resume action then get the file size
            long remoteSize = -1;

            // set the transfer size so we can do some calc on percentage completed
            // in the TransferBytes() method of the base class
            if (IsTransferProgressEventSet() || action == FileAction.Resume) {
                // try to get the remote file size 
                TryGetFileSize(remotePath, out remoteSize);
                
                if (IsTransferProgressEventSet()) {
                    SetTransferSize (remoteSize);
                }
            }

            localPath = CorrectLocalPath(localPath);

            WriteToLog(String.Format("Action='GetFile';Status='TransferBegin';LocalPath='{0}';RemotePath='{1}';FileAction='{2}'", localPath, remotePath, action.ToString()));

            FtpsRequest request = new FtpsRequest(base.Encoding, FtpsCmd.Retr, remotePath);

            try
            {
                switch (action)
                {
                    case FileAction.CreateNew:
                        // create a file stream to stream the file locally to disk that only creates the file if it does not already exist
                        using (Stream localFile = File.Open(localPath, FileMode.CreateNew))
                        {
                            TransferData(TransferDirection.ToClient, request, localFile);
                        }
                        break;
                    
                    case FileAction.Create:
                        // create a file stream to stream the file locally to disk
                        using (Stream localFile = File.Open(localPath, FileMode.Create))
                        {
                            TransferData(TransferDirection.ToClient, request, localFile);
                        }
                        break;
                    case FileAction.CreateOrAppend:
                        // open the local file
                        using (Stream localFile = File.Open(localPath, FileMode.OpenOrCreate))
                        {
                            // set the file position to the end so that any new data will be appended                        
                            localFile.Position = localFile.Length;
                            TransferData(TransferDirection.ToClient, request, localFile);
                        }
                        break;
                    case FileAction.Resume:
                        if (!base.Features.Contains(FtpsCmd.Rest, "STREAM"))
                            throw new FtpsCommandNotSupportedException("Cannot resume file transfer." ,"REST STREAM");

                        using (Stream localFile = File.Open(localPath, FileMode.Open))
                        {
                            //  make sure we have a valid file size
                            if (remoteSize == -1)
                                throw new FtpsException("unable to determine file size for resume transfer");

                            // if the files are the same size then there is nothing to transfer
                            if (localFile.Length == remoteSize)
                                return;

                            // attempt to adjust the transfer size
                            if (IsTransferProgressEventSet()) {
                                if (localFile.Length > remoteSize) {
                                    SetTransferSize(localFile.Length - remoteSize);
                                }
                            }

                            TransferData(TransferDirection.ToClient, request, localFile, localFile.Length - 1);
                        }
                        break;
                    case FileAction.ResumeOrCreate:
                        if (File.Exists(localPath) && (new FileInfo(localPath)).Length > 0)
                            GetFile(remotePath, localPath, FileAction.Resume);
                        else
                            GetFile(remotePath, localPath, FileAction.Create);
                        break;
                }
            }
            catch (Exception ex)
            {
                WriteToLog(String.Format("Action='GetFile';Status='TransferError';LocalPath='{0}';RemotePath='{1}';FileAction='{1}';ErrorMessage='{2}", localPath, remotePath, action.ToString(), ex.Message));
                throw new FtpsException(String.Format("An unexpected exception occurred while retrieving file '{0}'.", remotePath), base.LastResponse, ex); 
            }

            WriteToLog(String.Format("Action='GetFile';Status='TransferSuccess';LocalPath='{0}';RemotePath='{1}';FileAction='{1}'", localPath, remotePath, action.ToString()));
        }


        /// <summary>
        /// Retrieves a remote file from the FTP server and writes the data to a local stream object
        /// specfied in the outStream parameter.
        /// </summary> 
        /// <param name="remotePath">A path and/or file name to the remote file.</param>
        /// <param name="outStream">An output stream object used to stream the remote file to the local machine.</param>
        /// <remarks>
        /// If the remote path is a file name then the file is downloaded from the FTP server current working directory.  Otherwise a fully qualified
        /// path for the remote file may be specified.  The output stream must be writeable and can be any stream object.  Finally, the restart parameter
        /// is used to send a restart command to the FTP server.  The FTP server is instructed to restart the download process at the last position of
        /// of the output stream.  Not all FTP servers support the restart command.  If the FTP server does not support the restart (REST) command,
        /// an FtpException error is thrown.
        /// Note that some FTP servers will not accept a full path.  On those systems you must navigate to
        /// the directory you wish to get the file using with the ChangeDirectory() or ChangeDirectoryMultiPath()
        /// method.
        /// </remarks>
        /// <seealso cref="GetFileAsync(string, string, FileAction)"/>
        /// <seealso cref="PutFile(string, string, FileAction)"/>
        /// <seealso cref="PutFileAsync(string, string, FileAction)"/>
        /// <seealso cref="MoveFile"/>
        /// <seealso cref="FxpCopy"/>
        /// <seealso cref="FxpCopyAsync"/>
        public void GetFile(string remotePath, Stream outStream)
        {
            GetFile(remotePath, outStream, false);
        }

        /// <summary>
        /// Retrieves a remote file from the FTP server and writes the data to a local stream object
        /// specfied in the outStream parameter.
        /// </summary> 
        /// <param name="remotePath">A path and/or file name to the remote file.</param>
        /// <param name="outStream">An output stream object used to stream the remote file to the local machine.</param>
        /// <param name="resume">A true/false value to indicate if the file download needs to be restarted due to a previous partial download.</param>
        /// <remarks>
        /// If the remote path is a file name then the file is downloaded from the FTP server current working directory.  Otherwise a fully qualified
        /// path for the remote file may be specified.  The output stream must be writeable and can be any stream object.  Finally, the restart parameter
        /// is used to send a restart command to the FTP server.  The FTP server is instructed to restart the download process at the last position of
        /// of the output stream.  Not all FTP servers support the restart command.  If the FTP server does not support the restart (REST) command,
        /// an FtpException error is thrown.
        /// Note that some FTP servers will not accept a full path.  On those systems you must navigate to
        /// the directory you wish to get the file using with the ChangeDirectory() or ChangeDirectoryMultiPath()
        /// method.
        /// </remarks>
        /// <seealso cref="GetFileAsync(string, string, FileAction)"/>
        /// <seealso cref="PutFile(string, string, FileAction)"/>
        /// <seealso cref="PutFileAsync(string, string, FileAction)"/>
        /// <seealso cref="MoveFile"/>
        /// <seealso cref="FxpCopy"/>
        /// <seealso cref="FxpCopyAsync"/>
        public void GetFile(string remotePath, Stream outStream, bool resume)
        {
            if (remotePath == null)
                throw new ArgumentNullException("remotePath");
            if (remotePath.Length == 0)
                throw new ArgumentException("must contain a value", "remotePath");
            if (outStream == null)
                throw new ArgumentNullException("outStream");
            if (!outStream.CanWrite)
                throw new ArgumentException("outStream not writable");
            VerifyOpened();

            // if the transfer event is being subscribed or this is a resume action then get the file size
            long remoteSize = -1;

            // set the transfer size so we can do some calc on percentage completed
            // in the TransferBytes() method of the base class
            if (IsTransferProgressEventSet() || resume)
            {
                // try to get the remote file size 
                TryGetFileSize(remotePath, out remoteSize);

                if (IsTransferProgressEventSet())
                {
                    SetTransferSize(remoteSize);
                }
            }

            WriteToLog(String.Format("Action='GetFile';Status='TransferBegin';LocalPath='{0}';RemotePath='{1}';FileAction='{2}'", "none", remotePath, "GetFileToStream"));

            FtpsRequest request = new FtpsRequest(base.Encoding, FtpsCmd.Retr, remotePath);

            try
            {

                if (resume)
                {
                    if (!base.Features.Contains(FtpsCmd.Rest, "STREAM"))
                        throw new FtpsCommandNotSupportedException("Cannot resume file transfer.", "REST STREAM");

                    //  make sure we have a valid file size
                    if (remoteSize == -1)
                        throw new FtpsException("unable to determine file size for resume transfer");

                    // if the files are the same size then there is nothing to transfer
                    if (outStream.Length == remoteSize)
                        return;

                    // attempt to adjust the transfer size
                    if (IsTransferProgressEventSet())
                    {
                        if (outStream.Length > remoteSize)
                        {
                            SetTransferSize(outStream.Length - remoteSize);
                        }
                    }

                    TransferData(TransferDirection.ToClient, request, outStream, outStream.Length - 1);

                }
                else
                {
                    TransferData(TransferDirection.ToClient, request, outStream);
                }
            }
            catch (Exception ex)
            {
                WriteToLog(String.Format("Action='GetFile';Status='TransferError';LocalPath='{0}';RemotePath='{1}';FileAction='{1}';ErrorMessage='{2}", "none", remotePath, "GetFileToStream", ex.Message));
                throw new FtpsException(String.Format("An unexpected exception occurred while retrieving file '{0}'.", remotePath), base.LastResponse, ex);
            }

            WriteToLog(String.Format("Action='GetFile';Status='TransferSuccess';LocalPath='{0}';RemotePath='{1}';FileAction='{1}'", "none", remotePath, "GetFileToStream"));
        }

        /// <summary>
        /// Tests to see if a path exists on the remote server.  The current working directory must be the
        /// parent or root directory of the file or directory whose existence is being tested.  For best results, 
        /// call this method from the root working directory ("/").
        /// </summary>
        /// <param name="path">The full path to the remote directory relative to the current working directory you want to test for existence of.</param>
        /// <returns>Boolean value indicating if directory exists or not.</returns>
        /// <remarks>This method will execute a change working directory (CWD) command prior to testing to see if the  
        /// file exists.  The original working directory will be changed back to the original value
        /// after this method has completed.  This method may not work on systems where the directory listing is not being
        /// parsed correctly.  If the method call GetDirList() does not work properly with your FTP server, this method may not
        /// produce reliable results.  This method will also not produce reliable results if the directory or file is hidden on the
        /// remote FTP server.</remarks>
        /// <seealso cref="GetDirList()"/>
        /// <seealso cref="Exists(string, string)"/>
        public bool Exists(string path)
        {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentException("must have a value", "path");
            VerifyOpened();            

            // replace the windows style directory delimiter with a unix style delimiter
            path = path.Replace("\\", "/");
            string fname = Path.GetFileName(path);

            string[] dirs = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (dirs.Length == 1)
            {
                return Exists("/", fname);
            }
            else
            {
                return Exists(path.Substring(0, path.Length - (fname.Length)), fname);
            }
        }

        /// <summary>
        /// Tests to see if a file exists on the remote server.  The current working directory must be the
        /// parent or root directory of the file or directory whose existence is being tested.  For best results, 
        /// call this method from the root working directory ("/").
        /// </summary>
        /// <param name="path">The full path to the remote directory relative to the current working directory.</param>
        /// <param name="filename">The name of the file (or directory) test for existence of.</param>
        /// <returns>Boolean value indicating if file exists or not.</returns>
        /// <remarks>This method will execute a change working directory (CWD) command prior to testing to see if the  
        /// file exists.  The original working directory will be changed back to the original value
        /// after this method has completed.  This method may not work on systems where the directory listing is not being
        /// parsed correctly.  If the method call GetDirList() does not work properly with your FTP server, this method may not
        /// produce reliable results.  This method will also not produce reliable results if the directory or file is hidden on the
        /// remote FTP server.</remarks>
        /// <seealso cref="GetDirList()"/>
        /// <seealso cref="Exists(string)"/>
        public bool Exists(string path, string filename)
        {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentException("must have a value", "path");
            if (String.IsNullOrEmpty(filename))
                throw new ArgumentException("must have a value", "filename");
            VerifyOpened();            

            // replace the windows style directory delimiter with a unix style delimiter
            path = path.Replace("\\", "/");

            // variables for the algorithm
            int dirCount = 0;        // keep up with the number of valid CHDIR commands issued 
            bool found = false;      // found flag

            string[] dirs = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                // if the path contains more than just the filename then 
                // we must change the directory to where the file is located
                if (path != "/")
                {
                    foreach(string dir in dirs)
                    {
                        ChangeDirectory(dir);
                        dirCount++;
                    }
                }

                found = GetDirList().Contains(filename);
            }
            catch (FtpsException)
            {  
                // surpress ftp CHDIR exceptions
            }
            finally
            {
                // always change directory back up to the original directory based on the number
                // of subdirectories was have gone down into
                // this is a very reliable method to change directories on all FTP servers
                for (int j = 0; j < dirCount; j++)
                    this.ChangeDirectoryUp();
            }

            return found;
        }


        /// <summary>
        /// Retrieves a file name listing of the current working directory from the 
        /// remote FTP server using the NLST command.
        /// </summary>
        /// <returns>A string containing the file listing from the current working directory.</returns>
        /// <seealso cref="GetDirList(string)"/>
        /// <seealso cref="GetDirListAsText()"/>
        /// <seealso cref="GetDirListAsText(string)"/>
        /// <seealso cref="GetDirListAsync()"/>
        /// <seealso cref="GetDirListAsync(string)"/>
        /// <seealso cref="GetDirListDeep"/>
        /// <seealso cref="GetDirListDeepAsync(string)"/>
        public string GetNameList()
        {
            VerifyOpened();                    
            return base.TransferText(new FtpsRequest(base.Encoding, FtpsCmd.Nlst));
        }

        /// <summary>
        /// Retrieves a file name listing of the current working directory from the 
        /// remote FTP server using the NLST command.
        /// </summary>
        /// <param name="path">The path to a directory on the remote FTP server.</param>
        /// <returns>A string containing the file listing from the current working directory.</returns>
        /// <remarks>
        /// Note that some FTP servers will not accept a full path.  On those systems you must navigate to
        /// the parent directory you wish to get the name list using with the ChangeDirectory() or ChangeDirectoryMultiPath()
        /// method.
        /// </remarks>
        /// <seealso cref="GetDirList(string)"/>
        /// <seealso cref="GetDirListAsText()"/>
        /// <seealso cref="GetDirListAsText(string)"/>
        /// <seealso cref="GetDirListAsync()"/>
        /// <seealso cref="GetDirListAsync(string)"/>
        /// <seealso cref="GetDirListDeep(string)"/>
        /// <seealso cref="GetDirListDeepAsync()"/>
        /// <seealso cref="GetDirListDeepAsync(string)"/>
        public string GetNameList(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            VerifyOpened();            

            return base.TransferText(new FtpsRequest(base.Encoding, FtpsCmd.Nlst, path));
        }

        /// <summary>
        /// Retrieves file information for one specific file system object.
        /// </summary>
        /// <param name="path">The path to the file system object on the remote FTP server.</param>
        /// <returns>A FtpItem object containing the file system object information.</returns>
        /// <remarks>
        /// If the FTP features list MLST then the MLST command will be executed and a FtpMlsxItem cast 
        /// as a FtpItem will be returned.  Otherwise, the standard LIST command will be executed and a
        /// standard FtpItem object will be returned.
        /// 
        /// Note that some FTP servers will not accept a full path to the file system object.  On those systems you must navigate to
        /// the parent directory which contains the file system object using ChangeDirectory() or ChangeDirectoryMultiPath()
        /// method and invoke the method without any additional path information.
        /// </remarks>
        /// <exception cref="FtpsFeatureException"></exception>
        /// <seealso cref="GetDirList(string)"/>
        /// <seealso cref="GetDirListAsText()"/>
        /// <seealso cref="GetDirListAsText(string)"/>
        /// <seealso cref="GetDirListAsync()"/>
        /// <seealso cref="GetDirListAsync(string)"/>
        /// <seealso cref="GetDirListDeep(string)"/>
        /// <seealso cref="GetDirListDeepAsync()"/>
        /// <seealso cref="GetDirListDeepAsync(string)"/>
        public FtpsItem GetFileInfo(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            VerifyOpened();            

            FtpsRequest r = GetFileInfoRequest(path);

            if (r.Command == FtpsCmd.List)
            {
                string text = base.TransferText(r);
                // if there is not text data then return null
                if (String.IsNullOrEmpty(text))
                    return null;
                string[] lines = text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                // if there are are no lines with data then return null
                if (lines.Length == 0)
                    return null;
                // only parse the first line encountered in the event
                // the caller did not limit the path query correctly
                return _listItemParser.ParseLine(lines[0]);
            }
            else // FtpCmd.Mlst
            {
                if (!base.Features.Contains(FtpsCmd.Mlst))
                    throw new FtpsFeatureException("MLST command not a listed supported feature");
                base.SendRequest(new FtpsRequest(base.Encoding, FtpsCmd.Mlst, path));
                FtpsResponseCollection col = base.LastResponseList;
                if (col.Count != 3)
                    throw new FtpsException("incorrect number of response lines received by MLST command text");
                // parse the second line and return a mlsx object
                FtpsItem mlsx = _mlsxItemParser.ParseLine(col[1].RawText); ;
                return mlsx;
            }
        }

        private FtpsRequest GetFileInfoRequest(string path)
        {
            switch (_dirListingMethod)
            {
                case ListingMethod.List:
                    return new FtpsRequest(base.Encoding, FtpsCmd.List, path);
                case ListingMethod.ListAl:
                    return new FtpsRequest(base.Encoding, FtpsCmd.List, "-al", path);
                case ListingMethod.Automatic:
                    if (!base.Features.Contains(FtpsCmd.Mlst))
                        return new FtpsRequest(base.Encoding, FtpsCmd.List, "-al", path);
                    else
                        return new FtpsRequest(base.Encoding, FtpsCmd.Mlst, path);
                case ListingMethod.Mlsx:
                    return new FtpsRequest(base.Encoding, FtpsCmd.Mlst, path);
                default:
                    throw new FtpsException("unknown directory listing method");
            }
        }

        /// <summary>
        /// Retrieves a directory listing of the current working directory from the 
        /// remote FTP server using the LIST command.
        /// </summary>
        /// <returns>A string containing the directory listing of files from the current working directory.</returns>
        /// <seealso cref="GetDirList()"/>
        /// <seealso cref="GetDirList(string)"/>
        /// <seealso cref="GetDirListAsText(string)"/>
        /// <seealso cref="GetDirListAsync()"/>
        /// <seealso cref="GetDirListAsync(string)"/>
        /// <seealso cref="GetDirListDeep(string)"/>
        /// <seealso cref="GetDirListDeepAsync()"/>
        /// <seealso cref="GetDirListDeepAsync(string)"/>
        /// <seealso cref="GetDirListDeepAsync(string)"/>
        /// <seealso cref="GetNameList()"/>
        /// <seealso cref="GetNameList(string)"/>
        public string GetDirListAsText()
        {
            VerifyOpened();            
            return base.TransferText(CreateDirListingRequest());
        }

        /// <summary>
        /// Retrieves a directory listing of the current working directory from the 
        /// remote FTP server using the LIST command.
        /// </summary>
        /// <param name="path">The path to a directory on the remote FTP server.</param>
        /// <returns>A string containing the directory listing of files from the current working directory.</returns>
        /// <remarks>
        /// Note that some FTP servers will not accept a full path.  On those systems you must navigate to
        /// the parent directory you wish to get the name list using with the ChangeDirectory() or ChangeDirectoryMultiPath()
        /// method.
        /// </remarks>
        /// <seealso cref="GetDirList()"/>
        /// <seealso cref="GetDirList(string)"/>
        /// <seealso cref="GetDirListAsText()"/>
        /// <seealso cref="GetDirListAsync()"/>
        /// <seealso cref="GetDirListAsync(string)"/>
        /// <seealso cref="GetDirListDeep(string)"/>
        /// <seealso cref="GetDirListDeepAsync()"/>
        /// <seealso cref="GetDirListDeepAsync(string)"/>
        /// <seealso cref="GetDirListDeepAsync(string)"/>
        /// <seealso cref="GetNameList()"/>
        /// <seealso cref="GetNameList(string)"/>
        public string GetDirListAsText(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            VerifyOpened();            			

            return base.TransferText(CreateDirListingRequest());
        }

        /// <summary>
        /// Retrieves a list of the files from current working directory on the remote FTP 
        /// server using the LIST command.  
        /// </summary>
        /// <returns>FtpItemList collection object.</returns>
        /// <remarks>
        /// This method returns a FtpItemList collection of FtpItem objects.
        /// </remarks>
        /// <seealso cref="GetDirList(string)"/>
        /// <seealso cref="GetDirListAsText()"/>
        /// <seealso cref="GetDirListAsText(string)"/>
        /// <seealso cref="GetDirListAsync()"/>
        /// <seealso cref="GetDirListAsync(string)"/>
        /// <seealso cref="GetDirListDeep(string)"/>
        /// <seealso cref="GetDirListDeepAsync()"/>
        /// <seealso cref="GetDirListDeepAsync(string)"/>
        /// <seealso cref="GetNameList()"/>
        /// <seealso cref="GetNameList(string)"/>
        public FtpsItemCollection GetDirList()
        {
            VerifyOpened();            			
            return new FtpsItemCollection(_currentDirectory, base.TransferText(CreateDirListingRequest()), GetItemParser());
        }

        /// <summary>
        /// Retrieves a list of the files from a specified path on the remote FTP 
        /// server using the LIST command. 
        /// </summary>
        /// <param name="path">The path to a directory on the remote FTP server.</param>
        /// <returns>FtpFileCollection collection object.</returns>
        /// <remarks>
        /// This method returns a FtpFileCollection object containing a collection of 
        /// FtpItem objects.  Some FTP server implementations will not accept a full path to a resource.  On those
        /// systems it is best to change the working directory using the ChangeDirectoryMultiPath(string) method and then call
        /// the method GetDirList().
        /// </remarks>
        /// <seealso cref="GetDirList()"/>
        /// <seealso cref="GetDirListAsText()"/>
        /// <seealso cref="GetDirListAsText(string)"/>
        /// <seealso cref="GetDirListAsync()"/>
        /// <seealso cref="GetDirListAsync(string)"/>
        /// <seealso cref="GetDirListDeep(string)"/>
        /// <seealso cref="GetDirListDeepAsync()"/>
        /// <seealso cref="GetDirListDeepAsync(string)"/>
        /// <seealso cref="GetNameList()"/>
        /// <seealso cref="GetNameList(string)"/>
        public FtpsItemCollection GetDirList(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            VerifyOpened();            

            return new FtpsItemCollection(path, base.TransferText(CreateDirListingRequest(path)), GetItemParser());
        }

        /// <summary>
        /// Deeply retrieves a list of all files and all sub directories from a specified path on the remote FTP 
        /// server using the LIST command. 
        /// </summary>
        /// <param name="path">The path to a directory on the remote FTP server.</param>
        /// <returns>FtpFileCollection collection object.</returns>
        /// <remarks>
        /// This method returns a FtpFileCollection object containing a collection of 
        /// FtpItem objects.
        /// Note that some FTP servers will not accept a full path.  On those systems you must navigate to
        /// the parent directory you wish to get the directory list using with the ChangeDirectory() or ChangeDirectoryMultiPath()
        /// method.
        /// </remarks>
        /// <seealso cref="GetDirList()"/>
        /// <seealso cref="GetDirList(string)"/>
        /// <seealso cref="GetDirListAsText()"/>
        /// <seealso cref="GetDirListAsText(string)"/>
        /// <seealso cref="GetDirListAsync()"/>
        /// <seealso cref="GetDirListAsync(string)"/>
        /// <seealso cref="GetDirListDeepAsync(string)"/>
        /// <seealso cref="GetNameList()"/>
        /// <seealso cref="GetNameList(string)"/>
        public FtpsItemCollection GetDirListDeep(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            VerifyOpened();            			
            
            FtpsItemCollection deepCol = new FtpsItemCollection();
            ParseDirListDeep(path, deepCol);
            return deepCol;
        }

        /// <summary>
        /// Renames a file or directory on the remote FTP server.
        /// </summary>
        /// <param name="name">The name or absolute path of the file or directory you want to rename.</param>
        /// <param name="newName">The new name or absolute path of the file or directory.</param>
        /// <seealso cref="SetDateTime"/>
        public void Rename(string name, string newName)
        {
            if (name == null)
                throw new ArgumentNullException("name", "must have a value");
            if (name.Length == 0)
                throw new ArgumentException("must have a value", "name");
            if (newName == null)
                throw new ArgumentNullException("newName", "must have a value");
            if (newName.Length == 0)
                throw new ArgumentException("must have a value", "newName");
            VerifyOpened();            

            try
            {
                base.SendRequest(new FtpsRequest(base.Encoding, FtpsCmd.Rnfr, name));
                base.SendRequest(new FtpsRequest(base.Encoding, FtpsCmd.Rnto, newName));
            }
            catch (FtpsException fex)
            {
                throw new FtpsException(String.Format("The FTP destination was unable to rename the file or directory '{0}' to the new name '{1}'.", name, newName), base.LastResponse, fex);
            }

        }

        /// <summary>
        /// Send a raw FTP command to the server.  Same method as Quote().
        /// </summary>
        /// <param name="command">A string containing a valid FTP command value such as SYST.</param>
        /// <returns>The raw textual response from the server.</returns>
        /// <remarks>
        /// This is an advanced feature of the FtpsClient class that allows for any custom or specialized
        /// FTP command to be sent to the FTP server.  Some FTP server support custom commands outside of
        /// the standard FTP command list.  The following commands are not supported: PASV, RETR, STOR, STRU, 
        /// EPRT, and EPSV. The commands LIST, NLST, and MLSD are supported.
        /// </remarks>
        /// <example>
        /// <code>
        /// FtpsClient ftp = new FtpsClient("ftp.microsoft.com");
        /// ftp.Open("anonymous", "myemail@server.com");
        /// string r = ftp.Quote("SYST");
        /// System.Diagnostics.Debug.WriteLine(r);
        /// ftp.Close();
        /// </code>
        /// </example>
        /// <seealso cref="Quote(string)"/>
        public string SendCustomCommand(string command)
        {
                return Quote(command);
        }

        /// <summary>
        /// Send a raw FTP command to the server.
        /// </summary>
        /// <param name="command">A string containing a valid FTP command value such as SYST.</param>
        /// <returns>The raw textual response from the server.</returns>
        /// <remarks>
        /// This is an advanced feature of the FtpsClient class that allows for any custom or specialized
        /// FTP command to be sent to the FTP server.  Some FTP server support custom commands outside of
        /// the standard FTP command list.  The following commands are not supported: PASV, RETR, STOR, STRU, 
        /// EPRT, and EPSV. The commands LIST, NLST, and MLSD are supported.
        /// </remarks>
        /// <example>
        /// <code>
        /// FtpsClient ftp = new FtpsClient("ftp.microsoft.com");
        /// ftp.Open("anonymous", "myemail@server.com");
        /// string r = ftp.Quote("SYST");
        /// System.Diagnostics.Debug.WriteLine(r);
        /// ftp.Close();
        /// </code>
        /// </example>
        public string Quote(string command)
        {
            if (command == null)
                throw new ArgumentNullException("command");
            if (command.Length < 3)
                throw new ArgumentException(String.Format("Invalid command '{0}'.", command), "command");
            VerifyOpened();            

            char[] separator = { ' ' };
            string[] values = command.Split(separator);

            // extract just the code value
            string code;
            if (values.Length == 0)
            {
                code = command;
            }
            else
            {
                code = values[0];
            }

            // extract the arguments
            string args = string.Empty;
            if (command.Length > code.Length)
            {
                args = command.Replace(code, "").TrimStart();
            }

            FtpsCmd ftpCmd = FtpsCmd.Unknown;
            try
            {
                // try to parse out the command if we can
                ftpCmd = (FtpsCmd)Enum.Parse(typeof(FtpsCmd), code, true);
            }
            catch { }

            if (ftpCmd == FtpsCmd.Pasv 
                || ftpCmd == FtpsCmd.Retr 
                || ftpCmd == FtpsCmd.Stor 
                || ftpCmd == FtpsCmd.Stou 
                || ftpCmd == FtpsCmd.Eprt 
                || ftpCmd == FtpsCmd.Epsv)
                throw new ArgumentException(String.Format("Command '{0}' not supported by Quote() method.", code), "command");

            if (ftpCmd == FtpsCmd.List 
                || ftpCmd == FtpsCmd.Nlst
                || ftpCmd == FtpsCmd.Mlsd)
                return base.TransferText(new FtpsRequest(base.Encoding, ftpCmd, args));
            
            if (ftpCmd == FtpsCmd.Unknown)
                base.SendRequest(new FtpsRequest(base.Encoding, ftpCmd, command));
            else
                base.SendRequest(new FtpsRequest(base.Encoding, ftpCmd, args));

            return base.LastResponseList.GetRawText();
        }

        

        /// <summary>
        /// Sends a NOOP or no operation command to the FTP server.  This can be used to prevent some servers from logging out the
        /// interactive session during file transfer process.
        /// </summary>
        public void NoOperation()
        {
            VerifyOpened();            
            try
            {
                base.SendRequest(new FtpsRequest(base.Encoding, FtpsCmd.Noop));
            }
            catch (FtpsException fex)
            {
                throw new FtpsException("An error occurred while issuing the No Operation command (NOOP).", base.LastResponse, fex);
            }
        }

        /// <summary>
        /// Issues a site specific change file mode (CHMOD) command to the server.  Not all servers implement this command.
        /// </summary>
        /// <param name="path">The path to the file or directory you want to change the mode on.</param>
        /// <param name="octalValue">The CHMOD octal value.</param>
        /// <remarks>
        /// Common CHMOD values used on web servers.
        /// 
        ///       Value 	User 	Group 	Other
        ///         755 	rwx 	r-x 	r-x
        ///         744 	rwx 	r--	    r--
        ///         766 	rwx 	rw- 	rw-
        ///         777 	rwx 	rwx 	rwx
        /// Note that some FTP servers will not accept a full path.  On those systems you must navigate to
        /// the directory containing the file or directory you wish to change the mode on by using with the 
        /// ChangeDirectory() or ChangeDirectoryMultiPath() method.
        /// </remarks>
        /// <seealso cref="SetDateTime"/>
        /// <seealso cref="Rename"/>
        public void ChangeMode(string path, int octalValue)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            if (path.Length == 0)
                throw new ArgumentException("must have a value", "path");
            VerifyOpened();            

            try
            {
                base.SendRequest(new FtpsRequest(base.Encoding, FtpsCmd.Site, "CHMOD", octalValue.ToString(), path));
            }
            catch (FtpsException fex)
            {
                throw new FtpsException(String.Format("Unable to the change file mode for file {0}.  Reason: {1}", path, base.LastResponse.Text), base.LastResponse, fex);
            }

            if (base.LastResponse.Code == FtpsResponseCode.CommandNotImplementedSuperfluousAtThisSite)
                throw new FtpsException(String.Format("Unable to the change file mode for file {0}.  Reason: {1}", path, base.LastResponse.Text), base.LastResponse);
        }

        /// <summary>
        /// Issue a SITE command to the FTP server for site specific implementation commands.
        /// </summary>
        /// <param name="argument">One or more command arguments</param>
        /// <remarks>
        /// For example, the CHMOD command is issued as a SITE command.
        /// </remarks>
        public void Site(string argument)
        {
            if (argument == null)
                throw new ArgumentNullException("argument", "must have a value");
            if (argument.Length == 0)
                throw new ArgumentException("must have a value", "argument");
            VerifyOpened();

            base.SendRequest(new FtpsRequest(base.Encoding, FtpsCmd.Site, argument));
        }

        /// <summary>
        /// Uploads a local file specified in the path parameter to the remote FTP server.   
        /// </summary>
        /// <param name="localPath">Path to a file on the local machine.</param>
        /// <param name="remotePath">Filename or full path to file on the remote FTP server.</param>
        /// <param name="action">The type of put action taken.</param>
        /// <remarks>
        /// The file is uploaded to the current working directory on the remote server.  The remotePath
        /// parameter is used to specify the path and file name used to store the file on the remote server.
        /// Note that some FTP servers will not accept a full path.  On those systems you must navigate to
        /// the directory you wish to put the file using with the ChangeDirectory() or ChangeDirectoryMultiPath()
        /// method.
        /// </remarks>
        /// <seealso cref="PutFileAsync(string, string, FileAction)"/>
        /// <seealso cref="PutFileUnique(string)"/>
        /// <seealso cref="GetFile(string, string, FileAction)"/>
        /// <seealso cref="GetFileAsync(string, string, FileAction)"/>
        /// <seealso cref="MoveFile"/>
        /// <seealso cref="FxpCopy"/>
        /// <seealso cref="FxpCopyAsync"/>    
        public void PutFile(string localPath, string remotePath, FileAction action)
        {
            using (FileStream fileStream = File.OpenRead(localPath))
            {
                PutFile(fileStream, remotePath, action);
            }
        }

        /// <summary>
        /// Uploads a local file specified in the path parameter to the remote FTP server.   
        /// An FtpException is thrown if the file already exists.
        /// </summary>
        /// <param name="localPath">Path to a file on the local machine.</param>
        /// <param name="remotePath">Filename or full path to file on the remote FTP server.</param>
        /// <remarks>
        /// The file is uploaded to the current working directory on the remote server.  The remotePath
        /// parameter is used to specify the path and file name used to store the file on the remote server.
        /// To overwrite an existing file see the method PutFile(string, string, FileAction) and specify the 
        /// FileAction Create.
        /// Note that some FTP servers will not accept a full path.  On those systems you must navigate to
        /// the directory you wish to put the file using with the ChangeDirectory() or ChangeDirectoryMultiPath()
        /// method.
        /// </remarks>
        /// <seealso cref="PutFileAsync(string, string, FileAction)"/>
        /// <seealso cref="PutFileUnique(string)"/>
        /// <seealso cref="GetFile(string, string, FileAction)"/>
        /// <seealso cref="GetFileAsync(string, string, FileAction)"/>
        /// <seealso cref="MoveFile"/>
        /// <seealso cref="FxpCopy"/>
        /// <seealso cref="FxpCopyAsync"/>            
        public void PutFile(string localPath, string remotePath)
        {
            using (FileStream fileStream = File.OpenRead(localPath))
            {
                PutFile(fileStream, remotePath, FileAction.CreateNew);
            }
        }

        /// <summary>
        /// Uploads a local file specified in the path parameter to the remote FTP server.   
        /// </summary>
        /// <param name="localPath">Path to a file on the local machine.</param>
        /// <param name="action">The type of put action taken.</param>
        /// <remarks>
        /// The file is uploaded to the current working directory on the remote server. 
        /// </remarks>
        /// <seealso cref="PutFileAsync(string, string, FileAction)"/>
        /// <seealso cref="PutFileUnique(string)"/>
        /// <seealso cref="GetFile(string, string, FileAction)"/>
        /// <seealso cref="GetFileAsync(string, string, FileAction)"/>
        /// <seealso cref="MoveFile"/>
        /// <seealso cref="FxpCopy"/>
        /// <seealso cref="FxpCopyAsync"/>            
        public void PutFile(string localPath, FileAction action)
        {
            using (FileStream fileStream = File.OpenRead(localPath))
            {
                PutFile(fileStream, ExtractPathItemName(localPath), action);
            }
        }

        /// <summary>
        /// Uploads a local file specified in the path parameter to the remote FTP server.   
        /// An FtpException is thrown if the file already exists.
        /// </summary>
        /// <param name="localPath">Path to a file on the local machine.</param>
        /// <remarks>
        /// The file is uploaded to the current working directory on the remote server. 
        /// </remarks>
        /// <seealso cref="PutFileAsync(string, string, FileAction)"/>
        /// <seealso cref="PutFileUnique(string)"/>
        /// <seealso cref="GetFile(string, string, FileAction)"/>
        /// <seealso cref="GetFileAsync(string, string, FileAction)"/>
        /// <seealso cref="MoveFile"/>
        /// <seealso cref="FxpCopy"/>
        /// <seealso cref="FxpCopyAsync"/>    
        public void PutFile(string localPath)
        {
            using (FileStream fileStream = File.OpenRead(localPath))
            {
                PutFile(fileStream, ExtractPathItemName(localPath), FileAction.CreateNew);
            }
        }

        /// <summary>
        /// Uploads stream data specified in the inputStream parameter to the remote FTP server.   
        /// </summary>
        /// <param name="inputStream">Any open stream object on the local client machine.</param>
        /// <param name="remotePath">Filename or path and filename of the file stored on the remote FTP server.</param>
        /// <param name="action">The type of put action taken.</param>
        /// <remarks>
        /// The stream is uploaded to the current working directory on the remote server.  The remotePath
        /// parameter is used to specify the path and file name used to store the file on the remote server.
        /// Note that some FTP servers will not accept a full path.  On those systems you must navigate to
        /// the directory you wish to put the file using with the ChangeDirectory() or ChangeDirectoryMultiPath()
        /// method.
        /// </remarks>
        /// <seealso cref="PutFileAsync(string, string, FileAction)"/>
        /// <seealso cref="PutFileUnique(string)"/>
        /// <seealso cref="GetFile(string, string, FileAction)"/>
        /// <seealso cref="GetFileAsync(string, string, FileAction)"/>
        /// <seealso cref="MoveFile"/>
        /// <seealso cref="FxpCopy"/>
        /// <seealso cref="FxpCopyAsync"/>        
        public void PutFile(Stream inputStream, string remotePath, FileAction action)
        {
            if (inputStream == null)
                throw new ArgumentNullException("inputStream");
            if (!inputStream.CanRead)
                throw new ArgumentException("must be readable", "inputStream");
            if (remotePath == null)
                throw new ArgumentNullException("remotePath");
            if (remotePath.Length == 0)
                throw new ArgumentException("must contain a value", "remotePath");
            if (action == FileAction.None)
                throw new ArgumentOutOfRangeException("action", "must contain a value other than 'Unknown'");
            VerifyOpened();           

            // set the inputStream if the user supplied it pointing to the end of the stream
            if (inputStream.CanSeek && inputStream.Position == inputStream.Length)
                inputStream.Position = 0;

            WriteToLog(String.Format("Action='PutFile';Status='TransferBegin';RemotePath='{0}';FileAction='{1}'", remotePath, action.ToString()));

            // set the transfer size so we can do some calc on percentage completed
            // in the TransferBytes() method of the base class
            if (IsTransferProgressEventSet()) {
                SetTransferSize(inputStream.Length);
            }

            try
            {
                switch (action)
                {
                    case FileAction.CreateOrAppend:
                        base.TransferData(TransferDirection.ToServer, new FtpsRequest(base.Encoding, FtpsCmd.Appe, remotePath), inputStream);
                        break;
                    case FileAction.CreateNew:
                        if (Exists(remotePath))
                        {
                            throw new FtpsException("Cannot overwrite existing file when action FileAction.CreateNew is specified.");
                        }
                        base.TransferData(TransferDirection.ToServer, new FtpsRequest(base.Encoding, FtpsCmd.Stor, remotePath), inputStream);
                        break;
                    case FileAction.Create:
                        base.TransferData(TransferDirection.ToServer, new FtpsRequest(base.Encoding, FtpsCmd.Stor, remotePath), inputStream);
                        break;
                    case FileAction.Resume:
                        if (!base.Features.Contains(FtpsCmd.Rest, "STREAM"))
                            throw new FtpsCommandNotSupportedException("Cannot resume file transfer.", "REST STREAM");

                        //  get the file size for a resume
                        long remoteSize;
                        if (!TryGetFileSize(remotePath, out remoteSize))
                            throw new FtpsException("unable to determine file size for resume transfer");

                        //  if the files are the same size then there is nothing to transfer
                        if (remoteSize == inputStream.Length)
                            return;

                        // attempt to adjust the transfer size
                        if (IsTransferProgressEventSet() && inputStream.CanSeek) {
                            if (inputStream.Length > remoteSize) {
                                SetTransferSize(inputStream.Length - remoteSize);
                            }
                        }

                        //  transfer file to the server
                        base.TransferData(TransferDirection.ToServer, new FtpsRequest(base.Encoding, FtpsCmd.Stor, remotePath), inputStream, remoteSize);
                        break;
                    case FileAction.ResumeOrCreate:
                        // if the remote file exists then do a resume otherwise do a create
                        if (Exists(remotePath))
                        {
                            PutFile(inputStream, remotePath, FileAction.Resume);
                        }
                        else
                        {
                            PutFile(inputStream, remotePath, FileAction.Create);
                        }
                        break;
                }
            }
            catch (FtpsException fex)
            {
                WriteToLog(String.Format("Action='PutFile';Status='TransferError';RemotePath='{0}';FileAction='{1}';ErrorMessage='{2}'", remotePath, action.ToString(), fex.Message));
                throw new FtpsDataTransferException(String.Format("An error occurred while putting fileName '{0}'.", remotePath), base.LastResponse, fex);
            }

            WriteToLog(String.Format("Action='PutFile';Status='TransferSuccess';RemotePath='{0}';FileAction='{1}'", remotePath, action.ToString()));
        }

        /// <summary>
        /// File Exchange Protocol (FXP) allows server-to-server transfer which can greatly speed up file transfers.
        /// </summary>
        /// <param name="fileName">The name of the file to transfer.</param>
        /// <param name="destination">The destination FTP server which must be supplied as an open and connected FtpsClient object.</param>
        /// <remarks>
        /// Both servers must support and have FXP enabled before you can transfer files between two remote servers using FXP.  One FTP server must support PASV mode and the other server must allow PORT commands from a foreign address.  Finally, firewall settings may interfer with the ability of one server to access the other server.
        /// Starksoft FtpsClient will coordinate the FTP negoitaion and necessary PORT and PASV transfer commands.
        /// </remarks>
        /// <seealso cref="FxpTransferTimeout"/>
        /// <seealso cref="FxpCopyAsync"/> 
        public void FxpCopy(string fileName, FtpsClient destination)
        {
            if (this.IsConnected == false)
                throw new FtpsException("The connection must be open before a transfer between servers can be intitiated.");
            if (destination == null)
                throw new ArgumentNullException("destination");
            if (destination.IsConnected == false)
                throw new FtpsException("The destination object must be open and connected before a transfer between servers can be intitiated.");
            if (String.IsNullOrEmpty(fileName))
                throw new ArgumentException("must have a value", "fileName");
			VerifyOpened();            			

            //  send command to destination FTP server to get passive port to be used from the source FTP server
            try
            {
                destination.SendRequest(new FtpsRequest(base.Encoding, FtpsCmd.Pasv));
            }
            catch (FtpsException fex)
            {
                throw new FtpsException(String.Format("An error occurred when trying to set up the passive connection on '{1}' for a destination to destination copy between '{0}' and '{1}'.", this.Host, destination.Host), base.LastResponse, fex);
            }

            //  get the begin and end positions to extract data from the response string
            int startIdx = destination.LastResponse.Text.IndexOf("(") + 1;
            int endIdx = destination.LastResponse.Text.IndexOf(")");
            string dataPortInfo = destination.LastResponse.Text.Substring(startIdx, endIdx - startIdx);

            //  send a command to the source server instructing it to connect to
            //  the local ip address and port that the destination server will be bound to
            try
            {
                this.SendRequest(new FtpsRequest(base.Encoding, FtpsCmd.Port, dataPortInfo));
            }
            catch (FtpsException fex)
            {
                throw new FtpsException(String.Format("Command instructing '{0}' to open connection failed.", this.Host), base.LastResponse, fex);
            }

            // send command to tell the source server to retrieve the file from the destination server
            try
            {
                this.SendRequest(new FtpsRequest(base.Encoding, FtpsCmd.Retr, fileName));
            }
            catch (FtpsException fex)
            {
                throw new FtpsException(String.Format("An error occurred transfering on a server to server copy between '{0}' and '{1}'.", this.Host, destination.Host), base.LastResponse, fex);
            }

            // send command to tell the destination to store the file
            try
            {
                destination.SendRequest(new FtpsRequest(base.Encoding, FtpsCmd.Stor, fileName));
            }
            catch (FtpsException fex)
            {
                throw new FtpsException(String.Format("An error occurred transfering on a server to server copy between '{0}' and '{1}'.", this.Host, destination.Host), base.LastResponse, fex);
            }

            // wait until we get a file completed response back from the destination server and the source server
            destination.WaitForHappyCodes(this.FxpTransferTimeout, FtpsResponseCode.RequestedFileActionOkayAndCompleted, FtpsResponseCode.ClosingDataConnection);
            this.WaitForHappyCodes(this.FxpTransferTimeout, FtpsResponseCode.RequestedFileActionOkayAndCompleted, FtpsResponseCode.ClosingDataConnection);
        }

        /// <summary>
        /// Instructs the FTP server to try to set the server to UTF-8 encoding on.  ASCII encoding is assumed on a failure.
        /// </summary>
        /// <remarks>
        /// This method will attempt to execute the OPTS UTF8 ON command if UTF8 is a supported feature and OPTS is a supported command.
        /// FileZilla and UTF-8 Info: http://wiki.filezilla-project.org/Character_Set
        /// RFC 2640 - UTF8 options: http://tools.ietf.org/html/rfc2640        
        /// </remarks>
        /// <seealso cref="SetUtf8On"/>
        /// <seealso cref="SetUtf8Off"/>
        /// <returns>True if UTF-8 encoding is set; otherwise false.</returns>
        public bool TrySetUtf8On()
        {
            try
            {
                SetUtf8On();
                return true;
            }
            catch (FtpsException)
            {
                // assume ASCII encoding on failure.
                base.Encoding = Encoding.ASCII;
            };

            return false;
        }

        /// <summary>
        /// Instructs the FTP server to set the server to UTF-8 encoding on.
        /// </summary>
        /// <remarks>
        /// This method will attempt to execute the OPTS UTF8 ON command if UTF8 is a supported feature and OPTS is a supported command.
        /// FileZilla and UTF-8 Info: http://wiki.filezilla-project.org/Character_Set
        /// RFC 2640 - UTF8 options: http://tools.ietf.org/html/rfc2640        
        /// </remarks>
        /// <exception cref="FtpsFeatureException">If UTF-8 encoding is not a supported feature, a FtpFeatureException will be thrown.</exception>
        /// <exception cref="FtpsCommandNotSupportedException">If OPTS is not a supported command then a FtpCommandNotSupportedException will be thrown.</exception>
        /// <seealso cref="SetUtf8Off"/>
        /// <seealso cref="TrySetUtf8On"/>
        public void SetUtf8On()
        {
            // test to see if UTF8 is supported by the server in the features list
            if (base.Features.Contains("UTF8"))
            {
                SetOptions("UTF8 ON");
                base.Encoding = Encoding.UTF8;
            }
            else
            {
                throw new FtpsFeatureException("The feature option UTF8 is not supported by the FTP server.");
            }
        }

        /// <summary>
        /// Instructs the FTP server to set the server to UTF-8 encoding off.
        /// </summary>
        /// <remarks>
        /// This method will attempt to execute the OPTS UTF8 OFF command if UTF8 is a supported feature and OPTS is a supported command.
        /// FileZilla and UTF-8 Info: http://wiki.filezilla-project.org/Character_Set
        /// RFC 2640 - UTF8 options: http://tools.ietf.org/html/rfc2640        
        /// </remarks>
        /// <exception cref="FtpsFeatureException">If UTF-8 encoding is not a supported feature, a FtpFeatureException will be thrown.</exception>
        /// <exception cref="FtpsCommandNotSupportedException">If OPTS is not a supported command then a FtpCommandNotSupportedException will be thrown.</exception>
        /// <seealso cref="SetUtf8On"/>
        /// <seealso cref="TrySetUtf8On"/>
        public void SetUtf8Off()
        {
            // test to see if UTF8 is supported by the server in the features list
            if (base.Features.Contains("UTF8"))
            {
                SetOptions("UTF8 OFF");
                base.Encoding = Encoding.ASCII;
            }
            else
            {
                throw new FtpsFeatureException("The feature option UTF8 is not supported by the FTP server.");
            }
        }

        
        /// <summary>
        /// Executes the OPTS (Options) command on the FTP server.
        /// </summary>
        /// <remarks>
        /// Every FTP server supports a different set of options that can be set and specified
        /// to turn on or off specific features on the FTP server.  Use this command to set 
        /// specific options on or off.
        /// Example:
        ///     Options("UTF8 OFF");
        /// </remarks>
        /// <param name="parameters">Paramater text to execute with the OPTS command.</param>
        /// <seealso cref="SetUtf8On"/>
        /// <seealso cref="SetUtf8Off"/>
        /// <seealso cref="TrySetUtf8On"/>
        public void SetOptions(string parameters)
        {
            if (String.IsNullOrEmpty(parameters))
                throw new ArgumentException("must have a value", "fileName");
            VerifyOpened();            

            try
            {
                base.SendRequest(new FtpsRequest(base.Encoding, FtpsCmd.Opts, parameters));
            }
            catch (FtpsException fex)
            {
                throw new FtpsException("An error occurred while executing the OPTS command.", base.LastResponse, fex);
            }

        }        

#endregion

#region Private Methods

        /// <summary>
        /// Attempt to send the CLieNT command to identify the FTP client.  Some FTP
        /// servers require this command to function properly and many other ignore it
        /// or simply do not implement the command.  Errors are ignored.
        /// </summary>
        private void TrySetClient()
        {
            try
            {
                if (base.Features.Contains(FtpsCmd.Clnt))
                    SendRequest(new FtpsRequest(base.Encoding, FtpsCmd.Clnt, FTP_CLIENT_NAME));
            }
            catch (FtpsException)
            {
                // ignore errors
            }
        }


        /// <summary>
        /// Attempt to set the file transfer type.  If unsuccesful ignore errors
        /// and set the FileTransferType to 'Unknown'.
        /// </summary>
        private void TrySetFileTransferType()
        {
            try
            {
                SetFileTransferType(_fileTransferType);
            }
            catch (FtpsException)
            {
                // on error assume Unknown transfer type
                _fileTransferType = TransferType.Unknown;
            }
        }

        /// <summary>
        /// Attempt to enable compress if that option has been selected.  If the operation fails
        /// an exception is thrown and the connection is terminated.
        /// </summary>
        private void SetCompression()
        {
            if (base.IsCompressionEnabled)
            {
                base.CompressionOn();
            }
        }

        private void CreateItemParsers()
        {
            // if the custom item parser is not set then set to use the built-in generic parser
            if (_listItemParser == null)
                _listItemParser = new FtpsListItemParser();
            // create the MLSx item parser
            _mlsxItemParser = new FtpsMlsxItemParser();
        }

        // send command to set the file transfer type on the FTP server
        private void SetFileTransferType(TransferType t)
        {
            switch (t)
            {
                case TransferType.Binary:
                    base.SendRequest(new FtpsRequest(base.Encoding, FtpsCmd.Type, TYPE_BINARY_I));
                    break;
                case TransferType.Ascii:
                    base.SendRequest(new FtpsRequest(base.Encoding, FtpsCmd.Type, TYPE_ASCII_A));
                    break;
            }
        }

        private void SendPassword(string password)
        {
            // some ftp servers do not require passwords for users and will log you in immediately - no password command is required
            if (base.LastResponse.Code != FtpsResponseCode.UserLoggedIn)
            {
                try
                {
                    // send the password request as ASCII encoded text always                    
                    base.SendRequest(new FtpsRequest(Encoding.ASCII, FtpsCmd.Pass, password));
                }
                catch (FtpsException fex)
                {
                    throw new FtpsConnectionOpenException(String.Format("An error occurred when sending password information.  Reason: {0}", base.LastResponse.Text), fex);
                }

                if (base.LastResponse.Code == FtpsResponseCode.NotLoggedIn)
                    throw new FtpsLoginException("Unable to log into FTP destination with supplied username and password.");
            }
        }

        private void SendUser(string user)
        {
            try
            {
                // send the username request as ASCII encoded text always
                base.SendRequest(new FtpsRequest(Encoding.ASCII, FtpsCmd.User, user));
            }
            catch (FtpsException fex)
            {
                throw new FtpsConnectionOpenException(String.Format("An error occurred when sending user information.  Reason: {0}", base.LastResponse.Text), fex);
            }
        }

        private bool CatchUserCancel()
        {
            // test to see if this is an asychronous operation and if so make sure 
            // the user has not requested the operation to be canceled
            if (base.AsyncWorker != null && base.AsyncWorker.CancellationPending)
            {
                base.CloseAllConnections();
                return true;
            }
            return false;
        }

        private void ParseDirListDeep(string path, FtpsItemCollection deepCol)
        {
            FtpsItemCollection list = GetDirList(path);
            deepCol.Merge(list);

            foreach (FtpsItem item in list)
            {
                // if the this call is being completed asynchronously and the user requests a cancellation
                // then stop processing the items and return
                if (base.AsyncWorker != null && base.AsyncWorker.CancellationPending)
                    return;

                // if the item is of type Directory then parse the directory list recursively
                if (item.ItemType == FtpItemType.Directory)
                    ParseDirListDeep(item.FullPath, deepCol);
            }
        }

        private string CorrectLocalPath(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            if (path.Length == 0)
                throw new ArgumentException("must have a value", "path");

            string fileName = ExtractPathItemName(path);
            string pathOnly = path.Substring(0, path.Length - fileName.Length - 1);

            // if the pathOnly portion contains the root node then we need to add the 
            // a directory slash back otherwise the final combined path will be something
            // like c:myfile.txt and this will result
            if (pathOnly.EndsWith(":") && pathOnly.IndexOf("\\") == -1)
            {
                pathOnly += "\\";
            }

            char[] invalidPath = Path.GetInvalidPathChars();
            if(path.IndexOfAny(invalidPath) != -1)
            {
                for (int i = 0; i < invalidPath.Length; i++)
                {
                    if (pathOnly.IndexOf(invalidPath[i]) != -1)
                        pathOnly = pathOnly.Replace(invalidPath[i], '_');
                }
            }

            char[] invalidFile = Path.GetInvalidFileNameChars();
            if (fileName.IndexOfAny(invalidFile) != -1)
            {
                for (int i = 0; i < invalidFile.Length; i++)
                {
                    if (fileName.IndexOf(invalidFile[i]) != -1)
                        fileName = fileName.Replace(invalidFile[i], '_');
                }
            }

            return Path.Combine(pathOnly, fileName);

        }
       
        private string ExtractPathItemName(string path)
        {
            if (path.IndexOf("\\") != -1)
                return path.Substring(path.LastIndexOf("\\") + 1);
            else if (path.IndexOf("/") != -1)
                return path.Substring(path.LastIndexOf("/") + 1);
            else if (path.Length > 0)
                return path;
            else
                throw new FtpsException(String.Format(CultureInfo.InvariantCulture, "Item name not found in path {0}.", path));
        }


        private void WriteToLog(string message)
        {
            if (!_isLoggingOn)
                return;

            string line = String.Format("[{0}] [{1}] [{2}] {3}\r\n", DateTime.Now.ToString("G"), base.Host, base.Port.ToString(), message);
            byte[] buffer = base.Encoding.GetBytes(line);
            _log.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Set the date and time for a specific file or directory on the server using a specific FTP command.
        /// </summary>
        /// <param name="path">The path or name of the file or directory.</param>
        /// <param name="dateTime">New date to set on the file or directory.</param>
        /// <param name="cmd">MFCT or MFMT command to use</param>
        private void SetDateTime(string path, DateTime dateTime, FtpsCmd cmd)
        {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentException("must have a value", "path");

            if (cmd != FtpsCmd.Mfmt && cmd != FtpsCmd.Mfct)
                throw new ArgumentOutOfRangeException("cmd", "only Mfmt and Mfct options are supported");

            if (!base.Features.Contains(cmd))
                throw new FtpsCommandNotSupportedException("Cannot set date time of file.", cmd);
                        
            // replay format: MFMT [YYMMDDHHMMSS] [filename]

            string dateTimeArg = dateTime.ToString("yyyyMMddHHmmss");

            try
            {
                base.SendRequest(new FtpsRequest(base.Encoding, cmd, dateTimeArg, path));
            }
            catch (FtpsException fex)
            {
                throw new FtpsException(String.Format("An error occurred when setting a file date and time for '{0}'.", path), fex);
            }
        }

        private FtpsRequest CreateDirListingRequest()
        {
            return CreateDirListingRequest("");
        }


        private FtpsRequest CreateDirListingRequest(string path)
        {
            switch(_dirListingMethod)
            {
                case ListingMethod.List:
                    return new FtpsRequest(base.Encoding, FtpsCmd.List, path);
                case ListingMethod.ListAl:
                    return new FtpsRequest (base.Encoding, FtpsCmd.List, "-al", path);
                case ListingMethod.Mlsx:
                    return new FtpsRequest(base.Encoding, FtpsCmd.Mlsd, path);
                case ListingMethod.Automatic:
                    if (base.Features.Contains(FtpsCmd.Mlsd) || base.Features.Contains(FtpsCmd.Mlst))
                        return new FtpsRequest(base.Encoding, FtpsCmd.Mlsd, path);
                    else
                        return new FtpsRequest(base.Encoding, FtpsCmd.List, "-al", path);
                default:
                    throw new FtpsException("unknown directory listing option");
            }
        }

        private IFtpsItemParser GetItemParser()
        {
            switch (_dirListingMethod)
            {
                case ListingMethod.List:
                case ListingMethod.ListAl:
                    return _listItemParser;
                case ListingMethod.Mlsx:
                    return _mlsxItemParser;
                case ListingMethod.Automatic:
                    if (base.Features.Contains(FtpsCmd.Mlsd) || base.Features.Contains(FtpsCmd.Mlst))
                        return _mlsxItemParser;
                    else
                        return _listItemParser;
                default:
                    throw new FtpsException("unknown directory listing option");
            }
        }

        
#endregion

#region  Asynchronous Methods and Events

        private Exception _asyncException;

        ////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Event handler for GetDirListAsync method.
        /// </summary>
        /// <seealso cref="GetDirListAsync(string)"/>
        /// <seealso cref="FtpsBase.CancelAsync"/>
        public event EventHandler<GetDirListAsyncCompletedEventArgs> GetDirListAsyncCompleted;

        /// <summary>
        /// Asynchronously retrieves a list of the files from current working directory on the remote FTP 
        /// server using the LIST command.  
        /// </summary>
        /// <remarks>
        /// This method returns a FtpItemList collection of FtpItem objects through the GetDirListAsyncCompleted event.
        /// </remarks>
        /// <seealso cref="GetDirListAsyncCompleted"/>
        /// <seealso cref="FtpsBase.CancelAsync"/>
        /// <seealso cref="GetDirList()"/>
        /// <seealso cref="GetDirList(string)"/>
        /// <seealso cref="GetDirListAsText()"/>
        /// <seealso cref="GetDirListAsText(string)"/>
        /// <seealso cref="GetDirListAsync(string)"/>
        /// <seealso cref="GetDirListDeep(string)"/>
        /// <seealso cref="GetDirListDeepAsync(string)"/>
        /// <seealso cref="GetNameList()"/>
        /// <seealso cref="GetNameList(string)"/>
        public void GetDirListAsync()
        {
            GetDirListAsync(string.Empty);
        }

        /// <summary>
        /// Asynchronously retrieves a list of the files from a specified path on the remote FTP 
        /// server using the LIST command. 
        /// </summary>
        /// <param name="path">The path to a directory on the remote FTP server.</param>
        /// <remarks>This method returns a FtpFileCollection object containing a collection of 
        /// FtpItem objects.  The FtpFileCollection is returned though the GetDirListAsyncCompleted event.</remarks>
        /// <seealso cref="FtpsBase.CancelAsync"/>
        /// <seealso cref="GetDirList()"/>
        /// <seealso cref="GetDirList(string)"/>
        /// <seealso cref="GetDirListAsText()"/>
        /// <seealso cref="GetDirListAsText(string)"/>
        /// <seealso cref="GetDirListAsync()"/>
        /// <seealso cref="GetDirListDeep(string)"/>
        /// <seealso cref="GetDirListDeepAsync(string)"/>
        /// <seealso cref="GetNameList()"/>
        /// <seealso cref="GetNameList(string)"/>
        public void GetDirListAsync(string path)
        {
            if (base.AsyncWorker != null && base.AsyncWorker.IsBusy)
                throw new InvalidOperationException("The FtpsClient object is already busy executing another asynchronous operation.  You can only execute one asychronous method at a time.");

            base.CreateAsyncWorker();
            base.AsyncWorker.WorkerSupportsCancellation = true;
            base.AsyncWorker.DoWork += new DoWorkEventHandler(GetDirListAsync_DoWork);
            base.AsyncWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(GetDirListAsync_RunWorkerCompleted);
            base.AsyncWorker.RunWorkerAsync(path);
        }

        private void GetDirListAsync_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                string path = (string)e.Argument;
                e.Result = GetDirList(path);
            }
            catch (Exception ex)
            {
                _asyncException = ex;
            }
        }

        private void GetDirListAsync_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (GetDirListAsyncCompleted != null)
                GetDirListAsyncCompleted(this, new GetDirListAsyncCompletedEventArgs(_asyncException, base.IsAsyncCanceled, (FtpsItemCollection)e.Result));
            _asyncException = null;
        }

        ////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Event handler for GetDirListDeepAsync method.
        /// </summary>
        public event EventHandler<GetDirListDeepAsyncCompletedEventArgs> GetDirListDeepAsyncCompleted;

        /// <summary>
        /// Asynchronous deep retrieval of a list of all files and all sub directories from the current path on the remote FTP 
        /// server using the LIST command. 
        /// </summary>
        /// <remarks>This method returns a FtpFileCollection object containing a collection of FtpItem objects through the GetDirListDeepAsyncCompleted event.</remarks>
        /// <seealso cref="GetDirListDeepAsyncCompleted"/>
        /// <seealso cref="FtpsBase.CancelAsync"/>
        /// <seealso cref="GetDirList()"/>
        /// <seealso cref="GetDirList(string)"/>
        /// <seealso cref="GetDirListAsText()"/>
        /// <seealso cref="GetDirListAsText(string)"/>
        /// <seealso cref="GetDirListAsync(string)"/>
        /// <seealso cref="GetDirListDeep(string)"/>
        /// <seealso cref="GetDirListDeepAsync(string)"/>
        /// <seealso cref="GetNameList()"/>
        /// <seealso cref="GetNameList(string)"/>
        public void GetDirListDeepAsync()
        {
            GetDirListDeepAsync(GetWorkingDirectory());
        }

        /// <summary>
        /// Asynchronous deep retrieval of a list of all files and all sub directories from a specified path on the remote FTP 
        /// server using the LIST command. 
        /// </summary>
        /// <param name="path">The path to a directory on the remote FTP server.</param>
        /// <remarks>This method returns a FtpFileCollection object containing a collection of 
        /// FtpItem objects the GetDirListDeepAsyncCompleted event.</remarks>
        /// <seealso cref="GetDirListDeepAsyncCompleted"/>
        /// <seealso cref="FtpsBase.CancelAsync"/>
        /// <seealso cref="GetDirList()"/>
        /// <seealso cref="GetDirList(string)"/>
        /// <seealso cref="GetDirListAsText()"/>
        /// <seealso cref="GetDirListAsText(string)"/>
        /// <seealso cref="GetDirListAsync()"/>
        /// <seealso cref="GetDirListDeep(string)"/>
        /// <seealso cref="GetDirListDeepAsync(string)"/>
        /// <seealso cref="GetNameList()"/>
        /// <seealso cref="GetNameList(string)"/>
        public void GetDirListDeepAsync(string path)
        {
            if (base.AsyncWorker != null && base.AsyncWorker.IsBusy)
                throw new InvalidOperationException("The FtpsClient object is already busy executing another asynchronous operation.  You can only execute one asychronous method at a time.");

            base.CreateAsyncWorker();
            base.AsyncWorker.WorkerSupportsCancellation = true;
            base.AsyncWorker.DoWork += new DoWorkEventHandler(GetDirListDeepAsync_DoWork);
            base.AsyncWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(GetDirListDeepAsync_RunWorkerCompleted);
            base.AsyncWorker.RunWorkerAsync(path);
        }

        private void GetDirListDeepAsync_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                string path = (string)e.Argument;
                e.Result = GetDirList(path);
            }
            catch (Exception ex)
            {
                _asyncException = ex;
            }
        }

        private void GetDirListDeepAsync_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (GetDirListDeepAsyncCompleted != null)
                GetDirListDeepAsyncCompleted(this, new GetDirListDeepAsyncCompletedEventArgs(_asyncException, base.IsAsyncCanceled, (FtpsItemCollection)e.Result));
            _asyncException = null;
        }


        ////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Event that fires when the GetFileAsync method is invoked.
        /// </summary>
        public event EventHandler<GetFileAsyncCompletedEventArgs> GetFileAsyncCompleted;

        /// <summary>
        /// Asynchronously retrieves a remote file from the FTP server and writes the data to a local file
        /// specfied in the localPath parameter.
        /// </summary>
        /// <param name="remotePath">A path and/or file name to the remote file.</param>
        /// <param name="localPath">A fully qualified local path to a file on the local machine.</param>
        /// <param name="action">The type of action to take.</param>
        /// <seealso cref="GetFileAsyncCompleted"/>
        /// <seealso cref="FtpsBase.CancelAsync"/>
        /// <seealso cref="GetFile(string, string, FileAction)"/>
        /// <seealso cref="PutFile(string)"/>
        /// <seealso cref="PutFileAsync(string, string, FileAction)"/>
        /// <seealso cref="MoveFile"/>
        /// <seealso cref="FxpCopy"/>
        /// <seealso cref="FxpCopyAsync"/>
        public void GetFileAsync(string remotePath, string localPath, FileAction action)
        {
            if (base.AsyncWorker != null && base.AsyncWorker.IsBusy)
                throw new InvalidOperationException("The FtpsClient object is already busy executing another asynchronous operation.  You can only execute one asychronous method at a time.");

            base.CreateAsyncWorker();
            base.AsyncWorker.WorkerSupportsCancellation = true;
            base.AsyncWorker.DoWork += new DoWorkEventHandler(GetFileAsync_DoWork);
            base.AsyncWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(GetFileAsync_RunWorkerCompleted);
            Object[] args = new Object[3];
            args[0] = remotePath;
            args[1] = localPath;
            args[2] = action;
            base.AsyncWorker.RunWorkerAsync(args);
        }

        private void GetFileAsync_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                Object[] args = (Object[])e.Argument;
                GetFile((string)args[0], (string)args[1], (FileAction)args[2]);
            }
            catch (Exception ex)
            {
                _asyncException = ex;
            }

        }

        private void GetFileAsync_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (GetFileAsyncCompleted != null)
                GetFileAsyncCompleted(this, new GetFileAsyncCompletedEventArgs(_asyncException, base.IsAsyncCanceled));
            _asyncException = null;
        }

        /// <summary>
        /// Asynchronously retrieves a remote file from the FTP server and writes the data to a local stream object
        /// specfied in the outStream parameter.
        /// </summary> 
        /// <param name="remotePath">A path and/or file name to the remote file.</param>
        /// <param name="outStream">An output stream object used to stream the remote file to the local machine.</param>
        /// <param name="restart">A true/false value to indicate if the file download needs to be restarted due to a previous partial download.</param>
        /// <remarks>
        /// If the remote path is a file name then the file is downloaded from the FTP server current working directory.  Otherwise a fully qualified
        /// path for the remote file may be specified.  The output stream must be writeable and can be any stream object.  Finally, the restart parameter
        /// is used to send a restart command to the FTP server.  The FTP server is instructed to restart the download process at the last position of
        /// of the output stream.  Not all FTP servers support the restart command.  If the FTP server does not support the restart (REST) command,
        /// an FtpException error is thrown.
        /// </remarks>        
        /// <seealso cref="GetFileAsyncCompleted"/>
        /// <seealso cref="FtpsBase.CancelAsync"/>
        /// <seealso cref="GetFile(string, string)"/>
        /// <seealso cref="PutFile(string, string, FileAction)"/>
        /// <seealso cref="PutFileAsync(string, string, FileAction)"/>
        /// <seealso cref="MoveFile"/>
        /// <seealso cref="FxpCopy"/>
        /// <seealso cref="FxpCopyAsync"/>
        public void GetFileAsync(string remotePath, Stream outStream, bool restart)
        {
            if (base.AsyncWorker != null && base.AsyncWorker.IsBusy)
                throw new InvalidOperationException("The FtpsClient object is already busy executing another asynchronous operation.  You can only execute one asychronous method at a time.");

            base.CreateAsyncWorker();
            base.AsyncWorker.WorkerSupportsCancellation = true;
            base.AsyncWorker.DoWork += new DoWorkEventHandler(GetFileStreamAsync_DoWork);
            base.AsyncWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(GetFileAsync_RunWorkerCompleted);
            Object[] args = new Object[3];
            args[0] = remotePath;
            args[1] = outStream;
            args[2] = restart;
            base.AsyncWorker.RunWorkerAsync(args);
        }

        private void GetFileStreamAsync_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                Object[] args = (Object[])e.Argument;
                GetFile((string)args[0], (Stream)args[1], (bool)args[2]);
            }
            catch (Exception ex)
            {
                _asyncException = ex;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Asynchronous event for PutFileAsync method.
        /// </summary>
        /// <seealso cref="PutFileAsync(string, string, FileAction)"/>
        public event EventHandler<PutFileAsyncCompletedEventArgs> PutFileAsyncCompleted;

        /// <summary>
        /// Asynchronously uploads a local file specified in the path parameter to the remote FTP server.   
        /// </summary>
        /// <param name="localPath">Path to a file on the local machine.</param>
        /// <param name="remotePath">Filename or full path to file on the remote FTP server.</param>
        /// <param name="action">The type of put action taken.</param>
        /// <remarks>
        /// The file is uploaded to the current working directory on the remote server.  The remotePath
        /// parameter is used to specify the path and file name used to store the file on the remote server.
        /// </remarks>
        /// <seealso cref="PutFileAsyncCompleted"/>
        /// <seealso cref="FtpsBase.CancelAsync"/>
        /// <seealso cref="PutFile(string, string, FileAction)"/>
        /// <seealso cref="PutFileUnique(string)"/>
        /// <seealso cref="GetFile(string, string, FileAction)"/>
        /// <seealso cref="GetFileAsync(string, string, FileAction)"/>
        /// <seealso cref="MoveFile"/>
        /// <seealso cref="FxpCopy"/>
        /// <seealso cref="FxpCopyAsync"/>  
        public void PutFileAsync(string localPath, string remotePath, FileAction action)
        {
            if (base.AsyncWorker != null && base.AsyncWorker.IsBusy)
                throw new InvalidOperationException("The FtpsClient object is already busy executing another asynchronous operation.  You can only execute one asychronous method at a time.");

            base.CreateAsyncWorker();
            base.AsyncWorker.WorkerSupportsCancellation = true;
            base.AsyncWorker.DoWork += new DoWorkEventHandler(PutFileAsync_DoWork);
            base.AsyncWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(PutFileAsync_RunWorkerCompleted);
            Object[] args = new Object[3];
            args[0] = localPath;
            args[1] = remotePath;
            args[2] = action;
            base.AsyncWorker.RunWorkerAsync(args);
        }

        private void PutFileAsync_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                Object[] args = (Object[])e.Argument;
                PutFile((string)args[0], (string)args[1], (FileAction)args[2]);
            }
            catch (Exception ex)
            {
                _asyncException = ex;
            }
        }

        private void PutFileAsync_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (PutFileAsyncCompleted != null)
                PutFileAsyncCompleted(this, new PutFileAsyncCompletedEventArgs(_asyncException, base.IsAsyncCanceled));
            _asyncException = null;
        }

        /// <summary>
        /// Asynchronously uploads stream data specified in the inputStream parameter to the remote FTP server.   
        /// </summary>
        /// <param name="inputStream">Any open stream object on the local client machine.</param>
        /// <param name="remotePath">Filename or path and filename of the file stored on the remote FTP server.</param>
        /// <param name="action">The type of put action taken.</param>
        /// <remarks>
        /// The stream is uploaded to the current working directory on the remote server.  The remotePath
        /// parameter is used to specify the path and file name used to store the file on the remote server.
        /// </remarks>
        /// <seealso cref="PutFileAsyncCompleted"/>
        /// <seealso cref="FtpsBase.CancelAsync"/>
        /// <seealso cref="PutFile(string, string, FileAction)"/>
        /// <seealso cref="GetFile(string, string, FileAction)"/>
        /// <seealso cref="GetFileAsync(string, string, FileAction)"/>
        /// <seealso cref="MoveFile"/>
        /// <seealso cref="FxpCopy"/>
        /// <seealso cref="FxpCopyAsync"/>    
        public void PutFileAsync(Stream inputStream, string remotePath, FileAction action)
        {
            if (base.AsyncWorker != null && base.AsyncWorker.IsBusy)
                throw new InvalidOperationException("The FtpsClient object is already busy executing another asynchronous operation.  You can only execute one asychronous method at a time.");

            base.CreateAsyncWorker();
            base.AsyncWorker.WorkerSupportsCancellation = true;
            base.AsyncWorker.DoWork += new DoWorkEventHandler(PutFileStreamAsync_DoWork);
            base.AsyncWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(PutFileAsync_RunWorkerCompleted);
            Object[] args = new Object[3];
            args[0] = inputStream;
            args[1] = remotePath;
            args[2] = action;
            base.AsyncWorker.RunWorkerAsync(args);
        }

        private void PutFileStreamAsync_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                Object[] args = (Object[])e.Argument;
                PutFile((Stream)args[0], (string)args[1], (FileAction)args[2]);
            }
            catch (Exception ex)
            {
                _asyncException = ex;
            }
        }

        /// <summary>
        /// Asynchronously uploads a local file specified in the path parameter to the remote FTP server.   
        /// </summary>
        /// <param name="localPath">Path to a file on the local machine.</param>
        /// <param name="action">The type of put action taken.</param>
        /// <remarks>
        /// The file is uploaded to the current working directory on the remote server. 
        /// </remarks>
        /// <seealso cref="PutFileAsyncCompleted"/>
        /// <seealso cref="FtpsBase.CancelAsync"/>
        /// <seealso cref="PutFile(string, string, FileAction)"/>
        /// <seealso cref="PutFileUnique(string)"/>
        /// <seealso cref="GetFile(string, string, FileAction)"/>
        /// <seealso cref="GetFileAsync(string, string, FileAction)"/>
        /// <seealso cref="MoveFile"/>
        /// <seealso cref="FxpCopy"/>
        /// <seealso cref="FxpCopyAsync"/>    
        public void PutFileAsync(string localPath, FileAction action)
        {
            if (base.AsyncWorker != null && base.AsyncWorker.IsBusy)
                throw new InvalidOperationException("The FtpsClient object is already busy executing another asynchronous operation.  You can only execute one asychronous method at a time.");

            base.CreateAsyncWorker();
            base.AsyncWorker.WorkerSupportsCancellation = true;
            base.AsyncWorker.DoWork += new DoWorkEventHandler(PutFileLocalAsync_DoWork);
            base.AsyncWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(PutFileAsync_RunWorkerCompleted);
            Object[] args = new Object[2];
            args[0] = localPath;
            args[1] = action;
            base.AsyncWorker.RunWorkerAsync(args);
        }

        private void PutFileLocalAsync_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                Object[] args = (Object[])e.Argument;
                PutFile((string)args[0], (FileAction)args[1]);
            }
            catch (Exception ex)
            {
                _asyncException = ex;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Event handler for OpenAsync method.
        /// </summary>
        public event EventHandler<OpenAsyncCompletedEventArgs> OpenAsyncCompleted;

        /// <summary>
        /// Asynchronously opens a connection to the remote FTP server and log in with user name and password credentials.
        /// </summary>
        /// <param name="user">User name.  Many public ftp allow for this value to 'anonymous'.</param>
        /// <param name="password">Password.  Anonymous public ftp servers generally require a valid email address for a password.</param>
        /// <remarks>Use the Close() method to log off and close the connection to the FTP server.</remarks>
        /// <seealso cref="OpenAsyncCompleted"/>
        /// <seealso cref="FtpsBase.CancelAsync"/>
        /// <seealso cref="Open"/>
        /// <seealso cref="Reopen"/>
        /// <seealso cref="Close"/>
        public void OpenAsync(string user, string password)
        {
            if (base.AsyncWorker != null && base.AsyncWorker.IsBusy)
                throw new InvalidOperationException("The FtpsClient object is already busy executing another asynchronous operation.  You can only execute one asychronous method at a time.");

            base.CreateAsyncWorker();
            base.AsyncWorker.WorkerSupportsCancellation = true;
            base.AsyncWorker.DoWork += new DoWorkEventHandler(OpenAsync_DoWork);
            base.AsyncWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(OpenAsync_RunWorkerCompleted);
            Object[] args = new Object[2];
            args[0] = user;
            args[1] = password;
            base.AsyncWorker.RunWorkerAsync(args);
        }

        private void OpenAsync_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                Object[] args = (Object[])e.Argument;
                Open((string)args[0], (string)args[1]);
            }
            catch (Exception ex)
            {
                _asyncException = ex;
            }
        }

        private void OpenAsync_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (OpenAsyncCompleted != null)
                OpenAsyncCompleted(this, new OpenAsyncCompletedEventArgs(_asyncException, base.IsAsyncCanceled));
            _asyncException = null;
        }


        ////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Asynchronous event for FxpCopyAsync method.
        /// </summary>
        public event EventHandler<FxpCopyAsyncCompletedEventArgs> FxpCopyAsyncCompleted;

        /// <summary>
        /// Asynchronous File Exchange Protocol (FXP) allows server-to-server transfer which can greatly speed up file transfers.
        /// </summary>
        /// <param name="fileName">The name of the file to transfer.</param>
        /// <param name="destination">The destination FTP server which must be supplied as an open and connected FtpsClient object.</param>
        /// <remarks>
        /// Both servers must support and have FXP enabled before you can transfer files between two remote servers using FXP.  One FTP server must support PASV mode and the other server must allow PORT commands from a foreign address.  Finally, firewall settings may interfer with the ability of one server to access the other server.
        /// Starksoft FtpsClient will coordinate the FTP negoitaion and necessary PORT and PASV transfer commands.
        /// </remarks>
        /// <seealso cref="FxpCopyAsyncCompleted"/>
        /// <seealso cref="FxpTransferTimeout"/>
        /// <seealso cref="FxpCopy"/>
        /// <seealso cref="FtpsBase.CancelAsync"/>
        public void FxpCopyAsync(string fileName, FtpsClient destination)
        {
            if (base.AsyncWorker != null && base.AsyncWorker.IsBusy)
                throw new InvalidOperationException("The FtpsClient object is already busy executing another asynchronous operation.  You can only execute one asychronous method at a time.");

            base.CreateAsyncWorker();
            base.AsyncWorker.WorkerSupportsCancellation = true;
            base.AsyncWorker.DoWork += new DoWorkEventHandler(FxpCopyAsync_DoWork);
            base.AsyncWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(FxpCopyAsync_RunWorkerCompleted);
            Object[] args = new Object[2];
            args[0] = fileName;
            args[1] = destination;
            base.AsyncWorker.RunWorkerAsync(args);
        }

        private void FxpCopyAsync_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                Object[] args = (Object[])e.Argument;
                FxpCopy((string)args[0], (FtpsClient)args[1]);
            }
            catch (Exception ex)
            {
                _asyncException = ex;
            }
        }

        private void FxpCopyAsync_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (FxpCopyAsyncCompleted != null)
                FxpCopyAsyncCompleted(this, new FxpCopyAsyncCompletedEventArgs(_asyncException, base.IsAsyncCanceled));
            _asyncException = null;
        }

#endregion

#region Destructors

        /// <summary>
        /// Disposes all FtpsClient objects and connections.
        /// </summary>
        new protected virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose Method.
        /// </summary>
        /// <param name="disposing"></param>
        override protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                // close any managed objects
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Dispose deconstructor.
        /// </summary>
        ~FtpsClient()
        {
            Dispose(false);
        }

#endregion
    }
}




