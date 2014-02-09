
namespace SV.ImageLoader
{
    using System;
    using System.Windows.Threading;

    public static class DispatcherHelper
    {
        private static Dispatcher dispatcher;

        public static void Initialize()
        {
            dispatcher = Dispatcher.CurrentDispatcher;
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
