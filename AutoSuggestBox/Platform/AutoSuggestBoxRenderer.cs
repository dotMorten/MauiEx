#if !NETSTANDARD2_0
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using Xamarin.Forms;
using dotMorten.Xamarin.Forms;
#if __ANDROID__
using Xamarin.Forms.Platform.Android;
using XAutoSuggestBoxSuggestionChosenEventArgs = dotMorten.Xamarin.Forms.AutoSuggestBoxSuggestionChosenEventArgs;
using XAutoSuggestBoxTextChangedEventArgs = dotMorten.Xamarin.Forms.AutoSuggestBoxTextChangedEventArgs;
using XAutoSuggestBoxQuerySubmittedEventArgs = dotMorten.Xamarin.Forms.AutoSuggestBoxQuerySubmittedEventArgs;
using NativeAutoSuggestBox = dotMorten.Xamarin.Forms.Platform.Android.AndroidAutoSuggestBox;
#elif __IOS__
using UIKit;
using Xamarin.Forms.Platform.iOS;
using XAutoSuggestBoxSuggestionChosenEventArgs = dotMorten.Xamarin.Forms.AutoSuggestBoxSuggestionChosenEventArgs;
using XAutoSuggestBoxTextChangedEventArgs = dotMorten.Xamarin.Forms.AutoSuggestBoxTextChangedEventArgs;
using XAutoSuggestBoxQuerySubmittedEventArgs = dotMorten.Xamarin.Forms.AutoSuggestBoxQuerySubmittedEventArgs;
using NativeAutoSuggestBox = dotMorten.Xamarin.Forms.Platform.iOS.iOSAutoSuggestBox;
#elif NETFX_CORE
using Xamarin.Forms.Platform.UWP;
using Windows.UI.Xaml.Media;
using NativeAutoSuggestBox = Windows.UI.Xaml.Controls.AutoSuggestBox;
using XAutoSuggestBoxSuggestionChosenEventArgs = Windows.UI.Xaml.Controls.AutoSuggestBoxSuggestionChosenEventArgs;
using XAutoSuggestBoxTextChangedEventArgs = Windows.UI.Xaml.Controls.AutoSuggestBoxTextChangedEventArgs;
using XAutoSuggestBoxQuerySubmittedEventArgs = Windows.UI.Xaml.Controls.AutoSuggestBoxQuerySubmittedEventArgs;
#endif

#if __ANDROID__
namespace dotMorten.Xamarin.Forms.Platform.Android {
#elif __IOS__
namespace dotMorten.Xamarin.Forms.Platform.iOS {
#elif NETFX_CORE
namespace dotMorten.Xamarin.Forms.Platform.UWP {
#endif

    /// <summary>
    /// Platform specific renderer for the <see cref="AutoSuggestBox"/>
    /// </summary>
    public class AutoSuggestBoxRenderer : ViewRenderer<AutoSuggestBox, NativeAutoSuggestBox>
    {
#if !NETFX_CORE
        private bool suppressTextChangedEvent;
#endif
#if __ANDROID__
        /// <summary>
        /// Initializes a new instance of the <see cref="AutoSuggestBoxRenderer"/>
        /// </summary>
        /// <param name="context">Context</param>
        public AutoSuggestBoxRenderer(global::Android.Content.Context context) : base(context)
        {
        }
#endif

#if __IOS__
        static readonly int baseHeight = 10;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoSuggestBoxRenderer"/>
        /// </summary>
        public AutoSuggestBoxRenderer()
        {
            Frame = new RectangleF(0, 20, 320, 40);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        protected override void OnElementChanged(ElementChangedEventArgs<dotMorten.Xamarin.Forms.AutoSuggestBox> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                if (Control != null)
                {
                    Control.SuggestionChosen -= AutoSuggestBox_SuggestionChosen;
                    Control.TextChanged -= AutoSuggestBox_TextChanged;
                    Control.QuerySubmitted -= AutoSuggestBox_QuerySubmitted;
#if __IOS__
                    Control.EditingDidBegin -= Control_EditingDidBegin;
                    Control.EditingDidEnd -= Control_EditingDidEnd;
#elif NETFX_CORE
                    Control.GotFocus -= Control_GotFocus;
#endif
                }
            }

            if (e.NewElement != null)
            {
                if (Control == null)
                {
                    var box = CreateNativeControl();
                    SetNativeControl(box);
                }
                Control.Text = e.NewElement.Text ?? string.Empty;
                UpdateTextColor();
                UpdatePlaceholderText();
                UpdatePlaceholderTextColor();
                UpdateTextMemberPath();
                UpdateDisplayMemberPath();
                UpdateIsEnabled();
                Control.UpdateTextOnSelect = e.NewElement.UpdateTextOnSelect;
                Control.IsSuggestionListOpen = e.NewElement.IsSuggestionListOpen;
                UpdateItemsSource();

                Control.SuggestionChosen += AutoSuggestBox_SuggestionChosen;
                Control.TextChanged += AutoSuggestBox_TextChanged;
                Control.QuerySubmitted += AutoSuggestBox_QuerySubmitted;
#if __IOS__
                Control.EditingDidBegin += Control_EditingDidBegin;
                Control.EditingDidEnd += Control_EditingDidEnd;
#elif NETFX_CORE
                Control.GotFocus += Control_GotFocus;
#endif
            }
        }

#if __IOS__
        private void Control_EditingDidBegin(object sender, EventArgs e)
        {
            Element?.SetValue(VisualElement.IsFocusedPropertyKey, true);
        }
        private void Control_EditingDidEnd(object sender, EventArgs e)
        {
            Element?.SetValue(VisualElement.IsFocusedPropertyKey, false);
        }
#elif NETFX_CORE
        private void Control_GotFocus(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (Element?.ItemsSource?.Count > 0)
                (sender as NativeAutoSuggestBox).IsSuggestionListOpen = true;
        }
#endif

        private void AutoSuggestBox_QuerySubmitted(object sender, XAutoSuggestBoxQuerySubmittedEventArgs e)
        {
            MessagingCenter.Send(Element, "AutoSuggestBox_" + nameof(AutoSuggestBox.QuerySubmitted), (e.QueryText, e.ChosenSuggestion));
        }

        private void AutoSuggestBox_TextChanged(object sender, XAutoSuggestBoxTextChangedEventArgs e)
        {
            MessagingCenter.Send(Element, "AutoSuggestBox_" + nameof(AutoSuggestBox.TextChanged), (Control.Text, (AutoSuggestionBoxTextChangeReason)e.Reason));
        }

        private void AutoSuggestBox_SuggestionChosen(object sender, XAutoSuggestBoxSuggestionChosenEventArgs e)
        {
            MessagingCenter.Send(Element, "AutoSuggestBox_" + nameof(AutoSuggestBox.SuggestionChosen), e.SelectedItem);
        }

        /// <inheritdoc />
#if NETFX_CORE
        protected NativeAutoSuggestBox CreateNativeControl()
#else
        protected override NativeAutoSuggestBox CreateNativeControl()
#endif
        {
#if __ANDROID__
            return new AndroidAutoSuggestBox(this.Context);
#elif __IOS__
            return new iOSAutoSuggestBox();
#elif NETFX_CORE
            return new Windows.UI.Xaml.Controls.AutoSuggestBox();
#else
            throw new NotImplementedException();
#endif
        }

        /// <inheritdoc />
        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Control == null)
            {
                return;
            }
            if (e.PropertyName == nameof(AutoSuggestBox.Text))
            {
                if (Control.Text != Element.Text)
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
            else if (e.PropertyName == nameof(AutoSuggestBox.PlaceholderTextColor))
            {
                UpdatePlaceholderTextColor();
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
            else if (e.PropertyName == nameof(AutoSuggestBox.UpdateTextOnSelect))
            {
                Control.UpdateTextOnSelect = Element.UpdateTextOnSelect;
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

#if NETFX_CORE
        // Brush _placeholderDefaultBrush;
        // Brush _defaultPlaceholderColorFocusBrush;
#endif
        private void UpdatePlaceholderTextColor()
        {
            var placeholderColor = Element.PlaceholderTextColor;
#if NETFX_CORE
            // Not currently supported by UWP's control
            // UpdateColor(placeholderColor, ref _placeholderDefaultBrush,
            //     () => Control.PlaceholderForegroundBrush, brush => Control.PlaceholderForegroundBrush = brush);
            // UpdateColor(placeholderColor, ref _defaultPlaceholderColorFocusBrush,
            //     () => Control.PlaceholderForegroundFocusBrush, brush => Control.PlaceholderForegroundFocusBrush = brush);
#elif __ANDROID__ || __IOS__
            Control.SetPlaceholderTextColor(placeholderColor);
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
            Control.ItemsSource = Element?.ItemsSource;
#elif __ANDROID__ || __IOS__
            Control.SetItems(Element?.ItemsSource?.OfType<object>(), (o) => FormatType(o, Element?.DisplayMemberPath), (o) => FormatType(o, Element?.TextMemberPath));
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