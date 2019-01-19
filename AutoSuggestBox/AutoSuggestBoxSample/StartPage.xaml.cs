using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AutoSuggestBoxSample
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class StartPage : ContentPage
	{
		public StartPage ()
		{
			InitializeComponent ();
		}

        private void SimpleASB_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new AutoSuggestBoxSamples.Simple());
        }

        private void DynamicASB_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new AutoSuggestBoxSamples.Dynamic());
        }

        private void EmailASB_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new AutoSuggestBoxSamples.Email());
        }
    }
}