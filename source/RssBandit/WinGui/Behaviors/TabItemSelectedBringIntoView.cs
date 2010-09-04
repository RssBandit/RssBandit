using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using Infragistics.Windows.Ribbon;
using RssBandit.Util;
using RssBandit.WinGui.ViewModel;

namespace RssBandit.WinGui.Behaviors
{
    public class TabItemSelectedBringIntoView : DependencyObject
    {

        #region RibbonTabContext

        /// <summary>
        /// RibbonTabContext Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty RibbonTabContextProperty =
            DependencyProperty.RegisterAttached("RibbonTabContext", typeof(RibbonContext), typeof(TabItemSelectedBringIntoView),
                new FrameworkPropertyMetadata(RibbonContext.Home));

        /// <summary>
        /// Gets the RibbonTabContext property. This dependency property 
        /// indicates ....
        /// </summary>
        public static RibbonContext GetRibbonTabContext(DependencyObject d)
        {
            return (RibbonContext)d.GetValue(RibbonTabContextProperty);
        }

        /// <summary>
        /// Sets the RibbonTabContext property. This dependency property 
        /// indicates ....
        /// </summary>
        public static void SetRibbonTabContext(DependencyObject d, RibbonContext value)
        {
            d.SetValue(RibbonTabContextProperty, value);
        }

        #endregion


        #region CurrentContext

        /// <summary>
        /// CurrentContext Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty CurrentContextProperty =
            DependencyProperty.RegisterAttached("CurrentContext", typeof(RibbonContext), typeof(TabItemSelectedBringIntoView),
                new FrameworkPropertyMetadata(RibbonContext.Home,
                    new PropertyChangedCallback(OnCurrentContextChanged)));

        /// <summary>
        /// Gets the CurrentContext property. This dependency property 
        /// indicates ....
        /// </summary>
        public static RibbonContext GetCurrentContext(DependencyObject d)
        {
            return (RibbonContext)d.GetValue(CurrentContextProperty);
        }

        /// <summary>
        /// Sets the CurrentContext property. This dependency property 
        /// indicates ....
        /// </summary>
        public static void SetCurrentContext(DependencyObject d, RibbonContext value)
        {
            d.SetValue(CurrentContextProperty, value);
        }

        /// <summary>
        /// Handles changes to the CurrentContext property.
        /// </summary>
        private static void OnCurrentContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RibbonContext oldCurrentContext = (RibbonContext)e.OldValue;
            RibbonContext newCurrentContext = (RibbonContext)d.GetValue(CurrentContextProperty);

            var tab = (RibbonTabItem)d;

            if (newCurrentContext == GetRibbonTabContext(d))
            {
                var ribbon = tab.FindParent<XamRibbon>();

                if (ribbon.SelectedTab == tab)
                    return;

                foreach (var tabItem in ribbon.Tabs)
                {
                    if (tabItem.ContextualTabGroup != null)
                        tabItem.Visibility = Visibility.Collapsed;

                }

                tab.BringIntoView();

                ribbon.SelectedTab = tab;
            }
        }

        #endregion

        
      
        
        #region IsSelected

        /// <summary>
        /// IsSelected Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.RegisterAttached("IsSelected", typeof(bool), typeof(TabItemSelectedBringIntoView),
                new FrameworkPropertyMetadata((bool)false,
                    new PropertyChangedCallback(OnIsSelectedChanged)));

        /// <summary>
        /// Gets the IsSelected property. This dependency property 
        /// indicates ....
        /// </summary>
        public static bool GetIsSelected(DependencyObject d)
        {
            return (bool)d.GetValue(IsSelectedProperty);
        }

        /// <summary>
        /// Sets the IsSelected property. This dependency property 
        /// indicates ....
        /// </summary>
        public static void SetIsSelected(DependencyObject d, bool value)
        {
            d.SetValue(IsSelectedProperty, value);
        }

        /// <summary>
        /// Handles changes to the IsSelected property.
        /// </summary>
        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            bool oldIsSelected = (bool)e.OldValue;
            bool newIsSelected = (bool)d.GetValue(IsSelectedProperty);

            var tab = (RibbonTabItem)d;

            if (newIsSelected)
                tab.BringIntoView();
        }

        #endregion



        
        
    }
}
