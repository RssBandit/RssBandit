#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;

namespace NewsComponents
{
	/// <summary>
	/// SimpleHyperLink is a container for an Url and an assotiated Title
	/// </summary>
	public struct TitledLink : IEquatable<TitledLink>
	{
		public static TitledLink Empty = new TitledLink();
		
		#region ivars

		private readonly string _url;
		private readonly string _title;

		#endregion

		#region ctor's

		/// <summary>
		/// Initializes a new instance of the <see cref="TitledLink"/> class.
		/// </summary>
		/// <param name="url">The navigate URL.</param>
		public TitledLink(string url) :
			this(url, null) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="TitledLink"/> class.
		/// </summary>
		/// <param name="url">The navigate URL.</param>
		/// <param name="title">The text.</param>
		public TitledLink(string url, string title)
		{
			_url = (url != null ? url.ToLowerInvariant() : null);
			_title = title;
		}

		
		#endregion

		#region public properties

		/// <summary>
		/// Gets or sets the link title.
		/// </summary>
		/// <value>The title text.</value>
		public string Title
		{
			get { return _title; }
		}

		/// <summary>
		/// Gets or sets the link URL.
		/// </summary>
		/// <value>The link URL.</value>
		public string Url
		{
			get { return _url; }
		}

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/> by
		/// returning the NavigateUrl;
		/// </returns>
		public override string ToString()
		{
			return String.Format("{0}: {1}",String.IsNullOrEmpty(Title) ? "<none>" : _title, _url);
		}

		#endregion

		#region overrides / IEquatable<SimpleHyperLink>

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// We only consider the Url to compare, the title is irrelevant here!
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
		/// </returns>
		public bool Equals(TitledLink other)
		{
			if (!String.Equals(_url, other._url, StringComparison.Ordinal))
				return false;
			return true;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is TitledLink && Equals((TitledLink) obj);
		}

		public override int GetHashCode()
		{
			int result = 7;
			result = 29*result + (_url != null ? _url.GetHashCode() : 0);
			result = 29*result + (_title != null ? _title.GetHashCode() : 0);
			return result;
		}


		public static bool operator ==(TitledLink left, TitledLink right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(TitledLink left, TitledLink right)
		{
			return !left.Equals(right);
		}

		#endregion
	}
}
