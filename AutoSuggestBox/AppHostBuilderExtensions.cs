using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotMorten.Maui
{/// <summary>
 /// AppHost builder methods for registering the controls with .NET MAUI.
 /// </summary>
    public static class AppHostBuilderExtensions
    {
        public static MauiAppBuilder UseAutoSuggestBox(this MauiAppBuilder builder)
        {
            return builder.ConfigureMauiHandlers((a) => { a.AddHandler(typeof(AutoSuggestBox), typeof(Handlers.AutoSuggestBoxHandler)); });
        }
    }
}