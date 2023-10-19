namespace SampleApp.Samples.AutoSuggestBoxSamples;

[System.ComponentModel.Description("Email completion")]
[SamplePriority(3)]
public partial class Email : ContentPage
{
    private static string[] AutoCompleteEmailNames = new[] { "aol.com", "gmail.com", "hotmail.com", "icloud.com", "outlook.com", "mail.com", "yahoo.com" };

    public Email()
    {
        InitializeComponent();
    }

    private void SuggestBox_TextChanged(object sender, dotMorten.MauiEx.AutoSuggestBoxTextChangedEventArgs e)
    {
        AutoSuggestBox box = (AutoSuggestBox)sender;
        var text = box.Text;
        if (string.IsNullOrEmpty(text))
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