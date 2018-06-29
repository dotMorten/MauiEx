using System;
using System.Collections.Generic;
using System.Text;

namespace dotMorten.Xamarin.Forms
{
    /// <summary>
    /// Provides data for the TextChanged event.
    /// </summary>
    public sealed class AutoSuggestBoxTextChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutoSuggestBoxTextChangedEventArgs"/> class.
        /// </summary>
        /// <param name="reason"></param>
        internal AutoSuggestBoxTextChangedEventArgs(AutoSuggestionBoxTextChangeReason reason)
        {
            Reason = reason;
        }

        /// <summary>
        /// Returns a Boolean value indicating if the current value of the TextBox is unchanged from the point in time when the TextChanged event was raised.
        /// </summary>
        /// <returns>Indicates if the current value of the TextBox is unchanged from the point in time when the TextChanged event was raised.</returns>
        public bool CheckCurrent() => true; //TODO

        /// <summary>
        /// Gets or sets a value that indicates the reason for the text changing in the AutoSuggestBox.
        /// </summary>
        /// <value>The reason for the text changing in the AutoSuggestBox.</value>
        public AutoSuggestionBoxTextChangeReason Reason { get; }
    }
}
