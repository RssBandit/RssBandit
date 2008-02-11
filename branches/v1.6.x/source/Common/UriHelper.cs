
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RssBandit.Common
{
    /// <summary>
    /// Helper class that treats strings as canonicalized URIs then compares them
    /// </summary>
    internal class UriComparer: IComparer<string>{

        public int Compare(string x, string y){
            if( x == null || y == null) return -1; 
            
            Uri a = null, b = null; 
            Uri.TryCreate(x,UriKind.Absolute, out a);
            Uri.TryCreate(y, UriKind.Absolute, out b); 
            if( a != null && b != null)
                return a.CanonicalizedUri().CompareTo(b.CanonicalizedUri());
            else 
                return x.CompareTo(y); 
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
                builder.Host = (builder.Host.ToLower().StartsWith("www.") ? builder.Host.Substring(4) : builder.Host);
            }
            builder.Path = (builder.Path.EndsWith("/") ? builder.Path.Substring(0, builder.Path.Length - 1) : builder.Path);

            string strUri = builder.ToString();

            if (builder.Scheme == "http" && builder.Port == 80)
                strUri = strUri.Replace(":" + builder.Port + "/", "/");

            if (builder.Scheme == "https" && builder.Port == 443)
                strUri = strUri.ToString().Replace(":" + builder.Port + "/", "/");

            return strUri;
        }
    }
}


/* HACK: We add ExtensionAttribute class here so we don't need project to target .NET 3.5 */ 
namespace System.Runtime.CompilerServices

   {
         [AttributeUsage(AttributeTargets.Method)]
         public sealed class ExtensionAttribute : Attribute
         {
             public ExtensionAttribute() { }
         }
     }