import os
import pydantic
import requests
import typing
import ulid

from hyperedge.sdk.ws import HeWsClient
from hyperedge.sdk.models.data import DataRef
from hyperedge.sdk.models.types import optional_field


_EMPTY_ULID = ulid.ULID(bytes(16))


class DataClassFieldDTO(pydantic.BaseModel):
    Name: str
    Typename: str
    DefaultValue: typing.Optional[str]


class DataClassDTO(pydantic.BaseModel):
    Id: str = str(_EMPTY_ULID)
    Name: str
    Base: typing.Optional[str]
    FilePath: typing.Optional[str]
    Fields: typing.List[DataClassFieldDTO]


class DataClassInstanceFieldDTO(pydantic.BaseModel):
    Name: str
    Value: typing.Any


class DataClassInstanceDTO(pydantic.BaseModel):
    Id: str = str(_EMPTY_ULID)
    Name: str
    Fields: typing.List[DataClassInstanceFieldDTO]


class DataClassInstanceFieldsDTO(pydantic.BaseModel):
    Fields: typing.List[DataClassInstanceFieldDTO]


class UserGroupClassDTO(pydantic.BaseModel):
    Id: str = str(_EMPTY_ULID)
    Name: str
    Fields: typing.List[DataClassFieldDTO]
    StorageClasses: typing.List[str]


class InventoryItemDefDTO(pydantic.BaseModel):
    Id: str
    Typename: str


class InventoryDefDTO(pydantic.BaseModel):
    Name: str
    Items: typing.List[InventoryItemDefDTO]


class Erc721RewardDTO(pydantic.BaseModel):
    EntityName: str
    ItemId: str
    Amount: int


class Erc1155RewardDTO(pydantic.BaseModel):
    ItemId: str
    Amount: int


class RewardDTO(pydantic.BaseModel):
    Id: typing.Optional[str]
    Name: typing.Optional[str]
    Erc721Rewards: typing.List[Erc721RewardDTO]
    Erc1155Rewards: typing.List[Erc1155RewardDTO]


class Erc721CostDTO(pydantic.BaseModel):
    EntityName: str
    ItemId: str
    Amount: int
    Conditions: typing.List[str]


class Erc1155CostDTO(pydantic.BaseModel):
    ItemId: str
    Amount: int


class CostDTO(pydantic.BaseModel):
    Erc721Costs: typing.List[Erc721CostDTO]
    Erc1155Costs: typing.List[Erc1155CostDTO]


class CraftRulesDTO(pydantic.BaseModel):
    Name: str
    Cost: CostDTO
    Reward: RewardDTO


class DataClassFieldsDTO(pydantic.BaseModel):
    Fields: typing.List[DataClassFieldDTO]


class ProgressionSystemDTO(pydantic.BaseModel):
    EntityName: str
    IsExperienceBased: bool
    LevelField: typing.Optional[str] = 'Level'
    ExperienceField: typing.Optional[str] = 'Exp'
    LadderLevelData: typing.Optional[DataClassFieldsDTO]


class GenericLadderLevelDTO(pydantic.BaseModel):
    Exp: typing.Optional[int]
    Reward: typing.Optional[RewardDTO]
    Cost: typing.Optional[CostDTO]
    Conditions: typing.Optional[typing.List[str]]
    Data: typing.Optional[DataClassInstanceFieldsDTO]


class GenericLadderDTO(pydantic.BaseModel):
    Name: str
    LadderType: str
    ProgressionId: typing.Optional[str]
    ProgressionName: str
    Levels: typing.List[GenericLadderLevelDTO]


class BattlePassDTO(pydantic.BaseModel):
    Name: str
    LevelData: typing.Optional[DataClassFieldsDTO]
    Model: typing.Optional[DataClassFieldsDTO]
    Data: typing.Optional[DataClassFieldsDTO]


class BattlePassInstanceDTO(pydantic.BaseModel):
    Id: typing.Optional[str]
    Name: str
    BattlePassId: typing.Optional[str]
    BattlePassName: str
    Fields: typing.List[DataClassInstanceFieldDTO]
    Levels: typing.List[GenericLadderLevelDTO]


class QuestDTO(pydantic.BaseModel):
    Name: str
    AcceptConditions: typing.Optional[typing.List[str]]
    FinishConditions: typing.Optional[typing.List[str]]
    ModelUid: typing.Optional[str]
    Model: DataClassFieldsDTO
    DataUid: typing.Optional[str]
    Data: typing.Optional[DataClassFieldsDTO]


class TournamentDTO(pydantic.BaseModel):
    Name: str
    Model: DataClassFieldsDTO
    Data: DataClassFieldsDTO


class EnergySystemDTO(pydantic.BaseModel):
    Id: typing.Optional[str]
    Name: str
    Data: DataClassDTO
    Model: DataClassDTO


class RequestHandlerDTO(pydantic.BaseModel):
    Id: typing.Optional[str]
    Name: str
    RequestClassId: typing.Optional[str]
    ResponseClassId: typing.Optional[str]
    RequestClassName: str
    ResponseClassName: str
    Code: str


class JobHandlerDTO(pydantic.BaseModel):
    Id: typing.Optional[str]
    Name: str
    JobDataClassId: typing.Optional[str]
    JobDataClassName: str
    Code: str


class EventHandlerDTO(pydantic.BaseModel):
    Id: typing.Optional[str]
    Name: str
    EventClassId: typing.Optional[str]
    EventClassName: str
    Code: str


class AbilityNodeDTO(pydantic.BaseModel):
    Id: typing.Optional[str]
    Name: str
    Base: typing.Optional[str]
    Category: str
    Code: str


class AbilityGraphJsonDTO(pydantic.BaseModel):
    Id: typing.Optional[str]
    Name: str
    Data: str


class NetEntityFieldDTO(pydantic.BaseModel):
    Name: str
    Typename: str
    DefaultValue: typing.Optional[str]
    InitFrom: typing.Optional[str]


class NetEntityDTO(pydantic.BaseModel):
    Id: typing.Optional[str]
    Name: str
    Model: typing.Optional[str]
    Data: typing.Optional[str]
    SyncFields: typing.List[NetEntityFieldDTO]
    Fields: typing.List[NetEntityFieldDTO]


class TurnBattlerUnitDTO(pydantic.BaseModel):
    Name: str
    UnitData: DataClassDTO
    UnitLevelData: DataClassDTO
    Stats: typing.Optional[typing.List[str]]


class TurnBattlerModelUnitDTO(pydantic.BaseModel):
    Name: str
    Model: str
    Stats: typing.Optional[typing.List[str]]


class TurnGameSystemDTO(pydantic.BaseModel):
    Id: typing.Optional[str]
    Name: str
    #
    Units: typing.List[TurnBattlerUnitDTO]
    ModelUnits: typing.List[TurnBattlerModelUnitDTO]
    UnitSlots: typing.List[DataClassDTO]


class MessageRelayTypeDTO(pydantic.BaseModel):
    Name: str
    RequestClassName: str
    ResponseClassName: str
    Relay: bool
    Code: str


class MessageRelaySystemDTO(pydantic.BaseModel):
    Id: typing.Optional[str]
    Name: str
    Standalone: bool
    MessageTypes: typing.List[MessageRelayTypeDTO]


class StorageClassDefDTO(pydantic.BaseModel):
    StorageType: int
    StorageClass: DataClassDTO


class AppDefDTO(pydantic.BaseModel):
    Id: typing.Optional[str]
    Name: str
    DataClasses: typing.List[DataClassDTO]
    UGCDataClasses: typing.List[DataClassDTO]
    ModelClasses: typing.List[DataClassDTO]
    StructClasses: typing.List[DataClassDTO]
    EventClasses: typing.List[DataClassDTO]
    StorageClasses: typing.List[StorageClassDefDTO]
    GroupClasses: typing.List[UserGroupClassDTO]
    DataClassInstances: typing.Dict[str, typing.List[DataClassInstanceDTO]]
    Inventories: typing.List[InventoryDefDTO]
    Quests: typing.List[QuestDTO]
    Tournaments: typing.List[TournamentDTO]
    BattlePasses: typing.List[BattlePassDTO]
    BattlePassInstances: typing.List[BattlePassInstanceDTO]
    Progressions: typing.List[ProgressionSystemDTO]
    ProgressionLadders: typing.List[GenericLadderDTO]
    CraftRules: typing.List[CraftRulesDTO]
    Rewards: typing.List[RewardDTO]
    EnergySystems: typing.List[EnergySystemDTO]
    RequestHandlers: typing.List[RequestHandlerDTO]
    JobHandlers: typing.List[JobHandlerDTO]
    EventHandlers: typing.List[EventHandlerDTO]
    #
    AbilityGraphs: typing.List[AbilityGraphJsonDTO]
    AbilityNodes: typing.List[AbilityNodeDTO]
    #
    MsgRelaySystems: typing.List[MessageRelaySystemDTO]
    TurnGameSystems: typing.List[TurnGameSystemDTO]
    NetEntities: typing.List[NetEntityDTO]


class ExportAppRequest(pydantic.BaseModel):
    AppId: str
    AppDef: AppDefDTO


class ExportAppResponse(pydantic.BaseModel):
    AppId: str
    AppDefFileId: str
    CurrentVersionId: str
    TestEnvId: str


class ReleaseAppRequest(pydantic.BaseModel):
    AppId: str
    VersionName: str
    AppDef: AppDefDTO


class ReleaseAppResponse(pydantic.BaseModel):
    AppId: str
    VersionId: str
    VersionName: str
    AppDefFileId: str


class GenCodeRequest(pydantic.BaseModel):
    Id: str


class GenCodeResponse(pydantic.BaseModel):
    ServerFilesArchiveId: str


class BuildServerRequest(pydantic.BaseModel):
    Id: str


class BuildAppVersionRequest(pydantic.BaseModel):
    AppId: str
    VersionName: str
    DoBuild: bool


class BuildAppVersionResponse(pydantic.BaseModel):
    ServerImageId: typing.Optional[str]
    SyncBotImageId: typing.Optional[str]


class CreateAppEnvRequest(pydantic.BaseModel):
    AppId: str
    Name: str


class CreateAppEnvResponse(pydantic.BaseModel):
    Id: str
    AppId: str
    Name: str


class RunAppRequest(pydantic.BaseModel):
    AppId: str
    VersionId: str
    EnvId: str


class ExportBuildRunCurrentVersionRequest(pydantic.BaseModel):
    AppId: str
    AppDef: AppDefDTO
    DoBuild: typing.Optional[bool]
    DoRun: typing.Optional[bool]


class ExportGenCodeCurrentVersionRequest(pydantic.BaseModel):
    AppId: str
    AppDef: AppDefDTO
    DoGenCode: bool


class StopAppRequest(pydantic.BaseModel):
    AppId: str
    VersionId: typing.Optional[str]
    EnvId: typing.Optional[str]


class LlmGenDataRequest(pydantic.BaseModel):
    AppId: str
    DataClassName: str


class LlmGenDataResponse(pydantic.BaseModel):
    Success: bool
    Data: typing.List[DataClassInstanceFieldDTO]


class LlmProposeModelRequest(pydantic.BaseModel):
    AppId: str
    EntityName: str
    Description: str


class LlmExportProposalResponse(pydantic.BaseModel):
    AppId: str
    VersionId: str
    LlmProposalId: str
    AppDefFileId: str


class LlmProposeModelResponse(pydantic.BaseModel):
    Success: bool


class CoDesignerPlanAndExecuteRequest(pydantic.BaseModel):
    AppId: str
    Description: str


class ConvertLlmThreadToAppDefRequest(pydantic.BaseModel):
    AppId: str
    LlmThreadId: str


class HEClient(object):
    def __init__(self, url=None):
        self._api_key = os.environ.get('HE_API_KEY')
        self._host = url or os.environ.get('HE_API_URL')
        self._url = f'https://{self._host}'
        self._ws = None

    @property
    def ws(self):
        if self._ws is None:
            ticket = self.get_ticket()
            self._ws = HeWsClient(url=f'wss://{self._host}/ws/', ticket=ticket)
        return self._ws

    @property
    def _auth_base_url(self):
        return f'{self._url}/api/IAuthService'

    @property
    def _depot_base_url(self):
        return f'{self._url}/api/IDepotService'

    @property
    def _apps_base_url(self):
        return f'{self._url}/api/IAppsService'

    @property
    def _misc_base_url(self):
        return f'{self._url}/api/bc'

    def export_app(self, data: AppDefDTO):
        req = ExportAppRequest(AppId=data.Id or str(_EMPTY_ULID), AppDef=data)
        resp = self._post_json(f'{self._depot_base_url}/ExportApp', req.json())
        job_data = self.ws.wait_for_job(resp['JobId'])
        if not job_data.success:
            raise Exception()
        return ExportAppResponse(**job_data.retval)

    def release_app(self, data: AppDefDTO, app_uid: ulid.ULID, version_name: str):
        req = ReleaseAppRequest(AppId=str(app_uid), VersionName=version_name, AppDef=data)
        resp = self._post_json(f'{self._depot_base_url}/ReleaseApp', req.json())
        job_data = self.ws.wait_for_job(resp['JobId'])
        if not job_data.success:
            raise Exception()
        return ReleaseAppResponse(**job_data.retval)

    def build_app_version(self, app_uid: ulid.ULID, version_name: str, do_build: bool):
        req = BuildAppVersionRequest(AppId=str(app_uid), VersionName=version_name, DoBuild=do_build)
        resp = self._post_json(f'{self._apps_base_url}/BuildApp', req.json())
        job_data = self.ws.wait_for_job(resp['JobId'])
        if not job_data.success:
            raise Exception()
        return BuildAppVersionResponse(**job_data.retval)

    def create_app_env(self, app_uid: ulid.ULID, env_name: str):
        req = CreateAppEnvRequest(AppId=str(app_uid), Name=env_name)
        resp = self._post_json(f'{self._apps_base_url}/CreateAppEnv', req.json())
        return CreateAppEnvResponse(**resp.get('AppEnv', dict()))

    def run_app(self, app_uid: str, version_uid: str, env_uid: str):
        req = RunAppRequest(AppId=app_uid, VersionId=version_uid, EnvId=env_uid)
        resp = self._post_json(f'{self._apps_base_url}/RunApp', req.json())
        job_data = self.ws.wait_for_job(resp['JobId'])
        if not job_data.success:
            raise Exception()

    def export_gen_code_current_version(self, app_uid: str, app_def: AppDefDTO, do_gencode=True):
        req = ExportGenCodeCurrentVersionRequest(AppId=app_uid, AppDef=app_def, DoGenCode=do_gencode)
        resp = self._post_json(f'{self._apps_base_url}/ExportGenCodeCurrentVersion', req.json())
        job_data = self.ws.wait_for_job(resp['JobId'])
        if not job_data.success:
            raise Exception()
        return job_data.retval

    def export_build_run_current_version(self, app_uid: str, app_def: AppDefDTO, do_build: bool, do_run: bool):
        req = ExportBuildRunCurrentVersionRequest(AppId=app_uid, AppDef=app_def, DoBuild=do_build, DoRun=do_run)
        resp = self._post_json(f'{self._apps_base_url}/ExportBuildRunCurrentVersion', req.json())
        job_data = self.ws.wait_for_job(resp['JobId'])
        if not job_data.success:
            raise Exception()
        return job_data.retval

    def stop_app(self, app_uid: str, version_uid: str, env_uid: str):
        req = RunAppRequest(AppId=app_uid, VersionId=version_uid, EnvId=env_uid)
        resp = self._post_json(f'{self._apps_base_url}/StopApp', req.json())
        job_data = self.ws.wait_for_job(resp['JobId'])
        if not job_data.success:
            raise Exception()

    def gen_code(self, uid: str):
        req = GenCodeRequest(Id=uid)
        resp = self._post_json(f'{self._misc_base_url}/GenCode', req.json())
        job_data = self.ws.wait_for_job(resp['JobId'])
        if not job_data.success:
            raise Exception()
        return GenCodeResponse(ServerFilesArchiveId=job_data.retval['ServerFilesArchiveId'])

    def build_server(self, uid):
        req = BuildServerRequest(Id=uid)
        resp = self._post_json(f"{self._misc_base_url}/BuildServer", req.json())
        job_data = self.ws.wait_for_job(resp['JobId'])
        if not job_data.success:
            raise Exception()
        return None

    def llm_gen_data(self, app_uid: str, data_cls_name: str):
        req = LlmGenDataRequest(
            AppId=app_uid,
            DataClassName=data_cls_name
        )
        resp = self._post_json(f"{self._misc_base_url}/llm/gen_data", req.json())
        job_data = self.ws.wait_for_job(resp['JobId'])
        if not job_data.success:
            raise Exception()
        return LlmGenDataResponse(**job_data.retval)

    def llm_propose_model(self, app_uid: str, entity_name: str, description: str):
        req = LlmProposeModelRequest(
            AppId=app_uid,
            EntityName=entity_name,
            Description=description
        )
        resp = self._post_json(f"{self._misc_base_url}/llm/gd_decompose", req.json())
        job_data = self.ws.wait_for_job(resp['JobId'])
        if not job_data.success:
            raise Exception()
        return LlmProposeModelResponse(**job_data.retval)

    def convert_llm_thread_to_app_def(self, app_uid: str, ai_thread_uid: str):
        req = ConvertLlmThreadToAppDefRequest(AppId=app_uid, LlmThreadId=ai_thread_uid)
        resp = self._post_json(f"{self._misc_base_url}/ai/ConvertLlmThreadToAppDef", req.json())
        job_data = self.ws.wait_for_job(resp['JobId'])
        if not job_data.success:
            raise Exception()
        return LlmExportProposalResponse(**job_data.retval)

    def get_ticket(self) -> str:
        resp = self._get_json(f"{self._misc_base_url}/ws/ticket")
        return resp['ticket']

    def download_file_by_id(self, file_uid: str, filepath: str):
        link = self._get_file_link(file_uid)
        resp = requests.get(link)
        with open(filepath, 'w') as f:
            f.write(resp.text)

    def _get_file_link(self, file_uid: str) -> str:
        resp = self._get_json(f"{self._misc_base_url}/file/{file_uid}")
        return resp['url']

    def _get_headers(self) -> dict:
        if not self._api_key:
            raise ValueError('HyperEdge API key is not defined')
        return {
            'Content-type': 'application/json',
            'Accept': 'text/plain',
            'X-API-Key': self._api_key
        }

    def _get_json(self, url: str) -> dict:
        headers = self._get_headers()
        resp = requests.get(url, headers=headers)
        print(f'Status: {resp.status_code}')
        print(resp.json())
        return resp.json()

    def _post_json(self, url: str, data: str) -> dict:
        headers = self._get_headers()
        resp = requests.post(url, data=data, headers=headers)
        print(f'Status: {resp.status_code}')
        try:
            print(resp.json())
        except requests.exceptions.JSONDecodeError:
            print(f'Status: {resp.status_code}')
            print(resp.text)
            raise
        return resp.json()
