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
using System.Globalization;

namespace Starksoft.Aspen.Ftps
{
    /// <summary>
    /// FTP server commands.
    /// </summary>
    public enum FtpsCmd
    {
        /// <summary>
        /// Unknown command issued.
        /// </summary>
        Unknown,
        /// <summary>
        /// The USER command.
        /// </summary>
        User,
        /// <summary>
        /// The PASS command.
        /// </summary>
        Pass,
        /// <summary>
        /// The MKD command.  Make new directory.
        /// </summary>
        Mkd,
        /// <summary>
        /// The RMD command.  Remove directory.
        /// </summary>
        Rmd,
        /// <summary>
        /// The RETR command.  Retrieve file.
        /// </summary>
        Retr,
        /// <summary>
        /// The PWD command.  Print working directory.
        /// </summary>
        Pwd,
        /// <summary>
        /// The SYST command.  System status.
        /// </summary>
        Syst,
        /// <summary>
        /// The CDUP command.  Change directory up.
        /// </summary>
        Cdup,
        /// <summary>
        /// The DELE command.  Delete file or directory.
        /// </summary>
        Dele,
        /// <summary>
        /// The TYPE command.  Transfer type.
        /// </summary>
        Type,
        /// <summary>
        /// The CWD command.  Change working directory.
        /// </summary>
        Cwd,
        /// <summary>
        /// The PORT command.  Data port.
        /// </summary>
        Port,
        /// <summary>
        /// The PASV command.  Passive port.
        /// </summary>
        Pasv,
        /// <summary>
        /// The STOR command.  Store file.
        /// </summary>
        Stor,
        /// <summary>
        /// The STOU command.  Store file unique.
        /// </summary>
        Stou,
        /// <summary>
        /// The APPE command.  Append file.
        /// </summary>
        Appe,
        /// <summary>
        /// The RNFR command.  Rename file from.
        /// </summary>
        Rnfr,
        /// <summary>
        /// The RFTO command.  Rename file to.
        /// </summary>
        Rnto,
        /// <summary>
        /// The ABOR command.  Abort current operation.
        /// </summary>
        Abor,
        /// <summary>
        /// The LIST command.  List files.
        /// </summary>
        List,
        /// <summary>
        /// The NLST command.  Namelist files.
        /// </summary>
        Nlst,
        /// <summary>
        /// The SITE command.  Site.
        /// </summary>
        Site,
        /// <summary>
        /// The STAT command.  Status.
        /// </summary>
        Stat,
        /// <summary> 
        /// The NOOP command.  No operation.
        /// </summary>
        Noop,
        /// <summary>
        /// The HELP command.  Help.
        /// </summary>
        Help,
        /// <summary>
        /// The ALLO command.  Allocate space.
        /// </summary>
        Allo,
        /// <summary>
        /// The QUIT command.  Quite session.
        /// </summary>
        Quit,
        /// <summary>
        /// The REST command.  Restart transfer.
        /// </summary>
        Rest,
        /// <summary>
        /// The AUTH command.  Initialize authentication.
        /// </summary>
        Auth,
        /// <summary>
        /// The PBSZ command.
        /// </summary>
        Pbsz,
        /// <summary>
        /// The PROT command.  Security protocol.
        /// </summary>
        Prot,
        /// <summary>
        /// The MODE command.  Data transfer mode.
        /// </summary>
        Mode,
        /// <summary>
        /// The MDTM command.  Month Day Time command.
        /// </summary>
        Mdtm,
        /// <summary>
        /// The SIZE command.  File size.
        /// </summary>
        /// <remarks>
        /// This command retrieves the size of the file as stored on the FTP server.  Not all FTP servers
        /// support this command.
        /// </remarks>
        Size,
        /// <summary>
        /// The FEAT command.  Supported features.
        /// </summary>
        /// <remarks>
        /// This command gets a list of supported features from the FTP server.  The feature list may contain
        /// extended commands in addition to proprietrary commands that are not defined in RFC documents.
        /// </remarks>
        Feat,
        /// <summary>
        /// The XCRC command.  CRC file accuracy testing.
        /// </summary>
        /// <remarks>
        /// This is a non-standard command not supported by all FTP servers and not defined in RFC documents.
        /// </remarks>
        Xcrc,
        /// <summary>
        /// The XMD5 command.  MD5 file integrity hashing.
        /// </summary>
        /// <remarks>
        /// This is a non-standard command not supported by all FTP servers and not defined in RFC documents.
        /// </remarks>
        Xmd5,
        /// <summary>
        /// The XSHA1 command.  SHA1 file integerity hashing.
        /// </summary>
        /// <remarks>
        /// This is a non-standard command not supported by all FTP servers and not defined in RFC documents.
        /// </remarks>
        Xsha1,
        /// <summary>
        /// The XSHA256 command.  SHA-256 file integerity hashing.
        /// </summary>
        /// <remarks>
        /// This is a non-standard command not supported by all FTP servers and not defined in RFC documents.
        /// </remarks>
        Xsha256,
        /// <summary>
        /// The XSHA512 command.  SHA-512 file integerity hashing.
        /// </summary>
        /// <remarks>
        /// This is a non-standard command not supported by all FTP servers and not defined in RFC documents.
        /// </remarks>
        Xsha512,
        /// <summary>
        /// The EPSV command.  Extended passive command.
        /// </summary>
        /// <remarks>
        /// The EPSV command is an extended PASV command that supports both IPv4 and IPv6 network addressing and is defined
        /// in the RFC 2428 document.
        /// </remarks>
        Epsv,
        /// <summary>
        /// The EPRT command.  Extended port command.
        /// </summary>
        /// <remarks>
        /// The EPRT command is an extended PORT command that supports both IPv4 and IPv6 network addressing and is defined
        /// in the RFC 2428 document.
        /// </remarks>
        Eprt,
        /// <summary>
        /// The MFMT command. Modify File Modification Time command.
        /// </summary>
        Mfmt,
        /// <summary>
        /// The MFCT command. Modify File Creation Time command.
        /// </summary>
        Mfct,
        /// <summary>
        /// The OPTS command.  This command allows an FTP client to define a parameter that will be used by a subsequent command. 
        /// </summary>
        Opts,
        /// <summary>
        /// The HASH command.  This command is not supported by all FTP servers.
        /// </summary>
        /// <remarks>
        /// This command is in the RFC draft phase and is used to generate file hashes on FTP server.  
        /// More information can be found searching the document named
        /// "File Transfer Protocol HASH Command for Cryptographic Hashes" draft-ietf-ftpext2-hash-03.
        /// </remarks>
        Hash,
        /// <summary>
        /// The RANG command.  This command is not supported by all FTP servers.
        /// </summary>
        /// <remarks>
        /// This command is in the RFC draft phase and is used specify a byte range for partical file hashes.  
        /// More information can be found searching the document named
        /// "File Transfer Protocol HASH Command for Cryptographic Hashes" draft-ietf-ftpext2-hash-03.
        /// </remarks>
        Rang,
        /// <summary>
        /// The CLNT command.  This command is not supported by all FTP servers.
        /// </summary>
        /// <remarks>
        /// The CLieNT command is a command whereby the FTP client can identify itself to the FTP server.
        /// Many FTP servers ignore this command.  The ServU FTP server requires this command to be sent
        /// prior to other important commands. 
        /// </remarks>
        Clnt,
        /// <summary>
        /// The MLST command.  This command is not supported by all FTP servers.
        /// </summary>
        /// <remarks>
        /// The MLST and MLSD commands are intended to standardize the file and
        /// directory information returned by the server-FTP process.  These
        /// commands differ from the LIST command in that the format of the
        /// replies is strictly defined although extensible.
        /// </remarks>
        Mlst,
        /// <summary>
        /// The MLSD command.  This command is not supported by all FTP servers.
        /// </summary>
        /// <remarks>
        /// The MLST and MLSD commands are intended to standardize the file and
        /// directory information returned by the server-FTP process.  These
        /// commands differ from the LIST command in that the format of the
        /// replies is strictly defined although extensible.
        /// </remarks>
        Mlsd
    }
 
    /// <summary>
    /// FTP request object which contains the command, arguments and text or an FTP request.
    /// </summary>
    public class FtpsRequest
    {
        private FtpsCmd _command;
        private string[] _arguments;
        private string _text;
        private Encoding _encoding;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public FtpsRequest()
        {
            _encoding = Encoding.UTF8;
            _command = new FtpsCmd();
            _text = string.Empty;
        }

        /// <summary>
        /// FTP request constructor.
        /// </summary>
        /// <param name="encoding">Text encoding object to use.</param>
        /// <param name="command">FTP request command.</param>
        /// <param name="arguments">Parameters for the request</param>
        internal FtpsRequest(Encoding encoding, FtpsCmd command, params string[] arguments)
        {
            _encoding = encoding;
            _command = command;
            _arguments = arguments;
            _text = BuildCommandText();
        }

        /// <summary>
        /// FTP request constructor.
        /// </summary>
        /// <param name="encoding">Text encoding object to use.</param>
        /// <param name="command">FTP request command.</param>
        internal FtpsRequest(Encoding encoding, FtpsCmd command) : this(encoding, command, null)
        { }

        /// <summary>
        /// Get the FTP command enumeration value.
        /// </summary>
        public FtpsCmd Command
        {
            get { return _command; }
        }

        /// <summary>
        /// Get the FTP command arguments (if any).
        /// </summary>
        public List<string> Arguments
        {
            get 
            {  
                return new List<string>(_arguments); 
            }
        }

        /// <summary>
        /// Get the FTP command text with any arguments.
        /// </summary>
        public string Text
        {
            get { return _text; }
        }

        /// <summary>
        /// Gets a boolean value indicating if the command is a file transfer or not.
        /// </summary>
        public bool IsFileTransfer
        {
            get
            {
                return ((_command == FtpsCmd.Retr)
                  || (_command == FtpsCmd.Stor)
                  || (_command == FtpsCmd.Stou)
                  || (_command == FtpsCmd.Appe)
                  );
            }
        }

        internal string BuildCommandText()
        {
            string commandText = _command.ToString().ToUpper(CultureInfo.InvariantCulture);

            if (_arguments == null)
            {
                return commandText;
            }
            else
            {
                StringBuilder builder = new StringBuilder();
                foreach (string arg in _arguments)
                {
                    builder.Append(arg);
                    builder.Append(" ");
                }
                string argText = builder.ToString().TrimEnd();

                if (_command == FtpsCmd.Unknown)
                    return argText;
                else
                    return String.Format("{0} {1}", commandText, argText).TrimEnd();
            }
        }

        internal byte[] GetBytes()
        {
            return _encoding.GetBytes(String.Format("{0}\r\n", _text));
        }

        internal bool HasHappyCodes
        {
            get { return GetHappyCodes().Length == 0 ? false : true; }
        }


        internal FtpsResponseCode[]  GetHappyCodes()
        {
            switch(_command)
            {
                case FtpsCmd.Unknown:
                    return BuildResponseArray();
                case FtpsCmd.Allo:
                    return BuildResponseArray(FtpsResponseCode.CommandOkay, FtpsResponseCode.CommandNotImplementedSuperfluousAtThisSite);
                case FtpsCmd.User:
                    return BuildResponseArray(FtpsResponseCode.UserNameOkayButNeedPassword, FtpsResponseCode.ServiceReadyForNewUser, FtpsResponseCode.UserLoggedIn);
                case FtpsCmd.Pass:
                    return BuildResponseArray(FtpsResponseCode.UserLoggedIn, FtpsResponseCode.ServiceReadyForNewUser, FtpsResponseCode.NotLoggedIn);
                case FtpsCmd.Cwd:
                    return BuildResponseArray(FtpsResponseCode.RequestedFileActionOkayAndCompleted);
                case FtpsCmd.Pwd:
                    return BuildResponseArray(FtpsResponseCode.PathNameCreated);
                case FtpsCmd.Dele:
                    return BuildResponseArray(FtpsResponseCode.RequestedFileActionOkayAndCompleted);
                case FtpsCmd.Abor:
                    return BuildResponseArray();
                case FtpsCmd.Mkd:
                    return BuildResponseArray(FtpsResponseCode.PathNameCreated);
                case FtpsCmd.Rmd:
                    return BuildResponseArray(FtpsResponseCode.RequestedFileActionOkayAndCompleted);
                case FtpsCmd.Help:
                    return BuildResponseArray(FtpsResponseCode.SystemStatusOrHelpReply, FtpsResponseCode.HelpMessage, FtpsResponseCode.FileStatus);
                case FtpsCmd.Mdtm:
                    return BuildResponseArray(FtpsResponseCode.FileStatus, FtpsResponseCode.RequestedFileActionOkayAndCompleted);
                case FtpsCmd.Stat:
                    return BuildResponseArray(FtpsResponseCode.SystemStatusOrHelpReply, FtpsResponseCode.DirectoryStatus, FtpsResponseCode.FileStatus);
                case FtpsCmd.Cdup:
                    return BuildResponseArray(FtpsResponseCode.CommandOkay, FtpsResponseCode.RequestedFileActionOkayAndCompleted);
                case FtpsCmd.Size:
                    return BuildResponseArray(FtpsResponseCode.FileStatus);
                case FtpsCmd.Feat:
                    return BuildResponseArray(FtpsResponseCode.SystemStatusOrHelpReply);
                case FtpsCmd.Syst:
                    return BuildResponseArray(FtpsResponseCode.NameSystemType);
                case FtpsCmd.Rnfr:
                    return BuildResponseArray(FtpsResponseCode.RequestedFileActionPendingFurtherInformation);
                case FtpsCmd.Rnto:
                    return BuildResponseArray(FtpsResponseCode.RequestedFileActionOkayAndCompleted);
                case FtpsCmd.Noop:
                    return BuildResponseArray(FtpsResponseCode.CommandOkay);                
                case FtpsCmd.Site:
                    return BuildResponseArray(FtpsResponseCode.CommandOkay, FtpsResponseCode.CommandNotImplementedSuperfluousAtThisSite, FtpsResponseCode.RequestedFileActionOkayAndCompleted);
                case FtpsCmd.Pasv:
                    return BuildResponseArray(FtpsResponseCode.EnteringPassiveMode);
                case FtpsCmd.Port:
                    return BuildResponseArray(FtpsResponseCode.CommandOkay);
                case FtpsCmd.Type:
                    return BuildResponseArray(FtpsResponseCode.CommandOkay);
                case FtpsCmd.Rest:
                    return BuildResponseArray(FtpsResponseCode.RequestedFileActionPendingFurtherInformation);
                case FtpsCmd.Mode:
                    return BuildResponseArray(FtpsResponseCode.CommandOkay);
                case FtpsCmd.Quit:
                    return BuildResponseArray();
                case FtpsCmd.Auth:
                    return BuildResponseArray(FtpsResponseCode.AuthenticationCommandOkay, FtpsResponseCode.AuthenticationCommandOkaySecurityDataOptional);
                case FtpsCmd.Pbsz:
                    return BuildResponseArray(FtpsResponseCode.CommandOkay);
                case FtpsCmd.Prot:
                    return BuildResponseArray(FtpsResponseCode.CommandOkay);
                case FtpsCmd.List:
                case FtpsCmd.Nlst:
                case FtpsCmd.Mlsd:
                    return BuildResponseArray(FtpsResponseCode.DataConnectionAlreadyOpenSoTransferStarting,
                                FtpsResponseCode.FileStatusOkaySoAboutToOpenDataConnection,
                                FtpsResponseCode.ClosingDataConnection,
                                FtpsResponseCode.RequestedFileActionOkayAndCompleted);
                case FtpsCmd.Appe:
                case FtpsCmd.Stor:
                case FtpsCmd.Stou:
                case FtpsCmd.Retr:
                    return BuildResponseArray(FtpsResponseCode.DataConnectionAlreadyOpenSoTransferStarting, 
                                FtpsResponseCode.FileStatusOkaySoAboutToOpenDataConnection, 
                                FtpsResponseCode.ClosingDataConnection,
                                FtpsResponseCode.RequestedFileActionOkayAndCompleted);
                case FtpsCmd.Mlst:
                    return BuildResponseArray(FtpsResponseCode.RequestedFileActionOkayAndCompleted);
                case FtpsCmd.Xcrc:
                case FtpsCmd.Xmd5:
                case FtpsCmd.Xsha1:
                case FtpsCmd.Xsha256:
                case FtpsCmd.Xsha512:
                    return BuildResponseArray(FtpsResponseCode.RequestedFileActionOkayAndCompleted);
                case FtpsCmd.Epsv:
                    return BuildResponseArray();
                case FtpsCmd.Eprt:
                    return BuildResponseArray();
                case FtpsCmd.Mfmt:
                    return BuildResponseArray(FtpsResponseCode.FileStatus);
                case FtpsCmd.Mfct:
                    return BuildResponseArray(FtpsResponseCode.FileStatus);
                case FtpsCmd.Opts:
				return BuildResponseArray(FtpsResponseCode.CommandOkay, FtpsResponseCode.CommandNotImplementedSuperfluousAtThisSite);
                case FtpsCmd.Hash:
                    return BuildResponseArray(FtpsResponseCode.FileStatus);
                case FtpsCmd.Clnt:
                    return BuildResponseArray(FtpsResponseCode.CommandOkay);
                default:
                    throw new FtpsException(String.Format("No response code(s) defined for FtpCmd {0}.", _command.ToString()));
            }
        }
        
        private FtpsResponseCode[] BuildResponseArray(params FtpsResponseCode[] codes)
        {
            return codes;
        }
    
    }
}
