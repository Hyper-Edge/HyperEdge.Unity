using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

using Nethereum.Hex.HexConvertors.Extensions;


namespace HyperEdge.Sdk.Unity.APITester
{
    public class TestUserData
    {
        public string Email;
        public string AddressHex;
        public string PrivateKey;
    }

    public class TestUserGenerator
    {
        public static TestUserData GenerateNew()
        {
            var ecKey = Nethereum.Signer.EthECKey.GenerateKey();
            var privateKey = ecKey.GetPrivateKeyAsBytes().ToHex();
            var account = new Nethereum.Web3.Accounts.Account(privateKey);
            return new TestUserData
            {
                Email = $"{Ulid.NewUlid().ToString()}@hyperedgelabs.xyz",
                AddressHex = account.Address,
                PrivateKey = privateKey
            };
        }
    }
}
