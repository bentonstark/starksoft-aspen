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

namespace Starksoft.Aspen.GnuPG
{
    /// <summary>
    /// This exception is thrown when a bad passphrase is given resulting in an error condition when running the GPG.EXE program.   
    /// </summary>
    [Serializable()]
    public class GpgBadPassphraseException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public GpgBadPassphraseException()
        { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Exception message text.</param>
        public GpgBadPassphraseException(string message)
            : base(message)
        { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Exception message text.</param>
        /// <param name="innerException">The inner exception object.</param>
        public GpgBadPassphraseException(string message, Exception innerException)
            : base(message, innerException)
        { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="info">Serialization information.</param>
        /// <param name="context">Stream context information.</param>
        protected GpgBadPassphraseException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        { }
    }  

}

