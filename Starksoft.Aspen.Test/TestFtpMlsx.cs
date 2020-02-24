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
using System.IO;
using NUnit.Framework;
using System.Diagnostics;
using Starksoft.Aspen.Ftps;

namespace Starksoft.Aspen.Tests
{
    /// <summary>
    /// Test fixture for Starksoft.Net.FtpsClient.
    /// 
    /// To run the unit tests you will need Nunit.Framework 3.9+ installed.  To execute unit tests from visual
    /// studio you will also need NUnit 3 Test Adapater and Microsoft.NET.Test.SDK installed.  All can be installed
    /// via the Nuget manager and then associated with the project.
    /// 
    /// </summary>
    [TestFixture]

    public class TestFtpMlsx
    {
        /// <summary>
        /// Test the MSLx item class.
        /// </summary>
        /// <param name="line">MLSx line text to parse and load.</param>
        [CLSCompliant(false)]
        [TestCase("Type=file;Size=1830;Modify=19940916055648;Perm=r; hatch.c", "hatch.c" )]
        [TestCase("modify=20120822211414;perm=adfr;size=2101;type=file;unique=16UF3F5;UNIX.group=49440;UNIX.mode=0744;UNIX.owner=49440; iphone_settings_icon.jpg", "iphone_settings_icon.jpg")]
        [TestCase("modify=20030225143801;perm=adfr;size=503;type=file;unique=12U24470006;UNIX.group=0;UNIX.mode=0644;UNIX.owner=0; welcome.msg", "welcome.msg")]
        [TestCase("type=dir;modify=20120825130005; /Test", "/Test")]
        [TestCase("create=20120825130005;lang=en;media-type=TEXT;charset=UTF-8;modify=20030225143801;perm=acdeflmprw;size=3243243332343503;type=file;unique=12U24470006;UNIX.group=100;UNIX.mode=0644;UNIX.owner=1000; wel come.msg", "wel come.msg")]
        [TestCase("create=20120825130005;lang=fr;media-type=TEXT;charset=UTF-7;modify=20030225143801;perm=acdeflmprw;size=3243243332343503;type=pdir;unique=12U24470006;UNIX.group=100;UNIX.mode=0644;UNIX.owner=1000; wel come.msg", "wel come.msg")]
        [TestCase("create=20120825130005;lang=sp;media-type=TEXT;charset=UTF-8;modify=20030225143801;perm=acdeflmprw;size=3243243332343503;type=cdir;unique=12U24470006;UNIX.group=100;UNIX.mode=0644;UNIX.owner=1000; wel come.msg", "wel come.msg")]
        [TestCase("create=20120825130005;lang=en;media-type=TEXT;charset=UTF-8;modify=20030225143801;perm=acdeflmprw;size=3243243332343503;type=dir;unique=12U24470006;UNIX.group=100;UNIX.mode=0644;UNIX.owner=1000; wel come.msg", "wel come.msg")]
        [TestCase("type=file;modify=20180302161100;size=523843732;UNIX.mode=0777;UNIX.owner=testuser;UNIX.group=users; The Rob Schneider Chronicles;S11E24.mp4", "The Rob Schneider Chronicles;S11E24.mp4")]
        [TestCase("type=file;perm=-rw-r--r--;modify=20171222063634;size=16471; mail-editor.png", "mail-editor.png")]
        public void TestMlsxItem(string line, string filename)
        {
            FtpsMlsxItemParser p = new FtpsMlsxItemParser();
            FtpsItem item = p.ParseLine(line);
            Assert.AreEqual( filename, item.Name );
            Console.WriteLine(p.ToString());
        }

        /// <summary>
        /// Test the get MLST method.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="protocol"></param>
        /// <param name="user"></param>
        /// <param name="pwd"></param>
        /// <param name="method"></param>
        [CLSCompliant(false)]
        [TestCase("127.0.0.1", 2121, FtpsSecurityProtocol.None, "test", "test", ListingMethod.Automatic)]
        [TestCase("127.0.0.1", 2121, FtpsSecurityProtocol.None, "test", "test", ListingMethod.List)]
        [TestCase("127.0.0.1", 2121, FtpsSecurityProtocol.None, "test", "test", ListingMethod.ListAl)]
        [TestCase("127.0.0.1", 2121, FtpsSecurityProtocol.None, "test", "test", ListingMethod.Mlsx)]
        public void TestGetFileInfo(string host, int port, FtpsSecurityProtocol protocol,
            string user, string pwd, ListingMethod method)
        {
            using (FtpsClient c = new FtpsClient(host, port, protocol))
            {
                Console.WriteLine("********** BEGINNING **********");
                c.AlwaysAcceptServerCertificate = true;
                c.DirListingMethod = method;
                c.Open(user, pwd);
                Assert.IsTrue(c.IsConnected);
                // get information about the root directory
                FtpsItem m = c.GetFileInfo(".");
                if (m is FtpsMlsxItem)
                    Console.Write(((FtpsMlsxItem)m).ToString());
                else
                    if (m != null)
                        Console.Write(m.ToString());
                Console.WriteLine("********** ENDING **********");
            }
        }

    }
}
