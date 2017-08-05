using System;
using System.Collections.Generic;

namespace RssBandit.Common
{
    /// <summary>
    /// Helper class that treats strings as canonicalized URIs then compares them
    /// </summary>
    internal class UriComparer: IComparer<string>{

        public int Compare(string x, string y){
            if( x == null || y == null) return -1; 
            
            Uri a, b; 
            Uri.TryCreate(x,UriKind.Absolute, out a);
            Uri.TryCreate(y, UriKind.Absolute, out b);
        	return a != null && b != null
        	       	? a.CanonicalizedUri().CompareTo(b.CanonicalizedUri())
        	       	: x.CompareTo(y);
        }
    }

    internal class UriEqualityComparer : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            if (x == null || y == null) return false;

            Uri a, b;
            Uri.TryCreate(x, UriKind.Absolute, out a);
            Uri.TryCreate(y, UriKind.Absolute, out b);
            return a != null && b != null
                       ? a.CanonicalizedUri().Equals(b.CanonicalizedUri())
                       : x.Equals(y);
        }

        public int GetHashCode(string obj)
        {
            if (obj == null) return 0;

            Uri.TryCreate(obj, UriKind.Absolute, out Uri a);

            return a?.GetHashCode() ?? 0;
        }
    }

    /// <summary>
    /// Helper class used to add extension method to the System.Uri class. 
    /// </summary>
    public static class UriHelper
    {

        /// <summary>
        /// Helper class that treats strings as canonicalized URIs then compares them
        /// </summary>
        public static IComparer<string> Comparer = new UriComparer();
        public static IEqualityComparer<string> EqualityComparer = new UriEqualityComparer();

        /// <summary>
        /// Returns a the URI canonicalized in the following way. (1) if the file is a UNC or file URI then it only returns the local part. 
        /// (2) for Web URIs it removes trailing slashes
        /// </summary>
        /// <param name="uri">The URI to canonicalize</param>
        /// <returns>The canonicalized URI as a string</returns>
        public static string CanonicalizedUri(this Uri uri)
        {
            return CanonicalizedUri(uri, false);
        }

        /// <summary>
        /// Returns a the URI canonicalized in the following way. (1) if the file is a UNC or file URI then it only returns the local part. 
        /// (2) for Web URIs it removes trailing slashes and preceding "www." 
        /// </summary>
        /// <param name="uri">The URI to canonicalize</param>
        /// <param name="replaceWWW">Indicates whether preceding 'www' should be removed or not</param>
        /// <returns>The canonicalized URI as a string</returns>        
        public static string CanonicalizedUri(Uri uri, bool replaceWWW)
        {

            if (uri.IsFile || uri.IsUnc)
                return uri.LocalPath;

            UriBuilder builder = new UriBuilder(uri);
            if (replaceWWW)
            {
                builder.Host = (builder.Host.StartsWith("www.", StringComparison.OrdinalIgnoreCase) ? builder.Host.Substring(4) : builder.Host);
            }
            builder.Path = (builder.Path.EndsWith("/", StringComparison.Ordinal) && String.IsNullOrEmpty(builder.Query) ? builder.Path.Substring(0, builder.Path.Length - 1) : builder.Path);

            string strUri = builder.ToString();

            if (builder.Scheme == "http" && builder.Port == 80)
                strUri = strUri.Replace(":" + builder.Port + "/", "/");

            if (builder.Scheme == "https" && builder.Port == 443)
                strUri = strUri.Replace(":" + builder.Port + "/", "/");

            return strUri;
        }
    }
}
