using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Lab2
{
    public delegate TResult WorkerAction<TArgument, TResult>(TArgument argument);
    public partial class CustomExecutor<TArgument, TResult>
    {
        private readonly ConcurrentQueue<KeyValuePair<WorkerItem, AsyncResult<TResult>>> _tasks = new ConcurrentQueue<KeyValuePair<WorkerItem, AsyncResult<TResult>>>();
        private int _maxWorkers;

        private int _activeWorkers = 0;
        private int ActiveWorkers
        {
            get => _activeWorkers;
            set
            {
                _activeWorkers = value;
                if (value == 0)
                {
                    resetEvent.Set();
                }
            }
        }

        private object locker = new object();

        private AutoResetEvent resetEvent;

        public CustomExecutor(int maxWorkers)
        {
            _maxWorkers = maxWorkers;
            resetEvent = new AutoResetEvent(false);
        }

        /// <summary>
        /// Метод execute(func, args)
        /// </summary>
        public IAsyncResult<TResult> EnqueueTask(WorkerAction<TArgument, TResult> action, TArgument argument)
        {
            var result = enqueueTask(action, argument);
            resetEvent.Reset();
            execute();
            return result;
        }

        /// <summary>
        /// Метод map(func, args_array)
        /// </summary>
        public IEnumerable<IAsyncResult<TResult>> EnqueueTasks(WorkerAction<TArgument, TResult> action, params TArgument[] arguments)
        {
            var results = new List<IAsyncResult<TResult>>();
            foreach (var argument in arguments)
            {
                results.Add(EnqueueTask(action, argument));
            }
            resetEvent.Reset();
            execute(arguments.Length);
            return results;
        }

        private IAsyncResult<TResult> enqueueTask(WorkerAction<TArgument, TResult> action, TArgument argument)
        {
            var result = new AsyncResult<TResult>();
            _tasks.Enqueue(new KeyValuePair<WorkerItem, AsyncResult<TResult>>(new WorkerItem(action, argument), result));
            return result;
        }

        public void ShutDown()
        {
            resetEvent.WaitOne();
        }

        private void execute(int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                if (ActiveWorkers >= _maxWorkers)
                {
                    break;
                }

                ActiveWorkers++;
                var worker = new Worker(_tasks);
                worker.Executed += () =>
                {
                    lock (locker)
                    {
                        ActiveWorkers--;
                    }
                };

                worker.Execute();
            }
        }
    }
}
