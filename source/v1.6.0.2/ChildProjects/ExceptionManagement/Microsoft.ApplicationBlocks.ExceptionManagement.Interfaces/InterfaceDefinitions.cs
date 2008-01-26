//===============================================================================
// Microsoft Exception Management Application Block for .NET
// http://msdn.microsoft.com/library/en-us/dnbda/html/emab-rm.asp
//
// InterfaceDefinitions.cs
// This file contains the interface definitions for the IExceptionPublisher and 
// IExceptionXmlPublisher interfaces. 
//
// For more information see the Implementing the Interfaces Assembly section 
// of the Exception Management Application Block Implementation Overview. 
//===============================================================================
// Copyright (C) 2000-2001 Microsoft Corporation
// All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================

using System;
using System.Collections.Specialized;
using System.Xml;

namespace Microsoft.ApplicationBlocks.ExceptionManagement
{
	#region Publishing Interfaces
	/// <summary>
	/// Interface to publish exception information.  All exception information is passed as the chain of exception objects.
	/// </summary>
	public interface IExceptionPublisher
	{
		/// <summary>
		/// Method used to publish exception information and additional information.
		/// </summary>
		/// <param name="exception">The exception object whose information should be published.</param>
		/// <param name="additionalInfo">A collection of additional data that should be published along with the exception information.</param>
		/// <param name="configSettings">A collection of name/value attributes specified in the config settings.</param>
		void Publish(Exception exception, NameValueCollection additionalInfo, NameValueCollection configSettings);
	}

	/// <summary>
	/// Interface to publish exception information.  All exception information is passed as XML.
	/// </summary>
	public interface IExceptionXmlPublisher
	{
		/// <summary>
		/// Method used to publish exception information and any additional information in XML.
		/// </summary>
		/// <param name="exceptionInfo">An XML Document containing the all exception information.</param>
		/// <param name="configSettings">A collection of name/value attributes specified in the config settings.</param>
		void Publish(XmlDocument exceptionInfo, NameValueCollection configSettings);
	}
	#endregion
}
