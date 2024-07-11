using System.Globalization;
using Nethereum.Hex.HexTypes;

namespace EthereumLab;

public static class Extensions
{
    public static int FromHex(this HexBigInteger hexBigInteger) =>
        int.Parse(hexBigInteger.HexValue[2..], NumberStyles.HexNumber);
}
