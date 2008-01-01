#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

#region CVS Version Log
/*
 * $Log: FileHelper.cs,v $
 * Revision 1.11  2007/02/07 15:19:43  t_rendelmann
 * fixed: line endings (source code)
 *
 * Revision 1.10  2006/12/24 15:18:54  carnage4life
 * Added support for erroring when enclosure cache limit is reached
 *
 * Revision 1.9  2006/10/24 15:15:13  carnage4life
 * Changed the default folders for podcasts
 *
 * Revision 1.8  2006/10/17 15:23:26  carnage4life
 * Integrated BITS code for downloading enclosures
 *
 * Revision 1.7  2006/09/29 18:06:26  t_rendelmann
 * added CVS change history
 *
 */
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using ICSharpCode.SharpZipLib.Zip;

namespace NewsComponents.Utils
{
	/// <summary>
	/// File Helper class.
	/// </summary>
	public sealed class FileHelper {
	 
		#region private vars
		private static readonly int msecsBetweenRetries = 100;
		private static readonly int bufferSize = 1024 * 20;	// 20K read/write buffer
		#endregion


		#region public members

		/// <summary>
		/// This marker is written at the end of the binary files containing the content of 
		/// RSS items to indicate the end of the file. 
		/// </summary>
		public static string EndOfBinaryFileMarker = ".\r\n";


		/// <summary>
		/// Converts the input string to a valid file name. 
		/// </summary>
		/// <param name="name">The input string</param>
		/// <returns>A valid file name</returns>
		public static string CreateValidFileName(string name){

			// first trim the raw string
			string safe = name.Trim();				 			
 
			// trim out illegal characters
			safe = safe.Replace(Path.VolumeSeparatorChar,'_');
			safe = safe.Replace(Path.DirectorySeparatorChar,'_');
			safe = safe.Replace(Path.AltDirectorySeparatorChar,'_');
			safe = safe.Replace(Path.PathSeparator,'_');			

			foreach(Char c in Path.GetInvalidFileNameChars()){				
				safe = safe.Replace(c,'_');
			}			
 
			// trim the length
			if(safe.Length > 250)
				safe = safe.Substring(0, 249);
 
			// clean the beginning and end of the filename
			char[] replace = {'-','.'};
			safe = safe.TrimStart(replace);
			safe = safe.TrimEnd(replace);
 
			return safe;			
		}

		/// <summary>
		/// Tries to open a file for write. On concurrent access failures it will retry 10 times.
		/// </summary>
		/// <param name="fileName">filename to open</param>
		/// <returns>FielStream</returns>
		public static FileStream OpenForWrite(string fileName ) {
			FileStream fileStream = null;
			int retries = 10;

			while ( retries > 0 ) {
				try {
					fileStream = new FileStream(
						fileName,
						FileMode.Create,
						FileAccess.Write,
						FileShare.None,bufferSize);
				}
				catch (Exception) {
					retries--;
					
					if (retries <= 0)
						throw;	// giving up and report error

					// yield control to other threads so that we get a little
					// wait before we retry.
					Thread.Sleep(msecsBetweenRetries);
					continue;    
				}
				break;                
			}
			return fileStream;
		}

		/// <summary>
		/// Tries to open a file for write (append). On concurrent access failures it will retry 10 times.
		/// </summary>
		/// <param name="fileName">filename to open</param>
		/// <returns>FielStream</returns>
		public static FileStream OpenForWriteAppend(string fileName ) {
			FileStream fileStream = null;
			int retries = 10;

			while ( retries > 0 ) {
				try {
					fileStream = new FileStream(
						fileName,
						FileMode.Append,
						FileAccess.Write,
						FileShare.None,bufferSize);
				}
				catch (Exception) {
					retries--;

					if (retries <= 0)
						throw;	// giving up and report error

					// yield control to other threads so that we get a little
					// wait before we retry.
					Thread.Sleep(msecsBetweenRetries);
					continue;    
				}
				break;                
			}
			return fileStream;
		}

		/// <summary>
		/// Tries to open a file for read/write. On concurrent access failures it will retry 10 times.
		/// </summary>
		/// <param name="fileName">filename to open</param>
		/// <returns>FielStream</returns>
		public static FileStream OpenForReadWrite(string fileName) {
			FileStream fileStream = null;
			int retries = 10;

			while ( retries > 0 ) {
				try {
					fileStream = new FileStream(
						fileName,
						FileMode.OpenOrCreate,
						FileAccess.ReadWrite,
						FileShare.None,bufferSize);
				}
				catch {
					retries--;

					if (retries <= 0)
						throw;	// giving up and report error

					// yield control to other threads so that we get a little
					// wait before we retry.
					Thread.Sleep(msecsBetweenRetries);
					continue;    
				}
				break;                
			}
			return fileStream;
		}

		/// <summary>
		/// Tries to open a file for read. On concurrent access failures it will retry 10 times.
		/// </summary>
		/// <param name="fileName">filename to open</param>
		/// <returns>FielStream</returns>
		public static FileStream OpenForRead(string fileName) {
			FileStream fileStream = null;
			int retries = 10;

			while ( retries > 0 ) {
				try {
					fileStream = new FileStream(
						fileName,
						FileMode.Open,
						FileAccess.Read,
						FileShare.Read,bufferSize);
				}
				catch {
					retries--;

					if (retries <= 0)
						throw;	// giving up and report error

					// yield control to other threads so that we get a little
					// wait before we retry.
					Thread.Sleep(msecsBetweenRetries);
					continue;    
				}
				break;                
			}
			return fileStream;
		}

		/// <summary>
		/// Tries to delete a file. On concurrent access failures it will retry 10 times.
		/// </summary>
		/// <param name="fileName">filename to delete</param>
		/// <returns>FielStream</returns>
		public static void Delete(string fileName) {
			int retries = 10;

			while ( retries > 0 ) {
				try {
					File.Delete(fileName);
				}	catch {
					retries--;

					if (retries <= 0)
						throw;	// giving up and report error

					// yield control to other threads so that we get a little
					// wait before we retry.
					Thread.Sleep(msecsBetweenRetries);
					continue;    
				}
				break;                
			}
		}

		/// <summary>
		/// Writes a stream to the fielsystem with the full qualified fileName in a save manner: 
		/// 1. It writes a file named <c>fileName</c>+".new" first. 
		/// 2. It renames the old file (if exists) to <c>fileName</c>+".bak".
		/// 3. It renames the file written in step (1) to <c>fileName</c>.
		/// </summary>
		/// <param name="fileName">Full qualified file name incl. path</param>
		/// <param name="stream">The Stream to be written.</param>
		/// <exception cref="ArgumentNullException">If fileName or stream is null</exception>
		/// <returns>True on success, else false</returns>
		/// <remarks>The caller have to close the provided stream by itself!</remarks>
		public static bool WriteStreamWithBackup(string fileName, Stream stream) {
			
			if (string.IsNullOrEmpty(fileName))
				throw new ArgumentNullException("fileName");
			if (stream == null)
				throw new ArgumentNullException("stream");

			bool saveSuccess;

            try
            {
                if (stream.CanSeek) // reset stream pointer
                    stream.Seek(0, SeekOrigin.Begin);

                string dataPath = fileName + ".new";
                Stream fsStream = OpenForWrite(dataPath);
                if (fsStream == null)
                {
                    // to get the exception:
                    fsStream = new FileStream(dataPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize);
                }
                try
                {
                    int size = 2048;
                    byte[] writeData = new byte[size];
                    while (true)
                    {
                        size = stream.Read(writeData, 0, size);
                        if (size > 0)
                        {
                            fsStream.Write(writeData, 0, size);
                        }
                        else
                        {
                            break;
                        }
                    }
                    fsStream.Flush();
                    saveSuccess = true;
                }
                finally
                {
                    fsStream.Close();
                }
                // only after successful save of the stream, copy/rename/move to real location
                // any exception will skip this code and go to the hanlder below

                Delete(fileName + ".bak");
                if (File.Exists(fileName))
                {
                    // if it is the very initial write, there is nothing to rename to .bak
                    File.Move(fileName, fileName + ".bak");
                }
                File.Move(fileName + ".new", fileName);
            }
            catch (Exception ex)
            {
                // write out message to the attached trace listeners
                saveSuccess = false;
                Trace.WriteLine("WriteStreamWithBackup('" + fileName + "') caused exception: " + ex.Message);
            }

			return saveSuccess;
		}

		/// <summary>
		/// Writes a stream to the filesystem with the full qualified fileName in a save manner: 
		/// 1. It writes a file named <c>fileName</c>+".new" first. 
		/// 2. It deletes the old file (if exists)
		/// 3. It renames the file written in step (1) to <c>fileName</c>.
		/// </summary>
		/// <param name="fileName">Full qualified file name incl. path</param>
		/// <param name="stream">The Stream to be written.</param>
		/// <exception cref="ArgumentNullException">If fileName or stream is null</exception>
		/// <returns>True on success, else false</returns>
		/// <remarks>The caller have to close the provided stream by itself!</remarks>
		public static bool WriteStreamWithRename(string fileName, Stream stream) {
			
			if (string.IsNullOrEmpty(fileName))
				throw new ArgumentNullException("fileName");
			if (stream == null)
				throw new ArgumentNullException("stream");

			bool saveSuccess;

            try
            {
                if (stream.CanSeek) // reset stream pointer
                    stream.Seek(0, SeekOrigin.Begin);

                string dataPath = fileName + ".new";
                Stream fsStream = OpenForWrite(dataPath);
                if (fsStream == null)
                {
                    // to get the exception:
                    fsStream = new FileStream(dataPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize);
                }
                try
                {
                    int size = 2048;
                    byte[] writeData = new byte[size];
                    while (true)
                    {
                        size = stream.Read(writeData, 0, size);
                        if (size > 0)
                        {
                            fsStream.Write(writeData, 0, size);
                        }
                        else
                        {
                            break;
                        }
                    }
                    fsStream.Flush();
                    saveSuccess = true;
                }
                finally
                {
                    fsStream.Close();
                }
                // only after successful save of the stream, copy/rename/move to real location


                if (File.Exists(fileName))
                {
                    // if it is the very initial write, there is nothing to delete
                    Delete(fileName);
                }
                File.Move(fileName + ".new", fileName);
            }
            catch (Exception ex)
            {
                // write out message to the attached trace listeners
                saveSuccess = false;
                Trace.WriteLine("WriteStreamWithRename('" + fileName + "') caused exception: " + ex.Message);
            }

			return saveSuccess;
		}

		/// <summary>
		/// Helper to copy a non-seekable stream to a seekable memory stream.
		/// </summary>
		/// <param name="input">A (optional) non-seekable stream</param>
		/// <returns>Stream positioned at the beginning</returns>
		public static Stream StreamToMemory(Stream input) {
			const int BUFFER_SIZE = 4096;	// 4K read buffer
			MemoryStream output = new MemoryStream();
			int size = BUFFER_SIZE;
			byte[] writeData = new byte[BUFFER_SIZE];
			while (true) {
				size = input.Read(writeData, 0, size);
				if (size > 0) {
					output.Write(writeData, 0, size);
				} 
				else {
					break;
				}
			}
			output.Seek(0, SeekOrigin.Begin);
			return output;
		}

		/// <summary>
		/// Helper to copy a whole file from disk to a seekable memory stream.
		/// </summary>
		/// <param name="file">A filename</param>
		/// <returns>Stream positioned at the beginning</returns>
		public static Stream FileToMemory(string file) {
			using (Stream input = OpenForRead(file)) {
				return StreamToMemory(input);
			}
		}

		/// <summary>
		/// Returns whether the path is a UNC path.
		/// </summary>
		/// <param name="path">The path string.</param>
		/// <returns><c>true</c> if the path is a UNC path.</returns>
		public static bool IsUncPath( string path ) {
			//  FIRST, check if this is a URL or a UNC path; do this by attempting to construct uri object from it
			try {
				Uri url = new Uri( path );
				if( url.IsUnc ) 
					return true;
			} catch (UriFormatException) {}

			return false;
		}

		/// <summary>
		/// Takes a UNC or URL path, determines which it is (NOT hardened against bad strings, assumes one or the other is present)
		/// and returns the path with correct trailing slash: backslash for UNC or
		/// slash mark for URL.
		/// </summary>
		/// <param name="path">The URL or UNC string.</param>
		/// <returns>Path with correct terminal slash.</returns>
		public static string AppendSlashUrlOrUnc( string path ) {					
			if( IsUncPath( path ) ) {
				//  it is a unc path, so decorate the end with a back-slash (to correct misconfigurations, defend against trivial errors)
				return AppendTerminalBackslash( path );
			}
			else {
				//  assume URL here
				return AppendTerminalForwardSlash( path );
			}
		}

		/// <summary>
		/// If not present appends terminal backslash to paths.
		/// </summary>
		/// <param name="path">A path string; for example, "C:\Temp".</param>
		/// <returns>A path string with trailing backslash; for example, "C:\Temp\".</returns>
		public static string AppendTerminalBackslash( string path ) {
			if( path.IndexOf( Path.DirectorySeparatorChar, path.Length - 1 ) == -1 ) {
				return path + Path.DirectorySeparatorChar;
			}
			else {
				return path;
			}
		}
		
		/// <summary>
		/// Appends a terminal slash mark if there is not already one; returns corrected path.
		/// </summary>
		/// <param name="path">The path that may be missing a terminal slash mark.</param>
		/// <returns>The corrected path with terminal slash mark.</returns>
		public static string AppendTerminalForwardSlash( string path ) {
			if( path.IndexOf( Path.AltDirectorySeparatorChar, path.Length - 1 ) == -1 ) {
				return path + Path.AltDirectorySeparatorChar;
			}
			else {
				return path;
			}
		}

		/// <summary>
		/// Creates a new temporary folder under the system temp folder
		/// and returns its full pathname.
		/// </summary>
		/// <returns>The full temp path string.</returns>
		public static string CreateTemporaryFolder() {
			return Path.Combine( Path.GetTempPath(), Path.GetFileNameWithoutExtension( Path.GetTempFileName() ) );
		}
		
		/// <summary>
		/// Copies files from the source to destination directories. Directory.Move is not 
		/// suitable here because the downloader may still have the temporary 
		/// directory locked. 
		/// </summary>
		/// <param name="sourcePath">The source path.</param>
		/// <param name="destinationPath">The destination path.</param>
		public static void CopyDirectory( string sourcePath, string destinationPath ) {
			CopyDirectory( sourcePath, destinationPath, true );
		}
		
		/// <summary>
		/// Copies files from the source to destination directories. Directory.Move is not 
		/// suitable here because the downloader may still have the temporary 
		/// directory locked. 
		/// </summary>
		/// <param name="sourcePath">The source path.</param>
		/// <param name="destinationPath">The destination path.</param>
		/// <param name="overwrite">Indicates whether the destination files should be overwritten.</param>
		public static void CopyDirectory( string sourcePath, string destinationPath, bool overwrite ) {
			CopyDirRecurse( sourcePath, destinationPath, destinationPath, overwrite );
		}

		/// <summary>
		/// Move a file from a folder to a new one.
		/// </summary>
		/// <param name="existingFileName">The original file name.</param>
		/// <param name="newFileName">The new file name.</param>
		/// <param name="flags">Flags about how to move the files.</param>
		/// <returns>indicates whether the file was moved.</returns>
		public static bool MoveFile( string existingFileName, string newFileName, MoveFileFlag flags) {

			int retries = 10;

			while ( retries > 0 ) {
				try {
					return MoveFileEx( existingFileName, newFileName, flags );
				}
				catch (Exception) {
					retries--;

					if (retries <= 0)
						throw;	// giving up and report error

					// yield control to other threads so that we get a little
					// wait before we retry.
					Thread.Sleep(msecsBetweenRetries);
					continue;    
				}				
			}//while 

			return false; 
		}

		/// <summary>
		/// Deletes a folder. If the folder cannot be deleted at the time this method is called,
		/// the deletion operation is delayed until the next system boot.
		/// </summary>
		/// <param name="folderPath">The directory to be removed</param>
		public static void DestroyFolder( string folderPath ) {
			try {
				if ( Directory.Exists( folderPath) ) {
					Directory.Delete( folderPath, true );
				}
			}
			catch( Exception ) {
				// If we couldn't remove the files, postpone it to the next system reboot
				if ( Directory.Exists( folderPath) ) {
					MoveFile(
						folderPath,
						null,
						MoveFileFlag.DelayUntilReboot );
				}
			}
		}

		/// <summary>
		/// Deletes a file. If the file cannot be deleted at the time this method is called,
		/// the deletion operation is delayed until the next system boot.
		/// </summary>
		/// <param name="filePath">The file to be removed</param>
		public static void DestroyFile( string filePath ) {
			try {
				if ( File.Exists( filePath ) ) {
					File.Delete( filePath );
				}
			}
			catch {
				if ( File.Exists( filePath ) ) {
					MoveFile(
						filePath,
						null,
						MoveFileFlag.DelayUntilReboot );
				}
			}
		}


		#region zip support
		
		public static void ZipFiles(string[] files, string zipToFile) {
			using (ZipOutputStream zipStream = OpenForWriteCompressed(zipToFile)) {
				ZipFiles(files, zipStream);
			}
		}
		
		#endregion

		#endregion

		#region Private members

		#region zip support
		
		/// <summary>
		/// Opens a file stream for write compressed content.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		/// <returns></returns>
		private static ZipOutputStream OpenForWriteCompressed(string fileName) {
			return new ZipOutputStream(OpenForWrite(fileName));
		}

		/// <summary>
		/// Zips up the files listed into the specified stream
		/// </summary>
		/// <param name="files">The list of files to zip</param>
		/// <param name="zos">The stream to store the zipped files</param>
		private static void ZipFiles(IEnumerable<string> files, ZipOutputStream zos){
		
			byte[] buffer = new byte[bufferSize];
		    zos.SetLevel(5); 

			foreach(string file in files){

				string fileToProcess = file;
		try_again:				
				if(File.Exists(fileToProcess)){
					try {
						using (FileStream fs = OpenForRead(fileToProcess)) {

							ZipEntry entry = new ZipEntry(Path.GetFileName(file));
							zos.PutNextEntry(entry);

						    int size;
						    do {
						
								size = fs.Read(buffer, 0, buffer.Length);
								zos.Write(buffer, 0, size);

							} while (size > 0);

						}

					} catch (IOException) {
						// file possibly in use/locked, try to copy and use that:
						string newFile = Path.GetTempFileName();
						File.Copy(file, newFile, true);
						fileToProcess = newFile;
						goto try_again;
					}
				}
			}
            		
			zos.Finish();		
		}
		#endregion

		/// <summary>
		/// API declaration of the Win32 function.
		/// </summary>
		/// <param name="lpExistingFileName">Existing file path.</param>
		/// <param name="lpNewFileName">The file path.</param>
		/// <param name="dwFlags">Move file flags.</param>
		/// <returns>Whether the file was moved or not.</returns>
		[DllImport("KERNEL32.DLL")]
		private static extern bool MoveFileEx( 
			string lpExistingFileName, 
			string lpNewFileName,
            MoveFileFlag dwFlags);


		/// <summary>
		/// Determines the size of a Directory
		/// </summary>
		/// <param name="d">The directory</param>
		/// <returns>The size of the directory</returns>
		public static long GetSize(DirectoryInfo d) {    
			long Size = 0;    
			// Add file sizes.
			FileInfo[] fis = d.GetFiles();
			foreach (FileInfo fi in fis) {      
				Size += fi.Length;    
			}
			// Add subdirectory sizes.
			DirectoryInfo[] dis = d.GetDirectories();
			foreach (DirectoryInfo di in dis) {
				Size += GetSize(di);   
			}
			return(Size);  
		}

		/// <summary>
		/// Utility function that recursively copies directories and files.
		/// Again, we could use Directory.Move but we need to preserve the original.
		/// </summary>
		/// <param name="sourcePath">The source path to copy.</param>
		/// <param name="destinationPath">The destination path to copy to.</param>
		/// <param name="originalDestination">The original dstination path.</param>
		/// <param name="overwrite">Whether the folders should be copied recursively.</param>
		private static void CopyDirRecurse( string sourcePath, string destinationPath, string originalDestination, bool overwrite ) {
			//  ensure terminal backslash
			sourcePath = AppendTerminalBackslash( sourcePath );
			destinationPath = AppendTerminalBackslash( destinationPath );

			if ( !Directory.Exists( destinationPath ) ) {
				Directory.CreateDirectory( destinationPath );
			}

			//  get dir info which may be file or dir info object
			DirectoryInfo dirInfo = new DirectoryInfo( sourcePath );

		    foreach( FileSystemInfo fsi in dirInfo.GetFileSystemInfos() ) {
				if ( fsi is FileInfo )
				{
				    string destFileName = Path.Combine( destinationPath, fsi.Name );

				    //  if file object just copy when overwrite is allowed
					if ( File.Exists( destFileName ) ) {
						if ( overwrite ) {
							File.Copy( fsi.FullName, destFileName, true );
						}
					}
					else {
						File.Copy( fsi.FullName, destFileName );
					}
				}
				else {
					// avoid this recursion path, otherwise copying directories as child directories
					// would be an endless recursion (up to an stack-overflow exception).
					if ( fsi.FullName != originalDestination ) {
						//  must be a directory, create destination sub-folder and recurse to copy files
						//Directory.CreateDirectory( destinationPath + fsi.Name );
						CopyDirRecurse( fsi.FullName, destinationPath + fsi.Name, originalDestination, overwrite );
					}
				}
			}
		}
		#endregion

		#region constructor

		private FileHelper() {}

		#endregion

	}

	#region MoveFileFlag enum
	/// <summary>
	/// Indicates how to proceed with the move file operation. 
	/// </summary>
	[Flags]
	public enum MoveFileFlag
	{
		/// <summary>
		/// Perform a default move funtion.
		/// </summary>
		None				= 0x00000000,
		/// <summary>
		/// If the target file exists, the move function will replace it.
		/// </summary>
		ReplaceExisting     = 0x00000001,
		/// <summary>
		/// If the file is to be moved to a different volume, 
		/// the function simulates the move by using the CopyFile and DeleteFile functions. 
		/// </summary>
		CopyAllowed         = 0x00000002,
		/// <summary>
		/// The system does not move the file until the operating system is restarted. 
		/// The system moves the file immediately after AUTOCHK is executed, but before 
		/// creating any paging files. Consequently, this parameter enables the function 
		/// to delete paging files from previous startups. 
		/// </summary>
		DelayUntilReboot    = 0x00000004,
		/// <summary>
		/// The function does not return until the file has actually been moved on the disk. 
		/// </summary>
		WriteThrough        = 0x00000008,
		/// <summary>
		/// Reserved for future use.
		/// </summary>
		CreateHardLink      = 0x00000010,
		/// <summary>
		/// The function fails if the source file is a link source, but the file cannot be tracked after the move. This situation can occur if the destination is a volume formatted with the FAT file system.
		/// </summary>
		FailIfNotTrackable	= 0x00000020,
	}
	#endregion
}
