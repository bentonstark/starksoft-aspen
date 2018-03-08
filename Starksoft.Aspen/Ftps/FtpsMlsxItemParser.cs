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
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Text;

namespace Starksoft.Aspen.Ftps
{
    /// <summary>
    /// Generic ftp file and directory MLSx listing parser that supports most Unix, Dos, and Windows style FTP 
    /// directory listings.  A custom parser can be created using the IFtpItemParser interface in the event
    /// this parser does not suit the needs of a specific FTP server directory format listing.
    /// </summary>
    public class FtpsMlsxItemParser : IFtpsItemParser
    {
        // general information
        private string _name;

        // RFC defined MLSx object facts
        private DateTime? _modified;
        private long _size;
        private FtpItemType _itemType;
        private DateTime? _create;  // create=
        private string _unique;     // unique=
        private MlsxPerm _perm;      // perm=
        private string _lang;       // lang=
        private string _mediaType;  // media-type=
        private string _charSet;    // charset=

        // UNIX specific MLSx object facts
        private string _unixGroup;  // UNIX.group=
        private Int32? _unixMode;   // UNIX.mode=
        private string _unixOwner;  // UNIX.owner=

        private string _attributes;

        // MLSx fact names
        private const string FACT_SIZE = "size";
        private const string FACT_MODIFY = "modify";
        private const string FACT_CREATE = "create";
        private const string FACT_TYPE = "type";
        private const string FACT_UNIQUE = "unique";
        private const string FACT_PERM = "perm";
        private const string FACT_LANG = "lang";
        private const string FACT_MEDIA_TYPE = "media-type";
        private const string FACT_CHAR_SET = "charset";

        // MLSx extension fact names
        private const string FACT_EXT_UNIX_GROUP = "unix.group";
        private const string FACT_EXT_UNIX_MODE = "unix.mode";
        private const string FACT_EXT_UNIX_OWNER = "unix.owner";

        // MLSx media type indicators
        private const string TYPE_FILE = "file";
        private const string TYPE_CURRENT_DIRECTORY = "cdir";
        private const string TYPE_DIRECTORY = "dir";
        private const string TYPE_PARENT_DIRECTORY = "pdir";

        // MLSx permission indicators
        private const char PERM_CAN_APPENDED_FILE = 'a';
        private const char PERM_CAN_CREATE_FILE = 'c';
        private const char PERM_CAN_DELETE_FILE = 'd';
        private const char PERM_CAN_CHANGE_DIRECTORY = 'e';
        private const char PERM_CAN_RENAME = 'f';
        private const char PERM_CAN_LIST_FILES = 'l';
        private const char PERM_CAN_CREATE_DIRECTORY = 'm';
        private const char PERM_CAN_DELETE_DIRECTORY = 'p';
        private const char PERM_CAN_RETRIEVE_FILE = 'r';
        private const char PERM_CAN_STORE_FILE = 'w';
        // Unix permission not set indicator
        private const char PERM_NOT_SET = '-';

        // MLSx date/time field size
        private const int DATE_TIME_MIN_LEN = 14;
        private const int DATE_TIME_MAX_LEN = 18;

        /// <summary>
        /// Method to parse a line of file listing data from the FTP server.
        /// </summary>
        /// <param name="line">Line to parse.</param>
        /// <returns>Object representing data in parsed file listing line.</returns>
        public FtpsItem ParseLine(string line)
        {
            Trace.WriteLine("ftps: '" + line + "'");

            if (String.IsNullOrEmpty(line))
                throw new FtpsItemParsingException("line cannot be empty");
            Parse(line);

            FtpsMlsxItem m = new FtpsMlsxItem(_name,
                                            _modified,
                                            _size,
                                            _itemType,
                                            _attributes,
                                            _unixMode,
                                            _lang,
                                            _create,
                                            _unique,
                                            _perm,
                                            _lang,
                                            _mediaType,
                                            _charSet,
                                            _unixGroup,
                                            _unixOwner);

            return m;                                
        }

        private void Parse(string line)
        {
            // ex: Type=file;Size=1830;Modify=19940916055648;Perm=r; hatch.c
            // ex: modify=20120822211414;perm=adfr;size=2101;type=file;unique=16UF3F5;UNIX.group=49440;UNIX.mode=0744;UNIX.owner=49440; iphone_settings_icon.jpg
            // ex: modify=20030225143801;perm=adfr;size=503;type=file;unique=12U24470006;UNIX.group=0;UNIX.mode=0644;UNIX.owner=0; welcome.msg

            string filename;

            int spaceIndex = line.IndexOf( " ", StringComparison.InvariantCulture );

            if( spaceIndex == -1 ) {
                return;
            }

            filename = line.Substring( spaceIndex );
            if( filename.Length == 1 ) {
                return;
            }

            if( spaceIndex != 0 ) {
                string facts = line.Substring( 0, spaceIndex - 1 );

                string[] fields = facts.Split( new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries );

                // parse fact fields
                for( int i = 0; i < fields.Length - 1; i++ ) {
                    ParseField( fields[ i ] );
                }
            }

            // last field is always name of the object
            ParseName(filename);

            if (_name == ".")
                _itemType = FtpItemType.CurrentDirectory;
            if (_name == "..")
                _itemType = FtpItemType.ParentDirectory;
        }

        private void ParseName(string name)
        {
            if (String.IsNullOrEmpty(name))
                throw new FtpsItemParsingException("name cannot be a null or empty value");

            if (name.Length < 2)
                throw new FtpsItemParsingException("name length is too short");

            // first character must always be a space
            if (!(name[0] == ' ' && name[1] != ' '))
                throw new FtpsItemParsingException("name is not in proper format");

            _name = name.Trim();
        }

        private void ParseField(string field)
        {
            if (String.IsNullOrEmpty(field))
                throw new FtpsItemParsingException("field cannot be a null or empty value");

            if (field.IndexOf('=') == -1)
                throw new FtpsItemParsingException("field must contain equals '=' value" );

            // split the field fact
            string[] fact = field.Split('=');

            if (fact.Length != 2)
                throw new FtpsItemParsingException("field must contain equals '=' value with only a name and value");

            string name = fact[0].Trim().ToLower();
            string value = fact[1].Trim();

            if (name.Length == 0)
                throw new FtpsItemParsingException("field fact name cannot be empty");

            switch (name)
            {
                case FACT_CHAR_SET:
                    _charSet = value;
                    break;
                case FACT_CREATE:
                    _create = ParseDateTime(value);
                    break;
                case FACT_LANG:
                    _lang = value;
                    break;
                case FACT_MEDIA_TYPE:
                    _mediaType = value;
                    break;
                case FACT_MODIFY:
                    _modified = ParseDateTime(value);
                    break;
                case FACT_PERM:
                    _perm = ParsePerm(value);
                    break;
                case FACT_SIZE:
                    _size = ParseSize(value);
                    break;
                case FACT_TYPE:
                    _itemType = ParseType(value);
                    break;
                case FACT_UNIQUE:
                    _unique = value;
                    break;
                case FACT_EXT_UNIX_GROUP:
                    _unixGroup = value;
                    break;
                case FACT_EXT_UNIX_MODE:
                    _unixMode = ParseInt32(value);
                    if (_unixMode != null)
                        _attributes = FtpsUtilities.ModeToAttribute(_unixMode.Value);
                    break;
                case FACT_EXT_UNIX_OWNER:
                    _unixOwner = value;
                    break;
                default:
                    break;
            }
        }

        private DateTime? ParseDateTime(string v)
        {
            if (String.IsNullOrEmpty(v))
                return null;

            if (v.Length < DATE_TIME_MIN_LEN)
                throw new FtpsItemParsingException(String.Format("MLSx '{0}' date/time value too small", v));

            if (v.Length > DATE_TIME_MAX_LEN)
                throw new FtpsItemParsingException(String.Format("MLSx '{0}' date/time value too large", v));

            // YYYYMMDDHHMMSS.sss
            int yr = int.Parse(v.Substring(0, 4), CultureInfo.InvariantCulture);
            int mon = int.Parse(v.Substring(4, 2), CultureInfo.InvariantCulture);
            int day = int.Parse(v.Substring(6, 2), CultureInfo.InvariantCulture);
            int hr = int.Parse(v.Substring(8, 2), CultureInfo.InvariantCulture);
            int min = int.Parse(v.Substring(10, 2), CultureInfo.InvariantCulture);
            int sec = int.Parse(v.Substring(12, 2), CultureInfo.InvariantCulture);
            int ms = 0;  // optional

            if (v.Length == DATE_TIME_MAX_LEN)
                ms = int.Parse(v.Substring(15, 3), CultureInfo.InvariantCulture);

            DateTime dateUtc = new DateTime(yr,
                                mon,
                                day,
                                hr,
                                min,
                                sec,
                                ms,
                                DateTimeKind.Utc);

            // adjust the UTC (GMT) time to local time
            return new DateTime(dateUtc.ToLocalTime().Ticks);
        }

        private Int32? ParseInt32(string v)
        {
            if (String.IsNullOrEmpty(v))
                return null;

            int r;
            if (!Int32.TryParse(v, out r))
                throw new FtpsItemParsingException("MLSx fact value cannot be converted to integer value");

            return r;
        }

        private long ParseSize(string v)
        {
            if (String.IsNullOrEmpty(v))
                return 0;

            long r;
            if (!Int64.TryParse(v, out r))
                throw new FtpsItemParsingException("MLSx 'size' fact value cannot be converted to integer value");

            return r;
        }

        private MlsxPerm ParsePerm(string v)
        {
            if (String.IsNullOrEmpty(v))
                return MlsxPerm.None;

            char[] pary;

            v = v.ToLower();

            // Wing FTP Server returns POSIX-style permissions. So just pull out the user read and write permissions and parse those.
            if( v.Length == 10 && ( v.Contains( "-" ) || v == "drwxrwxrwx" ) ) {
                pary = new[] { v[ 1 ], v[ 2 ] };
            }
            else {
                pary = v.ToCharArray();
            }

            MlsxPerm pem = MlsxPerm.None;

            foreach (char pchar in pary)
            {
                switch (pchar)
                {
                    case PERM_CAN_APPENDED_FILE:
                        pem = pem | MlsxPerm.CanAppendFile;
                        break;
                    case PERM_CAN_CHANGE_DIRECTORY:
                        pem = pem | MlsxPerm.CanChangeDirectory;
                        break;
                    case PERM_CAN_CREATE_DIRECTORY:
                        pem = pem | MlsxPerm.CanCreateDirectory;
                        break;
                    case PERM_CAN_CREATE_FILE:
                        pem = pem | MlsxPerm.CanCreateFile;
                        break;
                    case PERM_CAN_DELETE_DIRECTORY:
                        pem = pem | MlsxPerm.CanDeleteDirectory;
                        break;
                    case PERM_CAN_DELETE_FILE:
                        pem = pem | MlsxPerm.CanDeleteFile;
                        break;
                    case PERM_CAN_LIST_FILES:
                        pem = pem | MlsxPerm.CanListFiles;
                        break;
                    case PERM_CAN_RENAME:
                        pem = pem | MlsxPerm.CanRename;
                        break;
                    case PERM_CAN_RETRIEVE_FILE:
                        pem = pem | MlsxPerm.CanRetrieveFile;
                        break;
                    case PERM_CAN_STORE_FILE:
                        pem = pem | MlsxPerm.CanStoreFile;
                        break;
                    case PERM_NOT_SET:
                        break;
                    default:
                        throw new FtpsItemParsingException(String.Format("unknown MLSx 'perm' value '{0}' encountered", pchar));
                }
            }

            return pem;
        }

        private FtpItemType ParseType(string v)
        {
            if (String.IsNullOrEmpty(v))
                return FtpItemType.None;

            switch (v.ToLower())
            {
                case TYPE_CURRENT_DIRECTORY:
                    return FtpItemType.CurrentDirectory;
                case TYPE_DIRECTORY:
                    return FtpItemType.Directory;
                case TYPE_FILE:
                    return FtpItemType.File;
                case TYPE_PARENT_DIRECTORY:
                    return FtpItemType.ParentDirectory;
                default:
                    throw new FtpsItemParsingException(String.Format("unknown MLSx 'type' value '{0}' encountered", v));
            }
        }


    }
}
