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
using System.Net.Sockets;

namespace Starksoft.Aspen.Proxy
{
    /// <summary>
    /// The type of proxy.
    /// </summary>
    public enum ProxyType
    {
        /// <summary>
        /// No Proxy specified.  Note this option will cause an exception to be thrown if used to create a proxy object by the factory.
        /// </summary>
        None,
        /// <summary>
        /// HTTP Proxy
        /// </summary>
        Http,
        /// <summary>
        /// SOCKS v4 Proxy
        /// </summary>
        Socks4,
        /// <summary>
        /// SOCKS v4a Proxy
        /// </summary>
        Socks4a,
        /// <summary>
        /// SOCKS v5 Proxy
        /// </summary>
        Socks5
    }

    /// <summary>
    /// Factory class for creating new proxy client objects.
    /// </summary>
    /// <remarks>
    /// <code>
    /// // create an instance of the client proxy factory
    /// ProxyClientFactory factory = new ProxyClientFactory();
    ///        
	/// // use the proxy client factory to generically specify the type of proxy to create
    /// // the proxy factory method CreateProxyClient returns an IProxyClient object
    /// IProxyClient proxy = factory.CreateProxyClient(ProxyType.Http, "localhost", 6588);
    ///
	/// // create a connection through the proxy to www.starksoft.com over port 80
    /// System.Net.Sockets.TcpClient tcpClient = proxy.CreateConnection("www.starksoft.com", 80);
    /// </code>
    /// </remarks>
    public class ProxyClientFactory
    {

        /// <summary>
        /// Factory method for creating new proxy client objects.
        /// </summary>
        /// <param name="type">The type of proxy client to create.</param>
        /// <returns>Proxy client object.</returns>
        public IProxyClient CreateProxyClient(ProxyType type)
        {
            if (type == ProxyType.None)
                throw new ArgumentOutOfRangeException("type");

            switch (type)
            {
                case ProxyType.Http:
                    return new HttpProxyClient();
                case ProxyType.Socks4:
                    return new Socks4ProxyClient();
                case ProxyType.Socks4a:
                    return new Socks4aProxyClient();
                case ProxyType.Socks5:
                    return new Socks5ProxyClient();
                default:
                    throw new ProxyException(String.Format("Unknown proxy type {0}.", type.ToString()));
            }
        }        

        /// <summary>
        /// Factory method for creating new proxy client objects using an existing TcpClient connection object.
        /// </summary>
        /// <param name="type">The type of proxy client to create.</param>
        /// <param name="tcpClient">Open TcpClient object.</param>
        /// <returns>Proxy client object.</returns>
        public IProxyClient CreateProxyClient(ProxyType type, TcpClient tcpClient)
        {
            if (type == ProxyType.None)
                throw new ArgumentOutOfRangeException("type");
            
            switch (type)
            {
                case ProxyType.Http:
                    return new HttpProxyClient(tcpClient);
                case ProxyType.Socks4:
                    return new Socks4ProxyClient(tcpClient);
                case ProxyType.Socks4a:
                    return new Socks4aProxyClient(tcpClient);
                case ProxyType.Socks5:
                    return new Socks5ProxyClient(tcpClient);
                default:
                    throw new ProxyException(String.Format("Unknown proxy type {0}.", type.ToString()));
            }
        }        
        
        /// <summary>
        /// Factory method for creating new proxy client objects.  
        /// </summary>
        /// <param name="type">The type of proxy client to create.</param>
        /// <param name="proxyHost">The proxy host or IP address.</param>
        /// <param name="proxyPort">The proxy port number.</param>
        /// <returns>Proxy client object.</returns>
        public IProxyClient CreateProxyClient(ProxyType type, string proxyHost, int proxyPort)
        {
            if (type == ProxyType.None)
                throw new ArgumentOutOfRangeException("type");
            
            switch (type)
            {
                case ProxyType.Http:
                    return new HttpProxyClient(proxyHost, proxyPort);
                case ProxyType.Socks4:
                    return new Socks4ProxyClient(proxyHost, proxyPort);
                case ProxyType.Socks4a:
                    return new Socks4aProxyClient(proxyHost, proxyPort);
                case ProxyType.Socks5:
                    return new Socks5ProxyClient(proxyHost, proxyPort);
                default:
                    throw new ProxyException(String.Format("Unknown proxy type {0}.", type.ToString()));
            }
        }

        /// <summary>
        /// Factory method for creating new proxy client objects.  
        /// </summary>
        /// <param name="type">The type of proxy client to create.</param>
        /// <param name="proxyHost">The proxy host or IP address.</param>
        /// <param name="proxyPort">The proxy port number.</param>
        /// <param name="proxyUsername">The proxy username.  This parameter is only used by Http, Socks4 and Socks5 proxy objects.</param>
        /// <param name="proxyPassword">The proxy user password.  This parameter is only used Http, Socks5 proxy objects.</param>
        /// <returns>Proxy client object.</returns>
        public IProxyClient CreateProxyClient(ProxyType type, string proxyHost, int proxyPort, string proxyUsername, string proxyPassword)
        {
            if (type == ProxyType.None)
                throw new ArgumentOutOfRangeException("type");

            switch (type)
            {
                case ProxyType.Http:
                    return new HttpProxyClient(proxyHost, proxyPort, proxyUsername, proxyPassword);
                case ProxyType.Socks4:
                    return new Socks4ProxyClient(proxyHost, proxyPort, proxyUsername);
                case ProxyType.Socks4a:
                    return new Socks4aProxyClient(proxyHost, proxyPort, proxyUsername);
                case ProxyType.Socks5:
                    return new Socks5ProxyClient(proxyHost, proxyPort, proxyUsername, proxyPassword);
                default:
                    throw new ProxyException(String.Format("Unknown proxy type {0}.", type.ToString()));
            }
        }

        /// <summary>
        /// Factory method for creating new proxy client objects.  
        /// </summary>
        /// <param name="type">The type of proxy client to create.</param>
        /// <param name="tcpClient">Open TcpClient object.</param>
        /// <param name="proxyHost">The proxy host or IP address.</param>
        /// <param name="proxyPort">The proxy port number.</param>
        /// <param name="proxyUsername">The proxy username.  This parameter is only used by Http, Socks4 and Socks5 proxy objects.</param>
        /// <param name="proxyPassword">The proxy user password.  This parameter is only used Http, Socks5 proxy objects.</param>
        /// <returns>Proxy client object.</returns>
        public IProxyClient CreateProxyClient(ProxyType type, TcpClient tcpClient, string proxyHost, int proxyPort, string proxyUsername, string proxyPassword)
        {
            IProxyClient c = CreateProxyClient(type, proxyHost, proxyPort, proxyUsername, proxyPassword);
            c.TcpClient = tcpClient;
            return c;
        }


    }



}
