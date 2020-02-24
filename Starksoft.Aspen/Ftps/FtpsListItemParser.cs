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
using System.Text.RegularExpressions;
using System.Globalization;
using System.Text;

namespace Starksoft.Aspen.Ftps
{
    /// <summary>
    /// Generic ftp file and directory LIST listing parser that supports most Unix, Dos, and Windows style FTP 
    /// directory listings.  A custom parser can be created using the IFtpItemParser interface in the event
    /// this parser does not suit the needs of a specific FTP server directory format listing.
    /// </summary>
    public class FtpsListItemParser : IFtpsItemParser  
    {
        // unix regex expressions
        Regex _isUnix = new Regex(@"(d|l|-|b|c|p|s)(r|w|x|-|t|s){9}", RegexOptions.Compiled);
        Regex _unixAttribs = new Regex(@"(d|l|-|b|c|p|s)(r|w|x|-|t|s){9}", RegexOptions.Compiled);
        Regex _unixMonth = new Regex(@"(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec|mrt|mei|okt)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        Regex _unixDay = new Regex(@"(?<=(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec|mrt|mei|okt)\s+)\d+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        Regex _unixYear = new Regex(@"(?<=(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec|mrt|mei|okt)\s+\d+\s+)(19|20)\d\d", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        Regex _unixTime = new Regex(@"(?<=(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec|mrt|mei|okt)\s+\d+\s+)\d+:\d\d", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        Regex _unixSize = new Regex(@"\d+(?=(\s+(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec|mrt|mei|okt)))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        Regex _unixName = new Regex(@"((?<=((Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec|mrt|mei|okt)\s+\d+\s+(19|20)\d\d\s+)|((Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec|mrt|mei|okt)\s+\d+\s+\d+:\d\d\s+)).+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        Regex _unixSymbLink = new Regex(@"(?<=\s+->\s+).+", RegexOptions.Compiled);
        Regex _unixType = new Regex(@"(d|l|-|b|c|p|s)(?=(r|w|x|-|t|s){9})", RegexOptions.Compiled);

        // dos and other expressions
        Regex _dosName = new Regex(@"((?<=<DIR>\s+).+)|((?<=\d\d:\d\d\s+).+)|((?<=(\d\d:\d\d(AM|PM|am|pm)\s+\d+\s+)).+)", RegexOptions.Compiled);
        Regex _dosDate = new Regex(@"(\d\d-\d\d-\d\d)", RegexOptions.Compiled);
        Regex _dosTime = new Regex(@"(\d\d:\d\d\s*(AM|PM|am|pm))|(\d\d:\d\d)", RegexOptions.Compiled);
        Regex _dosSize = new Regex(@"((?<=(\d\d:\d\d\s*(AM|PM|am|pm)\s*))\d+)|(\d+(?=\s+\d\d-\d\d-\d\d\s+))", RegexOptions.Compiled);
        Regex _dosDir = new Regex(@"<DIR>|\sDIR\s", RegexOptions.Compiled);


        /// <summary>
        /// Method to parse a line of file listing data from the FTP server.
        /// </summary>
        /// <param name="line">Line to parse.</param>
        /// <returns>Object representing data in parsed file listing line.</returns>
        public FtpsItem ParseLine(string line)
        {
            if (_isUnix.IsMatch(line))
                return ParseUnixFormat(line);
            else
                return ParseDosFormat(line); 
        }
        
        private FtpsItem ParseUnixFormat(string line)
        {
            string attribs = _unixAttribs.Match(line).ToString();
            string month = _unixMonth.Match(line).ToString();
            string day = _unixDay.Match(line).ToString();
            string year = _unixYear.Match(line).ToString();
            string time = _unixTime.Match(line).ToString();
            string size = _unixSize.Match(line).ToString(); 
            string name = _unixName.Match(line).ToString().Trim();
            string symbLink = "";

            // ignore the microsoft 'etc' file that IIS uses for WWW users
            if (name == "~ftpsvc~.ckm")
                return null;

            //  if we find a symbolic link then extract the symbolic link first and then
            //  extract the file name portion
            if (_unixSymbLink.IsMatch(name))
            {
                symbLink = _unixSymbLink.Match(name).ToString();
                name = name.Substring(0, name.IndexOf("->")).Trim();
            }

            string itemType = _unixType.Match(line).ToString();


            //  if the current year is not given in unix then we need to figure it out.
            //  basically, if a date is within the past 6 months unix will show the 
            //  time instead of the year
            if (year.Length == 0)
            {
                int curMonth = DateTime.Today.Month;
                int curYear = DateTime.Today.Year;

                DateTime result;
                if (DateTime.TryParse(String.Format(CultureInfo.InvariantCulture, "1-{0}-2007", month), out result))
                {
                    if ((curMonth - result.Month) < 0)
                        year = Convert.ToString(curYear - 1, CultureInfo.InvariantCulture);
                    else
                        year = curYear.ToString(CultureInfo.InvariantCulture);
                }
            }

            DateTime dateObj;
            DateTime.TryParse(String.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2} {3}", day, month, year, time), out dateObj);

            long sizeLng = 0;
            Int64.TryParse(size, out sizeLng);

            FtpItemType itemTypeObj = FtpItemType.Other;
            switch (itemType.ToLower(CultureInfo.InvariantCulture))
            {
                case "l":
                    itemTypeObj = FtpItemType.SymbolicLink;
                    break;
                case "d":
                    itemTypeObj = FtpItemType.Directory;
                    break;
                case "-":
                    itemTypeObj = FtpItemType.File;
                    break;
                case "b":
                    itemTypeObj = FtpItemType.BlockSpecialFile;
                    break;
                case "c":
                    itemTypeObj = FtpItemType.CharacterSpecialFile;
                    break;
                case "p":
                    itemTypeObj = FtpItemType.NamedSocket;
                    break;
                case "s":
                    itemTypeObj = FtpItemType.DomainSocket;
                    break;
            }

            if (name == ".")
                itemTypeObj = FtpItemType.CurrentDirectory;
            if (name == "..")
                itemTypeObj = FtpItemType.ParentDirectory;

            if (itemTypeObj == FtpItemType.Other || name.Trim().Length == 0) 
                return null;
            else
                return new FtpsItem(name, dateObj, sizeLng, itemTypeObj, attribs, FtpsUtilities.AttributeToMode(attribs), symbLink, line);
        }

        private FtpsItem ParseDosFormat(string line)
        {
            string name = _dosName.Match(line).ToString().Trim();

            // if the name has no length the simply stop processing and return null.
            if (name.Trim().Length == 0)
                return null;

            string date = _dosDate.Match(line).ToString();
            string time = _dosTime.Match(line).ToString();
            string size = _dosSize.Match(line).ToString();
            string dir = _dosDir.Match(line).ToString().Trim();

            // put togther the date/time
            DateTime dateTime = DateTime.MinValue;
            DateTime.TryParse(String.Format(CultureInfo.InvariantCulture, "{0} {1}", date, time), out dateTime);

            // parse the file size
            long sizeLng = 0;
            Int64.TryParse(size, out sizeLng);           

            // determine the file item itemType
            FtpItemType itemTypeObj;
            if (dir.Length > 0)
                itemTypeObj = FtpItemType.Directory;
            else
                itemTypeObj = FtpItemType.File;

            if (name == ".")
                itemTypeObj = FtpItemType.CurrentDirectory;
            if (name == "..")
                itemTypeObj = FtpItemType.ParentDirectory;

            return new FtpsItem(name, dateTime, sizeLng, itemTypeObj, String.Empty, null, String.Empty, line);
        }
        
    }
}



