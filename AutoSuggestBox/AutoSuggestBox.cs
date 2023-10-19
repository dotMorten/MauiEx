using System;
using System.Linq;
using System.Runtime.CompilerServices;
#if !NETSTANDARD2_0
#if __ANDROID__
#elif __IOS__
using CoreGraphics;
#elif WINDOWS
using NativeAutoSuggestBox = Microsoft.UI.Xaml.Controls.AutoSuggestBox;
#endif
#endif

namespace dotMorten.Maui
{
    /// <summary>
    /// Represents a text control that makes suggestions to users as they type. The app is notified when text 
    /// has been changed by the user and is responsible for providing relevant suggestions for this control to display.
    /// Use the UWP Reference doc for more information: <a href="https://msdn.microsoft.com/en-us/library/windows/apps/mt280217.aspx">Link</a>
    /// </summary>
	public class AutoSuggestBox : View, IAutoSuggestBox
    {
        private bool suppressTextChangedEvent;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoSuggestBox"/> class
        /// </summary>
        public AutoSuggestBox()
        {
        }

        /// <summary>
        /// Gets or sets the Text property
        /// </summary>
        /// <seealso cref="TextColor"/>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Text"/> bindable property.
        /// </summary>
        public static readonly BindableProperty TextProperty =
            BindableProperty.Create(nameof(Text), typeof(string), typeof(AutoSuggestBox), "", BindingMode.OneWay, null, OnTextPropertyChanged);

        private static void OnTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var box = (AutoSuggestBox)bindable;
            if (!box.suppressTextChangedEvent) //Ensure this property changed didn't get call because we were updating it from the native text property
                box.TextChanged?.Invoke(box, new AutoSuggestBoxTextChangedEventArgs(AutoSuggestionBoxTextChangeReason.ProgrammaticChange));
        }

        /// <summary>
        /// Gets or sets the foreground color of the control
        /// </summary>
        /// <seealso cref="Text"/>
        public Color TextColor
        {
            get { return (Color)GetValue(TextColorProperty); }
            set { SetValue(TextColorProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="TextColor"/> bindable property.
        /// </summary>
        public static readonly BindableProperty TextColorProperty =
            BindableProperty.Create(nameof(TextColor), typeof(Color ), typeof(AutoSuggestBox), Colors.Gray, BindingMode.OneWay, null, null);

        /// <summary>
        /// Gets or sets the PlaceholderText
        /// </summary>
        /// <seealso cref="PlaceholderTextColor"/>
        public string PlaceholderText
        {
            get { return (string)GetValue(PlaceholderTextProperty); }
            set { SetValue(PlaceholderTextProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="PlaceholderText"/> bindable property.
        /// </summary>
        public static readonly BindableProperty PlaceholderTextProperty =
            BindableProperty.Create(nameof(PlaceholderText), typeof(string), typeof(AutoSuggestBox), string.Empty, BindingMode.OneWay, null, null);

        /// <summary>
        /// Gets or sets the foreground color of the control
        /// </summary>
        /// <seealso cref="PlaceholderText"/>
        public Color PlaceholderTextColor
        {
            get { return (Color)GetValue(PlaceholderTextColorProperty); }
            set { SetValue(PlaceholderTextColorProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="PlaceholderTextColor"/> bindable property.
        /// </summary>
        public static readonly BindableProperty PlaceholderTextColorProperty =
            BindableProperty.Create(nameof(PlaceholderTextColor), typeof(Color), typeof(AutoSuggestBox), Colors.Gray, BindingMode.OneWay, null, null);

        /// <summary>
        /// Gets or sets the property path that is used to get the value for display in the
        /// text box portion of the AutoSuggestBox control, when an item is selected.
        /// </summary>
        /// <value>
        /// The property path that is used to get the value for display in the text box portion
        /// of the AutoSuggestBox control, when an item is selected.
        /// </value>
        public string TextMemberPath
        {
            get { return (string)GetValue(TextMemberPathProperty); }
            set { SetValue(TextMemberPathProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="TextMemberPath"/> bindable property.
        /// </summary>
        public static readonly BindableProperty TextMemberPathProperty =
            BindableProperty.Create(nameof(TextMemberPath), typeof(string), typeof(AutoSuggestBox), string.Empty, BindingMode.OneWay, null, null);

        /// <summary>
        /// Gets or sets the name or path of the property that is displayed for each data item.
        /// </summary>
        /// <value>
        /// The name or path of the property that is displayed for each the data item in
        /// the control. The default is an empty string ("").
        /// </value>
        public string DisplayMemberPath
        {
            get { return (string)GetValue(DisplayMemberPathProperty); }
            set { SetValue(DisplayMemberPathProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="DisplayMemberPath"/> bindable property.
        /// </summary>
        public static readonly BindableProperty DisplayMemberPathProperty =
            BindableProperty.Create(nameof(DisplayMemberPath), typeof(string), typeof(AutoSuggestBox), string.Empty, BindingMode.OneWay, null, null);

        /// <summary>
        /// Gets or sets a Boolean value indicating whether the drop-down portion of the AutoSuggestBox is open.
        /// </summary>
        /// <value>A Boolean value indicating whether the drop-down portion of the AutoSuggestBox is open.</value>
        public bool IsSuggestionListOpen
        {
            get { return (bool)GetValue(IsSuggestionListOpenProperty); }
            set { SetValue(IsSuggestionListOpenProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="IsSuggestionListOpen"/> bindable property.
        /// </summary>
        public static readonly BindableProperty IsSuggestionListOpenProperty =
            BindableProperty.Create(nameof(IsSuggestionListOpen), typeof(bool), typeof(AutoSuggestBox), false, BindingMode.OneWay, null, null);


        /// <summary>
        /// Used in conjunction with <see cref="TextMemberPath"/>, gets or sets a value indicating whether items in the view will trigger an update 
        /// of the editable text part of the <see cref="AutoSuggestBox"/> when clicked.
        /// </summary>
        /// <value>A value indicating whether items in the view will trigger an update of the editable text part of the <see cref="AutoSuggestBox"/> when clicked.</value>
        public bool UpdateTextOnSelect
        {
            get { return (bool)GetValue(UpdateTextOnSelectProperty); }
            set { SetValue(UpdateTextOnSelectProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="UpdateTextOnSelect"/> bindable property.
        /// </summary>
        public static readonly BindableProperty UpdateTextOnSelectProperty =
            BindableProperty.Create(nameof(UpdateTextOnSelect), typeof(bool), typeof(AutoSuggestBox), true, BindingMode.OneWay, null, null);

        /// <summary>
        /// Gets or sets the header object for the text box portion of this control.
        /// </summary>
        /// <value>The header object for the text box portion of this control.</value>
        public System.Collections.IList ItemsSource
        {
            get { return GetValue(ItemsSourceProperty) as System.Collections.IList; }
            set { SetValue(ItemsSourceProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="ItemsSource"/> bindable property.
        /// </summary>
        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(nameof(ItemsSource), typeof(System.Collections.IList), typeof(AutoSuggestBox), null, BindingMode.OneWay, null, null);

        void IAutoSuggestBox.SuggestionChosen(object selectedItem)
        {
            SuggestionChosen?.Invoke(this, new AutoSuggestBoxSuggestionChosenEventArgs(selectedItem));
        }

        /// <summary>
        /// Raised before the text content of the editable control component is updated.
        /// </summary>
        public event EventHandler<AutoSuggestBoxSuggestionChosenEventArgs> SuggestionChosen;

        // Called by the native control when users enter text
        void IAutoSuggestBox.TextChanged(string text, AutoSuggestionBoxTextChangeReason reason)
        {
            suppressTextChangedEvent = true; //prevent loop of events raising, as setting this property will make it back into the native control
            Text = text;
            suppressTextChangedEvent = false;
            TextChanged?.Invoke(this, new AutoSuggestBoxTextChangedEventArgs(reason));
        }

        /// <summary>
        /// Raised after the text content of the editable control component is updated.
        /// </summary>
        public event EventHandler<AutoSuggestBoxTextChangedEventArgs> TextChanged;

        void IAutoSuggestBox.QuerySubmitted(string queryText, object chosenSuggestion)
        {
            QuerySubmitted?.Invoke(this, new AutoSuggestBoxQuerySubmittedEventArgs(queryText, chosenSuggestion));
        }

        /// <summary>
        /// Occurs when the user submits a search query.
        /// </summary>
        public event EventHandler<AutoSuggestBoxQuerySubmittedEventArgs> QuerySubmitted;
    }
}
