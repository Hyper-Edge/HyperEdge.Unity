import typing

from .base import _BaseModel
from .data import BaseData
from .models import DataModel
from .reward import Reward
from .types import *


class GenericQuestData(BaseData):
    Name: str
    Reward: typing.Optional[Reward]

    @classmethod
    def to_dict(cls):
        flds=[]
        for fname, fdef in cls.__fields__.items():
            if fname in GenericQuestData.__fields__:
                continue
            flds.append({'Name': fname, 'Typename': get_cs_type(fdef.outer_type_)})
        base = cls.base(GenericQuestData)
        return dict(
            Name=cls.__name__,
            Fields=flds,
            Base=base.__name__ if base else None)


class Quest(_BaseModel):
    Name: str
    AcceptConditions: typing.Optional[typing.List[str]]
    FinishConditions: typing.Optional[typing.List[str]]
    Model: typing.Type[DataModel]
    Data: typing.Optional[typing.Type[BaseData]]
    #
    FullModelClass: typing.Type[DataModel] = optional_field(None)
    FullDataClass: typing.Type[GenericQuestData] = optional_field(None)

    def to_dict(self):
        return dict(
            Name=self.Name,
            AcceptConditions=self.AcceptConditions,
            FinishConditions=self.FinishConditions,
            Model=self.model_class.to_dict(),
            Data=self.data_class.to_dict()
        )

    @property
    def model_class(self) -> typing.Type[DataModel]:
        if self.FullModelClass:
            return self.FullModelClass
        cls_name = f'{self.Name}QuestModel'
        dyn_cls_args = dict(__base__=self.Model if self.Model else DataModel)
        dyn_cls_args['Data'] = (self.data_class, ...)
        self.FullModelClass = pydantic.create_model(cls_name, **dyn_cls_args)
        return self.FullModelClass

    @property
    def data_class(self) -> typing.Type[BaseData]:
        if self.FullDataClass:
            return self.FullDataClass
        cls_name = f'{self.Name}QuestData'
        if self.Data:
            dyn_cls_args = dict(__base__=self.Data)
        else:
            dyn_cls_args = dict(__base__=GenericQuestData)
        if 'Name' not in dyn_cls_args:
            dyn_cls_args['Name'] = (str, ...)
        #
        self.FullDataClass = pydantic.create_model(cls_name, **dyn_cls_args)
        return self.FullDataClass

    def define(self, **kwargs):
        return self.data_class.define(**kwargs)
