#if __IOS__
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreGraphics;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace dotMorten.Xamarin.Forms.Platform.iOS
{
    /// <summary>
    ///  Creates a UIView with dropdown with a similar API and behavior to UWP's AutoSuggestBox
    /// </summary>
    public class iOSAutoSuggestBox : UIKit.UIView
    {
        private nfloat keyboardHeight;
        private NSLayoutConstraint bottomConstraint;
        private Func<object, string> textFunc;
        private CoreAnimation.CALayer border;
        private bool showBottomBorder = true;
        private static UIColor _deafultColor;

        /// <summary>
        /// Gets a reference to the text field in the view
        /// </summary>
        public UIKit.UITextField InputTextField { get; }

        /// <summary>
        /// Gets a reference to the drop down selection list in the view
        /// </summary>
        public UIKit.UITableView SelectionList { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="iOSAutoSuggestBox"/>.
        /// </summary>
        public iOSAutoSuggestBox()
        {
            InputTextField = new UIKit.UITextField()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BorderStyle = UITextBorderStyle.None,
                ReturnKeyType = UIKit.UIReturnKeyType.Search,
                AutocorrectionType = UITextAutocorrectionType.No
            };
            InputTextField.ShouldReturn = InputText_OnShouldReturn;
            InputTextField.EditingDidBegin += OnEditingDidBegin;
            InputTextField.EditingDidEnd += OnEditingDidEnd;
            InputTextField.EditingChanged += InputText_EditingChanged;

            AddSubview(InputTextField);
            InputTextField.TopAnchor.ConstraintEqualTo(TopAnchor).Active = true;
            InputTextField.LeftAnchor.ConstraintEqualTo(LeftAnchor).Active = true;
            InputTextField.WidthAnchor.ConstraintEqualTo(WidthAnchor).Active = true;
            InputTextField.HeightAnchor.ConstraintEqualTo(HeightAnchor).Active = true;
            SelectionList = new UIKit.UITableView() { TranslatesAutoresizingMaskIntoConstraints = false };
            _deafultColor = SelectionList.BackgroundColor;

            UIKit.UIKeyboard.Notifications.ObserveWillShow(OnKeyboardShow);
            UIKit.UIKeyboard.Notifications.ObserveWillHide(OnKeyboardHide);
        }


        internal void SetBackgroundColor(Color color)
        {
            UIColor uiColor = global::Xamarin.Forms.Platform.iOS.ColorExtensions.ToUIColor(color);
            InputTextField.BackgroundColor = uiColor;
            if (uiColor.ToColor().A > 0)
            {
                SelectionList.BackgroundColor = global::Xamarin.Forms.Platform.iOS.ColorExtensions.ToUIColor(color);
            }
            else
                SelectionList.BackgroundColor = _deafultColor;
        }


        /// <inheritdoc />
        public override void MovedToWindow()
        {
            base.MovedToWindow();
            UpdateSuggestionListOpenState();
        }

        private void OnEditingDidBegin(object sender, EventArgs e)
        {
            IsSuggestionListOpen = true;
            EditingDidBegin?.Invoke(this, e);
        }

        private void OnEditingDidEnd(object sender, EventArgs e)
        {
            IsSuggestionListOpen = false;
            EditingDidEnd?.Invoke(this, e);
        }

        internal EventHandler EditingDidBegin;

        internal EventHandler EditingDidEnd;

        /// <inheritdoc />
        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            AddBottomBorder();
        }

        private void AddBottomBorder()
        {
            var width = 1f;
            InputTextField.Layer.BorderColor = UIColor.LightGray.CGColor;
            InputTextField.Layer.BorderWidth = width;
            //border = new CoreAnimation.CALayer();
            //border.BorderColor = UIColor.LightGray.CGColor;
            //border.Frame = new CGRect(0, Frame.Size.Height - width, Frame.Size.Width, Frame.Size.Height);
            //border.BorderWidth = width;
            //border.Hidden = !showBottomBorder;
            //Layer.AddSublayer(border);
            //Layer.MasksToBounds = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to render a border line under the text field
        /// </summary>
        public bool ShowBottomBorder
        {
            get => showBottomBorder;
            set
            {
                showBottomBorder = value;
                if (border != null) border.Hidden = !value;
            }
        }

        /// <summary>
        /// Gets or sets the font of the <see cref="InputTextField"/>
        /// </summary>
        public virtual UIFont Font
        {
            get => InputTextField.Font;
            set => InputTextField.Font = value;
        }

        internal void SetItems(IEnumerable<object> items, Func<object, string> labelFunc, Func<object, string> textFunc)
        {
            this.textFunc = textFunc;
            if (SelectionList.Source is TableSource<object> oldSource)
            {
                oldSource.TableRowSelected -= SuggestionTableSource_TableRowSelected;
            }
            SelectionList.Source = null;

            IEnumerable<object> suggestions = items?.OfType<object>();
            if (suggestions != null && suggestions.Any())
            {
                TableSource<object> suggestionTableSource = new TableSource<object>(suggestions, labelFunc, InputTextField);
                suggestionTableSource.TableRowSelected += SuggestionTableSource_TableRowSelected;
                SelectionList.Source = suggestionTableSource;
                SelectionList.ReloadData();
                IsSuggestionListOpen = true;
            }
            else
            {
                IsSuggestionListOpen = false;
            }
        }

        /// <summary>
        /// Gets or sets the placeholder text to be displayed in the <see cref="InputTextField"/>.
        /// </summary>
        public virtual string PlaceholderText
        {
            get => InputTextField.Placeholder;
            set => InputTextField.Placeholder = value;
        }

        /// <summary>
        /// Gets or sets the color of the <see cref="PlaceholderText"/> in the <see cref="InputTextField" />.
        /// </summary>
        /// <param name="color">color</param>
        public virtual void SetPlaceholderTextColor(global::Xamarin.Forms.Color color)
        {
            // See https://github.com/xamarin/Xamarin.Forms/blob/4d9a5bf3706778770026a18ae81a7dd5c4c15db4/Xamarin.Forms.Platform.iOS/Renderers/EntryRenderer.cs#L260
            InputTextField.AttributedPlaceholder = new NSAttributedString(InputTextField.Placeholder ?? string.Empty, null, ColorExtensions.ToUIColor(color));
        }

        private bool _isSuggestionListOpen;

        /// <summary>
        /// Gets or sets a Boolean value indicating whether the drop-down portion of the AutoSuggestBox is open.
        /// </summary>
        public virtual bool IsSuggestionListOpen
        {
            get => _isSuggestionListOpen;
            set
            {
                _isSuggestionListOpen = value;
                UpdateSuggestionListOpenState();
            }
        }

        private void UpdateSuggestionListOpenState()
        {
            if (_isSuggestionListOpen && SelectionList.Source != null && SelectionList.Source.RowsInSection(SelectionList, 0) > 0)
            {
                var viewController = InputTextField.Window?.RootViewController;
                if (viewController == null)
                    return;

                if (SelectionList.Superview != null)
                    SelectionList.RemoveFromSuperview();
                viewController.Add(SelectionList);

                NSLayoutConstraint TopConstraint = SelectionList.TopAnchor.ConstraintEqualTo(InputTextField.BottomAnchor);
                TopConstraint.Priority = 1000;
                TopConstraint.Active = true;
                SelectionList.LeftAnchor.ConstraintEqualTo(InputTextField.LeftAnchor).Active = true;
                SelectionList.RightAnchor.ConstraintEqualTo(InputTextField.RightAnchor).Active = true;
                NSLayoutConstraint HeightConstraint = SelectionList.HeightAnchor.ConstraintLessThanOrEqualTo(InputTextField.HeightAnchor, SelectionList.NumberOfRowsInSection(0));
                HeightConstraint.Active = true;
                HeightConstraint.Priority = 750;

                NSLayoutConstraint BottomConstraint = SelectionList.BottomAnchor.ConstraintEqualTo(SelectionList.Superview.BottomAnchor, -(keyboardHeight + 20));
                BottomConstraint.Active = true;
                BottomConstraint.Priority = 750;

                SelectionList.UpdateConstraints();
            }
            else
            {
                if (SelectionList.Superview != null)
                    SelectionList.RemoveFromSuperview();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether items in the view will trigger an update of the editable text part of the AutoSuggestBox when clicked.
        /// </summary>
        public virtual bool UpdateTextOnSelect { get; set; } = true;

        private void OnKeyboardHide(object sender, UIKeyboardEventArgs e)
        {
            keyboardHeight = 0;
            if (bottomConstraint != null)
            {
                bottomConstraint.Constant = keyboardHeight;
                SelectionList.UpdateConstraints();
            }
        }

        private void OnKeyboardShow(object sender, UIKeyboardEventArgs e)
        {
            UpdateSuggestionListOpenState();
            NSValue nsKeyboardBounds = (NSValue)e.Notification.UserInfo.ObjectForKey(UIKeyboard.BoundsUserInfoKey);
            var keyboardBounds = nsKeyboardBounds.RectangleFValue;
            keyboardHeight = keyboardBounds.Height;
            if (bottomConstraint != null)
            {
                bottomConstraint.Constant = keyboardHeight;
                SelectionList.UpdateConstraints();
            }
        }

        private bool InputText_OnShouldReturn(UITextField field)
        {
            if (string.IsNullOrWhiteSpace(field.Text)) { return false; }
            field.ResignFirstResponder();
            QuerySubmitted?.Invoke(this, new AutoSuggestBoxQuerySubmittedEventArgs(InputTextField.Text, null));
            return true;
        }

        /// <inheritdoc />
        public override bool BecomeFirstResponder()
        {
            return InputTextField.BecomeFirstResponder();
        }

        /// <inheritdoc />
        public override bool ResignFirstResponder()
        {
            return InputTextField.ResignFirstResponder();
        }

        /// <inheritdoc />
        public override bool IsFirstResponder => InputTextField.IsFirstResponder;

        private void SuggestionTableSource_TableRowSelected(object sender, TableRowSelectedEventArgs<object> e)
        {
            SelectionList.DeselectRow(e.SelectedItemIndexPath, false);
            var selection = e.SelectedItem;
            if (UpdateTextOnSelect)
            {
                InputTextField.Text = textFunc(selection);
                TextChanged?.Invoke(this, new AutoSuggestBoxTextChangedEventArgs(AutoSuggestionBoxTextChangeReason.SuggestionChosen));
            }
            SuggestionChosen?.Invoke(this, new AutoSuggestBoxSuggestionChosenEventArgs(selection));
            QuerySubmitted?.Invoke(this, new AutoSuggestBoxQuerySubmittedEventArgs(Text, selection));
            IsSuggestionListOpen = false;
        }

        private void InputText_EditingChanged(object sender, EventArgs e)
        {
            TextChanged?.Invoke(this, new AutoSuggestBoxTextChangedEventArgs(AutoSuggestionBoxTextChangeReason.UserInput));
            IsSuggestionListOpen = true;
        }

        /// <summary>
        /// Gets or sets the text displayed in the <see cref="InputTextField"/>
        /// </summary>
        public virtual string Text
        {
            get => InputTextField.Text;
            set
            {
                InputTextField.Text = value;
                this.TextChanged?.Invoke(this, new AutoSuggestBoxTextChangedEventArgs(AutoSuggestionBoxTextChangeReason.ProgrammaticChange));
            }
        }

        /// <summary>
        /// Assigns the text color to the <see cref="InputTextField"/>
        /// </summary>
        /// <param name="color">color</param>
        public virtual void SetTextColor(global::Xamarin.Forms.Color color)
        {
            InputTextField.TextColor = global::Xamarin.Forms.Platform.iOS.ColorExtensions.ToUIColor(color);
        }

        /// <summary>
        /// Raised after the text content of the editable control component is updated.
        /// </summary>
        public event EventHandler<AutoSuggestBoxTextChangedEventArgs> TextChanged;

        /// <summary>
        /// Occurs when the user submits a search query.
        /// </summary>
        public event EventHandler<AutoSuggestBoxQuerySubmittedEventArgs> QuerySubmitted;

        /// <summary>
        /// Raised before the text content of the editable control component is updated.
        /// </summary>
        public event EventHandler<AutoSuggestBoxSuggestionChosenEventArgs> SuggestionChosen;

        private class TableSource<T> : UITableViewSource
        {
            readonly IEnumerable<T> _items;
            readonly Func<T, string> _labelFunc;
            readonly string _cellIdentifier;
            readonly UIFont _inputFont;
            readonly UIColor _inputTextColor;
            readonly nfloat _inputHeight;
            public TableSource(IEnumerable<T> items, Func<T, string> labelFunc, UITextField input)
            {
                _items = items;
                _labelFunc = labelFunc;
                _cellIdentifier = Guid.NewGuid().ToString();
                _inputFont = input.Font;
                _inputTextColor = input.TextColor;
                _inputHeight = input.Frame.Size.Height;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.DequeueReusableCell(_cellIdentifier);
                if (cell == null)
                    cell = new UITableViewCell(UITableViewCellStyle.Default, _cellIdentifier);
                cell.BackgroundColor = tableView.BackgroundColor;
                cell.TextLabel.TextColor = _inputTextColor;
                cell.TextLabel.Font = _inputFont;
                var item = _items.ElementAt(indexPath.Row);

                cell.TextLabel.Text = _labelFunc(item);

                return cell;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                OnTableRowSelected(indexPath);
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                return _items.Count();
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return _inputHeight;
            }

            public event EventHandler<TableRowSelectedEventArgs<T>> TableRowSelected;

            private void OnTableRowSelected(NSIndexPath itemIndexPath)
            {
                var item = _items.ElementAt(itemIndexPath.Row);
                var label = _labelFunc(item);
                TableRowSelected?.Invoke(this, new TableRowSelectedEventArgs<T>(item, label, itemIndexPath));
            }
        }

        private class TableRowSelectedEventArgs<T> : EventArgs
        {
            public TableRowSelectedEventArgs(T selectedItem, string selectedItemLabel, NSIndexPath selectedItemIndexPath)
            {
                SelectedItem = selectedItem;
                SelectedItemLabel = selectedItemLabel;
                SelectedItemIndexPath = selectedItemIndexPath;
            }

            public T SelectedItem { get; }
            public string SelectedItemLabel { get; }
            public NSIndexPath SelectedItemIndexPath { get; }
        }
    }
}
#endif