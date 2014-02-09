
namespace SV.ImageLoader
{
    using System;
    using Windows.UI.Core;

    public static class DispatcherHelper
    {
        private static CoreDispatcher dispatcher;

        public static void Initialize()
        {
            dispatcher = Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher;
        }

        public static void InvokeAsync(Action action)
        {
            if (dispatcher.HasThreadAccess)
            {
                action();
            }
            else
            {
                dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action());
            }
        }
    }
}
