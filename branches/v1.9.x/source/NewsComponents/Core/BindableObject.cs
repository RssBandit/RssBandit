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
using System.ComponentModel;
using System.Linq.Expressions;
using JetBrains.Annotations;
using TorSteroids.Common.Extensions;

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
		protected void RaisePropertyChanged([NotNull] string propName)
        {
            if (propName == null) 
                throw new ArgumentNullException("propName");

            var evt = PropertyChanged;
            if(evt != null)
                evt(this, new PropertyChangedEventArgs(propName));
        }

		/// <summary>
		/// Recommended method to use/provide a strong typed property name instead of a plain string:
		/// Call it OnPropertyChanged(() =&gt; MyProperty);
		/// </summary>
		/// <param name="expression">The expression.</param>
		/// <exception cref="System.ArgumentNullException">expression</exception>
		protected void RaisePropertyChanged([NotNull] Expression<Func<object>> expression)
		{
			RaisePropertyChanged(expression.GetPropertyName());
		}

    }
}
