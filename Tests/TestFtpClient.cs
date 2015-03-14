/*
* Copyright (c) 2015 Benton Stark
* This file is part of the Starksoft Aspen library.
*
* Starksoft Aspen is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
* 
* Starksoft Aspen is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
* 
* You should have received a copy of the GNU General Public License
* along with Starksoft Aspen.  If not, see <http://www.gnu.org/licenses/>.
*   
*/

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
    /// </summary>
    [TestFixture]
    public class TestFtpsClient
    {
        [CLSCompliant(false)]
        [TestCase("127.0.0.1", 990, FtpsSecurityProtocol.Tls1OrSsl3Implicit, "test", "test", "FileZilla")]
        public void TestUserReportedError1(string host, int port, FtpsSecurityProtocol protocol,
            string user, string pwd, string server)
        {
            FtpsClient ftp = new FtpsClient("127.0.0.1", port, protocol);
            ftp.AlwaysAcceptServerCertificate = true;
            ftp.Open("test", "test");
            ftp.GetFile("luna.h", "c:\\temp\\luna1-2.h", FileAction.Create);
            ftp.Close();
        }

        [CLSCompliant(false)]
        [TestCase("127.0.0.1", 21, FtpsSecurityProtocol.Tls1OrSsl3Explicit, "test", "test", "FileZilla")]
        public void TestUserReportedError2(string host, int port, FtpsSecurityProtocol protocol,
            string user, string pwd, string server)
        {
            FtpsClient ftp = new FtpsClient("127.0.0.1", port, protocol);
            ftp.AlwaysAcceptServerCertificate = true;
            ftp.Open("test", "test");
            ftp.GetFile("luna.h", "luna1-2.h", FileAction.Create);
            ftp.Close();
        }

        /// <summary>
        /// Test the ABORt command.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="protocol"></param>
        /// <param name="user"></param>
        /// <param name="pwd"></param>
        /// <param name="server"></param>
        [CLSCompliant(false)]
        [TestCase("ftp.NetBSD.org", 21, FtpsSecurityProtocol.None, "ftp", "ftp", "NetBSD-ftpd")]
        public void TestAbort(string host, int port, FtpsSecurityProtocol protocol,
            string user, string pwd, string server)
        {
            using (FtpsClient c = new FtpsClient(host, port, protocol))
            {
                c.AlwaysAcceptServerCertificate = true;
                c.Open(user, pwd);
                Assert.IsTrue(c.IsConnected);
                c.Abort();
            }
        }

        /// <summary>
        /// Test the ALLOcate Storage command.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="protocol"></param>
        /// <param name="user"></param>
        /// <param name="pwd"></param>
        /// <param name="server"></param>
        [CLSCompliant(false)]
        [TestCase("ftp.NetBSD.org", 21, FtpsSecurityProtocol.None, "ftp", "ftp", "NetBSD-ftpd")]
        public void TestAllocateStorage(string host, int port, FtpsSecurityProtocol protocol,
            string user, string pwd, string server)
        {
            using (FtpsClient c = new FtpsClient(host, port, protocol))
            {
                c.AlwaysAcceptServerCertificate = true;
                c.Open(user, pwd);
                Assert.IsTrue(c.IsConnected);
                c.AllocateStorage(1024 * 16);
            }
        }
        
        /// <summary>
        /// Test Change Working Directory CWD.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="protocol"></param>
        /// <param name="user"></param>
        /// <param name="pwd"></param>
        /// <param name="server"></param>
        [CLSCompliant(false)]
        [TestCase("127.0.0.1", 21, FtpsSecurityProtocol.None, "test", "test", "FileZilla")]
        public void TestChangeDirectory(string host, int port, FtpsSecurityProtocol protocol,
            string user, string pwd, string server)
        {
            using (FtpsClient c = new FtpsClient(host, port, protocol))
            {
                c.AlwaysAcceptServerCertificate = true;
                c.Open(user, pwd);
                Assert.IsTrue(c.IsConnected);
                string dir1 = Guid.NewGuid().ToString();
                string dir2 = Guid.NewGuid().ToString();
                string dir3 = Guid.NewGuid().ToString();
                string dir4 = Guid.NewGuid().ToString();

                // create the directories and change into them
                c.MakeDirectory(dir1);
                c.ChangeDirectory(dir1);
                c.MakeDirectory(dir2);
                c.ChangeDirectory(dir2);
                c.MakeDirectory(dir3);
                c.ChangeDirectory(dir3);
                c.MakeDirectory(dir4);

                c.ChangeDirectoryUp();
                c.ChangeDirectoryUp();
                c.ChangeDirectoryUp();
                c.ChangeDirectoryUp();

                // try changing directory using a full path which does 
                // not work for all FTP servers
                c.ChangeDirectory(String.Format("{0}/{1}/{2}/{3}", dir1, dir2, dir3, dir4));
                c.ChangeDirectory("//");

                // try changing directory using multipath command which should work
                // for all FTP servers
                c.ChangeDirectoryMultiPath(String.Format("{0}/{1}/{2}/{3}", dir1, dir2, dir3, dir4));
                c.ChangeDirectoryUp();
  
                // delete the temporary directories
                c.DeleteDirectory(dir4);
                c.ChangeDirectoryUp();
                c.DeleteDirectory(dir3);
                c.ChangeDirectoryUp();
                c.DeleteDirectory(dir2);
                c.ChangeDirectoryUp();
                c.DeleteDirectory(dir1);
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Test the FtpsClient
        /// </summary>
        [CLSCompliant(false)]
        [TestCase("127.0.0.1", 21, FtpsSecurityProtocol.None, "ftp", "ftp", "FileZilla")]
        public void TestOpen(string host, int port, FtpsSecurityProtocol protocol, 
            string user, string pwd, string server)
        {
            FtpsClient c = new FtpsClient(host, port, protocol);
            c.AlwaysAcceptServerCertificate = true;
            c.Open(user, pwd);
            Assert.IsTrue(c.IsConnected);
            c.Close();
        }

        /// <summary>
        /// Open multiple, simultaneous connections to FTP servers.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="protocol"></param>
        /// <param name="user"></param>
        /// <param name="pwd"></param>
        /// <param name="server"></param>
        /// <param name="connections"></param>
        [CLSCompliant(false)]
        [TestCase("127.0.0.1", 21, FtpsSecurityProtocol.Tls1Explicit, "test", "test", "FileZilla", 5)]
        [TestCase("127.0.0.1", 21, FtpsSecurityProtocol.Ssl3Explicit, "test", "test", "FileZilla", 5)]
        [TestCase("127.0.0.1", 21, FtpsSecurityProtocol.None, "test", "test", "FileZilla", 5)]
        public void TestMultiOpen(string host, int port, FtpsSecurityProtocol protocol,
            string user, string pwd, string server, int connections)
        {
            FtpsClient[] lst = new FtpsClient[connections];

            for(int i=0; i< lst.Length; i++)
            {
                FtpsClient c = new FtpsClient(host, port, protocol);
                c.AlwaysAcceptServerCertificate = true;
                c.Open(user, pwd);
                Assert.IsTrue(c.IsConnected);
                lst[i] = c;
            }

            for (int i = 0; i < lst.Length; i++)
            {
                lst[i].Close();
            }
            
        }

        /// <summary>
        /// Open multiple connections to local FTP server to test reliability.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="protocol"></param>
        /// <param name="user"></param>
        /// <param name="pwd"></param>
        /// <param name="server"></param>
        /// <param name="connections"></param>
        [CLSCompliant(false)]
        [TestCase("127.0.0.1", 21, FtpsSecurityProtocol.Tls1Explicit, "test", "test", "FileZilla", 10)]
        [TestCase("127.0.0.1", 21, FtpsSecurityProtocol.Ssl3Explicit, "test", "test", "FileZilla", 10)]
        [TestCase("127.0.0.1", 21, FtpsSecurityProtocol.None, "test", "test", "FileZilla", 10)]
        [TestCase("127.0.0.1", 900, FtpsSecurityProtocol.Tls1Implicit, "testssl", "test", "FileZilla", 1)]
        [TestCase("127.0.0.1", 900, FtpsSecurityProtocol.Ssl3Implicit, "testssl", "test", "FileZilla", 1)]
        public void TestRepeatedOpen(string host, int port, FtpsSecurityProtocol protocol,
            string user, string pwd, string server, int connections)
        {
            for (int i = 0; i < connections; i++)
            {
                using (FtpsClient c = new FtpsClient(host, port, protocol))
                {
                    Debug.WriteLine("********** START **********");
                    c.AlwaysAcceptServerCertificate = true;
                    c.Open(user, pwd);
                    Assert.IsTrue(c.IsConnected);
                    Debug.WriteLine("********** PASSED **********");
                }
            }
        }
        


        /// <summary>
        /// Test the open using command.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="protocol"></param>
        /// <param name="user"></param>
        /// <param name="pwd"></param>
        /// <param name="server"></param>
        [CLSCompliant(false)]
        [TestCase("127.0.0.1", 21, FtpsSecurityProtocol.None, "test", "test", "FileZilla")]
        public void TestOpenUsing(string host, int port, FtpsSecurityProtocol protocol,
            string user, string pwd, string server)
        {
            using (FtpsClient c = new FtpsClient(host, port, protocol))
            {
                c.AlwaysAcceptServerCertificate = true;
                c.Open(user, pwd);
                Assert.IsTrue(c.IsConnected);
            }
        }



        /// <summary>
        /// Test the SYSTem command.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="protocol"></param>
        /// <param name="user"></param>
        /// <param name="pwd"></param>
        /// <param name="server"></param>
        [CLSCompliant(false)]
        [TestCase("127.0.0.1", 21, FtpsSecurityProtocol.None, "test", "test", "FileZilla")]
        public void TestGetSystemType(string host, int port, FtpsSecurityProtocol protocol,
            string user, string pwd, string server)
        {
            using (FtpsClient c = new FtpsClient(host, port, protocol))
            {
                c.AlwaysAcceptServerCertificate = true;
                c.Open(user, pwd);
                Assert.IsTrue(c.IsConnected);
                string r = c.GetSystemType();
                Assert.IsNotNullOrEmpty(r);
                Debug.WriteLine("RESULT: " + r);
            }
        }


        /// <summary>
        /// Test the STOR command.  
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="protocol"></param>
        /// <param name="user"></param>
        /// <param name="pwd"></param>
        /// <param name="server"></param>
        [CLSCompliant(false)]
        [TestCase("127.0.0.1", 21, FtpsSecurityProtocol.None, "test", "test", "FileZilla")]
        public void TestPutFileUnique(string host, int port, FtpsSecurityProtocol protocol,
            string user, string pwd, string server)
        {
            using (FtpsClient c = new FtpsClient(host, port, protocol))
            {
                c.AlwaysAcceptServerCertificate = true;
                c.Open(user, pwd);
                Assert.IsTrue(c.IsConnected);

                byte[] bytes = new byte[1024];
                MemoryStream m = new MemoryStream(bytes);
                string fname = c.PutFileUnique(m);
                c.DeleteFile(fname);
            }
        }

        /// <summary>
        /// Test the STORe command.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="protocol"></param>
        /// <param name="user"></param>
        /// <param name="pwd"></param>
        /// <param name="server"></param>
        /// <param name="fileSize"></param>
        [CLSCompliant(false)]
        [TestCase("127.0.0.1", 21, FtpsSecurityProtocol.None, "test", "test", "FileZilla", 1024 * 1024)]
        public void TestPutFileCreate(string host, int port, FtpsSecurityProtocol protocol,
            string user, string pwd, string server, int fileSize)
        {
            using (FtpsClient c = new FtpsClient(host, port, protocol))
            {
                c.AlwaysAcceptServerCertificate = true;
                c.Open(user, pwd);
                Assert.IsTrue(c.IsConnected);

                MemoryStream m1 = GetRandom(fileSize);
                MemoryStream m2 = GetRandom(fileSize);
                string fname = GetGuidString();

                try
                {
                    c.PutFile(m1, fname, FileAction.Create);

                    Assert.IsTrue(c.Exists(fname));

                    MemoryStream o1 = new MemoryStream();
                    c.GetFile(fname, o1, false);
                    o1.Position = 0;
                    
                    //compare bytes to verify
                    Assert.IsTrue(Compare(m1, o1));

                    // put a second file which should overwrite the first file
                    c.PutFile(m1, fname, FileAction.Create);

                    Assert.IsTrue(c.Exists(fname));

                    MemoryStream o2 = new MemoryStream();
                    c.GetFile(fname, o2, false);
                    o1.Position = 0;

                    //compare bytes to verify
                    Assert.IsTrue(Compare(m1, o2));
                }
                finally
                {
                    c.DeleteFile(fname);
                }

            }
        }

        /// <summary>
        /// Test the STORe command.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="protocol"></param>
        /// <param name="user"></param>
        /// <param name="pwd"></param>
        /// <param name="server"></param>
        /// <param name="fileSize"></param>
        [CLSCompliant(false)]
        [TestCase("127.0.0.1", 21, FtpsSecurityProtocol.None, "test", "test", "FileZilla", 1024 * 1024)]
        public void TestPutFileCreateNew(string host, int port, FtpsSecurityProtocol protocol,
            string user, string pwd, string server, int fileSize)
        {
            using (FtpsClient c = new FtpsClient(host, port, protocol))
            {
                c.AlwaysAcceptServerCertificate = true;
                c.Open(user, pwd);
                Assert.IsTrue(c.IsConnected);

                MemoryStream m1 = GetRandom(fileSize);
                MemoryStream m2 = GetRandom(fileSize);
                string fname = GetGuidString();
                bool fail = false;

                try
                {
                    c.PutFile(m1, fname, FileAction.Create);

                    Assert.IsTrue(c.Exists(fname));

                    MemoryStream o1 = new MemoryStream();
                    c.GetFile(fname, o1, false);
                    o1.Position = 0;

                    //compare bytes to verify
                    Assert.IsTrue(Compare(m1, o1));

                    c.PutFile(m1, fname, FileAction.Create);

                    try
                    {
                        // put a second time which should cause an exception to be thrown
                        // since there is an existing file already on the server
                        c.PutFile(m1, fname, FileAction.CreateNew);
                    }
                    catch (FtpsDataTransferException)
                    {
                        fail = true;
                    }

                    Assert.IsTrue(fail);

                }
                finally
                {
                    c.DeleteFile(fname);
                }

            }
        }

        /// <summary>
        /// Test the STORe command.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="protocol"></param>
        /// <param name="user"></param>
        /// <param name="pwd"></param>
        /// <param name="server"></param>
        /// <param name="fileSize"></param>
        [CLSCompliant(false)]
        [TestCase("127.0.0.1", 21, FtpsSecurityProtocol.None, "test", "test", "FileZilla", 1024 * 1024)]
        public void TestPutFileCreateOrAppend(string host, int port, FtpsSecurityProtocol protocol,
            string user, string pwd, string server, int fileSize)
        {
            using (FtpsClient c = new FtpsClient(host, port, protocol))
            {
                c.AlwaysAcceptServerCertificate = true;
                c.Open(user, pwd);
                Assert.IsTrue(c.IsConnected);

                MemoryStream m1 = GetRandom(fileSize);
                MemoryStream m2 = GetRandom(fileSize);
                string fname = GetGuidString();

                try
                {
                    c.PutFile(m1, fname, FileAction.Create);

                    Assert.IsTrue(c.Exists(fname));

                    MemoryStream o1 = new MemoryStream();
                    c.GetFile(fname, o1, false);
                    o1.Position = 0;

                    //compare bytes to verify
                    Assert.IsTrue(Compare(m1, o1));

                    // put a second file which should append to the first file
                    c.PutFile(m2, fname, FileAction.CreateOrAppend);

                    Assert.IsTrue(c.Exists(fname));

                    MemoryStream o2 = new MemoryStream();
                    c.GetFile(fname, o2, false);
                    o1.Position = 0;

                    //compare bytes to verify m1 and m2 were appended together
                    Assert.IsTrue(Compare(m1, m2, o2));
                }
                finally
                {
                    c.DeleteFile(fname);
                }

            }
        }

        /// <summary>
        /// Test the RESTart command.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="protocol"></param>
        /// <param name="user"></param>
        /// <param name="pwd"></param>
        /// <param name="server"></param>
        /// <param name="fileSize"></param>
        [CLSCompliant(false)]
        [TestCase("127.0.0.1", 21, FtpsSecurityProtocol.None, "test", "test", "FileZilla", 1024 * 1024)]
        public void TestPutFileResume(string host, int port, FtpsSecurityProtocol protocol,
            string user, string pwd, string server, int fileSize)
        {
            using (FtpsClient c = new FtpsClient(host, port, protocol))
            {
                c.AlwaysAcceptServerCertificate = true;
                c.Open(user, pwd);
                Assert.IsTrue(c.IsConnected);

                MemoryStream m1 = GetRandom(fileSize);
                MemoryStream m2 = GetRandom(fileSize);

                byte[] a = m1.ToArray();
                byte[] b = m2.ToArray();
                byte[] l = new byte[a.Length + b.Length];
                Array.Copy(a, l, a.Length);
                Array.Copy(b, 0, l, a.Length, b.Length);

                // m3 is m1 + m2
                MemoryStream m3 = new MemoryStream(l);

                string fname = GetGuidString();

                try
                {
                    c.PutFile(m1, fname, FileAction.Create);

                    Assert.IsTrue(c.Exists(fname));

                    MemoryStream o1 = new MemoryStream();
                    c.GetFile(fname, o1, false);
                    o1.Position = 0;

                    //compare bytes to verify
                    Assert.IsTrue(Compare(m1, o1));

                    // put m3 as a resume file
                    c.PutFile(m3, fname, FileAction.Resume);

                    Assert.IsTrue(c.Exists(fname));

                    MemoryStream o2 = new MemoryStream();
                    c.GetFile(fname, o2, false);
                    o1.Position = 0;

                    //compare bytes to verify
                    Assert.IsTrue(Compare(m3, o2));
                }
                finally
                {
                    c.DeleteFile(fname);
                }

            }
        }

        /// <summary>
        /// Test the RESTart command.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="protocol"></param>
        /// <param name="user"></param>
        /// <param name="pwd"></param>
        /// <param name="server"></param>
        /// <param name="fileSize"></param>
        [CLSCompliant(false)]
        [TestCase("127.0.0.1", 21, FtpsSecurityProtocol.None, "test", "test", "FileZilla", 1024 * 1024)]
        public void TestPutFileResumeCreate(string host, int port, FtpsSecurityProtocol protocol,
            string user, string pwd, string server, int fileSize)
        {
            using (FtpsClient c = new FtpsClient(host, port, protocol))
            {
                c.AlwaysAcceptServerCertificate = true;
                c.Open(user, pwd);
                Assert.IsTrue(c.IsConnected);

                MemoryStream m1 = GetRandom(fileSize);
                MemoryStream m2 = GetRandom(fileSize);
                string fname = GetGuidString();

                try
                {
                    // attempt to put a new file on the system which will result in 
                    // no resume action but rather a create action to be performed
                    c.PutFile(m1, fname, FileAction.ResumeOrCreate);

                    Assert.IsTrue(c.Exists(fname));

                    MemoryStream o1 = new MemoryStream();
                    c.GetFile(fname, o1, false);
                    o1.Position = 0;

                    //compare bytes to verify
                    Assert.IsTrue(Compare(m1, o1));

                    // try to resume the file after it has already been transmitted
                    // this should result in a test on the file lengths and no action
                    // being performed by the FTP client
                    c.PutFile(m1, fname, FileAction.Resume);

                }
                finally
                {
                    c.DeleteFile(fname);
                }

            }
        }

        /// <summary>
        /// Test the QUOTe command.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="protocol"></param>
        /// <param name="user"></param>
        /// <param name="pwd"></param>
        /// <param name="server"></param>
        [CLSCompliant(false)]
        [TestCase("127.0.0.1", 21, FtpsSecurityProtocol.None, "test", "test", "FileZilla")]
        public void TestQuote(string host, int port, FtpsSecurityProtocol protocol,
            string user, string pwd, string server)
        {
            using (FtpsClient c = new FtpsClient(host, port, protocol))
            {
                c.AlwaysAcceptServerCertificate = true;
                c.Open(user, pwd);
                Assert.IsTrue(c.IsConnected);
                string result = c.Quote("HELP");

            }
        }

        /// <summary>
        /// Test the NOOPeration command.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="protocol"></param>
        /// <param name="user"></param>
        /// <param name="pwd"></param>
        /// <param name="server"></param>
        [CLSCompliant(false)]
        [TestCase("127.0.0.1", 21, FtpsSecurityProtocol.None, "test", "test", "FileZilla")]
        public void TestNoOp(string host, int port, FtpsSecurityProtocol protocol,
            string user, string pwd, string server)
        {
            using (FtpsClient c = new FtpsClient(host, port, protocol))
            {
                c.AlwaysAcceptServerCertificate = true;
                c.Open(user, pwd);
                Assert.IsTrue(c.IsConnected);
                c.NoOperation();
            }
        }

        /// <summary>
        /// Test the OPTS command.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="protocol"></param>
        /// <param name="user"></param>
        /// <param name="pwd"></param>
        /// <param name="server"></param>
        [CLSCompliant(false)]
        [TestCase("127.0.0.1", 21, FtpsSecurityProtocol.None, "test", "test", "FileZilla")]
        public void TestSetOptions(string host, int port, FtpsSecurityProtocol protocol,
            string user, string pwd, string server)
        {
            using (FtpsClient c = new FtpsClient(host, port, protocol))
            {
                c.AlwaysAcceptServerCertificate = true;
                c.Open(user, pwd);
                Assert.IsTrue(c.IsConnected);
                c.SetOptions("UTF8 OFF");
            }
        }

        /// <summary>
        /// Test the UTF-8 On command.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="protocol"></param>
        /// <param name="user"></param>
        /// <param name="pwd"></param>
        /// <param name="server"></param>
        [CLSCompliant(false)]
        [TestCase("127.0.0.1", 21, FtpsSecurityProtocol.None, "test", "test", "FileZilla")]
        public void TestUtf8On(string host, int port, FtpsSecurityProtocol protocol,
            string user, string pwd, string server)
        {
            using (FtpsClient c = new FtpsClient(host, port, protocol))
            {
                c.AlwaysAcceptServerCertificate = true;
                c.Open(user, pwd);
                Assert.IsTrue(c.IsConnected);
                c.SetUtf8On();
                Assert.IsTrue(c.Encoding == Encoding.UTF8);
            }
        }

        /// <summary>
        /// Test the UTF-8 Off command.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="protocol"></param>
        /// <param name="user"></param>
        /// <param name="pwd"></param>
        /// <param name="server"></param>
        [CLSCompliant(false)]
        [TestCase("127.0.0.1", 21, FtpsSecurityProtocol.None, "test", "test", "FileZilla")]
        public void TestUtf8On2(string host, int port, FtpsSecurityProtocol protocol,
            string user, string pwd, string server)
        {
            using (FtpsClient c = new FtpsClient(host, port, protocol))
            {
                c.AlwaysAcceptServerCertificate = true;
                c.Open(user, pwd);
                Assert.IsTrue(c.IsConnected);
                c.SetUtf8Off();
                Assert.IsTrue(c.Encoding == Encoding.ASCII);
            }
        }



        private static string GetGuidString()
        {
            return Guid.NewGuid().ToString();
        }

        private static MemoryStream GetRandom(int size)
        {
            byte[] bytes = new byte[size];
            Random r = new Random();
            r.NextBytes(bytes);
            return new MemoryStream(bytes);
        }

        /// <summary>
        /// Test directory listing command.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="protocol"></param>
        /// <param name="user"></param>
        /// <param name="pwd"></param>
        /// <param name="server"></param>
        [CLSCompliant(false)]
        [TestCase("127.0.0.1", 21, FtpsSecurityProtocol.None, "ftp", "ftp", "FileZilla")]
        public void TestGetDirList(string host, int port, FtpsSecurityProtocol protocol,
            string user, string pwd, string server)
        {
            using (FtpsClient c = new FtpsClient(host, port, protocol))
            {
                c.AlwaysAcceptServerCertificate = true;
                c.Open(user, pwd);
                Assert.IsTrue(c.IsConnected);
                FtpsItemCollection lst = c.GetDirList();

                Debug.WriteLine("===================================================");
                Debug.WriteLine("DIRECTORY DUMP");
                Debug.WriteLine("===================================================");
                foreach (FtpsItem item in lst)
                {
                    Debug.WriteLine(item.RawText);
                    Debug.WriteLine(item.ToString());
                }
                Debug.WriteLine("===================================================");

            }
        }



        private static bool Compare(MemoryStream a, MemoryStream b)
        {
            a.Position = 0;
            b.Position = 0;
            return Compare(a.ToArray(), b.ToArray());
        }

        private static bool Compare(MemoryStream a, MemoryStream b, MemoryStream c)
        {
            a.Position = 0;
            b.Position = 0;
            c.Position = 0;

            byte[] j = a.ToArray();
            byte[] k = b.ToArray();
            byte[] l = new byte[j.Length + k.Length];
            Array.Copy(j, l, j.Length);
            Array.Copy(k, 0, l, j.Length, k.Length);

            return Compare(c.ToArray(), l); 
        }


        private static bool Compare(byte[] a, byte[] b)
        {
            if (a.Length != a.Length)
                return false;

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                    return false;
            }

            return true;
        }


    }
}
