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
    /// To run the unit tests you will need Nunit.Framework 3.9+ installed.  To execute unit tests from visual
    /// studio you will also need NUnit 3 Test Adapater and Microsoft.NET.Test.SDK installed.  All can be installed
    /// via the Nuget manager and then associated with the project.
    /// 
    /// </remarks>
    [TestFixture]
    public class TestGpg
    {
        private const String HOME_PATH = "~/";
        //private const String GPG_BINARY_PATH = "usr/bin/gpg2";
        private const String GPG_BINARY_PATH = @"C:\Program Files (x86)\GnuPG\bin\gpg.exe";
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
            Assert.IsTrue(k.UserName == "ultimate John Jones (this is a long long long comment that I want to see what happens when the commen tis very long and and does something to the ui)");
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

            string testKey = "-----BEGIN PGP PUBLIC KEY BLOCK-----\r\n\r\nmQENBFqkNe4BCAC7rlgsdmfAAS8ieopmI4wqBO7YjgN5XspZUHX532eqWu7S7mAd\r\n1F43ES6zwS/9Z/t/tMX2iR6vKgfSGic2MI7sVvlCSGS7marOMeId6bngXPD1BQEe\r\n0WKHIYY5SwBWtqLRE2L3k+8w9K5SHHsDwbme5x4SneWz/4/bc2SMwyXsIb8EQn6R\r\nLsgt7w6Kk18Z1zWJG+bN3K1Es7jset2+cNhCfZnXSXhPyuzRoO3sNy7QQyYzsiKn\r\nTujfv4OsUMX/lyPPUPEUR03QjWA29lYpZUUZFYfvEV7s+ROA92LZ/chOKJUVgPG1\r\nYKuSEYFZUStAS4h17Q9bzwm37irZXh8Kemm/ABEBAAG0GUpvZSBUZXN0IDxqb2VA\r\nZG9tYWluLmNvbT6JAU4EEwEIADgWIQRQqHmapKYL4iMZN8sT8cK7WOeUCwUCWqQ1\r\n7gIbAwULCQgHAgYVCgkICwIEFgIDAQIeAQIXgAAKCRAT8cK7WOeUCyppCACAKES5\r\nVGit762bnHT5AcgrcqgP+kflNnaw987B4ekrzt3aOCnrfKdVYj72oDx9GwRcd2GS\r\nqQTyU+dD1BeYF24gO5u0YEJkDdwQGruaXz62kGPSkN4jW+YQJqc1Ecp8iDMON6KI\r\nbxIRaOl3ZZhV23RCJY0jb4FOxqrsY3ytmWUss4wynpv6zkfNzzuQwv95OnDQVZn4\r\n84VmblLt7D82IGxvzgRBJ9o54f0AyqXkcJguup6eYhj6isS/9rDebwyfAtH1wFqX\r\nY8Cogm3kcn/y5haX9GfEI4dhuPFyw/6AviDSZQDr1/Xtpaw0xuVRqC7Pre07Rz8s\r\naYtqhGBynMrMPeceuQENBFqkNe4BCACyOqWFrEUG04Vuuh+KMVuKasrMkc/UcHgw\r\nmDhkDnvsA3OejEuoV+s17BGVv4eEJH3nYmaYqrWX1pHolGOjkeSUunxfIu00NAta\r\nu8sX3zVDIn2MJLT8h1ecof6Dvx1ceSdKGdAIbS5S0rX0tn2bnY9RzemBDCc92o7r\r\nyCRqgsTalflIT1TSoP96d3N0CgYvpLd4exSubYAXztGBTu8tozT7/SE8aYK/ao0A\r\nGG72BGPpnwRHNgiBDwFaScLO10hDoeL+fVPQFl/GFg49By9GnM2wTP6G6UqIfcFo\r\nIUTUvmNyxXoxu7Ex85fCCcOXMrWTj5Vyx6z3HHb+Z+MVXgQxXphhABEBAAGJATYE\r\nGAEIACAWIQRQqHmapKYL4iMZN8sT8cK7WOeUCwUCWqQ17gIbDAAKCRAT8cK7WOeU\r\nC+A2B/wLOJD45hyTsvUPR67/j46y/vNvT0oQuzzYGsoFsq5N6eCruHTZsv6eqgsH\r\nEeVHb8kjj437C1aFHN5/evO2fibNw7osHstFevj48YJ0n3TdYEklaSXIQMTpZbLD\r\nOcbvcmzC8r9D+2syCecgTRYcuCouJTJyQsI7JdAxeJInycKI5lvR4+O1SQFz9m/3\r\nyF9WqAtR0AXnoQjS2Y82lAKdaa52IIuiRUjJyP9YVRPTcH6LlyoRp5OXAQzreFGa\r\nP+CnwGYC2WV1hQ7qxpI2fAJ4/DquVgbuX85bLf+rbC7eP+4xti0tSRm9yVlB7qsY\r\nwcLNxOrwIKeLtws5Vmv5XNB7E3Fc\r\n=BllK\r\n-----END PGP PUBLIC KEY BLOCK-----\r\n";
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
