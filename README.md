# starksoft-aspen
cryptography library that support FTPS, GnuPG, Smartbadges, and proxy servers

* Use with .NET Framework 2.0, 3.0, 3.5, and 4.0 and Mono 2.x without recompiling.
* 100% CLR compliant C# managed code using .NET sockets - no third party code or dependencies.
* Execute commands in the background using fire and forget async command equivalents (OpenAsync(), PutFileAsync(), GetFileAsync(), etc).
* Transfer data both active and passive mode.
* Control which active ports to use better firewall support.
* Speed up data tranfers with built-in support for data compression (compress algorithm).
* Secure data transfers with FTPS (SSL 2.0, SSL 3.0, and TLS 1.0 explicit and implicit)
* Verify file upload and download file integrity with built-in support for CRC-32, SHA1, and MD5 hash checking to automatically compare files on server with XCRC, XSHA1, and XMD5 commands.
* Transfer files server-to-server using FXP.
* Retrieve file directory information as a standard DataSet object or an object collection for ease of use. Data bind the results directly to a data aware object.
* Extract symbolic link and permission information from UNIX directory listing formats.
* Easily log transfer events.
* Easily test to see if a file exists on the server with the method Exists().
* Retrieve recursive directory listing of files on the FTP server using the method GetDirListDeep().
* Upload and download files using any .NET stream object.
* Easily restart FTP transfer. Component will automatically figure out the restart byte position for you.
* Throttle data transfers with MaxUploadSpeed and MaxDownloadSpeed properties to prevent bandwidth staturation.
* Connect through HTTP and SOCKS v4, 4a, and 5 proxy servers.
* Parse UNIX and DOS directory listing formats. Parse UNIX file attributes and symbolic links.
* Create your own pluggable directory parser with your own code for archaic FTP directory listings by implementing the IFtpItemParser interface.
* Filter directory and file listings using wildcards and regular expressions.
* Monitor file transfer progress using events.
* Easily move files on the FTP server using the Move() method. This feature is especially useful when archiving data on the server after processing or downloading files.
* Transfer your files in binary or ASCII mode.
* Automatically adjusts date and time to the correct time zone of local machine.
* Supports SIZE FTP server command to retrieve the size of the file.
* Implements RFC 959 and RFC 1579.

    Full IPv6 support (EPRT and EPSV). User can now specify IPv4 or IPv6 mode.
    Allow user to optionally specify a specific client IP address to use with PORT and EPRT commands.
    UTF8 encoding set in Open() method.
    Comprehensive FEATS support with Features collection and method testing.
    Support for HASH and RANG command for file integrity checking.
    Support for XSHA256 and XSHA512.
    Allow user to specify a specific IPv4 or IPV6 address to use for the client.
    Full support for MLST and MLSD for directory and file listings. Fall back on LIST command if not available.
    New Options() method to execute OPTS command.
    New GetFileInfo() method to option information about a specific file.
    New percent completed, time to completion, bytes to transfer values available in the transfer event.
    Refactoring of code and better exception handling.
    Extensive refactoring on Open() method to deal with servers that have different feature sets.
    Support for MFMT and MFCT command for setting the modified date/time and created date/time on FTP servers that support those features.
    Now sending CLNT command for those servers that care.
