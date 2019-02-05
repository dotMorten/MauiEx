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

namespace AutoSuggestBoxSample.AutoSuggestBoxSamples
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
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
#if __IOS__
        protected override dotMorten.Xamarin.Forms.Platform.NativeAutoSuggestBox CreateNativeControl()
        {
            var control = base.CreateNativeControl();
            //Override the border style for iOS:
            control.InputTextField.BorderStyle = UIKit.UITextBorderStyle.RoundedRect;
            return control;
        }
#endif
    }
#endif
}