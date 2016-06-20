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
    /// The type of item.
    /// </summary>
    public enum FtpItemType
    {
        /// <summary>
        /// No item type specified.
        /// </summary>
        None,
        /// <summary>
        /// Directory item.
        /// </summary>
        Directory,
        /// <summary>
        /// File item.
        /// </summary>
        File,
        /// <summary>
        /// Symbolic link item.
        /// </summary>
        SymbolicLink,
        /// <summary>
        /// Block special file item.
        /// </summary>
        BlockSpecialFile,
        /// <summary>
        /// Character special file item.
        /// </summary>
        CharacterSpecialFile,
        /// <summary>
        /// Name socket item.
        /// </summary>
        NamedSocket,
        /// <summary>
        /// Domain socket item.
        /// </summary>
        DomainSocket,
        /// <summary>
        /// Pathname of the directory whose contents are listed. 
        /// </summary>
        CurrentDirectory,
        /// <summary>
        /// Parent directory. 
        /// </summary>
        ParentDirectory,
        /// <summary>
        /// Other item type.
        /// </summary>
        Other
    }

    /// <summary>
    /// Base class for the FTP item.
    /// </summary>
    public class FtpsItem
    {
        private string _rawText;
        private string _parentPath;
        private FtpItemType _itemType;
        private string _name;
        private long _size;
        private DateTime? _modified;
        private string _attributes;
        private Int32? _mode;
        private string _symbolicLink;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public FtpsItem()
        { }

        /// <summary>
        /// Constructor to create a new ftp item.
        /// </summary>
        /// <param name="name">Name of the item.</param>
        /// <param name="modified">Modified date and/or time of the item.</param>
        /// <param name="size">Number of bytes or size of the item.</param>
        /// <param name="itemType">Type of the item.</param>
        /// <param name="attributes">UNIX style permission attribute text for item.</param>
        /// <param name="mode">UNIX style mode permission integer value.</param>
        /// <param name="symbolicLink">UNIX style symbolic linked file name .</param>
        /// <param name="rawText">The raw text of the item.</param>
        public FtpsItem(string name, 
                       DateTime? modified, 
                       long size, 
                       FtpItemType itemType, 
                       string attributes,
                       Int32? mode,
                       string symbolicLink,
                       string rawText)
		{
            _name = name;
            _modified = modified;
            _size = size;
            _itemType = itemType;
            _attributes = attributes;
            _mode = mode;
            _symbolicLink = symbolicLink;
            _rawText = rawText;
		}


        /// <summary>
        /// Gets the raw text line for the item.
        /// </summary>
        public string RawText
        {
            get { return _rawText; }
        }

        /// <summary>
        /// Gets the parent path for the item.
        /// </summary>
        public string ParentPath
        {
            get { return _parentPath; }
        }

        /// <summary>
        /// Internal method to set the parent path value.
        /// </summary>
        /// <param name="path">Parent path.</param>
        internal void SetParentPath(string path)
        {
            _parentPath = path;
        }

        /// <summary>
        /// Gets the name of the item.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Gets the size of the file or directory.
        /// </summary>
        public long Size
        {
            get { return _size; }
        }

        /// <summary>
        /// Gets the last modification date and time.  Adjusted to from UTC (GMT) to local time.
        /// </summary>
        public DateTime? Modified
        {
            get { return _modified; }
        }
        
        /// <summary>
        /// Gets the item type.
        /// </summary>
        public FtpItemType ItemType
        {
            get { return _itemType; }
        }

        /// <summary>
        /// UNIX style permissions text for the item.  
        /// </summary>
        public string Attributes
        {
            get { return _attributes; }
        }

        /// <summary>
        /// UNIX style mode permission integer value for the item.  
        /// </summary>
        public Int32? Mode
        {
            get { return _mode; }
        }

        /// <summary>
        /// The symbolic link name if the item is of itemType symbolic link.
        /// </summary>
        public string SymbolicLink
        {
            get { return _symbolicLink; }
        }

        /// <summary>
        /// Item full path.
        /// </summary>
        public string FullPath
        {
            get { return _parentPath == "/" || _parentPath == "//" ? String.Format("{0}{1}", _parentPath, _name) : String.Format("{0}/{1}", _parentPath, _name); }
        }

        /// <summary>
        /// Clone the current object.
        /// </summary>
        /// <returns></returns>
        public virtual FtpsItem Clone()
        {
            FtpsItem c = new FtpsItem(_name,
                                    _modified,
                                    _size,
                                    _itemType,
                                    _attributes,
                                    _mode,
                                    _symbolicLink,
                                    _rawText);
            return c;
        }

        /// <summary>
        /// Gets the item base properties as string.
        /// </summary>
        /// <returns>String object.</returns>
        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            b.AppendFormat("ParentPath={0}\r\n", _parentPath);
            b.AppendFormat("Name={0}\r\n", _name);
            b.AppendFormat("Size={0}\r\n", _size);
            if (_modified != null)
                b.AppendFormat("Modified={0}\r\n", _modified);
            else
                b.AppendFormat("Modified=null\r\n");
            b.AppendFormat("ItemType={0}\r\n", _itemType);
            b.AppendFormat("Attributes{0}\r\n", _attributes);
            if (_mode != null)
                b.AppendFormat("Mode={0}\r\n", _mode);
            else
                b.AppendFormat("Mode=null\r\n");
            b.AppendFormat("SymbolicLink={0}\r\n", _symbolicLink);

            return b.ToString();
        }
    }
}
