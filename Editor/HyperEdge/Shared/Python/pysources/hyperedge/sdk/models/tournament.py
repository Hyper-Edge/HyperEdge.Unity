import typing

from .base import _BaseModel
from .data import BaseData
from .models import DataModel
from .types import *


class Tournament(_BaseModel):
    Name: str
    ScoreFieldName: str = optional_field('Score')
    Model: typing.Optional[typing.Type[DataModel]]
    Data: typing.Optional[typing.Type[BaseData]]
    #
    FullModelClass: typing.Type[DataModel] = optional_field(None)
    FullDataClass: typing.Type[BaseData] = optional_field(None)

    def to_dict(self):
        return dict(
            Name=self.Name,
            ScoreFieldName=self.ScoreFieldName,
            Model=self.model_class.to_dict(),
            Data=self.data_class.to_dict())

    @property
    def model_class(self) -> typing.Type[DataModel]:
        if self.FullModelClass:
            return self.FullModelClass
        cls_name = f'{self.Name}TournamentModel'
        dyn_cls_args = dict(__base__=self.Model if self.Model else DataModel)
        if self.ScoreFieldName not in dyn_cls_args:
            dyn_cls_args[self.ScoreFieldName] = (UInt64, ...)
        dyn_cls_args['Data'] = (self.data_class, ...)
        self.FullModelClass = pydantic.create_model(cls_name, **dyn_cls_args)
        return self.FullModelClass

    @property
    def data_class(self) -> typing.Type[BaseData]:
        if self.FullDataClass:
            return self.FullDataClass
        cls_name = f'{self.Name}TournamentData'
        if self.Data:
            dyn_cls_args = dict(__base__=self.Data)
        else:
            dyn_cls_args = dict(__base__=BaseData)
        if 'Name' not in dyn_cls_args:
            dyn_cls_args['Name'] = (str, ...)
        #
        self.FullDataClass = pydantic.create_model(cls_name, **dyn_cls_args)
        return self.FullDataClass

    def define(self, **kwargs):
        return self.data_class.define(**kwargs)
