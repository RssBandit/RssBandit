#region CVS Version Header
/*
 * $Id: DownloadFilesCollection.cs,v 1.4 2007/06/14 18:58:43 carnage4life Exp $
 * Last modified by $Author: carnage4life $
 * Last modified at $Date: 2007/06/14 18:58:43 $
 * $Revision: 1.4 $
 */
#endregion

using System;
using System.Collections;
using System.IO;
using NewsComponents.Utils;

namespace NewsComponents.Net
{

	/// <summary>
	/// Defines a collection of files.
	/// </summary>
	[Serializable]
	public class DownloadFilesCollection : CollectionBase {

		#region Private members

		#endregion

		#region Constructors

		/// <summary>
		/// Creates a DownloadFilesCollection using the list of FileManifest objects.
		/// </summary>
		/// <param name="downloadFiles">The list of files.</param>
		public DownloadFilesCollection( DownloadFile[] downloadFiles ) {
			if ( downloadFiles != null && downloadFiles.Length > 0 ) {
				for(int i=0; i<downloadFiles.Length; i++) {
					List.Add(downloadFiles[i]);
				}
			}
		}
		/// <summary>
		/// Default constructor
		/// </summary>
		public DownloadFilesCollection() {}
		#endregion

		#region Public properties

		/// <summary>
		/// Indexer that returns a file for a given index.
		/// </summary>
		/// <param name="index">The index of the file to locate.</param>
		public DownloadFile this[ int index ] {
			get {
				return (DownloadFile)List[ index ];
			}
			set {
				List[ index ] = value;
			}
		}

		#endregion

		#region Public methods

		/// <summary>
		/// Allows determining if the file is already contained in the collection.
		/// </summary>
		/// <param name="value">The FileManiest instance.</param>
		/// <returns>A boolean value indicating whether file is in the collection.</returns>
		public bool Contains( DownloadFile value) {
			return List.Contains( value );
		}

		/// <summary>
		/// Adds a file to the file collection.
		/// </summary>
		/// <param name="value">The FileManiest instance.</param>
		public void Add( DownloadFile value ) {
			List.Add( value );
		}

		/// <summary>
		/// Removes the file from the file collection.
		/// </summary>
		/// <param name="value">The FileManiest instance.</param>
		public void Remove( DownloadFile value ) {
			List.Remove( value );
		}

		/// <summary>
		/// Insert the file at a specific index in the collection.
		/// </summary>
		/// <param name="index">The index to insert the file.</param>
		/// <param name="value">The FileManiest instance.</param>
		public void Insert( int index, DownloadFile value ) {
			List.Insert( index, value );
		}

		#endregion
	}	

	/// <summary>
	/// Defines the information of a file to download.
	/// </summary>
	public class DownloadFile {
		#region Private members

		/// <summary>
		/// The source location for the file.
		/// </summary>
		private string sourceLocation;

		/// <summary>
		/// Indicates what MimeType was suggested for the content
		/// of the file.
		/// </summary>
		private MimeType suggestedMimeType;

		/// <summary>
		/// The expected file size
		/// </summary>
		private long expectedSize;

		/// <summary>
		/// The file name used for local storage
		/// </summary>
		private string localName;

		#endregion

		#region Constructor
	
		/// <summary>
		/// Creates a new FileManifest with the deserialized manifest file information.
		/// </summary>
		/// <param name="source">The File source location.</param>
		/// <param name="suggestedType">MimeType</param>
		/// <param name="expectedSize">long</param>
		public DownloadFile(Enclosure enclosure ) {
			
			this.sourceLocation = enclosure.Url;
			this.suggestedMimeType = new MimeType(enclosure.MimeType); 				
			this.expectedSize = enclosure.Length;
			
			this.GuessLocalFileName(); 									
		}

		#endregion

		#region Public properties

		/// <summary>
		/// The source location of the file.
		/// </summary>
		public string Source {
			get {
				return sourceLocation;
			}
		} 

		/// <summary>
		/// Suggested MimeType.
		/// </summary>
		public MimeType SuggestedType {
			get {
				return suggestedMimeType;
			}	
			set{
				this.suggestedMimeType = value; 
				this.GuessLocalFileName();
			}
		}


		/// <summary>
		/// The file size
		/// </summary>
		public long FileSize {
			get {
				return expectedSize;
			}
			set{
				this.expectedSize = value;
			}
		}
		/// <summary>
		/// The local name of the file.
		/// </summary>
		public string LocalName {
			get {
				return localName;
			}
			set {
				localName = value;
			}
		} 

		#endregion
	
		#region Static Methods 

		/// <summary>
		/// Guesses the name of the local file.
		/// </summary>		
		private void GuessLocalFileName() {						

			try{		
				int index = this.sourceLocation.LastIndexOf("/"); 	 

				if((index != -1) && (index + 1 < this.sourceLocation.Length)){
					this.localName = this.sourceLocation.Substring(index + 1); 
				}else{ 
					this.localName = this.sourceLocation;
				}				

				if(this.localName.IndexOf(".")== -1){
					this.localName = this.localName + "." + this.suggestedMimeType.GetFileExtension(); 
				}
				
			}catch(Exception){
			
				if(this.localName == null){
					this.localName = Guid.NewGuid().ToString() + "." + this.suggestedMimeType.GetFileExtension(); 
				}
			}
				
		}
		#endregion
	}

}
