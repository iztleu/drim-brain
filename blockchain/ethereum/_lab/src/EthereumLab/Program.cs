using EthereumLab;
using EthereumLab.Features;
using Microsoft.Extensions.Options;
using Nethereum.Web3;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<Web3Options>(builder.Configuration.GetSection("Web3"));
builder.Services.AddSingleton(provider =>
{
    var options = provider.GetRequiredService<IOptions<Web3Options>>().Value;
    return new Web3(options.Url);
});

var app = builder.Build();

Basics.Map(app);
SmartContracts.Map(app);
Erc20Contracts.Map(app);

app.Run();
