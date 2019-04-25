#if __WPF__
using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
                                     //(used if a resource is not found in the page, 
                                     // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
                                              //(used if a resource is not found in the page, 
                                              // app, or any theme specific resource dictionaries)
)]

namespace dotMorten.Xamarin.Forms
{
    internal class NativeAutoSuggestBox : AutoCompleteTextBox
    {
        public event EventHandler<AutoSuggestBoxTextChangedEventArgs> TextChanged;

        public event EventHandler<AutoSuggestBoxQuerySubmittedEventArgs> QuerySubmitted;

        public event EventHandler<AutoSuggestBoxSuggestionChosenEventArgs> SuggestionChosen;

        public new event EventHandler<RoutedEventArgs> GotFocus;

        public NativeAutoSuggestBox()
        {
            Foreground = new SolidColorBrush(Colors.Black);
        }

        public string PlaceholderText
        {
            get => Watermark;
            set => Watermark = value;
        }

        public bool IsSuggestionListOpen { get; set; }

        public bool UpdateTextOnSelect { get; set; }

        protected override void OnTextChanged()
        {
            base.OnTextChanged();

            if (IsUpdatingText)
            {
                return;
            }

            var reason = IsUpdatingText ? AutoSuggestionBoxTextChangeReason.ProgrammaticChange : AutoSuggestionBoxTextChangeReason.UserInput;
            TextChanged?.Invoke(this, new AutoSuggestBoxTextChangedEventArgs(reason));
        }

        protected override void OnItemSelected()
        {
            base.OnItemSelected();

            QuerySubmitted?.Invoke(this, new AutoSuggestBoxQuerySubmittedEventArgs(Text, SelectedItem));

            if (SelectedItem != null)
            {
                SuggestionChosen?.Invoke(this, new AutoSuggestBoxSuggestionChosenEventArgs(SelectedItem));
            }
        }

        protected override void OnGotFocus(object sender, RoutedEventArgs e)
        {
            base.OnGotFocus(sender, e);

            GotFocus?.Invoke(this, e);
        }
    }

    [TemplatePart(Name = PartEditor, Type = typeof(TextBox))]
    [TemplatePart(Name = PartPopup, Type = typeof(Popup))]
    [TemplatePart(Name = PartSelector, Type = typeof(Selector))]
    public class AutoCompleteTextBox : Control
    {
        public const string PartEditor = "PART_Editor";
        public const string PartPopup = "PART_Popup";
        public const string PartSelector = "PART_Selector";

        public static readonly DependencyProperty DelayProperty = DependencyProperty.Register("Delay", typeof(int), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(200));
        public static readonly DependencyProperty DisplayMemberProperty = DependencyProperty.Register("DisplayMember", typeof(string), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(string.Empty));
        public static readonly DependencyProperty IconPlacementProperty = DependencyProperty.Register("IconPlacement", typeof(IconPlacement), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(IconPlacement.Left));
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(object), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty IconVisibilityProperty = DependencyProperty.Register("IconVisibility", typeof(Visibility), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(Visibility.Visible));
        public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register("IsDropDownOpen", typeof(bool), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register("IsLoading", typeof(bool), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty ItemTemplateSelectorProperty = DependencyProperty.Register("ItemTemplateSelector", typeof(DataTemplateSelector), typeof(AutoCompleteTextBox));
        public static readonly DependencyProperty LoadingContentProperty = DependencyProperty.Register("LoadingContent", typeof(object), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(Enumerable.Empty<object>(), new PropertyChangedCallback(ItemsSourcePropertyChanged)));
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(null, OnSelectedItemChanged));
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(TextPropertyChanged)));
        public static readonly DependencyProperty MaxLengthProperty = DependencyProperty.Register("MaxLength", typeof(int), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(0));
        public static readonly DependencyProperty CharacterCasingProperty = DependencyProperty.Register("CharacterCasing", typeof(CharacterCasing), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(CharacterCasing.Normal));
        public static readonly DependencyProperty MaxPopUpHeightProperty = DependencyProperty.Register("MaxPopUpHeight", typeof(int), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(600));
        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.Register("Watermark", typeof(string), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(string.Empty));
        public static readonly DependencyProperty SuggestionBackgroundProperty = DependencyProperty.Register("SuggestionBackground", typeof(Brush), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(Brushes.White));

        private TextBox _editor;
        private Popup _popup;
        private Selector _selector;
        private SelectionAdapter _selectionAdapter;

        private static void TextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AutoCompleteTextBox t)
            {
                t.OnTextChanged();
            }
        }

        private static void ItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AutoCompleteTextBox t)
            {
                t.OnUpdatedSuggestions();
            }
        }

        private bool _isUpdatingText;
        protected bool IsUpdatingText
        {
            get => _isUpdatingText;
            private set => _isUpdatingText = value;
        }

        private bool _selectionCancelled;

        static AutoCompleteTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(typeof(AutoCompleteTextBox)));
        }

        public int MaxPopupHeight
        {
            get => (int)GetValue(MaxPopUpHeightProperty);
            set => SetValue(MaxPopUpHeightProperty, value);
        }

        public BindingEvaluator BindingEvaluator { get; set; }

        public CharacterCasing CharacterCasing
        {
            get => (CharacterCasing)GetValue(CharacterCasingProperty);
            set => SetValue(CharacterCasingProperty, value);
        }

        public int MaxLength
        {
            get => (int)GetValue(MaxLengthProperty);
            set => SetValue(MaxLengthProperty, value);
        }

        public int Delay
        {
            get => (int)GetValue(DelayProperty);
            set => SetValue(DelayProperty, value);
        }

        public string DisplayMember
        {
            get => (string)GetValue(DisplayMemberProperty);
            set => SetValue(DisplayMemberProperty, value);
        }

        public string Filter { get; set; }

        public object Icon
        {
            get => GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public IconPlacement IconPlacement
        {
            get => (IconPlacement)GetValue(IconPlacementProperty);
            set => SetValue(IconPlacementProperty, value);
        }

        public Visibility IconVisibility
        {
            get => (Visibility)GetValue(IconVisibilityProperty);
            set => SetValue(IconVisibilityProperty, value);
        }

        public bool IsDropDownOpen
        {
            get => (bool)GetValue(IsDropDownOpenProperty);
            set => SetValue(IsDropDownOpenProperty, value);
        }

        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        public DataTemplate ItemTemplate
        {
            get => (DataTemplate)GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        public DataTemplateSelector ItemTemplateSelector
        {
            get => ((DataTemplateSelector)(GetValue(ItemTemplateSelectorProperty)));
            set => SetValue(ItemTemplateSelectorProperty, value);
        }

        public object LoadingContent
        {
            get => GetValue(LoadingContentProperty);
            set => SetValue(LoadingContentProperty, value);
        }

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public string Watermark
        {
            get => (string)GetValue(WatermarkProperty);
            set => SetValue(WatermarkProperty, value);
        }
        public Brush SuggestionBackground
        {
            get => (Brush)GetValue(SuggestionBackgroundProperty);
            set => SetValue(SuggestionBackgroundProperty, value);
        }

        public static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AutoCompleteTextBox act = null;
            act = d as AutoCompleteTextBox;
            if (act != null)
            {
                if (!act.IsUpdatingText)
                {
                    act.IsUpdatingText = true;
                    act.Text = act.BindingEvaluator.Evaluate(e.NewValue);
                    act.IsUpdatingText = false;
                }
            }
        }

        private void ScrollToSelectedItem()
        {
            if (_selector is ListBox listBox && listBox.SelectedItem != null)
                listBox.ScrollIntoView(listBox.SelectedItem);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _editor = Template.FindName(PartEditor, this) as TextBox;
            _popup = Template.FindName(PartPopup, this) as Popup;
            _selector = Template.FindName(PartSelector, this) as Selector;
            BindingEvaluator = new BindingEvaluator(new Binding(DisplayMember));

            if (_editor != null)
            {
                _editor.PreviewKeyDown += OnEditorKeyDown;
                _editor.LostFocus += OnEditorLostFocus;

                if (SelectedItem != null)
                {
                    IsUpdatingText = true;
                    Text = BindingEvaluator.Evaluate(SelectedItem);
                    IsUpdatingText = false;
                }

            }

            GotFocus += OnGotFocus;

            if (_popup != null)
            {
                _popup.Opened += OnPopupOpened;
                _popup.Closed += OnPopupClosed;
            }

            if (_selector != null)
            {
                _selectionAdapter = new SelectionAdapter(_selector);
                _selectionAdapter.Commit += OnSelectionAdapterCommit;
                _selectionAdapter.Cancel += OnSelectionAdapterCancel;
                _selectionAdapter.SelectionChanged += OnSelectionAdapterSelectionChanged;
                _selector.PreviewMouseDown += ItemsSelector_PreviewMouseDown;
            }
        }

        private void ItemsSelector_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((e.OriginalSource as FrameworkElement)?.DataContext == null)
                return;
            if (!_selector.Items.Contains(((FrameworkElement)e.OriginalSource)?.DataContext))
                return;
            _selector.SelectedItem = ((FrameworkElement)e.OriginalSource)?.DataContext;
            OnSelectionAdapterCommit();
        }

        protected virtual void OnGotFocus(object sender, RoutedEventArgs e)
        {
            _editor?.Focus();
        }

        private string GetDisplayText(object dataItem)
        {
            if (BindingEvaluator == null)
            {
                BindingEvaluator = new BindingEvaluator(new Binding(DisplayMember));
            }
            if (dataItem == null)
            {
                return string.Empty;
            }
            if (string.IsNullOrEmpty(DisplayMember))
            {
                return dataItem.ToString();
            }
            return BindingEvaluator.Evaluate(dataItem);
        }

        private void OnEditorKeyDown(object sender, KeyEventArgs e)
        {
            if (_selectionAdapter != null)
            {
                if (IsDropDownOpen)
                    _selectionAdapter.HandleKeyDown(e);
                else
                    IsDropDownOpen = e.Key == Key.Down || e.Key == Key.Up;
            }
        }

        private void OnEditorLostFocus(object sender, RoutedEventArgs e)
        {
            if (!IsKeyboardFocusWithin)
            {
                IsDropDownOpen = false;
            }
        }

        protected virtual void OnTextChanged()
        {
            if (IsUpdatingText)
                return;

            SetSelectedItem(null);
            if (Text.Length > 0)
            {
                IsLoading = true;
                IsDropDownOpen = true;
            }
            else
            {
                IsDropDownOpen = false;
            }
        }

        private void OnPopupClosed(object sender, EventArgs e)
        {
            if (!_selectionCancelled)
            {
                OnSelectionAdapterCommit();
            }
        }

        private void OnPopupOpened(object sender, EventArgs e)
        {
            _selectionCancelled = false;
            _selector.SelectedItem = SelectedItem;
        }

        private void OnSelectionAdapterCancel()
        {
            IsUpdatingText = true;
            _editor.Text = SelectedItem == null ? Filter : GetDisplayText(SelectedItem);
            _editor.SelectionStart = _editor.Text.Length;
            _editor.SelectionLength = 0;
            IsUpdatingText = false;
            IsDropDownOpen = false;
            _selectionCancelled = true;
        }

        private void OnSelectionAdapterCommit()
        {
            if (_selector.SelectedItem != null)
            {
                SelectedItem = _selector.SelectedItem;
                IsUpdatingText = true;
                Text = GetDisplayText(_selector.SelectedItem);
                SetSelectedItem(_selector.SelectedItem);
                IsUpdatingText = false;
                IsDropDownOpen = false;
            }
        }

        private void OnSelectionAdapterSelectionChanged()
        {
            IsUpdatingText = true;
            _editor.Text = _selector.SelectedItem == null ? Filter : GetDisplayText(_selector.SelectedItem);
            _editor.SelectionStart = _editor.Text.Length;
            _editor.SelectionLength = 0;
            ScrollToSelectedItem();
            IsUpdatingText = false;
        }

        private void SetSelectedItem(object item)
        {
            OnItemSelecting();
            SelectedItem = item;
            OnItemSelected();
        }

        protected virtual void OnItemSelecting()
        {
            IsUpdatingText = true;
        }

        protected virtual void OnItemSelected()
        {
            IsUpdatingText = false;
        }

        private void OnUpdatedSuggestions()
        {
            if (ItemsSource is ICollection c)
            {
                IsDropDownOpen = c.Count > 0;
            }
            else if (_selector != null)
            {
                IsDropDownOpen = _selector.HasItems;
            }
            else
            {
                IsDropDownOpen = false;
            }

            IsLoading = false;
        }
    }

    public class SelectionAdapter
    {
        public SelectionAdapter(Selector selector)
        {
            SelectorControl = selector;
            SelectorControl.PreviewMouseUp += OnSelectorMouseDown;
        }

        public delegate void CancelEventHandler();

        public delegate void CommitEventHandler();

        public delegate void SelectionChangedEventHandler();

        public event CancelEventHandler Cancel;
        public event CommitEventHandler Commit;
        public event SelectionChangedEventHandler SelectionChanged;

        public Selector SelectorControl { get; set; }

        public void HandleKeyDown(KeyEventArgs key)
        {
            switch (key.Key)
            {
                case Key.Down:
                    IncrementSelection();
                    break;
                case Key.Up:
                    DecrementSelection();
                    break;
                case Key.Enter:
                    Commit?.Invoke();

                    break;
                case Key.Escape:
                    Cancel?.Invoke();

                    break;
                case Key.Tab:
                    Commit?.Invoke();

                    break;
            }
        }

        private void DecrementSelection()
        {
            if (SelectorControl.SelectedIndex == -1)
            {
                SelectorControl.SelectedIndex = SelectorControl.Items.Count - 1;
            }
            else
            {
                SelectorControl.SelectedIndex -= 1;
            }

            SelectionChanged?.Invoke();
        }

        private void IncrementSelection()
        {
            if (SelectorControl.SelectedIndex == SelectorControl.Items.Count - 1)
            {
                SelectorControl.SelectedIndex = -1;
            }
            else
            {
                SelectorControl.SelectedIndex += 1;
            }

            SelectionChanged?.Invoke();
        }

        private void OnSelectorMouseDown(object sender, MouseButtonEventArgs e)
        {
            Commit?.Invoke();
        }
    }

    public class BindingEvaluator : FrameworkElement
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(string), typeof(BindingEvaluator), new FrameworkPropertyMetadata(string.Empty));

        public BindingEvaluator(Binding binding)
        {
            ValueBinding = binding;
        }

        public string Value
        {
            get => (string)GetValue(ValueProperty);

            set => SetValue(ValueProperty, value);
        }

        public Binding ValueBinding { get; set; }

        public string Evaluate(object dataItem)
        {
            DataContext = dataItem;
            SetBinding(ValueProperty, ValueBinding);
            return Value;
        }
    }

    public enum IconPlacement
    {
        Left,
        Right
    }
}
#endif
