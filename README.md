# Maui Controls
A set of .NET MAUI controls

### NuGet Packages:
  - [dotMorten.MauiEx](https://www.nuget.org/packages/dotMorten.MauiEx/)

# Usage:
In `MauiProgram.CreateMauiApp` register the controls:

```cs
using MauiEx; // Include using to get extension method

namespace SampleApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
          .UseMauiApp<App>()
          .ConfigureFonts(fonts =>
          {
              fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
              fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
          })
          .UseMauiEx(); //Register control library
        return builder.Build();
    }
}
```

## Sponsoring

If you like this library and use it a lot, consider sponsoring me. Anything helps and encourages me to keep going.

See here for details: https://github.com/sponsors/dotMorten

### Controls

- [AutoSuggestBox](AutoSuggestBox/) : Represents a text control that makes suggestions to users as they type. The app is notified when text has been changed by the user and is responsible for providing relevant suggestions for this control to display.
![autosuggestbox](https://user-images.githubusercontent.com/1378165/51137780-42b30b80-17f4-11e9-8ac1-7b129fc3d9ee.gif)

