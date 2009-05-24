#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Windows.Forms;
using RssBandit.UI.Forms;

namespace RssBandit.UI
{
	internal class Startup
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			
			Application.Run(new Main());
		}

	}
}
