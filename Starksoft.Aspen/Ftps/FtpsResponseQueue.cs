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


namespace Starksoft.Aspen.Ftps
{
    /// <summary>
    /// Thread safe FtpResponse queue object.
    /// </summary>
    internal class FtpsResponseQueue
    {
        private Queue<FtpsResponse> _queue = new Queue<FtpsResponse>(10);
        
        /// <summary>
        /// Gets the number of elements contained in the FtpResponseQueue.
        /// </summary>
        public int Count 
        {
            get 
            {
                lock (this) 
                { 
                    return _queue.Count; 
                } 
            } 
        }





        /// <summary>
        /// Adds an Response object to the end of the FtpResponseQueue.
        /// </summary>
        /// <param name="item">An FtpResponse item.</param>
        public void Enqueue(FtpsResponse item)
        {
            lock (this)
            {
                _queue.Enqueue(item);
            }
        }

        /// <summary>
        /// Removes and returns the FtpResponse object at the beginning of the FtpResponseQueue.
        /// </summary>
        /// <returns>FtpResponse object at the beginning of the FtpResponseQueue</returns>
        public FtpsResponse Dequeue()
        {
            lock (this)
            {
                return _queue.Dequeue();
            }
        }





    }
}
