using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeriodicBatching.Models
{
    public class PeriodicBatchingConfiguration<TEvent>
    {
        public int BatchSizeLimit { get; set; } = 50;
        
        public TimeSpan Period { get; set; } = TimeSpan.FromSeconds(10);

        public Func<List<TEvent>, Task> BatchingFunc { get; set; }

        public int FailuresBeforeDroppingBatch { get; set; } = 5;

        public int FailuresBeforeDroppingQueue { get; set; } = 10;

        public Func<Exception, int, int, Task> SingleFailureCallback { get; set; }

        public Func<List<TEvent>, Task> DropBatchCallback { get; set; }

        public Func<List<TEvent>, Task> DropQueueCallback { get; set; }

        public TimeSpan MinimumBackoffPeriod { get; set; } = TimeSpan.FromSeconds(3);

        public TimeSpan MaximumBackoffInterval { get; set; } = TimeSpan.FromMinutes(5);

        public void Validate()
        {
            if (this.BatchSizeLimit < 0)
            {
                throw new ArgumentOutOfRangeException("BatchSizeLimit must be greater then 0");
            }

            if (this.Period < TimeSpan.FromSeconds(1))
            {
                throw new ArgumentOutOfRangeException($"Period must be greater then or equal to 1 second");
            }

            if (this.MinimumBackoffPeriod < TimeSpan.FromSeconds(1))
            {
                throw new ArgumentOutOfRangeException($"MinimumBackoffPeriod must be greater then or equal to 1 second");
            }

            if (this.MaximumBackoffInterval < this.MinimumBackoffPeriod || this.MaximumBackoffInterval < this.Period)
            {
                throw new ArgumentOutOfRangeException($"MaximumBackoffInterval must be greater then or equal to MinimumBackoffPeriod and Period");
            }

            if (this.BatchingFunc == null)
            {
                throw new ArgumentNullException("BatchingFunc cannot be null");
            }
        }
    }
}
