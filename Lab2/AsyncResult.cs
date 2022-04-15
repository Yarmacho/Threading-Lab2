using System.Threading;

namespace Lab2
{
    public interface IAsyncResult<T> 
    {
        T Result { get; }

        bool IsCompleted { get; }

        EventWaitHandle WaitHandler { get; }
    }

    internal class AsyncResult<T> : IAsyncResult<T>
    {
        private T _result;
        public T Result 
        {
            get
            {
                WaitHandler.WaitOne();
                return _result;
            }
            set
            {
                _result = value;
                IsCompleted = true;
            }
        }

        public bool IsCompleted { get; set; }

        public EventWaitHandle WaitHandler { get; } = new AutoResetEvent(false);
    }
}
