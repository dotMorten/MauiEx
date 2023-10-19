using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using dotMorten.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;


#if __ANDROID__
using XAutoSuggestBoxSuggestionChosenEventArgs = dotMorten.Maui.AutoSuggestBoxSuggestionChosenEventArgs;
using XAutoSuggestBoxTextChangedEventArgs = dotMorten.Maui.AutoSuggestBoxTextChangedEventArgs;
using XAutoSuggestBoxQuerySubmittedEventArgs = dotMorten.Maui.AutoSuggestBoxQuerySubmittedEventArgs;
using NativeAutoSuggestBox = dotMorten.Maui.Platform.Android.AndroidAutoSuggestBox;
#elif __IOS__
using UIKit;
using XAutoSuggestBoxSuggestionChosenEventArgs = dotMorten.Maui.AutoSuggestBoxSuggestionChosenEventArgs;
using XAutoSuggestBoxTextChangedEventArgs = dotMorten.Maui.AutoSuggestBoxTextChangedEventArgs;
using XAutoSuggestBoxQuerySubmittedEventArgs = dotMorten.Maui.AutoSuggestBoxQuerySubmittedEventArgs;
using NativeAutoSuggestBox = dotMorten.Maui.Platform.iOS.iOSAutoSuggestBox;
#elif WINDOWS
using Microsoft.UI.Xaml.Media;
using NativeAutoSuggestBox = Microsoft.UI.Xaml.Controls.AutoSuggestBox;
using XAutoSuggestBoxSuggestionChosenEventArgs = Microsoft.UI.Xaml.Controls.AutoSuggestBoxSuggestionChosenEventArgs;
using XAutoSuggestBoxTextChangedEventArgs = Microsoft.UI.Xaml.Controls.AutoSuggestBoxTextChangedEventArgs;
using XAutoSuggestBoxQuerySubmittedEventArgs = Microsoft.UI.Xaml.Controls.AutoSuggestBoxQuerySubmittedEventArgs;
#endif

namespace dotMorten.Maui.Handlers;

/// <summary>
/// Platform specific renderer for the <see cref="AutoSuggestBox"/>
/// </summary>
public class AutoSuggestBoxHandler : ViewHandler<IAutoSuggestBox, NativeAutoSuggestBox>
{
    /// <summary>
    /// Property mapper for the <see cref="AutoSuggestBox"/> control.
    /// </summary>
    public static PropertyMapper<IAutoSuggestBox, AutoSuggestBoxHandler> AutoSuggestBoxMapper = new PropertyMapper<IAutoSuggestBox, AutoSuggestBoxHandler>(ViewHandler.ViewMapper)
    {
        [nameof(IAutoSuggestBox.Text)] = MapText,
        [nameof(IAutoSuggestBox.TextColor)] = MapTextColor,
        [nameof(IAutoSuggestBox.PlaceholderText)] = MapPlaceholderText,
        [nameof(IAutoSuggestBox.PlaceholderTextColor)] = MapPlaceholderTextColor,
        [nameof(IAutoSuggestBox.TextMemberPath)] = MapTextMemberPath,
        [nameof(IAutoSuggestBox.DisplayMemberPath)] = MapDisplayMemberPath,
        [nameof(IAutoSuggestBox.IsSuggestionListOpen)] = MapIsSuggestionListOpen,
        [nameof(IAutoSuggestBox.UpdateTextOnSelect)] = MapUpdateTextOnSelect,
        [nameof(IAutoSuggestBox.ItemsSource)] = MapItemsSource,
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="AutoSuggestBoxHandler"/>
    /// </summary>
    public AutoSuggestBoxHandler() : this(AutoSuggestBoxMapper)
    {
    }

    /// <summary>
    /// Instantiates a new instance of the <see cref="AutoSuggestBoxHandler"/> class.
    /// </summary>
    /// <param name="mapper">property mapper</param>
    public AutoSuggestBoxHandler(PropertyMapper? mapper = null) : base(mapper ?? AutoSuggestBoxMapper)
    {
    }

    /// <inheritdoc/>
    protected override void ConnectHandler(NativeAutoSuggestBox platformView)
    {
        base.ConnectHandler(platformView);
        platformView.SuggestionChosen += AutoSuggestBox_SuggestionChosen;
        platformView.TextChanged += AutoSuggestBox_TextChanged;
        platformView.QuerySubmitted += AutoSuggestBox_QuerySubmitted;
#if __IOS__
        platformView.EditingDidBegin += Control_EditingDidBegin;
        platformView.EditingDidEnd += Control_EditingDidEnd;
#elif WINDOWS
        platformView.Loaded += PlatformView_Loaded;
        platformView.GotFocus += Control_GotFocus;
#endif
    }

    /// <inheritdoc/>       
    protected override void DisconnectHandler(NativeAutoSuggestBox platformView)
    {
        base.DisconnectHandler(platformView);
        platformView.SuggestionChosen -= AutoSuggestBox_SuggestionChosen;
        platformView.TextChanged -= AutoSuggestBox_TextChanged;
        platformView.QuerySubmitted -= AutoSuggestBox_QuerySubmitted;
#if __IOS__
        platformView.EditingDidBegin -= Control_EditingDidBegin;
        platformView.EditingDidEnd -= Control_EditingDidEnd;
#elif WINDOWS
        platformView.Loaded -= PlatformView_Loaded;
        platformView.GotFocus -= Control_GotFocus;
#endif
    }

#if WINDOWS
    private void PlatformView_Loaded(object? sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Workaround issue in WinUI where the list doesn't open if you set before load
        if(VirtualView.IsSuggestionListOpen && sender is NativeAutoSuggestBox box)
            box.IsSuggestionListOpen = true;
    }
#endif

#if __IOS__
    private void Control_EditingDidBegin(object? sender, EventArgs e)
    {
        (VirtualView as VisualElement)?.SetValue(VisualElement.IsFocusedPropertyKey, true);
    }
    private void Control_EditingDidEnd(object? sender, EventArgs e)
    {
        (VirtualView as VisualElement)?.SetValue(VisualElement.IsFocusedPropertyKey, false);
    }
#elif WINDOWS
    private void Control_GotFocus(object? sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (VirtualView?.ItemsSource?.Count > 0 && sender is NativeAutoSuggestBox box)
                box.IsSuggestionListOpen = true;
        }
#endif

    private void AutoSuggestBox_QuerySubmitted(object? sender, XAutoSuggestBoxQuerySubmittedEventArgs e) => VirtualView.QuerySubmitted(e.QueryText, e.ChosenSuggestion);

    private void AutoSuggestBox_TextChanged(object? sender, XAutoSuggestBoxTextChangedEventArgs e) => VirtualView.TextChanged(PlatformView.Text, (AutoSuggestionBoxTextChangeReason)e.Reason);

    private void AutoSuggestBox_SuggestionChosen(object? sender, XAutoSuggestBoxSuggestionChosenEventArgs e) => VirtualView.SuggestionChosen(e.SelectedItem);

    /// <inheritdoc />
    protected override NativeAutoSuggestBox CreatePlatformView()
    {
#if __ANDROID__
        return new NativeAutoSuggestBox(this.Context);
#elif __IOS__ || WINDOWS
        return new NativeAutoSuggestBox();
#else
        throw new NotImplementedException();
#endif
    }

    /// <summary>
    /// Maps the <see cref="IAutoSuggestBox.Text"/> property to the native AutoSuggestBox control.
    /// </summary>
    /// <param name="handler">View handler</param>
    /// <param name="autoSuggestBox">IAutoSuggestBox instance</param>
    public static void MapText(AutoSuggestBoxHandler handler, IAutoSuggestBox autoSuggestBox)
    {
#if WINDOWS || __ANDROID__ || __IOS__
        if (handler.PlatformView.Text != autoSuggestBox.Text)
            handler.PlatformView.Text = autoSuggestBox.Text;
#endif
    }

    /// <summary>
    /// Maps the <see cref="IAutoSuggestBox.TextColor"/> property to the native AutoSuggestBox control.
    /// </summary>
    /// <param name="handler">View handler</param>
    /// <param name="autoSuggestBox">IAutoSuggestBox instance</param>
    public static void MapTextColor(AutoSuggestBoxHandler handler, IAutoSuggestBox autoSuggestBox)
    {
#if WINDOWS
        handler.PlatformView.Foreground = autoSuggestBox.TextColor.ToPlatform();
#elif __ANDROID__ || __IOS__
        handler.PlatformView.SetTextColor(autoSuggestBox.TextColor);
#endif
    }

    /// <summary>
    /// Maps the <see cref="IAutoSuggestBox.PlaceholderTextColor"/> property to the native AutoSuggestBox control.
    /// </summary>
    /// <param name="handler">View handler</param>
    /// <param name="autoSuggestBox">IAutoSuggestBox instance</param>
    public static void MapPlaceholderTextColor(AutoSuggestBoxHandler handler, IAutoSuggestBox autoSuggestBox)
    {
        var color = autoSuggestBox.PlaceholderTextColor;
#if WINDOWS
            // Not currently supported by WinUI control
            // UpdateColor(placeholderColor, ref _placeholderDefaultBrush,
            //     () => PlatformView.PlaceholderForegroundBrush, brush => Control.PlaceholderForegroundBrush = brush);
            // UpdateColor(placeholderColor, ref _defaultPlaceholderColorFocusBrush,
            //     () => PlatformView.PlaceholderForegroundFocusBrush, brush => Control.PlaceholderForegroundFocusBrush = brush);
#elif __ANDROID__ || __IOS__
        handler.PlatformView.SetPlaceholderTextColor(color);
#endif
    }

    /// <summary>
    /// Maps the <see cref="IAutoSuggestBox.PlaceholderText"/> property to the native AutoSuggestBox control.
    /// </summary>
    /// <param name="handler">View handler</param>
    /// <param name="autoSuggestBox">IAutoSuggestBox instance</param>
    public static void MapPlaceholderText(AutoSuggestBoxHandler handler, IAutoSuggestBox autoSuggestBox)
    {
#if WINDOWS || __ANDROID__ || __IOS__
        handler.PlatformView.PlaceholderText = autoSuggestBox.PlaceholderText;
#endif
    }

    /// <summary>
    /// Maps the <see cref="IAutoSuggestBox.TextMemberPath"/> property to the native AutoSuggestBox control.
    /// </summary>
    /// <param name="handler">View handler</param>
    /// <param name="autoSuggestBox">IAutoSuggestBox instance</param>
    public static void MapTextMemberPath(AutoSuggestBoxHandler handler, IAutoSuggestBox autoSuggestBox)
    {
#if WINDOWS //|| __ANDROID__ || __IOS__
            handler.PlatformView.TextMemberPath = autoSuggestBox.TextMemberPath;
#endif
    }
    /// <summary>
    /// Maps the <see cref="IAutoSuggestBox.DisplayMemberPath"/> property to the native AutoSuggestBox control.
    /// </summary>
    /// <param name="handler">View handler</param>
    /// <param name="autoSuggestBox">IAutoSuggestBox instance</param>
    public static void MapDisplayMemberPath(AutoSuggestBoxHandler handler, IAutoSuggestBox autoSuggestBox)
    {
#if WINDOWS
            handler.PlatformView.DisplayMemberPath = autoSuggestBox.DisplayMemberPath;
#elif __ANDROID__ || __IOS__
        handler.PlatformView.SetItems(autoSuggestBox.ItemsSource?.OfType<object>(), (o) => FormatType(o, autoSuggestBox.DisplayMemberPath), (o) => FormatType(o, autoSuggestBox.TextMemberPath));
#endif
    }

    /// <summary>
    /// Maps the <see cref="IView.IsEnabled"/> property to the native AutoSuggestBox control.
    /// </summary>
    /// <param name="handler">View handler</param>
    /// <param name="autoSuggestBox">IAutoSuggestBox instance</param>
    public static void MapIsEnabled(AutoSuggestBoxHandler handler, IAutoSuggestBox autoSuggestBox)
    {
#if WINDOWS
        handler.PlatformView.IsEnabled = autoSuggestBox.IsEnabled;
#elif __ANDROID__
        handler.PlatformView.Enabled = autoSuggestBox.IsEnabled;
#elif __IOS__
        handler.PlatformView.UserInteractionEnabled = autoSuggestBox.IsEnabled;
#endif
    }

    /// <summary>
    /// Maps the <see cref="IAutoSuggestBox.UpdateTextOnSelect"/> property to the native AutoSuggestBox control.
    /// </summary>
    /// <param name="handler">View handler</param>
    /// <param name="autoSuggestBox">IAutoSuggestBox instance</param>
    public static void MapUpdateTextOnSelect(AutoSuggestBoxHandler handler, IAutoSuggestBox autoSuggestBox)
    {
#if WINDOWS || __ANDROID__ || __IOS__
        handler.PlatformView.UpdateTextOnSelect = autoSuggestBox.UpdateTextOnSelect;
#endif
    }

    /// <summary>
    /// Maps the <see cref="IAutoSuggestBox.IsSuggestionListOpen"/> property to the native AutoSuggestBox control.
    /// </summary>
    /// <param name="handler">View handler</param>
    /// <param name="autoSuggestBox">IAutoSuggestBox instance</param>
    public static void MapIsSuggestionListOpen(AutoSuggestBoxHandler handler, IAutoSuggestBox autoSuggestBox)
    {
#if WINDOWS || __ANDROID__ || __IOS__
#if WINDOWS
        if (handler.PlatformView.IsLoaded) // Delay until load
#endif
            handler.PlatformView.IsSuggestionListOpen = autoSuggestBox.IsSuggestionListOpen;
#endif
    }

    /// <summary>
    /// Maps the <see cref="IAutoSuggestBox.ItemsSource"/> property to the native AutoSuggestBox control.
    /// </summary>
    /// <param name="handler">View handler</param>
    /// <param name="autoSuggestBox">IAutoSuggestBox instance</param>
    public static void MapItemsSource(AutoSuggestBoxHandler handler, IAutoSuggestBox autoSuggestBox)
    {
#if WINDOWS
            handler.PlatformView.ItemsSource = autoSuggestBox?.ItemsSource;
#elif __ANDROID__ || __IOS__
        handler.PlatformView.SetItems(autoSuggestBox?.ItemsSource?.OfType<object>(), (o) => FormatType(o, autoSuggestBox?.DisplayMemberPath), (o) => FormatType(o, autoSuggestBox?.TextMemberPath));
#endif
    }

#if __ANDROID__ || __IOS__
    private static string FormatType(object instance, string? memberPath)
    {
        if (!string.IsNullOrEmpty(memberPath))
            return instance?.GetType().GetProperty(memberPath)?.GetValue(instance)?.ToString() ?? "";
        else
            return instance?.ToString() ?? "";
    }
#endif
}