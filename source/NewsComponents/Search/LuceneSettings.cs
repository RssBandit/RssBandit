#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.IO;
using Lucene.Net.Store;
using RssBandit.AppServices.Configuration;
using Directory=Lucene.Net.Store.Directory;
using Logger = RssBandit.Common.Logging;

namespace NewsComponents.Search
{
	/// <summary>
	/// LuceneSettings implements a persistent storage of
	/// settings to keep longer than the current session/application
	/// lifetime.
	/// </summary>
	internal class LuceneSettings
	{
		private const string IndexFolderName = "index";

		private IPersistedSettings settings;
		private SearchIndexBehavior behavior;

		private string indexPath;

		/// <summary>
		/// Initializes a new instance of the <see cref="LuceneSettings"/> class.
		/// </summary>
		/// <param name="configuration">The configuration.</param>
		public LuceneSettings(INewsComponentsConfiguration configuration) {
			settings = configuration.PersistedSettings;
			behavior = configuration.SearchIndexBehavior;
			indexPath = BuildAndCreateIndexDirectoryPath(configuration);
		}

		/// <summary>
		/// Gets a value indicating whether this search behavior is a file based search.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this a is file based search config; otherwise, <c>false</c>.
		/// </value>
		public bool IsFileBasedSearch {
			get {
				return (this.behavior != SearchIndexBehavior.NoIndexing) && (
				       	this.behavior == SearchIndexBehavior.AppDataDirectoryBased ||
				        this.behavior == SearchIndexBehavior.LocalAppDataDirectoryBased ||
				        this.behavior == SearchIndexBehavior.TempDirectoryBased);
			}
		}
		
		/// <summary>
		/// Gets a value indicating whether this search behavior is a RAM based search.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is RAM based search; otherwise, <c>false</c>.
		/// </value>
		public bool IsRAMBasedSearch {
			get {
#if DEBUG
				return (this.behavior != SearchIndexBehavior.NoIndexing) && 
				       (this.behavior == SearchIndexBehavior.RAMDirectoryBased);
#else
				return false;
#endif
			}
		}
		
		/// <summary>
		/// Gets the index path.
		/// </summary>
		/// <value>The index path.</value>
		internal string IndexPath
		{
			get { return this.indexPath; }
		}

        /// <summary>
        /// Gets the index directory (returns a new object instance).
        /// </summary>
        /// <value>The index directory.</value>
		internal Directory GetIndexDirectory()
		{
			return this.GetIndexDirectory(false);
		}

        /// <summary>
        /// Gets the index directory (returns a new object instance).
        /// </summary>
        /// <value>The index directory.</value>
		internal Directory GetIndexDirectory(bool create)
        {
	        if (IsRAMBasedSearch)
			{
				return new RAMDirectory();
			}
	        return FSDirectory.GetDirectory(this.indexPath, create);
        }

		/// <summary>
		/// Gets the search index behavior.
		/// </summary>
		/// <value>The search index behavior.</value>
		public SearchIndexBehavior SearchIndexBehavior
		{
			get { return this.behavior; }
		}
		
		/// <summary>
		/// Gets or sets the last index optimization date time (Utc).
		/// </summary>
		/// <value>The last index optimization.</value>
		public DateTime LastIndexOptimization
		{
			get
			{
				return settings.GetProperty(Ps.LuceneLastIndexOptimization, DateTime.MinValue);
			}
			set
			{
				settings.SetProperty(Ps.LuceneLastIndexOptimization, value);
			}
		}

		/// <summary>
		/// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
		/// </returns>
		public override string ToString()
		{
			if (IsRAMBasedSearch)
				return "Index@RAM";
			
			return "Index@"+ this.indexPath;
		}

		private static string BuildAndCreateIndexDirectoryPath(INewsComponentsConfiguration configuration) 
		{
			string path = null;
			if (configuration.SearchIndexBehavior != SearchIndexBehavior.NoIndexing) 
			{
				
				switch (configuration.SearchIndexBehavior) {
					case SearchIndexBehavior.LocalAppDataDirectoryBased:
						{
							path = Path.Combine(configuration.UserLocalApplicationDataPath, 
							                    IndexFolderName);
							break;
						}
					case SearchIndexBehavior.AppDataDirectoryBased:
						{
							path = Path.Combine(configuration.UserApplicationDataPath, 
							                    IndexFolderName);
							break;
						}
					case SearchIndexBehavior.TempDirectoryBased:
						{
							path = Path.Combine(Path.GetTempPath(), 
							                    String.Format("{0}.{1}", configuration.ApplicationID, IndexFolderName));
							break;
						}
				}

				if (path != null) {
					if (!System.IO.Directory.Exists(path))
						System.IO.Directory.CreateDirectory(path);
				}
			}
			return path;
		}

	}
}

#region CVS Version Log
/*
 * $Log: LuceneSettings.cs,v $
 * Revision 1.2  2007/07/21 12:26:21  t_rendelmann
 * added support for "portable Bandit" version
 *
 * Revision 1.1  2006/12/07 13:17:18  t_rendelmann
 * now Lucene.OptimizeIndex() calls are only at startup and triggered by index folder modification datetime
 *
 */
#endregion
