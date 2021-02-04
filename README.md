[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=ThiagoBarradas_periodic-batching&metric=alert_status)](https://sonarcloud.io/dashboard?id=ThiagoBarradas_periodic-batching)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=ThiagoBarradas_periodic-batching&metric=coverage)](https://sonarcloud.io/dashboard?id=ThiagoBarradas_periodic-batching)
[![NuGet Downloads](https://img.shields.io/nuget/dt/PeriodicBatching.svg)](https://www.nuget.org/packages/PeriodicBatching/)
[![NuGet Version](https://img.shields.io/nuget/v/PeriodicBatching.svg)](https://www.nuget.org/packages/PeriodicBatching/)

# Periodic Batching

Execute a batching function with a controlled size every some specified time interval. Very useful for background processes to send events to some broker or something like it. Based on serilog algorithm, refactored with a little customization.

# Sample

```c#

// some class 
public class MyEvent
{
	public string SomeProperty { get; set; }
}

// method that will be called every X seconds/minutes/etc
public static async Task ExecuteMethod(List<MyEvent> events)
{
	// do something
}

// configuring periodic batching
var config = new PeriodicBatchingConfiguration<MyEvent>
{
    BatchSizeLimit = 5,                               // list size for when BatchingFunc method is called. default 50.
    FailuresBeforeDroppingBatch = 3,                  // after X consecutive failures, current batch will be dropped. Use -1 to cancel this behavior. default 5.
    FailuresBeforeDroppingQueue = 10,                 // after X consecutive failures, all queue items will be dropped. Use -1 to cancel this behavior. deafult 10.
    BatchingFunc = ExecuteMethod,                     // (required) delegate called to process batch
    SingleFailureCallback = SingleFailure,            // (optional) delegate called when a single failure happens - exception and failure count are passed as parameter.
    DropBatchCallback = DropBatch,                    // (optional) delegate called when a batch is dropped - current batch items are passed as parameter.
    DropQueueCallback = DropQueue,                    // (optional) delegate called when a queue is dropped - all queue items are passed as parameter.
    Period = TimeSpan.FromSeconds(5),                 // interval to execute main method - BatchingFunc. default 10s.
    MinimumBackoffPeriod = TimeSpan.FromSeconds(3),   // interval used for next inteval when a failure happens. for each error, interval is increased exponentially based on this value. default 3s.
    MaximumBackoffInterval = TimeSpan.FromMinutes(5), // max interval. this parameter will limit the exponential interval. default 5m.
};

IPeriodicBatching<MyEvent> periodicBatching = new PeriodicBatching<MyEvent>(config);

// or 

IPeriodicBatching<MyEvent> periodicBatchingOtherWay = new PeriodicBatching<MyEvent>();
periodicBatchingOtherWay.Setup(config);

// sample of other delegates

public static async Task SingleFailure(Exception e, int failures)
{
	// do something
}

public static async Task DropBatch(List<SomeEvent> events)
{
	// do something
}

public static async Task DropQueue(List<SomeEvent> events)
        {
	// do something
}

```

## Install via NuGet

```
PM> Install-Package PeriodicBatching
```

## How can I contribute?

Please, refer to [CONTRIBUTING](.github/CONTRIBUTING.md)

## Found something strange or need a new feature?

Open a new Issue following our issue template [ISSUE_TEMPLATE](.github/ISSUE_TEMPLATE.md)

## Did you like it? Please, make a donate :)

if you liked this project, please make a contribution and help to keep this and other initiatives, send me some Satochis.

BTC Wallet: `1G535x1rYdMo9CNdTGK3eG6XJddBHdaqfX`

![1G535x1rYdMo9CNdTGK3eG6XJddBHdaqfX](https://i.imgur.com/mN7ueoE.png)
