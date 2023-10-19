using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dotMorten.Maui;
using System.IO;
#if __IOS__
using NativeAutoSuggestBox = dotMorten.Maui.Platform.iOS.iOSAutoSuggestBox;
#elif __ANDROID__
using Xamarin.Forms.Platform.Android;
using dotMorten.Maui.Platform.Android;
using NativeAutoSuggestBox = dotMorten.Maui.Platform.Android.AndroidAutoSuggestBox;
#elif NETFX_CORE
using Xamarin.Forms.Platform.UWP;
using dotMorten.Maui.Platform.UWP;
using NativeAutoSuggestBox = Windows.UI.Xaml.Controls.AutoSuggestBox;
#endif


namespace SampleApp.Samples.AutoSuggestBoxSamples
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [System.ComponentModel.Description("Uses a custom renderer to overrides the style on iOS")]
    public partial class CustomRendering : ContentPage
    {
        public CustomRendering()
        {
            InitializeComponent();
            List<string> countries;
            using (var s = typeof(Simple).Assembly.GetManifestResourceStream("SampleApp.Data.Countries.txt"))
            {
                countries = new StreamReader(s).ReadToEnd().Split('\n').Select(t => t.Trim()).ToList();
            }
            SuggestBox1.TextChanged += (s, e) => 
                SuggestBox1.ItemsSource = string.IsNullOrWhiteSpace(SuggestBox1.Text) ? null : 
                    countries.Where(t => t.StartsWith(SuggestBox1.Text, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }
    }

    public class CustomAutoSuggestBox : AutoSuggestBox { }

#if !NETSTANDARD2_0
    public class CustomAutoSuggestBoxRenderer : AutoSuggestBoxRenderer
    {
#if __ANDROID__
        public CustomAutoSuggestBoxRenderer(global::Android.Content.Context context) : base(context) { }
#endif
        protected override void OnElementChanged(ElementChangedEventArgs<AutoSuggestBox> e)
        {
            base.OnElementChanged(e);
#if __IOS__
            //Override the border style for iOS:
            if(Control != null)
            {
               Control.InputTextField.BorderStyle = UIKit.UITextBorderStyle.RoundedRect;
               Control.ShowBottomBorder = false;
            }
#endif
        }
    }
#endif
}