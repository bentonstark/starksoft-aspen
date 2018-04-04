using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Starksoft.Aspen.GnuPG
{
    /// <summary>
    /// GPG version number class.
    /// </summary>
    public class GpgVersion
    {
        private int _major;
        private int _minor;
        private string _revision;

        public GpgVersion(int major, int minor, string revision)
        {
            _major = major;
            _minor = minor;
            _revision = revision;
        }

        /// <summary>
        /// Gets the GPG major version number. 
        /// </summary>
        /// <returns>Major version number</returns>
        public int Major
        {
            get { return _major; }
        }

        /// <summary>
        /// Gets the GPG minor version number. 
        /// </summary>
        /// <returns>Minor version number</returns>
        public int Minor
        {
            get { return _minor; }
        }

        /// <summary>
        /// Gets the GPG revision string. 
        /// </summary>
        /// <returns>Revision string</returns>
        public string Revision
        {
            get { return _revision; }
        }
    }
}
