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
    public class FtpsFeatureCollection : IEnumerable<FtpsFeature>
    {
        private List<FtpsFeature> _list = new List<FtpsFeature>();
        private string _text;

        private const int FEAT_PREAMBLE_LEN = 4;
        private const int FEAT_EPILOGUE_LEN = 7;
        private const string FEAT_PREAMBLE = "211-";
        private const string NO_FEAT_PREAMBLE = "211 ";
        private const string FEAT_EPILOGUE = "211 END";


        /// <summary>
        /// Default constructor for no features.
        /// </summary>
        public FtpsFeatureCollection()
        {
            _text = NO_FEAT_PREAMBLE;
        }

        /// <summary>
        /// Default constructor with features.
        /// </summary>
        /// <param name="text">Raw feature list text.</param>
        public FtpsFeatureCollection(string text)
        {
            if (String.IsNullOrEmpty(text))
                throw new FtpsFeatureException("text");
            
            if (text.Length < FEAT_PREAMBLE_LEN)
                throw new FtpsFeatureException("text argument length too short");

            _text = text;
            Parse();
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the
        /// first occurrence within the entire FtpFeatureCollection list.
        /// </summary>
        /// <param name="item">The FtpFeature object to locate in the collection.</param>
        /// <returns>The zero-based index of the first occurrence of item within the entire if found; otherwise, -1.</returns>
        public int IndexOf(FtpsFeature item)
        {
            return _list.IndexOf(item);
        }
        
        /// <summary>
        /// Adds an FtpFeature to the end of the FtpFeatureCollection list.
        /// </summary>
        /// <param name="item">FtpFeature object to add.</param>
        public void Add(FtpsFeature item)
        {
            _list.Add(item);
        }

        /// <summary>
        ///  Gets the number of elements actually contained in the FtpFeatureCollection list.
        /// </summary>
        public int Count
        {
            get { return _list.Count; }
        }

        IEnumerator<FtpsFeature> IEnumerable<FtpsFeature>.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        /// <summary>
        /// Gets an FtpFeature from the FtpFeatureCollection list based on index value.
        /// </summary>
        /// <param name="index">Numeric index of item to retrieve.</param>
        /// <returns>FtpFeature object.</returns>
        public FtpsFeature this[int index]
        {
            get { return _list[index]; }
        }

        /// <summary>
        /// Gets an FtpFeature from the FtpFeatureCollection list based on name.
        /// </summary>
        /// <param name="name">Name of the feature.</param>
        /// <returns>FtpFeature object.</returns>
        public FtpsFeature this[string name]
        {
            get { return Find(name); }
        }

        /// <summary>
        /// Gets an FtpFeature from the FtpFeatureCollection list based on name.
        /// </summary>
        /// <param name="name">Name of the feature.</param>
        /// <returns>FtpFeature object.</returns>
        public FtpsFeature this[FtpsCmd name]
        {
            get { return Find(name); }
        }


        /// <summary>
        /// Remove all elements from the FtpFeatureCollection list.
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
        /// <remarks>
        /// example:  col.Find("UTF8");
        /// </remarks>
        /// <param name="name">The name of the FtpFeature to locate in the collection.</param>
        /// <returns>FtpFeature object if the name if found; otherwise null.</returns>
        public FtpsFeature Find(string name)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException("name", "must have a value");

            foreach (FtpsFeature item in _list)
            {
                if (String.Compare(name, item.Name, true) == 0)
                    return item;
            }

            return null;
        }

        /// <summary>
        /// Linearly searches for the specified object based on the feature 'name' parameter value
        /// and returns the corresponding object with the name is found; otherwise null.  Search is case insensitive.
        /// </summary>
        /// <param name="name">The name of the FtpFeature to locate in the collection.</param>
        /// <returns>FtpFeature object if the name if found; otherwise null.</returns>
        public FtpsFeature Find(FtpsCmd name)
        {
            return Find(name.ToString());
        }

        /// <summary>
        /// Linearly searches for the specified object based on the feature 'name' parameter value
        /// and returns true if an object with the name is found; otherwise false.  Search is case insensitive.
        /// </summary>
        /// <remarks>
        /// example:  col.Contains("UTF8");
        /// </remarks>
        /// <param name="name">The name of the FtpFeature to locate in the collection.</param>
        /// <returns>True if the name if found; otherwise false.</returns>
        public bool Contains(string name)
        {
            return Find(name) != null ?  true : false;
        }

        /// <summary>
        /// Linearly searches for the specified object based on the feature 'name' and 'argument' parameter values
        /// and returns true if an object with the name is found; otherwise false.  Search is case insensitive.
        /// </summary>
        /// <remarks>
        /// examples:  col.Contains("REST", "STREAM");
        ///            col.Contains(FtpCmd.Hash, "SHA-1");
        /// </remarks>
        /// <param name="name">The name of the FtpFeature to locate in the collection.</param>
        /// <param name="argument">The argument for the FtpFeature to locate in the collection.</param>
        /// <returns>True if the name if found; otherwise false.</returns>
        public bool Contains(string name, string argument)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException("name", "must have a value");

            if (String.IsNullOrEmpty(argument))
                throw new ArgumentNullException("argument", "must have a value");

            FtpsFeature f = Find(name);

            if (f == null)
            {
                return false;
            }
            else
            {
                foreach (FtpsFeatureArgument arg in f.Arguments)
                {
                    if (String.Compare(argument, arg.Name, true) == 0)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Linearly searches for the specified object based on the feature 'name' parameter value
        /// and returns true if an object with the name is found; otherwise false.  Search is case insensitive.
        /// </summary>
        /// <param name="name">The name of the FtpFeature to locate in the collection.</param>
        /// <returns>True if the name if found; otherwise false.</returns>
        public bool Contains(FtpsCmd name)
        {
            return Contains(name.ToString());
        }

        /// <summary>
        /// Linearly searches for the specified object based on the feature 'name' parameter value
        /// and returns true if an object with the name is found; otherwise false.  Search is case insensitive.
        /// </summary>
        /// <param name="name">The name of the FtpFeature to locate in the collection.</param>
        /// <param name="arguments">The argument for the FtpFeature to locate in the collection.</param>
        /// <returns>True if the name if found; otherwise false.</returns>
        public bool Contains(FtpsCmd name, string arguments)
        {
            return Contains(name.ToString(), arguments);
        }

        /// <summary>
        /// Parses the raw features text into objects and loads them into the collection.
        /// </summary>
        /// <remarks>
        /// http://tools.ietf.org/html/rfc2389#section-3
        /// 
        /// Note that each feature line in the feature-listing begins with a
        /// single space.  That space is not optional, nor does it indicate
        /// general white space.  This space guarantees that the feature line can
        /// never be misinterpreted as the end of the feature-listing, but is
        /// required even where there is no possibility of ambiguity.
        /// 
        /// Syntax:
        /// feat-response   = error-response / no-features / feature-listing
        ///         no-features     = "211" SP *TCHAR CRLF
        ///         feature-listing = "211-" *TCHAR CRLF
        ///                           1*( SP feature CRLF )
        ///                           "211 End" CRLF
        ///         feature         = feature-label [ SP feature-parms ]
        ///         feature-label   = 1*VCHAR
        ///         feature-parms   = 1*TCHAR
        ///         
        /// Example:
        ///         C> FEAT
        ///         S> 211-Extensions supported:
        ///         S>  MLST size*;create;modify*;perm;media-type
        ///         S>  SIZE
        ///         S>  COMPRESSION
        ///         S>  MDTM
        ///         S> 211 END
        /// 
        /// </remarks>
        private void Parse()
        {
            if (_text.Length < FEAT_PREAMBLE_LEN)
                throw new FtpsFeatureException("preamble length not valid");
            
            string preamble = _text.Substring(0, FEAT_PREAMBLE_LEN).ToUpper();
            // test to see if there are no features listed
            if (preamble == NO_FEAT_PREAMBLE)
                return;
            else // test to make sure preamble is correct
                if (preamble != FEAT_PREAMBLE)
                    throw new FtpsFeatureException("preamble not found while parsing feature list");

            string[] lines = SplitCrLf(_text);

            // spin the lines of the features
            string fl = "";
            for(int i = 1; i < lines.Length; i++)
            {
                fl = lines[i];
                if (fl.Length < 2)
                    throw new FtpsFeatureException("not a properly formatted feature line");
                if (fl.Substring(0, 1) != " ")
                    break;
                string[] v = SplitFeature(fl);
                _list.Add(new FtpsFeature(v[0],v[1]));
            }

            if (fl.Length < FEAT_EPILOGUE_LEN)
                throw new FtpsFeatureException("epilogue length invalid");

            string epilogue = fl.Substring(0, FEAT_EPILOGUE_LEN).ToUpper();
            if (epilogue == FEAT_EPILOGUE)
                return;
            else 
                throw new FtpsFeatureException("epilogue not found while parsing feature list");
        }

        private string[] SplitCrLf(string list)
        {
            return list.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Split the feature line by extracting the feature command from the optional arguments.
        /// </summary>
        /// <param name="fl">Feature line to parse.</param>
        /// <returns>Array of two string elements.  The first element contains the command and the second the optional arguments.</returns>
        private string[] SplitFeature(string fl)
        {
            fl = fl.Trim();
            string[] v = fl.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (v == null || v.Length == 0)
                throw new FtpsFeatureException("error splitting features");

            string[] a = new string[2];
            a[0] = v[0];
            if (v.Length > 1)
                a[1] = fl.Substring(v[0].Length);
            else
                a[1] = "";

            // raise the command value to uppercase and trim
            a[0].ToUpper().Trim();
            // trim the arguments value but leave in native case
            a[1].Trim();

            return a;
        }
       
    
    }
}