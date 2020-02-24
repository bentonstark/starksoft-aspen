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
using System.Collections;
using System.Collections.Generic;
using System.Text;


namespace Starksoft.Aspen.Ftps
{
    /// <summary>
    /// Ftp response collection.
    /// </summary>
    public class FtpsResponseCollection : IEnumerable<FtpsResponse>
    {
        private List<FtpsResponse> _list = new List<FtpsResponse>();

        /// <summary>
        /// Default constructor.
        /// </summary>
        public FtpsResponseCollection()
        { }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the
        /// first occurrence within the entire FtpResponseCollection list.
        /// </summary>
        /// <param name="item">The FtpResponse object to locate in the collection.</param>
        /// <returns>The zero-based index of the first occurrence of item within the entire if found; otherwise, -1.</returns>
        public int IndexOf(FtpsResponse item)
        {
            return _list.IndexOf(item);
        }



        /// <summary>
        /// Adds an FtpResponse to the end of the FtpResponseCollection list.
        /// </summary>
        /// <param name="item">FtpResponse object to add.</param>
        public void Add(FtpsResponse item)
        {
            _list.Add(item);
        }

        /// <summary>
        ///  Gets the number of elements actually contained in the FtpResponseCollection list.
        /// </summary>
        public int Count
        {
            get { return _list.Count; }
        }

        IEnumerator<FtpsResponse> IEnumerable<FtpsResponse>.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        /// <summary>
        /// Gets an FtpResponse from the FtpResponseCollection list based on index value.
        /// </summary>
        /// <param name="index">Numeric index of item to retrieve.</param>
        /// <returns>FtpResponse object.</returns>
        public FtpsResponse this[int index]
        {
            get { return _list[index]; }
        }

        /// <summary>
        /// Remove all elements from the FtpResponseCollection list.
        /// </summary>
        public void Clear()
        {
            _list.Clear();
        }

        /// <summary>
        /// Get the raw FTP server supplied reponse text.
        /// </summary>
        /// <returns>A string containing the FTP server response.</returns>
        public string GetRawText()
        {
            StringBuilder builder = new StringBuilder();
            foreach(FtpsResponse item in _list)
            {
                builder.Append(item.RawText);
                builder.Append("\r\n");
            }
            return builder.ToString();
        }

        /// <summary>
        /// Get the last server response from the FtpResponseCollection list.
        /// </summary>
        /// <returns>FtpResponse object.</returns>
        public FtpsResponse GetLast()
        {
            if (_list.Count == 0)
                return new FtpsResponse();
            else
                return _list[_list.Count - 1];
        }
    }
}