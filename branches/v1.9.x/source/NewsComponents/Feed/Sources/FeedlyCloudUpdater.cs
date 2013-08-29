#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

#define SANDBOX

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace NewsComponents.Feed.Sources
{
	#region FeedlyCloudOperation enum

	/// <summary>
	/// This is an enum that describes the set of operations that can be placed in the 
	/// queue of network operations to perform on Feedly Cloud. 
	/// </summary>
	public enum FeedlyCloudOperation : byte
	{
		/// <summary>
		/// TODO: review!!!
		/// </summary>
		AddFeed = 51, // == queue priority!
		/// <summary>
		/// 
		/// </summary>
		AddLabel = 41,
		/// <summary>
		/// 
		/// </summary>
		DeleteFeed = 50,
		/// <summary>
		/// 
		/// </summary>
		DeleteLabel = 40,
		/// <summary>
		/// 
		/// </summary>
		MarkAllItemsRead = 61,
		/// <summary>
		/// 
		/// </summary>
		MarkSingleItemRead = 60,
		/// <summary>
		/// 
		/// </summary>
		MarkSingleItemTagged = 59,
		/// <summary>
		/// 
		/// </summary>
		MoveFeed = 45,
		/// <summary>
		/// 
		/// </summary>
		RenameFeed = 21,
		/// <summary>
		/// 
		/// </summary>
		RenameLabel = 20,
	}

	#endregion

	#region PendingFeedlyCloudOperation class

	/// <summary>
	/// This is a class that is used to represent a pending operation on the index in 
	/// that is currently in the pending operation queue. 
	/// </summary>
	public class PendingFeedlyCloudOperation : IEquatable<PendingFeedlyCloudOperation>
	{
		/// <summary>
		/// Google Reader Operation action
		/// </summary>
		public FeedlyCloudOperation Action;
		/// <summary>
		/// Google user name
		/// </summary>
		public string GoogleUserName;
		/// <summary>
		/// 
		/// </summary>
		public object[] Parameters;

		/// <summary>
		/// No default constructor
		/// </summary>
		private PendingFeedlyCloudOperation()
		{
			;
		}

		/// <summary>
		/// Constructor 
		/// </summary>
		/// <param name="action">The operation to perform on the index</param>
		/// <param name="parameters">The parameters to the operation</param>
		/// <param name="googleUserID">The Google User ID of the account under which this operation will be performed.</param>
		public PendingFeedlyCloudOperation(FeedlyCloudOperation action, object[] parameters, string googleUserID)
		{
			Action = action;
			Parameters = parameters;
			GoogleUserName = googleUserID;
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="p1">The p1.</param>
		/// <param name="p2">The p2.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator !=(PendingFeedlyCloudOperation p1, PendingFeedlyCloudOperation p2)
		{
			return !Equals(p1, p2);
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="p1">The p1.</param>
		/// <param name="p2">The p2.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator ==(PendingFeedlyCloudOperation p1, PendingFeedlyCloudOperation p2)
		{
			return Equals(p1, p2);
		}

		/// <summary>
		/// Compares for equality of the instance and the specified pending google reader operation.
		/// </summary>
		/// <param name="pendingFeedlyCloudOperation">The pending google reader operation.</param>
		/// <returns></returns>
		public bool Equals(PendingFeedlyCloudOperation pendingFeedlyCloudOperation)
		{
			if (pendingFeedlyCloudOperation == null)
				return false;

			if (pendingFeedlyCloudOperation.Action != Action)
				return false;
			if (!Equals(GoogleUserName, pendingFeedlyCloudOperation.GoogleUserName))
				return false;
			if (pendingFeedlyCloudOperation.Parameters.Length != Parameters.Length) return false;

			for (int i = 0; i < pendingFeedlyCloudOperation.Parameters.Length; i++)
			{
				if (!pendingFeedlyCloudOperation.Parameters[i].Equals(Parameters[i]))
					return false;
			}

			return true;
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
			return Equals(obj as PendingFeedlyCloudOperation);
		}

		/// <summary>
		/// Serves as a hash function for a particular type.
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object"/>.
		/// </returns>
		public override int GetHashCode()
		{
			int result = Action.GetHashCode();
			result = 29 * result + (GoogleUserName != null ? GoogleUserName.GetHashCode() : 0);
			result = 29 * result + (Parameters != null ? Parameters.GetHashCode() : 0);
			return result;
		}
	}

	#endregion

	/// <summary>
    /// Class which updates Feedly Reader in the background. 
    /// </summary>
    internal class FeedlyCloudUpdater
	{
		/// <summary>
		/// Name of the file where pending network operations are saved on shut down. 
		/// </summary>
		private readonly string pendingFeedlyCloudOperationsFile = "pending-feedlycloud-operations.xml";

		/// <summary>
		/// Queue of pending network operations to perform against Feedly Cloud
		/// </summary>
		private List<PendingFeedlyCloudOperation> pendingFeedlyCloudOperations =
			new List<PendingFeedlyCloudOperation>();

		private readonly Dictionary<string, FeedlyCloudFeedSource> FeedSources =
			new Dictionary<string, FeedlyCloudFeedSource>();

		#region ctor's 

        /// <summary>
        /// Instance of this class must always be created with a path to where to save and load state. 
        /// </summary>
        private FeedlyCloudUpdater()
        {
            ;
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
		public FeedlyCloudUpdater(string applicationDataPath)
        {
            pendingFeedlyCloudOperationsFile = Path.Combine(applicationDataPath, pendingFeedlyCloudOperationsFile);
            LoadPendingOperations();
        }

        #endregion

		#region public methods

		/// <summary>
		/// Adds the feed source to the list of GoogleReaderFeedSources being modified by this class 
		/// </summary>
		/// <param name="source"></param>
		public void RegisterFeedSource(FeedlyCloudFeedSource source)
		{
			FeedSources.Add(source.GoogleUserName, source);
		}

		/// <summary>
		/// Removes the feed source from the list of GoogleReaderFeedSources being modified by this class. 
		/// </summary>
		/// <param name="source"></param>
		public void UnregisterFeedSource(FeedlyCloudFeedSource source)
		{
			FeedSources.Remove(source.GoogleUserName);
		}

		#endregion

		#region private methods

		/// <summary>
		/// Loads pending operations from disk
		/// </summary>
		private void LoadPendingOperations()
		{
			if (File.Exists(pendingFeedlyCloudOperationsFile))
			{
				XmlSerializer serializer =
					XmlHelper.SerializerCache.GetSerializer(typeof(List<PendingFeedlyCloudOperation>));
				pendingFeedlyCloudOperations =
					serializer.Deserialize(XmlReader.Create(pendingFeedlyCloudOperationsFile)) as
					List<PendingFeedlyCloudOperation>;
			}
		}

		#endregion
	}
}
