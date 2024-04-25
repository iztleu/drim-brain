using System.Collections.Concurrent;
using System.Threading.Channels;

namespace BankingService.Features.Deposits.Services;

public class DepositsStream
{
    private readonly ConcurrentDictionary<int, Channel<BlockchainService.Client.DepositDto>> _userChannels = new();

    public async Task PublishDeposit(BlockchainService.Client.DepositDto deposit)
    {
        if (_userChannels.TryGetValue(deposit.UserId, out var channel))
        {
            await channel.Writer.WriteAsync(deposit);
        }
    }

    public ChannelReader<BlockchainService.Client.DepositDto> Subscribe(int userId)
    {
        var channel = Channel.CreateUnbounded<BlockchainService.Client.DepositDto>();
        _userChannels.TryAdd(userId, channel);

        return channel.Reader;
    }

    public void Unsubscribe(int userId)
    {
        if (_userChannels.TryRemove(userId, out var channel))
        {
            channel.Writer.Complete();
        }
    }
}
