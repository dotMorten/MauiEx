namespace MauiEx
{
    /// <summary>
    /// AppHost builder methods for registering the controls with .NET MAUI.
    /// </summary>
    public static class AppHostBuilderExtensions
    {
        /// <summary>
        /// Initializes the MauiEx controls with .NET MAUI.
        /// </summary>
        /// <param name="builder">The .NET MAUI host builder.</param>
        /// <returns>The host builder.</returns>
        public static MauiAppBuilder UseMauiEx(this MauiAppBuilder builder)
            => builder.ConfigureMauiHandlers((h) =>
            {
                h.AddHandler(typeof(AutoSuggestBox), typeof(Handlers.AutoSuggestBoxHandler));
            });
    }
}