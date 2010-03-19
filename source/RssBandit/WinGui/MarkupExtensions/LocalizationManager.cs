using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading;

namespace RssBandit.WinGui.MarkupExtensions
{
    public class LocalizationManager
    {
        /// <summary>
        /// Occurs when the culture changed.
        /// </summary>
        public static event EventHandler CultureChanged;
        
        /// <summary>
        /// List of registered localization resource providers
        /// </summary>
        public static ILocalizedResourceProvider LocalizationProvider { get; set; }

        /// <summary>
        /// Gets the supported cultures.
        /// </summary>
        /// <value>The supported cultures.</value>
        public static IList<CultureInfo> SupportedCultures { get; private set; }

        /// <summary>
        /// Gets and sets the currently selected culture
        /// </summary>
        public static CultureInfo CurrentCulture
        {
            get { return CultureInfo.CurrentUICulture; }
            set
            {
                Thread.CurrentThread.CurrentUICulture = value;

                if (CultureChanged != null)
                {
                    CultureChanged(null, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets a localized value for the specified resource key
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static object GetValue(string key)
        {
            if (LocalizationProvider != null)
            {
                return LocalizationProvider.GetValue(key);
            }
            return null;
        }


        /// <summary>
        /// Initializes the <see cref="LocalizationManager"/> class.
        /// </summary>
        static LocalizationManager()
        {
            SupportedCultures = new List<CultureInfo>();
        }

    }
}
