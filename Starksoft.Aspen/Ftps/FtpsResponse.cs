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

namespace Starksoft.Aspen.Ftps
{
    /// <summary>
    /// FTP response class containing the FTP raw text, response code, and other information.
    /// </summary>
    public class FtpsResponse
    {
        private string _rawText;
        private string _text;
        private FtpsResponseCode _code = FtpsResponseCode.None;
        private bool _isInformational;

        /// <summary>
        /// Default constructor for FtpResponse.
        /// </summary>
        public FtpsResponse()
        { }

        /// <summary>
        /// Constructor for FtpResponse.
        /// </summary>
        /// <param name="rawText">Raw text information sent from the FTP server.</param>
        public FtpsResponse(string rawText)
        {
            _rawText = rawText;
            _text = ParseText(rawText);
            _code = ParseCode(rawText);
            _isInformational = ParseInformational(rawText);
        }

        /// <summary>
        /// Constructor for FtpResponse.
        /// </summary>
        /// <param name="response">FtpResponse object.</param>
        public FtpsResponse(FtpsResponse response)
        {
            _rawText = response.RawText;
            _text = response.Text;
            _code = response.Code;
            _isInformational = response.IsInformational;
        }

        /// <summary>
        /// Get raw server response text information.
        /// </summary>
        public string RawText
        {
            get { return _rawText; }
        }

        /// <summary>
        /// Get the server response text.
        /// </summary>
        public string Text
        {
            get { return _text; }
        }

        /// <summary>
        /// Get a value indicating the FTP server response code.
        /// </summary>
        public FtpsResponseCode Code
        {
            get { return _code; }
        }

        internal bool IsInformational
        {
            get { return _isInformational; }
        }

        private FtpsResponseCode ParseCode(string rawText)
        {
            FtpsResponseCode code = FtpsResponseCode.None;

            if (rawText.Length >= 3)
            {
                string codeString = rawText.Substring(0, 3);
                int codeInt = 0;

                if (Int32.TryParse(codeString, out codeInt))
                {
                    code = (FtpsResponseCode)codeInt;
                }
            }

            return code;
        }

        private string ParseText(string rawText)
        {
            if (rawText.Length > 4)
                return rawText.Substring(4).Trim();
            else
                return string.Empty;
        }

        private bool ParseInformational(string rawText)
        {
            if (rawText.Length >= 4 && rawText[3] == '-')
                return true;
            else
                return false;
        }


    }
}
