//===============================================================================
// Microsoft Exception Management Application Block for .NET
// http://msdn.microsoft.com/library/en-us/dnbda/html/emab-rm.asp
//
// ExceptionManagerSectionHandler.cs
// This file contains the configuration section handler for the exception 
// Management settings.
//
// For more information see the Implementation of the 
// ExceptionManagementSectionHandler Class section of the Exception Management 
// Application Block Implementation Overview. 
//===============================================================================
// Copyright (C) 2000-2001 Microsoft Corporation
// All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================

using System;
using System.Resources;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;
using System.Xml.Serialization;
using System.Configuration;
using System.Globalization;

namespace Microsoft.ApplicationBlocks.ExceptionManagement
{
	#region Configuration Class Definitions
	#region Enum Definitions
	/// <summary>
	/// Enum containing the mode options for the exceptionManagement tag.
	/// </summary>
	public enum ExceptionManagementMode 
	{
		/// <summary>The ExceptionManager should not process exceptions.</summary>
		Off,
		/// <summary>The ExceptionManager should process exceptions. This is the default.</summary>
		On
	}
	/// <summary>
	/// Enum containing the mode options for the publisher tag.
	/// </summary>
	public enum PublisherMode
	{
		/// <summary>The ExceptionManager should not call the publisher.</summary>
		Off,
		/// <summary>The ExceptionManager should call the publisher. This is the default.</summary>
		On		
	}
	/// <summary>
	/// Enum containing the format options for the publisher tag.
	/// </summary>
	public enum PublisherFormat 
	{
		/// <summary>The ExceptionManager should call the IExceptionPublisher interface of the publisher. 
		/// This is the default.</summary>
		Exception,
		/// <summary>The ExceptionManager should call the IExceptionXmlPublisher interface of the publisher.</summary>
		Xml
	}
	#endregion

	#region Class Definitions
	/// <summary>
	/// Class that defines the exception management settings in the config file.
	/// </summary>
	public class ExceptionManagementSettings
	{
		private ExceptionManagementMode mode = ExceptionManagementMode.On;
		private ArrayList publishers = new ArrayList();

		/// <summary>
		/// Specifies the whether the exceptionManagement settings are "on" or "off".
		/// </summary>
		public ExceptionManagementMode Mode
		{
			get
			{
				return mode;
			}
			set
			{
				mode = value;
			}
		}

		/// <summary>
		/// An ArrayList containing all of the PublisherSettings listed in the config file.
		/// </summary>
		public ArrayList Publishers
		{
			get
			{
				return publishers;
			}
		}

		/// <summary>
		/// Adds a PublisherSettings to the arraylist of publishers.
		/// </summary>
		public void AddPublisher(PublisherSettings publisher)
		{
			publishers.Add(publisher);
		}
	}

	/// <summary>
	/// Class that defines the publisher settings within the exception management settings in 
	/// the config file.
	/// </summary>
	public class PublisherSettings
	{
		private PublisherMode mode = PublisherMode.On;
		private PublisherFormat exceptionFormat = PublisherFormat.Exception;
		private string assemblyName;
		private string typeName;
		private TypeFilter includeTypes;
		private TypeFilter excludeTypes;
		private NameValueCollection otherAttributes = new NameValueCollection();
		
		/// <summary>
		/// Specifies the whether the exceptionManagement settings are "on" or "off".
		/// </summary>
		public PublisherMode Mode
		{
			get
			{
				return mode;
			}
			set
			{
				mode = value;
			}
		}

		/// <summary>
		/// Specifies the whether the publisher supports the IExceptionXmlPublisher interface (value is set to "xml")
		/// or the publisher supports the IExceptionPublisher interface (value is either left off or set to "exception").
		/// </summary>
		public PublisherFormat ExceptionFormat
		{
			get
			{
				return exceptionFormat;
			}
			set
			{
				exceptionFormat = value;
			}
		}

		/// <summary>
		/// The assembly name of the publisher component that will be used to invoke the object.
		/// </summary>
		public string AssemblyName
		{
			get
			{
				return assemblyName;
			}
			set
			{
				assemblyName = value;
			}
		}

		/// <summary>
		/// The type name of the publisher component that will be used to invoke the object.
		/// </summary>
		public string TypeName
		{
			get
			{
				return typeName;
			}
			set
			{
				typeName = value;
			}
		}

		/// <summary>
		/// A semicolon delimited list of all exception types that the publisher will be invoked for. 
		/// A "*" can be used to specify all types and is the default value if this is left off.
		/// </summary>
		public TypeFilter IncludeTypes
		{
			get
			{
				return includeTypes;
			}
			set
			{
				includeTypes = value;
			}
		}

		/// <summary>
		/// A semicolon delimited list of all exception types that the publisher will not be invoked for. 
		/// A "*" can be used to specify all types. The default is to exclude no types.
		/// </summary>
		public TypeFilter ExcludeTypes
		{
			get
			{
				return excludeTypes;
			}
			set
			{
				excludeTypes = value;
			}
		}
				
		/// <summary>
		/// Determines whether the exception type is to be filtered out based on the includes and exclude
		/// types specified.
		/// </summary>
		/// <param name="exceptionType">The Type of the exception to check for filtering.</param>
		/// <returns>True is the exception type is to be filtered out, false if it is not filtered out.</returns>
		public bool IsExceptionFiltered(Type exceptionType)
		{
			// If no types are excluded then the exception type is not filtered.
			if (excludeTypes == null) return false;

			// If the Type is in the Exclude Filter
			if (MatchesFilter(exceptionType, excludeTypes))
			{
				// If the Type is in the Include Filter
				if (MatchesFilter(exceptionType, includeTypes))
				{
					// The Type is not filtered out because it was explicitly Included.
					return false;
				}
				// If the Type is not in the Exclude Filter
				else
				{
					// The Type is filtered because it was Excluded and did not match the Include Filter.
					return true;
				}
			}
			// Otherwise it is not Filtered.
			else
			{
				// The Type is not filtered out because it did not match the Exclude Filter.
				return false;
			}
		}
		
		/// <summary>
		/// Determines if a type is contained the supplied filter. 
		/// </summary>
		/// <param name="type">The Type to look for</param> 
		/// <param name="typeFilter">The Filter to test against the Type</param>
		/// <returns>true or false</returns>
		private bool MatchesFilter(Type type, TypeFilter typeFilter)
		{
			TypeInfo typeInfo;

			// If no filter is provided type does not match the filter.
			if (typeFilter == null) return false;

			// If all types are accepted in the filter (using the "*") return true.
			if (typeFilter.AcceptAllTypes) return true;

			// Loop through the types specified in the filter.
			for (int i=0;i<typeFilter.Types.Count;i++)
			{
				typeInfo = (TypeInfo)typeFilter.Types[i];

				// If the Type matches this type in the Filter, then return true.
				if (typeInfo.ClassType.Equals(type)) return true;

				// If the filter type includes SubClasses of itself (it had a "+" before the type in the
				// configuration file) AND the Type is a SubClass of the filter type, then return true.
                if (typeInfo.IncludeSubClasses == true && typeInfo.ClassType.IsAssignableFrom(type)) return true;			
			}
			// If no matches are found return false.
			return false;
		}

		/// <summary>
		/// A collection of any other attributes included within the publisher tag in the config file. 
		/// </summary>
		public NameValueCollection OtherAttributes
		{
			get
			{
				return otherAttributes;
			}
		}

		/// <summary>
		/// Allows name/value pairs to be added to the Other Attributes collection.
		/// </summary>
		public void AddOtherAttributes(string name, string value)
		{
			otherAttributes.Add(name, value);
		}
	}
		
	/// <summary>
	/// TypeFilter class stores contents of the Include and Exclude filters provided in the
	/// configuration file
	/// </summary>
	public class TypeFilter
	{	
		private bool acceptAllTypes = false;
		private ArrayList types = new ArrayList();
		
		/// <summary>
		/// Indicates if all types should be accepted for a filter
		/// </summary>
		public bool AcceptAllTypes
		{
			get
			{
				return acceptAllTypes;
			}
			
			set
			{
				acceptAllTypes = value;
			}
		}
		
		/// <summary>
		/// Collection of types for the filter
		/// </summary>
		public ArrayList Types
		{
			get
			{
				return types;
			}
		}
	}
	
	/// <summary>
	/// TypeInfo class contains information about each type within a TypeFilter
	/// </summary>
	public class TypeInfo
	{
		private Type classType;
		private bool includeSubClasses = false;
		
		/// <summary>
		/// Indicates if subclasses are to be included with the type specified in the Include and Exclude filters
		/// </summary>
		public bool IncludeSubClasses
		{
			get
			{
				return includeSubClasses;
			}
			
			set
			{
				includeSubClasses = value;
			}
		}
		
		/// <summary>
		/// The Type class representing the type specified in the Include and Exclude filters
		/// </summary>
		public Type ClassType
		{
			get
			{
				return classType;	
			}
			
			set
			{
				classType = value;
			}
		}
	}

	#endregion
	#endregion

	#region ExceptionManagerSectionHandler
	/// <summary>
	/// The Configuration Section Handler for the "exceptionManagement" section of the config file. 
	/// </summary>
	public class ExceptionManagerSectionHandler : IConfigurationSectionHandler
	{
		#region Constructors
		/// <summary>
		/// The constructor for the ExceptionManagerSectionHandler to initialize the resource file.
		/// </summary>
		public ExceptionManagerSectionHandler()
		{
			// Load Resource File for localized text.
			resourceManager = new ResourceManager(this.GetType().Namespace + ".ExceptionManagerText",this.GetType().Assembly);
		}
		#endregion

		#region Declare variables
		// Member variables.
		private readonly static char EXCEPTION_TYPE_DELIMITER = Convert.ToChar(";");
		private const string EXCEPTIONMANAGEMENT_MODE = "mode";
		private const string PUBLISHER_NODENAME = "publisher";
		private const string PUBLISHER_MODE = "mode";
		private const string PUBLISHER_ASSEMBLY = "assembly";
		private const string PUBLISHER_TYPE = "type";
		private const string PUBLISHER_EXCEPTIONFORMAT = "exceptionFormat";
		private const string PUBLISHER_INCLUDETYPES = "include";
		private const string PUBLISHER_EXCLUDETYPES = "exclude";
		private ResourceManager resourceManager;
		#endregion

		/// <summary>
		/// Builds the ExceptionManagementSettings and PublisherSettings structures based on the configuration file.
		/// </summary>
		/// <param name="parent">Composed from the configuration settings in a corresponding parent configuration section.</param>
		/// <param name="configContext">Provides access to the virtual path for which the configuration section handler computes configuration values. Normally this parameter is reserved and is null.</param>
		/// <param name="section">The XML node that contains the configuration information to be handled. section provides direct access to the XML contents of the configuration section.</param>
		/// <returns>The ExceptionManagementSettings struct built from the configuration settings.</returns>
		public object Create(object parent,object configContext,XmlNode section)
		{
			try
			{
				ExceptionManagementSettings settings = new ExceptionManagementSettings();

				// Exit if there are no configuration settings.
				if (section == null) return settings;

				XmlNode currentAttribute;
				XmlAttributeCollection nodeAttributes = section.Attributes;

				// Get the mode attribute.
				currentAttribute = nodeAttributes.RemoveNamedItem(EXCEPTIONMANAGEMENT_MODE);
				if (currentAttribute != null && currentAttribute.Value.ToUpper(CultureInfo.InvariantCulture) == "OFF") 
				{
					settings.Mode = ExceptionManagementMode.Off;
				}

				#region Loop through the publisher components and load them into the ExceptionManagementSettings
				// Loop through the publisher components and load them into the ExceptionManagementSettings.
				PublisherSettings publisherSettings;
				foreach(XmlNode node in section.ChildNodes)
				{
					if (node.Name == PUBLISHER_NODENAME)
					{
						// Initialize a new PublisherSettings.
						publisherSettings = new PublisherSettings();

						// Get a collection of all the attributes.
						nodeAttributes = node.Attributes;

						#region Remove the known attributes and load the struct values
						// Remove the mode attribute from the node and set its value in PublisherSettings.
						currentAttribute = nodeAttributes.RemoveNamedItem(PUBLISHER_MODE);
						if (currentAttribute != null && currentAttribute.Value.ToUpper(CultureInfo.InvariantCulture) == "OFF") publisherSettings.Mode = PublisherMode.Off;
				
						// Remove the assembly attribute from the node and set its value in PublisherSettings.
						currentAttribute = nodeAttributes.RemoveNamedItem(PUBLISHER_ASSEMBLY);
						if (currentAttribute != null) publisherSettings.AssemblyName = currentAttribute.Value;
				
						// Remove the type attribute from the node and set its value in PublisherSettings.
						currentAttribute = nodeAttributes.RemoveNamedItem(PUBLISHER_TYPE);
						if (currentAttribute != null) publisherSettings.TypeName = currentAttribute.Value;

						// Remove the exceptionFormat attribute from the node and set its value in PublisherSettings.
						currentAttribute = nodeAttributes.RemoveNamedItem(PUBLISHER_EXCEPTIONFORMAT);
						if (currentAttribute != null && currentAttribute.Value.ToUpper(CultureInfo.InvariantCulture) == "XML") publisherSettings.ExceptionFormat = PublisherFormat.Xml;

						// Remove the include attribute from the node and set its value in PublisherSettings.
						currentAttribute = nodeAttributes.RemoveNamedItem(PUBLISHER_INCLUDETYPES);
						if (currentAttribute != null)
						{
							publisherSettings.IncludeTypes = LoadTypeFilter(currentAttribute.Value.Split(EXCEPTION_TYPE_DELIMITER));
						}

						// Remove the exclude attribute from the node and set its value in PublisherSettings.
						currentAttribute = nodeAttributes.RemoveNamedItem(PUBLISHER_EXCLUDETYPES);
						if (currentAttribute != null)
						{
							publisherSettings.ExcludeTypes = LoadTypeFilter(currentAttribute.Value.Split(EXCEPTION_TYPE_DELIMITER));
						}

						#endregion

						#region Loop through any other attributes and load them into OtherAttributes
						// Loop through any other attributes and load them into OtherAttributes.
						for (int i = 0; i < nodeAttributes.Count; i++)
						{
							publisherSettings.AddOtherAttributes(nodeAttributes.Item(i).Name,nodeAttributes.Item(i).Value);
						}
						#endregion

						// Add the PublisherSettings to the publishers collection.
						settings.Publishers.Add(publisherSettings);
					}
				}

				// Remove extra allocated space of the ArrayList of Publishers. 
				settings.Publishers.TrimToSize();

				#endregion

				// Return the ExceptionManagementSettings loaded with the values from the config file.
				return settings;
			}
			catch(Exception exc)
			{
				throw new ConfigurationException(resourceManager.GetString("RES_EXCEPTION_LOADING_CONFIGURATION"), exc, section);
			}
		}

		/// <summary>
		/// Creates TypeFilter with type information from the string array of type names.
		/// </summary>
		/// <param name="rawFilter">String array containing names of types to be included in the filter.</param>
		/// <returns>TypeFilter object containing type information.</returns>
		private TypeFilter LoadTypeFilter(string[] rawFilter)
		{
			// Initialize filter
			TypeFilter typeFilter = new TypeFilter();

			// Verify information was passed in
			if (rawFilter != null) 
			{
				TypeInfo exceptionTypeInfo;
		
				// Loop through the string array
				for (int i=0;i<rawFilter.GetLength(0);i++)
				{
					// If the wildcard character "*" exists set the TypeFilter to accept all types.
					if (rawFilter[i] == "*") 
					{
						typeFilter.AcceptAllTypes = true;
					}

					else
					{
						try
						{
							if (rawFilter[i].Length > 0)
							{
								// Create the TypeInfo class
								exceptionTypeInfo = new TypeInfo();

								// If the string starts with a "+"
								if (rawFilter[i].Trim().StartsWith("+")) 
								{
									// Set the TypeInfo class to include subclasses
									exceptionTypeInfo.IncludeSubClasses = true;
									// Get the Type class from the filter privided.
									exceptionTypeInfo.ClassType = Type.GetType(rawFilter[i].Trim().TrimStart(Convert.ToChar("+")),true);
									
								}
								else
								{
									// Set the TypeInfo class not to include subclasses
									exceptionTypeInfo.IncludeSubClasses = false;
									// Get the Type class from the filter privided.
									exceptionTypeInfo.ClassType = Type.GetType(rawFilter[i].Trim(),true);	
																
								}

								// Add the TypeInfo class to the TypeFilter
								typeFilter.Types.Add(exceptionTypeInfo);
							}
						}
						catch(TypeLoadException e)
						{
							// If the Type could not be created throw a configuration exception.
							ExceptionManager.PublishInternalException(new ConfigurationException(resourceManager.GetString("RES_EXCEPTION_LOADING_CONFIGURATION"), e), null);
						}													
					}
				}
			}
			return typeFilter;
		}
	}
	#endregion
}
