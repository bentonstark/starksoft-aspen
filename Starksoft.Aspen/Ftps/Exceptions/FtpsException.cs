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
    /// This exception is thrown when a general FTP exception occurs.
    /// </summary>
    [Serializable()]
    public class FtpsException : Exception
    {
        private FtpsResponse _response = new FtpsResponse();
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public FtpsException()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Exception message text.</param>
        public FtpsException(string message)
            : base(message)
        {
        }
 

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Exception message text.</param>
        /// <param name="innerException">The inner exception object.</param>
        public FtpsException(string message, Exception innerException)
            :
           base(message, innerException)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Exception message text.</param>
        /// <param name="response">Response object.</param>
        /// <param name="innerException">The inner exception object.</param>
        public FtpsException(string message, FtpsResponse response, Exception innerException)
            :
           base(message, innerException)
        {
            _response = response;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Exception message text.</param>
        /// <param name="response">Ftp response object.</param>
        public FtpsException(string message, FtpsResponse response)
            : base(message)
        {
            _response = response;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="info">Serialization information.</param>
        /// <param name="context">Stream context information.</param>
        protected FtpsException(SerializationInfo info,
           StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Gets the last FTP response if one is available.
        /// </summary>
        public FtpsResponse LastResponse
        {
            get { return _response; }
        }


        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        public override string Message
        {
            get
            {
                if (_response.Code == FtpsResponseCode.None)
                    return base.Message;
                else
                    return String.Format("{0}  (Last Server Response: {1}  {2})", base.Message, _response.Text, _response.Code); ;
            }
        }
    }

}
