namespace Lab2
{
    public partial class CustomExecutor<TArgument, TResult>
    {
        private class WorkerItem
        {
            public WorkerAction<TArgument, TResult> Action { get; }
            public TArgument Argument { get; }
            public WorkerItem(WorkerAction<TArgument, TResult> action, TArgument argument)
            {
                Action = action;
                Argument = argument;
            }
        }
    }
}
