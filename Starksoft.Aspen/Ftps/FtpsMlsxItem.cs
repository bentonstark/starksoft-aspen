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
using System.Text;
using System.Globalization;

namespace Starksoft.Aspen.Ftps
{
    /// <summary>
    /// The MLSx perm options as defined by RFC 3659.
    /// </summary>
    [Flags]
    public enum MlsxPerm
    {
        /// <summary>
        /// No permissions specified.
        /// </summary>
        None = 0x000,
        /// <summary>
        /// Indicates the file can be appended to.
        /// </summary>
        /// <remarks>
        /// The "a" permission applies to objects of type=file, and indicates
        /// that the APPE (append) command may be applied to the file named.
        /// </remarks>
        CanAppendFile = 0x001,
        /// <summary>
        /// Indicates the directory allow files to be created.
        /// </summary>
        /// <remarks>
        /// The "c" permission applies to objects of type=dir (and type=pdir,
        /// type=cdir).  It indicates that files may be created in the directory
        /// named.  That is, that a STOU command is likely to succeed, and that
        /// STOR and APPE commands might succeed if the file named did not
        /// previously exist, but is to be created in the directory object that
        /// has the "c" permission.  It also indicates that the RNTO command is
        /// likely to succeed for names in the directory.
        /// </remarks>
        CanCreateFile = 0x002,
        /// <summary>
        /// Indicates the directory or file can be deleted.
        /// </summary>
        /// <remarks>
        /// The "d" permission applies to all types.  It indicates that the
        /// object named may be deleted, that is, that the RMD command may be
        /// applied to it if it is a directory, and otherwise that the DELE
        /// command may be applied to it.
        /// </remarks>
        CanDeleteFile = 0x004,
        /// <summary>
        /// Indicates the change directory command can be executed sucessfully.
        /// </summary>
        /// <remarks>
        /// The "e" permission applies to the directory types.  When set on an
        /// object of type=dir, type=cdir, or type=pdir it indicates that a CWD
        /// command naming the object should succeed, and the user should be able
        /// to enter the directory named.  For type=pdir it also indicates that
        /// the CDUP command may succeed (if this particular pathname is the one
        /// to which a CDUP would apply.) 
        /// </remarks>
        CanChangeDirectory = 0x008,
        /// <summary>
        /// Indicates the directory for file can be renamed.
        /// </summary>
        /// <remarks>
        /// The "f" permission for objects indicates that the object named may be
        /// renamed - that is, may be the object of an RNFR command.
        /// </remarks>
        CanRename = 0x010,
        /// <summary>
        /// Indicates the files for the directory can be listed.
        /// </summary>
        /// <remarks>
        /// The "l" permission applies to the directory file types, and indicates
        /// that the listing commands, LIST, NLST, and MLSD may be applied to the
        /// directory in question.
        /// </remarks>
        CanListFiles = 0x020,
        /// <summary>
        /// Indicates directories in this directory may be created.
        /// </summary>
        /// <remarks>
        /// The "m" permission applies to directory types, and indicates that the
        /// MKD command may be used to create a new directory within the
        /// directory under consideration.
        /// </remarks>
        CanCreateDirectory = 0x040,
        /// <summary>
        /// Indicates objects in this directory may be deleted.
        /// </summary>
        /// <remarks>
        /// The "p" permission applies to directory types, and indicates that
        /// objects in the directory may be deleted, or (stretching naming a
        /// little) that the directory may be purged.  Note: it does not indicate
        /// that the RMD command may be used to remove the directory named
        /// itself, the "d" permission indicator indicates that.
        /// </remarks>
        CanDeleteDirectory = 0x080,
        /// <summary>
        /// Indicates files in this directory may be retrieved.
        /// </summary>
        /// <remarks>
        /// The "r" permission applies to type=file objects, and for some
        /// systems, perhaps to other types of objects, and indicates that the
        /// RETR command may be applied to that object.
        /// </remarks>
        CanRetrieveFile = 0x100,
        /// <summary>
        /// Indicates files in this directory may be stored.
        /// </summary>
        /// <remarks>
        /// The "w" permission applies to type=file objects, and for some
        /// systems, perhaps to other types of objects, and indicates that the
        /// STOR command may be applied to the object named.
        /// Note: That a permission indicator is set can never imply that the
        /// appropriate command is guaranteed to work
        /// </remarks>
        CanStoreFile = 0x200
    }

    /// <summary>
    /// FTP MLSx item class.
    /// </summary>
    public class FtpsMlsxItem : FtpsItem
    {

        // RFC defined MLSx object facts
        private DateTime? _created;     // create=
        private string _uniqueId;       // unique=
        private MlsxPerm _permissions;  // perm=
        private string _language;       // lang=
        private string _mediaType;      // media-type=
        private string _characterSet;   // charset=

        // UNIX specific MLSx object facts
        private string _group;      // UNIX.group=
        private string _owner;      // UNIX.owner=

        /// <summary>
        /// Default constructor.
        /// </summary>
        public FtpsMlsxItem()
        { }


        /// <summary>
        /// Constructor to create a new ftp item.
        /// </summary>
        /// <param name="name">Name of the item.</param>
        /// <param name="modified">Modified date and/or time of the item.</param>
        /// <param name="size">Number of bytes or size of the item.</param>
        /// <param name="itemType">Type of the item.</param>
        /// <param name="attributes">UNIX style attributes value.</param>
        /// <param name="mode">UNIX style mode value</param>
        /// <param name="rawText">The raw text of the item.</param>
        /// <param name="created">Created date.</param>
        /// <param name="uniqueId">Unique identifier.</param>
        /// <param name="permissions">File action permissions.</param>
        /// <param name="language">File language.</param>
        /// <param name="mediaType">MIME file type.</param>
        /// <param name="characterSet">Character set of the file.</param>
        /// <param name="group">UNIX style group value.</param>
        /// <param name="owner">UNIX style owner value.</param>
        public FtpsMlsxItem(string name, 
                       DateTime? modified, 
                       long size, 
                       FtpItemType itemType, 
                       string attributes, 
                       Int32? mode,
                       string rawText,
                       DateTime? created,
                       string uniqueId,
                       MlsxPerm permissions,
                       string language,
                       string mediaType,
                       string characterSet,
                       string group,
                       string owner
                     ) : base(name, modified, size, itemType, attributes, mode, String.Empty, rawText)
        {
            _created = created;
            _uniqueId = uniqueId;
            _permissions = permissions;
            _language = language;
            _mediaType = mediaType;
            _characterSet = characterSet;
            _group = group;
            _owner = owner;
        }

               
        /// <summary>
        /// Gets the creation date and time.  Adjusted to from UTC (GMT) to local time.
        /// </summary>
        public DateTime? Created
        {
            get { return _created; }
        }

               
        /// <summary>
        /// Gets the unique id of the file or directory.
        /// </summary>
        public string UniqueId
        {
            get { return _uniqueId; }
        }

        /// <summary>
        /// Gets the enumerated file permissions allowed for the currently logged in user.
        /// </summary>
        /// <remarks>
        /// Permissions are based on FTP server implementation and are represented as
        /// a flag based enumeration.
        /// </remarks>
        public MlsxPerm Permissions
        {
            get { return _permissions; }
        }

        /// <summary>
        /// Gets the language of the file per the IANA registry.
        /// </summary>
        public string Language
        {
            get { return _language; }
        }

        /// <summary>
        /// Gets MIME media-type of teh file contents per the IANA registry.
        /// </summary>
        public string MediaType
        {
            get { return _mediaType; }
        }

        /// <summary>
        /// Gets the character set per the IANA registry if not UTF-8.
        /// </summary>
        public string CharacterSet
        {
            get { return _characterSet; }
        }

        /// <summary>
        /// Gets the UNIX specific extension Group integer value for the item.  
        /// </summary>
        /// <remarks>
        /// The UNIX.group fact extension value is not defined by RFC documents and are
        /// implemented by convention only.  Not all FTP servers will provide the 
        /// UNIX.* facts.  This value is a nullable field and will be null if no
        /// fact name was found.
        /// </remarks>
        public string Group
        {
            get { return _group; }
        }

        /// <summary>
        /// Gets the UNIX specific extension Owner integer value for the item.
        /// </summary>
        /// <remarks>
        /// The UNIX.owner fact extension value is not defined by RFC documents and are
        /// implemented by convention only.  Not all FTP servers will provide the 
        /// UNIX.* facts.  This value is a nullable field and will be null if no
        /// fact name was found.
        /// </remarks>
        public string Owner
        {
            get { return _owner; }
        }
        
        /// <summary>
        /// Creates a clone of the item object.
        /// </summary>
        /// <returns>Returns a FtpMlsxItem clone.</returns>
        public override FtpsItem Clone()
        {
            FtpsMlsxItem c = new FtpsMlsxItem(base.Name,
                                            base.Modified,
                                            base.Size,
                                            base.ItemType,
                                            base.Attributes,
                                            base.Mode,
                                            base.RawText,
                                            _created,
                                            _uniqueId,
                                            _permissions,
                                            _language,
                                            _mediaType,
                                            _characterSet,
                                            _group,
                                            _owner);
            return c;
        }

        /// <summary>
        /// Get the string representation of the FtpMlsxItem.
        /// </summary>
        /// <returns>String containing FtpMlsxItem information.</returns>
        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            // get the base object ToString()
            b.Append(base.ToString());
            
            // create the sub-class object ToString()
            if (_created != null)
                b.AppendFormat("Created={0}\r\n", _created);
            else
                b.Append("Created=null\r\n");
            b.AppendFormat("UniqueId={0}\r\n", _uniqueId);
            b.AppendFormat("Permissions={0}\r\n", _permissions);
            b.AppendFormat("Language={0}\r\n", _language);
            b.AppendFormat("MediaType={0}\r\n", _mediaType);
            b.AppendFormat("CharacterSet={0}\r\n", _characterSet);
            if (_group != null)
                b.AppendFormat("Group={0}\r\n", _group);
            else
                b.Append("Group=null\r\n");
            if (_owner != null)
                b.AppendFormat("Owner={0}\r\n", _owner);
            else
                b.Append("Owner=null\r\n");
            return b.ToString();
        }
    }
}
