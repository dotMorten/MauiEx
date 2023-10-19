using System.ComponentModel;

namespace SampleApp.Samples.AutoSuggestBoxSamples;

[Description("Test using control in a long list of items")]
[SamplePriority(5)]
public partial class ScrollViewList : ContentPage
{
    public ScrollViewList()
    {
        InitializeComponent();
        Initialize();
    }

    private List<string> countries;

    private void Initialize()
    {
        using (var s = typeof(Simple).Assembly.GetManifestResourceStream("SampleApp.Data.Countries.txt"))
        {
            countries = new StreamReader(s).ReadToEnd().Split('\n').Select(t => t.Trim()).ToList();
        }
    }

    private void SuggestBox_TextChanged(object sender, AutoSuggestBoxTextChangedEventArgs e)
    {
        AutoSuggestBox box = (AutoSuggestBox)sender;
        // Filter the list based on text input
        box.ItemsSource = GetSuggestions(box.Text);
    }

    private List<string> GetSuggestions(string text)
    {
        return string.IsNullOrWhiteSpace(text) ? null : countries.Where(s => s.StartsWith(text, StringComparison.InvariantCultureIgnoreCase)).ToList();
    }

    private void SuggestBox_QuerySubmitted(object sender, AutoSuggestBoxQuerySubmittedEventArgs e)
    {
        if (e.ChosenSuggestion == null)
            status.Text = "Query submitted: " + e.QueryText;
        else
            status.Text = "Suggestion chosen: " + e.ChosenSuggestion;
    }

    private void SuggestBox_Focused(object sender, FocusEventArgs e)
    {
        //SuggestBox1.IsSuggestionListOpen = true;
    }
}