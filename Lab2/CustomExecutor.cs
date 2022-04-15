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
        private object locker = new object();

        public CustomExecutor(int maxWorkers)
        {
            _maxWorkers = maxWorkers;
        }

        /// <summary>
        /// Метод execute(func, args)
        /// </summary>
        public IAsyncResult<TResult> EnqueueTask(WorkerAction<TArgument, TResult> action, TArgument argument)
        {
            var result = enqueueTask(action, argument);
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
            while (_activeWorkers != 0)
            {
                Thread.Sleep(200);
            }
        }

        private void execute(int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                if (_activeWorkers >= _maxWorkers)
                {
                    break;
                }

                _activeWorkers++;
                var worker = new Worker(_tasks);
                worker.Executed += () =>
                {
                    lock (locker)
                    {
                        _activeWorkers--;
                    }
                };

                worker.Execute();
            }
        }
    }
}
