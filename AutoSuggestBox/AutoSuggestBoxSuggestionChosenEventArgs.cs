using System;
using System.Collections.Generic;
using System.Text;

namespace dotMorten.Xamarin.Forms
{
    /// <summary>
    /// Provides data for the <see cref="AutoSuggestBox.SuggestionChosen"/> event.
    /// </summary>
    public sealed class AutoSuggestBoxSuggestionChosenEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutoSuggestBoxSuggestionChosenEventArgs"/> class.
        /// </summary>
        /// <param name="selectedItem"></param>
        internal AutoSuggestBoxSuggestionChosenEventArgs(object selectedItem)
        {
            SelectedItem = selectedItem;
        }

        /// <summary>
        /// Gets a reference to the selected item.
        /// </summary>
        /// <value>A reference to the selected item.</value>
        public object SelectedItem { get; }
    }
}
