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
    public class FtpsFeatureArgumentCollection : IEnumerable<FtpsFeatureArgument>
    {
        private List<FtpsFeatureArgument> _list = new List<FtpsFeatureArgument>();
        private string _text;

        /// <summary>
        /// Default constructor for no feature arguments.
        /// </summary>
        public FtpsFeatureArgumentCollection()
        {

        }

        /// <summary>
        /// Default constructor with features.
        /// </summary>
        /// <param name="text">Raw feature list text.</param>
        public FtpsFeatureArgumentCollection(string text)
        {
            if (String.IsNullOrEmpty(text))
                throw new ArgumentNullException("text");

            _text = text;
            Parse();
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the
        /// first occurrence within the entire FtpFeatureArgumentCollection list.
        /// </summary>
        /// <param name="item">The FtpFeatureArgument object to locate in the collection.</param>
        /// <returns>The zero-based index of the first occurrence of item within the entire if found; otherwise, -1.</returns>
        public int IndexOf(FtpsFeatureArgument item)
        {
            return _list.IndexOf(item);
        }

        /// <summary>
        /// Adds an FtpFeatureArgument to the end of the FtpFeatureArgumentCollection list.
        /// </summary>
        /// <param name="item">FtpFeatureArgument object to add.</param>
        public void Add(FtpsFeatureArgument item)
        {
            _list.Add(item);
        }

        /// <summary>
        ///  Gets the number of elements actually contained in the FtpFeatureArgumentCollection list.
        /// </summary>
        public int Count
        {
            get { return _list.Count; }
        }

        IEnumerator<FtpsFeatureArgument> IEnumerable<FtpsFeatureArgument>.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        /// <summary>
        /// Gets an FtpFeatureArgument from the FtpFeatureArgumentCollection list based on index value.
        /// </summary>
        /// <param name="index">Numeric index of item to retrieve.</param>
        /// <returns>FtpFeatureArgument object.</returns>
        public FtpsFeatureArgument this[int index]
        {
            get { return _list[index]; }
        }

        /// <summary>
        /// Gets an FtpFeature from the FtpFeatureCollection list based on name.
        /// </summary>
        /// <param name="name">Name of the feature.</param>
        /// <returns>FtpFeature object.</returns>
        public FtpsFeatureArgument this[string name]
        {
            get { return Find(name); }
        }

        /// <summary>
        /// Remove all elements from the FtpFeatureArgumentCollection list.
        /// </summary>
        public void Clear()
        {
            _list.Clear();
        }

        /// <summary>
        /// Get the raw FTP server supplied reponse text for features.
        /// </summary>
        /// <returns>A string containing the FTP feature list.</returns>
        public string GetRawText()
        {
            return _text;
        }

        /// <summary>
        /// Linearly searches for the specified object based on the feature 'name' parameter value
        /// and returns the corresponding object with the name is found; otherwise null.  Search is case insensitive.
        /// </summary>
        /// <param name="name">The name of the FtpFeatureArgument to locate in the collection.</param>
        /// <returns>FtpFeatureArgument object if the name if found; otherwise null.</returns>
        public FtpsFeatureArgument Find(string name)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException("name", "must have a value");

            foreach (FtpsFeatureArgument item in _list)
            {
                if (String.Compare(name, item.Name, true) == 0)
                    return item;
            }

            return null;
        }

        /// <summary>
        /// Linearly searches for the specified object based on the feature 'name' parameter value
        /// and returns true if an object with the name is found; otherwise false.  Search is case insensitive.
        /// </summary>
        /// <remarks>
        /// example:  col.Contains("UTF8");
        /// </remarks>
        /// <param name="name">The name of the FtpFeatureArgument to locate in the collection.</param>
        /// <returns>True if the name if found; otherwise false.</returns>
        public bool Contains(string name)
        {
            return Find(name) != null ? true : false;
        }

        private void Parse()
        {
            string[] args = GetArgumentArray();
            foreach (string arg in args)
            {
                _list.Add(new FtpsFeatureArgument(arg));
            }
        }

        /// <summary>
        /// Gets the FTP feature arguments as a string array.
        /// </summary>
        /// <returns>Array of strings containing arguments; otherwise null.</returns>
        public string[] GetArgumentArray()
        {
            if (String.IsNullOrEmpty(_text))
                return null;
            else
                if (_text.Contains(";"))
                    return SplitSemi(_text);
                else
                    if (_text.Contains(" "))
                        return SplitSpace(_text);
                    else
                    {
                        string[] a = new string[1];
                        a[0] = _text;
                        return a;
                    }
        }

        private string[] SplitSemi(string list)
        {
            return list.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private string[] SplitSpace(string list)
        {
            return list.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }



    }
}