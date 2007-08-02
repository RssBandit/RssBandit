using System;

namespace RssBandit.UIServices
{
	/// <summary>
	/// PluginBase.
	/// To have a chance to apply some code restrictions/security.
	/// </summary>
	public class AddInBase: MarshalByRefObject, IDisposable
	{
		private bool _disposed = false;

		public AddInBase(){}

		public override object InitializeLifetimeService() 
		{
			//never ending Lease:
			return null;
		}

		#region IDisposable Members

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion
		protected virtual void Dispose(bool disposing) {

			if (!this._disposed) {

				// if this is a dispose call dispose on all state you 
				// hold, and take yourself off the Finalization queue.

				if (disposing) {

					// free managed resources:

				}

				// free your own state (unmanaged objects) here:

				// flag:
				this._disposed = true;

			}

		}

		// finalizer simply calls Dispose(false)
		~AddInBase() {
			Dispose(false);
		}

	}
}
