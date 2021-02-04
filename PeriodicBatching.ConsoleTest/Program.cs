using PeriodicBatching.Interfaces;
using PeriodicBatching.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PeriodicBatching.ConsoleTest
{
    class Program
    {
        public static async Task ExecuteMethod(List<SomeEvent> events)
        {
            await Task.Delay(500);

            Console.WriteLine("#################### PROCESSING START {0}", DateTime.Now.ToString("yyyy-MM-ss HH:mm:ss"));

            foreach (var _event in events)
            {
                if (_event.Prop2 == "error")
                {
                    throw new Exception();
                }

                Console.WriteLine("####################");
                Console.WriteLine(_event.Prop1);
                Console.WriteLine(_event.Prop2);
            }

            Console.WriteLine("#################### PROCESSING FINISH");
        }

        public static async Task SingleFailure(Exception e, int failures)
        {
            await Task.Delay(50);

            Console.WriteLine("#################### FAILURES {0} - {1}", failures, DateTime.Now.ToString("yyyy-MM-ss HH:mm:ss"));
            Console.WriteLine(e.Message);
        }

        public static async Task DropBatch(List<SomeEvent> events)
        {
            await Task.Delay(50);

            Console.WriteLine("#################### DropBatch START {0}", DateTime.Now.ToString("yyyy-MM-ss HH:mm:ss"));

            foreach (var _event in events)
            {
                Console.WriteLine("####################");
                Console.WriteLine(_event.Prop1);
                Console.WriteLine(_event.Prop2);
            }

            Console.WriteLine("#################### DropBatch FINISH");
        }

        public static async Task DropQueue(List<SomeEvent> events)
        {
            await Task.Delay(50);

            Console.WriteLine("#################### DropQueue START {0}", DateTime.Now.ToString("yyyy-MM-ss HH:mm:ss"));

            foreach (var _event in events)
            {
                Console.WriteLine("####################");
                Console.WriteLine(_event.Prop1);
                Console.WriteLine(_event.Prop2);
            }

            Console.WriteLine("#################### DropQueue FINISH");
        }

        static void Main(string[] args)
        {
            var config = new PeriodicBatchingConfiguration<SomeEvent>
            {
                BatchSizeLimit = 5,
                FailuresBeforeDroppingBatch = 3,
                FailuresBeforeDroppingQueue = 10,
                BatchingFunc = ExecuteMethod,
                SingleFailureCallback = SingleFailure,
                DropBatchCallback = DropBatch,
                DropQueueCallback = DropQueue,
                Period = TimeSpan.FromSeconds(5),
                MinimumBackoffPeriod = TimeSpan.FromSeconds(3),
                MaximumBackoffInterval = TimeSpan.FromMinutes(5),
            };

            IPeriodicBatching<SomeEvent> periodicBatching = new PeriodicBatching<SomeEvent>(config);

            int i = 0;
            int max = 100;
            string command = "error";
            while (true)
            {
                if (i < max)
                {
                    periodicBatching.Add(new SomeEvent { Prop1 = i.ToString(), Prop2 = command });
                    Thread.Sleep(100);
                    i++;
                }
                else
                {
                    Thread.Sleep(5000);
                }
            }
        }
    }
}
