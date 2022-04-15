using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Lab2
{
    public partial class CustomExecutor<TArgument, TResult>
    {
        private class Worker
        {
            private ConcurrentQueue<KeyValuePair<WorkerItem, AsyncResult<TResult>>> _tasks;
            public Worker(ConcurrentQueue<KeyValuePair<WorkerItem, AsyncResult<TResult>>> tasks)
            {
                _tasks = tasks;
            }
            public event Action Executed;

            public void Execute()
            {
                new Thread(() =>
                {
                    while (_tasks.TryDequeue(out var task))
                    {
                        if (task.Key?.Action == null)
                        {
                            task.Value.Result = default;
                            Executed?.Invoke();
                            throw new NullReferenceException();
                        }
                        task.Value.Result = task.Key.Action(task.Key.Argument);
                        task.Value.WaitHandler.Set();
                    }
                    Executed?.Invoke();
                }).Start();
            }
        }
    }
}
