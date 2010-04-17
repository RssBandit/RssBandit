

namespace RssBandit.WinGui.Forms
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow 
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += delegate
            {
                Splash.Close();
                this.Loaded -= delegate { };
            };
            
        }
        
    }
}
