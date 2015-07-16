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
    /// GnuPG output signature itemType.
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
    /// as access modules for all kind of public key directories. GPG.EXE and GPG2.EXE, is a command line tool that is installed with GnuPG and contains features for easy integration with other applications. 
    /// </para>
    /// <para>
    /// The Starksoft OpenPGP Component for .NET provides classes that interface with the GPG.EXE command line tool.  The Starksoft OpenPGP libraries allows any .NET application to use GPG.EXE to encrypt or decypt data using
    /// .NET IO Streams.  No temporary files are required and everything is handled through streams.  Any .NET Stream object can be used as long as the source stream can be read and the 
    /// destination stream can be written to.  But, in order for the Starksoft OpenPGP Component for .NET to work you must first install the lastest version of GnuPG which includes GPG.EXE.  
    /// You can obtain the latest version at http://www.gnupg.org/.  See the GPG.EXE or GPG2.EXE tool documentation for information
    /// on how to add keys to the GPG key ring and creating your public and private keys.
    /// </para>
    /// <para>
    /// If you are new to GnuPG please install the application and then read how to generate new key pair or importing existing OpenPGP keys. 
    /// You can rad more about key generation and importing at http://www.gnupg.org/gph/en/manual.html#AEN26
    /// </para>
    /// <para>
    /// Encrypt File Example:
    /// <code>
    /// // create a new GnuPG object
    /// GnuPG gpg = new GnuPG();
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
    /// GnuPG gpg = new GnuPG();
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
        private string _passphrase;
        private string _recipient; // -r, --recipient USER-ID    encrypt for USER-ID
        private string _localUser; // -u, --local-user USER-ID   use USER-ID to sign or decrypt
        private string _homePath;
        private string _binaryPath;
        private OutputTypes _outputType;
        private OutputSignatureTypes _outputSignatureTypes;
		private int _timeout = 10000; // 10 seconds
		private Process _proc;

        private Stream _outputStream;
        private Stream _errorStream;

        private const string GPG_EXECUTABLE_V1 = "gpg.exe";
        private const string GPG_EXECUTABLE_V2 = "gpg2.exe";
        private const string GPG_REGISTRY_KEY_UNINSTALL_V1 = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\GnuPG";
        private const string GPG_REGISTRY_KEY_UNINSTALL_V2 = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\GPG4Win";
        private const string GPG_REGISTRY_VALUE_INSTALL_LOCATION = "InstallLocation";
        private const string GPG_REGISTRY_VALUE_DISPLAYVERSION = "DisplayVersion";
        private const string GPG_COMMON_INSTALLATION_PATH = @"C:\Program Files\GNU\GnuPG";

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
            Verify
		};

        /// <summary>
        /// GnuPG interface class default constructor.
        /// </summary>
        /// <remarks>
        /// The GPG executable location is obtained by information in the windows registry.  Home path is set to the same as the
        /// GPG executable path.  Output itemType defaults to Ascii Armour.
        /// </remarks>
     	public Gpg()
		{
            SetDefaults();
        }

        /// <summary>
        /// GnuPG interface class constuctor.
        /// </summary>
        /// <remarks>Output itemType defaults to Ascii Armour.</remarks>
        /// <param name="homePath">The home directory where files to encrypt and decrypt are located.</param>
        /// <param name="binaryPath">The GnuPG executable binary directory.</param>
		public Gpg(string homePath, string binaryPath)
		{
            _homePath = homePath;
            _binaryPath = binaryPath;
            SetDefaults();
		}

        /// <summary>
        /// GnuPG interface class constuctor.
        /// </summary>
        /// <param name="homePath">The home directory where files to encrypt and decrypt are located.</param>
        /// <remarks>
        /// The GPG executable location is obtained by information in the windows registry.  Output itemType defaults to Ascii Armour.
        /// </remarks>
        public Gpg(string homePath)
        {
            _homePath = homePath;
            SetDefaults();
        }

        /// <summary>
        /// Get or set the timeout value for the GnuPG operations in milliseconds. 
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
        /// Recipient name of the encrypted data.
        /// </summary>
        public string Recipient
        {
            get { return _recipient; }
            set { _recipient = value; }
        }

        /// <summary>
        /// Local user name to sign or decrypt data.
        /// </summary>
        public string LocalUser
        {
            get { return _localUser; }
            set { _localUser = value; }
        }

        /// <summary>
        /// Secret passphrase text value.
        /// </summary>
        public string Passphrase
        {
            get { return _passphrase; }
            set { _passphrase = value; }
        }

        /// <summary>
        /// The itemType of output that GPG should generate.
        /// </summary>
        public OutputTypes OutputType
        {
            get { return _outputType; }
            set { _outputType = value; }
        }

        /// <summary>
        /// Path to your home directory.
        /// </summary>
        public string HomePath
        {
            get { return _homePath; }
            set { _homePath = value; }
        }

        /// <summary>
        /// Path to the location of the GPG.EXE binary executable.
        /// </summary>
        public string BinaryPath
        {
            get { return _binaryPath; }
            set { _binaryPath = value; }
        }

        /// <summary>
        /// Encrypt OpenPGP data using IO Streams.
        /// </summary>
        /// <param name="inputStream">Input stream data containing the data to encrypt.</param>
        /// <param name="outputStream">Output stream which will contain encrypted data.</param>
        /// <remarks>
        /// You must add the recipient's public key to your GnuPG key ring before calling this method.  Please see the GnuPG documentation for more information.
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

            ExecuteGPG(ActionTypes.Encrypt, inputStream, outputStream);
        }

        /// <summary>
        /// Decrypt OpenPGP data using IO Streams.
        /// </summary>
        /// <param name="inputStream">Input stream containing encrypted data.</param>
        /// <param name="outputStream">Output stream which will contain decrypted data.</param>
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
            
            ExecuteGPG(ActionTypes.Decrypt, inputStream, outputStream);
        }

        /// <summary>
        /// Sign input stream data with default user key.
        /// </summary>
        /// <param name="inputStream">Input stream containing data to sign.</param>
        /// <param name="outputStream">Output stream containing signed data.</param>
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

            ExecuteGPG(ActionTypes.Sign, inputStream, outputStream);
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

            ExecuteGPG(ActionTypes.Verify, inputStream, new MemoryStream());
        }


        /// <summary>
        /// Retrieves a collection of secret keys from the GnuPG application.
        /// </summary>
        /// <returns>Collection of GnuPGKey objects.</returns>
        public GpgKeyCollection GetSecretKeys()
        {
            return new GpgKeyCollection(GetCommand("--list-secret-keys"));
        }

        /// <summary>
        /// Retrieves a collection of all keys from the GnuPG application.
        /// </summary>
        /// <returns>Collection of GnuPGKey objects.</returns>
        public GpgKeyCollection GetKeys()
        {
            return new GpgKeyCollection(GetCommand("--list-keys"));
        }

        private StreamReader GetCommand(string command)
        {
            StringBuilder options = new StringBuilder();

            //  set a home directory if the user specifies one
            if (_homePath != null && _homePath.Length != 0)
                options.Append(String.Format(CultureInfo.InvariantCulture, "--homedir \"{0}\" ", _homePath));

            options.Append(command);

            // get the full path to either GPG.EXE or GPG2.EXE
            string gpgPath = GetGnuPGPath();
                        
            //  create a process info object with command line options
            ProcessStartInfo procInfo = new ProcessStartInfo(gpgPath, options.ToString());

            //  init the procInfo object
            procInfo.CreateNoWindow = true;
            procInfo.UseShellExecute = false;
            procInfo.RedirectStandardInput = true;
            procInfo.RedirectStandardOutput = true;
            procInfo.RedirectStandardError = true;

            MemoryStream outputStream = new MemoryStream();

            try
            {
                //  start the gpg process and get back a process start info object
                _proc = Process.Start(procInfo);

                //  push passphrase onto stdin with a CRLF
                //_proc.StandardInput.WriteLine("");
                _proc.StandardInput.Flush();

                //  wait for the process to return with an exit code (with a timeout variable)
                if (!_proc.WaitForExit(Timeout))
                {
                    throw new GpgException("A time out event occurred while executing the GPG program.");
                }

                //  if the process exit code is not 0 then read the error text from the gpg.exe process and throw an exception
                if (_proc.ExitCode != 0)
                    throw new GpgException(_proc.StandardError.ReadToEnd());

                CopyStream(_proc.StandardOutput.BaseStream, outputStream);
            }
            catch (Exception exp)
            {
                throw new GpgException(String.Format("An error occurred while trying to execute command {0}.", command, exp));
            }
            finally
            {
                Dispose(true);
            }

            StreamReader reader = new StreamReader(outputStream);
            reader.BaseStream.Position = 0;
            return reader;
        }

        private string GetCmdLineSwitches(ActionTypes action)
        {
            StringBuilder options = new StringBuilder();

            //  set a home directory if the user specifies one
            if (_homePath != null && _homePath.Length != 0)
                options.Append(String.Format(CultureInfo.InvariantCulture, "--homedir \"{0}\" ", _homePath));

            //  read the passphrase from the standard input
            options.Append("--passphrase-fd 0 ");

            //  turn off verbose statements
            options.Append("--no-verbose --batch ");

            //  always use the trusted model so we don't get an interactive session with gpg.exe
            options.Append("--trust-model always ");

            //  handle the action
            switch (action)
            {
                case ActionTypes.Encrypt:
                    if (String.IsNullOrEmpty(_recipient))
                        throw new GpgException("A Recipient is required before encrypting data. Please specify a valid recipient using the Recipient property on the GnuPG object.");

                    //  check to see if the user wants ascii armor output or binary output (binary is the default mode for gpg)
                    if (_outputType == OutputTypes.AsciiArmor)
                        options.Append("--armor ");
                    options.Append(String.Format(CultureInfo.InvariantCulture, "--recipient \"{0}\" --encrypt ", _recipient));
                    break;
                case ActionTypes.Decrypt:
                    // set local user if specified
                    if (!String.IsNullOrEmpty(_localUser))
                        options.Append(String.Format(CultureInfo.InvariantCulture, "--local-user \"{0}\" ", _localUser));
                    options.Append("--decrypt ");
                    break;
                case ActionTypes.Sign:
                    // set local user if specified
                    if (!String.IsNullOrEmpty(_localUser))
                        options.Append(String.Format(CultureInfo.InvariantCulture, "--local-user \"{0}\" ", _localUser));
                    switch (_outputSignatureTypes)
                    {
                        case OutputSignatureTypes.ClearText:
                            options.Append("--clearsign ");
                            break;
                        case OutputSignatureTypes.Detached:
                            options.Append("--detach-sign ");
                            break;
                        case OutputSignatureTypes.Signature:
                        default:
                            options.Append("--sign ");
                            break;
                    }
                    break;
                case ActionTypes.Verify:
                    options.Append("--verify ");
                    break;
            }

            return options.ToString();
        }

		private void ExecuteGPG(ActionTypes action, Stream inputStream, Stream outputStream)
		{
            string gpgErrorText = string.Empty;

            string gpgPath = GetGnuPGPath();

            //  create a process info object with command line options
			ProcessStartInfo procInfo = new ProcessStartInfo(gpgPath, GetCmdLineSwitches(action));
			
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

                //  push passphrase onto stdin with a CRLF
                _proc.StandardInput.WriteLine(_passphrase);
                _proc.StandardInput.Flush();
                
                _outputStream = outputStream;
                _errorStream = new MemoryStream();

                // set up threads to run the output stream and error stream asynchronously
                ThreadStart outputEntry = new ThreadStart(AsyncOutputReader);
                Thread outputThread = new Thread(outputEntry);
                outputThread.Name = "GnuPG Output Thread";
                outputThread.Start();
                ThreadStart errorEntry = new ThreadStart(AsyncErrorReader);
                Thread errorThread = new Thread(errorEntry);
                errorThread.Name = "GnuPG Error Thread";
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
                if (_proc.ExitCode != 0)
                {
                    StreamReader rerror = new StreamReader(_errorStream);
                    _errorStream.Position = 0;
                    gpgErrorText = rerror.ReadToEnd();
                }        

            }
            catch (Exception exp)
            {
                throw new GpgException(String.Format(CultureInfo.InvariantCulture, "An error occurred while trying to {0} data using GnuPG.  GPG.EXE command switches used: {1}", action.ToString(), procInfo.Arguments), exp);
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

        private string GetGnuPGPath()
        {
            // if a full path is provided then use that
            if (!String.IsNullOrEmpty(_binaryPath))
            {
                if (!File.Exists(_binaryPath))
                    throw new GpgException(String.Format("binary path to GnuPG executable invalid or file permissions do not allow access: {0}", _binaryPath));
                return _binaryPath;
            }


            // try to find the Windows registry key that contains GPG.EXE (version 1)
            string pathv1 = "";
            RegistryKey hKeyLM_1 = Registry.LocalMachine;
            try
            {
                hKeyLM_1 = hKeyLM_1.OpenSubKey(GPG_REGISTRY_KEY_UNINSTALL_V1);
                pathv1 = (string)hKeyLM_1.GetValue(GPG_REGISTRY_VALUE_INSTALL_LOCATION);
                Path.Combine(pathv1, GPG_EXECUTABLE_V1);
            }
            finally
            {
                if (hKeyLM_1 != null)
                    hKeyLM_1.Close();
            }

            // try to find the Windows registry key that contains GPG2.EXE (version 2)
            string pathv2 = "";
            RegistryKey hKeyLM_2 = Registry.LocalMachine;
            try
            {
                hKeyLM_2 = hKeyLM_2.OpenSubKey(GPG_REGISTRY_KEY_UNINSTALL_V2);
                pathv2 = (string)hKeyLM_2.GetValue(GPG_REGISTRY_VALUE_INSTALL_LOCATION);
                Path.Combine(pathv2, GPG_EXECUTABLE_V2);
            }
            finally
            {
                if (hKeyLM_2 != null)
                    hKeyLM_2.Close();
            }
            
            // try to figure out which path is good
            if (File.Exists(pathv2))
                return pathv2;
            else if (File.Exists(pathv1))
                return pathv1;
            else if (File.Exists(GPG_COMMON_INSTALLATION_PATH))
                return GPG_COMMON_INSTALLATION_PATH;
            else
                throw new GpgException("cannot find a valid GPG.EXE or GPG2.EXE file path - set the property 'BinaryPath' to specify a hard path to the executable or verify file permissions are correct.");
        }

        private void CopyStream(Stream input, Stream output)
        {
            if (_asyncWorker != null && _asyncWorker.CancellationPending)
                return;                 

            const int BUFFER_SIZE = 4096;
            byte[] bytes = new byte[BUFFER_SIZE];
            int i;
            while ((i = input.Read(bytes, 0, bytes.Length)) != 0)
            {
                if (_asyncWorker != null && _asyncWorker.CancellationPending)
                    break;                 
                output.Write(bytes, 0, i);
            }
        }

        private void SetDefaults()
        {
            _outputType = OutputTypes.AsciiArmor;
        }

        
        private void AsyncOutputReader()
        {
            Stream input = _proc.StandardOutput.BaseStream;
            Stream output = _outputStream;

            const int BUFFER_SIZE = 4096;
            byte[] bytes = new byte[BUFFER_SIZE];
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

            const int BUFFER_SIZE = 4096;
            byte[] bytes = new byte[BUFFER_SIZE];
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
                throw new InvalidOperationException("The GnuPG object is already busy executing another asynchronous operation.  You can only execute one asychronous method at a time.");

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
                throw new InvalidOperationException("The GnuPG object is already busy executing another asynchronous operation.  You can only execute one asychronous method at a time.");

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
