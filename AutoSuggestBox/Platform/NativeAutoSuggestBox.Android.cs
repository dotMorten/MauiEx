#if __ANDROID__
using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Java.Lang;

namespace dotMorten.Xamarin.Forms.Platform.Android
{
    /// <summary>
    ///  Extends AutoCompleteTextView to have similar APIs and behavior to UWP's AutoSuggestBox, which greatly simplifies wrapping it
    /// </summary>
    public class AndroidAutoSuggestBox : AutoCompleteTextView
    {
        private bool suppressTextChangedEvent;
        private Func<object, string> textFunc;
        private SuggestCompleteAdapter adapter;

        /// <summary>
        /// Initializes a new instance of the <see cref="AndroidAutoSuggestBox"/>.
        /// </summary>
        public AndroidAutoSuggestBox(Context context) : base(context)
        {
            SetMaxLines(1);
            Threshold = 0;
            InputType = global::Android.Text.InputTypes.TextFlagNoSuggestions | global::Android.Text.InputTypes.TextVariationVisiblePassword; //Disables text suggestions as the auto-complete view is there to do that
            ItemClick += OnItemClick;
            Adapter = adapter = new SuggestCompleteAdapter(Context, global::Android.Resource.Layout.SimpleDropDownItem1Line);
        }

        /// <inheritdoc />
        public override bool EnoughToFilter()
        {
            // Setting Threshold = 0 in the constructor does not allow the control to display suggestions when the Text property is null or empty.
            // This is by design by Android.
            // See https://stackoverflow.com/questions/2126717/android-autocompletetextview-show-suggestions-when-no-text-entered for details
            // Overriding this method to always returns true changes this behaviour.
            return true;
        }

        /// <inheritdoc />
        protected override void OnFocusChanged(bool gainFocus, [GeneratedEnum] FocusSearchDirection direction, Rect previouslyFocusedRect)
        {
            IsSuggestionListOpen = gainFocus;
            base.OnFocusChanged(gainFocus, direction, previouslyFocusedRect);
        }

        internal void SetItems(IEnumerable<object> items, Func<object, string> labelFunc, Func<object, string> textFunc)
        {
            this.textFunc = textFunc;
            if (items == null)
                adapter.UpdateList(Enumerable.Empty<string>(), labelFunc);
            else
                adapter.UpdateList(items.OfType<object>(), labelFunc);
        }

        /// <summary>
        /// Gets or sets the text displayed in the entry field
        /// </summary>
        public virtual new string Text
        {
            get => base.Text;
            set
            {
                suppressTextChangedEvent = true;
                base.Text = value;
                suppressTextChangedEvent = false;
                this.TextChanged?.Invoke(this, new AutoSuggestBoxTextChangedEventArgs(AutoSuggestionBoxTextChangeReason.ProgrammaticChange));
            }
        }

        /// <summary>
        /// Sets the text color on the entry field
        /// </summary>
        /// <param name="color"></param>
        public virtual void SetTextColor(global::Xamarin.Forms.Color color)
        {
            this.SetTextColor(global::Xamarin.Forms.Platform.Android.ColorExtensions.ToAndroid(color));
        }

        /// <summary>
        /// Gets or sets the placeholder text to be displayed in the <see cref="AutoCompleteTextView"/>
        /// </summary>
        public virtual string PlaceholderText
        {
            set => HintFormatted = new Java.Lang.String(value as string ?? "");
        }

        /// <summary>
        /// Gets or sets the color of the <see cref="PlaceholderText"/>.
        /// </summary>
        /// <param name="color">color</param>
        public virtual void SetPlaceholderTextColor(global::Xamarin.Forms.Color color)
        {
            this.SetHintTextColor(global::Xamarin.Forms.Platform.Android.ColorExtensions.ToAndroid(color));
        }

        /// <summary>
        /// Sets a Boolean value indicating whether the drop-down portion of the AutoSuggestBox is open.
        /// </summary>
        public virtual bool IsSuggestionListOpen
        {
            set
            {
                if (value)
                    ShowDropDown();
                else
                    DismissDropDown();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether items in the view will trigger an update of the editable text part of the AutoSuggestBox when clicked.
        /// </summary>
        public virtual bool UpdateTextOnSelect { get; set; } = true;

        /// <inheritdoc />
        protected override void OnTextChanged(ICharSequence text, int start, int lengthBefore, int lengthAfter)
        {
            if (!suppressTextChangedEvent)
                this.TextChanged?.Invoke(this, new AutoSuggestBoxTextChangedEventArgs(AutoSuggestionBoxTextChangeReason.UserInput));
            base.OnTextChanged(text, start, lengthBefore, lengthAfter);
        }

        private void DismissKeyboard()
        {
            var imm = (global::Android.Views.InputMethods.InputMethodManager)Context.GetSystemService(Context.InputMethodService);
            imm.HideSoftInputFromWindow(WindowToken, 0);
        }

        private void OnItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            DismissKeyboard();
            var obj = adapter.GetObject(e.Position);
            if (UpdateTextOnSelect)
            {
                suppressTextChangedEvent = true;
                base.Text = textFunc(obj);
                suppressTextChangedEvent = false;
                TextChanged?.Invoke(this, new AutoSuggestBoxTextChangedEventArgs(AutoSuggestionBoxTextChangeReason.SuggestionChosen));
            }
            SuggestionChosen?.Invoke(this, new AutoSuggestBoxSuggestionChosenEventArgs(obj));
            QuerySubmitted?.Invoke(this, new AutoSuggestBoxQuerySubmittedEventArgs(Text, obj));
        }

        /// <inheritdoc />
        public override void OnEditorAction([GeneratedEnum] ImeAction actionCode)
        {
            if (actionCode == ImeAction.Done || actionCode == ImeAction.Next)
            {
                DismissDropDown();
                DismissKeyboard();
                QuerySubmitted?.Invoke(this, new AutoSuggestBoxQuerySubmittedEventArgs(Text, null));
            }
            else
                base.OnEditorAction(actionCode);
        }

        /// <inheritdoc />
        protected override void ReplaceText(ICharSequence text)
        {
            //Override to avoid updating textbox on itemclick. We'll do this later using TextMemberPath and raise the proper TextChanged event then
        }

        /// <summary>
        /// Raised after the text content of the editable control component is updated.
        /// </summary>
        public new event EventHandler<AutoSuggestBoxTextChangedEventArgs> TextChanged;

        /// <summary>
        /// Occurs when the user submits a search query.
        /// </summary>
        public event EventHandler<AutoSuggestBoxQuerySubmittedEventArgs> QuerySubmitted;

        /// <summary>
        /// Raised before the text content of the editable control component is updated.
        /// </summary>
        public event EventHandler<AutoSuggestBoxSuggestionChosenEventArgs> SuggestionChosen;

        private class SuggestCompleteAdapter : ArrayAdapter, IFilterable
        {
            private SuggestFilter filter = new SuggestFilter();
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
                filter.SetFilter(resultList.Select(s=>labelFunc(s)));
                NotifyDataSetChanged();
            }

            public override int Count
            {
                get
                {
                    return resultList.Count;
                }
            }

            public override Filter Filter => filter;

            public override Java.Lang.Object GetItem(int position)
            {
                return labelFunc(GetObject(position));
            }

            public object GetObject(int position)
            {
                return resultList[position];
            }

            public override long GetItemId(int position)
            {
                return base.GetItemId(position);
            }

            private class SuggestFilter : Filter
            {
                private IEnumerable<string> resultList;

                public SuggestFilter()
                {
                }
                public void SetFilter(IEnumerable<string> list)
                {
                    resultList = list;
                }
                protected override FilterResults PerformFiltering(ICharSequence constraint)
                {
                    if (resultList == null)
                        return new FilterResults() { Count = 0, Values = null };
                    var arr = resultList.ToArray();
                    return new FilterResults() { Count = arr.Length, Values = arr };
                }
                protected override void PublishResults(ICharSequence constraint, FilterResults results)
                {
                }
            }
        }
    }
}
#endif