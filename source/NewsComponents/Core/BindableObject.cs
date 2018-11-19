using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NewsComponents.Core
{
    
    /// <summary>
    ///  Base class for objects implementing INotifyPropertyChanged
    /// </summary>
    [Serializable]
    public abstract class BindableObject : INotifyPropertyChanged
    {
        [field:NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Raises the property changed.
		/// </summary>
		/// <param name="propName">Name of the property.</param>
		/// <exception cref="System.ArgumentNullException">propName</exception>
		protected void RaisePropertyChanged([CallerMemberName] string propName = null)
        {
            if (propName == null) 
                throw new ArgumentNullException(nameof(propName));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
