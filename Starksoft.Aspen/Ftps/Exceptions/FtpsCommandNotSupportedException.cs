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
using System.Runtime.Serialization;

namespace Starksoft.Aspen.Ftps
{

    /// <summary>
    /// This exception is thrown when attempting to executed a command this is not listed
    /// in the FTP server features.
    /// </summary>
    [Serializable()]
    public class FtpsCommandNotSupportedException : FtpsSecureConnectionException
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public FtpsCommandNotSupportedException()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Exception message text.</param>
        /// <param name="command">FTP command.</param>
        public FtpsCommandNotSupportedException(string message, string command)
            : base(String.Format("{0}  Command '{1}' not a supported feature.", message, command.ToUpper()))
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Exception message text.</param>
        /// <param name="command">Enumerated FTP command.</param>
        public FtpsCommandNotSupportedException(string message, FtpsCmd command)
            : this(message, command.ToString())
        {  
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Exception message text.</param>
        /// <param name="innerException">The inner exception object.</param>
        public FtpsCommandNotSupportedException(string message, Exception innerException)
            :
           base(message, innerException)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="info">Serialization information.</param>
        /// <param name="context">Stream context information.</param>
        protected FtpsCommandNotSupportedException(SerializationInfo info,
           StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Exception message text.</param>
        /// <param name="response">Ftp response object.</param>
        /// <param name="innerException">The inner exception object.</param>
        public FtpsCommandNotSupportedException(string message, FtpsResponse response, Exception innerException)
            :
           base(message, response, innerException)
        { }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Exception message text.</param>
        /// <param name="response">Ftp response object.</param>
        public FtpsCommandNotSupportedException(string message, FtpsResponse response)
            : base(message, response)
        { }

    }

}
