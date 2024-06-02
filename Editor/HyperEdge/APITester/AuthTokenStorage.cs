using System;
using System.Collections.Concurrent;


namespace HyperEdge.Sdk.Unity.APITester   
{
    public class AuthTokenData
    {
        public AuthTokenData(string token, DateTimeOffset expiration)
        {
            Token = token;
            Expiration = expiration;
        }
        public bool IsExpired => Token == null || Expiration < DateTimeOffset.Now;
        public string Token { get; private set; }
        public DateTimeOffset Expiration { get; private set; }
    }
 
    public class AuthTokenStorage
    {
        public static AuthTokenStorage Current { get; } = new AuthTokenStorage();
        private readonly ConcurrentDictionary<string, AuthTokenData> _tokens = new();

        public AuthTokenData? GetToken(string userId)
        {
            if (!_tokens.TryGetValue(userId, out var tokenData))
            {
                return null;
            }
            return tokenData;
        }

        public bool Update(string userId, string token, DateTimeOffset expiration)
        {
            _tokens.TryRemove(userId, out var prevToken);
            return _tokens.TryAdd(userId, new AuthTokenData(token, expiration));
        }
    }
}
