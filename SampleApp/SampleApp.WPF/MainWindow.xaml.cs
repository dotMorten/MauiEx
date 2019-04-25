using dotMorten.Xamarin.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Platform.WPF;

namespace SampleApp.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            AutoSuggestBox.Init();

            Forms.Init();
            LoadApplication(new SampleApp.App());
        }
    }
}
