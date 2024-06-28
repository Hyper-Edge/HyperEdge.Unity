import json
import importlib
import inspect
import os
import pathlib
import typing

from hyperedge.sdk.models.types import *
from hyperedge.sdk.models.base import _BaseModel
from hyperedge.sdk.models import *
from hyperedge.sdk.client import AbilityGraphJsonDTO


def datainst_to_json(cls, data_inst):
    flds = []
    for fname, fdef in cls.__fields__.items():
        if fname == 'id':
            continue
        t_origin = typing.get_origin(fdef.outer_type_)
        fval = getattr(data_inst, fname)
        if isinstance(fval, Reward):
            fval = json.dumps(fval.to_dict())
        if t_origin is list:
            fval = json.dumps(fval)
        flds.append({'Name': fname, 'Value': fval})
    #
    return dict(
        Name=data_inst.id,
        Fields=flds
    )


class DataLoader(object):
    def __init__(self, package_name):
        self._package_name = package_name
        self._data_classes = []
        self._ugc_data_classes = []
        self._model_classes = []
        self._struct_classes = []
        self._storage_classes = []
        self._user_group_classes = []
        self._event_classes = [OnRegisterEvent, OnLoginEvent]
        self._rewards: typing.List[Reward] = []
        self._crafts: typing.Dict[str, CraftRule] = {}
        self._progressions: typing.Dict[str, ProgressionLadder] = {}
        self._ladders: typing.Dict[str, GenericLadderBase] = {}
        self._battle_passes: typing.List[BattlePass] = []
        self._battle_pass_instances: typing.List[BattlePassInstance] = []
        self._quests: typing.List[Quest] = []
        self._energy_systems: typing.List[EnergySystem] = []
        self._tournaments: typing.List[Tournament] = []
        self._request_handlers: typing.List[Handler] = []
        self._job_handlers: typing.List[Job] = []
        self._event_handlers: typing.List[EventHandler] = []
        self._ability_graphs: typing.List[AbilityGraph] = []
        self._unity_ability_graphs: typing.List[AbilityGraphJsonDTO] = []
        self._ability_nodes: typing.List[AbilityNode] = []
        self._tg_systems: typing.List[TurnGameSystem] = []
        self._net_entities: typing.List[NetEntity] = []

        self.iterate_dataclasses(package_name)
        self._load_manual_data(package_name)

    def _load_manual_data(self, package_name):
        package = importlib.import_module(package_name)
        data_p = pathlib.Path(inspect.getfile(package)).parents[1].joinpath('data')
        for jdata_fp in data_p.rglob('*.json'):
            sub_path = jdata_fp.parent.relative_to(data_p)
            entity_name = jdata_fp.stem
            with open(jdata_fp, 'r') as jdata_f:
                jdata = json.load(jdata_f)
            #
            if str(sub_path) == 'NodeFlows':
                ag = AbilityGraphJsonDTO(Name=entity_name, Data=json.dumps(jdata))
                self._unity_ability_graphs.append(ag)
                continue
            #
            if entity_name == 'Rewards':
                for el in jdata:
                    reward = Reward.from_dict(el)
                    self._rewards.append(reward)
            elif entity_name == 'CraftRules':
                for el in jdata:
                    cr = CraftRule.from_dict(el)
                    self._crafts[cr.id] = cr
            else:
                for el in jdata:
                    self._update_di(entity_name, el)

    def _update_di(self, cls_name: str, data: dict):
        di = BaseData.get_data_instance(cls_name, data['Name'])
        for fld_data in data['Fields']:
            setattr(di, fld_data['Name'], fld_data['Value'])

    def iterate_dataclasses_in_package(self, package_name):
        package = importlib.import_module(package_name)
        for (name, obj) in inspect.getmembers(package):
            if inspect.isclass(obj):
                #
                if not issubclass(obj, _BaseModel):
                    continue
                if obj.__module__ != package.__name__:
                    continue
                if obj is _BaseModel:
                    continue
                #
                if issubclass(obj, StorageBase):
                    self._storage_classes.append((obj.storage_type(), obj))
                elif issubclass(obj, DataModel):
                    self._model_classes.append(obj)
                elif issubclass(obj, UGCData):
                    self._ugc_data_classes.append(obj)
                elif issubclass(obj, BaseData):
                    self._data_classes.append(obj)
                elif issubclass(obj, BaseEvent):
                    self._event_classes.append(obj)
                elif issubclass(obj, UserGroup):
                    self._user_group_classes.append(obj)
                else:
                    self._struct_classes.append(obj)
            elif isinstance(obj, Reward):
                if obj.name:
                    self._rewards.append(obj)
            elif isinstance(obj, CraftRule):
                self._crafts[obj.id] = obj
            elif isinstance(obj, ProgressionLadder):
                self._progressions[obj.id] = obj
            elif isinstance(obj, GenericLadderBase):
                self._ladders[obj.id] = obj
            elif isinstance(obj, BattlePass):
                self._battle_passes.append(obj)
            elif isinstance(obj, BattlePassInstance):
                self._battle_pass_instances.append(obj)
            elif isinstance(obj, Quest):
                self._quests.append(obj)
            elif isinstance(obj, EnergySystem):
                self._energy_systems.append(obj)
            elif isinstance(obj, Tournament):
                self._tournaments.append(obj)
            elif isinstance(obj, Handler):
                self._request_handlers.append(obj)
            elif isinstance(obj, Job):
                self._job_handlers.append(obj)
            elif isinstance(obj, EventHandler):
                self._event_handlers.append(obj)
            elif isinstance(obj, NetEntity):
                self._net_entities.append(obj)
            elif isinstance(obj, TurnBattlerSystem):
                self._tg_systems.append(obj)

    def iterate_dataclasses(self, package_name: str):
        
        package = importlib.import_module(package_name)
        package_path = pathlib.Path(package.__file__).parent  # Use pathlib for platform-agnostic path handling
        
        for fpath in package_path.rglob("*.py"):
            # Use pathlib's features to create a platform-agnostic, dot-separated package path
            rel_path = fpath.relative_to(package_path.parent)
            pname = '.'.join(rel_path.with_suffix('').parts)
            
            self.iterate_dataclasses_in_package(pname)

    def to_json(self):
        data = self.to_dict()
        return json.dumps(data, indent=4)

    def to_dict(self):
        data_class_instances = {}
        #
        for cls in BaseData.dataclasses():
            if issubclass(cls, GenericLadderBase):
                continue
            for data_inst in cls.instances():
                j = datainst_to_json(cls, data_inst)
                if cls.__name__ not in data_class_instances:
                    data_class_instances[cls.__name__] = []
                data_class_instances[cls.__name__].append(j)
        #
        return dict(
            DataClasses=[cls.to_dict() for cls in self._data_classes],
            UGCDataClasses=[cls.to_dict() for cls in self._ugc_data_classes],
            ModelClasses=[cls.to_dict() for cls in self._model_classes],
            StructClasses=[cls.to_dict() for cls in self._struct_classes],
            EventClasses=[cls.to_dict() for cls in self._event_classes],
            StorageClasses=[dict(StorageType=st_type, StorageClass=cls.to_dict()) for (st_type, cls) in self._storage_classes],
            DataClassInstances=data_class_instances,
            GroupClasses=[cls.to_dict() for cls in self._user_group_classes],
            Inventories=[inv.to_dict() for inv in Inventory.all()],
            Quests=[q.to_dict() for q in self._quests],
            Tournaments=[t.to_dict() for t in self._tournaments],
            BattlePasses=[bp.to_dict() for bp in self._battle_passes],
            BattlePassInstances=[bp.to_dict() for bp in self._battle_pass_instances],
            Progressions=[p.to_dict() for p in self._progressions.values()],
            ProgressionLadders=[l.to_dict() for l in self._ladders.values()],
            CraftRules=[c.to_dict() for c in self._crafts.values()],
            Rewards=[r.to_dict() for r in self._rewards if r.name],
            EnergySystems=[es.to_dict() for es in self._energy_systems],
            RequestHandlers=[h.to_dict() for h in self._request_handlers],
            JobHandlers=[h.to_dict() for h in self._job_handlers],
            EventHandlers=[h.to_dict() for h in self._event_handlers],
            #
            AbilityGraphs=[g.dict() for g in self._unity_ability_graphs],
            AbilityNodes=[n.to_dict() for n in self._ability_nodes],
            #
            MsgRelaySystems=[],
            TurnGameSystems=[tg.to_dict() for tg in self._tg_systems],
            #
            NetEntities=[e.to_dict() for e in self._net_entities])
