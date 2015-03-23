/*
* Copyright (c) 2015 Benton Stark
* This file is part of the Starksoft Aspen library.
*
* Starksoft Aspen is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
* 
* Starksoft Aspen is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
* 
* You should have received a copy of the GNU General Public License
* along with Starksoft Aspen.  If not, see <http://www.gnu.org/licenses/>.
*   
*/

using System;

namespace Starksoft.Aspen.Ftps
{
    /// <summary>
    /// Event arguments to facilitate the response event from the FTP server.
    /// </summary>
    public class ConnectionClosedEventArgs : EventArgs
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ConnectionClosedEventArgs()
        {
        }
    }
}