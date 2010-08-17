#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System.Globalization;
using System.Windows;
using System.Windows.Markup;

namespace RssBandit.WinGui.Utility
{

    public class WpfUtils
    {

        /// <summary>
        /// Applies the culture to WPF.
        /// </summary>
        /// <param name="cultureInfo">The culture info.</param>
        /// <remarks>Ensure the current culture passed into bindings is the OS culture.
        /// By default, WPF uses en-US as the culture, regardless of the system settings.</remarks>
        public static void ApplyCulture(CultureInfo cultureInfo)
        {
            if (cultureInfo != null)
            {
                FrameworkElement.LanguageProperty.OverrideMetadata(
                    typeof(FrameworkElement),
                    new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(cultureInfo.IetfLanguageTag)));
            }
        }

        /// <summary>
        /// Retrieves the root element from a particular element by walking
        /// the tree of parents up backwards.
        /// 
        /// Note: fails on some elements that don't have parent elements like
        /// context menus.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        /// <remarks>
        /// © 2009 Rick Strahl. All rights reserved. 
        /// See http://wpflocalization.codeplex.com for related whitepaper and updates
        /// See http://wpfclientguidance.codeplex.com for other WPF resources
        /// </remarks>
        public static DependencyObject GetRootVisual(DependencyObject element)
        {
            if (element == null)
                return null;

            if (element is FrameworkElement)
            {
                FrameworkElement el = element as FrameworkElement;
                while (el.Parent != null)
                {
                    el = el.Parent as FrameworkElement;
                }
                return el;
            }
            else if (element is FrameworkContentElement)
            {
                FrameworkContentElement fce = element as FrameworkContentElement;
                while (fce.Parent != null)
                {
                    DependencyObject dnew = fce.Parent;
                    if (dnew is FrameworkElement)
                        return GetRootVisual(dnew);
                }
                return fce;
            }
            return null;
        }
    }
}
