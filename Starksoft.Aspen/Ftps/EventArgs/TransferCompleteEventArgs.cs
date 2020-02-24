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
    /// Event arguments to facilitate the transfer complete event.
    /// </summary>
    public class TransferCompleteEventArgs : EventArgs
    {

        private long _bytesTransferred;
        private int _bytesPerSecond;
        private TimeSpan _elapsedTime;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="bytesTransferred">The total number of bytes transferred.</param>
        /// <param name="bytesPerSecond">The data transfer speed in bytes per second.</param>
        /// <param name="elapsedTime">The time that has elapsed since the data transfer started.</param>
        public TransferCompleteEventArgs(long bytesTransferred, int bytesPerSecond, TimeSpan elapsedTime)
        {
            _bytesTransferred = bytesTransferred;
            _bytesPerSecond = bytesPerSecond;
            _elapsedTime = elapsedTime;
        }

        /// <summary>
        /// The total number of bytes transferred.
        /// </summary>
        public long BytesTransferred
        {
            get { return _bytesTransferred; }
        }

        /// <summary>
        /// Gets the data transfer speed in bytes per second.
        /// </summary>
        public int BytesPerSecond
        {
            get { return _bytesPerSecond; }
        }

        /// <summary>
        /// Gets the data transfer speed in kilobytes per second.
        /// </summary>
        public int KilobytesPerSecond
        {
            get { return _bytesPerSecond / 1024; }
        }

        /// <summary>
        /// Gets the time that has elapsed since the data transfer started.
        /// </summary>
        public TimeSpan ElapsedTime
        {
            get { return _elapsedTime; }
        }


    }
}
