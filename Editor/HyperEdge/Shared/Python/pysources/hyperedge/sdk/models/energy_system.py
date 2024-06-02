from .data import BaseData
from .models import DataModel
from .types import *


class GenericEnergySystemData(BaseData):
    Name: str
    InitialValue: int
    RegenValue: int
    RegenRate: int
    MaxCapacity: int

    @classmethod
    def to_dict(cls):
        flds=[]
        for fname, fdef in cls.__fields__.items():
            if fname in GenericEnergySystemData.__fields__:
                continue
            flds.append({'Name': fname, 'Typename': get_cs_type(fdef.outer_type_)})
        base = cls.base(GenericEnergySystemData)
        return dict(
            Name=cls.__name__,
            Fields=flds,
            Base=base.__name__ if base else None)


class EnergySystem(pydantic.BaseModel):
    Name: str
    EnergyFieldName: str = optional_field('Energy')
    Data: typing.Type[BaseData] = optional_field(None)
    Model: typing.Type[DataModel] = optional_field(None)

    FullDataClass: typing.Type[GenericEnergySystemData] = optional_field(None)
    FullModelClass: typing.Type[DataModel] = optional_field(None)

    def define(self, **kwargs):
        return self.data_class.define(**kwargs)

    @property
    def data_class(self) -> typing.Type[BaseData]:
        if self.FullDataClass:
            return self.FullDataClass
        cls_name = f'{self.Name}EnergySystemData'
        dyn_cls_args = dict(__base__=GenericEnergySystemData)
        if self.Data:
            dyn_cls_args['Data'] = (self.Data, ...)
        self.FullDataClass = pydantic.create_model(cls_name, **dyn_cls_args)
        return self.FullDataClass

    @property
    def model_class(self) -> typing.Type[DataModel]:
        if self.FullModelClass:
            return self.FullModelClass
        cls_name = f'{self.Name}EnergySystemModel'
        dyn_cls_args = dict(__base__=self.Model if self.Model else DataModel)
        if self.EnergyFieldName not in dyn_cls_args:
            dyn_cls_args[self.EnergyFieldName] = (int, ...)
        dyn_cls_args['Data'] = (self.FullDataClass, ...)
        self.FullModelClass = pydantic.create_model(cls_name, **dyn_cls_args)
        return self.FullModelClass

    def to_dict(self):
        return dict(
            Name=self.Name,
            EnergyFieldName=self.EnergyFieldName,
            Data=self.data_class.to_dict(),
            Model=self.model_class.to_dict()
        )
