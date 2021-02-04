using PeriodicBatching.Models;
using System;

namespace PeriodicBatching
{
    internal class BatchedConnectionStatus<TEvent>
    {
        private PeriodicBatchingConfiguration<TEvent> PeriodicBatchingConfiguration { get; set; }

        public int CurrentFailuresSinceSuccessfulBatch { get; private set; }

        public BatchedConnectionStatus(PeriodicBatchingConfiguration<TEvent> config)
        {
            this.PeriodicBatchingConfiguration = config;
        }

        public void MarkSuccess()
        {
            this.CurrentFailuresSinceSuccessfulBatch = 0;
        }

        public void MarkFailure()
        {
            ++this.CurrentFailuresSinceSuccessfulBatch;
        }

        public TimeSpan NextInterval
        {
            get
            {
                if (CurrentFailuresSinceSuccessfulBatch <= 1)
                {
                    return this.PeriodicBatchingConfiguration.Period;
                }

                var backoffFactor = Math.Pow(2, (this.CurrentFailuresSinceSuccessfulBatch - 1));
                var backoffPeriod = this.PeriodicBatchingConfiguration.MinimumBackoffPeriod.Ticks;
                var backedOff = (long)(backoffPeriod * backoffFactor);
                var cappedBackoff = Math.Min(this.PeriodicBatchingConfiguration.MaximumBackoffInterval.Ticks, backedOff);
                var actual = Math.Max(this.PeriodicBatchingConfiguration.Period.Ticks, cappedBackoff);

                return TimeSpan.FromTicks(actual);
            }
        }

        public bool ShouldDropBatch => this.CurrentFailuresSinceSuccessfulBatch >= this.PeriodicBatchingConfiguration.FailuresBeforeDroppingBatch
                                       && this.PeriodicBatchingConfiguration.FailuresBeforeDroppingBatch >= 0;

        public bool ShouldDropQueue => this.CurrentFailuresSinceSuccessfulBatch >= this.PeriodicBatchingConfiguration.FailuresBeforeDroppingQueue
                                       && this.PeriodicBatchingConfiguration.FailuresBeforeDroppingQueue >= 0;
    }
}
