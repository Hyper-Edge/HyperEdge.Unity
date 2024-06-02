using MagicOnion;

using HyperEdge.Shared.Protocol;


namespace HyperEdge.Shared.Services
{
    public interface IDepotService : IService<IDepotService>
    {
        /* ERC-20 token API */
        public UnaryResult<CreateErc20TokenResponse> CreateErc20Token(CreateErc20TokenRequest req);
        public UnaryResult<GetErc20TokensResponse> GetErc20Tokens(GetErc20TokensRequest req);
        public UnaryResult<GetErc20TokenResponse> GetErc20Token(GetErc20TokenRequest req);

        /* ERC-1155 API */
        public UnaryResult<CreateErc1155TokenResponse> CreateErc1155Token(CreateErc1155TokenRequest req);
        public UnaryResult<GetErc1155TokensResponse> GetErc1155Tokens(GetErc1155TokensRequest req);
        public UnaryResult<GetErc1155TokenResponse> GetErc1155Token(GetErc1155TokenRequest req);
        
        /* Inventory API */
        public UnaryResult<AddInventoryItemResponse> AddInventoryItem(AddInventoryItemRequest req);
        public UnaryResult<GetInventoryItemsResponse> GetInventoryItems(GetInventoryItemsRequest req);

        /* ERC-721 API */
        public UnaryResult<CreateErc721TokenResponse> CreateErc721Token(CreateErc721TokenRequest req);
        public UnaryResult<UpdateErc721TokenResponse> UpdateErc721Token(UpdateErc721TokenRequest req);
        public UnaryResult<GetErc721TokensResponse> GetErc721Tokens(GetErc721TokensRequest req);
        public UnaryResult<GetErc721TokenResponse> GetErc721Token(GetErc721TokenRequest req);

        /* DataClass (Data contracts) API */
        public UnaryResult<CreateDataClassContractResponse> CreateDataClassContract(CreateDataClassContractRequest req);
        public UnaryResult<UpdateDataClassResponse> UpdateDataClass(UpdateDataClassRequest req);
        public UnaryResult<GetDataClassContractResponse> GetDataClassContract(GetDataClassContractRequest req);
        public UnaryResult<GetDataClassContractsResponse> GetDataClassContracts(GetDataClassContractsRequest req);

        /* DataClass items API */
        public UnaryResult<CreateDataClassItemResponse> CreateDataClassItem(CreateDataClassItemRequest req);
        public UnaryResult<UpdateDataClassItemResponse> UpdateDataClassItem(UpdateDataClassItemRequest req);
        public UnaryResult<GetDataClassItemsResponse> GetDataClassItems(GetDataClassItemsRequest req);

        public UnaryResult<GetWeb3AppResponse> GetWeb3App(GetWeb3AppRequest req);
        public UnaryResult<GetWeb3AppsResponse> GetWeb3Apps(GetWeb3AppsRequest req);
        public UnaryResult<CreateWeb3AppResponse> CreateWeb3App(CreateWeb3AppRequest req);

        /* Stores API */
        public UnaryResult<GetStoreResponse> GetStore(GetStoreRequest req);
        public UnaryResult<CreateStoreResponse> CreateStore(CreateStoreRequest req);
        public UnaryResult<UpdateStoreResponse> UpdateStore(UpdateStoreRequest req);

        /* Packages API */
        public UnaryResult<CreatePackageResponse> CreatePackage(CreatePackageRequest req);
        public UnaryResult<UpdatePackageResponse> UpdatePackage(UpdatePackageRequest req);
        public UnaryResult<SetPackagePriceResponse> SetPackagePrice(SetPackagePriceRequest req);
        public UnaryResult<GetPackageResponse> GetPackage(GetPackageRequest req);

        public UnaryResult<CreatePriceResponse> CreatePrice(CreatePriceRequest req);

        public UnaryResult<AddPackageErc20TokensResponse> AddPackageErc20Tokens(AddPackageErc20TokensRequest req);
        public UnaryResult<AddPackageErc721TokensResponse> AddPackageErc721Tokens(AddPackageErc721TokensRequest req);
        public UnaryResult<AddPackageErc1155TokensResponse> AddPackageErc1155Tokens(AddPackageErc1155TokensRequest req);

        public UnaryResult<AddPriceErc1155TokensResponse> AddPriceErc1155Tokens(AddPriceErc1155TokensRequest req);

        public UnaryResult<AddRequestHandlerResponse> AddRequestHandler(AddRequestHandlerRequest req);
        public UnaryResult<GetRequestHandlersResponse> GetRequestHandlers(GetRequestHandlersRequest req);

        /* Game Mechanics API */
        public UnaryResult<CreateEnergySystemResponse> AddEnergySystem(CreateEnergySystemRequest req);
        public UnaryResult<AddCraftRulesResponse> AddCraftRules(AddCraftRulesRequest req);
        public UnaryResult<AddRulesResponse> AddRules(AddRulesRequest req);
        
        public UnaryResult<AddProgressionResponse> AddProgression(AddProgressionRequest req);
        public UnaryResult<AddProgressionLadderResponse> AddProgressionLadder(AddProgressionLadderRequest req);        
        public UnaryResult<AddBattlePassResponse> AddBattlePass(AddBattlePassRequest req);
        public UnaryResult<AddTournamentResponse> AddTournament(AddTournamentRequest req);
        public UnaryResult<AddQuestResponse> AddQuest(AddQuestRequest req);
        public UnaryResult<AddRewardResponse> AddReward(AddRewardRequest req);

        public UnaryResult<AddGameDataResponse> AddGameData(AddGameDataRequest req);

        /* App Export API */
        public UnaryResult<ExportAppDefResponse> ExportApp(ExportAppDefRequest req);
        public UnaryResult<ReleaseAppDefResponse> ReleaseApp(ReleaseAppDefRequest req);

        public UnaryResult<DeployContractResponse> DeployContract(DeployContractRequest req);
    }
}
