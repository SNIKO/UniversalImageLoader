
namespace SV.ImageLoader
{
    using System;
    using System.Windows;
    using System.Windows.Threading;

    internal static class DispatcherHelper
    {
        private static Dispatcher dispatcher;

        public static void Initialize()
        {
            dispatcher = Application.Current.RootVisual.Dispatcher;
        }

        public static void InvokeAsync(Action action)
        {
            if (dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                dispatcher.BeginInvoke(action);
            }
        }
    }
}
