#region CVS Version Header
/*
 * $Id: FileHelper.cs,v 1.3 2005/02/17 09:12:37 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/02/17 09:12:37 $
 * $Revision: 1.3 $
 */
#endregion

using System;
using System.Collections;
using System.IO; 
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml; 
using System.Diagnostics;

namespace NewsComponents.Utils
{
	/// <summary>
	/// Helper class.
	/// </summary>
	public sealed class FileHelper {
	 
		private static int msecsBetweenRetries = 100;
		private static int bufferSize = 1024 * 20;	// 20K read/write buffer

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
			
			if (StringHelper.EmptyOrNull(fileName))
				throw new ArgumentNullException("fileName");
			if (stream == null)
				throw new ArgumentNullException("stream");

			bool saveSuccess = false;

			try {
				if (stream.CanSeek)	// reset stream pointer
					stream.Seek(0, SeekOrigin.Begin);

				string dataPath = fileName + ".new";
				Stream fsStream = OpenForWrite(dataPath);
				if (fsStream == null) {
					// to get the exception:
					fsStream = new FileStream(dataPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize);
				}
				try {
					int size = 2048;
					byte[] writeData = new byte[size];
					while (true) {
						size = stream.Read(writeData, 0, size);
						if (size > 0) {
							fsStream.Write(writeData, 0, size);
						} 
						else {
							break;
						}
					}
					fsStream.Flush();
					saveSuccess = true;
				}
				finally {
					fsStream.Close();
				}
				// only after successful save of the stream, copy/rename/move to real location
				if (saveSuccess) {
					Delete(fileName + ".bak");
					if (File.Exists(fileName)) {	// if it is the very initial write, there is nothing to rename to .bak
						File.Move(fileName, fileName + ".bak");
					}
					File.Move(fileName + ".new", fileName);
				}
			}
			catch (Exception ex){
				// write out message to the attached trace listeners
				saveSuccess = false;
				Trace.WriteLine("WriteStreamWithBackup('"+fileName+"') caused exception: "+ex.Message);
			}

			return saveSuccess;
		}

		/// <summary>
		/// Writes a stream to the fielsystem with the full qualified fileName in a save manner: 
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
			
			if (StringHelper.EmptyOrNull(fileName))
				throw new ArgumentNullException("fileName");
			if (stream == null)
				throw new ArgumentNullException("stream");

			bool saveSuccess = false;

			try {
				if (stream.CanSeek)	// reset stream pointer
					stream.Seek(0, SeekOrigin.Begin);

				string dataPath = fileName + ".new";
				Stream fsStream = OpenForWrite(dataPath);
				if (fsStream == null) {
					// to get the exception:
					fsStream = new FileStream(dataPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize);
				}
				try {
					int size = 2048;
					byte[] writeData = new byte[size];
					while (true) {
						size = stream.Read(writeData, 0, size);
						if (size > 0) {
							fsStream.Write(writeData, 0, size);
						} 
						else {
							break;
						}
					}
					fsStream.Flush();
					saveSuccess = true;
				}
				finally {
					fsStream.Close();
				}
				// only after successful save of the stream, copy/rename/move to real location
				if (saveSuccess) {
					
					if (File.Exists(fileName)) {	// if it is the very initial write, there is nothing to delete
						Delete(fileName);
					}
					File.Move(fileName + ".new", fileName);
				}
			}
			catch (Exception ex){
				// write out message to the attached trace listeners
				saveSuccess = false;
				Trace.WriteLine("WriteStreamWithRename('"+fileName+"') caused exception: "+ex.Message);
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

		private FileHelper() {}

	}

}
