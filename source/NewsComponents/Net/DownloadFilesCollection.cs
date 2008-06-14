#region CVS Version Header

/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */

#endregion

using System;
using NewsComponents.Core;
using NewsComponents.Utils;

namespace NewsComponents.Net
{
    /// <summary>
    /// Defines the information of a file to download.
    /// </summary>
    public class DownloadFile : BindableObject
    {
        #region Private members

        /// <summary>
        /// The source location for the file.
        /// </summary>
        private readonly string sourceLocation;

        /// <summary>
        /// Indicates what MimeType was suggested for the content
        /// of the file.
        /// </summary>
        private MimeType suggestedMimeType;

        /// <summary>
        /// The file name used for local storage
        /// </summary>
        private string localName;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new FileManifest with the deserialized manifest file information.
        /// </summary>
        /// <param name="enclosure"></param>
        public DownloadFile(IEnclosure enclosure)
        {
            sourceLocation = enclosure.Url;
            suggestedMimeType = new MimeType(enclosure.MimeType);
            FileSize = enclosure.Length;

            GuessLocalFileName();
        }

        #endregion

        #region Public properties

        /// <summary>
        /// The source location of the file.
        /// </summary>
        public string Source
        {
            get { return sourceLocation; }
        }

        /// <summary>
        /// Suggested MimeType.
        /// </summary>
        public MimeType SuggestedType
        {
            get { return suggestedMimeType; }
            set
            {
                suggestedMimeType = value;
                GuessLocalFileName();

                RaisePropertyChanged("SuggestedType");
                
            }
        }


        private long _fileSize;

        /// <summary>
        /// The file size
        /// </summary>
        public long FileSize
        {
            get { return _fileSize; }
            set
            {
                _fileSize = value;
                RaisePropertyChanged("FileSize");
            }
        }

        /// <summary>
        /// The local name of the file.
        /// </summary>
        public string LocalName
        {
            get { return localName; }
            set
            {
                localName = value;
                RaisePropertyChanged("LocalName");
            }
        }

        #endregion

        #region Static Methods 

        /// <summary>
        /// Guesses the name of the local file.
        /// </summary>		
        private void GuessLocalFileName()
        {
            try
            {
                int index = sourceLocation.LastIndexOf("/");

                if ((index != -1) && (index + 1 < sourceLocation.Length))
                {
                    LocalName = sourceLocation.Substring(index + 1);
                }
                else
                {
                    LocalName = sourceLocation;
                }

                if (LocalName.IndexOf(".") == -1)
                {
                    LocalName = LocalName + "." + suggestedMimeType.GetFileExtension();
                }
            }
            catch (Exception)
            {
                if (LocalName == null)
                {
                    LocalName = Guid.NewGuid() + "." + suggestedMimeType.GetFileExtension();
                }
            }
        }

        #endregion
    }
}