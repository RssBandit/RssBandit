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
using RssBandit.Common.Logging;

namespace RssBandit.WinGui.Utility
{
	/// <summary>
	/// Wraps the System.Windows.Forms.Clipboard object to handle
	/// ExternalExceptions properly
	/// </summary>
	public static class ClipboardHelper
	{
		/// <summary>
		/// Number of times to attempt placing the data to the clipboard.
		/// </summary>
		public static int RetryTimes = 10;
		/// <summary>
		/// Number of milliseconds to pause between retries.
		/// </summary>
		public static int RetryDelay = 100;


		/// <summary>
		/// Sets the data object.
		/// </summary>
		/// <param name="data">The data.</param>
		public static void SetDataObject(object data)
		{
			InternalSetDataObject(data, false);
		}

		/// <summary>
		/// Sets the data object.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <param name="copy">if set to <c>true</c> [copy].</param>
		public static void SetDataObject(object data, bool copy)
		{
			InternalSetDataObject(data, copy);
		}

		/// <summary>
		/// Gets the data object from the clipboard.
		/// </summary>
		/// <returns></returns>
		/// <remarks>Handles carefully the ExternalException that may be caused by the
		/// framework Clipboard.GetDataObject() call.</remarks>
		public static IDataObject GetDataObject()
		{
			IDataObject dataObject = null;

			if (RetryTimes < 0 || RetryDelay < 0)
				throw new InvalidOperationException("RetryTimes and RetryDelay must be greater/equal to 0 (zero).");
			try
			{

				int attempt = RetryTimes;
				Exception catched;
				do
				{
					try
					{
						dataObject = Clipboard.GetDataObject();
						// if we get here, the setdataobject call worked, which means we can exit the do loop
						catched = null;
						break;
					}
					catch (System.Runtime.InteropServices.ExternalException ex)
					{
						catched = ex;
						System.Threading.Thread.Sleep(RetryDelay);
						--attempt;

					}

				} while (attempt != 0);

				// unknown ErrorCode or we have exceeded our attempts, so let the outer try/catch deal with it.
				if (catched != null)
					throw catched;



			}
			catch (Exception ex)
			{
				Log.Error("Unhandled clipboard exception: " + ex.Message, ex);
			}
			return dataObject;
		}

		/// <summary>
		/// Sets the string as UnicodeText data.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <param name="copy">if set to <c>true</c> [copy].</param>
		public static void SetString(string data, bool copy)
		{
			IDataObject o = new DataObject();
			o.SetData(DataFormats.UnicodeText, data);
			InternalSetDataObject(o, true);
		}
		
		/// <summary>
		/// Sets the string as Html data.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <param name="copy">if set to <c>true</c> [copy].</param>
		public static void SetHtml(string data, bool copy)
		{
			IDataObject o = new DataObject();
			o.SetData(DataFormats.Html, data);
			InternalSetDataObject(o, true);
		}
		/// <summary>
		/// Sets the string and Html data at once.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <param name="html">The HTML.</param>
		/// <param name="copy">if set to <c>true</c> [copy].</param>
		public static void SetStringAndHtml(string data, string html, bool copy)
		{
			IDataObject o = new DataObject();
            o.SetData(DataFormats.Text, data); 
			o.SetData(DataFormats.UnicodeText, data);
			o.SetData(DataFormats.Html, html);
			InternalSetDataObject(o, true);
		}

		/// <summary>
		/// Sets the data.
		/// </summary>
		/// <param name="format">The format.</param>
		/// <param name="data">The data.</param>
		public static void SetData(string format, object data)
		{
			IDataObject o = new DataObject();
			o.SetData(format, data);
			InternalSetDataObject(o, true);
		}

		private static void InternalSetDataObject(object data, bool copy)
		{
			if (RetryTimes < 0 || RetryDelay < 0)
				throw new InvalidOperationException("RetryTimes and RetryDelay must be greater/equal to 0 (zero).");
			try
			{
				Clipboard.SetDataObject(data, copy, RetryTimes, RetryDelay);
			}
			catch (Exception ex)
			{
				Log.Error("Unhandled clipboard exception: " + ex.Message, ex);
			}
		}

	}
}
