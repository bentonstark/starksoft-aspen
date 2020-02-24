using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Starksoft.Aspen.GnuPG
{
    /// <summary>
    /// GPG command line version parser. This class parses the GPG stdout data in the expected format
    ///     gpg GnuPG major.minor.revision 
    /// where major and minor are expected to be integers and revsision can be an integer or string.
    ///     
    /// Examples:
    ///     gpg (GnuPG) 2.2.4
    ///     gpg (GnuPG) 2.1.1-beta3
    /// </summary>
    class GpgVersionParser
    {
        private static Regex _gpgVersion = new Regex(@"(?<=gpg \(GnuPG\) )([0-9]*\.[0-9]*\..*)", RegexOptions.Compiled);

        /// <summary>
        /// Private constructor.
        /// </summary>
        private GpgVersionParser() {  }

        /// <summary>
        /// Parse the GPG version information from the Stdout of the GPG command line interface
        /// executed with the --version argument.
        /// </summary>
        /// <param name="data">stdout stream data</param>
        /// <returns>GPG version object</returns>
        public static GpgVersion Parse(StreamReader data)
        {
            string firstLine = ReadFirstStdOutLine(data);
            GpgVersion ver = ParseVersion(firstLine);
            return ver;
        }

        private static string ReadFirstStdOutLine(StreamReader data)
        {
            return data.ReadLine();
        }

        private static GpgVersion ParseVersion(string line) {

            if (!_gpgVersion.IsMatch(line))
            {
                throw new GpgException(String.Format("unexpected gpg command line version data '{0}'", line));
            }

            string[] version = _gpgVersion.Match(line).ToString().Split('.');
            int major = Int32.Parse(version[0]);
            int minor = Int32.Parse(version[1]);
            string revision = version[2];

            return new GpgVersion(major, minor, revision);
        }

    }


}
