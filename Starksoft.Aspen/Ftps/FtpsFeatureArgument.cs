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
    /// Ftp feature argument item class.
    /// </summary>
    public class FtpsFeatureArgument
    {
        private string _name;
        private bool _isDefault;

        /// <summary>
        /// Constructor to create a new ftp feature.
        /// </summary>
        /// <param name="name">Feature name.</param>
        public FtpsFeatureArgument(string name)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException("name", "must have a value");

            _name = name.Trim();
            Parse();
        }

        /// <summary>
        /// Feature name.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Default value indicator. 
        /// </summary>
        /// <remarks>Some feature arguments may be marked as a default argument by the FTP server.</remarks>
        public bool IsDefault
        {
            get { return _isDefault; }
        }

        private void Parse()
        {
            char[] a = _name.ToCharArray();
            int last = a.Length - 1;
            if (a[last] == '*')
            {
                _isDefault = true;
                _name = _name.Substring(0, last);
            }

        }

    }
}
