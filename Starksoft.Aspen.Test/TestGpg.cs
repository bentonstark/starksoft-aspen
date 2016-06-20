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
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using Starksoft.Aspen.GnuPG;

namespace Starksoft.Aspen.Tests
{

    /// <summary>
    /// Test fixture for Starksoft.GnuPG.Gpg.
    /// </summary>
    /// <remarks>
    /// 
    /// To run the unit tests you will need Nunit 2.5.10 installed as well as gpg, gpg2 for Linux or 
    /// gpg2.exe for Windows.  In addition two keys need to be created via the gpg2 --gen-key command
    /// with email addresses of jones@domain.com and albert@domain.com.  For Windows builds a WIN
    /// compiler directive needs to be created or the GPG_BINARY_PATH will need to be updated
    /// to point to your install gpg2.exe program.
    /// 
    /// For those that are wondering why I am using nunit 2.x.  There are two reasons:  1) never versions are not
    /// supported in monodevelop for Linux.  2) nunit 3.x is far more of a pain to setup and run at the moment.
    /// This project is being maintained and tested under two environments so I need a unit test version that works
    /// for both.
    /// 
    /// </remarks>
    [TestFixture]
	public class TestGpg
	{
        private const String HOME_PATH = "~/";
#if WIN
        private const String GPG_BINARY_PATH = "C:\\Program Files (x86)\\GNU\\GnuPG\\gpg2.exe";
#else
        private const String GPG_BINARY_PATH = "/usr/bin/gpg2";
#endif
        private const String GPG_PASSWORD = "password";
        private const String GPG_LOCAL_USER_KEY = "jones@domain.com";
        private const String GPG_RECIPIENT_KEY = "albert@domain.com";

        private const String KEY_LIST_1 = "sec   1024D/543C3595 2016-12-10\r\nuid                  Firstname Lastname <firstname@domain.com>\r\nuid       ...\r\nssb   1024g/42A71AD8 2006-12-10\r\n";
        private const String KEY_LIST_2 = "pub   rsa2048/028ECE89 2016-06-07 [SC] [expires: 2018-06-07]\r\nuid         [ultimate] John Jones (this is a long long long comment that I want to see what happens when the commen tis very long and and does something to the ui) <john.jones@domain.com>        \r\nsub   rsa2048/40780734 2016-06-07 [E] [expires: 2018-06-07]";


        [Test]
        public void TestGpgKeyList1()
        {
            GpgKey k = new GpgKey(KEY_LIST_1);

            Assert.IsTrue(k.Raw == KEY_LIST_1);
            Assert.IsTrue(k.UserId == "firstname@domain.com");
            Assert.IsTrue(k.UserName == "Firstname Lastname");
            Assert.IsTrue(k.Key == "1024D/543C3595");
            Assert.IsTrue(DateTime.Compare(k.KeyExpiration, new DateTime(2016, 12, 10, 0, 0, 0)) == 0);
            Assert.IsNull(k.SubKey);
            Assert.IsTrue(DateTime.Compare(k.SubKeyExpiration, DateTime.MinValue) == 0);

        }

        [Test]
        public void TestGpgKeyList2()
        {
            GpgKey k = new GpgKey(KEY_LIST_2);

            Assert.IsTrue(k.Raw == KEY_LIST_2);
            Assert.IsTrue(k.UserId == "john.jones@domain.com");
            Assert.IsTrue(k.UserName == "[ultimate] John Jones (this is a long long long comment that I want to see what happens when the commen tis very long and and does something to the ui)");
            Assert.IsTrue(k.Key == "rsa2048/028ECE89");
            Assert.IsTrue(DateTime.Compare(k.KeyCreation, new DateTime(2016, 6, 7, 0, 0, 0)) == 0);
            Assert.IsTrue(DateTime.Compare(k.KeyExpiration, new DateTime(2018, 6, 7, 0, 0, 0)) == 0);
            Assert.IsNull(k.SubKey);
            Assert.IsTrue(DateTime.Compare(k.SubKeyExpiration, DateTime.MinValue) == 0);

        }

		/// <summary>
		/// Initializes a new instance of the <see cref="Starksoft.Aspen.Tests.TestGpg"/> class.
		/// </summary>
        [Test]
        public void TestGetKeys()
		{
            Gpg g = new Gpg(GPG_BINARY_PATH);
			GpgKeyCollection col = g.GetKeys();

            Debug.WriteLine("count: " + col.Count.ToString());

			foreach (GpgKey k in col) {
               PrintKey(k);
            }
		}

        private void PrintKey(GpgKey key)
        {
            Debug.WriteLine(key.ToString());
        }

        [Test]
        public void TestGetSecretKeys()
        {
            Gpg g = new Gpg(GPG_BINARY_PATH);
            g.Passphrase = GPG_PASSWORD;
            GpgKeyCollection col = g.GetSecretKeys();

            Debug.WriteLine("count: " + col.Count.ToString());

            foreach (GpgKey k in col) {
                PrintKey(k);
            }
        }


        [TestCase(OutputSignatureTypes.ClearText)]
        [TestCase(OutputSignatureTypes.Detached)]
        [TestCase(OutputSignatureTypes.Signature)]
        public void TestSign(OutputSignatureTypes sigType)
        {
            Gpg g = new Gpg(GPG_BINARY_PATH);
            g.LocalUser = GPG_LOCAL_USER_KEY;
            g.Passphrase = GPG_PASSWORD;
            g.OutputSignatureType = sigType;

            byte[] data = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04 };
            MemoryStream tbs = new MemoryStream(data);
            MemoryStream sig = new MemoryStream();

            g.Sign(tbs, sig);

            Assert.IsTrue(sig.Length > 0);
        }

        [Test]
        public void TestSignVerify()
        {
            Gpg g = new Gpg(GPG_BINARY_PATH);
            g.LocalUser = GPG_LOCAL_USER_KEY;
            g.Passphrase = GPG_PASSWORD;

            byte[] data = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04 };
            MemoryStream tbs = new MemoryStream(data);
            MemoryStream sig = new MemoryStream();

            g.Sign(tbs, sig);

            Assert.IsTrue(sig.Length > 0);

            // set the postion in the signature stream to start reading from at the beginning
            sig.Position = 0;

            g.Verify(sig);

        }

        [Test]
        public void TestVerifyFailInvalidData()
        {
            Gpg g = new Gpg(GPG_BINARY_PATH);
            g.LocalUser = GPG_LOCAL_USER_KEY;
            g.Passphrase = GPG_PASSWORD;

            byte[] data = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04 };
            MemoryStream sig = new MemoryStream(data);

            Assert.Throws<GpgException>(() => g.Verify(sig));
        }

        [Test]
        public void TestVerifyFailInvalidSignature()
        {
            Gpg g = new Gpg(GPG_BINARY_PATH);
            g.Passphrase = GPG_PASSWORD;

            byte[] data = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04 };
            MemoryStream tbs = new MemoryStream(data);
            MemoryStream sig = new MemoryStream();

            g.Sign(tbs, sig);

            Assert.IsTrue(sig.Length > 0);

            // tweak a few bytes on the sig to invalidate it
            sig.Position = sig.Length / 2 - 1;
            sig.WriteByte(0xFF);
            sig.WriteByte(0xFF);         
            sig.WriteByte(0xFF);

            // set the postion in the signature stream to start reading from at the beginning
            sig.Position = 0;

            Assert.Throws<GpgException>(() => g.Verify(sig));

        }

        [Test]
        public void TestEncryptDecrypt()
        {
            Gpg g = new Gpg(GPG_BINARY_PATH);
            g.LocalUser = GPG_LOCAL_USER_KEY;
            g.Recipient = GPG_RECIPIENT_KEY;
            g.Passphrase = GPG_PASSWORD;

            byte[] data = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04 };
            MemoryStream cleartext = new MemoryStream(data);
            MemoryStream ciphertext = new MemoryStream();

            g.Encrypt(cleartext, ciphertext);

            ciphertext.Position = 0;
            MemoryStream cleartext2 = new MemoryStream();
            g.Decrypt(ciphertext, cleartext2);

            assertEqual(cleartext, cleartext2);
        }

        [Test]
        public void TestImportKey()
        {
            Gpg g = new Gpg(GPG_BINARY_PATH);

            string testKey = "-----BEGIN PGP PUBLIC KEY BLOCK-----\r\n Version: GnuPG v2\r\n \r\n mI0EV2gu1gEEALT6ouVbTBSk8KUKbOkhA2cWk3oVo9ZstcI7+io1+VPzaExkyPLm\r\n IQ6RTHLnjuN21mWRt0XdPM9KyULGa0Mnfdgrj73vI8KiMsjKppADCCrON/gP1v7f\r\n VhbbELCZ2qJvEXcBRt481ZhHLJLbbja5ogE2Bgy/XUrZCcL7OQGRHNYZABEBAAG0\r\n H1Rlc3QgSW1wb3J0IDxpbXBvcnRAZG9tYWluLmNvbT6IvwQTAQgAKQUCV2gu1gIb\r\n AwUJEswDAAcLCQgHAwIBBhUIAgkKCwQWAgMBAh4BAheAAAoJEB18VzOpLkfW6k4E\r\n AKdvv/4nCZ5lBUxpfoeq68sBMUJSdkJynmlpFngUjT0JdcAuby0nL7xULtLxsfZJ\r\n 1V7eTK0DT5QIj+zqn0Yhv+W+RwAn2U8UK6nPz8HcPmkH6NwVwRwZyunxUu86jh9l\r\n R3rmlZGa9FtB2aXxvHToJ0H4ODlnBLpuVKyg67Rj+NhouI0EV2gu1gEEAKik+LqK\r\n 8nac+wBQqMqvMGgREDmw+6bjhKcxfMDkCU+5fTi0hoqw6JTJ1UV1tQ7/5JvFZJbf\r\n Sl5fjZda1TvnaBdh/P5+9TJZw2NJb0PlTz9K0LqFvyVrlUq0OVxGXVAOIgAnUKHp\r\n e5UagUlied+mfbJTuPDmpxLoNTxFw/iHmrbDABEBAAGIpQQYAQgADwUCV2gu1gIb\r\n DAUJEswDAAAKCRAdfFczqS5H1lqbA/9pojoBFCUX/WHFDIczff1XQ0FDr9UVajuU\r\n mKsOKuLvv623nZCJolZgo1YUFPi5WMBwDdDdno8e2pH+ij5SX+Kb7u/jZ12D918S\r\n 8fQkWAGKWK+8/c9XZEP4YRTkcDKrG2ABqiW4rPbJPX+xOt2AiVetzavDlmLQivEJ\r\n KOF6pCI72g==\r\n =xvcI\r\n -----END PGP PUBLIC KEY BLOCK-----\r\n";
            byte[] testKeyBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(testKey);
            MemoryStream key = new MemoryStream(testKeyBytes);
            string result = g.Import(key);
            Debug.WriteLine(result);
        }


        private void assertEqual(MemoryStream s1, MemoryStream s2)
        {
            s1.Position = 0;
            s2.Position = 0;

            byte[] a1 = s1.ToArray();
            byte[] a2 = s2.ToArray();

            Assert.IsTrue(a1.Length == a2.Length);

            for (int i = 0; i < a1.Length; i++)
            {
                Assert.IsTrue(a1[i] == a2[i]);
            }

        }

    }

}