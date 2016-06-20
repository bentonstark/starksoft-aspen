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
    /// This exception is thrown when a file integrity check fails.
    /// For detailed information about the error, the FTP server response 
    /// can be inspected via the Reponse property on this exception.
    /// </summary>
    [Serializable()]
    public class FtpsResponseException : FtpsException
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public FtpsResponseException()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Exception message text.</param>
        /// <param name="response">Ftp response object.</param>
        public FtpsResponseException(string message, FtpsResponse response)
            : base(message, response)
        {  }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="response">Ftp response object.</param>
        /// <param name="message">Exception message text.</param>
        /// <param name="innerException">The inner exception object.</param>
        public FtpsResponseException(string message, FtpsResponse response, Exception innerException)
            :
           base(message, response, innerException)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="info">Serialization information.</param>
        /// <param name="context">Stream context information.</param>
        protected FtpsResponseException(SerializationInfo info,
           StreamingContext context)
            : base(info, context)
        {
        }

    }

}
