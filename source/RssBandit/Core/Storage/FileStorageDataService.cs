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
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using NewsComponents;

using NewsComponents.Utils;
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

		public override string[] GetUserDataFileNames()
		{
			return new string[] { ColumnLayoutDefsFileName };
		}

		public override DataEntityName SetContentForDataFile(string dataFileName, Stream content)
		{
			string fileName = Path.GetFileName(ColumnLayoutDefsFileName);
			if (String.Equals(dataFileName, fileName, StringComparison.OrdinalIgnoreCase))
			{
				using (Stream s = FileHelper.OpenForWrite(ColumnLayoutDefsFileName))
				{
					byte[] buf = new byte[4096];
					int offset = 0, bytesRead;
					while (0 <= (bytesRead = content.Read(buf, offset, 4096)))
					{
						s.Write(buf, 0, bytesRead);
						offset += bytesRead;
					}
				}

				return DataEntityName.ColumnLayoutDefinitions;
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

		
	}
	
	/// <summary>
	/// Column Layout Definitions serializable root class
	/// </summary>
	[XmlType(Namespace = NamespaceCore.Feeds_vCurrent)]
	[XmlRoot("column-layouts", Namespace = NamespaceCore.Feeds_vCurrent, IsNullable = false)]
	public class SerializableColumnLayoutDefinitions
	{
		/// <summary/>
		[XmlElement("column-layout", Type = typeof(listviewLayout), IsNullable = false)]
		public List<listviewLayout> ColumnLayouts = new List<listviewLayout>();
	}

	/// <remarks/>
	[XmlType(Namespace = NamespaceCore.Feeds_vCurrent)]
	public class listviewLayout
	{
		public listviewLayout()
		{
		}

		public listviewLayout(string id, IFeedColumnLayout layout)
		{
			ID = id;
			FeedColumnLayout = (FeedColumnLayout)layout;
		}

		/// <remarks/>
		[XmlAttribute]
		public string ID;

		/// <remarks/>
		[XmlAnyAttribute]
		public XmlAttribute[] AnyAttr;

		/// <remarks/>
		[XmlElement] //?
		public FeedColumnLayout FeedColumnLayout;
	}
}
