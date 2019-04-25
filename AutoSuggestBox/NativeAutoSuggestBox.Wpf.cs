#if __WPF__
using System;
using System.Windows;
using System.Windows.Media;

namespace dotMorten.Xamarin.Forms
{
    internal class NativeAutoSuggestBox : WpfAutoSuggestBox.AutoCompleteTextBox
    {
        public event EventHandler<AutoSuggestBoxTextChangedEventArgs> TextChanged;

        public event EventHandler<AutoSuggestBoxQuerySubmittedEventArgs> QuerySubmitted;

        public event EventHandler<AutoSuggestBoxSuggestionChosenEventArgs> SuggestionChosen;

        public new event EventHandler<RoutedEventArgs> GotFocus;

        public NativeAutoSuggestBox()
        {
            Foreground = new SolidColorBrush(Colors.Black);
        }

        public string PlaceholderText
        {
            get => Watermark;
            set => Watermark = value;
        }

        public bool IsSuggestionListOpen { get; set; }

        public bool UpdateTextOnSelect { get; set; }

        protected override void OnTextChanged()
        {
            base.OnTextChanged();

            if (IsUpdatingText)
            {
                return;
            }

            var reason = IsUpdatingText ? AutoSuggestionBoxTextChangeReason.ProgrammaticChange : AutoSuggestionBoxTextChangeReason.UserInput;
            TextChanged?.Invoke(this, new AutoSuggestBoxTextChangedEventArgs(reason));
        }

        protected override void OnItemSelected()
        {
            base.OnItemSelected();

            QuerySubmitted?.Invoke(this, new AutoSuggestBoxQuerySubmittedEventArgs(Text, SelectedItem));

            if (SelectedItem != null)
            {
                SuggestionChosen?.Invoke(this, new AutoSuggestBoxSuggestionChosenEventArgs(SelectedItem));
            }
        }

        protected override void OnGotFocus(object sender, RoutedEventArgs e)
        {
            base.OnGotFocus(sender, e);

            GotFocus?.Invoke(this, e);
        }
    }
}
#endif
