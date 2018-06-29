#if __IOS__
using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using UIKit;

namespace dotMorten.Xamarin.Forms
{
    public partial class AutoSuggestBox
    {
        private nfloat keyboardHeight;
        private NSLayoutConstraint bottomConstraint;

        internal UIKit.UITextField inputText { get; }
        internal UIKit.UITableView selectionList { get; }

        private void UpdateItems()
        {
            if (selectionList.Source is TableSource<object> oldSource)
            {
                oldSource.TableRowSelected -= SuggestionTableSource_TableRowSelected;
            }
            selectionList.Source = null;

            IEnumerable<object> suggestions = ItemsSource?.OfType<object>();
            if (suggestions != null && suggestions.Count() > 0)
            {
                var suggestionTableSource = new TableSource<object>(suggestions, (t) =>
                {
                    if (!string.IsNullOrEmpty(DisplayMemberPath))
                        return t?.GetType().GetProperty(DisplayMemberPath)?.GetValue(t)?.ToString();
                    else
                        return t?.ToString();
                });
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

        private void OnKeyboardHide(object sender, UIKeyboardEventArgs e)
        {
            keyboardHeight = 0; ;
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
            if(e.Reason == UITextFieldDidEndEditingReason.Committed)
            {
                IsSuggestionListOpen = false;
            }
        }

        private void SuggestionTableSource_TableRowSelected(object sender, TableRowSelectedEventArgs<object> e)
        {
            selectionList.DeselectRow(e.SelectedItemIndexPath, false);
            IsSuggestionListOpen = false;
            var selection = e.SelectedItem;
            Text = string.IsNullOrEmpty(TextMemberPath) ? selection?.ToString() : selection.GetType().GetProperty(TextMemberPath)?.GetValue(selection)?.ToString();
            SuggestionChosen?.Invoke(this, new AutoSuggestBoxSuggestionChosenEventArgs(selection));
            QuerySubmitted?.Invoke(this, new AutoSuggestBoxQuerySubmittedEventArgs(Text, selection));
        }

        private void InputText_Started(object sender, EventArgs e)
        {
            if (ItemsSource != null && ItemsSource.OfType<object>().Any())
                IsSuggestionListOpen = true;
        }

        private void InputText_EditingChanged(object sender, EventArgs e)
        {
            var searchText = inputText.Text;
            suppressTextChangedEvent = true;
            Text = searchText;
            suppressTextChangedEvent = false;
            TextChanged?.Invoke(this, new AutoSuggestBoxTextChangedEventArgs(AutoSuggestionBoxTextChangeReason.UserInput));
        }

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