﻿#if __IOS__
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
                BorderStyle = UIKit.UITextBorderStyle.None,
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

            UIKit.UIKeyboard.Notifications.ObserveWillShow(OnKeyboardShow);
            UIKit.UIKeyboard.Notifications.ObserveWillHide(OnKeyboardHide);
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
            border = new CoreAnimation.CALayer();
            var width = 1f;
            border.BorderColor = UIColor.LightGray.CGColor;
            border.Frame = new CGRect(0, Frame.Size.Height - width, Frame.Size.Width, Frame.Size.Height);
            border.BorderWidth = width;
            border.Hidden = !showBottomBorder;
            Layer.AddSublayer(border);
            Layer.MasksToBounds = true;
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
                var suggestionTableSource = new TableSource<object>(suggestions, labelFunc);
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
                if (viewController.PresentedViewController != null)
                    viewController = viewController.PresentedViewController;
                if (SelectionList.Superview == null)
                {
                    viewController.Add(SelectionList);
                }

                if (SelectionList.Window == null)
                   InputTextField.Window.Add(SelectionList);

                SelectionList.TopAnchor.ConstraintEqualTo(InputTextField.BottomAnchor).Active = true;
                SelectionList.LeftAnchor.ConstraintEqualTo(InputTextField.LeftAnchor).Active = true;
                SelectionList.WidthAnchor.ConstraintEqualTo(InputTextField.WidthAnchor).Active = true;
                bottomConstraint = SelectionList.BottomAnchor.ConstraintGreaterThanOrEqualTo(SelectionList.Superview.BottomAnchor, -keyboardHeight);
                bottomConstraint.Active = true;
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
            NSValue nsKeyboardBounds = (NSValue)e.Notification.UserInfo.ObjectForKey(UIKeyboard.BoundsUserInfoKey);
            var keyboardBounds = nsKeyboardBounds.RectangleFValue;
            keyboardHeight = keyboardBounds.Height;
            if (bottomConstraint != null)
            {
                bottomConstraint.Constant = -keyboardHeight;
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

            public TableSource(IEnumerable<T> items, Func<T, string> labelFunc)
            {
                _items = items;
                _labelFunc = labelFunc;
                _cellIdentifier = Guid.NewGuid().ToString();
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.DequeueReusableCell(_cellIdentifier);
                if (cell == null)
                    cell = new UITableViewCell(UITableViewCellStyle.Default, _cellIdentifier);

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
                return 30f;
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