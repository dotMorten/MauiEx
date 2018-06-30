#if __IOS__
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreGraphics;
using Foundation;
using UIKit;

namespace dotMorten.Xamarin.Forms
{
    /// <summary>
    ///  Extends AutoCompleteTextView to have similar APIs and behavior to UWP's AutoSuggestBox, which greatly simplifies wrapping it
    /// </summary>
    internal class NativeAutoSuggestBox : UIKit.UIView
    {
        private bool suppressTextChangedEvent;
        private nfloat keyboardHeight;
        private NSLayoutConstraint bottomConstraint;
        private Func<object, string> textFunc;

        private UIKit.UITextField inputText { get; }

        private UIKit.UITableView selectionList { get; }

        public NativeAutoSuggestBox()
        {
            inputText = new UIKit.UITextField()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BorderStyle = UIKit.UITextBorderStyle.RoundedRect
            };
            inputText.ShouldReturn = InputText_OnShouldReturn;
            inputText.EditingChanged += InputText_EditingChanged;
            inputText.EndedWithReason += InputText_EndedWithReason;
            inputText.ReturnKeyType = UIKit.UIReturnKeyType.Search;

            AddSubview(inputText);
            inputText.TopAnchor.ConstraintEqualTo(TopAnchor).Active = true;
            inputText.LeftAnchor.ConstraintEqualTo(LeftAnchor).Active = true;
            inputText.WidthAnchor.ConstraintEqualTo(WidthAnchor).Active = true;
            inputText.HeightAnchor.ConstraintEqualTo(HeightAnchor).Active = true;
            selectionList = new UIKit.UITableView() { TranslatesAutoresizingMaskIntoConstraints = false };

            UIKit.UIKeyboard.Notifications.ObserveWillShow(OnKeyboardShow);
            UIKit.UIKeyboard.Notifications.ObserveWillHide(OnKeyboardHide);
        }

        public UIFont Font
        {
            get => inputText.Font;
            set => inputText.Font = value;
        }

        internal void SetItems(IEnumerable<object> items, Func<object, string> labelFunc, Func<object, string> textFunc)
        {
            this.textFunc = textFunc;
            if (selectionList.Source is TableSource<object> oldSource)
            {
                oldSource.TableRowSelected -= SuggestionTableSource_TableRowSelected;
            }
            selectionList.Source = null;

            IEnumerable<object> suggestions = items?.OfType<object>();
            if (suggestions != null && suggestions.Any())
            {
                var suggestionTableSource = new TableSource<object>(suggestions, labelFunc);
                suggestionTableSource.TableRowSelected += SuggestionTableSource_TableRowSelected;
                selectionList.Source = suggestionTableSource;
                selectionList.ReloadData();
                IsSuggestionListOpen = true;
            }
            else
            {
                IsSuggestionListOpen = false;
            }
        }

        public string PlaceholderText
        {
            get => inputText.Placeholder;
            set => inputText.Placeholder = value;
        }

		public bool IsSuggestionListOpen
        {
            get => selectionList.Superview != null;
			set
            {
                if (value && selectionList.Superview == null && selectionList.Source != null && selectionList.Source.RowsInSection(selectionList, 0) > 0)
                {
                    UIKit.UIApplication.SharedApplication.Windows[0].AddSubview(selectionList);
                    selectionList.TopAnchor.ConstraintEqualTo(inputText.BottomAnchor).Active = true;
                    selectionList.LeftAnchor.ConstraintEqualTo(inputText.LeftAnchor).Active = true;
                    selectionList.WidthAnchor.ConstraintEqualTo(inputText.WidthAnchor).Active = true;
                    bottomConstraint = selectionList.BottomAnchor.ConstraintGreaterThanOrEqualTo(selectionList.Superview.BottomAnchor, -keyboardHeight);
                    bottomConstraint.Active = true;
                    selectionList.UpdateConstraints();
                }
                else if (!value && selectionList.Superview != null)
                    selectionList.RemoveFromSuperview();
            }
        }

        private void OnKeyboardHide(object sender, UIKeyboardEventArgs e)
        {
            keyboardHeight = 0;
            if (bottomConstraint != null)
            {
                bottomConstraint.Constant = keyboardHeight;
                selectionList.UpdateConstraints();
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
                selectionList.UpdateConstraints();
            }
        }

        private bool InputText_OnShouldReturn(UITextField field)
        {
            if (string.IsNullOrWhiteSpace(field.Text)) { return false; }
            field.ResignFirstResponder();
            QuerySubmitted?.Invoke(this, new AutoSuggestBoxQuerySubmittedEventArgs(inputText.Text, null));
            return true;
        }

        private void InputText_EndedWithReason(object sender, UIKit.UITextFieldEditingEndedEventArgs e)
        {
            if (e.Reason == UITextFieldDidEndEditingReason.Committed)
            {
                IsSuggestionListOpen = false;
            }
        }

        private void SuggestionTableSource_TableRowSelected(object sender, TableRowSelectedEventArgs<object> e)
        {
            selectionList.DeselectRow(e.SelectedItemIndexPath, false);
            var selection = e.SelectedItem;
            suppressTextChangedEvent = true;
            inputText.Text = textFunc(selection);
            suppressTextChangedEvent = true;
            TextChanged?.Invoke(this, new AutoSuggestBoxTextChangedEventArgs(AutoSuggestionBoxTextChangeReason.SuggestionChosen));
            SuggestionChosen?.Invoke(this, new AutoSuggestBoxSuggestionChosenEventArgs(selection));
            QuerySubmitted?.Invoke(this, new AutoSuggestBoxQuerySubmittedEventArgs(Text, selection));
            IsSuggestionListOpen = false;
        }

        private void InputText_EditingChanged(object sender, EventArgs e)
        {
            TextChanged?.Invoke(this, new AutoSuggestBoxTextChangedEventArgs(AutoSuggestionBoxTextChangeReason.UserInput));
            IsSuggestionListOpen = true;
        }

		public string Text
        {
            get => inputText.Text;
			set
            {
                suppressTextChangedEvent = true;
                inputText.Text = value;
                suppressTextChangedEvent = true;
                this.TextChanged?.Invoke(this, new AutoSuggestBoxTextChangedEventArgs(AutoSuggestionBoxTextChangeReason.ProgrammaticChange));
            }
        }

        public void SetTextColor(global::Xamarin.Forms.Color color)
        {
            inputText.TextColor = global::Xamarin.Forms.Platform.iOS.ColorExtensions.ToUIColor(color);
        }

        public event EventHandler<AutoSuggestBoxTextChangedEventArgs> TextChanged;

        public event EventHandler<AutoSuggestBoxQuerySubmittedEventArgs> QuerySubmitted;

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