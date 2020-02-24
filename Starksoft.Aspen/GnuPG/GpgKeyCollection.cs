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
using System.IO;
using System.Data;

namespace Starksoft.Aspen.GnuPG
{
    /// <summary>
    /// Collection of PGP keys stored in the GnuPGP application.
    /// </summary>
    public class GpgKeyCollection : IEnumerable<GpgKey>
    {
        private List<GpgKey> _keyList = new List<GpgKey>();
        private string _raw;

        private static string COL_KEY = "Key";
        private static string COL_KEY_EXPIRATION = "KeyExpiration";
        private static string COL_USER_ID = "UserId";
        private static string COL_USER_NAME = "UserName";
        private static string COL_SUB_KEY = "SubKey";
        private static string COL_SUB_KEY_EXPIRATION = "SubKeyExpiration";
        private static string COL_RAW = "Raw";
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="keys">StreamReader object containing GnuPG raw key stream data.</param>
        public GpgKeyCollection(StreamReader keys)
        {
            Fill(keys);
            GetRaw(keys);
        }

        /// <summary>
        /// Raw key stream text data.
        /// </summary>
        public string Raw
        {
            get { return _raw; }
        }

        private void GetRaw(StreamReader keys)
        {
            keys.BaseStream.Position = 0;
            _raw = keys.ReadToEnd();
        }

        //private void Fill(StreamReader data)
        //{
        //    string line = null;
        //    line = data.ReadLine();
        //    line = data.ReadLine();
        //    do
        //    {
        //        string line1 = data.ReadLine();
        //        string line2 = data.ReadLine();
        //        string line3 = data.ReadLine();
        //        while (!data.EndOfStream && !(line3.StartsWith("sub") || line3.StartsWith("ssb")))
        //        {
        //            line3 = data.ReadLine();
        //        }
        //        GnuPGKey key = new GnuPGKey(String.Format("{0}\r\n{1}\r\n{2}", line1, line2, line3));
        //        _keyList.Add(key);
        //        data.ReadLine();
        //    }
        //    while (!data.EndOfStream);
        //}

        private void Fill(StreamReader data)
        {
			StringBuilder sb = new StringBuilder ();
                        
            while (!data.EndOfStream)
            {
                // read a line from the output stream
                string line = data.ReadLine();

                // skip lines we are not interested in
                if (!line.StartsWith("pub") && !line.StartsWith("sec") && !line.StartsWith("uid"))
                {
                    // make sure this isn't the end of a key parsing operation
                    if (sb.Length != 0)
                    {
						_keyList.Add(new GpgKey(sb.ToString()));
						sb = new StringBuilder ();  // clear out the string builder
                    }
                    continue;
                }
				sb.AppendLine(line);
            }
        }

        /// <summary>
        ///  Searches for the specified GnuPGKey object and returns the zero-based index of the
        ///  first occurrence within the entire GnuPGKeyCollection colleciton.
        /// </summary>
        /// <param name="item">The GnuPGKeyobject to locate in the GnuPGKeyCollection.</param>
        /// <returns>The zero-based index of the first occurrence of item within the entire GnuPGKeyCollection, if found; otherwise, –1.</returns>
        public int IndexOf(GpgKey item)
        {
            return _keyList.IndexOf(item);
        }

        /// <summary>
        ///  Retrieves the specified GnuPGKey object by zero-based index from the GnuPGKeyCollection.        
        /// </summary>
        /// <param name="index">Zero-based index integer value.</param>
        /// <returns>The GnuPGKey object corresponding to the index position.</returns>
        public GpgKey GetKey(int index)
        { 
            return _keyList[index]; 
        }

        /// <summary>
        /// Adds a GnuPGKey object to the end of the GnuPGKeyCollection.
        /// </summary>
        /// <param name="item">GnuPGKey item to add to the GnuPGKeyCollection.</param>
        public void AddKey(GpgKey item)
        {
            _keyList.Add(item);
        }

        /// <summary>
        /// Gets the number of elements actually contained in the GnuPGKeyCollection.
        /// </summary>
        public int Count
        { 
            get { return _keyList.Count; } 
        }

        IEnumerator<GpgKey> IEnumerable<GpgKey>.GetEnumerator()
        { 
            return _keyList.GetEnumerator(); 
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _keyList.GetEnumerator();
        }

        /// <summary>
        /// Indexer for the GnuPGKeyCollection collection.
        /// </summary>
        /// <param name="index">Zero-based index value.</param>
        /// <returns></returns>
        public GpgKey this[int index]
        {
            get
            {
                return _keyList[index];
            }

        }

        /// <summary>
        /// Convert current GnuPGKeyCollection to a DataTable object to make data binding a minpulation of key data easier.
        /// </summary>
        /// <returns>Data table object.</returns>
        public DataTable ToDataTable()
        {
            DataTable dataTbl = new DataTable();
            CreateColumns(dataTbl);

            foreach (GpgKey item in _keyList)
            {
                DataRow row = dataTbl.NewRow();
                row[COL_USER_ID] = item.UserId;
                row[COL_USER_NAME] = item.UserName;
                row[COL_KEY] = item.Key;
                row[COL_KEY_EXPIRATION] = item.KeyExpiration;
                row[COL_SUB_KEY] = item.SubKey;
                row[COL_SUB_KEY_EXPIRATION] = item.SubKeyExpiration;
                dataTbl.Rows.Add(row);
            }

            return dataTbl;
        }

        private void CreateColumns(DataTable dataTbl)
        {
            dataTbl.Columns.Add(new DataColumn(COL_USER_ID, typeof(string)));
            dataTbl.Columns.Add(new DataColumn(COL_USER_NAME, typeof(string)));
            dataTbl.Columns.Add(new DataColumn(COL_KEY, typeof(string)));
            dataTbl.Columns.Add(new DataColumn(COL_KEY_EXPIRATION, typeof(DateTime)));
            dataTbl.Columns.Add(new DataColumn(COL_SUB_KEY, typeof(string)));
            dataTbl.Columns.Add(new DataColumn(COL_SUB_KEY_EXPIRATION, typeof(DateTime)));
            dataTbl.Columns.Add(new DataColumn(COL_RAW, typeof(string)));
        }

    }
}
