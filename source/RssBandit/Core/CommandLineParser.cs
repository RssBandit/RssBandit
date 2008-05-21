#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Text;
using System.Security.Permissions;
using System.Runtime.Serialization;
using RssBandit.Resources;

namespace RssBandit.Utility {

	/// <summary>
	/// Used to control parsing of command-line arguments.
	/// </summary>
	[Flags]    
	public enum CommandLineArgumentTypes {
		/// <summary>
		/// Indicates that this field is required. An error will be displayed
		/// if it is not present when parsing arguments.
		/// </summary>
		Required    = 0x01,
		
		/// <summary>
		/// Only valid in conjunction with Multiple.
		/// Duplicate values will result in an error.
		/// </summary>
		Unique      = 0x02,

		/// <summary>
		/// Inidicates that the argument may be specified more than once.
		/// Only valid if the argument is a collection
		/// </summary>
		Multiple    = 0x04,

		/// <summary>
		/// Inidicates that if this argument is specified, no other arguments may be specified.
		/// </summary>
		Exclusive    = 0x08,

		/// <summary>
		/// The default type for non-collection arguments.
		/// The argument is not required, but an error will be reported if it is specified more than once.
		/// </summary>
		AtMostOnce  = 0x00,
        
		/// <summary>
		/// The default type for collection arguments.
		/// The argument is permitted to occur multiple times, but duplicate 
		/// values will cause an error to be reported.
		/// </summary>
		MultipleUnique  = Multiple | Unique,
	}
	
	/// <summary>
	/// Commandline parser.
	/// </summary>
	public class CommandLineParser {
    
		#region Public Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="CommandLineParser" /> class
		/// using possible arguments deducted from the specific <see cref="Type" />.
		/// </summary>
		/// <param name="argumentSpecification">The <see cref="Type" /> from which the possible command-line arguments should be retrieved.</param>
		/// <exception cref="ArgumentNullException"><paramref name="argumentSpecification" /> is a null reference.</exception>
		public CommandLineParser(Type argumentSpecification) {
			if (argumentSpecification == null) {
				throw new ArgumentNullException("argumentSpecification");
			}

			_argumentCollection = new CommandLineArgumentCollection();

			foreach (PropertyInfo propertyInfo in argumentSpecification.GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
				if (propertyInfo.CanWrite || typeof(ICollection).IsAssignableFrom(propertyInfo.PropertyType)) {
					CommandLineArgumentAttribute attribute = GetCommandLineAttribute(propertyInfo);
					if (attribute is DefaultCommandLineArgumentAttribute) {
						Debug.Assert(_defaultArgument == null);
						_defaultArgument = new CommandLineArgument(attribute, propertyInfo);
					} else if (attribute != null) {
						_argumentCollection.Add(new CommandLineArgument(attribute, propertyInfo));
					}
				}
			}

			_argumentSpecification = argumentSpecification;
		}
        
        #endregion Public Instance Constructors

    #region Public Instance Properties

		/// <summary>
		/// Gets a logo banner using version and copyright attributes defined on the 
		/// <see cref="Assembly.GetEntryAssembly()" /> or the 
		/// <see cref="Assembly.GetCallingAssembly()" />.
		/// </summary>
		/// <value>A logo banner.</value>
		public virtual string LogoBanner {
			get {
				StringBuilder logoBanner = new StringBuilder();
				Assembly assembly = Assembly.GetEntryAssembly();
				if (assembly == null) {
					assembly = Assembly.GetCallingAssembly();
				}

				// Add description to logo banner

				object[] productAttributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
				if (productAttributes.Length > 0) {
					AssemblyProductAttribute productAttribute = (AssemblyProductAttribute) productAttributes[0];
					if (productAttribute.Product != null && productAttribute.Product.Length != 0) {
						logoBanner.Append(productAttribute.Product);
					}
				} else {
					logoBanner.Append(assembly.GetName().Name);
				}

				// Add version information to logo banner

				object[] informationalVersionAttributes = assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);
				if (informationalVersionAttributes.Length > 0) {
					AssemblyInformationalVersionAttribute versionAttribute = (AssemblyInformationalVersionAttribute) informationalVersionAttributes[0];
					if (versionAttribute.InformationalVersion != null && versionAttribute.InformationalVersion.Length != 0) {
						logoBanner.Append(" version " + versionAttribute.InformationalVersion);
					}
				} else {
					FileVersionInfo info = FileVersionInfo.GetVersionInfo(assembly.Location);
					logoBanner.Append(" version " + info.FileVersion);
				}

				// Add copyright information to logo banner

				object[] copyrightAttributes = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
				if (copyrightAttributes.Length > 0) {
					AssemblyCopyrightAttribute copyrightAttribute = (AssemblyCopyrightAttribute) copyrightAttributes[0];
					if (copyrightAttribute.Copyright != null && copyrightAttribute.Copyright.Length != 0) {
						logoBanner.Append(" " + copyrightAttribute.Copyright);
					}
				}

				logoBanner.Append('\n');

				// Add company information to logo banner

				object[] companyAttributes = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
				if (companyAttributes.Length > 0) {
					AssemblyCompanyAttribute companyAttribute = (AssemblyCompanyAttribute) companyAttributes[0];
					if (companyAttribute.Company != null && companyAttribute.Company.Length != 0) {
						logoBanner.Append(companyAttribute.Company);
						logoBanner.Append('\n');
					}
				}

				return logoBanner.ToString();
			}
		}

		/// <summary>
		/// Gets the usage instructions.
		/// </summary>
		/// <value>The usage instructions.</value>
		public virtual string Usage {
			get {
				StringBuilder helpText = new StringBuilder();
				Assembly assembly = Assembly.GetEntryAssembly();
				if (assembly == null) {
					assembly = Assembly.GetCallingAssembly();
				}

				// Add usage instructions to helptext

				if (helpText.Length > 0) {
					helpText.Append('\n');
				}

				helpText.Append("Usage : " + assembly.GetName().Name + " [options]");

				if (_defaultArgument != null) {
					helpText.Append(" <" + _defaultArgument.LongName + ">");
					if (_defaultArgument.AllowMultiple) {
						helpText.Append(" <" + _defaultArgument.LongName + ">");
						helpText.Append(" ...");
					}
				}

				helpText.Append('\n');

				// Add options to helptext

				helpText.Append("Options : ");
				helpText.Append('\n');
				helpText.Append('\n');

				foreach (CommandLineArgument argument in _argumentCollection) {
					string valType = "";

					if (argument.ValueType == typeof(string)) {
						valType = ":<text>";
					} else if (argument.ValueType == typeof(bool)) {
						valType = "[+|-]";
					} else if (argument.ValueType == typeof(FileInfo)) {
						valType = ":<filename>";
					} else if (argument.ValueType == typeof(int)) {
						valType = ":<number>";
					} else {
						valType = ":" + argument.ValueType.FullName;
					}

					string optionName = argument.LongName;

					if (argument.ShortName != null) {
						if (argument.LongName.StartsWith(argument.ShortName)) {
							optionName = optionName.Insert(argument.ShortName.Length, "[") + "]";
						}
						helpText.AppendFormat(CultureInfo.InvariantCulture, "  -{0,-30}{1}", optionName + valType, argument.Description);

						if (!optionName.StartsWith(argument.ShortName)) {
							helpText.AppendFormat(CultureInfo.InvariantCulture, " (Short format: /{0})", argument.ShortName);
						}
					} else {
						helpText.AppendFormat(CultureInfo.InvariantCulture, "  -{0,-30}{1}", optionName + valType, argument.Description);
					}
					helpText.Append('\n');
				}

				return helpText.ToString();
			}
		}

		/// <summary>
		/// Gets a value indicating whether no arguments were specified on the
		/// command line.
		/// </summary>
		public bool NoArgs {
			get {
				foreach(CommandLineArgument argument in _argumentCollection) {
					if (argument.SeenValue) {
						return true;
					}
				}

				if (_defaultArgument != null) {
					return _defaultArgument.SeenValue;
				}

				return false;
			}
		}

    #endregion Public Instance Properties

    #region Public Instance Methods

		/// <summary>
		/// Parses an argument list.
		/// </summary>
		/// <param name="args">The arguments to parse.</param>
		/// <param name="destination">The destination object on which properties will be set corresponding to the specified arguments.</param>
		/// <exception cref="ArgumentNullException"><paramref name="destination" /> is a null reference.</exception>
		/// <exception cref="ArgumentException">The <see cref="Type" /> of <paramref name="destination" /> does not match the argument specification that was used to initialize the parser.</exception>
		public void Parse(string[] args, object destination) {
			if (destination == null) {
				throw new ArgumentNullException("destination");
			}

			if (!_argumentSpecification.IsAssignableFrom(destination.GetType())) {
				throw new ArgumentException("Type of destination does not match type of argument specification.");
			}

			ParseArgumentList(args);

			// check for missing required arguments
			foreach (CommandLineArgument arg in _argumentCollection) {
				arg.Finish(destination);
			}

			if (_defaultArgument != null) {
				_defaultArgument.Finish(destination);
			}
		}

        #endregion Public Instance Methods

        #region Private Instance Methods

		private void ParseArgumentList(string[] args) {
			if (args != null) {
				foreach (string argument in args) {
					if (argument.Length > 0) {
						switch (argument[0]) {
							case '-':
							case '/':
								int endIndex = argument.IndexOfAny(new char[] {':', '+', '-'}, 1);
								string option = argument.Substring(1, endIndex == -1 ? argument.Length - 1 : endIndex - 1);
								string optionArgument;

								if (option.Length + 1 == argument.Length) {
									optionArgument = null;
								} else if (argument.Length > 1 + option.Length && argument[1 + option.Length] == ':') {
									optionArgument = argument.Substring(option.Length + 2);
								} else {
									optionArgument = argument.Substring(option.Length + 1);
								}
                                
								CommandLineArgument arg = _argumentCollection[option];
								if (arg == null) {
									throw new CommandLineArgumentException(string.Format(CultureInfo.InvariantCulture, "Unknown argument '{0}'", argument));
								} else {
									if (arg.IsExclusive && args.Length > 1) {
										throw new CommandLineArgumentException(string.Format(CultureInfo.InvariantCulture, "Commandline argument '-{0}' cannot be combined with other arguments.", arg.LongName));
									} else {
										arg.SetValue(optionArgument);
									}
								}
								break;
							default:
								if (_defaultArgument != null) {
									_defaultArgument.SetValue(argument);
								} else {
									throw new CommandLineArgumentException(string.Format(CultureInfo.InvariantCulture, "Unknown argument '{0}'", argument));
								}
								break;
						}
					}
				}
			}
		}

        #endregion Private Instance Methods

        #region Private Static Methods

		/// <summary>
		/// Returns the <see cref="CommandLineArgumentAttribute" /> that's applied 
		/// on the specified property.
		/// </summary>
		/// <param name="propertyInfo">The property of which applied <see cref="CommandLineArgumentAttribute" /> should be returned.</param>
		/// <returns>
		/// The <see cref="CommandLineArgumentAttribute" /> that's applied to the 
		/// <paramref name="propertyInfo" />, or a null reference if none was applied.
		/// </returns>
		private static CommandLineArgumentAttribute GetCommandLineAttribute(PropertyInfo propertyInfo) {
			object[] attributes = propertyInfo.GetCustomAttributes(typeof(CommandLineArgumentAttribute), false);
			if (attributes.Length == 1)
				return (CommandLineArgumentAttribute) attributes[0];

			Debug.Assert(attributes.Length == 0);
			return null;
		}

    #endregion Private Static Methods

    #region Private Instance Fields

		private CommandLineArgumentCollection _argumentCollection; 
		private CommandLineArgument _defaultArgument;
		private Type _argumentSpecification;

    #endregion Private Instance Fields
	}

	/// <summary>
	/// Represents a valid command-line argument.
	/// </summary>
	public class CommandLineArgument {
  
		#region Public Instance Constructors

		public CommandLineArgument(CommandLineArgumentAttribute attribute, PropertyInfo propertyInfo) {
			_attribute = attribute;
			_propertyInfo = propertyInfo;
			_seenValue = false;

			_elementType = GetElementType(propertyInfo);
			_argumentType = GetArgumentType(attribute, propertyInfo);
           
			if (IsCollection || IsArray) {
				_collectionValues = new ArrayList();
			}
            
			Debug.Assert(LongName != null && LongName.Length > 0);
			Debug.Assert((!IsCollection && !IsArray) || AllowMultiple, "Collection and array arguments must have allow multiple");
			Debug.Assert(!Unique || (IsCollection || IsArray), "Unique only applicable to collection arguments");
		}
        
    #endregion Public Instance Constructors

    #region Public Instance Properties

		/// <summary>
		/// Gets the underlying <see cref="Type" /> of the argument.
		/// </summary>
		/// <value>The underlying <see cref="Type" /> of the argument.</value>
		/// <remarks>
		/// If the <see cref="Type" /> of the argument is a collection type,
		/// this property will returns the underlying type of that collection.
		/// </remarks>
		public Type ValueType {
			get { return IsCollection || IsArray ? _elementType : Type; }
		}
        
		/// <summary>
		/// Gets the long name of the argument.
		/// </summary>
		/// <value>The long name of the argument.</value>
		public string LongName {
			get { 
				if (_attribute != null && _attribute.Name != null) {
					return _attribute.Name;
				} else {
					return _propertyInfo.Name;
				}
			}
		}

		/// <summary>
		/// Gets the short name of the argument.
		/// </summary>
		/// <value>The short name of the argument.</value>
		public string ShortName {
			get { 
				if (_attribute != null) {
					return _attribute.ShortName;
				} else {
					return null;
				}
			}
		}

		/// <summary>
		/// Gets the description of the argument.
		/// </summary>
		/// <value>The description of the argument.</value>
		public string Description {
			get { 
				if (_attribute != null) {
					return _attribute.Description;
				} else {
					return null;
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether the argument is required.
		/// </summary>
		/// <value>
		/// <c>true</c> if the argument is required; otherwise, <c>false</c>.
		/// </value>
		public bool IsRequired {
			get { return 0 != (_argumentType & CommandLineArgumentTypes.Required); }
		}

		/// <summary>
		/// Gets a value indicating whether a mathing command-line argument 
		/// was already found.
		/// </summary>
		/// <value>
		/// <c>true</c> if a matching command-line argument was already
		/// found; otherwise, <c>false</c>.
		/// </value>
		public bool SeenValue {
			get { return _seenValue; }
		}
        
		/// <summary>
		/// Gets a value indicating whether the argument can be specified multiple
		/// times.
		/// </summary>
		/// <value>
		/// <c>true</c> if the argument may be specified multiple times;
		/// otherwise, <c>false</c>.
		/// </value>
		public bool AllowMultiple {
			get { return (IsCollection || IsArray) && (0 != (_argumentType & CommandLineArgumentTypes.Multiple)); }
		}
        
		/// <summary>
		/// Gets a value indicating whether the argument can only be specified once
		/// with a certain value.
		/// </summary>
		/// <value>
		/// <c>true</c> if the argument should always have a unique value;
		/// otherwise, <c>false</c>.
		/// </value>
		public bool Unique {
			get { return 0 != (_argumentType & CommandLineArgumentTypes.Unique); }
		}

		/// <summary>
		/// Gets the <see cref="Type" /> of the property to which the argument
		/// is applied.
		/// </summary>
		/// <value>
		/// The <see cref="Type" /> of the property to which the argument is
		/// applied.
		/// </value>
		public Type Type {
			get { return _propertyInfo.PropertyType; }
		}
        
		/// <summary>
		/// Gets a value indicating whether the argument is collection-based.
		/// </summary>
		/// <value>
		/// <c>true</c> if the argument is collection-based; otherwise, <c>false</c>.
		/// </value>
		public bool IsCollection {
			get { return IsCollectionType(Type); }
		}

		/// <summary>
		/// Gets a value indicating whether the argument is array-nased.
		/// </summary>
		/// <value>
		/// <c>true</c> if the argument is array-based; otherwise, <c>false</c>.
		/// </value>
		public bool IsArray {
			get { return IsArrayType(Type); }
		}
        
		/// <summary>
		/// Gets a value indicating whether the argument is the default argument.
		/// </summary>
		/// <value>
		/// <c>true</c> if the argument is the default argument; otherwise, <c>false</c>.
		/// </value>
		public bool IsDefault {
			get { return (_attribute != null && _attribute is DefaultCommandLineArgumentAttribute); }
		}

		/// <summary>
		/// Gets a value indicating whether the argument cannot be combined with
		/// other arguments.
		/// </summary>
		/// <value>
		/// <c>true</c> if the argument cannot be combined with other arguments; 
		/// otherwise, <c>false</c>.
		/// </value>
		public bool IsExclusive {
			get { return 0 != (_argumentType & CommandLineArgumentTypes.Exclusive); }
		}

        #endregion Public Instance Properties

        #region Public Instance Methods

		/// <summary>
		/// Sets the value of the argument on the specified object.
		/// </summary>
		/// <param name="destination">The object on which the value of the argument should be set.</param>
		/// <exception cref="CommandLineArgumentException">The argument is required and no value was specified.</exception>
		/// <exception cref="NotSupportedException">
		/// <para>
		/// The matching property is collection-based, but is not initialized 
		/// and cannot be written to.
		/// </para>
		/// <para>-or-</para>
		/// <para>
		/// The matching property is collection-based, but has no strongly-typed
		/// Add method.
		/// </para>
		/// <para>-or-</para>
		/// <para>
		/// The matching property is collection-based, but the signature of the 
		/// Add method is not supported.
		/// </para>
		/// </exception>
		[ReflectionPermission(SecurityAction.Demand, Flags=ReflectionPermissionFlag.NoFlags)]
		public void Finish(object destination) {
			if (IsRequired && !SeenValue) {
				throw new CommandLineArgumentException(string.Format(CultureInfo.InvariantCulture, "Missing required argument '-{0}'.", LongName));
			}

			if (IsArray) {
				_propertyInfo.SetValue(destination, _collectionValues.ToArray(_elementType), BindingFlags.Default, null, null, CultureInfo.InvariantCulture);
			} else if (IsCollection) {
				// If value of property is null, create new instance of collection 
				if (_propertyInfo.GetValue(destination, BindingFlags.Default, null, null, CultureInfo.InvariantCulture) == null) {
					if (!_propertyInfo.CanWrite) {
						throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Command-line argument '-{0}' is collection-based, but is not initialized and does not allow the collection to be initialized.", LongName));
					}
					object instance = Activator.CreateInstance(_propertyInfo.PropertyType, BindingFlags.Public | BindingFlags.Instance, null, null, CultureInfo.InvariantCulture);
					_propertyInfo.SetValue(destination, instance, BindingFlags.Default, null, null, CultureInfo.InvariantCulture);
				}
                
				object value = _propertyInfo.GetValue(destination, BindingFlags.Default, null, null, CultureInfo.InvariantCulture);
                 
				MethodInfo addMethod = null;

				// Locate Add method with 1 parameter
				foreach (MethodInfo method in value.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)) {
					if (method.Name == "Add" && method.GetParameters().Length == 1) {
						ParameterInfo parameter = method.GetParameters()[0];
						if (parameter.ParameterType != typeof(object)) {
							addMethod = method;
							break;
						}
					}
				}

				if (addMethod == null) {
					throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Collection-based command-line argument '-{0}' has no strongly-typed Add method.", LongName));
				} else {
					try {
						foreach (object item in _collectionValues) {
							addMethod.Invoke(value, BindingFlags.Default, null, new object[] {item}, CultureInfo.InvariantCulture);
						}
					} catch (Exception ex) {
						throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "The signature of the Add method for the collection-based command-line argument '-{0}' is not supported.", LongName), ex);
					}
				}
			} else {
				// this fails on mono if the _argumentValue is null
				if (_argumentValue != null) {
					_propertyInfo.SetValue(destination, _argumentValue, BindingFlags.Default, null, null, CultureInfo.InvariantCulture);
				}
			}
		}

		/// <summary>
		/// Assigns the specified value to the argument.
		/// </summary>
		/// <param name="value">The value that should be assigned to the argument.</param>
		/// <exception cref="CommandLineArgumentException">
		/// <para>Duplicate argument.</para>
		/// <para>-or-</para>
		/// <para>Invalid value.</para>
		/// </exception>
		public void SetValue(string value) {
			if (SeenValue && !AllowMultiple) {
				throw new CommandLineArgumentException(string.Format(CultureInfo.InvariantCulture, "Duplicate command-line argument '-{0}'.", LongName));
			}

			_seenValue = true;
            
			object newValue = ParseValue(ValueType, value);

			if (IsCollection || IsArray) {
				if (Unique && _collectionValues.Contains(newValue)) {
					throw new CommandLineArgumentException(string.Format(CultureInfo.InvariantCulture, "Duplicate value '-{0}' for command-line argument '{1}'.", value, LongName));
				} else {
					_collectionValues.Add(newValue);
				}
			} else {
				_argumentValue = newValue;
			}
		}

        #endregion Public Instance Methods

        #region Private Instance Methods

		private object ParseValue(Type type, string stringData) {
			// null is only valid for bool variables
			// empty string is never valid
			if ((stringData != null || type == typeof(bool)) && (stringData == null || stringData.Length > 0)) {
				try {
					if (type == typeof(string)) {
						return stringData;
					} else if (type == typeof(bool)) {
						if (stringData == null || stringData == "+") {
							return true;
						} else if (stringData == "-") {
							return false;
						}
					} else {
						if (type.IsEnum) {
							try {
								return Enum.Parse(type, stringData, true);
							} catch(ArgumentException ex) {
								string message = "Invalid value {0} for command-line argument '-{1}'. Valid values are: ";
								foreach (object value in Enum.GetValues(type)) {
									message += value.ToString() + ", ";
								}
								// strip last ,
								message = message.Substring(0, message.Length - 2) + ".";
								throw new CommandLineArgumentException(string.Format(CultureInfo.InvariantCulture, message, stringData, LongName), ex);
							}
						} else {
							// Make a guess that the there's a public static Parse method on the type of the property
							// that will take an argument of type string to convert the string to the type 
							// required by the property.
							System.Reflection.MethodInfo parseMethod = type.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, null, CallingConventions.Standard, new Type[] {typeof(string)}, null);
							if (parseMethod != null) {
								// Call the Parse method
								return parseMethod.Invoke(null, BindingFlags.Default, null, new object[] {stringData}, CultureInfo.InvariantCulture);
							} else if (type.IsClass) {
								// Search for a constructor that takes a string argument
								ConstructorInfo stringArgumentConstructor = type.GetConstructor(new Type[] {typeof(string)});

								if (stringArgumentConstructor != null) {
									return stringArgumentConstructor.Invoke(BindingFlags.Default, null, new object[] {stringData}, CultureInfo.InvariantCulture);
								}
							}
						}
					}
				} catch (Exception ex) {
					throw new CommandLineArgumentException(string.Format(CultureInfo.InvariantCulture, "Invalid value '{0}' for command-line argument '-{1}'.", stringData, LongName), ex);
				}
			}

			throw new CommandLineArgumentException(string.Format(CultureInfo.InvariantCulture, "Invalid value '{0}' for command-line argument '-{1}'.", stringData, LongName));
		}

        #endregion Private Instance Methods

        #region Private Static Methods

		private static CommandLineArgumentTypes GetArgumentType(CommandLineArgumentAttribute attribute, PropertyInfo propertyInfo) {
			if (attribute != null) {
				return attribute.Type;
			} else if (IsCollectionType(propertyInfo.PropertyType)) {
				return CommandLineArgumentTypes.MultipleUnique;
			} else {
				return CommandLineArgumentTypes.AtMostOnce;
			}
		}

		private static Type GetElementType(PropertyInfo propertyInfo) {
			Type elementType = null;

			if (propertyInfo.PropertyType.IsArray) {
				elementType = propertyInfo.PropertyType.GetElementType();
				if (elementType == typeof(object)) {
					throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Property {0} is not a strong-typed array.", propertyInfo.Name));
				}
			} else if (typeof(ICollection).IsAssignableFrom(propertyInfo.PropertyType)) {
				// Locate Add method with 1 parameter
				foreach (MethodInfo method in propertyInfo.PropertyType.GetMethods(BindingFlags.Public | BindingFlags.Instance)) {
					if (method.Name == "Add" && method.GetParameters().Length == 1) {
						ParameterInfo parameter = method.GetParameters()[0];
						if (parameter.ParameterType == typeof(object)) {
							throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Property {0} is not a strong-typed collection.", propertyInfo.Name));
						} else {
							elementType = parameter.ParameterType;
							break;
						}
					}
				}

				if (elementType == null) {
					throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Invalid commandline argument type for property {0}.", propertyInfo.Name));
				}
			}

			return elementType;
		}

		private static bool IsCollectionType(Type type) {
			return typeof(ICollection).IsAssignableFrom(type);
		}

		private static bool IsArrayType(Type type) {
			return type.IsArray;
		}

    #endregion Private Static Methods

    #region Private Instance Fields

		private Type _elementType;
		private bool _seenValue;
		private CommandLineArgumentTypes _argumentType;
		private object _argumentValue;
		private ArrayList _collectionValues;
		private PropertyInfo _propertyInfo;
		private CommandLineArgumentAttribute _attribute;

    #endregion Private Instance Fields
	}

	/// <summary>
	/// Contains a strongly typed collection of <see cref="CommandLineArgument"/> objects.
	/// </summary>
	[Serializable]
	public class CommandLineArgumentCollection : CollectionBase {
    
		#region Public Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="CommandLineArgumentCollection"/> class.
		/// </summary>
		public CommandLineArgumentCollection() {
		}
        
		/// <summary>
		/// Initializes a new instance of the <see cref="CommandLineArgumentCollection"/> class
		/// with the specified <see cref="CommandLineArgumentCollection"/> instance.
		/// </summary>
		public CommandLineArgumentCollection(CommandLineArgumentCollection value) {
			AddRange(value);
		}
        
		/// <summary>
		/// Initializes a new instance of the <see cref="CommandLineArgumentCollection"/> class
		/// with the specified array of <see cref="CommandLineArgument"/> instances.
		/// </summary>
		public CommandLineArgumentCollection(CommandLineArgument[] value) {
			AddRange(value);
		}

        #endregion Public Instance Constructors
        
        #region Public Instance Properties

		/// <summary>
		/// Gets or sets the element at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the element to get or set.</param>
		[System.Runtime.CompilerServices.IndexerName("Item")]
		public CommandLineArgument this[int index] {
			get {return ((CommandLineArgument)(base.List[index]));}
			set {base.List[index] = value;}
		}

		/// <summary>
		/// Gets the <see cref="CommandLineArgument"/> with the specified name.
		/// </summary>
		/// <param name="name">The name of the <see cref="CommandLineArgument"/> to get.</param>
		[System.Runtime.CompilerServices.IndexerName("Item")]
		public CommandLineArgument this[string name] {
			get {
				if (name != null) {
					// Try to locate instance using LongName
					foreach (CommandLineArgument CommandLineArgument in base.List) {
						if (name.Equals(CommandLineArgument.LongName)) {
							return CommandLineArgument;
						}
					}

					// Try to locate instance using ShortName
					foreach (CommandLineArgument CommandLineArgument in base.List) {
						if (name.Equals(CommandLineArgument.ShortName)) {
							return CommandLineArgument;
						}
					}
				}
				return null;
			}
		}

    #endregion Public Instance Properties

    #region Public Instance Methods
        
		/// <summary>
		/// Adds a <see cref="CommandLineArgument"/> to the end of the collection.
		/// </summary>
		/// <param name="item">The <see cref="CommandLineArgument"/> to be added to the end of the collection.</param> 
		/// <returns>The position into which the new element was inserted.</returns>
		public int Add(CommandLineArgument item) {
			return base.List.Add(item);
		}

		/// <summary>
		/// Adds the elements of a <see cref="CommandLineArgument"/> array to the end of the collection.
		/// </summary>
		/// <param name="items">The array of <see cref="CommandLineArgument"/> elements to be added to the end of the collection.</param> 
		public void AddRange(CommandLineArgument[] items) {
			for (int i = 0; (i < items.Length); i = (i + 1)) {
				Add(items[i]);
			}
		}

		/// <summary>
		/// Adds the elements of a <see cref="CommandLineArgumentCollection"/> to the end of the collection.
		/// </summary>
		/// <param name="items">The <see cref="CommandLineArgumentCollection"/> to be added to the end of the collection.</param> 
		public void AddRange(CommandLineArgumentCollection items) {
			for (int i = 0; (i < items.Count); i = (i + 1)) {
				Add(items[i]);
			}
		}
        
		/// <summary>
		/// Determines whether a <see cref="CommandLineArgument"/> is in the collection.
		/// </summary>
		/// <param name="item">The <see cref="CommandLineArgument"/> to locate in the collection.</param> 
		/// <returns>
		/// <c>true</c> if <paramref name="item"/> is found in the collection;
		/// otherwise, <c>false</c>.
		/// </returns>
		public bool Contains(CommandLineArgument item) {
			return base.List.Contains(item);
		}
        
		/// <summary>
		/// Copies the entire collection to a compatible one-dimensional array, starting at the specified index of the target array.        
		/// </summary>
		/// <param name="array">The one-dimensional array that is the destination of the elements copied from the collection. The array must have zero-based indexing.</param> 
		/// <param name="index">The zero-based index in <paramref name="array"/> at which copying begins.</param>
		public void CopyTo(CommandLineArgument[] array, int index) {
			base.List.CopyTo(array, index);
		}
        
		/// <summary>
		/// Retrieves the index of a specified <see cref="CommandLineArgument"/> object in the collection.
		/// </summary>
		/// <param name="item">The <see cref="CommandLineArgument"/> object for which the index is returned.</param> 
		/// <returns>
		/// The index of the specified <see cref="CommandLineArgument"/>. If the <see cref="CommandLineArgument"/> is not currently a member of the collection, it returns -1.
		/// </returns>
		public int IndexOf(CommandLineArgument item) {
			return base.List.IndexOf(item);
		}
        
		/// <summary>
		/// Inserts a <see cref="CommandLineArgument"/> into the collection at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
		/// <param name="item">The <see cref="CommandLineArgument"/> to insert.</param>
		public void Insert(int index, CommandLineArgument item) {
			base.List.Insert(index, item);
		}
        
		/// <summary>
		/// Returns an enumerator that can iterate through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="CommandLineArgumentEnumerator"/> for the entire collection.
		/// </returns>
		public new CommandLineArgumentEnumerator GetEnumerator() {
			return new CommandLineArgumentEnumerator(this);
		}
        
		/// <summary>
		/// Removes a member from the collection.
		/// </summary>
		/// <param name="item">The <see cref="CommandLineArgument"/> to remove from the collection.</param>
		public void Remove(CommandLineArgument item) {
			base.List.Remove(item);
		}
        
    #endregion Public Instance Methods
	}

	/// <summary>
	/// Enumerates the <see cref="CommandLineArgument"/> elements of a <see cref="CommandLineArgumentCollection"/>.
	/// </summary>
	public class CommandLineArgumentEnumerator : IEnumerator {

		#region Internal Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="CommandLineArgumentEnumerator"/> class
		/// with the specified <see cref="CommandLineArgumentCollection"/>.
		/// </summary>
		/// <param name="arguments">The collection that should be enumerated.</param>
		internal CommandLineArgumentEnumerator(CommandLineArgumentCollection arguments) {
			IEnumerable temp = arguments;
			_baseEnumerator = temp.GetEnumerator();
		}

    #endregion Internal Instance Constructors

    #region Implementation of IEnumerator
            
		/// <summary>
		/// Gets the current element in the collection.
		/// </summary>
		/// <returns>
		/// The current element in the collection.
		/// </returns>
		public CommandLineArgument Current {
			get { return (CommandLineArgument) _baseEnumerator.Current; }
		}

		object IEnumerator.Current {
			get { return _baseEnumerator.Current; }
		}

		/// <summary>
		/// Advances the enumerator to the next element of the collection.
		/// </summary>
		/// <returns>
		/// <c>true</c> if the enumerator was successfully advanced to the next element; 
		/// <c>false</c> if the enumerator has passed the end of the collection.
		/// </returns>
		public bool MoveNext() {
			return _baseEnumerator.MoveNext();
		}

		bool IEnumerator.MoveNext() {
			return _baseEnumerator.MoveNext();
		}
            
		/// <summary>
		/// Sets the enumerator to its initial position, which is before the 
		/// first element in the collection.
		/// </summary>
		public void Reset() {
			_baseEnumerator.Reset();
		}
            
		void IEnumerator.Reset() {
			_baseEnumerator.Reset();
		}

    #endregion Implementation of IEnumerator

    #region Private Instance Fields
    
		private IEnumerator _baseEnumerator;

    #endregion Private Instance Fields
	}

	/// <summary>
	/// The exception that is thrown when one of the command-line arguments provided 
	/// is not valid.
	/// </summary>
	[Serializable()]
	public sealed class CommandLineArgumentException : ArgumentException {
    #region Public Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="CommandLineArgumentException" /> class.
		/// </summary>
		public CommandLineArgumentException() : base() {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CommandLineArgumentException" /> class
		/// with a descriptive message.
		/// </summary>
		/// <param name="message">A descriptive message to include with the exception.</param>
		public CommandLineArgumentException(string message) : base(message) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CommandLineArgumentException" /> class
		/// with a descriptive message and an inner exception.
		/// </summary>
		/// <param name="message">A descriptive message to include with the exception.</param>
		/// <param name="innerException">A nested exception that is the cause of the current exception.</param>
		public CommandLineArgumentException(string message, Exception innerException) : base(message, innerException) {
		}

		#endregion Public Instance Constructors

    #region Private Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="CommandLineArgumentException" /> class 
		/// with serialized data.
		/// </summary>
		/// <param name="info">The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="StreamingContext" /> that contains contextual information about the source or destination.</param>
		private CommandLineArgumentException(SerializationInfo info, StreamingContext context) : base(info, context) {
		}

    #endregion Private Instance Constructors
	}


	/// <summary>
	/// Allows control of command line parsing.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class CommandLineArgumentAttribute : Attribute {
    #region Public Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="CommandLineArgumentAttribute" /> class
		/// with the specified argument type.
		/// </summary>
		/// <param name="argumentType">Specifies the checking to be done on the argument.</param>
		public CommandLineArgumentAttribute(CommandLineArgumentTypes argumentType) {
			_argumentType = argumentType;
		}

        #endregion Public Instance Constructors

        #region Public Instance Properties

		/// <summary>
		/// Gets or sets the checking to be done on the argument.
		/// </summary>
		/// <value>The checking that should be done on the argument.</value>
		public CommandLineArgumentTypes Type {
			get { return _argumentType; }
		}

		/// <summary>
		/// Gets or sets the long name of the argument.
		/// </summary>
		/// <value>The long name of the argument.</value>
		public string Name {
			get { return _name; }
			set { _name = value; }
		}

		/// <summary>
		/// Gets or sets the short name of the argument.
		/// </summary>
		/// <value>The short name of the argument.</value>
		public string ShortName {
			get { return _shortName; }
			set { _shortName = value; }
		}

		/// <summary>
		/// Gets or sets the description of the argument.
		/// </summary>
		/// <value>The description of the argument.</value>
		public string Description {
			get
			{
				if (_descriptionIsResourceId) {

                    string s = SR.ResourceManager.GetString(_description);
					if (string.IsNullOrEmpty(s))
						return _description;
					return s;
				}
				return _description;
			}
			set { _description = value; }
		}

		/// <summary>
		/// Gets or sets the value deciding whether the description is
		/// a normal string or a resource identifier.
		/// </summary>
		/// <value>The description of the argument.</value>
		public bool DescriptionIsResourceId {
			get { return _descriptionIsResourceId; }
			set { _descriptionIsResourceId = value; }
		}

		#endregion Public Instance Properties

    #region Private Instance Fields

		private CommandLineArgumentTypes _argumentType;
		private string _name;
		private string _shortName;
		private string _description;
		private bool _descriptionIsResourceId = false;

    #endregion Private Instance Fields
	}

	/// <summary>
	/// Marks a command-line option as being the default option.  When the name of 
	/// a command-line argument is not specified, this option will be assumed.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public sealed class DefaultCommandLineArgumentAttribute : CommandLineArgumentAttribute {
		/// <summary>
		/// Initializes a new instance of the <see cref="CommandLineArgumentAttribute" /> class
		/// with the specified argument type.
		/// </summary>
		/// <param name="argumentType">Specifies the checking to be done on the argument.</param>
		public DefaultCommandLineArgumentAttribute(CommandLineArgumentTypes argumentType) : base(argumentType) {
		}
	}

}
