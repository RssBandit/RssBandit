using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.IO;
using System.Windows;

namespace RssBandit.WinGui.MarkupExtensions
{
    public class ResourceFileProvider : ResourceManager, ILocalizedResourceProvider
    {
        /// <summary>
        /// Caches the current resource set
        /// </summary>
        private ResourceSet _resourceSet;

        /// <summary>
        /// Gets the localized value for the specified key
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public object GetValue(string key)
        {
            try
            {
                if (_resourceSet != null)
                {
                    return _resourceSet.GetObject(key);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Loads the resources.
        /// </summary>
        private void LoadResources()
        {
            ReleaseAllResources();
            _resourceSet = GetResourceSet(CultureInfo.CurrentUICulture, true, true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceFileProvider"/> class.
        /// </summary>
        /// <param name="baseName">Name of the base.</param>
        /// <param name="assembly">The assembly.</param>
        public ResourceFileProvider(string baseName, Assembly assembly)
            : base( baseName, assembly )
        {
            LoadResources();
            LocalizationManager.CultureChanged += (sender, e) => { LoadResources(); };
        }

       
    }
}
