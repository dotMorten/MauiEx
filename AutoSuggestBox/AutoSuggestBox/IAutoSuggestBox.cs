using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotMorten.MauiEx
{
    /// <summary>
    /// A set of AutoSuggestBox members used with the <see cref="Handlers.AutoSuggestBoxHandler"/> class.
    /// </summary>
    /// <remarks>
    /// This API might expand in the future, and implementing it isn't recommended.
    /// </remarks>
    public interface IAutoSuggestBox : IView
    {
        /// <inheritdoc cref="AutoSuggestBox.Text"/>
        string? Text { get; set; }

        /// <inheritdoc cref="AutoSuggestBox.TextColor"/>
        Color TextColor { get; set; }

        /// <inheritdoc cref="AutoSuggestBox.PlaceholderText"/>
        string? PlaceholderText { get; set; }

        /// <inheritdoc cref="AutoSuggestBox.PlaceholderTextColor"/>
        Color PlaceholderTextColor { get; set; }

        /// <inheritdoc cref="AutoSuggestBox.TextMemberPath"/>
        string? TextMemberPath { get; set; }

        /// <inheritdoc cref="AutoSuggestBox.DisplayMemberPath"/>
        string? DisplayMemberPath { get; set; }

        /// <inheritdoc cref="AutoSuggestBox.IsSuggestionListOpen"/>
        bool IsSuggestionListOpen { get; set; }

        /// <inheritdoc cref="AutoSuggestBox.UpdateTextOnSelect"/>
        bool UpdateTextOnSelect { get; set; }

        /// <inheritdoc cref="AutoSuggestBox.ItemsSource"/>
        System.Collections.IList? ItemsSource { get; set; }

        /// <inheritdoc cref="AutoSuggestBox.QuerySubmitted"/>
        void QuerySubmitted(string? queryText, object? chosenSuggestion);

        /// <inheritdoc cref="AutoSuggestBox.TextChanged"/>
        void TextChanged(string? text, AutoSuggestionBoxTextChangeReason reason);

        /// <inheritdoc cref="AutoSuggestBox.SuggestionChosen"/>
        void SuggestionChosen(object selectedItem);
    }
}
