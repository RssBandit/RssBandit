

using RssBandit.WinGui.Utility;

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
            WpfWindowSerializer.Register(this, WindowStates.All);

            Loaded += delegate
            {
                Splash.Close();
                this.Loaded -= delegate { };
            };
            
        }
        
    }
}
