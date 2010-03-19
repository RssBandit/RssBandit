// © 2009 Rick Strahl. All rights reserved. 
// See http://wpflocalization.codeplex.com for related whitepaper and updates
// See http://wpfclientguidance.codeplex.com for other WPF resources
//
// Based on Christian Moser's implementation:
// http://www.wpftutorial.net/LocalizeMarkupExtension.html

using System;
using System.Windows.Markup;
using System.Windows;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Resources;
using System.Reflection;
using System.Diagnostics;
using System.Linq;

namespace RssBandit.WinGui
{

    /// <summary>
    /// Based on Christian Moser's implementation:
    /// http://www.wpftutorial.net/LocalizeMarkupExtension.html
    /// 
    /// ResExtension markup extension returns localized values from a Resources Manager     
    /// 
    /// To use:
    ///   * in App.xaml.cs set the resource assembly and default resource manager
    ///     ResExtension.InitializeResExtension(Assembly.GetExecutingAssembly(),WpfClickOnce.Properties.Resources.ResourceManager);
    ///   * Make sure you register the namespace of this extension in the document header
    ///     xmlns:local="http://www.west-wind.com/WpfControls"
    ///     or
    ///     xmlns:local="clr-namespace:Westwind.Wpf.Controls,assembly=WpfControls"    
    ///   * Embed the extension as follows:
    ///     Content="{local:Res Id=HelloWorld,Default=Hello World}"   // gets from Resources
    ///     Content="{local:Res Id=HelloWorld,Default=Hello World,ResourceSet=WpfLocalizationResx.ResxResource,Format=You said: \{0\}"    
    ///     in addition you can pass Format (string.Format() string) and Convert (WPF Type Converter) as parameters
    ///   * Use Default values to ensure the designer displays correct content
    ///   * If you want live Resource values (rather than default values) copy this MarkupExtension
    ///     into your WPF project. It'll automatically pick up the default Resx resourcemanager and
    ///     startup assembly.
    ///     
    /// This extension requires that you have a Properties.Resources object defined
    /// in order to find the default resource set, and assembly.
    /// 
    /// NOTE:
    /// THIS MARKUP EXTENSION MUST BE PLACED IN THE SAME PROJECT AS THE RESOURCES YOU ARE TRYING
    /// IF YOU WANT TO SEE LIVE RESOURCE BINDINGS IN THE DESIGNER RATHER THAN DEFAULT VALUES.
    /// IF YOU LEAVE IT IN AN EXTERNAL ASSEMBLY DEFAULT VALUES WILL BE USED AND IF THEY ARE 
    /// MISSING BLANKS WILL BE RENDERED IN THE DESIGNER.
    /// 
    /// I've been unable to find a solution how to get access to the default assembly via code
    /// as the designer doesn't load App.Xaml nor provides a normal execution context. It also
    /// doesn't provide Parent properties on controls. If you know of a way to get a reference
    /// to the startup assembly or even the parent page from a control instance please contact me.    
    /// </summary>
    [MarkupExtensionReturnType(typeof(object)), Localizability(LocalizationCategory.NeverLocalize)]
    public class ResExtension : MarkupExtension
    {

        static ResExtension()
        {
            // *** YOU CAN OVERRIDE THE RESOURCE MANAGER USED HERE EXPLICITLY
            // *** THIS HAS TO HAPPEN IN HERE SO THE DESIGNER CAN SEE THE RESOURCE MANAGER
            // *** Any external assignment will work at runtime but fail the designer
            //ResExtension.InitializeResExtension(Assembly.GetExecutingAssembly(),
            //                                    WpfClickOnce.Properties.Resources.ResourceManager);                                                
        }

        /// <summary>
        /// Contains static settings for the ResourceAssembly and default Resource
        /// Manager. Looks in app.config for AppSettings DefaultResourceAssembly
        /// 
        /// </summary>
        //public static LocalizationSettings Settings = LocalizationSettings.Current;

        /// <summary>
        /// Caches the depending target object
        /// </summary>
        DependencyObject _targetObject;

        /// <summary>
        /// Caches the depending target property
        /// </summary>
        DependencyProperty _targetProperty;

        /// <summary>
        /// Caches the resolved default type converter
        /// </summary>
        TypeConverter _typeConverter;

        /// <summary>
        /// internally cached type that we are bound to when 
        /// binding using Static
        /// </summary>
        PropertyInfo _propertyInfo;

        /// <summary>
        /// Cache the ServiceProvider
        /// </summary>
        private IServiceProvider _serviceProvider;

        public ResExtension()
        {
            this.Default = string.Empty;
            if (LocalizationSettings.Current.CheckForCultureChange)
                LocalizationSettings.Current.CultureChanged += UpdateTarget;
        }
        ~ResExtension()
        {
            LocalizationSettings.Current.CultureChanged -= UpdateTarget;
        }

        /// <summary>
        /// Updates the target explicitly so we can update when culture changed.
        /// </summary>
        void UpdateTarget()
        {
            if (_targetObject != null && _targetProperty != null)
            {
                _targetObject.SetValue(_targetProperty, this.ProvideValueInternal());
            }
        }


        /// <summary>
        /// Allow calling the extension with a default parameter (id).
        /// Content="{local:Res HelloWorld}"
        /// </summary>
        /// <param name="param">The key that specifies a localization </param>
        public ResExtension(string id)
            : this()
        {
            Id = id as string;
        }


        /// <summary>
        /// The ResourceId to retrieve
        /// </summary>
        /// <value>The key.</value>
        [ConstructorArgument("Id")]
        public string Id { get; set; }


        /// <summary>
        /// A static value expression like props:Resources.HelloWorld in the 
        /// same format as used in the x:Static extension. This uses the
        /// strongly typed resources but avoids any of the lookups to find
        /// the resource manager.
        /// 
        /// When using this syntax to point at strongly typed resources
        /// you don't need to specify a ResourceSet or Assembly because it
        /// these are explicitly tied to the strongly typed resource.
        /// </summary>
        [ConstructorArgument("Static")]
        public string Static { get; set; }


        /// <summary>
        /// Optional Resource Set Name. If not provided it's
        /// assumed you want to access the global Resources
        /// resource set in Properties.Resources
        /// </summary>
        [ConstructorArgument("ResourceSet")]
        public string ResourceSet { get; set; }

        /// <summary>
        /// A default value that is returned when the the resource
        /// cannot be resolved. Also returned in the designer if
        /// there are no resources available.
        /// </summary>
        /// <value>The default value.</value>
        [ConstructorArgument("Default")]
        public object Default { get; set; }

        /// <summary>
        /// A string.Format format string that is applied.
        /// Note when provided the result value is always 
        /// converted into a string.
        /// </summary>
        /// <value>The format.</value>
        [ConstructorArgument("Format")]
        public string Format { get; set; }

        /// <summary>
        /// Allows to specify a custom Value Converter using standard WPF syntax.
        /// </summary>
        /// <value>The converter.</value>
        [ConstructorArgument("Converter")]
        public IValueConverter Converter { get; set; }


        /// <summary>
        /// Optional assembly name - use this if you are using
        /// this component in more than one assembly to uniquely
        /// identify the resource assembly
        /// </summary>
        [ConstructorArgument("Assembly")]
        public string Assembly { get; set; }

        /// <summary>
        /// The core method to provide a localized value.
        /// 
        /// Note the value can also be a non-string value to set non-string properties
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // Resolve the depending object and property
            if (_targetObject == null)
            {
                var targetService = (IProvideValueTarget)
                     serviceProvider.GetService(typeof(IProvideValueTarget));
                _targetObject = targetService.TargetObject as DependencyObject;
                _targetProperty = targetService.TargetProperty as DependencyProperty;
                _typeConverter = TypeDescriptor.GetConverter(_targetProperty.PropertyType);

                
            }
            _serviceProvider = serviceProvider;

            return ProvideValueInternal();
        }

     

        /// <summary>
        /// Internal value retrieval
        /// </summary>
        /// <returns></returns>
        private object ProvideValueInternal()
        {
            object localized = null;

            if (Static == null)
            {
                // Get a cached resource manager for this resource set
                ResourceManager resMan = LocalizationSettings.GetResourceManager(this.ResourceSet,this.Assembly);

                // Get the localized value 
                if (resMan != null)
                {
                    try
                    {
                        localized = resMan.GetObject(this.Id);
                    }
                    catch { /* eat exception here so we can still display default value */ }
                }
            }
            else
            {
                try
                {
                    // Parse Static=properties:Resources.HelloWorld like static resource
                    int index = this.Static.IndexOf('.');
                    if (index == -1)
                        throw new ArgumentException("Invalid Static Binding Syntax: " 
                                                    + this.Static);

                    if (this._propertyInfo == null)
                    {
                        // resolve properties:Resources
                        string typeName = this.Static.Substring(0, index);
                        IXamlTypeResolver service = _serviceProvider.GetService(typeof(IXamlTypeResolver))
                                                    as IXamlTypeResolver;
                        Type memberType = service.Resolve(typeName);

                        string propName = this.Static.Substring(index + 1);
                        this._propertyInfo = memberType.GetProperty(propName,
                                            BindingFlags.Public | BindingFlags.Static |
                                            BindingFlags.FlattenHierarchy);                                            
                    }                    
                    localized = _propertyInfo.GetValue(null, null);
                }                 
                catch
                {                    

                    // in Blend this will always fail and fall through to use the Default
                    // values

                    // Since this is a static property lets throw here 
                    //This way you'll see it - in the designer as an error.
                    //throw new ArgumentException(Resources.InvalidStaticBindingSyntax + ": " + 
                    //                            this.Static + "\r\n" + ex.Message);                 
                }
            }
            
            // If the value is null, use the Default value if available
            if (localized == null && this.Default != null)
                localized = this.Default;

            // fail type conversions silently and write to trace output
            try
            {
                // Convert if a type converter is availalbe
                if (localized != null &&
                    this.Converter == null &&
                    _typeConverter != null &&
                    _typeConverter.CanConvertFrom(localized.GetType()))
                    localized = _typeConverter.ConvertFrom(localized);

                // Apply a type converter if one was specified
                if (Converter != null)
                    localized = this.Converter.Convert(localized, _targetProperty.PropertyType,
                                                       null, CultureInfo.CurrentCulture);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("ResExtension Type conversion failed. Id: {0}, Error: {1}",
                                              Id, ex.Message));
                //Trace.WriteLine(string.Format("ResExtension Type conversion failed. Id: {0}, 
                // Error: {1}", Id, ex.Message));

                localized = null;
            }

            // If no fallback value is available, return the key
            if (localized == null)
            {
                if (_targetProperty != null &&
                    _targetProperty.PropertyType == typeof(string))
                    // Return the key surrounded by question marks for string type properties
                    localized = string.Concat("?", Id, "?");
                else
                    // Return the UnsetValue for all other types of dependency properties
                    return DependencyProperty.UnsetValue;
            }

            // Format if a format string was provided
            if (this.Format != null)
                localized = string.Format(CultureInfo.CurrentUICulture,
                                          this.Format, localized);
            
            return localized;
        }



        /// <summary>
        /// This method tries to find the assembly of resources and the strongly 
        /// typed Resource class in a project so the designer works properly.
        /// 
        /// It's recommended your application ALWAYS explicitly initializes the
        /// DefaultResourceAssembly and DefaultResourceManager in LocalizationSettings.
        /// 
        /// When running in the designer this code tries to find 
        /// </summary>
        /// <returns></returns>
        internal bool FindDefaultResourceAssembly()
        {
            Assembly asm = null;

            if (this.Assembly != null)
            {
                asm = LocalizationSettings.AssemblyList[this.Assembly] as Assembly;
                if (asm == null)
                    asm = LocalizationSettings.AddAssembly(this.Assembly);
                return true;
            }


            try
            {
                if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                {
                    // in designer find the Windows.Application instance
                    // NOTE: will not work in control projects but better than nothing
                    asm = (
                           from a in AppDomain.CurrentDomain.GetAssemblies()
                           where a.EntryPoint != null &&
                                 a.GetTypes().Any(t => t.IsSubclassOf(typeof(Application)))
                           select a
                          ).FirstOrDefault();
                }
                else
                {
                    // Do Private reflection on FrameworkElement
                    FrameworkElement element = _targetObject as FrameworkElement;
                    if (element != null)
                    {
                        object root = element.GetType().GetProperty("InheritanceParent", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(element, null);
                        asm = root.GetType().Assembly;
                    }                    
                }
            }
            catch
            { /* eat any exceptions here; */ }

            // Assume the Document we're called is in the same assembly as resources            
            if (asm == null)
                asm = System.Reflection.Assembly.GetExecutingAssembly();

            // Search for Properties.Resources in the Exported Types (has to be public!)
            Type ResType = asm.GetExportedTypes().Where(type => type.FullName.Contains(".Properties.Resources")).FirstOrDefault();
            if (ResType == null)
                return false;

            ResourceManager resMan = ResType.GetProperty("ResourceManager").GetValue(ResType, null) as ResourceManager;

            LocalizationSettings.Current.DefaultResourceAssembly = asm;
            LocalizationSettings.Current.DefaultResourceManager = resMan;

            return true;
        }

    }
}
