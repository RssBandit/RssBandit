// © 2009 Rick Strahl. All rights reserved. 
// See http://wpflocalization.codeplex.com for related whitepaper and updates
// See http://wpfclientguidance.codeplex.com for other WPF resources

using System;
using System.Windows;
using System.Reflection;
using System.Diagnostics;
using System.Resources;
using System.Collections;
using System.Globalization;

namespace RssBandit.WinGui.MarkupExtensions
{

    /// <summary>
    /// Provides a few attached properties for use in localization
    /// </summary>
    public class TranslationExtension : DependencyObject
    {

        public static readonly DependencyProperty TranslateProperty =
            DependencyProperty.RegisterAttached("Translate",
                                                typeof(bool),
                                                typeof(FrameworkElement),
                                                new FrameworkPropertyMetadata(false,
                                                         FrameworkPropertyMetadataOptions.AffectsRender,
                                                         new PropertyChangedCallback(OnTranslateChanged))
                                                );

        public static void SetTranslate(UIElement element, bool value)
        {
            element.SetValue(TranslateProperty, value);
        }
        public static bool GetTranslate(UIElement element)
        {
            return (bool)element.GetValue(TranslateProperty);
        }
        private static void OnTranslateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            if ((bool)e.NewValue == true)
                TranslateKeys(d as FrameworkElement);
        }


        public static readonly DependencyProperty TranslateResourceSetProperty =
                                    DependencyProperty.RegisterAttached("TranslateResourceSet",
                                                            typeof(string),
                                                            typeof(TranslationExtension),
                                                            new FrameworkPropertyMetadata("",
                                                                FrameworkPropertyMetadataOptions.AffectsRender));
        public static void SetTranslateResourceSet(UIElement element, string value)
        {
            element.SetValue(TranslateResourceSetProperty, value);
        }
        public static string GetTranslateResourceSet(UIElement element)
        {
            return element.GetValue(TranslateResourceSetProperty) as string;
        }

        public static readonly DependencyProperty TranslateResourceAssemblyProperty =
                            DependencyProperty.RegisterAttached("TranslateResourceAssembly",
                                                    typeof(string),
                                                    typeof(TranslationExtension),
                                                    new FrameworkPropertyMetadata("",
                                                        FrameworkPropertyMetadataOptions.AffectsRender)
                                                      );


        public static void SetTranslateResourceAssembly(UIElement element, string value)
        {
            element.SetValue(TranslateResourceAssemblyProperty, value);
        }
        public static string GetTranslateResourceAssembly(UIElement element)
        {
            return element.GetValue(TranslateResourceAssemblyProperty) as string;
        }

        static void TranslateKeys(UIElement element)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(element))
            {
                return;
            }

            FrameworkElement root = WpfUtils.GetRootVisual(element) as FrameworkElement;
            if (root == null)
                return;  // must be framework element to find root

            // Retrieve the resource set and assembly from the top level element
            string resourceset = root.GetValue(TranslateResourceSetProperty) as string;
            string resourceAssembly = root.GetValue(TranslateResourceAssemblyProperty) as string;

            ResourceManager manager;
            if (resourceAssembly == null)
                manager = LocalizationSettings.GetResourceManager(resourceset, root.GetType().Assembly);
            else
                manager = LocalizationSettings.GetResourceManager(resourceset, resourceAssembly);

            // find neutral culture so we can iterate over all keys


            ResourceSet set = manager.GetResourceSet(CultureInfo.InvariantCulture, true, true);
            IDictionaryEnumerator enumerator = set.GetEnumerator();

            while (enumerator.MoveNext())
            {
                string key = enumerator.Key as string;
                if (key.StartsWith(element.Uid + "."))
                {
                    string property = key.Split('.')[1] as string;
                    object value = manager.GetObject(key); // enumerator.Value;

                    // Bind the value AFTER control has initialized or else the
                    // default will override what we bind here
                    root.Initialized += delegate
                    {
                        try
                        {
                            PropertyInfo prop = element.GetType().GetProperty(property, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase);
                            prop.SetValue(element, value, null);
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(string.Format("TranslateExtension Resource Failure: {0}  - {1}", key, ex.Message));
                        }
                    };
                }
            }
        }

    }
}
