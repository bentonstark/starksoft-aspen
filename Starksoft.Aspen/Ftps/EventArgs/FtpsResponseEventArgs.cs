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
	/// Ftps response event arguments.
	/// </summary>
	public class FtpsResponseEventArgs : EventArgs
	{
        private FtpsResponse _response;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="response">FtpResponse object.</param>
        public FtpsResponseEventArgs(FtpsResponse response)
		{
			_response = response;
		}

        /// <summary>
        /// Response object containing response received from the server.
        /// </summary>
		public FtpsResponse Response
		{
			get	{ return _response;	}
		}
	}
} 