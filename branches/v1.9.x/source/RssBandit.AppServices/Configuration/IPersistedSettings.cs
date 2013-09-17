#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion


namespace RssBandit.AppServices.Configuration
{
	/// <summary>
	/// Defines the interface to a permanent/persisted settings storage impl.
	/// Permanent/persisted means: the settings must be available for multiple
	/// (sequential) running application sessions.
	/// </summary>
	public interface IPersistedSettings
	{
		/// <summary>
		/// Gets the property value or the default, if no value was found.
		/// </summary>
		/// <typeparam name="T">Property value type</typeparam>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns></returns>
		T GetProperty<T>(string propertyName, T defaultValue);

		/// <summary>
		/// Sets the property value.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="value">The value.</param>
		void SetProperty(string name, object value);
	}
}