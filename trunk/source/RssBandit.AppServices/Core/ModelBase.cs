using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using RssBandit.Util;

namespace RssBandit
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