using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;


namespace HyperEdge.Sdk.Unity.APITester
{
    public class LocalAccountRepo
    {
        private readonly string _envId;
        private Dictionary<string, AccountData> _accounts = new();
        private TestAccountsData _testAccData = null;

        public IReadOnlyList<AccountData> Accounts { get => _accounts.Values.ToList(); }

        public LocalAccountRepo(string envId)
        {
            _envId = envId;
            //
            LoadAccounts();
            if (_testAccData is null)
            {
                CreateNewTestAccountsData();
            }
        }

        private void CreateNewTestAccountsData()
        {
            _testAccData = ScriptableObject.CreateInstance<TestAccountsData>();
            _testAccData.EnvId = _envId;
            AssetUtils.CreateNewAsset<TestAccountsData>(_testAccData, $"TestAccounts-{_envId}");
        }

        private void LoadAccounts()
        {
            var assets = AssetUtils.FindAssetsByType<TestAccountsData>();
            var testAccData = assets.Find(d => d.EnvId == _envId);
            if (testAccData is null)
            {
                return;
            }
            _testAccData = testAccData;
            foreach (var accData in _testAccData.Accounts)
            {
                _accounts[accData.UserId] = accData;
            }
        }

        public void SaveAccounts()
        {
            EditorUtility.SetDirty(_testAccData);
            AssetDatabase.SaveAssets();
        }

        public AccountData? GetAccountData(string userId)
        {
            if (!_accounts.TryGetValue(userId, out var accData))
            {
                return null;
            }
            return accData;
        }

        public bool AddAccountData(AccountData accData)
        {
            bool succ = _accounts.TryAdd(accData.UserId, accData);
            if (succ)
            {
                _testAccData.Accounts.Add(accData);
                SaveAccounts();
                return true;
            }
            return false;
        }
    }
}
