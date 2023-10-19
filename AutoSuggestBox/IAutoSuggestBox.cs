using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotMorten.Maui
{
    public interface IAutoSuggestBox : IView
    {
        string Text { get; set; }
        Color TextColor { get; set; }
        string PlaceholderText { get; set; }
        Color PlaceholderTextColor { get; set; }
        string TextMemberPath { get; set; }
        string DisplayMemberPath { get; set; }
        bool IsSuggestionListOpen { get; set; }
        bool UpdateTextOnSelect { get; set; }
        System.Collections.IList ItemsSource { get; set; }
        
        void QuerySubmitted(string queryText, object chosenSuggestion);
        void TextChanged(string text, AutoSuggestionBoxTextChangeReason reason);
        void SuggestionChosen(object selectedItem);
    }
}
