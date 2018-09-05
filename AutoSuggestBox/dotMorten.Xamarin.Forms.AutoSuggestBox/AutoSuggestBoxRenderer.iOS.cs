#if __IOS__
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using dotMorten.Xamarin.Forms;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(AutoSuggestBox), typeof(AutoSuggestBoxRenderer))]
namespace dotMorten.Xamarin.Forms
{
    public class AutoSuggestBoxRenderer : ViewRenderer<AutoSuggestBox, NativeAutoSuggestBox>
    {
        static readonly int baseHeight = 10;

        public AutoSuggestBoxRenderer()
        {
            Frame = new RectangleF(0, 20, 320, 40);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<AutoSuggestBox> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
            {
                var nativeAutoSuggestBox = new NativeAutoSuggestBox();
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

        public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
        {
            var baseResult = base.GetDesiredSize(widthConstraint, heightConstraint);
            var testString = new Foundation.NSString("Tj");
            var testSize = testString.GetSizeUsingAttributes(new UIStringAttributes { Font = Control.Font });
            double height = baseHeight + testSize.Height;
            height = Math.Round(height);

            return new SizeRequest(new global::Xamarin.Forms.Size(baseResult.Request.Width, height));
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
