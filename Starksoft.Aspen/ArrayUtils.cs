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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Security.Cryptography;


namespace Starksoft.Aspen
{
    /// <summary>
    /// Parity options.
    /// </summary>
    public enum ParityOptions
    {
        /// <summary>
        /// Odd parity.
        /// </summary>
        Odd,
        /// <summary>
        /// Even parity.
        /// </summary>
        Even
    };
    
    /// <summary>
    /// Array utility class for working with byte arrays.
    /// </summary>
    public class ArrayUtils
    {

        /// <summary>
        /// Compares two byte arrays to make sure they contain the exact same data and are the same length.
        /// </summary>
        /// <param name="array1">First array to compare.</param>
        /// <param name="array2">Second array to compare.</param>
        /// <returns>A value of true if the arrays are the same; otherwise false.</returns>
        static public bool Compare(byte[] array1, byte[] array2)
        {
            if (array1 == null)
                throw new ArgumentNullException("array1");

            if (array2 == null)
                throw new ArgumentNullException("array2");

            if (array1.Length != array2.Length)
            {
                return false;
            }

            int len = array1.Length;
            for (int i = 0; i < len; i++)
            {
                if (array1[i] != array2[i]) 
                    return false;
            }

            return true;
        }


        /// <summary>
        /// Encodes a byte array to a string in 2 character hex format.
        /// </summary>
        /// <param name="data">Array of bytes to convert.</param>
        /// <returns>String containing encoded bytes.</returns>
        /// <remarks>e.g. 0x55 ==> "55", also left pads with 0 so that 0x01 is "01" and not "1"</remarks>
        public static string HexEncode(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            return HexEncode(data, false, data.Length);
        }

        //   Benton Stark    03-09-2011
        /// <summary>
        /// Encodes a byte to a string in 2 character hex format.
        /// </summary>
        /// <param name="data">Byte to convert.</param>
        /// <returns>String containing encoded byte.</returns>
        /// <remarks>e.g. 0x55 ==> "55", also left pads with 0 so that 0x01 is "01" and not "1"</remarks>
        public static string HexEncode(byte data)
        {
            byte[] b = new byte[1];
            b[0] = data;
            return HexEncode(b, false, b.Length);
        }

        /// <summary>
        /// Encodes a byte array to a string in 2 character hex format.
        /// </summary>
        /// <param name="data">Array of bytes to convert.</param>
        /// <param name="insertColonDelimiter">Insert colon as the delimiter between bytes.</param>
        /// <returns>String containing encoded bytes.</returns>
        /// <remarks>e.g. 0x55 ==> "55", also left pads with 0 so that 0x01 is "01" and not "1"</remarks>
        public static string HexEncode(byte[] data, bool insertColonDelimiter)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            return HexEncode(data, insertColonDelimiter, data.Length);
        }

        /// <summary>
        /// Encodes a byte array to a string in 2 character hex format.
        /// </summary>
        /// <param name="data">Array of bytes to encode.</param>
        /// <param name="insertColonDelimiter">Insert colon as the delimiter between bytes.</param>
        /// <param name="length">Number of bytes to encode.</param>
        /// <returns>String containing encoded bytes.</returns>
        /// <remarks>e.g. 0x55 ==> "55", also left pads with 0 so that 0x01 is "01" and not "1"</remarks>
        public static string HexEncode(byte[] data, bool insertColonDelimiter, int length)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            
            StringBuilder buffer = new StringBuilder(length * 2);

            int len = data.Length;
            for (int i = 0; i < len; i++ )
            {
                buffer.Append(data[i].ToString("x").PadLeft(2, '0')); //same as "%02X" in C
                if (insertColonDelimiter && i < len - 1)
                    buffer.Append(':');
            }
            return buffer.ToString();
        }


        /// <summary>
        /// Decodes a 2 character hex format string to a byte array.
        /// </summary>
        /// <param name="s">String containing hex values to decode..</param>
        /// <returns>Array of decoded bytes.</returns>
        /// <remarks>Input string may contain a ':' delimiter between each encoded byte pair.</remarks>
        public static byte[] HexDecode(string s)
        {
            return HexDecode(s, 0);
        }

        /// <summary>
        /// Decodes a 2 character hex format string to a byte array.
        /// </summary>
        /// <param name="s">String containing hex values to decode..</param>
        /// <param name="paddingBytes">Number of most significant byte padding to add.</param>
        /// <returns>Array of decoded bytes.</returns>
        /// <remarks>Input string may contain a ':' delimiter between each encoded byte pair.</remarks>
        public static byte[] HexDecode(string s, int paddingBytes)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }

            if (s.IndexOf(':') > -1)
                s = s.Replace(":", "");

            if ((s.Length % 2) != 0)
            {
                throw new FormatException("parameter 's' must have an even number of hex characters");
            }

            byte[] result = new byte[s.Length / 2 + paddingBytes];
            for (int i = 0; i < result.Length - paddingBytes; i++)
            {
                result[i] = byte.Parse(s.Substring(i * 2, 2), NumberStyles.AllowHexSpecifier);
            }
            return result;
        }

        /// <summary>
        /// Converts a 32-bit integer value to a 24-bit integer value.
        /// </summary>
        /// <param name="value">32-bit integer.</param>
        /// <returns>24-bit integer as a byte array.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If value is greater than 16777215.</exception>
        public static byte[] GetInt24(Int32 value)
        {
            if (value > 16777215)
                throw new ArgumentOutOfRangeException("value", "value can not be greater than 16777215 (max unsigned 24-bit integer)");
            
            byte[] bytes = BitConverter.GetBytes(value);
            byte[] buffer = new byte[3];
            Array.Copy(bytes, 0,  buffer, 0, 3);
            return buffer;
        }

        /// <summary>
        /// Converts a 24-bit integer byte array to a 32-bit integer.
        /// </summary>
        /// <param name="int24">24-bit integer value.</param>
        /// <returns>Unsigned 32-bit integer value.</returns>
        public static Int32 GetInt32(byte[] int24)
        {
            if (int24 == null)
                throw new ArgumentNullException("int24");
            
            if (int24.Length != 3)
            {
                throw new ArgumentOutOfRangeException("int24", "byte size must be exactly three");
            }
            ArrayBuilder buffer = new ArrayBuilder(4);
            buffer.Append(int24);
            return BitConverter.ToInt32(buffer.GetBytes(), 0);
        }

        /// <summary>
        /// Copies a byte array by creating a new array and transferring the values.
        /// </summary>
        /// <param name="array">Byte array to clone.</param>
        /// <returns>Cloned array.</returns>
        public static byte[] Clone(byte[] array)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            byte[] buffer = new byte[array.Length];
            Array.Copy(array, buffer, buffer.Length);
            return buffer;
        }

        /// <summary>
        /// Returns a copy of the supplied array in reverse order.
        /// </summary>
        /// <param name="array">Array to reverse.</param>
        /// <returns>Reverse array.</returns>
        public static byte[] Reverse(byte[] array)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            byte[] buffer = new byte[array.Length];
            Array.Copy(array, buffer, buffer.Length);
            Array.Reverse(buffer);
            return buffer;
        }

        /// <summary>
        /// Gets the length of an array object but does not throw an exception
        /// if the array is null.
        /// </summary>
        /// <param name="array">Array (can be null)</param>
        /// <returns>Length of array of 0 if the array is null.</returns>
        public static int GetLengthSafe(byte[] array)
        {
            if (array == null)
                return 0;
            else
                return array.Length;

        }

        /// <summary>
        /// Pad with bytes all of the same value as the number of padding bytes.
        /// </summary>
        /// <param name="array">Byte array to pad.</param>
        /// <param name="blockSize">Standard block size.</param>
        /// <remarks>
        /// This padding technique is documented in the RSA specification PKCS#5, PKCS#7 as well 
        /// as the RFC 3852 Section 6.3.  This method will paddi with bytes all of the same value 
        /// as the number of bytes padded.  So if you are short by 6 bytes then simply append six 
        /// bytes of data with the value 0x06.  Similarly, if you are short 2 bytes then appended 
        /// 2 padding bytes with the value 0x02.
        /// </remarks>
        /// <returns></returns>
        public static byte[] PadArrayPkcs7(byte[] array, int blockSize)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            if (blockSize <= 0)
                throw new ArgumentOutOfRangeException("blockSize", "must be 1 or greater");

            byte[] buffer = ArrayUtils.Clone(array);
            int rem = array.Length % blockSize;
            int pads = blockSize - rem; 
            
            if (rem == 0)
                return buffer;

            Array.Resize<byte>(ref buffer, buffer.Length + pads);

            for (int i = array.Length; i < buffer.Length; i++)
            {
                buffer[i] = (byte)pads;
            }

            return buffer;
        }

        /// <summary>
        /// Set the least significant odd party bit for supplied byte array.
        /// </summary>
        /// <param name="bytes"></param>
        public static void SetOddParity(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException("bytes");
            
            for (int i = 0; i < bytes.Length; i++)
            {
                int b = bytes[i];
                bytes[i] = (byte)((b & 0xfe) |
                                ((((b >> 1) ^
                                (b >> 2) ^
                                (b >> 3) ^
                                (b >> 4) ^
                                (b >> 5) ^
                                (b >> 6) ^
                                (b >> 7)) ^ 0x01) & 0x01));
            }
        }

        /// <summary>
        /// Set the most significate parity bit for supplied byte array.
        /// </summary>
        /// <param name="array">Input byte array.</param>
        /// <param name="parity">Parity to set.</param>
        /// <returns>New byte array with the correct parity.</returns>
        public static byte[] SetParity(byte[] array, ParityOptions parity)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            // An odd parity bit is set to 1 if the number of ones in a given set of bits is even 
            // (making the total number of ones, including the parity bit, odd).
            // http://en.wikipedia.org/wiki/Parity_bit

            // load the bytes into one contiguous bit array structure
            BitArray bits = new BitArray(array);
            byte[] buffer = Clone(array);

            // two loops - the outer is to loop through the bytes
            // and the inner is to loop through the bits in a particular byte
            for (int i = 0; i < array.Length; i++)
            {
                int oneCount = 0;
                // loop through the bits in the byte starting with
                // the second bit and running to the 8th bit. 
                // (remember that the array is zero indexed so it is off by 1)
                for (int j = 0; j < 7; j++)
                {
                    if (bits[i * 8 + j] == true)
                        oneCount++;
                }

                // set the parity bit (bit position 0) for the byte now that we have the one's count
                // if the number of one's is even then set to 1 ("true" for the BitArray)
                // so that now the total number of bits (8) are now odd when added up
                // the opposite is true for even parity
                switch (parity)
                {
                    case ParityOptions.Odd:
                        if (oneCount % 2 == 0)
                            buffer[i] |= (1 << 7);
                        else
                            buffer[i] &= unchecked((byte)(~(1 << 7)));
                        break;
                    case ParityOptions.Even:
                        if (oneCount % 2 == 0)
                            buffer[i] &= unchecked((byte)(~(1 << 7)));
                        else
                            buffer[i] |= (1 << 7);
                        break;
                }
            }

            return buffer;

        }

        /// <summary>
        /// Creates an array of bytes.
        /// </summary>
        /// <param name="length">Length of the array.</param>
        /// <param name="byteValue">Initial value for the array.</param>
        /// <returns>Padding byte array.</returns>
        public static byte[] CreateArray(int length, byte byteValue)
        {
            if (length <= 0)
                throw new ArgumentOutOfRangeException("length must be greater than 0");

            byte[] buf = new byte[length];
            for (int i = 0; i < buf.Length; i++)
            {
                buf[i] = byteValue;
            }
            return buf;
        }

        /// <summary>
        /// Retrieves a substring from this instance. The substring starts at a specified
        /// character position and has a specified length.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="startIndex">The index of the start of the substring.</param>
        /// <param name="length">The number of characters in the substring.</param>
        /// <returns>
        /// A Byte array equivalent to the substring of length that begins
        /// at startIndex.
        ///</returns>
        public static byte[] Subarray(byte[] array, int startIndex, int length)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            if (length > array.Length)
                throw new Exception("length exceeds size of array");
            byte[] buf = new byte[length];
            Array.Copy(array, startIndex, buf, 0, length);
            return buf;
        }

        /// <summary>
        /// Trim zero padding from the right side of the array.
        /// </summary>
        /// <param name="array">Array to trim.</param>
        /// <returns>Resized array without zero's padding.</returns>
        public static byte[] TrimPadding(byte[] array)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            
            int j = array.Length;
            for (int i = array.Length - 1; i >= 0; i--)
            {
                if (array[i] != 0)
                    break;
                j--;
            }

            return ArrayUtils.Subarray(array, 0, j);
        }

        // BKS 12-21-2009
        /// <summary>
        /// Clears all the bytes in an array by setting the elements to zero
        /// and then resizes the array to 0.
        /// </summary>
        /// <param name="array">Array of bytes to clear.</param>
        public static void Clear(byte[] array)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = 0;
            }

            Array.Resize<byte>(ref array, 0);
        }

        // Benton Stark    03-07-2011
        /// <summary>
        /// Tests if string data supplied is valid hex encoded value.
        /// </summary>
        /// <param name="data">String containing data</param>
        /// <returns>True if string is valid hex encoded value; otherwise false</returns>
        public static bool IsHexValue(string data)
        {
            try
            {
                HexDecode(data);
            }
            catch { return false; }
            return true;
        }

        //Benton Stark    09-25-2012  
        /// <summary>
        /// Zeros an array by setting all byte values to the value 0.
        /// </summary>
        /// <param name="array">Array to zero.</param>
        public static void Zero(byte[] array)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = 0;
            }
        }
        
        //Benton Stark    10-09-2012  
        /// <summary>
        /// Takes two equal size input byte arrays and xors their values and returns the xor result as a new byte array.
        /// </summary>
        /// <param name="array1">Array #1.</param>
        /// <param name="array2">Array #2.</param>
        /// <returns>New xor byte array.</returns>
        public static byte[] Xor(byte[] array1, byte[] array2)
        {
            if (array1 == null)
                throw new ArgumentNullException("array1");
            if (array2 == null)
                throw new ArgumentNullException("array2");
            if (array1.Length != array2.Length)
                throw new ArgumentOutOfRangeException("array1 and array2 must have same length");

            int len = array1.Length;
            byte[] xor = new byte[len];
            for (int i = 0; i < len; i++)
            {
                xor[i] = (byte)(Convert.ToInt32(array1[i]) ^ Convert.ToInt32(array2[i]));
            }

            return xor;
        }

        //Benton Stark    10-21-2013  
        /// <summary>
        /// Combine 2 or more byte arrays into a single, contegious byte array.
        /// </summary>
        /// <param name="list">Byte array parameter list.</param>
        /// <returns>Single byte array.</returns>
        public static byte[] Combine(params byte[][] list)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            int size = 0;
            foreach (byte[] item in list)
            {
                if (item == null)
                    throw new ArgumentNullException("list contains null array");
                size += item.Length;
            }
            ArrayBuilder b = new ArrayBuilder(size);
            foreach (byte[] item in list)
            {
                b.Append(item);
            }
            byte[] rtn = b.GetBytes();
            b.Clear();
            return rtn;
        }

        //Benton Stark    10-21-2013  
        /// <summary>
        /// Encrypt cleartext data using specified symmetric cipher and optional IV.
        /// </summary>
        /// <param name="algo">Symmetric cipher algorithm object.</param>
        /// <param name="key">Symmetric algorithm key.</param>
        /// <param name="iv">Optional IV value (can be null or empty byte array).</param>
        /// <param name="mode">Symmetric cipher block mode.</param>
        /// <param name="cleartext">Cleartext data to encrypt.</param>
        /// <returns>Ciphertext data.</returns>
        public static byte[] Encrypt(SymmetricAlgorithm algo, byte[] key, byte[] iv, CipherMode mode, byte[] cleartext)
        {
            if (algo == null)
                throw new ArgumentNullException("algo");
            if (key == null)
                throw new ArgumentNullException("key");
            if (cleartext == null)
                throw new ArgumentNullException("cleartext");
            algo.Mode = mode;
            ICryptoTransform c = algo.CreateEncryptor(key, iv);
            byte[] ciphertext = new byte[cleartext.Length];
            c.TransformBlock(cleartext, 0, cleartext.Length, ciphertext, 0);
            return ciphertext;
        }

        //Benton Stark    10-21-2013  
        /// <summary>
        /// Decrypt ciphertext data using specified symmetric cipher and optional IV.
        /// </summary>
        /// <param name="algo">Symmetric cipher algorithm object.</param>
        /// <param name="key">Symmetric algorithm key.</param>
        /// <param name="iv">Optional IV value (can be null or empty byte array).</param>
        /// <param name="mode">Symmetric cipher block mode.</param>
        /// <param name="ciphertext">Ciphertext data to decrypt.</param>
        /// <returns>Cleartext data.</returns>
        public static byte[] Decrypt(SymmetricAlgorithm algo, byte[] key, byte[] iv, CipherMode mode, byte[] ciphertext)
        {
            if (algo == null)
                throw new ArgumentNullException("algo");
            if (key == null)
                throw new ArgumentNullException("key");
            if (ciphertext == null)
                throw new ArgumentNullException("cleartext");
            algo.Mode = mode;
            ICryptoTransform d = algo.CreateDecryptor(key, iv);
            byte[] cleartext = new byte[ciphertext.Length];
            d.TransformBlock(ciphertext, 0, ciphertext.Length, cleartext, 0);
            return cleartext;
        }

        //Benton Stark    10-21-2013  
        /// <summary>
        /// Zero pads an array of bytes to specific size.
        /// </summary>
        /// <param name="data">Data to zero pad.</param>
        /// <param name="size">Size of the return array.</param>
        /// <returns>Array of bytes zero padded.</returns>
        public static byte[] ZeroPad(byte[] data, int size)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (data.Length > size)
                throw new ArgumentOutOfRangeException("size");
            ArrayBuilder b = new ArrayBuilder(size);
            b.Append(data);
            byte[] pad = b.GetBytes();
            b.Clear();  // clear the private key from the array builder memory
            return pad; // return pointer to the byte array
        }

        //Benton Stark    07-25-2014  
        /// <summary>
        /// Pads an array of bytes to specific size using hex value 0xFF.
        /// </summary>
        /// <param name="data">Data to pad.</param>
        /// <param name="size">Size of the return array.</param>
        /// <returns>Array of bytes padded.</returns>
        public static byte[] PadArrayFF(byte[] data, int size)
        {
            return PadArray(data, size, 0xff);
        }
        
        //Benton Stark    07-25-2014  
        /// <summary>
        /// Pads an array of bytes to specific size using a supplied padding value.
        /// </summary>
        /// <param name="data">Data to pad.</param>
        /// <param name="size">Size of the return array.</param>
        /// <param name="padValue">Padding value to use.</param>
        /// <returns>Array of bytes padded.</returns>
        public static byte[] PadArray(byte[] data, int size, byte padValue)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (data.Length > size)
                throw new ArgumentOutOfRangeException("size");
            ArrayBuilder b = new ArrayBuilder(size);
            b.Append(data);
            b.Append(ArrayUtils.CreateArray(size - data.Length, padValue));
            byte[] pad = b.GetBytes(); // get a copy of the padded bytes
            b.Clear();  // clear the private key from the array builder memory
            return pad; // return pointer to the byte array
        }

        //Benton Stark    10-21-2013  
        /// <summary>
        /// Compute a hash value across one or more byte arrays.
        /// </summary>
        /// <param name="algo">Hashing algorithm to use.</param>
        /// <param name="list">Byte array to compute hash across.</param>
        /// <returns>Byte array containing hash value.</returns>
        public static byte[] Hash(HashAlgorithm algo, params byte[][] list)
        {
            if (algo == null)
                throw new ArgumentNullException("algo");
            if (list == null)
                throw new ArgumentNullException("list");
            int size = 0;
            foreach (byte[] item in list)
            {
                if (item == null)
                    throw new ArgumentNullException("list contains null array");
                size += item.Length;
            }
            ArrayBuilder b = new ArrayBuilder(size);
            foreach (byte[] item in list)
            {
                b.Append(item);
            }
            byte[] hash = algo.ComputeHash(b.GetBytes());
            b.Clear();
            return hash;
        }
    }

}
