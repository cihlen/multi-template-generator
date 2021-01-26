using System;
using System.Threading;
using System.Threading.Tasks;

namespace MultiTemplateGenerator.UI.Helpers
{
    public static class AsyncHelper
    {
        private static TaskFactory _myTaskFactory;

        private static void Init()
        {
            _myTaskFactory = new
                TaskFactory(CancellationToken.None,
                    TaskCreationOptions.None,
                    TaskContinuationOptions.None,
                    TaskScheduler.Default);
        }

        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            Init();
            return _myTaskFactory
              .StartNew(func)
              .Unwrap()
              .GetAwaiter()
              .GetResult();
        }

        public static void RunSync(Func<Task> func)
        {
            Init();
            _myTaskFactory
              .StartNew(func)
              .Unwrap()
              .GetAwaiter()
              .GetResult();
        }
    }
}
