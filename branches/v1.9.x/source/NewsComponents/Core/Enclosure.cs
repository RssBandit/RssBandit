using System;
using NewsComponents.Core;

namespace NewsComponents
{
    /// <summary>
    /// Represents an RSS enclosure
    /// </summary>
    public class Enclosure : BindableObject, IEnclosure
    {
        private readonly string mimeType;
        private readonly long length;
        private readonly string url;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mimeType">The MIME type of the enclosure</param>
        /// <param name="length">The length of the enclosure in bytes</param>
        /// <param name="url">The URL of the enclosure</param>
        /// <param name="description">The description.</param>
        public Enclosure(string mimeType, long length, string url, string description)
        {
            this.length = length;
            this.mimeType = mimeType;
            this.url = url;
            Description = description;
            Duration = TimeSpan.MinValue;
        }

        /// <summary>
        /// The MIME type of the enclosure
        /// </summary>
        public string MimeType
        {
            get { return mimeType; }
        }

        /// <summary>
        /// The length of the enclosure in bytes
        /// </summary>
        public long Length
        {
            get { return length; }
        }

        /// <summary>
        /// The MIME type of the enclosure
        /// </summary>
        public string Url
        {
            get { return url; }
        }

        private string _description;

        /// <summary>
        /// The description associated with the item obtained via itunes:subtitle or media:title
        /// </summary>
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                RaisePropertyChanged("Description");
            }
        }

        private bool _downloaded;

        /// <summary>
        /// Indicates whether this enclosure has already been downloaded or not.
        /// </summary>
        public bool Downloaded
        {
            get { return _downloaded; }
            set
            {
                _downloaded = value;
                RaisePropertyChanged("Downloaded");
            }
        }

        private TimeSpan _duration;

        /// <summary>
        /// Gets the playing time of the enclosure. 
        /// </summary>
        public TimeSpan Duration
        {
            get { return _duration; }
            set
            {
                _duration = value;
                RaisePropertyChanged("Duration");
            }
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        public bool Equals(IEnclosure other)
        {
            return Equals(other as Enclosure);
        }

        /// <summary>
        /// Equalses the specified enclosure.
        /// </summary>
        /// <param name="enclosure">The enclosure.</param>
        /// <returns></returns>
        public bool Equals(Enclosure enclosure)
        {
            if (enclosure == null) return false;
            return Equals(url, enclosure.url);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">The <paramref name="obj"/> parameter is null.</exception>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj as Enclosure);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return url != null ? url.GetHashCode() : 0;
        }
    }
}