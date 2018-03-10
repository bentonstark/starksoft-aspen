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
using System.Collections.ObjectModel;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace Starksoft.Aspen.Ftps
{
    /// <summary>
    /// Ftp item list.
    /// </summary>
    /// <exception cref="FtpsFeatureException"></exception>
    public class FtpsItemCollection : IEnumerable<FtpsItem> 
	{
        private List<FtpsItem> _list = new List<FtpsItem>();

        private long _totalSize;

        // standard data columns
        private static string COL_NAME = "Name";
        private static string COL_MODIFIED = "Modified";
        private static string COL_SIZE = "Size";
        private static string COL_SYMBOLIC_LINK = "SymbolicLink";
        private static string COL_TYPE = "Type";
        private static string COL_ATTRIBUTES = "Attributes";
        private static string COL_MODE = "Mode";
        private static string COL_RAW_TEXT = "RawText";

        // MLSx specific data columns
        private static string COL_MLSX_CREATED = "Created";
        private static string COL_MLSX_UNIQUE_ID = "UniqueId";
        private static string COL_MLSX_PERMISSIONS = "Permissions";
        private static string COL_MLSX_LANGUAGE = "Language";
        private static string COL_MLSX_MEDIA_TYPE = "MediaType";
        private static string COL_MLSX_CHARACTER_SET = "CharacterSet";
        private static string COL_MLSX_GROUP = "Group";
        private static string COL_MLSX_OWNER = "Owner";

        /// <summary>
        /// Default constructor for FtpItemCollection.
        /// </summary>
        public FtpsItemCollection()
        {  }

        /// <summary>
        /// Split a multi-line file list text response and add the parsed items to the collection.
        /// </summary>
        /// <param name="path">Path to the item on the FTP server.</param>
        /// <param name="fileList">The multi-line file list text from the FTP server.</param>
        /// <param name="itemParser">Line item parser object used to parse each line of fileList data.</param>
        public FtpsItemCollection(string path, string fileList, IFtpsItemParser itemParser)
        {
            Parse(path, fileList, itemParser);
        }

        /// <summary>
        /// Merges two FtpItemCollection together into a single collection.
        /// </summary>
        /// <param name="items">Collection to merge with.</param>
        public void Merge(FtpsItemCollection items)
        {
            if (items == null)
                throw new ArgumentNullException("items", "must have a value");

            foreach (FtpsItem item in items)
            {
                FtpsItem n = new FtpsItem(item.Name, item.Modified, item.Size, item.ItemType, item.Attributes, item.Mode, item.SymbolicLink, item.RawText);
                n.SetParentPath(item.ParentPath);
                this.Add(n);
            }
        }

        private void Parse(string path, string fileList, IFtpsItemParser itemParser)
        {
            string[] lines = SplitFileList(fileList);

            int length = lines.Length - 1;
            for (int i = 0; i <= length; i++)
            {
                FtpsItem item = itemParser.ParseLine(lines[i]);
                if (item != null)
                {
                    // set the parent path to the value passed in
                    item.SetParentPath(path);
                    _list.Add(item);
                    _totalSize += item.Size;
                }
            }
        }

        private string[] SplitFileList(string response)
        {
            return response.Split(new char[] {'\r','\n'}, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Convert current FtpCollection to a DataTable object.
        /// </summary>
        /// <returns>Data table object.</returns>
        public DataTable ToDataTable()
        {
            DataTable dataTbl = new DataTable();
            dataTbl.Locale = CultureInfo.InvariantCulture;
            bool mlsx = false;

            if (_list.Count > 0)
            {
                if (_list[0] is FtpsMlsxItem)
                    mlsx = true;
            }

            CreateColumns(dataTbl, mlsx);

            foreach (FtpsItem item in _list)
            {
                // skip parent and current directory items to get consistent data
                if (item.ItemType == FtpItemType.ParentDirectory
                    || item.ItemType == FtpItemType.CurrentDirectory)
                    continue;

                DataRow row = dataTbl.NewRow();

                // set standard row columns for FtpItem object
                row[COL_NAME] = item.Name;
                row[COL_MODIFIED] = item.Modified == null ? DateTime.MinValue : item.Modified;
                row[COL_SIZE] = item.Size;
                row[COL_TYPE] = item.ItemType.ToString();
                row[COL_ATTRIBUTES] = item.Attributes == null ? String.Empty : item.Attributes;
                row[COL_SYMBOLIC_LINK] = item.SymbolicLink == null ? String.Empty : item.SymbolicLink;
                row[COL_RAW_TEXT] = item.RawText == null ? String.Empty : item.RawText;
                row[COL_MODE] = item.Mode == null ? 0 : item.Mode;

                // set MLSx specific columns if the object is a FtpMlsxItem object
                if (mlsx)
                {
                    FtpsMlsxItem mitem = (FtpsMlsxItem)item;
                    row[COL_MLSX_CREATED] = mitem.Created == null ? DateTime.MinValue : mitem.Created; ;
                    row[COL_MLSX_UNIQUE_ID] = mitem.UniqueId == null ? String.Empty : mitem.UniqueId;
                    row[COL_MLSX_PERMISSIONS] = mitem.Permissions.ToString();
                    row[COL_MLSX_LANGUAGE] = mitem.Language == null ? String.Empty : mitem.Language;
                    row[COL_MLSX_MEDIA_TYPE] = mitem.MediaType == null ? String.Empty : mitem.MediaType;
                    row[COL_MLSX_CHARACTER_SET] = mitem.CharacterSet == null ? String.Empty : mitem.CharacterSet;
                    row[COL_MLSX_GROUP] = mitem.Group == null ? "" : mitem.Group;
                    row[COL_MLSX_OWNER] = mitem.Owner == null ? "" : mitem.Owner;
                }

                dataTbl.Rows.Add(row);
            }

            return dataTbl;
        }
        
        private void CreateColumns(DataTable dataTbl, bool mlsx)
        {
            // standard data columns
            dataTbl.Columns.Add(new DataColumn(COL_NAME, typeof(string)));
            dataTbl.Columns.Add(new DataColumn(COL_MODIFIED, typeof(DateTime)));
            dataTbl.Columns.Add(new DataColumn(COL_SIZE, typeof(long)));
            dataTbl.Columns.Add(new DataColumn(COL_TYPE, typeof(string)));
            dataTbl.Columns.Add(new DataColumn(COL_ATTRIBUTES, typeof(string)));
            dataTbl.Columns.Add(new DataColumn(COL_SYMBOLIC_LINK, typeof(string)));
            dataTbl.Columns.Add(new DataColumn(COL_RAW_TEXT, typeof(string)));
            dataTbl.Columns.Add(new DataColumn(COL_MODE, typeof(Int32)));

            if (!mlsx)
                return;

            // MLSx specific data columns
            dataTbl.Columns.Add(new DataColumn(COL_MLSX_CREATED, typeof(DateTime)));
            dataTbl.Columns.Add(new DataColumn(COL_MLSX_UNIQUE_ID, typeof(String)));
            dataTbl.Columns.Add(new DataColumn(COL_MLSX_PERMISSIONS, typeof(String)));
            dataTbl.Columns.Add(new DataColumn(COL_MLSX_LANGUAGE, typeof(String)));
            dataTbl.Columns.Add(new DataColumn(COL_MLSX_MEDIA_TYPE, typeof(String)));
            dataTbl.Columns.Add(new DataColumn(COL_MLSX_CHARACTER_SET, typeof(String)));
            dataTbl.Columns.Add(new DataColumn(COL_MLSX_GROUP, typeof(String)));
            dataTbl.Columns.Add(new DataColumn(COL_MLSX_OWNER, typeof(String)));
        }

        /// <summary>
        /// Gets the size, in bytes, of all files in the collection as reported by the FTP server.
        /// </summary>
        public long TotalSize
        {
            get { return _totalSize; }
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the
        /// first occurrence within the entire FtpItemCollection list.
        /// </summary>
        /// <param name="item">The FtpItem to locate in the collection.</param>
        /// <returns>The zero-based index of the first occurrence of item within the entire if found; otherwise, -1.</returns>
        public int IndexOf(FtpsItem item)
        {
            return _list.IndexOf(item);
        }

        /// <summary>
        /// Adds an FtpItem to the end of the FtpItemCollection list.
        /// </summary>
        /// <param name="item">FtpItem object to add.</param>
        public void Add(FtpsItem item)
        {
            _list.Add(item);
        }

        /// <summary>
        ///  Gets the number of elements actually contained in the FtpItemCollection list.
        /// </summary>
        public int Count
        {
            get { return _list.Count; }
        }

        IEnumerator<FtpsItem> IEnumerable<FtpsItem>.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        /// <summary>
        /// Gets an FtpItem from the FtpItemCollection based on index value.
        /// </summary>
        /// <param name="index">Numeric index of item to retrieve.</param>
        /// <returns>FtpItem</returns>
        public FtpsItem this[int index]
        {
            get { return _list[index];  }
        }

        /// <summary>
        /// Linearly searches for the specified object based on the 'name' parameter value
        /// and returns true if an object with the name is found; otherwise false.
        /// </summary>
        /// <param name="name">The name of the FtpItem to locate in the collection.</param>
        /// <returns>True if the name if found; otherwise false.</returns>
        public bool Contains(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name", "must have a value");

            foreach (FtpsItem item in _list)
            {
                if (name == item.Name)
                    return true;
            }

            return false;
        }
    }
} 