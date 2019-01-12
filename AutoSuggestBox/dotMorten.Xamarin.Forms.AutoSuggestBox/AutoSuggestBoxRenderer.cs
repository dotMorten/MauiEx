#if !NETSTANDARD2_0
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using Xamarin.Forms;
#if __ANDROID__
using Xamarin.Forms.Platform.Android;
using XAutoSuggestBoxSuggestionChosenEventArgs = dotMorten.Xamarin.Forms.AutoSuggestBoxSuggestionChosenEventArgs;
using XAutoSuggestBoxTextChangedEventArgs = dotMorten.Xamarin.Forms.AutoSuggestBoxTextChangedEventArgs;
using XAutoSuggestBoxQuerySubmittedEventArgs = dotMorten.Xamarin.Forms.AutoSuggestBoxQuerySubmittedEventArgs;
#elif __IOS__
using UIKit;
using Xamarin.Forms.Platform.iOS;
using XAutoSuggestBoxSuggestionChosenEventArgs = dotMorten.Xamarin.Forms.AutoSuggestBoxSuggestionChosenEventArgs;
using XAutoSuggestBoxTextChangedEventArgs = dotMorten.Xamarin.Forms.AutoSuggestBoxTextChangedEventArgs;
using XAutoSuggestBoxQuerySubmittedEventArgs = dotMorten.Xamarin.Forms.AutoSuggestBoxQuerySubmittedEventArgs;
#elif NETFX_CORE
using Xamarin.Forms.Platform.UWP;
using NativeAutoSuggestBox = Windows.UI.Xaml.Controls.AutoSuggestBox;
using XAutoSuggestBoxSuggestionChosenEventArgs = Windows.UI.Xaml.Controls.AutoSuggestBoxSuggestionChosenEventArgs;
using XAutoSuggestBoxTextChangedEventArgs = Windows.UI.Xaml.Controls.AutoSuggestBoxTextChangedEventArgs;
using XAutoSuggestBoxQuerySubmittedEventArgs = Windows.UI.Xaml.Controls.AutoSuggestBoxQuerySubmittedEventArgs;
#endif

namespace dotMorten.Xamarin.Forms
{
    internal class AutoSuggestBoxRenderer : ViewRenderer<AutoSuggestBox, NativeAutoSuggestBox>
    {
        private bool suppressTextChangedEvent;
#if __ANDROID__
        public AutoSuggestBoxRenderer(Android.Content.Context context) : base(context)
        {
        }
#endif

#if __IOS__
        static readonly int baseHeight = 10;

        public AutoSuggestBoxRenderer()
        {
            Frame = new RectangleF(0, 20, 320, 40);
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
#endif

        protected override void OnElementChanged(ElementChangedEventArgs<dotMorten.Xamarin.Forms.AutoSuggestBox> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement == null)
            {
                var box = CreateNativeControl();

                box.SuggestionChosen += AutoSuggestBox_SuggestionChosen;
                box.TextChanged += AutoSuggestBox_TextChanged;
                box.QuerySubmitted += AutoSuggestBox_QuerySubmitted;
                SetNativeControl(box);
            }

            Control.Text = Element.Text;
            UpdateTextColor();
            UpdatePlaceholderText();
            UpdateTextMemberPath();
            UpdateDisplayMemberPath();
            UpdateIsEnabled();
            Control.IsSuggestionListOpen = Element.IsSuggestionListOpen;
            UpdateItemsSource();
        }

        private void AutoSuggestBox_QuerySubmitted(object sender, XAutoSuggestBoxQuerySubmittedEventArgs e)
        {
            Element.RaiseQuerySubmitted(e.QueryText, e.ChosenSuggestion);
        }

        private void AutoSuggestBox_TextChanged(object sender, XAutoSuggestBoxTextChangedEventArgs e)
        {
            Element.NativeControlTextChanged(Control.Text, (AutoSuggestionBoxTextChangeReason)e.Reason);
        }

        private void AutoSuggestBox_SuggestionChosen(object sender, XAutoSuggestBoxSuggestionChosenEventArgs e)
        {
            Element.RaiseSuggestionChosen(e.SelectedItem);
        }

#if NETFX_CORE
        protected NativeAutoSuggestBox CreateNativeControl()
#else
        protected override NativeAutoSuggestBox CreateNativeControl()
#endif
        {
#if __ANDROID__
            return new NativeAutoSuggestBox(this.Context);
#else
            return new NativeAutoSuggestBox();
#endif
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AutoSuggestBox.Text))
            {
                if(Control.Text != Element.Text)
                    Control.Text = Element.Text;
            }
            else if (e.PropertyName == nameof(AutoSuggestBox.TextColor))
            {
                UpdateTextColor();
            }
            else if (e.PropertyName == nameof(AutoSuggestBox.PlaceholderText))
            {
                UpdatePlaceholderText();
            }
            else if (e.PropertyName == nameof(AutoSuggestBox.TextMemberPath))
            {
                UpdateTextMemberPath();
            }
            else if (e.PropertyName == nameof(AutoSuggestBox.DisplayMemberPath))
            {
                UpdateDisplayMemberPath();
            }
            else if (e.PropertyName == nameof(AutoSuggestBox.IsEnabled))
            {
                UpdateIsEnabled();
            }
            else if (e.PropertyName == nameof(AutoSuggestBox.IsSuggestionListOpen))
            {
                Control.IsSuggestionListOpen = Element.IsSuggestionListOpen;
            }
            else if (e.PropertyName == nameof(AutoSuggestBox.ItemsSource))
            {
                UpdateItemsSource();
            }
            base.OnElementPropertyChanged(sender, e);
        }

        private void UpdateTextColor()
        {
            var color = Element.TextColor;
#if NETFX_CORE
            Control.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb((byte)(color.A * 255), (byte)(color.R * 255), (byte)(color.G * 255), (byte)(color.B * 255)));
#elif __ANDROID__ || __IOS__
            Control.SetTextColor(color);
#endif
        }

        private void UpdatePlaceholderText() => Control.PlaceholderText = Element.PlaceholderText;

        private void UpdateTextMemberPath()
        {
#if NETFX_CORE
            Control.TextMemberPath = Element.TextMemberPath;
#endif
        }

        private void UpdateDisplayMemberPath()
        {
#if NETFX_CORE
            Control.DisplayMemberPath = Element.DisplayMemberPath;
#elif __ANDROID__ || __IOS__
            Control.SetItems(Element.ItemsSource?.OfType<object>(), (o) => FormatType(o, Element.DisplayMemberPath), (o) => FormatType(o, Element.TextMemberPath));
#endif
        }

        private void UpdateIsEnabled()
        {
#if NETFX_CORE
            Control.IsEnabled = Element.IsEnabled;
#elif __ANDROID__
            Control.Enabled = Element.IsEnabled;
#elif __IOS__
            Control.UserInteractionEnabled = Element.IsEnabled;
#endif
        }

        private void UpdateItemsSource()
        {
#if NETFX_CORE
            Control.ItemsSource = Element.ItemsSource;
#elif __ANDROID__ || __IOS__
            Control.SetItems(Element.ItemsSource?.OfType<object>(), (o) => FormatType(o, Element.DisplayMemberPath), (o) => FormatType(o, Element.TextMemberPath));
#endif

        }










#if __ANDROID__ || __IOS__
        private static string FormatType(object instance, string memberPath)
        {
            if (!string.IsNullOrEmpty(memberPath))
                return instance?.GetType().GetProperty(memberPath)?.GetValue(instance)?.ToString() ?? "";
            else
                return instance?.ToString() ?? "";
        }
#endif
    }
}
#endif