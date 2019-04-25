using dotMorten.Xamarin.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SampleApp.Samples.AutoSuggestBoxSamples
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [System.ComponentModel.Description("Dynamic data lookup")]
    [SamplePriority(2)]
    public partial class Dynamic : ContentPage
    {
        private Random _random;
        private CancellationTokenSource _cts;

        public Dynamic()
        {
            _random = new Random();
            InitializeComponent();
        }

        private async void SuggestBox_TextChanged(object sender, dotMorten.Xamarin.Forms.AutoSuggestBoxTextChangedEventArgs args)
        {
            AutoSuggestBox box = (AutoSuggestBox)sender;
            // Only get results when it was a user typing, 
            // otherwise assume the value got filled in by TextMemberPath 
            // or the handler for SuggestionChosen.
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                if (string.IsNullOrWhiteSpace(box.Text))
                    box.ItemsSource = null;
                else
                {
                    try
                    {
                        if (_cts != null)
                        {
                            _cts.Cancel();
                        }
                        _cts = new CancellationTokenSource();

                        var suggestions = await GetSuggestions(box.Text, _cts.Token);
                        box.ItemsSource = suggestions.ToList();
                    }
                    catch (OperationCanceledException)
                    {
                        // do nothing
                    }
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
        private async Task<IEnumerable<City>> GetSuggestions(string text, CancellationToken ct)
        {
            var result = await Task.Run<IEnumerable<City>>(() =>
            {
                List<City> suggestions = new List<City>();
                using (var s = typeof(Dynamic).Assembly.GetManifestResourceStream("SampleApp.Data.USCities.txt"))
                {
                    using (var sr = new StreamReader(s))
                    {
                        while (!sr.EndOfStream)
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
            var delay = _random.Next(100, 300);
            await Task.Delay(delay);
            ct.ThrowIfCancellationRequested();
            return result;
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
