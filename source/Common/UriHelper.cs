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

    /// <summary>
    /// Helper class used to add extension method to the System.Uri class. 
    /// </summary>
    public static class UriHelper
    {

        /// <summary>
        /// Helper class that treats strings as canonicalized URIs then compares them
        /// </summary>
        public static IComparer<string> Comparer = new UriComparer();

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
        public static string CanonicalizedUri(this Uri uri, bool replaceWWW)
        {

            if (uri.IsFile || uri.IsUnc)
                return uri.LocalPath;

            UriBuilder builder = new UriBuilder(uri);
            if (replaceWWW)
            {
                builder.Host = (builder.Host.StartsWith("www.", StringComparison.OrdinalIgnoreCase) ? builder.Host.Substring(4) : builder.Host);
            }
			builder.Path = (builder.Path.EndsWith("/", StringComparison.Ordinal) ? builder.Path.Substring(0, builder.Path.Length - 1) : builder.Path);

            string strUri = builder.ToString();

            if (builder.Scheme == "http" && builder.Port == 80)
                strUri = strUri.Replace(":" + builder.Port + "/", "/");

            if (builder.Scheme == "https" && builder.Port == 443)
                strUri = strUri.Replace(":" + builder.Port + "/", "/");

            return strUri;
        }
    }
}
