using dotMorten.Xamarin.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AutoSuggestBoxSample.AutoSuggestBoxSamples
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Simple : ContentPage
	{
		public Simple ()
		{
			InitializeComponent ();
            Initialize();
        }

        private List<string> countries;

        private void Initialize()
        {
            using (var s = typeof(Simple).Assembly.GetManifestResourceStream("AutoSuggestBoxSample.Data.Countries.txt"))
            {
                countries = new StreamReader(s).ReadToEnd().Split('\n').Select(t => t.Trim()).ToList();
            }
        }

        private void SuggestBox_TextChanged(object sender, AutoSuggestBoxTextChangedEventArgs e)
        {
            AutoSuggestBox box = (AutoSuggestBox)sender;
            // Filter the list based on text input
            box.ItemsSource = string.IsNullOrWhiteSpace(box.Text) ? null : countries.Where(s => s.StartsWith(box.Text, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        private void SuggestBox_QuerySubmitted(object sender, AutoSuggestBoxQuerySubmittedEventArgs e)
        {
            if (e.ChosenSuggestion == null)
                status.Text = "Query submitted: " + e.QueryText;
            else
                status.Text = "Suggestion chosen: " + e.ChosenSuggestion;
        }
    }
}