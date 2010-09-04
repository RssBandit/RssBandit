using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace RssBandit.WinGui.Behaviors
{
    public class SelectedItemProxy : DependencyObject
    {
        #region SelectedItemInput

        /// <summary>
        /// SelectedItemInput Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty SelectedItemInputProperty =
            DependencyProperty.RegisterAttached("SelectedItemInput", typeof(object), typeof(SelectedItemProxy),
                new FrameworkPropertyMetadata((object)null,
                    new PropertyChangedCallback(OnSelectedItemInputChanged)));

        /// <summary>
        /// Gets the SelectedItemInput property. This dependency property 
        /// indicates ....
        /// </summary>
        public static object GetSelectedItemInput(DependencyObject d)
        {
            return (object)d.GetValue(SelectedItemInputProperty);
        }

        /// <summary>
        /// Sets the SelectedItemInput property. This dependency property 
        /// indicates ....
        /// </summary>
        public static void SetSelectedItemInput(DependencyObject d, object value)
        {
            d.SetValue(SelectedItemInputProperty, value);
        }

        /// <summary>
        /// Handles changes to the SelectedItemInput property.
        /// </summary>
        private static void OnSelectedItemInputChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            object oldSelectedItemInput = (object)e.OldValue;
            object newSelectedItemInput = (object)d.GetValue(SelectedItemInputProperty);

            SetSelectedItem(d, newSelectedItemInput);
        }

        #endregion

        #region SelectedItem

        /// <summary>
        /// SelectedItem Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.RegisterAttached("SelectedItem", typeof(object), typeof(SelectedItemProxy),
                new FrameworkPropertyMetadata((object)null));

        /// <summary>
        /// Gets the SelectedItem property. This dependency property 
        /// indicates ....
        /// </summary>
        public static object GetSelectedItem(DependencyObject d)
        {
            return (object)d.GetValue(SelectedItemProperty);
        }

        /// <summary>
        /// Sets the SelectedItem property. This dependency property 
        /// indicates ....
        /// </summary>
        public static void SetSelectedItem(DependencyObject d, object value)
        {
            d.SetValue(SelectedItemProperty, value);
        }

        #endregion

        
        
    }
}
