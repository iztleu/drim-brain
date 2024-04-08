# Reactive Programming

Reactive programming is an advanced programming paradigm centered around data streams and the propagation of change. It focuses on building systems that are responsive, resilient, and flexible to changes by treating data as asynchronous streams that can be observed, manipulated, and propagated through different parts of an application. This approach allows for a more declarative style of programming, where developers define what should happen in response to changes in data streams, rather than how to handle those changes step by step.

## Key Concepts

* __Observable Streams__: At the heart of reactive programming are observables, which represent sources of data or events that can change over time. These streams can emit zero or more items over their lifespan and can represent virtually anything, from variable changes and user inputs to messages from external systems.

* __Observers/Subscribers__: Observers (or subscribers) listen to observables, reacting to new data or changes in the existing data. When an observable emits a new item, it's propagated to all its subscribers, triggering them to update accordingly.

* __Operators__: Reactive programming libraries provide a rich set of operators that allow developers to create, filter, transform, combine, and otherwise manipulate observable streams. Operators like `map`, `filter`, `reduce`, and `merge` enable complex asynchronous code to be written more declaratively and concisely.

* __Backpressure__: This is a critical concept in reactive systems, referring to the ability to manage and control the flow of data between producers and consumers to prevent overwhelming the system. Reactive libraries offer mechanisms to deal with backpressure gracefully, ensuring that systems can remain responsive even under heavy load.

## Advantages

* __Improved Scalability and Responsiveness__: By treating data as asynchronous streams, reactive programming can improve an application's scalability and responsiveness, making it better suited to handle real-time updates and asynchronous operations.

* __Declarative Code__: The use of high-level operators for transforming and combining streams can lead to more declarative, understandable, and maintainable code.

* __Better Error Handling__: Reactive programming provides structured ways to handle errors that occur in asynchronous streams, allowing for more robust applications.

* __Easier Management of Side Effects__: By centralizing how data changes are propagated through the system, reactive programming makes it easier to manage side effects, leading to cleaner and more predictable code.

## Applications

Reactive programming is particularly well-suited for applications that require real-time data updates, including UIs, real-time data feeds, and microservices architectures. It's widely used in web development, financial trading systems, telecommunications, and anywhere else that systems need to remain responsive under varying loads or require real-time data processing.

## Implementations

There are many implementations of reactive programming across different programming languages, such as RxJS for JavaScript, RxJava for Java, ReactiveX for other languages, and many more. Each implementation adheres to the basic principles of reactive programming while offering utilities that cater to the specifics of its host language.

## Conclusion

Reactive programming represents a shift from traditional imperative and sequential programming models to a more dynamic, event-driven approach. It encourages building systems that are more robust, efficient, and easier to maintain, particularly when dealing with real-time data and asynchronous processes.

## Examples

### .NET

```csharp
using System;
using System.Reactive.Linq;
using System.Threading;

namespace ReactiveProgrammingComplexExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var rand = new Random();

            // Simulate stock price updates every second
            var stockPriceUpdates = Observable.Interval(TimeSpan.FromSeconds(1))
                .Select(_ => 100 + rand.NextDouble() * 10) // Simulate price change
                .Publish(); // Use Publish to share a single subscription to the underlying stream

            // Log every price change
            var loggingSubscription = stockPriceUpdates
                .Subscribe(price => Console.WriteLine($"New price: {price:0.00}"));

            // Alert for significant drops (simulate a 5% drop alert)
            var alertSubscription = stockPriceUpdates
                .Buffer(2, 1) // Look at the last 2 prices to calculate the change
                .Where(prices => prices[0] > prices[1] * 1.05) // More than 5% drop
                .Subscribe(prices => Console.WriteLine($"Alert: Significant drop from {prices[0]:0.00} to {prices[1]:0.00}"));

            // Calculate and display the average price every 5 seconds
            var averageSubscription = stockPriceUpdates
                .Buffer(TimeSpan.FromSeconds(5))
                .Where(prices => prices.Any())
                .Select(prices => prices.Average())
                .Subscribe(avg => Console.WriteLine($"Average price over the last 5 seconds: {avg:0.00}"));

            // Connect to start the hot observable sequence
            stockPriceUpdates.Connect();

            Console.WriteLine("Stock ticker is running. Press any key to exit...");
            Console.ReadKey();

            // Clean up
            loggingSubscription.Dispose();
            alertSubscription.Dispose();
            averageSubscription.Dispose();
        }
    }
}
```

The same logic in imperative code:

```csharp
using System;
using System.Collections.Generic;
using System.Threading;

namespace ImperativeProgrammingExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var stockTicker = new StockTicker();

            // Subscribe handlers
            stockTicker.PriceChanged += LogPriceChange;
            stockTicker.PriceChanged += CheckSignificantDrop;
            stockTicker.PriceChanged += CalculateAveragePrice;

            stockTicker.Start();

            Console.WriteLine("Stock ticker is running. Press any key to exit...");
            Console.ReadKey();

            // Unsubscribe handlers and stop the ticker
            stockTicker.PriceChanged -= LogPriceChange;
            stockTicker.PriceChanged -= CheckSignificantDrop;
            stockTicker.PriceChanged -= CalculateAveragePrice;
            stockTicker.Stop();
        }

        private static void LogPriceChange(object sender, double price)
        {
            Console.WriteLine($"New price: {price:0.00}");
        }

        private static void CheckSignificantDrop(object sender, double price)
        {
            var ticker = sender as StockTicker;
            if (ticker == null) return;

            // Check for a significant drop
            if (ticker.PreviousPrice > price * 1.05)
            {
                Console.WriteLine($"Alert: Significant drop from {ticker.PreviousPrice:0.00} to {price:0.00}");
            }
        }

        private static List<double> priceBuffer = new List<double>();
        private static DateTime lastAverageTime = DateTime.Now;
        private static void CalculateAveragePrice(object sender, double price)
        {
            priceBuffer.Add(price);

            if ((DateTime.Now - lastAverageTime).TotalSeconds >= 5)
            {
                var average = 0.0;
                if (priceBuffer.Count > 0)
                {
                    average = priceBuffer.Average();
                    priceBuffer.Clear();
                }
                Console.WriteLine($"Average price over the last 5 seconds: {average:0.00}");
                lastAverageTime = DateTime.Now;
            }
        }
    }

    public class StockTicker
    {
        private Timer timer;
        private Random rand = new Random();
        public double PreviousPrice { get; private set; }

        public event EventHandler<double> PriceChanged;

        public void Start()
        {
            timer = new Timer(Tick, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }

        public void Stop()
        {
            timer?.Dispose();
        }

        private void Tick(object state)
        {
            var newPrice = 100 + rand.NextDouble() * 10;
            PreviousPrice = newPrice;
            PriceChanged?.Invoke(this, newPrice);
        }
    }
}
```

#reactive-programming
