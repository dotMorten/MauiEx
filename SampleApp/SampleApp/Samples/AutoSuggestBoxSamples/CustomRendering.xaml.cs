using dotMorten.Xamarin.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;


#if !NETSTANDARD2_0
[assembly: ExportRenderer(typeof(AutoSuggestBoxSample.AutoSuggestBoxSamples.CustomAutoSuggestBox), typeof(AutoSuggestBoxSample.AutoSuggestBoxSamples.CustomAutoSuggestBoxRenderer))]
#endif

namespace SampleApp.Samples.AutoSuggestBoxSamples
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [System.ComponentModel.Description("Uses a custom renderer to override the style on iOS")]
    public partial class CustomRendering : ContentPage
    {
        public CustomRendering()
        {
            InitializeComponent();
        }
    }

    public class CustomAutoSuggestBox : AutoSuggestBox { }

#if !NETSTANDARD2_0
    public class CustomAutoSuggestBoxRenderer : dotMorten.Xamarin.Forms.Platform.AutoSuggestBoxRenderer
    {
        protected override dotMorten.Xamarin.Forms.Platform.NativeAutoSuggestBox CreateNativeControl()
        {
            var control = base.CreateNativeControl();
#if __IOS__
            //Override the border style for iOS:
            control.InputTextField.BorderStyle = UIKit.UITextBorderStyle.RoundedRect;
#endif
            return control;
        }
    }
#endif
}