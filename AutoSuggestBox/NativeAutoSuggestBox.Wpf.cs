#if __WPF__
using System;

namespace dotMorten.Xamarin.Forms
{
    internal class NativeAutoSuggestBox : WpfAutoSuggestBox.AutoCompleteTextBox
    {
        public NativeAutoSuggestBox()
        {
        }

        public string PlaceholderText
        {
            get => Watermark;
            set => Watermark = value;
        }

        public bool IsSuggestionListOpen { get; set; }

        public bool UpdateTextOnSelect { get; set; }

        public event EventHandler<AutoSuggestBoxTextChangedEventArgs> TextChanged;

        public event EventHandler<AutoSuggestBoxQuerySubmittedEventArgs> QuerySubmitted;

        public event EventHandler<AutoSuggestBoxSuggestionChosenEventArgs> SuggestionChosen;
    }
}
#endif
