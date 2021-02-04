using PeriodicBatching.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeriodicBatching.Interfaces
{
    public interface IPeriodicBatching<TEvent> : IDisposable
    {
        Func<List<TEvent>, Task> BatchingFunc { get; }

        void Setup(PeriodicBatchingConfiguration<TEvent> config);

        void Add(TEvent _event);

        void Flush();
    }
}
