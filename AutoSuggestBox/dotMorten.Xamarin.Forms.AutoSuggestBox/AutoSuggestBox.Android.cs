#if __ANDROID__
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Java.Lang;

namespace dotMorten.Xamarin.Forms
{
    public partial class AutoSuggestBox
    {
        private class OnKeyListener : Java.Lang.Object, global::Android.Widget.TextView.IOnEditorActionListener
        {
            private Action _action;
            public OnKeyListener(Action onSubmit)
            {
                _action = onSubmit;
            }

            public bool OnEditorAction(TextView v, [GeneratedEnum] ImeAction actionId, KeyEvent e)
            {
                if (actionId == ImeAction.Next)
                {
                    _action?.Invoke();
                    return true;
                }
                return false;
            }
        }

        private void AutocompleteView_OnSubmit()
        {
            DismissKeyboard(NativeAutoSuggestBox);
            QuerySubmitted?.Invoke(this, new AutoSuggestBoxQuerySubmittedEventArgs(NativeAutoSuggestBox.Text, null));
        }

        private void AutocompleteTextView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var autocompleteTextView = (AutoCompleteTextView)sender;
            DismissKeyboard(autocompleteTextView);
            
            SuggestionChosen?.Invoke(this, new AutoSuggestBoxSuggestionChosenEventArgs(autocompleteTextView.Text));
            QuerySubmitted?.Invoke(this, new AutoSuggestBoxQuerySubmittedEventArgs(Text, autocompleteTextView.Text));
        }

        private void AutocompleteTextView_TextChanged(object sender, global::Android.Text.TextChangedEventArgs e)
        {
            var autocompleteTextView = (AutoCompleteTextView)sender;
            var text = e.Text.ToString();
            if (text != Text)
            {
                suppressTextChangedEvent = true;
                Text = text;
                suppressTextChangedEvent = false;
                TextChanged?.Invoke(this, new AutoSuggestBoxTextChangedEventArgs(AutoSuggestionBoxTextChangeReason.UserInput));
            }
        }

        private void DismissKeyboard(AutoCompleteTextView textView)
        {
            var imm = (Android.Views.InputMethods.InputMethodManager)NativeAutoSuggestBox.Context.GetSystemService(Context.InputMethodService);
            imm.HideSoftInputFromWindow(textView.WindowToken, 0);
        }

		private void UpdateItems()
        {
            var adapter = NativeAutoSuggestBox.Adapter as SuggestCompleteAdapter;
            Func<object,string> func = (t) =>
            {
                if (!string.IsNullOrEmpty(DisplayMemberPath))
                    return t?.GetType().GetProperty(DisplayMemberPath)?.GetValue(t)?.ToString() ?? "";
                else
                    return t?.ToString() ?? "";
            };
            if (ItemsSource == null)
                adapter.UpdateList(Enumerable.Empty<string>(), func);
			else
            {
                adapter.UpdateList(ItemsSource.OfType<object>(), func);
            }
        }

        private class SuggestCompleteAdapter : ArrayAdapter, IFilterable
        {
            private List<object> resultList;
            private Func<object, string> labelFunc;
            public SuggestCompleteAdapter(Context context, int textViewResourceId) : base(context, textViewResourceId)
            {
                resultList = new List<object>();
                SetNotifyOnChange(true);
            }
            public void UpdateList(IEnumerable<object> list, Func<object, string> labelFunc)
            {
                this.labelFunc = labelFunc;
                resultList = list.ToList();
                NotifyDataSetChanged();
            }

            public override int Count
            {
                get
                {
                    return resultList.Count;
                }
            }

            public override Filter Filter
            {
                get
                {
                    return new SuggestFilter(resultList, (s) => labelFunc(s));
                }
            }

            public override Java.Lang.Object GetItem(int position)
            {
                return resultList[position]?.ToString();
            }

            public override long GetItemId(int position)
            {
                return base.GetItemId(position);
            }

            private class SuggestFilter : Filter
            {
                private IEnumerable<object> resultList;
                FilterResults results;
                private class JObject : Java.Lang.Object { public object Obj { get; set; } }
                Func<object, string> labelFunc;
                public SuggestFilter(IEnumerable<object> list, Func<object, string> labelFunc)
                {
                    resultList = list;
                    this.labelFunc = labelFunc;
                    results = new FilterResults() { Count = resultList.Count(), Values = resultList.Select(o => new JObject() { Obj = o }).ToArray() };
                }
                protected override FilterResults PerformFiltering(ICharSequence constraint)
                {
                    return results;
                }

                protected override void PublishResults(ICharSequence constraint, FilterResults results)
                {
                }

                public override ICharSequence ConvertResultToStringFormatted(Java.Lang.Object resultValue)
                {
                    return new Java.Lang.String(labelFunc(resultValue));
                    //return base.ConvertResultToStringFormatted(resultValue);
                }
            }
        }
    }
}
#endif