#if __ANDROID__
using System.ComponentModel;
using System.Linq;
using dotMorten.Xamarin.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(AutoSuggestBox), typeof(AutoSuggestBoxRenderer))]
namespace dotMorten.Xamarin.Forms
{
    public class AutoSuggestBoxRenderer : ViewRenderer<AutoSuggestBox, NativeAutoSuggestBox>
    {
        public AutoSuggestBoxRenderer(Android.Content.Context context) : base(context) 
        { 
        }

        protected override void OnElementChanged(ElementChangedEventArgs<AutoSuggestBox> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
            {
                var nativeAutoSuggestBox = new NativeAutoSuggestBox(Context);
                SetNativeControl(nativeAutoSuggestBox);
            }

            if (e.OldElement != null)
            {
                Control.SuggestionChosen -= NativeAutoSuggestBox_SuggestionChosen;
                Control.TextChanged -= NativeAutoSuggestBox_TextChanged;
                Control.QuerySubmitted -= NativeAutoSuggestBox_QuerySubmitted;
            }

            if (e.NewElement != null)
            {
                Control.SuggestionChosen += NativeAutoSuggestBox_SuggestionChosen;
                Control.TextChanged += NativeAutoSuggestBox_TextChanged;
                Control.QuerySubmitted += NativeAutoSuggestBox_QuerySubmitted;
            }
        }

        // TODO repeat each set into above OnElementChanged() through encapsulation
        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == AutoSuggestBox.ItemsSourceProperty.PropertyName)
            {
                Control.SetItems(Element.ItemsSource?.OfType<object>(), (o) => FormatType(o, Element.DisplayMemberPath), (o) => FormatType(o, Element.TextMemberPath));
            }
            else if (e.PropertyName == AutoSuggestBox.PlaceholderTextProperty.PropertyName)
            {
                Control.PlaceholderText = Element.PlaceholderText;
            }
        }

        private static string FormatType(object instance, string memberPath)
        {
            if (!string.IsNullOrEmpty(memberPath))
                return instance?.GetType().GetProperty(memberPath)?.GetValue(instance)?.ToString() ?? "";
            else
                return instance?.ToString() ?? "";
        }

        private void NativeAutoSuggestBox_QuerySubmitted(object sender, AutoSuggestBoxQuerySubmittedEventArgs e)
        {
            Element.FireQuerySubmitted(new AutoSuggestBoxQuerySubmittedEventArgs(e.QueryText, e.ChosenSuggestion));
        }

        private void NativeAutoSuggestBox_SuggestionChosen(object sender, AutoSuggestBoxSuggestionChosenEventArgs e)
        {
            Element.FireSuggestionChosen(new AutoSuggestBoxSuggestionChosenEventArgs(e.SelectedItem));
        }

        private void NativeAutoSuggestBox_TextChanged(object sender, AutoSuggestBoxTextChangedEventArgs e)
        {
            Element.FireTextChanged(Control.Text, new AutoSuggestBoxTextChangedEventArgs(e.Reason));
        }
    }
}
#endif
