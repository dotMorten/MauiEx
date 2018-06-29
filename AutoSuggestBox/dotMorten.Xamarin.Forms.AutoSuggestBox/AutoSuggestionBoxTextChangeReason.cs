using System;
using System.Collections.Generic;
using System.Text;

namespace dotMorten.Xamarin.Forms
{
    /// <summary>
    /// Provides data for the <see cref="AutoSuggestBox.TextChanged"/> event.
    /// </summary>
    public enum AutoSuggestionBoxTextChangeReason
    {
        /// <summary>The user edited the text.</summary>
        UserInput = 0,

        /// <summary>The text was changed via code.</summary>
        ProgrammaticChange = 1,

        /// <summary>The user selected one of the items in the auto-suggestion box.</summary>
        SuggestionChosen = 2
    }
}
