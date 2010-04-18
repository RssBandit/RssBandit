using System.Windows;
using System.Windows.Input;
using RssBandit.Resources;
using RssBandit.WinGui.Commands;
using RssBandit.WinGui.Forms;

namespace RssBandit.WinGui.ViewModel
{
    public class MainWindowViewModel : ApplicationViewModel
    {
        public MainWindowViewModel()
        {
            base.DisplayName = RssBanditApplication.CaptionOnly;
        }

        //private static readonly Images _images = new Images();

        //public Images Image
        //{
        //    get { return _images; }
        //}

        #region ThemeSelectedCommand

        RelayCommand _themeSelectedCommand;

        public ICommand ThemeSelectedCommand
        {
            get
            {
                if (_themeSelectedCommand == null)
                {
                    _themeSelectedCommand = new RelayCommand(this.SelectTheme, param => this.CanSelectTheme);
                }
                return _themeSelectedCommand;
            }
        }

        public void SelectTheme(object theme)
        {
            ((MainWindow)Application.Current.MainWindow).xamRibbon.Theme = (string)theme;
        }

        public bool CanSelectTheme
        {
            get { return true; }
        }

        #endregion
    }
}