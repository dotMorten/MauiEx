#if __WPF__
using System;
using System.Collections.Generic;
using System.Text;

namespace dotMorten.Xamarin.Forms
{
    internal class NativeAutoSuggestBox : WpfControls.AutoCompleteTextBox
    {
        public NativeAutoSuggestBox()
        {
        }

        public string PlaceholderText { get; set; }

        public bool IsSuggestionListOpen { get; set; }

        public bool UpdateTextOnSelect { get; set; }

        public event EventHandler<AutoSuggestBoxTextChangedEventArgs> TextChanged;

        public event EventHandler<AutoSuggestBoxQuerySubmittedEventArgs> QuerySubmitted;

        public event EventHandler<AutoSuggestBoxSuggestionChosenEventArgs> SuggestionChosen;
    }
}
#endif
