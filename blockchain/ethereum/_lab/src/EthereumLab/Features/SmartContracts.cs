using Microsoft.AspNetCore.Mvc;
using Nethereum.Hex.HexTypes;
using Nethereum.Model;
using Nethereum.Signer;
using Nethereum.Util;
using Nethereum.Web3;
using Account = Nethereum.Web3.Accounts.Account;

namespace EthereumLab.Features;

public static class SmartContracts
{
    private const string Abi = @"
    [
      {
        ""inputs"": [],
        ""name"": ""get"",
        ""outputs"": [
          {
            ""internalType"": ""uint256"",
            ""name"": """",
            ""type"": ""uint256""
          }
        ],
        ""stateMutability"": ""view"",
        ""type"": ""function""
      },
      {
        ""inputs"": [
          {
            ""internalType"": ""uint256"",
            ""name"": ""x"",
            ""type"": ""uint256""
          }
        ],
        ""name"": ""set"",
        ""outputs"": [],
        ""stateMutability"": ""nonpayable"",
        ""type"": ""function""
      }
    ]";

    public static void Map(WebApplication app)
    {
        app.MapPost("/contracts", async (
            DeployContractRequest request,
            [FromServices] Web3 web3) =>
        {
            var account = new Account(request.PrivateKey);

            var chainId = await web3.Eth.ChainId.SendRequestAsync();
            var nonce = await web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(account.Address);
            var gasPrice = await web3.Eth.GasPrice.SendRequestAsync();

            var transaction = new Transaction1559(
                chainId,
                new HexBigInteger(nonce),
                Web3.Convert.ToWei(1, UnitConversion.EthUnit.Gwei),
                new HexBigInteger(gasPrice),
                new HexBigInteger(200_000),
                null,
                0,
                request.ByteCode,
                null);

            var signer = new Transaction1559Signer();
            var signedTransaction = signer.SignTransaction(request.PrivateKey, transaction);

            var transactionHash = await web3.Eth.Transactions.SendRawTransaction.SendRequestAsync(signedTransaction);

            return new {transactionHash};
        });

        app.MapPost("/contracts/simple-storage/{address}/get", async (
            string address,
            [FromServices] Web3 web3) =>
        {
            var contract = web3.Eth.GetContract(Abi, address);
            var getFunction = contract.GetFunction("get");
            var value = await getFunction.CallAsync<int>();
            return new {value};
        });

        app.MapPost("/contracts/simple-storage/{address}/set", async (
            string address,
            SimpleStorageSetRequest request,
            [FromServices] Web3 web3) =>
        {
            var account = new Account(request.PrivateKey);

            var contract = web3.Eth.GetContract(Abi, address);
            var setFunction = contract.GetFunction("set");

            var chainId = await web3.Eth.ChainId.SendRequestAsync();
            var nonce = await web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(account.Address);
            var gasPrice = await web3.Eth.GasPrice.SendRequestAsync();

            var transactionInput = setFunction.CreateTransactionInput(account.Address, null, null, request.Value);
            var transaction = new Transaction1559(
                chainId,
                new HexBigInteger(nonce),
                Web3.Convert.ToWei(1, UnitConversion.EthUnit.Gwei),
                gasPrice,
                new HexBigInteger(50000),
                address,
                0,
                transactionInput.Data,
                null);

            var signer = new Transaction1559Signer();
            var signedTransaction = signer.SignTransaction(request.PrivateKey, transaction);

            var transactionHash = await web3.Eth.Transactions.SendRawTransaction.SendRequestAsync(signedTransaction);

            return new {transactionHash};
        });
    }

    public record DeployContractRequest(string PrivateKey, string ByteCode);

    public record SimpleStorageSetRequest(string PrivateKey, int Value);
}
