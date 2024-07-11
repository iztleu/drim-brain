using System.Threading.Channels;

namespace Channels;

public static class ChannelBackpressure
{
    public static async Task Run()
    {
        var cts = new CancellationTokenSource();

        //var channel = Channel.CreateUnbounded<int>();
        // var channel = Channel.CreateBounded<int>(new BoundedChannelOptions(20)
        // {
        //     FullMode = BoundedChannelFullMode.Wait,
        // });
        // var channel = Channel.CreateBounded<int>(new BoundedChannelOptions(20)
        // {
        //     FullMode = BoundedChannelFullMode.DropNewest,
        // });
        // var channel = Channel.CreateBounded<int>(new BoundedChannelOptions(20)
        // {
        //     FullMode = BoundedChannelFullMode.DropOldest,
        // });
        var channel = Channel.CreateBounded<int>(new BoundedChannelOptions(20)
        {
            FullMode = BoundedChannelFullMode.DropWrite,
        });

        var producerTask = Task.Run(async () =>
        {
            var i = 0;
            while (!cts.Token.IsCancellationRequested)
            {
                await channel.Writer.WriteAsync(i++);
                await Task.Delay(100);
            }

            channel.Writer.Complete();
        });

        var sizeTask = Task.Run(async () =>
        {
            while (!cts.Token.IsCancellationRequested)
            {
                await Task.Delay(5000);
                Console.WriteLine($"Channel size is {channel.Reader.Count}");
            }
        });

        var consumerTask = Task.Run(async () =>
        {
            await foreach (var item in channel.Reader.ReadAllAsync())
            {
                Console.WriteLine($"Consumed {item}");
                await Task.Delay(500);
            }
        });

        Console.ReadLine();

        await cts.CancelAsync();
        cts.Dispose();

        await Task.WhenAll(producerTask, sizeTask, consumerTask);
    }
}
