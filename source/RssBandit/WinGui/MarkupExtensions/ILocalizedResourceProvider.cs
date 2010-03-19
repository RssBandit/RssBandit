using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace RssBandit.WinGui.MarkupExtensions
{
    public interface ILocalizedResourceProvider
    {
        /// <summary>
        /// Gets the localized value for the specified key
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        object GetValue(string key);

    }
}
