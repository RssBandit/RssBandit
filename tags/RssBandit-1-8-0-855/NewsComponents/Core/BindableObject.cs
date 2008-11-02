using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

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

        protected void RaisePropertyChanged(string propName)
        {
            if (propName == null) 
                throw new ArgumentNullException("propName");

            var evt = PropertyChanged;
            if(evt != null)
                evt(this, new PropertyChangedEventArgs(propName));
        }
    }
}
