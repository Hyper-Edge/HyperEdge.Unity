using System;
using System.Threading.Tasks;
using UnityEngine;
using Nethereum.Signer;

using HyperEdge.Sdk.Shared.Protocol;


namespace HyperEdge.Sdk.Unity.APITester
{
    public class AuthHelper
    {
        private readonly ServerClient _serverClient;
        private readonly LocalAccountRepo _accountRepo;

        public AuthHelper(
            ServerClient serverClient,
            LocalAccountRepo accountRepo)
        {
            _serverClient= serverClient;
            _accountRepo = accountRepo;
        }

        public async Task<AccountData?> SwitchAccount(string userId)
        {
            var accData = _accountRepo.GetAccountData(userId);
            if (accData is null)
            {
                return null;
            }
            Debug.Log($"Trying to sign-in UserId={accData.UserId.ToString()}");
            var authResult = await this.SignInAsync(accData);
            Debug.Log($"Sign-in Success={authResult.Success}");
            if (authResult.Success)
            {
                AuthTokenStorage.Current.Update(accData.UserId, authResult.Token, authResult.Expiration);
                return accData;
            }
            return null;
        }

        public bool AddNewAccount(AccountData accData)
        {
            return _accountRepo.AddAccountData(accData);
        }
    
        public async Task<AccountData?> CreateNewAccountAsync()
        {
            var client = _serverClient.GetAccountService();

            var testUserData = TestUserGenerator.GenerateNew();
            var signer = new EthereumMessageSigner();
            //var msg = $"{testUserData.Email}:{testUserData.AddressHex}";

            var regAccReq = new RegisterAccountRequest
            {
                AddressHex = testUserData.AddressHex,
                Email = testUserData.Email,
                Signature = string.Empty,
                DeviceUId = SystemInfo.deviceUniqueIdentifier
            };
            //
            Debug.Log($"Sending request... {regAccReq.DeviceUId}");
            var regAccResp = await client.RegisterAccount(regAccReq);
            if (!regAccResp.Success)
            {
                Debug.Log("SignUp Failure");
                return null;
            }
            Debug.Log($"SignedUp as {regAccResp.Account.Id.ToString()}; UserId={regAccResp.UserId.ToString()}");
            //
            var accData = new AccountData
            {
                Email = testUserData.Email,
                AccountId = regAccResp.Account.Id.ToString(),
                UserId = regAccResp.UserId.ToString(),
                AddressHex = regAccResp.Account.AddressHex,
                PrivateKeyHex = testUserData.PrivateKey
            };
            //
            AddNewAccount(accData);
            return accData;
        }

        public async Task<ValidateChallengeResponse> SignInAsync(AccountData accData)
        {
            var client = _serverClient.GetAuthService();
            var getChallengeReq = new GetLoginChallengeRequest
            {
                UserId = Ulid.Parse(accData.UserId)
            };
            var challenge = await client.GetLoginChallenge(getChallengeReq); // Ulid
            Debug.Log($"Challenge: {challenge.Challenge}");

            var signer = new EthereumMessageSigner();
            var msg = $"{accData.UserId}:{challenge.Challenge}";
            Debug.Log($"SignIn message: {msg}");
        
            var validateChallengeReq = new ValidateChallengeRequest
            {
                UserId = Ulid.Parse(accData.UserId),
                Signature = signer.EncodeUTF8AndSign(msg, new EthECKey(accData.PrivateKeyHex))
            };

            var signInResult = await client.ValidateChallenge(validateChallengeReq);
            return signInResult;
        }
    }
}
