using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dotMorten.Xamarin.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.IO;
#if __IOS__
using Xamarin.Forms.Platform.iOS;
using dotMorten.Xamarin.Forms.Platform.iOS;
using NativeAutoSuggestBox = dotMorten.Xamarin.Forms.Platform.iOS.iOSAutoSuggestBox;
#elif __ANDROID__
using Xamarin.Forms.Platform.Android;
using dotMorten.Xamarin.Forms.Platform.Android;
using NativeAutoSuggestBox = dotMorten.Xamarin.Forms.Platform.Android.AndroidAutoSuggestBox;
#elif NETFX_CORE
using Xamarin.Forms.Platform.UWP;
using dotMorten.Xamarin.Forms.Platform.UWP;
using NativeAutoSuggestBox = Windows.UI.Xaml.Controls.AutoSuggestBox;
#endif

#if !NETSTANDARD2_0
[assembly: ExportRenderer(typeof(SampleApp.Samples.AutoSuggestBoxSamples.CustomAutoSuggestBox), typeof(SampleApp.Samples.AutoSuggestBoxSamples.CustomAutoSuggestBoxRenderer))]
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
            Control.InputTextField.BorderStyle = UIKit.UITextBorderStyle.RoundedRect;
            Control.ShowBottomBorder = false;
#endif
        }
    }
#endif
}