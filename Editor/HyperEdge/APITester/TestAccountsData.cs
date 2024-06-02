using System;
using System.Collections.Generic;
using UnityEngine;


namespace HyperEdge.Sdk.Unity.APITester
{
    [Serializable]
    public class AccountData
    {
        public string AccountId = string.Empty;
        public string UserId = string.Empty;
        public string Email = string.Empty;
        public string AddressHex = string.Empty;
        public string PublicKeyHex = string.Empty;
        public string PrivateKeyHex = string.Empty;
    }

    [Serializable]
    public class TestAccountsData : ScriptableObject
    {
        public string EnvId = string.Empty;
        public List<AccountData> Accounts = new();
    }
}
