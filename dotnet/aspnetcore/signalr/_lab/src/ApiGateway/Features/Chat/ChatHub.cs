using Microsoft.AspNetCore.SignalR;

namespace ApiGateway.Features.Chat;

public class ChatHub : Hub<IChatClient>
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.ReceiveMessage(user, message);
        //await Clients.Others.ReceiveMessage(user, message);
    }
}

public interface IChatClient
{
    Task ReceiveMessage(string user, string message);
}
