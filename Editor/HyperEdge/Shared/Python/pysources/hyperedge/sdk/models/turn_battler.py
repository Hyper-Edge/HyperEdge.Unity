import pydantic
import typing

from .types import *
from .models import DataModel, _BaseModel
from .data import BaseData
from hyperedge.sdk.utils import to_underscore


class GenericUnitLevelData(BaseData):
    def __init__(self, **kwargs):
        lvl = kwargs.get('Level')
        id_ = to_underscore(type(self).__name__) + f'_{lvl}'
        super().__init__(id=id_, **kwargs)

    Level: int = optional_field(0)
    MaxHealth: int
    Attack: int
    Defence: int
    CriticalChance: float = optional_field(0)
    CriticalDamage: float = optional_field(0)


class GenericUnitStatsLadder(BaseData):
    #
    Levels: typing.List[GenericUnitLevelData]

    def add_level(self, **kwargs):
        curr_lvl = len(self.Levels)
        ll = self.FullUnitLevelLadderData(Level=curr_lvl, **kwargs)
        self.Levels.append(ll)


class GenericUnitData(BaseData):
    Name: str
    Skills: typing.List[Ulid]
    #
    OnGameStart: typing.Optional[Ulid]
    OnGameEnd: typing.Optional[Ulid]
    OnTurnStart: typing.Optional[Ulid]
    OnTurnEnd: typing.Optional[Ulid]


class TurnBattleUnit(pydantic.BaseModel):
    Name: str
    UnitData: typing.Optional[typing.Type[BaseData]]
    UnitLevelData: typing.Optional[typing.Type[BaseData]]

    FullUnitData: typing.Type[GenericUnitData] = optional_field(None)
    FullUnitLevelData: typing.Type[GenericUnitLevelData] = optional_field(None)

    def to_dict(self):
        return dict(
            Name=self.Name,
            UnitData=self.unit_data_class.to_dict(),
            UnitLevelData=self.unit_level_data_class.to_dict())

    @property
    def unit_data_class(self) -> typing.Type[BaseData]:
        if self.FullUnitData:
            return self.FullUnitData
        cls_name = f'{self.Name}UnitData'
        dyn_cls_args = dict(__base__=GenericUnitData)
        if self.UnitData:
            for fname, fdef in self.UnitData.__fields__.items():
                dyn_cls_args[fname] = (fdef, ...)
        dyn_cls_args['Levels'] = (typing.List[self.unit_level_data_class], ...)
        self.FullUnitData = pydantic.create_model(cls_name, **dyn_cls_args)
        return self.FullUnitData

    @property
    def unit_level_data_class(self) -> typing.Type[BaseData]:
        if self.FullUnitLevelData:
            return self.FullUnitLevelData
        cls_name = f'{self.Name}UnitLevelData'
        dyn_cls_args = dict(__base__=GenericUnitLevelData)
        if self.UnitLevelData:
            for fname, fdef in self.UnitLevelData.__fields__.items():
                dyn_cls_args[fname] = (fdef, ...)
        self.FullUnitLevelData = pydantic.create_model(cls_name, **dyn_cls_args)
        return self.FullUnitLevelData


class ModelTurnBattleUnit(pydantic.BaseModel):
    Name: str
    Model: typing.Type[DataModel]

    def to_dict(self):
        return dict(
            Name=f'TBS{self.Name}Unit',
            Model=self.Model.__name__
        )


class TurnBattleUnitSlot(pydantic.BaseModel):
    Data: typing.Type[BaseData]


class DefaultUnitSlot(BaseData):
    pass


class TurnBattlerSystem(object):
    def __init__(self, name: str):
        self._name = name
        #
        self._units: typing.List[TurnBattleUnit] = []
        self._model_units: typing.List[ModelTurnBattleUnit] = []
        self._unit_slot_types: typing.List[TurnBattleUnitSlot] = []

    def add_unit_type(self, cls_: typing.Union[TurnBattleUnit, ModelTurnBattleUnit]):
        if isinstance(cls_, TurnBattleUnit):
            self._units.append(cls_)
        elif isinstance(cls_, ModelTurnBattleUnit):
            self._model_units.append(cls_)

    def add_unit_slot_type(self, slot: TurnBattleUnitSlot):
        self._unit_slot_types.append(slot)

    def to_dict(self):
        return dict(
            Name=self._name,
            Units=[unit.to_dict() for unit in self._units],
            ModelUnits=[unit.to_dict() for unit in self._model_units],
            UnitSlots=[]
        )
