using System.Collections;

namespace WpfAutoSuggestBox
{
    public interface ISuggestionProvider
    {
        IEnumerable GetSuggestions(string filter);
    }
}
