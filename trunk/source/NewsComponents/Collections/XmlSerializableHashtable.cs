#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion


// ===============================================================================
// Microsoft Configuration Management Application Block for .NET
// http://msdn.microsoft.com/library/en-us/dnbda/html/cmab.asp
//
// XmlSerializableHashtable.cs
//
// A helper class used to serialize a Hashtable instance on Xml.
//
// For more information see the Configuration Management Application Block Implementation Overview. 
// ==============================================================================

using System;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;

namespace NewsComponents.Collections
{
	/// <summary>
	/// This class is used to provide serialization support for a hashtable
	/// Thanks to Christoph, christophdotnet@austin.rr.com, for the xml-serializable hashtable implementation 
	/// </summary>
	[XmlInclude( typeof(string) )] 
	[XmlInclude( typeof(bool) )]
	[XmlInclude( typeof(short) )]
	[XmlInclude( typeof(int) )]
	[XmlInclude( typeof(long) )]
	[XmlInclude( typeof(float) )]
	[XmlInclude( typeof(double) )]
	[XmlInclude( typeof(DateTime) )]
	[XmlInclude( typeof(char) )]
	[XmlInclude( typeof(decimal) )]
	[XmlInclude( typeof(UInt16) )]
	[XmlInclude( typeof(UInt32) )]
	[XmlInclude( typeof(UInt64) )]
	[XmlInclude( typeof(Int64) )]
	public class XmlSerializableHashtable
	{

		#region Nested Class--Item

		/// <summary>
		/// Represents an entry for the hashtable
		/// </summary>
		public class Entry
		{
			private object _entryKey;
			private object _entryValue;

			/// <summary>
			/// Default constructor, needed by serialization support
			/// </summary>
			public Entry(){}

			/// <summary>
			/// Construct the Entity specifying the key and the entry
			/// </summary>
			/// <param name="entryKey"></param>
			/// <param name="entryValue"></param>
			public Entry( object entryKey , object entryValue )
			{
				_entryKey = entryKey;
				_entryValue = entryValue;
			}

			/// <summary>
			/// Return the key
			/// </summary>
			[XmlElement("key")]
            public object EntryKey
			{
				get{ return _entryKey; }
				set{ _entryKey = value; }
			}

			/// <summary>
			/// Return the entry value
			/// </summary>
			[XmlElement("value")]
            public object EntryValue
			{
				get{ return _entryValue; }
				set{ _entryValue = value; }
			}
		}

		#endregion

		#region Declarations
			
		private Hashtable _ht;

		#endregion

		#region Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		public XmlSerializableHashtable()
		{
			_ht = new Hashtable( 10 );
		}

		
		/// <summary>
		/// Creates a serializable hashtable using a hashtable
		/// </summary>
		/// <param name="ht"></param>
		public XmlSerializableHashtable( Hashtable ht )
		{
			_ht = ht;
		}

		
		#endregion

		#region Public Methods & Properties

		/// <summary>
		/// Returns the contained hashtable
		/// </summary>
		[XmlIgnore]
		public Hashtable InnerHashtable
		{
			get{ return _ht; }
			set{ _ht = value; }
		}

		/// <summary>
		/// Used to serilalize the contents of the hashtable
		/// </summary>
		[XmlElement("entries")]
		public Entry[] Entries
		{
			get
			{
				Entry[] entryArray = new Entry[ _ht.Count ];
				int i = 0;

				foreach( DictionaryEntry de in _ht )
				{
					entryArray[i] = new Entry( de.Key, de.Value );	
					i = i + 1;
				}

				return entryArray;
			}

			set
			{
				lock( _ht.SyncRoot )
				{
					_ht.Clear();
					foreach( Entry item in value )
					{
						_ht.Add	( 
								GetValueFromXml(item.EntryKey),
								GetValueFromXml(item.EntryValue)
								);
					}
				}
			}
		}
		
		private static object GetValueFromXml( object value )
		{
			object[] valueList = (object[])value;
			if( ( valueList.Length == 1 ) && (valueList[0] is XmlCharacterData) )
			{
				return ( (XmlCharacterData)valueList[0]).Value;
			}
			if( ( valueList.Length == 2 ) && ( valueList[1] is XmlCharacterData ) )
			{
				return ( (XmlCharacterData)valueList[1]).Value;
			}
			return "";
		}
		#endregion
	}
}
