#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

#if UseResourceInfragistics
// assembly namespace clash: within IG2.Shared and IG3.Wpf we have the "Infragistics.Shared" namepsace :-(
extern alias igwin; 
#endif

using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Threading;


namespace RssBandit.Resources
{
    #if UseResourceInfragistics
	/// <summary>
	/// Class to translate all used Infragistics controls
	/// </summary>
	public abstract class ResourceInfragistics
	{
		/// <summary>
		/// Translates all Infragistics Modules to the current UI Culture
		/// </summary>
		public static void TranslateAll() 
		{
			ResourceInfragisticsShared.Manager.Translate();
			ResourceInfragisticsWin.Manager.Translate();
			ResourceInfragisticsWinExplorerBar.Manager.Translate();
			ResourceInfragisticsWinToolbars.Manager.Translate();
			ResourceInfragisticsWinTree.Manager.Translate();
		}
	}
	
	internal abstract class ResourceBase
	{
		private ResourceManager _rm;
		
		protected ResourceManager rm
		{
			get
			{
				if (_rm == null)
					_rm = new ResourceManager(this.GetType().Namespace + ResourceFileName(), Assembly.GetExecutingAssembly());
					
				return _rm;
			}
		}

		public abstract string ResourceFileName();
	}

	/// <summary>
	/// Base Class for all Infragistics Resource Classes
	/// Sets the Localizable Strings for the entire Namespace only once
	/// </summary>
	internal abstract class ResourceInfragisticsBase : ResourceBase
	{
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ResourceInfragisticsBase"/> class.
		/// </summary>
		/// <param name="pCustomizer">The Infragistics Resource customizer instance</param>
        protected ResourceInfragisticsBase(igwin::Infragistics.Shared.ResourceCustomizer pCustomizer) 
		{
			mCustomizer = pCustomizer;
		}

		#region -------------- Instance part ----------------------

		// Holds the Customizer which must be supplied in Constructor!
        private readonly igwin::Infragistics.Shared.ResourceCustomizer mCustomizer;

		// Holds the last translated Language (ISO two Letter)
		private string mTranslatedLanguage = "";

		/// <summary>
		/// Translates alle the Strings!
		/// </summary>
		public virtual void Translate()
		{
			// Check current UI Culture and see if we already translated this language!
			CultureInfo lCurrentCulture = Thread.CurrentThread.CurrentUICulture;
			if (lCurrentCulture.TwoLetterISOLanguageName == mTranslatedLanguage)
			{
				return; // already translated
			}

			// Reset to original Strings handled by the Factory Method overriden in specialized Classes
			mCustomizer.ResetAllCustomizedStrings();

			// Iterate through all keys and set the translated Text
			ResourceSet rs = this.rm.GetResourceSet(lCurrentCulture, true, true);
			
			if (rs != null)
			{
				IDictionaryEnumerator dictEnum = rs.GetEnumerator();
				while (dictEnum.MoveNext())
				{
					string lKey = dictEnum.Key.ToString();
					// Leave the others untouched
					if (dictEnum.Value is string)
					{
						string lVal = dictEnum.Value.ToString();
						mCustomizer.SetCustomizedString(lKey, lVal);
					}
				}
			}
			// Store the current Language as translated
			mTranslatedLanguage = lCurrentCulture.TwoLetterISOLanguageName;			
		}

		#endregion
	}


	/// <summary>
	/// Resource Manager for Infragistics 
	/// Sets the Localizable Strings for the entire Namespace!
	/// </summary>
	internal class ResourceInfragisticsWin : ResourceInfragisticsBase
	{
		#region ------------ Static part --------------------------

		public ResourceInfragisticsWin() : base(igwin::Infragistics.Win.Resources.Customizer)
		{
			// Ensure Shared Translation
			ResourceInfragisticsShared.Manager.Translate();
		}


		// Singleton Instance
		private static readonly ResourceInfragisticsBase mManager = new ResourceInfragisticsWin();

		/// <summary>
		/// Gets the resource manager for the assembly resource file (Singleton)
		/// </summary>
		public static ResourceInfragisticsBase Manager
		{
			get { return mManager; }
		}

		#endregion

		#region -------------- Instance part ----------------------

		/// <summary>
		/// To be overridden if the resource file name differs from the used
		/// default DefaultResourceFileName (".MultiLang")
		/// </summary>
		/// <returns>the special Resource File Name</returns>
		public override string ResourceFileName()
		{
			return ".InfragisticsWin";
		}

		#endregion
	}


	/// <summary>
	/// Resource Manager for Infragistics 
	/// Sets the Localizable Strings for the entire Namespace!
	/// </summary>
	internal class ResourceInfragisticsShared : ResourceInfragisticsBase
	{
		#region ------------ Static part --------------------------

		public ResourceInfragisticsShared() : base(igwin::Infragistics.Shared.Resources.Customizer)
		{}


		// Singleton Instance
		private static readonly ResourceInfragisticsBase mManager = new ResourceInfragisticsShared();

		/// <summary>
		/// Gets the resource manager for the assembly resource file (Singleton)
		/// </summary>
		public static ResourceInfragisticsBase Manager
		{
			get { return mManager; }
		}

		#endregion

		#region -------------- Instance part ----------------------

		/// <summary>
		/// To be overridden if the resource file name differs from the used
		/// default DefaultResourceFileName (".MultiLang")
		/// </summary>
		/// <returns>the special Resource File Name</returns>
		public override string ResourceFileName()
		{
			return ".InfragisticsShared";
		}

		#endregion
	}


	/// <summary>
	/// Resource Manager for Infragistics 
	/// Sets the Localizable Strings for the entire Namespace!
	/// </summary>
	internal class ResourceInfragisticsWinExplorerBar : ResourceInfragisticsBase
	{
		#region ------------ Static part --------------------------

		public ResourceInfragisticsWinExplorerBar()
			: base(Infragistics.Win.UltraWinExplorerBar.Resources.Customizer)
		{
			// Translate the Win already
			ResourceInfragisticsWin.Manager.Translate();
		}

		// Singleton Instance
		private static readonly ResourceInfragisticsBase mManager = new ResourceInfragisticsWinExplorerBar();

		/// <summary>
		/// Gets the resource manager for the assembly resource file (Singleton)
		/// </summary>
		public static ResourceInfragisticsBase Manager
		{
			get { return mManager; }
		}

		#endregion

		#region -------------- Instance part ----------------------

		/// <summary>
		/// To be overridden if the resource file name differs from the used
		/// default DefaultResourceFileName (".MultiLang")
		/// </summary>
		/// <returns>the special Resource File Name</returns>
		public override string ResourceFileName()
		{
			return ".InfragisticsWinExplorerBar";
		}

		#endregion
	}


	/// <summary>
	/// Resource Manager for Infragistics 
	/// Sets the Localizable Strings for the entire Namespace!
	/// </summary>
	internal class ResourceInfragisticsWinMisc : ResourceInfragisticsBase
	{
		#region ------------ Static part --------------------------

		public ResourceInfragisticsWinMisc() : base(Infragistics.Win.Misc.Resources.Customizer)
		{
			// Translate the Win already
			ResourceInfragisticsWin.Manager.Translate();
		}

		// Singleton Instance
		private static readonly ResourceInfragisticsBase mManager = new ResourceInfragisticsWinMisc();

		/// <summary>
		/// Gets the resource manager for the assembly resource file (Singleton)
		/// </summary>
		public static ResourceInfragisticsBase Manager
		{
			get { return mManager; }
		}

		#endregion

		#region -------------- Instance part ----------------------

		/// <summary>
		/// To be overridden if the resource file name differs from the used
		/// default DefaultResourceFileName (".MultiLang")
		/// </summary>
		/// <returns>the special Resource File Name</returns>
		public override string ResourceFileName()
		{
			return ".InfragisticsWinMisc";
		}

		#endregion
	}



	/// <summary>
	/// Resource Manager for Infragistics 
	/// Sets the Localizable Strings for the entire Namespace!
	/// </summary>
	internal class ResourceInfragisticsWinStatusBar : ResourceInfragisticsBase
	{
		#region ------------ Static part --------------------------

		public ResourceInfragisticsWinStatusBar() : base(Infragistics.Win.UltraWinStatusBar.Resources.Customizer)
		{
			// Translate the Win already
			ResourceInfragisticsWin.Manager.Translate();
		}

		// Singleton Instance
		private static readonly ResourceInfragisticsBase mManager = new ResourceInfragisticsWinStatusBar();

		/// <summary>
		/// Gets the resource manager for the assembly resource file (Singleton)
		/// </summary>
		public static ResourceInfragisticsBase Manager
		{
			get { return mManager; }
		}

		#endregion

		#region -------------- Instance part ----------------------

		/// <summary>
		/// To be overridden if the resource file name differs from the used
		/// default DefaultResourceFileName (".MultiLang")
		/// </summary>
		/// <returns>the special Resource File Name</returns>
		public override string ResourceFileName()
		{
			return ".InfragisticsWinStatusBar";
		}

		#endregion
	}


	/// <summary>
	/// Resource Manager for Infragistics 
	/// Sets the Localizable Strings for the entire Namespace!
	/// </summary>
	internal class ResourceInfragisticsWinTabControl : ResourceInfragisticsBase
	{
		#region ------------ Static part --------------------------

		public ResourceInfragisticsWinTabControl() : base(Infragistics.Win.UltraWinTabControl.Resources.Customizer)
		{
			// Translate the Win already
			ResourceInfragisticsWin.Manager.Translate();
		}

		// Singleton Instance
		private static readonly ResourceInfragisticsBase mManager = new ResourceInfragisticsWinTabControl();

		/// <summary>
		/// Gets the resource manager for the assembly resource file (Singleton)
		/// </summary>
		public static ResourceInfragisticsBase Manager
		{
			get { return mManager; }
		}

		#endregion

		#region -------------- Instance part ----------------------

		/// <summary>
		/// To be overridden if the resource file name differs from the used
		/// default DefaultResourceFileName (".MultiLang")
		/// </summary>
		/// <returns>the special Resource File Name</returns>
		public override string ResourceFileName()
		{
			return ".InfragisticsWinTabControl";
		}

		#endregion
	}


	/// <summary>
	/// Resource Manager for Infragistics 
	/// Sets the Localizable Strings for the entire Namespace!
	/// </summary>
	internal class ResourceInfragisticsWinToolbars : ResourceInfragisticsBase
	{
		#region ------------ Static part --------------------------

		public ResourceInfragisticsWinToolbars() : base(Infragistics.Win.UltraWinToolbars.Resources.Customizer)
		{
			// Translate the Win already
			ResourceInfragisticsWin.Manager.Translate();
		}

		// Singleton Instance
		private static readonly ResourceInfragisticsBase mManager = new ResourceInfragisticsWinToolbars();

		/// <summary>
		/// Gets the resource manager for the assembly resource file (Singleton)
		/// </summary>
		public static ResourceInfragisticsBase Manager
		{
			get { return mManager; }
		}

		#endregion

		#region -------------- Instance part ----------------------

		/// <summary>
		/// To be overridden if the resource file name differs from the used
		/// default DefaultResourceFileName (".MultiLang")
		/// </summary>
		/// <returns>the special Resource File Name</returns>
		public override string ResourceFileName()
		{
			return ".InfragisticsWinToolbars";
		}

		#endregion
	}

	/// <summary>
	/// Resource Manager for Infragistics 
	/// Sets the Localizable Strings for the entire Namespace!
	/// </summary>
	internal class ResourceInfragisticsWinTree : ResourceInfragisticsBase
	{
		#region ------------ Static part --------------------------

		public ResourceInfragisticsWinTree() : base(Infragistics.Win.UltraWinTree.Resources.Customizer)
		{
			// Translate the Win already
			ResourceInfragisticsWin.Manager.Translate();
		}

		// Singleton Instance
		private static readonly ResourceInfragisticsBase mManager = new ResourceInfragisticsWinTree();

		/// <summary>
		/// Gets the resource manager for the assembly resource file (Singleton)
		/// </summary>
		public static ResourceInfragisticsBase Manager
		{
			get { return mManager; }
		}

		#endregion

		#region -------------- Instance part ----------------------

		/// <summary>
		/// To be overridden if the resource file name differs from the used
		/// default DefaultResourceFileName (".MultiLang")
		/// </summary>
		/// <returns>the special Resource File Name</returns>
		public override string ResourceFileName()
		{
			return ".InfragisticsWinTree";
		}

		#endregion
	}
#endif
}