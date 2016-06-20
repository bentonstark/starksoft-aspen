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
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace Starksoft.Aspen.Ftps
{
    /// <summary>
    /// Event arguments to facilitate the FtpsClient transfer progress and complete events.
    /// </summary>
    public class ValidateServerCertificateEventArgs : EventArgs
    {

        private X509Certificate2 _certificate;
        private X509Chain _chain;
        private SslPolicyErrors _policyErrors;
        private bool _isCertificateValid;

        /// <summary>
        /// ValidateServerCertificateEventArgs constructor.
        /// </summary>
        /// <param name="certificate">X.509 certificate object.</param>
        /// <param name="chain">X.509 certificate chain.</param>
        /// <param name="policyErrors">SSL policy errors.</param>
        public ValidateServerCertificateEventArgs(X509Certificate2 certificate, X509Chain chain, SslPolicyErrors policyErrors)
        {
            _certificate = certificate;
            _chain = chain;
            _policyErrors = policyErrors;
        }

        /// <summary>
        /// The X.509 version 3 server certificate.
        /// </summary>
        public X509Certificate2 Certificate
        {
            get { return _certificate; }
        }

        /// <summary>
        /// Server chain building engine for server certificate.
        /// </summary>
        public X509Chain Chain
        {
            get { return _chain; }
        }

        /// <summary>
        /// Enumeration representing SSL (Secure Socket Layer) errors.
        /// </summary>
        public SslPolicyErrors PolicyErrors
        {
            get { return _policyErrors; }
        }

        /// <summary>
        /// Boolean value indicating if the server certificate is valid and can
        /// be accepted by the FtpsClient object.
        /// </summary>
        /// <remarks>
        /// Default value is false which results in certificate being rejected and the SSL
        /// connection abandoned.  Set this value to true to accept the server certificate 
        /// otherwise the SSL connection will be closed.
        /// </remarks>
        public bool IsCertificateValid
        {
            get { return _isCertificateValid; }
            set { _isCertificateValid = value; }
        }
    }
}
