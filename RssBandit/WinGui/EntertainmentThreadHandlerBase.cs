#region CVS Version Header
/*
 * $Id: EntertainmentThreadHandlerBase.cs,v 1.1 2005/04/08 15:00:20 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/04/08 15:00:20 $
 * $Revision: 1.1 $
 */
#endregion

using System;
using System.Threading;
using System.Windows.Forms;
using RssBandit.WinGui.Forms;

namespace RssBandit.WinGui
{
	/// <summary>
	/// EntertainmentThreadHandlerBase used to display a waiting dialog (entertainment)
	/// for a longer running task in a separate thread.
	/// </summary>
	public abstract class EntertainmentThreadHandlerBase
	{
		protected AutoResetEvent p_workDone = new AutoResetEvent(false);
		protected Exception p_operationException = null;
		protected EntertainmentDialog p_waitDialog = null;
		protected Thread p_operationThread = null;
		protected TimeSpan p_operationTimeout = TimeSpan.Zero;	// no timeout
		protected string p_operationMessage = null;

		/// <summary>
		/// Initializer.
		/// </summary>
		public EntertainmentThreadHandlerBase()	{}
		
		/// <summary>
		/// Starts the task impl. within the abstract Run() method in a separate thread.
		/// </summary>
		/// <param name="owner">IWin32Window. Owner of the displayed entertainment dialog</param>
		/// <returns>DialogResult. 
		/// DialogResult.Cancel, if the user cancelled the operation by closing the dialog.
		/// DialogResult.Abort, if the operation time out.
		/// DialogResult.OK, else.
		/// </returns>
		/// <remarks>This call allow the user to cancel the operation by closing the entertainment dialog.
		/// If you don't want to allow that, call another overloaded Start() method</remarks>
		/// <exception cref="ArgumentNullException">If owner is null</exception>
		public DialogResult Start(IWin32Window owner) {
			return this.Start(owner, this.p_operationMessage);
		}

		/// <summary>
		/// Starts the task impl. within the abstract Run() method in a separate thread.
		/// </summary>
		/// <param name="owner">IWin32Window. Owner of the displayed entertainment dialog</param>
		/// <param name="waitMessage">String. Message, to display while entertained.</param>
		/// <returns>DialogResult. 
		/// DialogResult.Cancel, if the user cancelled the operation by closing the dialog.
		/// DialogResult.Abort, if the operation time out.
		/// DialogResult.OK, else.
		/// </returns>
		/// <remarks>This call allow the user to cancel the operation by closing the entertainment dialog.
		/// If you don't want to allow that, call another overloaded Start() method</remarks>
		/// <exception cref="ArgumentNullException">If owner is null</exception>
		public DialogResult Start(IWin32Window owner, string waitMessage) {
			return this.Start(owner, waitMessage, true);
		}

		/// <summary>
		/// Starts the task impl. within the abstract Run() method in a separate thread.
		/// </summary>
		/// <param name="owner">IWin32Window. Owner of the displayed entertainment dialog</param>
		/// <param name="waitMessage">String. Message, to display while entertained.</param>
		/// <param name="allowCancel">Boolean. Set to false to prevent user from cancelling the operation.</param>
		/// <returns>DialogResult. 
		/// DialogResult.Cancel, if the user cancelled the operation by closing the dialog.
		/// DialogResult.Abort, if the operation time out.
		/// DialogResult.OK, else.
		/// </returns>
		/// <exception cref="ArgumentNullException">If owner is null</exception>
		public DialogResult Start(IWin32Window owner, string waitMessage, bool allowCancel) {
			if (owner == null)
				throw new ArgumentNullException("owner");

			DialogResult result = DialogResult.OK;
			p_operationThread = new Thread(new ThreadStart(this.Run));

			p_waitDialog = new EntertainmentDialog(this.p_workDone, this.p_operationTimeout);
			p_waitDialog.Message = (waitMessage != null ? waitMessage: String.Empty);
			p_waitDialog.ControlBox = allowCancel;
			
			Form f = owner as Form;
			if (f != null)
				p_waitDialog.Icon = f.Icon;
			
			p_operationThread.Start();

			result = p_waitDialog.ShowDialog(owner);

			if (result != DialogResult.OK) {	// timeout, or cancelled by user
				p_operationThread.Abort();	// reqires the inerited classes to catch ThreadAbortException !
			}

			return result;
		}

		/// <summary>
		/// Implentation required for the Thread start call
		/// </summary>
		/// <example>
		/// Here is the impl. recommendation:
		/// <code>
		/// 	try {				
		///			// long running task
		///		} catch (System.Threading.ThreadAbortException) {
		///			// eat up: op. cancelled
		///		} catch(Exception ex) {
		///			// handle them, or publish:
		///			p_operationException = ex;
		///		} finally {
		///			this.WorkDone.Set();	// signal end of operation to dismiss the dialog
		///		}
		/// </code>
		/// </example>
		abstract protected void Run();

		/// <summary>
		/// Info about the operation result
		/// </summary>
		public TimeSpan Timeout {
			get {	return this.p_operationTimeout;	}
			set { this.p_operationTimeout = value; }
		}

		/// <summary>
		/// Info about the operation result
		/// </summary>
		public bool OperationSucceeds {
			get {	return this.p_operationException == null;	}
		}

		/// <summary>
		/// Gets the operation exception, if any
		/// </summary>
		public Exception OperationException {
			get {	return this.p_operationException;	}
		}

		/// <summary>
		/// Info about the operation displayed in the dialog
		/// </summary>
		public string OperationMessage {
			get {	return this.p_operationMessage;	}
			set { this.p_operationMessage = value; }
		}

		/// <summary>
		/// Gets the AutoResetEvent
		/// </summary>
		protected AutoResetEvent WorkDone {
			get {	return p_workDone;	}
		}

	}
}
