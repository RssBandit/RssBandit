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

namespace RssBandit.AppServices.Core
{
    [Serializable]
    public abstract class ModelBase : INotifyPropertyChanged
    {
        protected void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, args);
        }

        protected void OnPropertyChanged(Expression<Func<object>> expression)
        {
            PropertyChanged.Notify(expression);
        }

        [field : NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
    }
}