using System;
using System.Collections;

namespace WpfAutoSuggestBox
{
    public class SuggestionProvider : ISuggestionProvider
    {
        private readonly Func<string, IEnumerable> _method;

        public SuggestionProvider(Func<string, IEnumerable> method)
        {
            _method = method ?? throw new ArgumentNullException(nameof(method));
        }

        public IEnumerable GetSuggestions(string filter)
        {
            return _method(filter);
        }
    }
}
