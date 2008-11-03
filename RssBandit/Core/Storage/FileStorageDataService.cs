#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using NewsComponents;

using NewsComponents.Utils;
using RssBandit.Core.Storage.Serialization;
using RssBandit.WinGui;

namespace RssBandit.Core.Storage
{
	internal class FileStorageDataService: DataServiceBase
	{
		#region Overrides of DataServiceBase

		//public override void Initialize(string initData)
		//{
		//    base.Initialize(initData);
		//    //TODO: own inits
		//}

		public override FeedColumnLayoutCollection LoadColumnLayouts()
		{
			string fileName = ColumnLayoutDefsFileName;
			if (File.Exists(fileName))
			{
				XmlSerializer serializer = XmlHelper.SerializerCache.GetSerializer(typeof(SerializableColumnLayoutDefinitions));
				using (Stream s = FileHelper.OpenForRead(fileName))
				{
					SerializableColumnLayoutDefinitions root = (SerializableColumnLayoutDefinitions)serializer.Deserialize(s);
					FeedColumnLayoutCollection coll = new FeedColumnLayoutCollection(root.ColumnLayouts.Count);
					foreach (var layout in root.ColumnLayouts)
					{
						coll.Add(layout.ID, layout.FeedColumnLayout);
					}
					return coll;
				}
			}
			return new FeedColumnLayoutCollection();
		}

		public override void SaveColumnLayouts(FeedColumnLayoutCollection layouts)
		{
			string fileName = ColumnLayoutDefsFileName;
			if (layouts == null || layouts.Count == 0)
			{
				if (File.Exists(fileName))
					FileHelper.Delete(fileName);
				return;
			}

			XmlSerializer serializer = XmlHelper.SerializerCache.GetSerializer(typeof(SerializableColumnLayoutDefinitions));
			using (Stream stream = FileHelper.OpenForWrite(fileName))
			{
				SerializableColumnLayoutDefinitions root = new SerializableColumnLayoutDefinitions();
				root.ColumnLayouts = new List<listviewLayout>(layouts.Count);
				/* sometimes we get nulls in the arraylist, remove them */
				List<string> toRemove = new List<string>();
				foreach (string k in layouts.Keys)
				{
					var s = layouts[k];
					if (s == null)
						toRemove.Add(k);
					else
						root.ColumnLayouts.Add(new listviewLayout(k, s));
				}
				
				while (toRemove.Count > 0)
				{
					layouts.Remove(toRemove[0]);
					toRemove.RemoveAt(0);
				}
				
				serializer.Serialize(stream, root);
			}
		}

		public override IdentitiesDictionary LoadIdentities()
		{
			string fileName = UserIdentitiesFileName;
			if (File.Exists(fileName))
			{
				XmlSerializer serializer = XmlHelper.SerializerCache.GetSerializer(typeof(SerializableIdentities));
				using (Stream s = FileHelper.OpenForRead(fileName))
				{
					SerializableIdentities root = (SerializableIdentities)serializer.Deserialize(s);
					IdentitiesDictionary coll = new IdentitiesDictionary(root.identities.Count);
					foreach (var identity in root.identities)
					{
						coll.Add(identity.Name, identity);
					}
					return coll;
				}
			}
			return new IdentitiesDictionary();
		}

		public override void SaveIdentities(IdentitiesDictionary identities)
		{
			string fileName = UserIdentitiesFileName;
			if (identities == null || identities.Count == 0)
			{
				if (File.Exists(fileName))
					FileHelper.Delete(fileName);
				return;
			}

			XmlSerializer serializer = XmlHelper.SerializerCache.GetSerializer(typeof(SerializableIdentities));
			using (Stream stream = FileHelper.OpenForWrite(fileName))
			{
				SerializableIdentities root = new SerializableIdentities();
				root.identities = new List<UserIdentity>(identities.Values);
				serializer.Serialize(stream, root);
			}
		}

		public override string[] GetUserDataFileNames()
		{
			return new string[] { ColumnLayoutDefsFileName, UserIdentitiesFileName };
		}

		public override DataEntityName SetContentForDataFile(string dataFileName, Stream content)
		{
			string fileName = Path.GetFileName(ColumnLayoutDefsFileName);
			if (String.Equals(dataFileName, fileName, StringComparison.OrdinalIgnoreCase))
			{
				FileHelper.WriteStreamWithBackup(ColumnLayoutDefsFileName, content);
				return DataEntityName.ColumnLayoutDefinitions;
			}
			
			fileName = Path.GetFileName(UserIdentitiesFileName);
			if (String.Equals(dataFileName, fileName, StringComparison.OrdinalIgnoreCase))
			{
				FileHelper.WriteStreamWithBackup(UserIdentitiesFileName, content);
				return DataEntityName.UserIdentities;
			}

			return DataEntityName.None;
		}

		#endregion

		private string ColumnLayoutDefsFileName
		{
			get
			{
				return Path.Combine(initializationData,
					"column-layout-definitions.xml");
			}
		}

		private string UserIdentitiesFileName
		{
			get
			{
				return Path.Combine(initializationData,
					"user-identities.xml");
			}
		}
		
	}
	
	
}
