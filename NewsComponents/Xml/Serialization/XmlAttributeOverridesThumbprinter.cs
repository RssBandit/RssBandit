#region Using directives

using System;
using System.Collections;
using System.Text;
using System.Xml.Serialization;
using System.Reflection;
#endregion

namespace NewsComponents.Xml.Serialization
{
	/// <summary>
	/// Helpers to create a normalized thumbprint
	/// for XmlAttributeOverrides objects.
	/// </summary>
	public class XmlAttributeOverridesThumbprinter
	{
		private XmlAttributeOverridesThumbprinter()
		{

		}

		/// <summary>
		/// Gets a thumbprint (signature)
		/// for the content of the XmlAttributeOverrides
		/// </summary>
		/// <param name="overrides"></param>
		/// <returns></returns>
		public static string GetThumbprint( XmlAttributeOverrides overrides )
		{
			return GetClassThumbprint(overrides);
		}

		private static string GetClassThumbprint(XmlAttributeOverrides overrides)
		{
			Hashtable types = GetTypesHashtable(overrides);
			// Types are the most significant information
			// in the key ... that why the type info comes first

			// what does the thumbprint of an XmlAttributeOverrides need to
			// consist of?
			// Type names are the key of the outer hashtable
			// The values of the outer hashtable are more hashtables
			// The keys of the inner hashtables are member names. The
			// values of the inner hashtable are XmlAttributes objects.
			// I.e. to create a content-based thumbprint of an 
			// XmlAttributeOverrides we need to normalize first the
			// type names, then the member names and finally include the
			// values of each XmlAttributes object in the thumbprint.

			// so my first attempt at a thumbprint looks like this:
			// typeName:memberName:attributes:memberName:attributes:...
			// memberNames are orders alphabetically
			
			// the complete thumbprint for the XmlAttributeOverrides
			// concatenates the individual thumbrprints for a type in
			// the alphabetical order of the type names.

			StringSorter sorter = new StringSorter();
			foreach (Type t in types.Keys)
			{
				sorter.AddString(t.AssemblyQualifiedName);
			}
			string[] sortedTypeNames = sorter.GetOrderedArray();
			
			// now we have the types for which we have overriding attributes 
			// in alphabetical order

			// TODO
			// Now generate thumbprint for each member of
			// each type
			StringBuilder printBuilder = new StringBuilder();

			foreach (string typeName in sortedTypeNames)
			{
				System.Diagnostics.Debug.WriteLine(string.Format("+++ Starting thumbprint for type {0}", typeName));
				printBuilder.AppendFormat(">>{0}>>", typeName);
				GetTypePrint(typeName, types, printBuilder);
				printBuilder.Append("<<");
				System.Diagnostics.Debug.WriteLine(string.Format("--- Finished thumbprint for type {0}", typeName));
			}
			return printBuilder.ToString();

		}

		private static void GetTypePrint(string typeName, Hashtable attributes, StringBuilder printBuilder)
		{
			Type t = Type.GetType(typeName);
			Hashtable memberAttributes = attributes[t] as Hashtable;
			System.Diagnostics.Debug.Assert(null != memberAttributes);
			StringSorter sorter = new StringSorter();
			foreach (string memberName in memberAttributes.Keys)
			{
				sorter.AddString(memberName);
			}
			string[] sortedMemberNames = sorter.GetOrderedArray();
		
			foreach (string memberName in sortedMemberNames)
			{
				printBuilder.AppendFormat("**{0}**", memberName);
				System.Diagnostics.Debug.WriteLine(string.Format("++ Started member thumbprint for type {0}, member:{1}.", typeName, memberName));
				GetXmlAttributesThumbprint(memberAttributes[memberName] as XmlAttributes, printBuilder);
				printBuilder.Append("***");
				System.Diagnostics.Debug.WriteLine(string.Format("-- Finished member thumbprint for type {0}, member:{1}.", typeName, memberName));
			}
		}

		private static void GetXmlAttributesThumbprint(XmlAttributes atts, StringBuilder printBuilder )
		{
			if (null == atts) return;

			printBuilder.Append("any");
			printBuilder.Append(":");
			AddXmlAnyElementsPrint(atts.XmlAnyElements, printBuilder);
			printBuilder.Append(":");
			AddXmlArrayPrint(atts.XmlArray, printBuilder);
			printBuilder.Append(":");
			AddXmlArrayItemsPrint(atts.XmlArrayItems, printBuilder);
			printBuilder.Append(":");
			AddXmlAttributePrint( atts.XmlAttribute, printBuilder );
			printBuilder.Append(":");
			AddXmlChoiceIdentifierPrint( atts.XmlChoiceIdentifier, printBuilder );
			printBuilder.Append(":");
			AddXmlDefaultValuePrint( atts.XmlDefaultValue, printBuilder );
			printBuilder.Append(":");
			AddXmlElementsPrint( atts.XmlElements, printBuilder );
			printBuilder.Append(":");
			AddXmlEnumPrint( atts.XmlEnum, printBuilder );
			printBuilder.Append(":");
			AddXmlIgnorePrint( atts.XmlIgnore, printBuilder );
			printBuilder.Append(":");
			AddXmlNamespacePrint( atts.Xmlns, printBuilder );
			printBuilder.Append(":");
			AddXmlRootPrint( atts.XmlRoot, printBuilder );
			printBuilder.Append(":");
			AddXmlTextPrint( atts.XmlText, printBuilder );
			printBuilder.Append(":");
			AddXmlTypePrint( atts.XmlType, printBuilder );
		}

		private static void AddXmlAnyElementsPrint(XmlAnyElementAttributes atts, StringBuilder printBuilder)
		{
			if (null != atts)
			{
				foreach (XmlAnyElementAttribute att in atts)
				{
					printBuilder.Append("anyatt");
					printBuilder.Append("/");
					printBuilder.Append(att.Name);
					printBuilder.Append("/");
					printBuilder.Append(att.Namespace);
					printBuilder.Append("::");
				}
			}
			printBuilder.Append("%%");
		}

		private static void AddXmlArrayPrint(XmlArrayAttribute att, StringBuilder printBuilder)
		{
			if (null != att)
			{
				printBuilder.Append(att.ElementName);
				printBuilder.Append("/");
				printBuilder.Append(att.Form);
				printBuilder.Append("/");
				printBuilder.Append(att.IsNullable);
				printBuilder.Append("/");
				printBuilder.Append(att.Namespace);
			}
			printBuilder.Append("%%");
		}

		private static void AddXmlArrayItemsPrint( XmlArrayItemAttributes atts, StringBuilder printBuilder )
		{
			if (null != atts)
			{
				foreach (XmlArrayItemAttribute att in atts)
				{
					printBuilder.Append(att.DataType);
					printBuilder.Append("/");
					printBuilder.Append(att.ElementName);
					printBuilder.Append("/");
					printBuilder.Append(att.Form);
					printBuilder.Append("/");
					printBuilder.Append(att.IsNullable);
					printBuilder.Append("/");
					printBuilder.Append(att.Namespace);
					printBuilder.Append("/");
					printBuilder.Append(att.NestingLevel);
					if (null != att.Type)
					{
						printBuilder.Append("/");
						printBuilder.Append(att.Type.AssemblyQualifiedName);
					}
					printBuilder.Append("::");
				}
			}
			printBuilder.Append("%%");
		}

		private static void AddXmlAttributePrint(XmlAttributeAttribute att, StringBuilder printBuilder)
		{
			if (null != att)
			{
				printBuilder.Append(att.AttributeName);
				printBuilder.Append("/");
				printBuilder.Append(att.DataType);
				printBuilder.Append("/");
				printBuilder.Append(att.Form);
				printBuilder.Append("/");
				printBuilder.Append(att.Namespace);
				printBuilder.Append("/");
				if (null != att.Type)
				{
					printBuilder.Append(att.Type.AssemblyQualifiedName);
				}
			}
			printBuilder.Append("%%");
		}

		private static void AddXmlChoiceIdentifierPrint(XmlChoiceIdentifierAttribute att, StringBuilder printBuilder)
		{
			if (null != att)
			{
				printBuilder.Append(att.MemberName);
			}
			printBuilder.Append("%%");

		}

		private static void AddXmlDefaultValuePrint(object defaultValue, StringBuilder printBuilder)
		{
			if (null != defaultValue)
			{
				printBuilder.Append(defaultValue);
			}
			printBuilder.Append("%%");
		}

		private static void AddXmlElementsPrint(XmlElementAttributes atts, StringBuilder printBuilder)
		{
			if (null != atts)
			{
				foreach (XmlElementAttribute att in atts)
				{
					printBuilder.Append(att.DataType);
					printBuilder.Append("/");
					printBuilder.Append(att.ElementName);
					printBuilder.Append("/");
					printBuilder.Append(att.Form);
					printBuilder.Append("/");
					printBuilder.Append(att.IsNullable);
					printBuilder.Append("/");
					printBuilder.Append(att.Namespace);
					printBuilder.Append("/");
					if (null != att.Type)
					{
						printBuilder.Append(att.Type.AssemblyQualifiedName);
					}
					printBuilder.Append("::");
				}
			}
			printBuilder.Append("%%");
		}

		private static void AddXmlEnumPrint(XmlEnumAttribute att, StringBuilder printBuilder)
		{
			if (null != att)
			{
				printBuilder.Append(att.Name);
			}
			printBuilder.Append("%%");
		}

		private static void AddXmlIgnorePrint(bool ignore, StringBuilder printBuilder)
		{
			printBuilder.Append(ignore);
			printBuilder.Append("%%");
		}

		private static void AddXmlNamespacePrint(bool Xmlns, StringBuilder printBuilder)
		{
			printBuilder.Append(Xmlns);
			printBuilder.Append("%%");
		}

		internal static void AddXmlRootPrint(XmlRootAttribute root, StringBuilder printBuilder)
		{
			if (null != root)
			{
				printBuilder.Append(root.DataType);
				printBuilder.Append("/");
				printBuilder.Append(root.ElementName);
				printBuilder.Append("/");
				printBuilder.Append(root.IsNullable);
				printBuilder.Append("/");
				printBuilder.Append(root.Namespace);
			}
			printBuilder.Append("%%");
		}

		private static void AddXmlTextPrint(XmlTextAttribute att, StringBuilder printBuilder)
		{
			if (null != att)
			{
				printBuilder.Append(att.DataType);
				printBuilder.Append("/");
				if (null != att.Type)
				{
					printBuilder.Append(att.Type.AssemblyQualifiedName);
				}
			}
			printBuilder.Append("%%");
		}

		private static void AddXmlTypePrint(XmlTypeAttribute att, StringBuilder printBuilder)
		{
			if (null != att)
			{
				printBuilder.Append(att.IncludeInSchema);
				printBuilder.Append("/");
				printBuilder.Append(att.Namespace);
				printBuilder.Append("/");
				printBuilder.Append(att.TypeName);
			}
			printBuilder.Append("%%");
		}

		private static Hashtable GetTypesHashtable(XmlAttributeOverrides overrides)
		{
			return GetHashtable(overrides, "types");
		}

		private static Hashtable GetHashtable(XmlAttributeOverrides overrides, string fieldName)
		{
			Hashtable types;
			FieldInfo typesInfo = typeof(XmlAttributeOverrides).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField) as FieldInfo;
			if (null == typesInfo)
			{
				throw new System.ArgumentException("XmlAttributeOverrides does conform to expected structure");
			}

			types = typesInfo.GetValue(overrides) as Hashtable;
			System.Diagnostics.Debug.Assert(null != types);
			return types;
		}

	}
}
