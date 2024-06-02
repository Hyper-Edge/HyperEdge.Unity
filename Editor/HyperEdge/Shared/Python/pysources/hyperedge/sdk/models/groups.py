import typing

from .base import _BaseModel
from .models import DataModel


class UserGroup(_BaseModel):
    _storage_models: typing.Dict[str, typing.Type[DataModel]] = {}

    @classmethod
    def add_storage(cls, storage_cls: typing.Type[DataModel]):
        cls._storage_models[storage_cls.__name__] = storage_cls

    @classmethod
    def to_dict(cls):
        d = super().to_dict()
        d['StorageClasses'] = list(cls._storage_models.keys())
        return d

