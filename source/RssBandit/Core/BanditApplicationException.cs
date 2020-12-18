#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Runtime.Serialization;
using System.Collections.Specialized;
using System.Security.Permissions;
using System.Security;
using System.Threading;
using System.Security.Principal;

using Microsoft.ApplicationBlocks.ExceptionManagement;

namespace RssBandit {
	
	/// <summary>
	/// Some handled exceptions
	/// </summary>
	public enum ApplicationExceptions {
		/// <summary>
		/// Unknown or unhandled exception.
		/// </summary>
		Unknown,
		/// <summary>
		/// Old feedlist file or old feedlist content format detected
		/// </summary>
		FeedlistOldFormat,
		/// <summary>
		/// File IO error while reading feedlist
		/// </summary>
		FeedlistOnRead,
		/// <summary>
		/// Usually a XmlException while processing the feedlist content
		/// </summary>
		FeedlistOnProcessContent,
		/// <summary>
		/// No feedlist found/available
		/// </summary>
		FeedlistNA,

	}

	#region BanditApplicationException class
	/// <summary>
	///  Bandit Exception should be used for all our own exceptions
	/// </summary>
	[Serializable]
	public class BanditApplicationException :  BaseApplicationException , ISerializable {
		
		internal readonly ApplicationExceptions number;
		private string osNameVersion; 
		private string frameworkVersion;
		private DateTime createdDateTime = DateTime.Now;
		
		#region ctor's
		/// <summary>
		///  Base Contructor
		/// </summary>
		public BanditApplicationException():base() {
			InitializeEnvironmentInformation();
		}

		/// <summary>
		///  Base Constructor with Error Number
		/// </summary>
		/// <param name="number">ApplicationExceptions</param>
		public BanditApplicationException(ApplicationExceptions number):base() {
			this.number = number;
			InitializeEnvironmentInformation();
		}

		/// <summary>
		///  Base Constructor with Error Text
		/// </summary>
		/// <param name="errorText">Error Text </param>
		public BanditApplicationException(string errorText):base(errorText) {
			InitializeEnvironmentInformation();
		}

		/// <summary>
		///	 Base Constructor with Error Text and Error Number
		/// </summary>
		/// <param name="number">ApplicationExceptions</param>
		/// <param name="errorText">Error Text</param>
		public BanditApplicationException(ApplicationExceptions number,string errorText):base(errorText) {
			this.number = number;
			InitializeEnvironmentInformation();
		}

		/// <summary>
		///	 Base Constructor with Error Text and Error Number
		/// </summary>
		/// <param name="number">ApplicationExceptions</param>
		/// <param name="inner">Inner Exception</param>
		public BanditApplicationException(ApplicationExceptions number, Exception inner):base(String.Empty, inner) {
			this.number = number;
			InitializeEnvironmentInformation();
		}

		/// <summary>
		/// Base Constructor with Error Text and Exception
		/// </summary>
		/// <param name="errorText">Error Text </param>
		/// <param name="inner">Inner Exception</param>
		public BanditApplicationException(string errorText,Exception inner):base(errorText,inner) {
			InitializeEnvironmentInformation();
		}

		/// <summary>
		/// Base Constructor with Error Text and Exception and Number
		/// </summary>
		/// <param name="number">ApplicationExceptions</param>
		/// <param name="errorText">Error Text</param>
		/// <param name="inner">Exception</param>
		public BanditApplicationException(ApplicationExceptions number,string errorText,Exception inner):base(errorText,inner) {
			this.number = number;
			InitializeEnvironmentInformation();
		}


		#endregion
	
		#region Serialization
		/// <summary>
		/// ISeriazable Constructor used for Serializing the Exception
		/// </summary>
		/// <param name="info">Serialization Info Object</param>
		/// <param name="context">Serializtion Context </param>
		protected BanditApplicationException(SerializationInfo info,StreamingContext context):base(info,context) {
			try {
				this.number = (ApplicationExceptions)info.GetValue("number", typeof(ApplicationExceptions));
				this.osNameVersion = info.GetString("osNameVersion");
				this.frameworkVersion = info.GetString("frameworkVersion");
				
			}
			catch {}
			finally {
				this.number = ApplicationExceptions.Unknown;
			}
		}

		/// <summary>
		/// Interface for serializing
		/// </summary>
		/// <param name="info">Info Object</param>
		/// <param name="context">Context Object</param>
		public override void GetObjectData(SerializationInfo info ,StreamingContext context) {
			info.AddValue("number",this.number);
			info.AddValue("osNameVersion", osNameVersion, typeof(string));
			info.AddValue("frameworkVersion", frameworkVersion, typeof(string));
			// base class operation
			base.GetObjectData(info,context);
		}


		#endregion

		#region Public Properties
		/// <summary>
		///  Returns the number of the Exception
		/// </summary>
		public ApplicationExceptions Number {
			get { return this.number;}
		}


		/// <summary>
		/// Machine name where the exception occurred.
		/// </summary>
		public string OsNameVersion {
			get {
				return osNameVersion;
			}
		}

		/// <summary>
		/// Framework Version used where the exception occurred.
		/// </summary>
		public string FrameworkVersion {
			get {
				return frameworkVersion;
			}
		}

		#endregion

		#region Privat Properties
		/// <summary>
		/// Initialization function that gathers environment information safely.
		/// </summary>
		private void InitializeEnvironmentInformation() {									
			try {				
				osNameVersion = Environment.OSVersion.ToString();
			} catch {
				osNameVersion = "n/a";
			}
					
			try {					
				frameworkVersion =  ".NET CLR " + System.Runtime.InteropServices.RuntimeEnvironment.GetSystemVersion();
			} catch {
				frameworkVersion = "n/a";
			}			
		}
		#endregion
	}
	#endregion

}
