using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace HyperEdge.Sdk.Server
{
    public interface ILeaderBoard
    {
        Task<double?> GetScoreAsync(Ulid id);
        double? GetScore(Ulid id);
        //
        Task<long?> GetRankAsync(Ulid id);
        long? GetRank(Ulid id);
        //
        Task RemoveEntriesAsync(List<Ulid> ids);
        Task RemoveEntryAsync(Ulid id);
        //
        Task<long> GetCountAsync();
        long GetCount();
        //
        Task ClearAsync();
        void Clear();
    }
}
