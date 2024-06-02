import pydantic
import typing

from hyperedge.sdk.models.base import _BaseModel
from hyperedge.sdk.models.models import BaseData, DataModel
from hyperedge.sdk.models.types import optional_field, get_cs_type
from hyperedge.sdk.utils import camelize_string


class NetEntityField(pydantic.BaseModel):
    Name: str
    Typename: str
    DefaultValue: typing.Optional[str]
    InitFrom: typing.Optional[str]


class NetEntity(_BaseModel):
    Name: str
    Model: typing.Optional[typing.Type[DataModel]]
    Data: typing.Optional[typing.Type[BaseData]]
    Fields: typing.List[NetEntityField]
    SyncFields: typing.List[NetEntityField]

    @classmethod
    def from_model(cls, model: typing.Type[DataModel], sync_fields: typing.List[str]):
        fields = []
        _sync_fields = []
        for fname, fdef in model.__fields__.items():
            if fname == 'id':
                continue
            ne_fld = NetEntityField(
                Name=camelize_string(fname),
                Typename=get_cs_type(fdef.outer_type_),
                InitFrom='InitFromModel')
            if fname in sync_fields:
                _sync_fields.append(ne_fld)
            else:
                fields.append(ne_fld)
        #
        return cls(
            Name=model.__name__,
            Model=model,
            Data=model.dataclass(),
            Fields=fields,
            SyncFields=_sync_fields
        )

    def to_dict(self):
        return dict(
            Name=self.Name,
            Model=(self.Model.__name__ if self.Model else ''),
            Data=(self.Data.__name__ if self.Data else ''),
            Fields=[fld.dict() for fld in self.Fields],
            SyncFields=[fld.dict() for fld in self.SyncFields]
        )


class MultiPlayerSystem(object):
    def __init__(self, name: str):
        self._name = name
        self._entities = []

    def add_entity(self, entity: NetEntity):
        self._entities.append(entity)

    @property
    def name(self):
        return self._name

    def to_dict(self):
        return dict(
            Name=self._name,
            NetEntities=[e.Name for e in self._entities]
        )
