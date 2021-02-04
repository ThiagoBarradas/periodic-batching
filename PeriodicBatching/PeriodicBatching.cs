using PeriodicBatching.Interfaces;
using PeriodicBatching.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PeriodicBatching
{
    public class PeriodicBatching<TEvent> : IPeriodicBatching<TEvent>
    {
        private int BatchSizeLimit;

        private bool IsAlreadyInitialized => (this.BatchingFunc != null);

        private BoundedConcurrentQueue<TEvent> Queue;

        private PortableTimer Timer;

        private readonly List<TEvent> WaitingBatch = new List<TEvent>();

        private readonly object StateLock = new object();

        private bool IsUnloading;

        private bool IsStarted;

        private BatchedConnectionStatus<TEvent> Status;

        private PeriodicBatchingConfiguration<TEvent> PeriodicBatchingConfiguration { get; set; }

        public Func<List<TEvent>, Task> BatchingFunc { get; private set; }

        public PeriodicBatching() { }

        public PeriodicBatching(PeriodicBatchingConfiguration<TEvent> config)
        {
            this.Setup(config);
        }

        public void Setup(PeriodicBatchingConfiguration<TEvent> config)
        {
            if (this.IsAlreadyInitialized)
            {
                throw new InvalidOperationException("Setup cannot be called two times");
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            config.Validate();

            this.PeriodicBatchingConfiguration = config;
            this.Status = new BatchedConnectionStatus<TEvent>(config);
            this.BatchingFunc = config.BatchingFunc;
            this.BatchSizeLimit = config.BatchSizeLimit;
            this.Queue = new BoundedConcurrentQueue<TEvent>();
            this.Timer = new PortableTimer(cancel => OnTick());
        }

        async Task OnTick()
        {
            try
            {
                bool batchWasFull = false;

                do
                {
                    while (this.WaitingBatch.Count < this.BatchSizeLimit && this.Queue.TryDequeue(out TEvent next))
                    {
                        this.WaitingBatch.Add(next);
                    }

                    if (this.WaitingBatch.Count == 0)
                    {
                        return;
                    }

                    await this.BatchingFunc.Invoke(this.WaitingBatch);

                    batchWasFull = this.WaitingBatch.Count >= this.BatchSizeLimit;

                    this.WaitingBatch.Clear();
                    this.WaitingBatch.TrimExcess();
                    this.Status.MarkSuccess();
                    GC.Collect();
                    // Success
                }
                while (batchWasFull);
            }
            catch (Exception e)
            {
                this.Status.MarkFailure();
                
                await this.PeriodicBatchingConfiguration.SingleFailureCallback?.Invoke(e, 
                    this.Status.CurrentFailuresSinceSuccessfulBatch,
                    this.Queue.Count);   
            }
            finally
            {
                if (this.Status.ShouldDropBatch)
                {
                    await this.PeriodicBatchingConfiguration?.DropBatchCallback(this.WaitingBatch);
                    this.WaitingBatch.Clear();
                }

                if (this.Status.ShouldDropQueue)
                {
                    var currentEvents = new List<TEvent>();
                    while (this.Queue.TryDequeue(out TEvent _event)) 
                    {
                        if (this.PeriodicBatchingConfiguration.DropQueueCallback != null)
                        {
                            currentEvents.Add(_event);
                        }
                    }
                    await this.PeriodicBatchingConfiguration?.DropQueueCallback(currentEvents);
                }

                lock (this.StateLock)
                {
                    if (!this.IsUnloading)
                    {
                        this.Timer.Start(this.Status.NextInterval);
                    }
                }
            }
        }

        public void Dispose()
        {
            this.CloseAndFlush();
        }

        void CloseAndFlush()
        {
            lock (this.StateLock)
            {
                if (!this.IsStarted || this.IsUnloading)
                {
                    return;
                }

                this.IsUnloading = true;
            }

            this.Timer.Dispose();
            this.ResetSyncContextAndWait(OnTick);
        }

        void ResetSyncContextAndWait(Func<Task> taskFactory)
        {
            var prevContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(null);

            try
            {
                taskFactory().Wait();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(prevContext);
            }
        }

        public void Add(TEvent _event)
        {
            if (_event == null)
            {
                throw new ArgumentNullException(nameof(_event));
            }

            if (this.IsUnloading)
            {
                return;
            }

            if (!IsStarted)
            {
                lock (this.StateLock)
                {
                    if (this.IsUnloading)
                    {
                        return;
                    }

                    if (!this.IsStarted)
                    {
                        this.Queue.TryEnqueue(_event);
                        this.IsStarted = true;
                        this.Timer.Start(TimeSpan.Zero);
                        return;
                    }
                }
            }

            this.Queue.TryEnqueue(_event);
        }

        public async void Flush()
        {
            await this.OnTick();
        }
    }
}
