using dotMorten.Xamarin.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AutoSuggestBoxSample
{
	public partial class MainPage : ContentPage
	{
        private List<string> countries;

        public MainPage()
		{
			InitializeComponent();
            Initialize();
		}

        private void Initialize()
        {
            using (var s = typeof(MainPage).Assembly.GetManifestResourceStream("AutoSuggestBoxSample.Countries.txt"))
            {
                countries = new StreamReader(s).ReadToEnd().Split('\n').Select(t => t.Trim()).ToList();
            }
        }

        private void staticSuggestBox_TextChanged(object sender, AutoSuggestBoxTextChangedEventArgs e)
        {
            // Filter the list based on text input
            staticSuggestBox.ItemsSource = string.IsNullOrWhiteSpace(staticSuggestBox.Text) ? null : countries.Where(s => s.StartsWith(staticSuggestBox.Text, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        private async void dynamicSuggestBox_TextChanged(object sender, dotMorten.Xamarin.Forms.AutoSuggestBoxTextChangedEventArgs args)
        {
            AutoSuggestBox box = (AutoSuggestBox)sender;
            // Only get results when it was a user typing, 
            // otherwise assume the value got filled in by TextMemberPath 
            // or the handler for SuggestionChosen.
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                if (string.IsNullOrWhiteSpace(box.Text) || box.Text.Length < 3)
                    box.ItemsSource = null;
                else
                {
                    var suggestions = await GetSuggestions(box.Text);
                    box.ItemsSource = suggestions.ToList();
                }
            }
        }

        public class City
        {
            public string Name { get; set; }
            public string State { get; set; }
            public string DisplayName => $"{Name}, {State}";
            public string FullDisplayName => $"{Name}, {State}, USA";
            public override string ToString() => FullDisplayName;
        }

        /// <summary>
        /// Simulates querying a server for a large list of data
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private async Task<IEnumerable<City>> GetSuggestions(string text)
        {
            var result = await Task.Run<IEnumerable<City>>(() =>
            {
                List<City> suggestions = new List<City>();
                using (var s = typeof(MainPage).Assembly.GetManifestResourceStream("AutoSuggestBoxSample.USCities.txt"))
                {
                    using (var sr = new StreamReader(s))
                    {
                        while (!sr.EndOfStream && suggestions.Count < 20)
                        {
                            var data = sr.ReadLine().Split('\t');
                            var city = new City() { Name = data[0], State = data[1] };
                            if (city.FullDisplayName.StartsWith(text, StringComparison.InvariantCultureIgnoreCase))
                            {
                                suggestions.Add(city);
                            }
                        }
                    }
                }
                return suggestions;
            });
            return result;
        }

        private void SuggestBox_QuerySubmitted(object sender, AutoSuggestBoxQuerySubmittedEventArgs e)
        {
            if(e.ChosenSuggestion == null)
                status.Text = "Query submitted: " + e.QueryText;
            else
                status.Text = "Suggestion chosen: " + e.ChosenSuggestion;
        }
    }
}
