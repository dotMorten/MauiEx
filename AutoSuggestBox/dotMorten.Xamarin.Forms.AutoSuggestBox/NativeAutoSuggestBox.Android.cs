#if __ANDROID__
using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Runtime;
using Android.Views.InputMethods;
using Android.Widget;
using Java.Lang;

namespace dotMorten.Xamarin.Forms
{
    /// <summary>
    ///  Extends AutoCompleteTextView to have similar APIs and behavior to UWP's AutoSuggestBox, which greatly simplifies wrapping it
    /// </summary>
    internal class NativeAutoSuggestBox : AutoCompleteTextView
    {
        private bool suppressTextChangedEvent;
        private Func<object, string> textFunc;
        private SuggestCompleteAdapter adapter;

        public NativeAutoSuggestBox(Context context) : base(context)
        {
            SetMaxLines(1);
            Threshold = 0;
            InputType = Android.Text.InputTypes.ClassText;
            ItemClick += OnItemClick;
            Adapter = adapter = new SuggestCompleteAdapter(Context, Android.Resource.Layout.SimpleDropDownItem1Line);
        }

        public void SetItems(IEnumerable<object> items, Func<object, string> labelFunc, Func<object, string> textFunc)
        {
            this.textFunc = textFunc;
            if (items == null)
                adapter.UpdateList(Enumerable.Empty<string>(), labelFunc);
            else
                adapter.UpdateList(items.OfType<object>(), labelFunc);
        }

        public new string Text
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

        public void SetTextColor(global::Xamarin.Forms.Color color)
        {
            this.SetTextColor(global::Xamarin.Forms.Platform.Android.ColorExtensions.ToAndroid(color));
        }

        public string PlaceholderText
        {
            set => HintFormatted = new Java.Lang.String(value as string ?? "");
        }

        private Android.Graphics.Color tintBackgroundColor;

        public void SetBackgroundTintList(global::Xamarin.Forms.Color color)
        {
            tintBackgroundColor = global::Xamarin.Forms.Platform.Android.ColorExtensions.ToAndroid(color);

               if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Lollipop)
                this.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(global::Xamarin.Forms.Platform.Android.ColorExtensions.ToAndroid(color));
            else
                this.Background.SetColorFilter(global::Xamarin.Forms.Platform.Android.ColorExtensions.ToAndroid(color), Android.Graphics.PorterDuff.Mode.SrcAtop);
        }

        public Android.Graphics.Color onFocusTintBackgroundColor { get; private set; }

        public void SetOnFocusTintBackgroundColor(global::Xamarin.Forms.Color color)
        {
           onFocusTintBackgroundColor = global::Xamarin.Forms.Platform.Android.ColorExtensions.ToAndroid(color);

            this.FocusChange += Control_FocusChange;
        }

        private void Control_FocusChange(object sender, Android.Views.View.FocusChangeEventArgs e)
        {
                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Lollipop)
                    this.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(e.HasFocus != null ? onFocusTintBackgroundColor : Android.Graphics.Color.Green);
                else
                    this.Background.SetColorFilter(e.HasFocus ? onFocusTintBackgroundColor : Android.Graphics.Color.Green, Android.Graphics.PorterDuff.Mode.SrcAtop); 
        }

        public void SetPlaceHolderTextColor(global::Xamarin.Forms.Color color)
        {
            this.SetHintTextColor(global::Xamarin.Forms.Platform.Android.ColorExtensions.ToAndroid(color));
        }

        public bool IsSuggestionListOpen
        {
            set
            {
                if (value)
                    ShowDropDown();
                else
                    DismissDropDown();
            }
        }

        protected override void OnTextChanged(ICharSequence text, int start, int lengthBefore, int lengthAfter)
        {
            if (!suppressTextChangedEvent)
                this.TextChanged?.Invoke(this, new AutoSuggestBoxTextChangedEventArgs(AutoSuggestionBoxTextChangeReason.UserInput));
            base.OnTextChanged(text, start, lengthBefore, lengthAfter);
        }

        private void DismissKeyboard()
        {
            var imm = (Android.Views.InputMethods.InputMethodManager)Context.GetSystemService(Context.InputMethodService);
            imm.HideSoftInputFromWindow(WindowToken, 0);
        }

        private void OnItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            DismissKeyboard();
            var obj = adapter.GetObject(e.Position);
            suppressTextChangedEvent = true;
            base.Text = textFunc(obj);
            suppressTextChangedEvent = false;
            TextChanged?.Invoke(this, new AutoSuggestBoxTextChangedEventArgs(AutoSuggestionBoxTextChangeReason.SuggestionChosen));
            SuggestionChosen?.Invoke(this, new AutoSuggestBoxSuggestionChosenEventArgs(obj));
            QuerySubmitted?.Invoke(this, new AutoSuggestBoxQuerySubmittedEventArgs(Text, obj));
        }

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

        protected override void ReplaceText(ICharSequence text)
        {
            //Override to avoid updating textbox on itemclick. We'll do this later using TextMemberPath and raise the proper TextChanged event then
        }

        public new event EventHandler<AutoSuggestBoxTextChangedEventArgs> TextChanged;

        public event EventHandler<AutoSuggestBoxQuerySubmittedEventArgs> QuerySubmitted;

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
