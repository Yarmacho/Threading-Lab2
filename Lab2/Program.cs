using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lab2
{
    class Program
    {
        static void Main(string[] args)
        {
            var executor = new CustomExecutor<int, string>(2);
            var results = executor.EnqueueTasks(x =>
            {
                Thread.Sleep(2000);
                return x.ToString();
            }, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10);

            foreach (var res in results)
            {
                Console.WriteLine("{0} - {1}", DateTime.Now.ToString(), res.Result);
            }

            Console.WriteLine(new string('-', 50));
            results = executor.EnqueueTasks(x =>
            {
                Thread.Sleep(2000);
                return x.ToString();
            }, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10);

            executor.ShutDown();
            foreach (var res in results)
            {
                Console.WriteLine("{0} - {1}", DateTime.Now.ToString(), res.Result);
            }
            Console.ReadKey();
        }
    }
}
