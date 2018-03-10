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
using System.IO;
using NUnit.Framework;
using Starksoft.Aspen.Ftps;

namespace Starksoft.Aspen.Tests
{
    /// <summary>
    /// Test fixture for Starksoft.Net.FtpsClient.
    /// </summary>
    /// <remarks>
    /// 
    /// This is the test class for the FtpsClient.  Testing assumes a FileZilla local FTP server running either under
    /// Windows or Linux/Wine.  The unit tests have been tested under Windows 10 in Visual Studio 2017.
    /// The FileZilla server must have an account created with user "test" and password "test".  In addition the 
    /// server must be configured to listen on port 2121 for FTP and explicit FTPS connections and port 9990 for
    /// implicit FTPS connections.  The server certificate can be self signed.  The home directory for user test
    /// must all all file operations (read, write, append, create dir, delete, dir, etc).
    /// 
    /// The tests can be run with IPv6 or IPv4 based on the FTP_HOST and NETWORK_PROTOCOL const settings.  Other
    /// defaults such as FTP user name, password and ports can be set by changing the constants.
    /// 
    /// To run the unit tests you will need Nunit.Framework 3.9+ installed.  To execute unit tests from visual
    /// studio you will also need NUnit 3 Test Adapater and Microsoft.NET.Test.SDK installed.  All can be installed
    /// via the Nuget manager and then associated with the project.
    /// 
    /// </remarks>
    [TestFixture]
    public class TestFtpsClient
    {
        //private const string FTP_HOST = "::1";
        //private const NetworkVersion NETWORK_PROTOCOL = NetworkVersion.IPv6;
        private const string FTP_HOST = "127.0.0.1";
        private const NetworkVersion NETWORK_PROTOCOL = NetworkVersion.IPv4;
        private const int FTP_STD_PORT = 2121;
        private const int FTP_TLS_IMPLICIT_PORT = 9990;
        private const string FTP_USER = "test";
        private const string FTP_PASS = "test";
        private const int DEFAULT_FILE_SIZE = 100; // bytes

        [Test]
        public void TestUserReportedError1()
        {
            FtpsClient ftp = new FtpsClient(FTP_HOST, FTP_STD_PORT, FtpsSecurityProtocol.Tls12Explicit);
            ftp.NetworkProtocol = NETWORK_PROTOCOL;
            ftp.AlwaysAcceptServerCertificate = true;
            ftp.Open(FTP_USER, FTP_PASS);
            ftp.PutFile(GetMemoryStreamRandom(100), "testfile.txt", FileAction.Create);
            MemoryStream fout = new MemoryStream();
            ftp.GetFile("testfile.txt", fout, false);
            ftp.DeleteFile("testfile.txt");
            ftp.Close();
        }

        [Test]
        public void TestUserReportedError2()
        {
            // clean up anything that may have been left over from previous tests
            FtpsClient ftp1 = new FtpsClient(FTP_HOST, FTP_STD_PORT, FtpsSecurityProtocol.None);
            ftp1.NetworkProtocol = NETWORK_PROTOCOL;
            ftp1.Open(FTP_USER, FTP_PASS);

            try
            {
                ftp1.ChangeDirectory("test dir");
            }
            catch
            {
            }
            try
            {
                ftp1.DeleteFile("testfile.txt");
            }
            catch
            {
            }

            try
            {
                ftp1.ChangeDirectory("\\");
            }
            catch
            {
            }

            try
            {
                ftp1.DeleteDirectory("test dir");
            }
            catch
            {
            }


            FtpsClient ftp = new FtpsClient(FTP_HOST, FTP_STD_PORT, FtpsSecurityProtocol.Tls12Explicit);
            ftp.NetworkProtocol = NETWORK_PROTOCOL;
            ftp.AlwaysAcceptServerCertificate = true;
            ftp.Open(FTP_USER, FTP_PASS);
            ftp.MakeDirectory("test dir");
            ftp.ChangeDirectory("test dir");
            ftp.PutFile(GetMemoryStreamRandom(100), "testfile.txt", FileAction.Create);
            FtpsItemCollection col = ftp.GetDirList();
            ftp.Close();

            Assert.IsTrue(col.Count == 1);

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
        [TestCase("ftp.NetBSD.org", 21, FtpsSecurityProtocol.None, "ftp", "ftp")]
        public void TestAbort(string host, int port, FtpsSecurityProtocol protocol,
            string user, string pwd)
        {
            using (FtpsClient c = new FtpsClient(host, port, protocol))
            {
                c.NetworkProtocol = NETWORK_PROTOCOL;
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
        [TestCase("ftp.NetBSD.org", 21, FtpsSecurityProtocol.None, "ftp", "ftp")]
        public void TestAllocateStorage(string host, int port, FtpsSecurityProtocol protocol,
            string user, string pwd)
        {
            using (FtpsClient c = new FtpsClient(host, port, protocol))
            {
                c.NetworkProtocol = NETWORK_PROTOCOL;
                c.AlwaysAcceptServerCertificate = true;
                c.Open(user, pwd);
                Assert.IsTrue(c.IsConnected);
                c.AllocateStorage(DEFAULT_FILE_SIZE * 16);
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
        [TestCase(FTP_HOST, FTP_STD_PORT, FtpsSecurityProtocol.None, FTP_USER, FTP_PASS)]
        public void TestChangeDirectory(string host, int port, FtpsSecurityProtocol protocol,
            string user, string pwd)
        {
            using (FtpsClient c = new FtpsClient(host, port, protocol))
            {
                c.NetworkProtocol = NETWORK_PROTOCOL;
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
        [TestCase(FTP_HOST, FTP_STD_PORT, FtpsSecurityProtocol.None, FTP_USER, FTP_PASS)]
        [TestCase(FTP_HOST, FTP_STD_PORT, FtpsSecurityProtocol.Tls1Explicit, FTP_USER, FTP_PASS)]
        public void TestOpen(string host, int port, FtpsSecurityProtocol protocol, 
            string user, string pwd)
        {
            FtpsClient c = new FtpsClient(host, port, protocol);
            c.NetworkProtocol = NETWORK_PROTOCOL;
            c.AlwaysAcceptServerCertificate = true;
            c.Open(user, pwd);
            Assert.IsTrue(c.IsConnected);
            c.Close();
        }

        /*
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
        [TestCase(FTP_HOST, FTP_STD_PORT, FtpsSecurityProtocol.Tls1Explicit, FTP_USER, FTP_USER, 5)]
        [TestCase(FTP_HOST, FTP_STD_PORT, FtpsSecurityProtocol.None, FTP_USER, FTP_USER, 5)]
        public void TestMultiOpen(string host, int port, FtpsSecurityProtocol protocol,
            string user, string pwd, int connections)
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
        */

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
        [TestCase(FTP_HOST, FTP_STD_PORT, FtpsSecurityProtocol.Tls1Explicit, FTP_USER, FTP_USER, 5)]
        [TestCase(FTP_HOST, FTP_STD_PORT, FtpsSecurityProtocol.None, FTP_USER, FTP_USER, 5)]
        [TestCase(FTP_HOST, FTP_TLS_IMPLICIT_PORT, FtpsSecurityProtocol.Tls1Implicit, FTP_USER, FTP_USER, 5)]
        public void TestRepeatedOpen(string host, int port, FtpsSecurityProtocol protocol,
            string user, string pwd, int connections)
        {
            for (int i = 0; i < connections; i++)
            {
                using (FtpsClient c = new FtpsClient(host, port, protocol))
                {
                    Console.WriteLine("********** START **********");
                    c.NetworkProtocol = NETWORK_PROTOCOL;
                    c.AlwaysAcceptServerCertificate = true;
                    c.Open(user, pwd);
                    Assert.IsTrue(c.IsConnected);
                    Console.WriteLine("********** PASSED **********");
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
        [TestCase(FTP_HOST, FTP_STD_PORT, FtpsSecurityProtocol.None, FTP_USER, FTP_PASS)]
        public void TestOpenUsing(string host, int port, FtpsSecurityProtocol protocol,
            string user, string pwd)
        {
            using (FtpsClient c = new FtpsClient(host, port, protocol))
            {
                c.NetworkProtocol = NETWORK_PROTOCOL;
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
        [TestCase(FTP_HOST, FTP_STD_PORT, FtpsSecurityProtocol.None, FTP_USER, FTP_PASS)]
        public void TestGetSystemType(string host, int port, FtpsSecurityProtocol protocol,
            string user, string pwd)
        {
            using (FtpsClient c = new FtpsClient(host, port, protocol))
            {
                c.NetworkProtocol = NETWORK_PROTOCOL;
                c.AlwaysAcceptServerCertificate = true;
                c.Open(user, pwd);
                Assert.IsTrue(c.IsConnected);
                string r = c.GetSystemType();
				Assert.IsNotNull(r);
				Assert.IsNotEmpty(r);
                Console.WriteLine("RESULT: " + r);
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
        [TestCase(FTP_HOST, FTP_STD_PORT, FtpsSecurityProtocol.None, FTP_USER, FTP_PASS)]
        public void TestPutFileUnique(string host, int port, FtpsSecurityProtocol protocol,
            string user, string pwd)
        {
            using (FtpsClient c = new FtpsClient(host, port, protocol))
            {
                c.NetworkProtocol = NETWORK_PROTOCOL;
                c.AlwaysAcceptServerCertificate = true;
                c.Open(user, pwd);
                Assert.IsTrue(c.IsConnected);

                byte[] bytes = new byte[DEFAULT_FILE_SIZE];
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
        [TestCase(FTP_HOST, FTP_STD_PORT, FtpsSecurityProtocol.None, FTP_USER, FTP_USER, DEFAULT_FILE_SIZE)]
        public void TestPutFileCreate(string host, int port, FtpsSecurityProtocol protocol,
            string user, string pwd, int fileSize)
        {
            using (FtpsClient c = new FtpsClient(host, port, protocol))
            {
                c.NetworkProtocol = NETWORK_PROTOCOL;
                c.AlwaysAcceptServerCertificate = true;
                c.NetworkProtocol = NetworkVersion.IPv6;
                c.Open(user, pwd);
                Assert.IsTrue(c.IsConnected);

                MemoryStream m1 = GetMemoryStreamRandom(fileSize);

                string fname = GetGuidString();

                try
                {
                    c.PutFile(m1, fname, FileAction.Create);

                    Assert.IsTrue(c.Exists(fname));

                    MemoryStream o1 = new MemoryStream();
                    c.GetFile(fname, o1, false);
                    o1.Position = 0;
                    
                    // compare bytes to verify
                    Assert.IsTrue(Compare(m1, o1));

                    // put a second file which should overwrite the first file
                    c.PutFile(m1, fname, FileAction.Create);

                    Assert.IsTrue(c.Exists(fname));

                    MemoryStream o2 = new MemoryStream();
                    c.GetFile(fname, o2, false);
                    o1.Position = 0;

                    // compare bytes to verify
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
        [TestCase(FTP_HOST, FTP_STD_PORT, FtpsSecurityProtocol.None, FTP_USER, FTP_USER, DEFAULT_FILE_SIZE)]
        public void TestPutFileCreateNew(string host, int port, FtpsSecurityProtocol protocol,
            string user, string pwd, int fileSize)
        {
            using (FtpsClient c = new FtpsClient(host, port, protocol))
            {
                c.NetworkProtocol = NETWORK_PROTOCOL;
                c.AlwaysAcceptServerCertificate = true;
                c.Open(user, pwd);
                Assert.IsTrue(c.IsConnected);

                MemoryStream m1 = GetMemoryStreamRandom(fileSize);
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
        [TestCase(FTP_HOST, FTP_STD_PORT, FtpsSecurityProtocol.None, FTP_USER, FTP_USER, DEFAULT_FILE_SIZE)]
        public void TestPutFileCreateOrAppend(string host, int port, FtpsSecurityProtocol protocol,
            string user, string pwd, int fileSize)
        {
            using (FtpsClient c = new FtpsClient(host, port, protocol))
            {
                c.NetworkProtocol = NETWORK_PROTOCOL;
                c.AlwaysAcceptServerCertificate = true;
                c.Open(user, pwd);
                Assert.IsTrue(c.IsConnected);

                MemoryStream m1 = GetMemoryStreamRandom(fileSize);
                MemoryStream m2 = GetMemoryStreamRandom(fileSize);
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
        [TestCase(FTP_HOST, FTP_STD_PORT, FtpsSecurityProtocol.None, FTP_USER, FTP_USER, DEFAULT_FILE_SIZE)]
        public void TestPutFileResume(string host, int port, FtpsSecurityProtocol protocol,
            string user, string pwd, int fileSize)
        {
            using (FtpsClient c = new FtpsClient(host, port, protocol))
            {
                c.NetworkProtocol = NETWORK_PROTOCOL;
                c.AlwaysAcceptServerCertificate = true;
                c.Open(user, pwd);
                Assert.IsTrue(c.IsConnected);

                MemoryStream m1 = GetMemoryStreamRandom(fileSize);
                MemoryStream m2 = GetMemoryStreamRandom(fileSize);

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
        [TestCase(FTP_HOST, FTP_STD_PORT, FtpsSecurityProtocol.None, FTP_USER, FTP_USER, DEFAULT_FILE_SIZE)]
        public void TestPutFileResumeCreate(string host, int port, FtpsSecurityProtocol protocol,
            string user, string pwd, int fileSize)
        {
            using (FtpsClient c = new FtpsClient(host, port, protocol))
            {
                c.NetworkProtocol = NETWORK_PROTOCOL;
                c.AlwaysAcceptServerCertificate = true;
                c.Open(user, pwd);
                Assert.IsTrue(c.IsConnected);

                MemoryStream m1 = GetMemoryStreamRandom(fileSize);
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
        [TestCase(FTP_HOST, FTP_STD_PORT, FtpsSecurityProtocol.None, FTP_USER, FTP_PASS)]
        public void TestQuote(string host, int port, FtpsSecurityProtocol protocol,
            string user, string pwd)
        {
            using (FtpsClient c = new FtpsClient(host, port, protocol))
            {
                c.NetworkProtocol = NETWORK_PROTOCOL;
                c.AlwaysAcceptServerCertificate = true;
                c.Open(user, pwd);
                Assert.IsTrue(c.IsConnected);
                string result = c.Quote("HELP");
                Assert.IsNotNull(result);
                Assert.IsNotEmpty(result);
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
        [TestCase(FTP_HOST, FTP_STD_PORT, FtpsSecurityProtocol.None, FTP_USER, FTP_PASS)]
        public void TestNoOp(string host, int port, FtpsSecurityProtocol protocol,
            string user, string pwd)
        {
            using (FtpsClient c = new FtpsClient(host, port, protocol))
            {
                c.NetworkProtocol = NETWORK_PROTOCOL;
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
        [TestCase(FTP_HOST, FTP_STD_PORT, FtpsSecurityProtocol.None, FTP_USER, FTP_PASS)]
        public void TestSetOptions(string host, int port, FtpsSecurityProtocol protocol,
            string user, string pwd)
        {
            using (FtpsClient c = new FtpsClient(host, port, protocol))
            {
                c.NetworkProtocol = NETWORK_PROTOCOL;
                c.AlwaysAcceptServerCertificate = true;
                c.Open(user, pwd);
                Assert.IsTrue(c.IsConnected);
                c.SetOptions("UTF8 ON");
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
        [TestCase(FTP_HOST, FTP_STD_PORT, FtpsSecurityProtocol.None, FTP_USER, FTP_PASS)]
        public void TestUtf8On(string host, int port, FtpsSecurityProtocol protocol,
            string user, string pwd)
        {
            using (FtpsClient c = new FtpsClient(host, port, protocol))
            {
                c.NetworkProtocol = NETWORK_PROTOCOL;
                c.AlwaysAcceptServerCertificate = true;
                c.Open(user, pwd);
                Assert.IsTrue(c.IsConnected);
                c.SetUtf8On();
                //Assert.IsTrue(c.Encoding == Encoding.UTF8);
            }
        }



        private static string GetGuidString()
        {
            return Guid.NewGuid().ToString();
        }

        private static MemoryStream GetMemoryStreamRandom(int size)
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
        [TestCase(FTP_HOST, FTP_STD_PORT, FtpsSecurityProtocol.None, FTP_USER, FTP_PASS)]
        public void TestGetDirList(string host, int port, FtpsSecurityProtocol protocol,
            string user, string pwd)
        {
            using (FtpsClient c = new FtpsClient(host, port, protocol))
            {
                c.NetworkProtocol = NETWORK_PROTOCOL;
                c.AlwaysAcceptServerCertificate = true;
                c.Open(user, pwd);
                Assert.IsTrue(c.IsConnected);
                FtpsItemCollection lst = c.GetDirList();

                Console.WriteLine("===================================================");
                Console.WriteLine("DIRECTORY DUMP");
                Console.WriteLine("===================================================");
                foreach (FtpsItem item in lst)
                {
                    Console.WriteLine(item.RawText);
                    Console.WriteLine(item.ToString());
                }
                Console.WriteLine("===================================================");

            }
        }

        /// <summary>
        /// Tests the get file.
        /// </summary>
        /// <param name="host">Host.</param>
        /// <param name="port">Port.</param>
        /// <param name="protocol">Protocol.</param>
        /// <param name="user">User.</param>
        /// <param name="pwd">Pwd.</param>
        [CLSCompliant(false)]
        [TestCase("ftp.gnupg.org", 21, FtpsSecurityProtocol.None, "ftp", "ftp")]
        public void TestGetFile(string host, int port, FtpsSecurityProtocol protocol, string user, string pwd)
        {
            using (FtpsClient c = new FtpsClient(host, port, protocol))
            {
                c.NetworkProtocol = NETWORK_PROTOCOL;
                c.AlwaysAcceptServerCertificate = true;
                c.TcpBufferSize = 80000;
                c.Open(user, pwd);

                Assert.IsTrue(c.IsConnected);

                MemoryStream msfile = new MemoryStream();
                c.GetFile("/gcrypt/gnupg/index.html", msfile, false);

                Assert.IsTrue(msfile.Length != 0);
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
