using System.ComponentModel;

namespace SampleApp.Samples.AutoSuggestBoxSamples;

[Description("Previous search and suggested")]
[SamplePriority(4)]
public partial class PreviousAndSuggested : ContentPage
{
public PreviousAndSuggested()
{
  InitializeComponent ();
        Initialize();
    }

    private List<string> _suggestedSearches;
    private List<string> _previousSearches;

    private void Initialize()
    {
        // suggested searches based on what was typed - normally loaded from some api
        _suggestedSearches = new List<string>
        {
            "suggestion 1",
            "suggestion 2",
            "suggestion 3"
        };

        // a history of searches the user has previous executed
        _previousSearches = new List<string>
        {
            "previous search 1",
            "previous search 2",
            "previous search 3",
        };
    }

    private void SuggestBox_TextChanged(object sender, AutoSuggestBoxTextChangedEventArgs e)
    {
        var box = (AutoSuggestBox)sender;
        UpdateSuggestionItemSource(box);
    }

    private void SuggestBox1_OnFocused(object sender, FocusEventArgs e)
    {
        var box = (AutoSuggestBox)sender;
        UpdateSuggestionItemSource(box);
    }

    private void UpdateSuggestionItemSource(AutoSuggestBox box)
    {
        if (box.Text.Length >= 3)
        {
            // show suggested searches, presumably loaded from an API
            box.ItemsSource = _suggestedSearches;
            box.IsSuggestionListOpen = true;
        }
        else if (box.Text.Length == 0)
        {
            // show the user's previous searches when they haven't typed anything yet
            box.ItemsSource = _previousSearches;
            box.IsSuggestionListOpen = true;
        }
        else
        {
            box.IsSuggestionListOpen = false;
            box.ItemsSource = null;
        }
    }

    private void SuggestBox_QuerySubmitted(object sender, AutoSuggestBoxQuerySubmittedEventArgs e)
    {
        SuggestBox1.Unfocus();
    }
}