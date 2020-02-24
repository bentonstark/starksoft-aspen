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

namespace Starksoft.Aspen.Ftps
{
    /// <summary>
    /// Event arguments to facilitate the FtpsClient request event.
    /// </summary>
	public class FtpsRequestEventArgs : EventArgs
	{
		private FtpsRequest _request;

        /// <summary>
        /// Constructor for FtpRequestEventArgs.
        /// </summary>
        /// <param name="request">An FtpRequest object.</param>
 		public FtpsRequestEventArgs(FtpsRequest request)
		{
            _request = request;
		}

        /// <summary>
        /// Client request command text sent from the client to the server.
        /// </summary>
		public FtpsRequest Request
		{
			get	{ return _request;	}
		}
	}

} 