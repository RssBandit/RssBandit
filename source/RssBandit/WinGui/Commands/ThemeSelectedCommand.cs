using System;

using System.Windows;
using System.Windows.Input;
using RssBandit.WinGui.Forms;

namespace RssBandit.WinGui.Commands
{
    public class ThemeSelectedCommand : RoutedCommand
    {
        public ThemeSelectedCommand():base("themeSelectedCommand", typeof(RssBanditApplication))
        {
        }

        #region Implementation of ICommand
        
        /// <summary>
        /// OnThemeSelected switches the theme depending upon the command parameter (string)
        /// specified in the ButtonTool that calls this method.
        /// There is a direct correlation between the ThemeName and the name
        /// specified in the "CommandParameter" property.  The theme name is case sensitive.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        public void Execute(object parameter)
        {
            ((MainWindow)Application.Current.MainWindow).xamRibbon.Theme = (string)parameter;
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <returns>
        /// true if this command can be executed; otherwise, false.
        /// </returns>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.
        ///                 </param>
        public bool CanExecute(object parameter)
        {
            return true;
        }

        #endregion
    }
}
