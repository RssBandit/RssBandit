
using System.Windows.Input;

namespace RssBandit.WinGui.Commands
{
    public class BanditAppCommands
    {
        public static readonly ICommand SThemeSelectedCommand = new ThemeSelectedCommand();

        public virtual ICommand ThemeSelectedCommand
        {
            get { return SThemeSelectedCommand; }
        }
        
    }
}
