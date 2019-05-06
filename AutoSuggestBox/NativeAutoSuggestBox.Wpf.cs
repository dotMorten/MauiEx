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

/*
 * AutoCompleteTextBox, BindingEvaluator and SelectionAdapter were originally
 * taken from https://github.com/quicoli/WPF-AutoComplete-TextBox, then cleaned
 * up a bit and stripped of all the features we don't need here.
 */

namespace dotMorten.Xamarin.Forms
{
    internal class NativeAutoSuggestBox : AutoCompleteTextBox
    {
        public event EventHandler<AutoSuggestBoxTextChangedEventArgs> TextChanged;

        public event EventHandler<AutoSuggestBoxQuerySubmittedEventArgs> QuerySubmitted;

        public event EventHandler<AutoSuggestBoxSuggestionChosenEventArgs> SuggestionChosen;

        public new event EventHandler<RoutedEventArgs> GotFocus;

        public bool IsSuggestionListOpen { get; set; }

        public bool UpdateTextOnSelect { get; set; }

        protected override void OnTextChanged()
        {
            base.OnTextChanged();

            if (_isUpdatingText)
            {
                return;
            }

            var reason = _isUpdatingText ? AutoSuggestionBoxTextChangeReason.ProgrammaticChange : AutoSuggestionBoxTextChangeReason.UserInput;
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
        private TextBox _editor;
        private Popup _popup;
        private Selector _selector;
        private SelectionAdapter _selectionAdapter;
        private bool _selectionCancelled;
        protected bool _isUpdatingText;

        public const string PartEditor = "PART_Editor";
        public const string PartPopup = "PART_Popup";
        public const string PartSelector = "PART_Selector";

        static AutoCompleteTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(typeof(AutoCompleteTextBox)));
        }

        public BindingEvaluator BindingEvaluator { get; set; }

        public static readonly DependencyProperty DisplayMemberPathProperty = DependencyProperty.Register(
            nameof(DisplayMemberPath),
            typeof(string),
            typeof(AutoCompleteTextBox),
            new FrameworkPropertyMetadata(string.Empty));

        public string DisplayMemberPath
        {
            get => (string)GetValue(DisplayMemberPathProperty);
            set => SetValue(DisplayMemberPathProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(AutoCompleteTextBox),
            new FrameworkPropertyMetadata(
                Enumerable.Empty<object>(),
                new PropertyChangedCallback(ItemsSourcePropertyChanged)));

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty PlaceholderTextProperty = DependencyProperty.Register(
            nameof(PlaceholderText),
            typeof(string),
            typeof(AutoCompleteTextBox),
            new FrameworkPropertyMetadata(string.Empty));

        public string PlaceholderText
        {
            get => (string)GetValue(PlaceholderTextProperty);
            set => SetValue(PlaceholderTextProperty, value);
        }

        public static readonly DependencyProperty PlaceholderTextForegroundProperty = DependencyProperty.Register(
            nameof(PlaceholderTextForeground),
            typeof(Brush),
            typeof(AutoCompleteTextBox),
            new FrameworkPropertyMetadata(Brushes.Gray));

        public Brush PlaceholderTextForeground
        {
            get => (Brush)GetValue(PlaceholderTextForegroundProperty);
            set => SetValue(PlaceholderTextForegroundProperty, value);
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(AutoCompleteTextBox),
            new FrameworkPropertyMetadata(
                string.Empty,
                new PropertyChangedCallback(TextPropertyChanged)));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register(
            nameof(IsDropDownOpen),
            typeof(bool),
            typeof(AutoCompleteTextBox),
            new FrameworkPropertyMetadata(false));

        public bool IsDropDownOpen
        {
            get => (bool)GetValue(IsDropDownOpenProperty);
            set => SetValue(IsDropDownOpenProperty, value);
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            nameof(SelectedItem),
            typeof(object),
            typeof(AutoCompleteTextBox),
            new FrameworkPropertyMetadata(null, OnSelectedItemChanged));

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public static readonly DependencyProperty MaxPopUpHeightProperty = DependencyProperty.Register(
            nameof(MaxPopupHeight),
            typeof(int),
            typeof(AutoCompleteTextBox),
            new FrameworkPropertyMetadata(600));

        public int MaxPopupHeight
        {
            get => (int)GetValue(MaxPopUpHeightProperty);
            set => SetValue(MaxPopUpHeightProperty, value);
        }

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

        public static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AutoCompleteTextBox act = null;
            act = d as AutoCompleteTextBox;
            if (act != null)
            {
                if (!act._isUpdatingText)
                {
                    act._isUpdatingText = true;
                    act.Text = act.BindingEvaluator.Evaluate(e.NewValue);
                    act._isUpdatingText = false;
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
            BindingEvaluator = new BindingEvaluator(new Binding(DisplayMemberPath));

            if (_editor != null)
            {
                _editor.PreviewKeyDown += OnEditorKeyDown;
                _editor.LostFocus += OnEditorLostFocus;

                if (SelectedItem != null)
                {
                    _isUpdatingText = true;
                    Text = BindingEvaluator.Evaluate(SelectedItem);
                    _isUpdatingText = false;
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
                BindingEvaluator = new BindingEvaluator(new Binding(DisplayMemberPath));
            }
            if (dataItem == null)
            {
                return string.Empty;
            }
            if (string.IsNullOrEmpty(DisplayMemberPath))
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
            if (_isUpdatingText)
                return;

            SetSelectedItem(null);
            if (Text.Length > 0)
            {
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
            _isUpdatingText = true;
            _editor.Text = SelectedItem == null ? Text : GetDisplayText(SelectedItem);
            _editor.SelectionStart = _editor.Text.Length;
            _editor.SelectionLength = 0;
            _isUpdatingText = false;
            IsDropDownOpen = false;
            _selectionCancelled = true;
        }

        private void OnSelectionAdapterCommit()
        {
            if (_selector.SelectedItem != null)
            {
                SelectedItem = _selector.SelectedItem;
                _isUpdatingText = true;
                Text = GetDisplayText(_selector.SelectedItem);
                SetSelectedItem(_selector.SelectedItem);
                _isUpdatingText = false;
                IsDropDownOpen = false;
            }
        }

        private void OnSelectionAdapterSelectionChanged()
        {
            _isUpdatingText = true;
            _editor.Text = _selector.SelectedItem == null ? Text : GetDisplayText(_selector.SelectedItem);
            _editor.SelectionStart = _editor.Text.Length;
            _editor.SelectionLength = 0;
            ScrollToSelectedItem();
            _isUpdatingText = false;
        }

        private void SetSelectedItem(object item)
        {
            OnItemSelecting();
            SelectedItem = item;
            OnItemSelected();
        }

        protected virtual void OnItemSelecting()
        {
            _isUpdatingText = true;
        }

        protected virtual void OnItemSelected()
        {
            _isUpdatingText = false;
        }

        private void OnUpdatedSuggestions()
        {
            if (ItemsSource is ICollection c)
            {
                IsDropDownOpen = c.Count > 0;
            }
            else
            {
                IsDropDownOpen = false;
            }
        }
    }

    public class SelectionAdapter
    {
        public SelectionAdapter(Selector selector)
        {
            SelectorControl = selector;
            SelectorControl.PreviewMouseDown += OnSelectorMouseDown;
            SelectorControl.PreviewMouseUp += OnSelectorMouseUp;
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
            e.Handled = true;
        }

        private void OnSelectorMouseUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
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
}
#endif
