# AutoSuggestBox

Represents a text control that makes suggestions to users as they type. The app is notified when text has been changed by the user and is responsible for providing relevant suggestions for this control to display.

The control is based on UWP's AutoSuggestBox behavior, with UI adapted for native look on each platform.

![autosuggestbox_ios](https://user-images.githubusercontent.com/1378165/51139691-6c226600-17f9-11e9-8f76-48a1128986ca.gif)

## NuGet
NuGet ID: `dotMorten.Xamarin.Forms.AutoSuggestBox`

You can get this from NuGet here: https://www.nuget.org/packages/dotMorten.Xamarin.Forms.AutoSuggestBox/

Or use the Package Manager:
```
Install-Package dotMorten.Xamarin.Forms.AutoSuggestBox 
```

## Description

Use an AutoSuggestBox to provide a list of suggestions for a user to select from as they type.

To use an AutoSuggestBox, you need to respond to 3 user actions.

- Text changed - When the user enters text, update the suggestion list.
- Suggestion chosen - When the user chooses a suggestion in the suggestion list, update the text box.
- Query submitted - When the user submits a query, show the query results.

### Text changed
The TextChanged event occurs whenever the content of the text box is updated. Use the event args Reason property to determine whether the change was due to user input. If the change reason is UserInput, filter your data based on the input. Then, set the filtered data as the ItemsSource of the AutoSuggestBox to update the suggestion list.

To display the text of a single property of your data item, set the DisplayMemberPath property to choose which property from your object to display in the suggestion list.

### Suggestion chosen

You can set the TextMemberPath property to choose which property from your data object to display in the text box. If you specify a TextMemberPath, the text box is updated automatically. You should typically specify the same value for DisplayMemberPath and TextMemberPath so the text is the same in the suggestion list and the text box.
If you need to show more than a simple property, handle the SuggestionChosen event to populate the text box with custom text based on the selected item.

### Query submitted

Handle the QuerySubmitted event to perform a query action appropriate to your app and show the result to the user.
The QuerySubmitted event occurs when a user commits a query string. The user can commit a query in one of these ways:

- While the focus is in the text box, press Enter or click the query icon. The event args ChosenSuggestion property is null.
- While the focus is in the suggestion list, press Enter, click, or tap an item. The event args ChosenSuggestion property contains the item that was selected from the list.
In all cases, the event args QueryText property contains the text from the text box. 


#### Examples
**xaml**
```xml
<AutoSuggestBox PlaceholderText="Search" WidthRequest="200"
                TextChanged="AutoSuggestBox_TextChanged"
                QuerySubmitted="AutoSuggestBox_QuerySubmitted"
                SuggestionChosen="AutoSuggestBox_SuggestionChosen"/>
```
**C#**
```cs
private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
{
    // Only get results when it was a user typing, 
    // otherwise assume the value got filled in by TextMemberPath 
    // or the handler for SuggestionChosen.
    if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
    {
        //Set the ItemsSource to be your filtered dataset
        //sender.ItemsSource = dataset;
    }
}


private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
{
    // Set sender.Text. You can use args.SelectedItem to build your text string.
}


private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
{
    if (args.ChosenSuggestion != null)
    {
        // User selected an item from the suggestion list, take an action on it here.
    }
    else
    {
        // User hit Enter from the search box. Use args.QueryText to determine what to do.
    }
}
```

UWP:

![image](https://user-images.githubusercontent.com/1378165/42106364-a65da5ae-7b88-11e8-81ce-e189ee4cdc8e.png)

Android:

![image](https://user-images.githubusercontent.com/1378165/42108971-edaa4914-7b90-11e8-95d8-063f1f857d5f.png)

iOS:

![image](https://user-images.githubusercontent.com/1378165/42109635-31d3ef44-7b93-11e8-8f65-2e1c9ec07f44.png)


