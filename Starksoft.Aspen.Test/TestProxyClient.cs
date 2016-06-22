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



// disable visual studio comment on public function warnings
// for unit tests
#pragma warning disable 1591

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NUnit.Framework;
using System.Diagnostics;
using System.Net.Sockets;
using Starksoft.Aspen.Proxy;

namespace Starksoft.Aspen.Tests
{
    /// <summary>
    /// Test fixture for Proxy classes.  A proxy server supporting SOCKS and HTTP must be running
    /// on the local host for the unit tests to work.  To run the unit tests you will need Nunit 2.5.10 installed.
    /// For the Tor specific test you need to have Tor installed and listening on port 9050.
    /// 
    /// For those that are wondering why I am using nunit 2.x.  There are two reasons:  1) never versions are not
    /// supported in monodevelop for Linux.  2) nunit 3.x is far more of a pain to setup and run at the moment.
    /// This project is being maintained and tested under two environments so I need a unit test version that works
    /// for both.
    /// 
    /// 
    /// proxy           listen
    /// protocol        port
    /// ========        ====
    /// SOCKSv4         1080
    /// SOCKSv4a        1080
    /// SOCKSv5         1080
    /// HTTP            8080
    /// 
    /// </summary>
    [TestFixture]
    public class TestProxyClient
    {
        [CLSCompliant(false)]
        [TestCase("localhost", 9050, "httpbin.org", 80)]
        public void TestSocks5CreateConnectionTor(string proxyHost, int proxyPort, string destHost, int destPort)
        {

            Socks5ProxyClient p = new Socks5ProxyClient();
            p.ProxyHost = proxyHost;
            p.ProxyPort = proxyPort;
            p.ProxyUserName = "";
            p.ProxyPassword = "";
            TcpClient c = p.CreateConnection(destHost, destPort);

            byte[] sendBuf = System.Text.ASCIIEncoding.ASCII.GetBytes("GET / HTTP/1.1\n");
            c.GetStream().Write(sendBuf, 0, sendBuf.Length);
            byte[] recvBuf = new byte[1024];
            c.GetStream().Read(recvBuf, 0, recvBuf.Length);
            Console.Out.WriteLine(System.Text.ASCIIEncoding.ASCII.GetString(recvBuf));
            c.Close();
        }

        [CLSCompliant(false)]
        [TestCase("localhost", 1080, "httpbin.org", 80)]
        public void TestSocks5CreateConnection(string proxyHost, int proxyPort, string destHost, int destPort)
        {

            Socks5ProxyClient p = new Socks5ProxyClient();
            p.ProxyHost = proxyHost;
            p.ProxyPort = proxyPort;
            TcpClient c = p.CreateConnection(destHost, destPort);

            byte[] sendBuf = System.Text.ASCIIEncoding.ASCII.GetBytes("GET / HTTP/1.1\n");
            c.GetStream().Write(sendBuf, 0, sendBuf.Length);
            byte[] recvBuf = new byte[1024];
            c.GetStream().Read(recvBuf, 0, recvBuf.Length);
            Console.Out.WriteLine(System.Text.ASCIIEncoding.ASCII.GetString(recvBuf));
            c.Close();
        }

        [CLSCompliant(false)]
        [TestCase("localhost", 8080, "httpbin.org", 80)]
        public void TestHttpCreateConnection(string proxyHost, int proxyPort, string destHost, int destPort)
        {

            HttpProxyClient p = new HttpProxyClient();
            p.ProxyHost = proxyHost;
            p.ProxyPort = proxyPort;
            TcpClient c = p.CreateConnection(destHost, destPort);

            byte[] sendBuf = System.Text.ASCIIEncoding.ASCII.GetBytes("GET / HTTP/1.1\n");
            c.GetStream().Write(sendBuf, 0, sendBuf.Length);
            byte[] recvBuf = new byte[1024];
            c.GetStream().Read(recvBuf, 0, recvBuf.Length);
            Console.Out.WriteLine(System.Text.ASCIIEncoding.ASCII.GetString(recvBuf));
            c.Close();
        }

        [CLSCompliant(false)]
        [TestCase("localhost", 1080, "httpbin.org", 80)]
        public void TestSocks4CreateConnection(string proxyHost, int proxyPort, string destHost, int destPort)
        {

            Socks4ProxyClient p = new Socks4ProxyClient();
            p.ProxyHost = proxyHost;
            p.ProxyPort = proxyPort;
            TcpClient c = p.CreateConnection(destHost, destPort);

            byte[] sendBuf = System.Text.ASCIIEncoding.ASCII.GetBytes("GET / HTTP/1.1\n");
            c.GetStream().Write(sendBuf, 0, sendBuf.Length);
            byte[] recvBuf = new byte[1024];
            c.GetStream().Read(recvBuf, 0, recvBuf.Length);
            Console.Out.WriteLine(System.Text.ASCIIEncoding.ASCII.GetString(recvBuf));
            c.Close();
        }

        [CLSCompliant(false)]
        [TestCase("localhost", 1080, "httpbin.org", 80)]
        public void TestSocks4aCreateConnection(string proxyHost, int proxyPort, string destHost, int destPort)
        {

            Socks4aProxyClient p = new Socks4aProxyClient();
            p.ProxyHost = proxyHost;
            p.ProxyPort = proxyPort;
            TcpClient c = p.CreateConnection(destHost, destPort);

            byte[] sendBuf = System.Text.ASCIIEncoding.ASCII.GetBytes("GET / HTTP/1.1\n");
            c.GetStream().Write(sendBuf, 0, sendBuf.Length);
            byte[] recvBuf = new byte[1024];
            c.GetStream().Read(recvBuf, 0, recvBuf.Length);
            Console.Out.WriteLine(System.Text.ASCIIEncoding.ASCII.GetString(recvBuf));
            c.Close();
        }

    }
}
