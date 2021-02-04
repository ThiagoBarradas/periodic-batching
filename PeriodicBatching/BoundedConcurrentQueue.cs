using System;
using System.Collections.Concurrent;
using System.Threading;

namespace PeriodicBatching
{
    internal class BoundedConcurrentQueue<T>
    {
        const int NON_BOUNDED = -1;

        readonly ConcurrentQueue<T> Queue = new ConcurrentQueue<T>();

        readonly int QueueLimit;

        int Counter;

        public BoundedConcurrentQueue()
        {
            this.QueueLimit = NON_BOUNDED;
        }

        public BoundedConcurrentQueue(int queueLimit)
        {
            if (queueLimit <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(queueLimit), "queue limit must be positive");
            }

            this.QueueLimit = queueLimit;
        }

        public int Count => this.Queue.Count;

        public bool TryDequeue(out T item)
        {
            if (this.QueueLimit == NON_BOUNDED)
            {
                return this.Queue.TryDequeue(out item);
            }

            if (this.Queue.TryDequeue(out item))
            {
                Interlocked.Decrement(ref this.Counter);
                return true;
            }

            return false;
        }

        public bool TryEnqueue(T item)
        {
            if (this.QueueLimit == NON_BOUNDED || Interlocked.Increment(ref this.Counter) <= this.QueueLimit)
            {
                this.Queue.Enqueue(item);
                return true;
            }

            Interlocked.Decrement(ref this.Counter);
            return false;
        }
    }
}
