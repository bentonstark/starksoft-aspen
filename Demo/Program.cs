using System;
using System.IO;
using System.Text;
using Starksoft.Aspen.GnuPG;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            string keyId = "416AB815";
            string passwd = "abc123!";
            byte[] byteArray = Encoding.ASCII.GetBytes("Some text");
            MemoryStream input = new MemoryStream(byteArray);
            MemoryStream output = new MemoryStream();
            Gpg gpg = new Gpg("C:\\Program Files (x86)\\GNU\\GnuPG\\pub\\gpg.exe");
            gpg.OutputType = OutputTypes.AsciiArmor;
            gpg.Passphrase = passwd;
            gpg.LocalUser = keyId;
            gpg.OutputSignatureType = OutputSignatureTypes.ClearText;
            gpg.Sign(input, output);
            output.Position = 0;
            StreamReader reader = new StreamReader(output);
            string text = reader.ReadToEnd();
            MemoryStream outputVer = new MemoryStream();
            var verifiedKey = gpg.Verify(output, outputVer);
            outputVer.Position = 0;
            StreamReader reader2 = new StreamReader(outputVer);
            string text2 = reader2.ReadToEnd();
            Console.WriteLine("Found key " + verifiedKey);
            Console.WriteLine("Output: " + text2);
            Console.ReadKey();
        }
    }
}
