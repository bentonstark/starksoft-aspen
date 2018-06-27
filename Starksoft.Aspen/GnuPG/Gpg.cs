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
using System.Text; 
using System.Diagnostics; 
using System.IO; 
using System.Threading;
using System.Globalization;
using Microsoft.Win32;
using System.Collections;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace Starksoft.Aspen.GnuPG
{

    /// <summary>
    /// GnuPG output itemType.
    /// </summary>
    public enum OutputTypes
    {
        /// <summary>
        /// Ascii armor output.
        /// </summary>
        AsciiArmor,
        /// <summary>
        /// Binary output.
        /// </summary>
        Binary
    };   
        
    /// <summary>
    /// GnuPG output signature type.
    /// </summary>
    public enum OutputSignatureTypes
    {
        /// <summary>
        /// Make a clear text signature.
        /// </summary>
        ClearText,
        /// <summary>
        /// Make a detached signature.
        /// </summary>
        Detached,
        /// <summary>
        /// Make a signature.
        /// </summary>
        Signature
    };


    /// <summary>
    /// Interface class for GnuPG.
    /// </summary>
    /// <remarks>
    /// <para>
    /// GNU Privacy Guard from the GNU Project (also called GnuPG or GPG for short) is a highly regarded and supported opensource project that provides a complete and free implementation of the OpenPGP standard as defined by RFC2440. 
    /// GnuPG allows you to encrypt and sign your data and communication, manage your public and privde OpenPGP keys as well 
    /// as access modules for all kind of public key directories. 
    /// 
    /// Tested and works with Windows GPG.EXE, GPG2.EXE and Linux gpg and gpg2.  Note that on Linux there is a problem passing the password in as a file descripter and therefore 
    /// gpg2 may prompt the user for a password rather than accept the password via stdin.
    /// </para>
    /// <para>
    /// The Starksoft OpenPGP Component for .NET provides classes that interface with the gpg command line tool.  Support to encrypt, decrypt, sign, verify data using
    /// .NET IO Streams.  No temporary files are required and everything is handled through streams.  Any .NET Stream object can be used as long as the source stream can be read and the 
    /// destination stream can be written to.  But, in order for the Starksoft OpenPGP Component for .NET to work you must first install the lastest version of GnuPG which includes GPG.EXE.  
    /// You can obtain the latest version at http://www.gnupg.org/.  
    /// </para>
    /// <para>
    /// If you are new to GnuPG please install the application and then read how to generate new key pair or importing existing OpenPGP keys. 
    /// You can rad more about key generation and importing at http://www.gnupg.org/gph/en/manual.html#AEN26
    /// </para>
    /// <para>
    /// Encrypt File Example:
    /// <code>
    /// // create a new GnuPG object
    /// Gpg gpg = new Gpg();
    /// // specify a recipient that is already on the key-ring 
    /// gpg.Recipient = "myfriend@domain.com";
    /// // create an IO.Stream object to the source of the data and open it
    /// FileStream sourceFile = new FileStream(@"c:\temp\source.txt", FileMode.Open);
    /// // create an IO.Stream object to a where I want the encrypt data to go
    /// FileStream outputFile = new FileStream(@"c:\temp\output.txt", FileMode.Create);
    /// // encrypt the data using IO Streams - any type of input and output IO Stream can be used
    /// // as long as the source (input) stream can be read and the destination (output) stream 
    /// // can be written to
    /// gpg.Encrypt(sourceFile, outputFile);
    /// // close the files
    /// sourceFile.Close();
    /// outputFile.Close();
    /// </code>
    /// </para>
    /// <para>
    /// Decrypt File Example:
    /// <code>
    /// // create a new GnuPG object
    /// Gpg gpg = new Gpg();
    /// // create an IO.Stream object to the encrypted source of the data and open it 
    /// FileStream encryptedFile = new FileStream(@"c:\temp\output.txt", FileMode.Open);
    /// // create an IO.Stream object to a where you want the decrypted data to go
    /// FileStream unencryptedFile = new FileStream(@"c:\temp\unencrypted.txt", FileMode.Create);
    /// // specify our secret passphrase (if we have one)
    /// gpg.Passphrase = "secret passphrase";            
    /// // decrypt the data using IO Streams - any type of input and output IO Stream can be used
    /// // as long as the source (input) stream can be read and the destination (output) stream 
    /// // can be written to
    /// gpg.Decrypt(encryptedFile, unencryptedFile);
    /// // close the files
    /// encryptedFile.Close();
    /// unencryptedFile.Close();
    /// </code>
    /// </para>
    /// </remarks>
    public class Gpg : IDisposable 
    {
        private const long DEFAULT_COPY_BUFFER_SIZE = 4096;
        private const int DEFAULT_TIMEOUT_MS = 10000; // 10 seconds
        private const OutputTypes DEFAULT_OUTPUT_TYPE = OutputTypes.AsciiArmor;
        private const OutputSignatureTypes DEFAULT_SIGNATURE_TYPE = OutputSignatureTypes.Signature;
        private string _passphrase;
        private string _recipient;
        private string _localUser;
        private string _homePath;
        private string _binaryPath;
        private OutputTypes _outputType = DEFAULT_OUTPUT_TYPE;
        private int _timeout = DEFAULT_TIMEOUT_MS; 
        private Process _proc;
        private OutputSignatureTypes _outputSignatureType = DEFAULT_SIGNATURE_TYPE;
        private string _filename;
        private long _copyBufferSize = DEFAULT_COPY_BUFFER_SIZE;
        private bool _ignoreErrors;
        private string _userOptions;

        private Stream _outputStream;
        private Stream _errorStream;

        /// <summary>
        /// GnuPG actions.
        /// </summary>
        private enum ActionTypes
        { 
            /// <summary>
            /// Encrypt data.
            /// </summary>
            Encrypt, 
            /// <summary>
            /// Decrypt data.
            /// </summary>
            Decrypt,
            /// <summary>
            /// Sign data.
            /// </summary>
            Sign,
            /// <summary>
            /// Verify signed data.  
            /// </summary>
            /// <remarks>
            /// Note that gpg does not support decrypt+verify.  These operations
            /// must be done separately.
            /// </remarks>
            Verify,
            /// <summary>
            /// Sign and encrypt data.
            /// </summary>
            SignEncrypt,
            /// <summary>
            /// Import a key.
            /// </summary>
            Import
        };

        /// <summary>
        /// GnuPG interface class default constructor.
        /// </summary>
        /// <remarks>
        /// The GPG executable location is obtained by information in the windows registry.  Home path is set to the same as the
        /// GPG executable path.  Output itemType defaults to Ascii Armour.
        /// </remarks>
        public Gpg()
        {  }

        /// <summary>
        /// GnuPG interface class constuctor.
        /// </summary>
        /// <remarks>Output itemType defaults to Ascii Armour.</remarks>
        /// <param name="gpgBinaryPath">Full path to the gpg, gpg2, gpg.exe or gpg2.exe executable binary.</param>
        /// <param name="gpgHomePath">The home directory where files to encrypt and decrypt are located.</param>
        public Gpg(string gpgBinaryPath, string gpgHomePath)
        {
            _binaryPath = gpgBinaryPath;
            _homePath = gpgHomePath;
        }

        /// <summary>
        /// GnuPG interface class constuctor.
        /// </summary>
        /// <param name="gpgBinaryPath">Full path to the gpg, gpg2, gpg.exe or gpg2.exe executable binary.</param>
        public Gpg(string gpgBinaryPath)
        {
            _binaryPath = gpgBinaryPath;
        }

        /// <summary>
        /// Gets or sets the timeout value for the GnuPG operations in milliseconds. 
        /// </summary>
        /// <remarks>
        /// The default timeout is 10000 milliseconds (10 seconds).
        /// </remarks>
        public int Timeout
        {
            get{ return(_timeout);	}
            set{ _timeout = value;	}
        }

        /// <summary>
        /// Gets or set the recipient name to an associated public key.  This selected key will
        /// be used to encrypt data.
        /// </summary>
        public string Recipient
        {
            get { return _recipient; }
            set { _recipient = value; }
        }

        /// <summary>
        /// Gets or set the local user name to an associated private key.  This selected key will
        /// be used to sign or decrypt data.
        /// </summary>
        public string LocalUser
        {
            get { return _localUser; }
            set { _localUser = value; }
        }

        /// <summary>
        /// Gets or set the secret passphrase text value used to gain access to the secret key.
        /// </summary>
        public string Passphrase
        {
            get { return _passphrase; }
            set { _passphrase = value; }
        }

        /// <summary>
        /// Gets or sets the output type that GPG should generate.
        /// </summary>
        public OutputTypes OutputType
        {
            get { return _outputType; }
            set { _outputType = value; }
        }

        /// <summary>
        /// Gets or sets a specific user home path if the default home path should not be used.
        /// </summary>
        public string HomePath
        {
            get { return _homePath; }
            set { _homePath = value; }
        }

        /// <summary>
        /// Gets or sets the full path to the gpg or gpg2 binary on the system.
        /// </summary>
        public string BinaryPath
        {
            get { return _binaryPath; }
            set { _binaryPath = value; }
        }

        /// <summary>
        /// Gets or sets the output signature that GPG should generate.
        /// </summary>
        public OutputSignatureTypes OutputSignatureType
        {
            get { return _outputSignatureType; }
            set { _outputSignatureType = value; }
        }

        /// <summary>
        /// Gets or sets the arg --set-filename so that the name of the file is embedded in the encrypted gpg blob.
        /// </summary>
        public string Filename
        {
            get { return _filename; }
            set { _filename = value; }
        }

        /// <summary>
        /// Gets or sets the size of the copy buffer.  This value can be adjusted for performance when
        /// operating on large files.
        /// </summary>
        /// <value>The size of the copy buffer.</value>
        public long CopyBufferSize
        {
            get { return _copyBufferSize; }
            set { _copyBufferSize = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether execution errors reported by gpg should be ignored.
        /// </summary>
        /// <value><c>true</c> if ignore errors; otherwise, <c>false</c>.</value>
        public bool IgnoreErrors
        {
            get { return _ignoreErrors; }
            set { _ignoreErrors = value; }
        }

        /// <summary>
        /// Gets or sets user specified option GPG CLI argument string for any additional GPG options 
        /// that need to be specified by the user.
        /// </summary>
        /// <value>String containing GPGP user command line options.</value>
        public string UserOptions
        {
            get { return _userOptions; }
            set { _userOptions = value; }
        }

        /// <summary>
        /// Sign + encrypt data using the gpg executable with --sign arg.  Input data is provide via a stream.  Output
        /// data is returned as a stream.  Note that MemoryStream is supported for use.
        /// </summary>
        /// <param name="inputStream">Input stream data containing the data to encrypt.</param>
        /// <param name="outputStream">Output stream which will contain encrypted data.</param>
        public void SignAndEncrypt(Stream inputStream, Stream outputStream)
        {
            if (inputStream == null)
                throw new ArgumentNullException("Argument inputStream can not be null.");
            if (outputStream == null)
                throw new ArgumentNullException("Argument outputStream can not be null.");
            if (!inputStream.CanRead)
                throw new ArgumentException("Argument inputStream must be readable.");
            if (!outputStream.CanWrite)
                throw new ArgumentException("Argument outputStream must be writable.");

            ExecuteGpg(ActionTypes.SignEncrypt, inputStream, outputStream);
        }

        /// <summary>
        /// Encrypt data using the gpg executable.  Input data is provide via a stream.  Output
        /// data is returned as a stream.  Note that MemoryStream is supported for use.
        /// </summary>
        /// <param name="inputStream">Input stream data containing the data to encrypt.</param>
        /// <param name="outputStream">Output stream which will contain encrypted data.</param>
        /// <remarks>
        /// You must add the recipient's public key to your GnuPG key ring before calling this method.  
        ///  Please see the GnuPG documentation for more information.
        /// </remarks>
        public void Encrypt(Stream inputStream, Stream outputStream)
        {
            if (inputStream == null)
                throw new ArgumentNullException("Argument inputStream can not be null.");
            if (outputStream == null)
                throw new ArgumentNullException("Argument outputStream can not be null.");
            if (!inputStream.CanRead)
                throw new ArgumentException("Argument inputStream must be readable.");
            if (!outputStream.CanWrite)
                throw new ArgumentException("Argument outputStream must be writable.");

            ExecuteGpg(ActionTypes.Encrypt, inputStream, outputStream);
        }

        /// <summary>
        /// Decrypt OpenPGP data using IO Streams.
        /// </summary>
        /// <param name="inputStream">Input stream containing encrypted data.</param>
        /// <param name="outputStream">Output stream which will contain decrypted data.</param>
        /// <remarks>
        /// You must have the appropriate secret key on the GnuPG key ring before calling this method.  
        /// Please see the GnuPG documentation for more information.
        /// </remarks>
        public void Decrypt(Stream inputStream, Stream outputStream)
        {
            if (inputStream == null)
                throw new ArgumentNullException("Argument inputStream can not be null.");
            if (outputStream == null)
                throw new ArgumentNullException("Argument outputStream can not be null.");
            if (!inputStream.CanRead)
                throw new ArgumentException("Argument inputStream must be readable.");
            if (!outputStream.CanWrite)
                throw new ArgumentException("Argument outputStream must be writable.");
            
            ExecuteGpg(ActionTypes.Decrypt, inputStream, outputStream);
        }

        /// <summary>
        /// Sign input stream data with secret key.
        /// </summary>
        /// <param name="inputStream">Input stream containing data to sign.</param>
        /// <param name="outputStream">Output stream containing signed data.</param>
        /// <remarks>
        /// You must have the appropriate secret key on the GnuPG key ring before calling this method.  
        /// Please see the GnuPG documentation for more information.
        /// </remarks>
        public void Sign(Stream inputStream, Stream outputStream)
        {
            if (inputStream == null)
                throw new ArgumentNullException("Argument inputStream can not be null.");
            if (outputStream == null)
                throw new ArgumentNullException("Argument outputStream can not be null.");
            if (!inputStream.CanRead)
                throw new ArgumentException("Argument inputStream must be readable.");
            if (!outputStream.CanWrite)
                throw new ArgumentException("Argument outputStream must be writable.");

            ExecuteGpg(ActionTypes.Sign, inputStream, outputStream);
        }

        /// <summary>
        /// Verify signed input stream data with default user key.
        /// </summary>
        /// <param name="inputStream">Input stream containing signed data to verify.</param>
        public void Verify(Stream inputStream)
        {
            if (inputStream == null)
                throw new ArgumentNullException("Argument inputStream can not be null.");
            if (!inputStream.CanRead)
                throw new ArgumentException("Argument inputStream must be readable.");
            if (inputStream.Position == inputStream.Length)
                throw new ArgumentException ("Argument inputStream position cannot be set to the end.  Nothing to read.");

            ExecuteGpg(ActionTypes.Verify, inputStream, new MemoryStream());
        }

        /// <summary>
        /// Retrieves a collection of all secret keys.
        /// </summary>
        /// <returns>Collection of GnuPGKey objects.</returns>
        public GpgKeyCollection GetSecretKeys()
        {
            return new GpgKeyCollection(ExecuteGpgNoIO("--list-secret-keys"));
        }

        /// <summary>
        /// Retrieves a collection of all public keys.
        /// </summary>
        /// <returns>Collection of GnuPGKey objects.</returns>
        public GpgKeyCollection GetKeys()
        {
            return new GpgKeyCollection(ExecuteGpgNoIO("--list-keys"));
        }

        /// <summary>
        /// Import public key
        /// </summary>
        /// <param name="inputStream">Input stream containing public key.</param>
        /// <returns>Name of imported key.</returns>
        public string Import(Stream inputStream)
        {
            if (inputStream == null)
                throw new ArgumentNullException("Argument inputStream can not be null.");

            if (!inputStream.CanRead)
                throw new ArgumentException("Argument inputStream must be readable.");

            using (Stream outputStream = new MemoryStream())
            {
                ExecuteGpg(ActionTypes.Import, inputStream, outputStream);
                outputStream.Seek(0, SeekOrigin.Begin);
                StreamReader sr = new StreamReader(outputStream);
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    // output looks like this:
                    // gpg: key 13F1C2BB58E7940B: public key \"Joe Test <joe@domain.com>\" imported
                    // gpg: key FF5176CC: public key "One Team <user@domain.com>" imported
                    Match m = Regex.Match(line, @"imported|not changed");
                    if (m.Success)
                    {
                        return m.Groups[1].Value;
                    }
                }
            }

            throw new GpgException("Unable to identify name of imported key.  Possible import error or unrecognized text output.");
        }

        /// <summary>
        /// Gets the GPG binary version number as reported by the executable.
        /// </summary>
        /// <returns>GPG version number in the string format x.y.z</returns>
        public GpgVersion GetGpgVersion()
        {
            GpgVersion ver = GpgVersionParser.Parse(ExecuteGpgNoIO("--version"));
            return ver;
        }


        /// <summary>
        /// Executes the gpg program as a single command without input or output streams.
        /// This method is used to report data back from gpg such as key list information.
        /// </summary>
        /// <returns>The gpg command.</returns>
        /// <param name="gpgArgs">gpg command arguments to pass to gpg executable</param>
        private StreamReader ExecuteGpgNoIO(string gpgArgs)
        {
            StringBuilder options = new StringBuilder();
            options.Append(gpgArgs);
            // append a space to allow for additional commands to be added if required
            options.Append(" ");  

            //  set a home directory if the user specifies one
            if (!String.IsNullOrEmpty(_homePath))
                options.Append(String.Format(CultureInfo.InvariantCulture, "--homedir \"{0}\" ", _homePath));

            //  create a process info object with command line options
            ProcessStartInfo procInfo = new ProcessStartInfo(GetGpgBinaryPath(), options.ToString());

            //  init the procInfo object
            procInfo.CreateNoWindow = true;
            procInfo.UseShellExecute = false;
            procInfo.RedirectStandardInput = true;
            procInfo.RedirectStandardOutput = true;
            procInfo.RedirectStandardError = true;

            // create a memory stream to hold the output in memory
            MemoryStream outputStream = new MemoryStream();

            try
            {
                //  start the gpg process and get back a process start info object
                _proc = Process.Start(procInfo);
                _proc.StandardInput.Flush();
                //  wait for the process to return with an exit code (with a timeout variable)
                if (!_proc.WaitForExit(Timeout))
                    throw new GpgException("A time out event occurred while executing the GPG program.");
                //  if the process exit code is not 0 then read the error text from the gpg.exe process and throw an exception
                if (_proc.ExitCode != 0)
                    throw new GpgException(_proc.StandardError.ReadToEnd());
                // grab a copy of the console output
                CopyStream(_proc.StandardOutput.BaseStream, outputStream);
            }
            catch (Exception exp)
            {
                throw new GpgException(String.Format("An error occurred while trying to execute command {0}: {1}.", gpgArgs, exp));
            }
            finally
            {
                Dispose(true);
            }

            // reset the stream position and return as a StreamReader
            StreamReader reader = new StreamReader(outputStream);
            reader.BaseStream.Position = 0;
            return reader;
        }

        private string GetGpgArgs(ActionTypes action)
        {
            // validate input
            switch (action) 
            {
                case ActionTypes.Encrypt:
                case ActionTypes.SignEncrypt:
                if (String.IsNullOrEmpty (_recipient))
                    throw new GpgException ("A Recipient is required before encrypting data.  Please specify a valid recipient using the Recipient property on the GnuPG object.");
                    break;
            }

            StringBuilder options = new StringBuilder();

            //  set a home directory if the user specifies one
            if (!String.IsNullOrEmpty (_homePath))
                options.Append(String.Format(CultureInfo.InvariantCulture, "--homedir \"{0}\" ", _homePath));
            
            //  tell gpg to read the passphrase from the standard input so we can automate providing it
            options.Append("--passphrase-fd 0 ");

            // if gpg cli version is >= 2.1 then instruct gpg not to prompt for a password
            // by specifying the pinetnry-mode argument
            GpgVersion ver = GetGpgVersion();
            if ((ver.Major == 2 && ver.Minor >= 1) || ver.Major >= 3)
            {
                options.Append("--pinentry-mode loopback ");
            }

            //  turn off verbose statements
            options.Append("--no-verbose ");
            
            // use batch mode and never ask or allow interactive commands. 
            options.Append("--batch ");

            //  always use the trusted model so we don't get an interactive session with gpg.exe
            options.Append("--trust-model always ");

            // if provided specify the key to use by local user name
            if (!String.IsNullOrEmpty(_localUser))
                options.Append(String.Format(CultureInfo.InvariantCulture, "--local-user {0} ", _localUser));

            // if provided specify the recipient key to use by recipient user name
            if (!String.IsNullOrEmpty(_recipient))
                options.Append(String.Format(CultureInfo.InvariantCulture, "--recipient {0} ", _recipient));

            // add any user specific options if provided
            if (!String.IsNullOrEmpty(_userOptions))
                options.Append(_userOptions);

            //  handle the action
            switch (action)
            {
                case ActionTypes.Encrypt:
                    if (_outputType == OutputTypes.AsciiArmor)
                        options.Append ("--armor ");

                    // if a filename needs to be embedded in the encrypted blob, set it
                    if (!String.IsNullOrEmpty(_filename))
                        options.Append(String.Format(CultureInfo.InvariantCulture, "--set-filename \"{0}\" ", _filename));

                    options.Append ("--encrypt ");
                    break;
                case ActionTypes.Decrypt:
                    options.Append("--decrypt ");
                    break;
                case ActionTypes.Sign:
                    switch (_outputSignatureType)
                    {
                        case OutputSignatureTypes.ClearText:
                            options.Append("--clearsign ");
                            break;
                        case OutputSignatureTypes.Detached:
                            options.Append("--detach-sign ");
                            break;
                        case OutputSignatureTypes.Signature:
                            options.Append("--sign ");
                            break;
                    }
                    break;
                case ActionTypes.SignEncrypt:
                    if (_outputType == OutputTypes.AsciiArmor)
                        options.Append ("--armor ");

                    // if a filename needs to be embedded in the encrypted blob, set it
                    if (!String.IsNullOrEmpty(_filename))
                        options.Append(String.Format(CultureInfo.InvariantCulture, "--set-filename \"{0}\" ", _filename));

                    // determine which type of signature to generate
                    switch (_outputSignatureType)
                    {
                        case OutputSignatureTypes.ClearText:
                            options.Append("--clearsign ");
                            break;
                        case OutputSignatureTypes.Detached:
                            options.Append("--detach-sign ");
                            break;
                        case OutputSignatureTypes.Signature:
                            options.Append("--sign ");
                            break;
                    }

                    options.Append("--encrypt ");
                    break;
                case ActionTypes.Verify:
                    options.Append("--verify ");
                    break;
                case ActionTypes.Import:
                    options.Append("--import ");
                    break;
            }

            return options.ToString();
        }

        /// <summary>
        /// Executes the gpg program with the correct command line args based on the selected crypto action
        /// and feeds the inputStream data to the program and returns the output data as an outputStream.
        /// </summary>
        /// <param name="action">Action to perform (sign, encrypt, etc).</param>
        /// <param name="inputStream">Input stream.</param>
        /// <param name="outputStream">Output stream.</param>
        private void ExecuteGpg(ActionTypes action, Stream inputStream, Stream outputStream)
        {
            string gpgErrorText = string.Empty;
            string gpgPath = GetGpgBinaryPath();
            string gpgArgs = GetGpgArgs(action);

            //  create a process info object with command line options
            ProcessStartInfo procInfo = new ProcessStartInfo(gpgPath, gpgArgs);
            
            //  init the procInfo object
            procInfo.CreateNoWindow = true; 
            procInfo.UseShellExecute = false;  
            procInfo.RedirectStandardInput = true;
            procInfo.RedirectStandardOutput = true;
            procInfo.RedirectStandardError = true;

            try
            {
                //  start the gpg process and get back a process start info object
                _proc = Process.Start(procInfo);
                
                _proc.StandardInput.WriteLine(_passphrase);
                _proc.StandardInput.Flush();

                _outputStream = outputStream;
                _errorStream = new MemoryStream();

                // set up threads to run the output stream and error stream asynchronously
                ThreadStart outputEntry = new ThreadStart(AsyncOutputReader);
                Thread outputThread = new Thread(outputEntry);
                outputThread.Name = "gpg stdout";
                outputThread.Start();
                ThreadStart errorEntry = new ThreadStart(AsyncErrorReader);
                Thread errorThread = new Thread(errorEntry);
                errorThread.Name = "gpg stderr";
                errorThread.Start();

                //  copy the input stream to the process standard input object
                CopyStream(inputStream, _proc.StandardInput.BaseStream);
                                
                _proc.StandardInput.Flush();
               
                // close the process standard input object
                _proc.StandardInput.Close();

                //  wait for the process to return with an exit code (with a timeout variable)
                if (!_proc.WaitForExit(_timeout))
                {
                    throw new GpgException("A time out event occurred while executing the GPG program.");
                }

                if (!outputThread.Join(_timeout / 2))
                    outputThread.Abort();

                if (!errorThread.Join(_timeout / 2))
                    errorThread.Abort();

                //  if the process exit code is not 0 then read the error text from the gpg.exe process 
                if (_proc.ExitCode != 0  && !_ignoreErrors)
                {
                    StreamReader rerror = new StreamReader(_errorStream);
                    _errorStream.Position = 0;
                    gpgErrorText = rerror.ReadToEnd();
                }        

                // key name is output to error stream so read from the error stream and write out
                // to the output stream
                if (action == ActionTypes.Import)
                {
                    _errorStream.Position = 0;
                    byte[] buffer = new byte[4048];
                    int count;
                    while ((count = _errorStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        outputStream.Write(buffer, 0, count);
                    }
                }

            }
            catch (Exception exp)
            {
                throw new GpgException(String.Format(CultureInfo.InvariantCulture, "Error.  Action: {0}.  Command args: {1}", action.ToString(), procInfo.Arguments), exp);
            }
            finally
            {
                Dispose();
            }

            // throw an exception with the error information from the gpg.exe process
            if (gpgErrorText.IndexOf("bad passphrase") != -1)
                throw new GpgBadPassphraseException(gpgErrorText);

            if (gpgErrorText.Length > 0)
                throw new GpgException(gpgErrorText);
        }

        private string GetGpgBinaryPath()
        {
            if (String.IsNullOrEmpty(_binaryPath))
                throw new GpgException("gpg binary path not set");
                    
            if (!File.Exists(_binaryPath))
                throw new GpgException(String.Format("gpg binary path executable invalid or file permissions do not allow access: {0}", _binaryPath));

            return _binaryPath;
        }

        private void CopyStream(Stream input, Stream output)
        {
            if (_asyncWorker != null && _asyncWorker.CancellationPending)
                return;                 

            byte[] bytes = new byte[_copyBufferSize];
            int i;
            while ((i = input.Read(bytes, 0, bytes.Length)) != 0)
            {
                if (_asyncWorker != null && _asyncWorker.CancellationPending)
                    break;                 
                output.Write(bytes, 0, i);
            }
        }

        private void AsyncOutputReader()
        {
            Stream input = _proc.StandardOutput.BaseStream;
            Stream output = _outputStream;

            byte[] bytes = new byte[_copyBufferSize];
            int i;
            while ((i = input.Read(bytes, 0, bytes.Length)) != 0)
            {
                output.Write(bytes, 0, i);
            }
        }

        private void AsyncErrorReader()
        {
            Stream input = _proc.StandardError.BaseStream;
            Stream output = _errorStream;

            byte[] bytes = new byte[_copyBufferSize];
            int i;
            while ((i = input.Read(bytes, 0, bytes.Length)) != 0)
            {
                output.Write(bytes, 0, i);
            }

        }

        /// <summary>
        /// Dispose method for the GnuPG inteface class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose method for the GnuPG interface class.
        /// </summary>       
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_proc != null)
                {
                    //  close all the streams except for our the output stream
                    _proc.StandardInput.Close();
                    _proc.StandardOutput.Close();
                    _proc.StandardError.Close(); 
                    _proc.Close();
                }
            }

            if (_proc != null)
            {
                _proc.Dispose();
                _proc = null;
            }
        }

        /// <summary>
        /// Destructor method for the GnuPG interface class.
        /// </summary>
        ~Gpg()
        {
          Dispose (false);
        }

#region Asynchronous Methods

        private BackgroundWorker _asyncWorker;
        private Exception _asyncException;
        bool _asyncCancelled;

        /// <summary>
        /// Gets a value indicating whether an asynchronous operation is running.
        /// </summary>
        /// <remarks>Returns true if an asynchronous operation is running; otherwise, false.
        /// </remarks>
        public bool IsBusy
        {
            get { return _asyncWorker == null ? false : _asyncWorker.IsBusy; }
        }

        /// <summary>
        /// Gets a value indicating whether an asynchronous operation is cancelled.
        /// </summary>
        /// <remarks>Returns true if an asynchronous operation is cancelled; otherwise, false.
        /// </remarks>
        public bool IsAsyncCancelled
        {
            get { return _asyncCancelled; }
        }

        /// <summary>
        /// Cancels any asychronous operation that is currently active.
        /// </summary>
        public void CancelAsync()
        {
            if (_asyncWorker != null && !_asyncWorker.CancellationPending && _asyncWorker.IsBusy)
            {
                _asyncCancelled = true;
                _asyncWorker.CancelAsync();
            }
        }

        private void CreateAsyncWorker()
        {
            if (_asyncWorker != null)
                _asyncWorker.Dispose();
            _asyncException = null;
            _asyncWorker = null;
            _asyncCancelled = false;
            _asyncWorker = new BackgroundWorker();
        }

        /// <summary>
        /// Event handler for EncryptAsync method completed.
        /// </summary>
        public event EventHandler<EncryptAsyncCompletedEventArgs> EncryptAsyncCompleted;

        /// <summary>
        /// Starts asynchronous execution to encrypt OpenPGP data using IO Streams.
        /// </summary>
        /// <param name="inputStream">Input stream data containing the data to encrypt.</param>
        /// <param name="outputStream">Output stream which will contain encrypted data.</param>
        /// <remarks>
        /// You must add the recipient's public key to your GnuPG key ring before calling this method.  Please see the GnuPG documentation for more information.
        /// </remarks>
        public void EncryptAsync(Stream inputStream, Stream outputStream)
        {
          if (_asyncWorker != null && _asyncWorker.IsBusy)
              throw new InvalidOperationException("The GnuPG object is already busy executing another asynchronous operation.  You can only execute one asychronous method at a time.");

          CreateAsyncWorker();
          _asyncWorker.WorkerSupportsCancellation = true;
          _asyncWorker.DoWork += new DoWorkEventHandler(EncryptAsync_DoWork);
          _asyncWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(EncryptAsync_RunWorkerCompleted);
          Object[] args = new Object[2];
          args[0] = inputStream;
          args[1] = outputStream;
          _asyncWorker.RunWorkerAsync(args);
      }

        private void EncryptAsync_DoWork(object sender, DoWorkEventArgs e)
        {
          try
          {
              Object[] args = (Object[])e.Argument;
              Encrypt((Stream)args[0], (Stream)args[1]);
          }
          catch (Exception ex)
          {
              _asyncException = ex;
          }
        }

        private void EncryptAsync_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (EncryptAsyncCompleted != null)
                EncryptAsyncCompleted(this, new EncryptAsyncCompletedEventArgs(_asyncException, _asyncCancelled));
        }

        /// <summary>
        /// Event handler for DecryptAsync completed.
        /// </summary>
        public event EventHandler<DecryptAsyncCompletedEventArgs> DecryptAsyncCompleted;

        /// <summary>
        /// Starts asynchronous execution to decrypt OpenPGP data using IO Streams.
        /// </summary>
        /// <param name="inputStream">Input stream containing encrypted data.</param>
        /// <param name="outputStream">Output stream which will contain decrypted data.</param>
        public void DecryptAsync(Stream inputStream, Stream outputStream)
        {
            if (_asyncWorker != null && _asyncWorker.IsBusy)
                throw new InvalidOperationException("The Gpg object is already busy executing another asynchronous operation.  You can only execute one asychronous method at a time.");

            CreateAsyncWorker();
            _asyncWorker.WorkerSupportsCancellation = true;
            _asyncWorker.DoWork += new DoWorkEventHandler(DecryptAsync_DoWork);
            _asyncWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(DecryptAsync_RunWorkerCompleted);
            Object[] args = new Object[2];
            args[0] = inputStream;
            args[1] = outputStream;
            _asyncWorker.RunWorkerAsync(args);
        }

        private void DecryptAsync_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                Object[] args = (Object[])e.Argument;
                Decrypt((Stream)args[0], (Stream)args[1]);
            }
            catch (Exception ex)
            {
                _asyncException = ex;
            }
        }

        private void DecryptAsync_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (DecryptAsyncCompleted != null)
                DecryptAsyncCompleted(this, new DecryptAsyncCompletedEventArgs(_asyncException, _asyncCancelled));
        }

        /// <summary>
        /// Event handler for SignAsync completed.
        /// </summary>
        public event EventHandler<SignAsyncCompletedEventArgs> SignAsyncCompleted;

        /// <summary>
        /// Starts asynchronous execution to Sign OpenPGP data using IO Streams.
        /// </summary>
        /// <param name="inputStream">Input stream containing data to sign.</param>
        /// <param name="outputStream">Output stream which will contain Signed data.</param>
        public void SignAsync(Stream inputStream, Stream outputStream)
        {
            if (_asyncWorker != null && _asyncWorker.IsBusy)
                throw new InvalidOperationException("The Gpg object is already busy executing another asynchronous operation.  You can only execute one asychronous method at a time.");

            CreateAsyncWorker();
            _asyncWorker.WorkerSupportsCancellation = true;
            _asyncWorker.DoWork += new DoWorkEventHandler(SignAsync_DoWork);
            _asyncWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(SignAsync_RunWorkerCompleted);
            Object[] args = new Object[2];
            args[0] = inputStream;
            args[1] = outputStream;
            _asyncWorker.RunWorkerAsync(args);
        }

        private void SignAsync_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                Object[] args = (Object[])e.Argument;
                Sign((Stream)args[0], (Stream)args[1]);
            }
            catch (Exception ex)
            {
                _asyncException = ex;
            }
        }

        private void SignAsync_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (SignAsyncCompleted != null)
                SignAsyncCompleted(this, new SignAsyncCompletedEventArgs(_asyncException, _asyncCancelled));
        }





#endregion

  }
}
