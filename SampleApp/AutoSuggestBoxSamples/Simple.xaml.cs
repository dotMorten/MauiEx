using System.ComponentModel;

namespace SampleApp.Samples.AutoSuggestBoxSamples;

[Description("Simple auto-completion")]
[SamplePriority(1)]
public partial class Simple : ContentPage
{
    public Simple()
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
        SuggestBox1.ItemsSource = countries;
        SuggestBox1.IsSuggestionListOpen = true;
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

        //Move focus to the next control or stop focus
        if (sender == SuggestBox1)
            SuggestBox2.Focus();
        else if (sender == SuggestBox2)
            SuggestBox2.Unfocus();
    }
}