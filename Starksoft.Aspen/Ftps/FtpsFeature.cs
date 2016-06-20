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
    /// Ftp feature item class.
    /// </summary>
    public class FtpsFeature
    {
        private string _name;
        private FtpsFeatureArgumentCollection _argList;
        

        /// <summary>
        /// Constructor to create a new ftp feature.
        /// </summary>
        /// <param name="name">Feature name (required).</param>
        /// <param name="arguments">Argument list (can empty).</param>
        public FtpsFeature(string name, string arguments)
		{
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException("name", "must have a value");
            if (arguments == null)
                throw new ArgumentNullException("arguments", "cannot be null");
            _name = name;
            if (arguments.Length != 0)
                _argList = new FtpsFeatureArgumentCollection(arguments);
            else
                _argList = new FtpsFeatureArgumentCollection();
		}

        /// <summary>
        /// Feature name.
        /// </summary>
		public string Name
		{
			get	{ return _name;	}
		}

        /// <summary>
        /// Get the FTP feature arguments collection.
        /// </summary>
        public FtpsFeatureArgumentCollection Arguments
        {
            get { return _argList; }
        }

    }
}
