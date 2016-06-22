# starksoft-aspen
security and cryptography library that includes a FTPS client, GnuPG wrapper, Windows smart card API, and proxy clients

Written in c# under LGPL3 and testing under .NET 4.0/4.5 and Mono 4.0/4.5.  Compiles with .NET/Mono 2.0+

official repository

nuget image: https://www.nuget.org/packages/starksoft.aspen

Gpg Features
* tested with Linux/Mono or Windows/.NET
* executes Windows gpg.exe / gpg2.exe or Linux gpg / gpg2 to sign, encrypt, decrypt, verify with streams
* retrieve a collection or DataSet of the keys
* output ASCII Armor or Binary
* optional passphrases when decryption data
* async methods
* import public keys
* select different signing options

Proxy Features
* tested with Linux/Mono or Windows/.NET
* SOCKS 4, 4a, 5 (with and without authentication)
* SOCKS5 supports Tor
* HTTP proxy
* factory abstraction
* open sockets supported by creating Tcpclient object and setting TcpClient.Client = socket and passing in the constructor

Smart card Features
* low level API for interfacing directly with smart cards
* works with Windows/.NET only
* p/invoke to system WINSCARD.DLL
* send adpu commands to device (send and receive)
* list readers
* list cards

Ftps client Features
* tested with Linux/Mono or Windows/.NET
* FTP RFC 959 and RFC 1579
* FTP over SSL / TLS
* asynchronous methods
* active and passive mode
* configurable active port range
* zLib data compression 
* upload and download file integrity support (CRC-32, SHA1, MD5)  with server XCRC, XSHA1, and XMD5 commands
* FXP 
* directory information as DataSet object or collection
* unix symbolic link and permission
* log transfer events
* exists() method
* recursive directory listing of files with GetDirListDeep()
* transfer files with streams
* ftp transfer restart
* throttle data transfers with MaxUploadSpeed and MaxDownloadSpeed 
* HTTP and SOCKS v4, 4a, and 5 proxy server support
* pluggable directory parser for archaic FTP directory listings
* directory and file listing wildcards and regex filters
* file transfer progress events
* move files on the FTP server using the Move() method
* binary or ASCII mode
* adjust date and time to the correct time zone of local machine
* SIZE FTP server command to retrieve the size of the file on server
* IPv6 support (EPRT and EPSV)
* specify IPv4 or IPv6 mode
* specify a specific client IP address to use with PORT and EPRT commands
* FEATS support and Features property collection
* HASH and RANG file integrity verification
* XSHA256 and XSHA512 support when compiled with .net 4+
* MLST and MLSD for directory and file listings. Fall back on LIST
* GetFileInfo() method to option information about a specific file
* percent completed, time to completion, bytes to transfer values available in the transfer event
* MFMT and MFCT for setting the modified date/time and created date/time 
