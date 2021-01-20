using System;
using System.Windows;
using System.Windows.Threading;

namespace MultiTemplateGenerator.UI.Helpers
{
    public static class InvokeHelper
    {
        /// <summary>
        ///     Invoke's Action using Application.Current.Dispatcher if not NULL, else normal Invoke.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="priority"></param>
        public static void Invoke(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            var dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;

            if (!dispatcher.CheckAccess())
                dispatcher.Invoke(priority, action);
            else
                action.Invoke();
        }

        public static T Invoke<T>(Func<T> func, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            T result = default(T);
            var dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;

            if (!dispatcher.CheckAccess())
                dispatcher.Invoke(() => { result = func.Invoke(); }, priority);
            else
                result = func.Invoke();

            return result;
        }
    }
}
