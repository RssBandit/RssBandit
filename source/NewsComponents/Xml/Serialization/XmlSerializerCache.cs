#undef USE_PERF_COUNTER

#region Using directives

using System;
using System.Collections;
using System.Xml.Serialization;
#if USE_PERF_COUNTER
using System.Management.Instrumentation;
#endif
#endregion

//
// Thanks to Daniel Cazzulino, kzu.net@gmail.com
// 
namespace NewsComponents.Xml.Serialization {
	
	/// <summary>
	/// Delegete type for events to raise from the
	/// serializer cache.
	/// </summary>
	public delegate void SerializerCacheDelegate(Type type
	, XmlAttributeOverrides overrides
	, Type[] types
	, XmlRootAttribute root
	, String defaultNamespace);

	/// <summary>
	/// The XmlSerializerCache allows to work around the 
	/// assembly leak problem in the XmlSerializer 
	/// ( LINK )
	/// The cache will inspect if it contains any previously cached 
	/// instances that are compatible with the parameters passed to the
	/// various overloads to the GetSerializer method before constructing 
	/// a new XmlSerializer instance.
	/// </summary>
	/// <remarks>
	/// In contrast to the XmlSerializer, the XmlSerializerCache requires
	/// a permission set that allows reflecting over private members.
	/// </remarks>
	public class XmlSerializerCache : IDisposable {

		/// <summary>
		/// The NewSerializer event fires when the XmlSerializerCache
		/// receives a request for an XmlSerializer instance
		/// that is not in the cache and it needs to create a
		/// new instance.
		/// </summary>
		public event SerializerCacheDelegate NewSerializer;

		/// <summary>
		/// The CacheHit even fires when the XmlSerializerCache
		/// receives a request for a previously cached instance
		/// of an XmlSerializer
		/// </summary>
		public event SerializerCacheDelegate CacheHit;

		/// <summary>
		/// The Dictionary to store cached serializer instances.
		/// </summary>
		private System.Collections.Generic.Dictionary<string, System.Xml.Serialization.XmlSerializer> Serializers;
		//private Hashtable Serializers;

		/// <summary>
		/// An object to synchonize access to the Dictionary instance.
		/// </summary>
		private object SyncRoot;

#if USE_PERF_COUNTER
		private PerfCounterManager stats;
#endif
		/// <summary>
		/// Default constructor to initialize
		/// an instance of the cache.
		/// </summary>
		public XmlSerializerCache() {
			SyncRoot = new object();
#if USE_PERF_COUNTER
			stats = new PerfCounterManager();
#endif
			//Serializers = new Dictionary<string, System.Xml.Serialization.XmlSerializer>();
			Serializers = new System.Collections.Generic.Dictionary<string,XmlSerializer>();
		}

		/// <summary>
		/// Get an XmlSerializer instance for the
		/// specified parameters. The method will check if
		/// any any previously cached instances are compatible
		/// with the parameters before constructing a new  
		/// XmlSerializer instance.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public System.Xml.Serialization.XmlSerializer GetSerializer(Type type
			) {
			return GetSerializer(type, null, new Type[0], null, null);
		}

		/// <summary>
		/// Get an XmlSerializer instance for the
		/// specified parameters. The method will check if
		/// any any previously cached instances are compatible
		/// with the parameters before constructing a new  
		/// XmlSerializer instance.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="defaultNamespace"></param>
		/// <returns></returns>
		public System.Xml.Serialization.XmlSerializer GetSerializer(Type type
			, String defaultNamespace) {
			return GetSerializer(type, null, new Type[0], null, defaultNamespace);
		}

		/// <summary>
		/// Get an XmlSerializer instance for the
		/// specified parameters. The method will check if
		/// any any previously cached instances are compatible
		/// with the parameters before constructing a new  
		/// XmlSerializer instance.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="root"></param>
		/// <returns></returns>
		public System.Xml.Serialization.XmlSerializer GetSerializer(Type type
			, XmlRootAttribute root) {
			return GetSerializer(type, null, new Type[0], root, null);
		}

		/// <summary>
		/// Get an XmlSerializer instance for the
		/// specified parameters. The method will check if
		/// any any previously cached instances are compatible
		/// with the parameters before constructing a new  
		/// XmlSerializer instance.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="overrides"></param>
		/// <returns></returns>
		public System.Xml.Serialization.XmlSerializer GetSerializer(Type type
			, XmlAttributeOverrides overrides) {
			return GetSerializer(type
				, overrides
				, new Type[0]
				, null
				, null);
		}

		/// <summary>
		/// Get an XmlSerializer instance for the
		/// specified parameters. The method will check if
		/// any any previously cached instances are compatible
		/// with the parameters before constructing a new  
		/// XmlSerializer instance.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="types"></param>
		/// <returns></returns>
		public System.Xml.Serialization.XmlSerializer GetSerializer(Type type
			, Type[] types) {
			return GetSerializer(type
				, null
				, types
				, null
				, null);
		}

		/// <summary>
		/// Get an XmlSerializer instance for the
		/// specified parameters. The method will check if
		/// any any previously cached instances are compatible
		/// with the parameters before constructing a new  
		/// XmlSerializer instance.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="overrides"></param>
		/// <param name="types"></param>
		/// <param name="root"></param>
		/// <param name="defaultNamespace"></param>
		/// <returns></returns>
		public System.Xml.Serialization.XmlSerializer GetSerializer(Type type
			, XmlAttributeOverrides overrides
			, Type[] types
			, XmlRootAttribute root
			, String defaultNamespace) {
			string key = CacheKeyFactory.MakeKey(type
				, overrides
				, types
				, root
				, defaultNamespace);

			System.Xml.Serialization.XmlSerializer serializer = null;
            if (false == Serializers.ContainsKey(key))
            {
				lock (SyncRoot) {
                    if (false == Serializers.ContainsKey(key))
                    {
#if USE_PERF_COUNTER
						stats.IncrementInstanceCount();
#endif
                        serializer = new System.Xml.Serialization.XmlSerializer(type
                            , overrides
                            , types
                            , root
                            , defaultNamespace);
                        Serializers.Add(key, serializer);

                        if (null != NewSerializer)
                        {
                            NewSerializer(type
                                , overrides
                                , types
                                , root
                                , defaultNamespace);
                        }
                    }else{
                        serializer = Serializers[key] as XmlSerializer;
                    }
				} // lock
			} // if( null == Serializers[key] )
			else {
				serializer = Serializers[key] as XmlSerializer;
#if USE_PERF_COUNTER
				stats.IncrementHitCount();
#endif
				// Tell the listeners that we already 
				// had a serializer that matched the attributes
				if (null != CacheHit) {
					CacheHit(type
						, overrides
						, types
						, root
						, defaultNamespace);
				}
			}

			System.Diagnostics.Debug.Assert(null != serializer);
			return serializer;
		}

		/// <summary>
		/// Finalizer
		/// </summary>
		~XmlSerializerCache() {
			Dispose(false);
		}

		private void Dispose(bool isDisposing) {
			if (true == isDisposing) {
#if USE_PERF_COUNTER
				stats.Dispose();
#endif
			}
		}

		#region IDisposable Members

		/// <summary>
		/// Implementation of IDisposable.Dispose. Call to 
		/// clean up resources held by the XmlSerializerCache.
		/// </summary>
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion
	}
}
