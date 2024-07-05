using EthereumLab;
using Microsoft.AspNetCore.Mvc;
using Nethereum.Hex.HexTypes;
using Nethereum.Model;
using Nethereum.Signer;
using Nethereum.Util;
using Nethereum.Web3;
using Account = Nethereum.Web3.Accounts.Account;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(new Web3("http://138.201.247.46:8545"));

var app = builder.Build();

app.MapGet("/info", async(
    [FromServices] Web3 web3) =>
{
    var blockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
    var gasPrice = await web3.Eth.GasPrice.SendRequestAsync();
    var chainId = await web3.Eth.ChainId.SendRequestAsync();
    return new
    {
        BlockNumber = blockNumber.FromHex(),
        GasPriceInEther = Web3.Convert.FromWei(gasPrice),
        ChainId = chainId.FromHex(),
    };
});

app.MapPost("/accounts/keystore", async (
    CreateAccountRequest request,
    [FromServices] Web3 web3) =>
{
    var account = await web3.Personal.NewAccount.SendRequestAsync(request.Passphrase);
    return new {account};
});

app.MapGet("/accounts/{account}/balance", async (
    string account,
    [FromServices] Web3 web3) =>
{
    var balance = await web3.Eth.GetBalance.SendRequestAsync(account);
    var balanceInEther = Web3.Convert.FromWei(balance.Value);
    return new {balanceInEther};
});

app.MapPost("/transactions", async (
    SendTransactionRequest request,
    [FromServices] Web3 web3) =>
{
    var unlockAccountResult = await web3.Personal.UnlockAccount.SendRequestAsync(request.From, request.Passphrase, 60);
    if (!unlockAccountResult)
    {
        // TODO: Use validation exception
        throw new Exception("Failed to unlock account");
    }

    try
    {
        var transactionHash = await web3.Eth.TransactionManager.SendTransactionAsync(request.From, request.To,
            new HexBigInteger(Web3.Convert.ToWei(request.ValueInEther)));

        return new {transactionHash};
    }
    finally
    {
        await web3.Personal.LockAccount.SendRequestAsync(request.From);
    }
});

app.MapGet("/transactions/{hash}", async (
    string hash,
    [FromServices] Web3 web3) =>
{
    var transaction = await web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(hash);
    return new
    {
        transaction.From,
        transaction.To,
        ValueInEther = Web3.Convert.FromWei(transaction.Value),
        transaction.Gas.HexValue,
        GasPriceInEther = Web3.Convert.FromWei(transaction.GasPrice),
        Nonce = transaction.Nonce.FromHex(),
        TransactionIndex = transaction.TransactionIndex.FromHex(),
        transaction.BlockHash,
        BlockNumber = transaction.BlockNumber.FromHex(),
        BlockNumberInHex = transaction.BlockNumber.HexValue,
        Type = transaction.Type.FromHex(),
        transaction.R,
        transaction.S,
    };
});

app.MapGet("/blocks/{number}", async (
    string number,
    [FromServices] Web3 web3) =>
{
    var block = await web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(new HexBigInteger(number));
    return new
    {
        block.Number,
        block.ParentHash,
        block.Nonce,
        block.Sha3Uncles,
        block.LogsBloom,
        block.TransactionsRoot,
        block.StateRoot,
        block.ReceiptsRoot,
        block.Miner,
        Difficulty = block.Difficulty.HexValue,
        TotalDifficulty = block.TotalDifficulty.HexValue,
        block.ExtraData,
        Size = block.Size.HexValue,
        GasLimit = block.GasLimit.HexValue,
        GasUsed = block.GasUsed.HexValue,
        Timestamp = block.Timestamp.HexValue,
        TransactionHashes = block.Transactions.Select(x => x.TransactionHash),
    };
});

app.MapPost("/accounts", () =>
{
    var ecKey = EthECKey.GenerateKey();

    var address = ecKey.GetPublicAddress();
    var privateKey = ecKey.GetPrivateKey();

    return new {address, privateKey};
});

app.MapPost("/transactions/sign", async (
    SignTransactionRequest request,
    [FromServices] Web3 web3) =>
{
    var account = new Account(request.PrivateKey);

    var chainId = await web3.Eth.ChainId.SendRequestAsync();
    var nonce = await web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(account.Address);
    var gasPrice = await web3.Eth.GasPrice.SendRequestAsync();

    var transaction = new Transaction1559(
        chainId,
        new HexBigInteger(nonce),
        Web3.Convert.ToWei(10, UnitConversion.EthUnit.Gwei),
        new HexBigInteger(gasPrice),
        new HexBigInteger(21000),
        request.To,
        new HexBigInteger(Web3.Convert.ToWei(request.ValueInEther)),
        null,
        null);

    var signer = new Transaction1559Signer();
    var signedTransaction = signer.SignTransaction(request.PrivateKey, transaction);

    return signedTransaction;
});

app.MapPost("/transactions/send-raw", async (
    SendRawTransactionRequest request,
    [FromServices] Web3 web3) =>
{
    var transactionHash = await web3.Eth.Transactions.SendRawTransaction.SendRequestAsync(request.RawTransaction);
    return new { transactionHash };
});

app.Run();

public record CreateAccountRequest(string Passphrase);

public record SendTransactionRequest(string From, string To, decimal ValueInEther, string Passphrase);

public record SignTransactionRequest(string PrivateKey, string To, decimal ValueInEther);

public record SendRawTransactionRequest(string RawTransaction);
