using dotMorten.Xamarin.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SampleApp.Samples.AutoSuggestBoxSamples
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
    [System.ComponentModel.Description("Email completion")]
    public partial class Email : ContentPage
	{
        private static string[] AutoCompleteEmailNames = new[] { "aol.com", "gmail.com", "hotmail.com", "icloud.com", "outlook.com", "mail.com", "yahoo.com" };

        public Email()
		{
			InitializeComponent();
		}

        private void SuggestBox_TextChanged(object sender, dotMorten.Xamarin.Forms.AutoSuggestBoxTextChangedEventArgs e)
        {
            AutoSuggestBox box = (AutoSuggestBox)sender;
            var text = box.Text;
            if(string.IsNullOrEmpty(text))
            {
                box.ItemsSource = null;
                return;
            }
            var suggestions = AutoCompleteEmailNames.Select(t => text + "@" + AutoCompleteEmailNames);
            string domain = "";
            int idx = text.IndexOf("@");
            if (idx > -1)
            {
                domain = text.Substring(idx + 1).ToLower();
                text = text.Substring(0, idx);
            }
            box.ItemsSource = AutoCompleteEmailNames.Where(s => s.StartsWith(domain)).Select(d => text + "@" + d).ToList();
        }

        private void SuggestBox_QuerySubmitted(object sender, AutoSuggestBoxQuerySubmittedEventArgs e)
        {
            if (e.ChosenSuggestion == null)
                status.Text = "You entered an email (no auto-complete): " + e.QueryText;
            else
                status.Text = "You auto-completed an email: " + e.ChosenSuggestion;
        }
    }
}