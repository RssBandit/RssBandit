using System.IO;
using System.Threading.Tasks;

namespace System.Net
{
    /// <summary>Extension methods for working with WebRequest asynchronously.</summary>
    public static class WebRequestExtensions
    {
        /// <summary>Creates a Task that represents an asynchronous request to GetResponse.</summary>
        /// <param name="webRequest">The WebRequest.</param>
        /// <returns>A Task containing the retrieved WebResponse.</returns>
        public static Task<WebResponse> GetResponseAsync(this WebRequest webRequest)
        {
            if (webRequest == null) throw new ArgumentNullException("webRequest");
            return Task<WebResponse>.Factory.FromAsync(
                webRequest.BeginGetResponse, webRequest.EndGetResponse, webRequest /* object state for debugging */);
        }      
    
    }
}
